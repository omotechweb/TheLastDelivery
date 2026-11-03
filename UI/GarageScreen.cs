using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TheLastDelivery.Audio;
using TheLastDelivery.Core;
using TheLastDelivery.Economy;
using TheLastDelivery.Graphics;
using TheLastDelivery.Localization;
using TheLastDelivery.Passengers;
using TheLastDelivery.Platform;

namespace TheLastDelivery.UI;

/// <summary>
/// Tren Deposu / Garaj ([G], mola yerlerinde: kasaba, benzinlik, ana istasyon). Oyuncunun sahip olduğu
/// tren görünümleri, duman, kabin süsleri, müzik kutusu, evcil hayvan ve vagonlar tek ekranda incelenip
/// değiştirilir. Vagonlar trenden ayrılabilir ya da yeniden bağlanabilir. Değişiklikler anında
/// kaydedilir; arkadaki tren ve sağdaki kabin önizlemesi hemen güncellenir. Açıkken oyun zamanı durur.
///   W/S: satır   A/D: seçenek değiştir   G / BACKSPACE: kapat
/// </summary>
public sealed class GarageScreen
{
    private enum RowKind { Skin, Cosmetic, Wagon }
    private sealed record Row(RowKind Kind, CosmeticSlot Slot = default, UpgradeType Wagon = default);

    private static readonly Rectangle Panel = new(40, 76, 600, 514); // 10 satır (3 vagonla) + ipucu
    private readonly List<Row> _rows = new();
    private int _cursor;
    private float _open;
    private float _time;
    private string _message;
    private float _messageTimer;

    public bool IsOpen { get; private set; }

    public void Open()
    {
        IsOpen = true;
        _open = 0f;
        _message = null;
        _rows.Clear();
        _rows.Add(new Row(RowKind.Skin));
        foreach (var slot in new[] { CosmeticSlot.Smoke, CosmeticSlot.Painting, CosmeticSlot.Radio, CosmeticSlot.Costume, CosmeticSlot.MusicBox, CosmeticSlot.Pet })
            _rows.Add(new Row(RowKind.Cosmetic, slot));
        foreach (var wagon in new[] { UpgradeType.CargoWagon, UpgradeType.PassengerWagon, UpgradeType.ArmoredWagon })
            if (SaveData.HasUpgrade(wagon)) _rows.Add(new Row(RowKind.Wagon, Wagon: wagon));
        _cursor = Math.Clamp(_cursor, 0, _rows.Count - 1);
    }

    public void Close() => IsOpen = false;

    private static List<TrainSkinRef> OwnedSkins() =>
        Cosmetics.TrainSkins.Where(s => SaveData.Current.OwnedSkins.Contains(s.Id) && DLCManager.Owns(s.Dlc))
            .Select(s => new TrainSkinRef(s.Id, s.Name)).ToList();

    private sealed record TrainSkinRef(string Id, string Name);

    private static CosmeticItem[] Owned(CosmeticSlot slot) => CosmeticCatalog.For(slot).Where(CosmeticCatalog.IsOwned).ToArray();

    /// <returns>Trene yansıması gereken bir değişiklik olduysa true (Game1 profili yeniden uygular).</returns>
    public bool Update(float dt, InputState input, Cabin cabin, ProceduralAudio audio)
    {
        _time += dt;
        _open = MathF.Min(1f, _open + dt * 5f);
        _messageTimer -= dt;

        if (input.AnyPressed(Keys.G, Keys.Back))
        {
            Close();
            audio.PlayClick();
            return false;
        }

        int before = _cursor;
        if (input.AnyPressed(Keys.W, Keys.Up)) _cursor = (_cursor - 1 + _rows.Count) % _rows.Count;
        if (input.AnyPressed(Keys.S, Keys.Down)) _cursor = (_cursor + 1) % _rows.Count;
        if (_cursor != before) audio.PlayClick();

        int dir = input.AnyPressed(Keys.A, Keys.Left) ? -1 : input.AnyPressed(Keys.D, Keys.Right) || input.AnyPressed(Keys.Enter) ? 1 : 0;
        if (dir == 0) return false;

        var row = _rows[_cursor];
        switch (row.Kind)
        {
            case RowKind.Skin:
            {
                var skins = OwnedSkins();
                int i = skins.FindIndex(s => s.Id == SaveData.Current.EquippedSkin);
                var next = skins[(Math.Max(0, i) + dir + skins.Count) % skins.Count];
                SaveData.Current.EquippedSkin = next.Id;
                SaveData.Save();
                break;
            }
            case RowKind.Cosmetic:
            {
                var items = Owned(row.Slot);
                if (items.Length <= 1)
                {
                    Show(Loc.T("garage.noMore"));
                    audio.PlayError();
                    return false;
                }
                int i = Array.FindIndex(items, it => it.Id == DLCManager.Active(row.Slot).Id);
                SaveData.TryEquip(items[(Math.Max(0, i) + dir + items.Length) % items.Length]);
                break;
            }
            default:
            {
                string key = row.Wagon.ToString();
                bool attached = !SaveData.Current.DetachedWagons.Contains(key);
                // Yolcu vagonundaki yolcular inmeden vagon ayrılamaz
                if (attached && row.Wagon == UpgradeType.PassengerWagon && cabin.Count > GameConfig.CabinCapacity)
                {
                    Show(Loc.T("garage.wagonBusy"));
                    audio.PlayError();
                    return false;
                }
                if (attached) SaveData.Current.DetachedWagons.Add(key);
                else SaveData.Current.DetachedWagons.Remove(key);
                SaveData.Save();
                break;
            }
        }
        audio.PlayCoin();
        return true;
    }

    private void Show(string text)
    {
        _message = text;
        _messageTimer = 2.5f;
    }

    // ------------------------------------------------------------------
    // Çizim
    // ------------------------------------------------------------------

    public void Draw(SpriteBatch sb)
    {
        float a = _open;
        // Yalnızca üst kısım kararır: alttaki tren (vagonlar, skin, duman) değişiklikleri canlı gösterir
        sb.FillRect(new Rectangle(0, 0, GameConfig.ScreenWidth, 440), Color.Black * (0.6f * a));
        var panel = Panel;
        panel.Y += (int)((1f - a) * 20f);
        UiDraw.FancyPanel(sb, panel, Hud.Amber * a);
        GameText.Draw(sb, Loc.T("garage.title"), new Vector2(panel.X + 24, panel.Y + 16), Hud.Amber * a, 3);

        int y = panel.Y + 62;
        for (int i = 0; i < _rows.Count; i++)
        {
            var row = _rows[i];
            bool selected = i == _cursor;
            var rect = new Rectangle(panel.X + 16, y, panel.Width - 32, 36);
            if (selected)
            {
                sb.FillRect(rect, Hud.Amber * (0.15f * a));
                sb.DrawBorder(rect, 1, Hud.Amber * a);
            }

            string label, value;
            Color valueColor = Color.White;
            switch (row.Kind)
            {
                case RowKind.Skin:
                    label = Loc.T("garage.skin");
                    value = DLCManager.ActiveSkin(SaveData.Current.EquippedSkin).Name;
                    break;
                case RowKind.Cosmetic:
                    label = Loc.T(row.Slot == CosmeticSlot.Pet ? "garage.pet" : $"custom.slot{(int)row.Slot}");
                    value = DLCManager.Active(row.Slot).Name;
                    break;
                default:
                    label = UpgradeCatalog.Name(row.Wagon);
                    bool attached = !SaveData.Current.DetachedWagons.Contains(row.Wagon.ToString());
                    value = Loc.T(attached ? "garage.attached" : "garage.detached");
                    valueColor = attached ? CabinPanel.Good : Hud.Label;
                    break;
            }
            GameText.Draw(sb, Hud.FitWidth(label, 230), new Vector2(rect.X + 10, rect.Y + 8), Hud.Label * a);
            string shown = selected ? $"<  {value}  >" : value;
            GameText.DrawRight(sb, Hud.FitWidth(shown, 300), rect.Right - 10, rect.Y + 8, valueColor * a);
            y += 38;
        }

        // Sağda kabin içi önizleme (tablo, radyo, kostüm, evcil hayvan)
        var preview = new Rectangle(670, 80, 570, 300);
        UiDraw.FancyPanel(sb, preview, Hud.PanelEdge * a);
        CabinDecor.DrawInterior(sb, new Rectangle(preview.X + 10, preview.Y + 10, preview.Width - 20, preview.Height - 20),
            DLCManager.Active(CosmeticSlot.Painting), DLCManager.Active(CosmeticSlot.Radio), DLCManager.Active(CosmeticSlot.Costume),
            a, _time, (PetKind)DLCManager.Active(CosmeticSlot.Pet).Value);

        // Evcil hayvanın etkisi
        var pet = (PetKind)DLCManager.Active(CosmeticSlot.Pet).Value;
        string petInfo = pet == PetKind.None ? Loc.T("garage.petNone") : Loc.T("garage.petPerk");
        float infoY = preview.Bottom + 10;
        foreach (string line in GameText.Wrap(petInfo, preview.Width, 2))
        {
            GameText.Draw(sb, line, new Vector2(preview.X, infoY), Hud.Label * a);
            infoY += GameText.LineHeight(2) + 4;
        }

        if (_message != null && _messageTimer > 0f)
            GameText.DrawCentered(sb, _message, panel.Center.X, panel.Bottom - 64, Hud.Danger * MathHelper.Clamp(_messageTimer, 0f, 1f));
        GameText.Draw(sb, Loc.T("garage.hint"), new Vector2(panel.X + 24, panel.Bottom - 32), Hud.Label * (0.85f * a));
    }
}
