using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TheLastDelivery.Audio;
using TheLastDelivery.Core;
using TheLastDelivery.Graphics;
using TheLastDelivery.Localization;
using TheLastDelivery.Passengers;

namespace TheLastDelivery.UI;

/// <summary>
/// Kabin sorgusu ([K]): yolcuların şüphe düzeyleri ve durumları listelenir;
/// şüphelenilen yolcu trenden kovulabilir (geride bırakılır). Açıkken oyun zamanı durur.
///   W/S ya da 1-3: yolcu seç   ENTER: kov (iki kez basarak onayla)   K / ESC: kapat
/// </summary>
public sealed class CabinMenu
{
    private static readonly Rectangle Panel = new(250, 110, 780, 470);
    private static readonly Color SuspicionColor = new(200, 130, 255);

    private int _cursor;
    private bool _confirming;
    private float _open;
    private float _time;

    public bool IsOpen { get; private set; }

    public void Open()
    {
        IsOpen = true;
        _cursor = 0;
        _confirming = false;
        _open = 0f;
    }

    public void Close() => IsOpen = false;

    /// <returns>Kovulmasına karar verilen yolcu (yoksa null).</returns>
    public Passenger Update(float dt, InputState input, Cabin cabin, ProceduralAudio audio)
    {
        _time += dt;
        _open = MathF.Min(1f, _open + dt * 5f);

        if (input.AnyPressed(Keys.K, Keys.Back))
        {
            Close();
            audio.PlayClick();
            return null;
        }
        if (cabin.Count == 0) return null;
        _cursor = Math.Clamp(_cursor, 0, cabin.Count - 1);

        int previous = _cursor;
        if (input.AnyPressed(Keys.W, Keys.Up)) _cursor = (_cursor - 1 + cabin.Count) % cabin.Count;
        if (input.AnyPressed(Keys.S, Keys.Down)) _cursor = (_cursor + 1) % cabin.Count;
        for (int i = 0; i < cabin.Count; i++)
            if (input.AnyPressed((Keys)((int)Keys.D1 + i), (Keys)((int)Keys.NumPad1 + i))) _cursor = i;
        if (_cursor != previous)
        {
            _confirming = false;
            audio.PlayClick();
        }

        if (!input.AnyPressed(Keys.Enter, Keys.E)) return null;

        if (!_confirming)
        {
            _confirming = true; // İlk basış: onay iste
            audio.PlayAlarm();
            return null;
        }

        _confirming = false;
        Close();
        return cabin.Passengers[_cursor];
    }

    public void Draw(SpriteBatch sb, Cabin cabin)
    {
        float a = _open;
        sb.FillRect(new Rectangle(0, 0, GameConfig.ScreenWidth, GameConfig.ScreenHeight), Color.Black * (0.55f * a));
        var panel = Panel;
        panel.Y += (int)((1f - a) * 24f);
        UiDraw.FancyPanel(sb, panel, SuspicionColor * a);

        GameText.Draw(sb, Loc.T("cabinMenu.title"), new Vector2(panel.X + 28, panel.Y + 20), SuspicionColor * a, 3);
        GameText.Draw(sb, Hud.FitWidth(Loc.T("cabinMenu.desc"), panel.Width - 220), new Vector2(panel.X + 28, panel.Y + 62), Hud.Label * a);

        // Kabin iç tasarımı (Vintage, Komuta Odası, Hologram kokpit) üst şeritte
        var interior = Economy.Cosmetics.Theme(Economy.SaveData.Current.EquippedTheme).Interior;
        if (interior != Economy.CabinInterior.Classic)
            CabinDecor.DrawBackdrop(sb, new Rectangle(panel.Right - 200, panel.Y + 12, 186, 76), interior, a * 0.9f, _time);
        // Sağ üstte kabin süsleri: duvardaki tablo ve çalan radyo (kozmetik paket)
        CabinDecor.DrawPainting(sb, new Rectangle(panel.Right - 180, panel.Y + 22, 64, 46),
            Platform.DLCManager.Active(Economy.CosmeticSlot.Painting), a);
        CabinDecor.DrawRadio(sb, new Point(panel.Right - 70, panel.Y + 72),
            Platform.DLCManager.Active(Economy.CosmeticSlot.Radio), a, _time);

        if (cabin.Count == 0)
        {
            GameText.DrawCentered(sb, Loc.T("cabinMenu.empty"), panel.Center.X, panel.Y + 200, Hud.Label * a);
        }

        // 6 yolcuda (Yolcu Vagonu) satırlar sıkışır, panelden taşmaz
        bool compact = cabin.Count > 3;
        int step = compact ? 54 : 100, height = compact ? 50 : 90;
        for (int i = 0; i < cabin.Count; i++)
        {
            var p = cabin.Passengers[i];
            bool selected = i == _cursor;
            var row = new Rectangle(panel.X + 24, panel.Y + 100 + i * step, panel.Width - 48, height);

            if (selected)
            {
                sb.FillRect(row, (_confirming ? Hud.Danger : SuspicionColor) * (0.14f * a));
                sb.DrawBorder(row, 2, (_confirming ? Hud.Danger : SuspicionColor) * a);
            }
            else
            {
                sb.DrawBorder(row, 1, Hud.PanelEdge * (0.6f * a));
            }

            int portrait = compact ? 40 : 70;
            int textX = row.X + portrait + 26;
            Portraits.Draw(sb, new Rectangle(row.X + 10, row.Y + 5, portrait, portrait), Portraits.ForPassenger(p.Profile.Id), a);
            GameText.Draw(sb, $"{i + 1}. {p.Name} - {p.Profession}", new Vector2(textX, row.Y + (compact ? 4 : 10)), Hud.Value * a);
            if (!compact)
                GameText.Draw(sb, $"({p.ConditionText})  {p.Profile.SkillTag}", new Vector2(textX, row.Y + 38), Hud.Label * a);

            // Şüphe çubuğu
            var bar = new Rectangle(textX, row.Y + (compact ? 30 : 66), 260, 12);
            float ratio = p.Suspicion / 100f;
            UiDraw.FancyBar(sb, bar, ratio, ratio, p.IsRevealed ? Hud.Danger : SuspicionColor);
            string label = p.IsRevealed ? Loc.T("cabin.traitor") : Loc.F("cabinMenu.suspicion", (int)MathF.Round(p.Suspicion));
            GameText.Draw(sb, label, new Vector2(bar.Right + 14, bar.Y - 4), (p.IsRevealed ? Hud.Danger : SuspicionColor) * a);

            if (selected)
            {
                string action = _confirming ? Loc.F("cabinMenu.confirm", p.Name) : Loc.T("cabinMenu.eject");
                float pulse = _confirming ? 0.6f + 0.4f * MathF.Sin(_time * 10f) : 1f;
                GameText.DrawRight(sb, action, row.Right - 14, row.Y + 10, (_confirming ? Hud.Danger : Hud.Amber) * (a * pulse));
            }
        }

        GameText.Draw(sb, Loc.T("cabinMenu.hint"), new Vector2(panel.X + 28, panel.Bottom - 36), Hud.Label * (0.8f * a));
    }
}
