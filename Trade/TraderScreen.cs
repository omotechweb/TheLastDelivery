using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TheLastDelivery.Audio;
using TheLastDelivery.Core;
using TheLastDelivery.Economy;
using TheLastDelivery.Graphics;
using TheLastDelivery.Items;
using TheLastDelivery.Localization;
using TheLastDelivery.Passengers;
using TheLastDelivery.UI;
using TheLastDelivery.World;

namespace TheLastDelivery.Trade;

/// <summary>Tüccarın bulunduğu yer: neler satıldığını belirler.</summary>
public enum TraderKind
{
    Depot, // Buzul benzinliği (bölüm ortası)
    Town,  // Kasabadaki erzak deposu
    Hub    // Bölüm sonu ana istasyonu: tren geliştirmeleri de satılır
}

/// <summary>
/// Tüccar arayüzü: iki sekme.
///   SAT       — kargodaki yağmaları Krediye çevir (tek tek ya da [X] ile hepsini birden)
///   SATIN AL  — yakıt (pompa mini oyunu), tamir, kullanılabilir eşyalar, ana istasyonda kalıcı geliştirmeler
/// Kontroller: [A/D] sekme, [W/S] satır, [ENTER/BOŞLUK] seç, [X] hepsini sat.
/// Tüccar yolcu (Lina) varsa satışlar daha kârlı, alışlar daha ucuzdur.
/// </summary>
public sealed class TraderScreen
{
    private enum Tab { Sell, Buy }
    private enum Phase { Menu, Pumping }

    private enum RowKind { Loot, SellAll, Fuel, Repair, Rations, Item, Upgrade, Leave }

    private sealed record Row(RowKind Kind, LootType Loot = default, ItemType Item = default, UpgradeType Upgrade = default);

    private static readonly Rectangle Panel = new(170, 40, 940, 600);
    private const int MaxVisibleRows = 9;               // Uzun listeler (ana istasyon) kayarak gösterilir
    private int RowHeight => _rows.Count > 8 ? 40 : 46;
    private int _scroll;

    private readonly PumpMinigame _pump = new();
    private readonly List<Row> _rows = new();
    private TraderKind _kind;
    private string _title;
    private Tab _tab;
    private Phase _phase;
    private int _cursor;
    private string _message;
    private Color _messageColor;
    private float _messageTimer;
    private float _time;
    private float _open;

    public bool IsFinished { get; private set; }

    /// <summary>Kalıcı bir geliştirme satın alındı (Game1 trene hemen uygular).</summary>
    public event Action UpgradePurchased;

    /// <summary>Tüccar ekranının kendi bildirim satırında mesaj gösterir (örn. bölüm tamamlama ödülü).</summary>
    public void Notify(string message, Color color) => Show(message, color);

    public void Open(TraderKind kind, string title)
    {
        _kind = kind;
        _title = title;
        _tab = Tab.Sell;
        _phase = Phase.Menu;
        _cursor = 0;
        _message = null;
        _messageTimer = 0f;
        _time = 0f;
        _open = 0f;
        IsFinished = false;
        BuildRows();
    }

    private void BuildRows()
    {
        _rows.Clear();
        if (_tab == Tab.Sell)
        {
            foreach (var loot in LootCatalog.All)
                _rows.Add(new Row(RowKind.Loot, Loot: loot));
            _rows.Add(new Row(RowKind.SellAll));
        }
        else
        {
            _rows.Add(new Row(RowKind.Fuel));
            _rows.Add(new Row(RowKind.Repair));
            _rows.Add(new Row(RowKind.Rations));
            foreach (var item in ItemCatalog.All)
                _rows.Add(new Row(RowKind.Item, Item: item));
            if (_kind == TraderKind.Hub)
                foreach (var upgrade in UpgradeCatalog.All)
                    _rows.Add(new Row(RowKind.Upgrade, Upgrade: upgrade));
        }
        _rows.Add(new Row(RowKind.Leave));
        _cursor = Math.Clamp(_cursor, 0, _rows.Count - 1);
        _scroll = 0;
        KeepCursorVisible();
    }

    private void KeepCursorVisible()
    {
        if (_cursor < _scroll) _scroll = _cursor;
        if (_cursor >= _scroll + MaxVisibleRows) _scroll = _cursor - MaxVisibleRows + 1;
        _scroll = Math.Clamp(_scroll, 0, Math.Max(0, _rows.Count - MaxVisibleRows));
    }

    // ------------------------------------------------------------------
    // Fiyatlar
    // ------------------------------------------------------------------

    private static int SellPrice(LootType loot, Cabin cabin) =>
        (int)MathF.Round(LootCatalog.Price(loot) * (cabin.Has(PassengerSkill.Trader) ? GameConfig.TraderSellBonus : 1f));

    private static int Discount(int price, Cabin cabin) =>
        (int)MathF.Round(price * (cabin.Has(PassengerSkill.Trader) ? GameConfig.TraderBuyDiscount : 1f));

    private static int ItemPrice(ItemType item) => item switch
    {
        ItemType.ThermalCoal => GameConfig.CoalPrice,
        ItemType.Medkit => GameConfig.MedkitPrice,
        ItemType.RareMedkit => GameConfig.RareMedkitPrice,
        ItemType.WolfBait => GameConfig.WolfBaitPrice,
        _ => GameConfig.PartsPrice
    };

    private int RowPrice(Row row, Cabin cabin) => row.Kind switch
    {
        RowKind.Loot => SellPrice(row.Loot, cabin),
        RowKind.Fuel => Discount(GameConfig.FuelPackagePrice, cabin),
        RowKind.Repair => Discount(GameConfig.RepairPrice, cabin),
        RowKind.Item => Discount(ItemPrice(row.Item), cabin),
        RowKind.Rations => Discount(GameConfig.RationPackPrice, cabin),
        RowKind.Upgrade => UpgradeCatalog.Price(row.Upgrade), // Kalıcı geliştirmelerde indirim yok
        _ => 0
    };

    // ------------------------------------------------------------------
    // Güncelleme
    // ------------------------------------------------------------------

    public void Update(float dt, InputState input, Train train, Inventory inventory, Cabin cabin, ProceduralAudio audio)
    {
        _time += dt;
        _open = MathF.Min(1f, _open + dt * 4f);
        _messageTimer -= dt;

        if (_phase == Phase.Pumping)
        {
            bool tapped = input.Pressed(Keys.Space);
            if (tapped) audio.PlayPump();
            _pump.Update(dt, tapped);

            if (_pump.IsDone)
            {
                int fuel = (int)MathF.Round(GameConfig.FuelPackageAmount * _pump.Yield);
                train.Refuel(fuel);
                Show(Loc.F("pump.result", fuel), CabinPanel.Good);
                audio.PlaySuccess();
                _phase = Phase.Menu;
            }
            return;
        }

        // Sekme değiştir
        if (input.AnyPressed(Keys.A, Keys.D, Keys.Left, Keys.Right, Keys.Tab))
        {
            _tab = _tab == Tab.Sell ? Tab.Buy : Tab.Sell;
            _cursor = 0;
            BuildRows();
            audio.PlayClick();
        }

        if (input.AnyPressed(Keys.W, Keys.Up)) { _cursor = (_cursor - 1 + _rows.Count) % _rows.Count; audio.PlayClick(); }
        if (input.AnyPressed(Keys.S, Keys.Down)) { _cursor = (_cursor + 1) % _rows.Count; audio.PlayClick(); }
        KeepCursorVisible();

        if (input.Pressed(Keys.X))
            SellAll(inventory, cabin, audio);
        else if (input.AnyPressed(Keys.Enter, Keys.Space))
            Execute(_rows[_cursor], train, inventory, cabin, audio);
    }

    private void Execute(Row row, Train train, Inventory inventory, Cabin cabin, ProceduralAudio audio)
    {
        int price = RowPrice(row, cabin);
        switch (row.Kind)
        {
            case RowKind.Leave:
                IsFinished = true;
                audio.PlayConfirm();
                return;

            case RowKind.SellAll:
                SellAll(inventory, cabin, audio);
                return;

            case RowKind.Loot:
                if (!inventory.Cargo.TryRemove(row.Loot))
                {
                    Fail(Loc.F("trade.noLoot", LootCatalog.Name(row.Loot)), audio);
                    return;
                }
                SaveData.Earn(price);
                Show(Loc.F("trade.sold", LootCatalog.Name(row.Loot), price), CabinPanel.Good);
                audio.PlayCoin();
                return;

            case RowKind.Fuel:
                if (train.FuelRatio >= 0.999f) { Fail(Loc.T("trade.tankFull"), audio); return; }
                if (!SaveData.TrySpend(price)) { Fail(Loc.T("trade.noCredits"), audio); return; }
                _pump.Start();
                _phase = Phase.Pumping;
                audio.PlayConfirm();
                return;

            case RowKind.Repair:
                if (train.IsFullHealth) { Fail(Loc.T("item.trainOk"), audio); return; }
                if (!SaveData.TrySpend(price)) { Fail(Loc.T("trade.noCredits"), audio); return; }
                train.Repair(GameConfig.RepairAmount);
                Show(Loc.F("sum.health", (int)GameConfig.RepairAmount), CabinPanel.Good);
                audio.PlayExplosion(); // Çekiç sesi
                return;

            case RowKind.Rations:
                if (inventory.Rations >= GameConfig.MaxRations) { Fail(Loc.T("rations.full"), audio); return; }
                if (!SaveData.TrySpend(price)) { Fail(Loc.T("trade.noCredits"), audio); return; }
                Show(Loc.F("rations.bought", inventory.AddRations(Difficulty.Scale(GameConfig.RationPack))), CabinPanel.Good);
                audio.PlayCoin();
                return;

            case RowKind.Item:
                if (!SaveData.TrySpend(price)) { Fail(Loc.T("trade.noCredits"), audio); return; }
                if (!inventory.TryAdd(row.Item))
                {
                    SaveData.Earn(price); // Yer yoksa para iade
                    Fail(Loc.T("store.full"), audio);
                    return;
                }
                Show(Loc.F("store.bought", ItemCatalog.Name(row.Item)), CabinPanel.Good);
                audio.PlayCoin();
                return;

            case RowKind.Upgrade:
                if (SaveData.HasUpgrade(row.Upgrade)) { Fail(Loc.T("trade.owned"), audio); return; }
                if (!SaveData.TryBuyUpgrade(row.Upgrade)) { Fail(Loc.T("trade.noCredits"), audio); return; }
                UpgradePurchased?.Invoke();
                Show(Loc.F("trade.upgraded", UpgradeCatalog.Name(row.Upgrade)), CabinPanel.Good);
                audio.PlaySuccess();
                return;
        }
    }

    /// <summary>"Tek tıkla" satış: kargodaki her şey Krediye çevrilir.</summary>
    private void SellAll(Inventory inventory, Cabin cabin, ProceduralAudio audio)
    {
        int total = 0, count = 0;
        foreach (var loot in LootCatalog.All)
        {
            while (inventory.Cargo.TryRemove(loot))
            {
                total += SellPrice(loot, cabin);
                count++;
            }
        }

        if (count == 0)
        {
            Fail(Loc.T("trade.cargoEmpty"), audio);
            return;
        }

        SaveData.Earn(total);
        Show(Loc.F("trade.soldAll", count, total), CabinPanel.Good);
        audio.PlayCoin();
        audio.PlaySuccess();
    }

    private void Fail(string message, ProceduralAudio audio)
    {
        Show(message, Hud.Danger);
        audio.PlayError();
    }

    private void Show(string message, Color color)
    {
        _message = message;
        _messageColor = color;
        _messageTimer = 3f;
    }

    // ------------------------------------------------------------------
    // Çizim
    // ------------------------------------------------------------------

    public void Draw(SpriteBatch sb, Train train, Inventory inventory, Cabin cabin)
    {
        sb.FillRect(new Rectangle(0, 0, GameConfig.ScreenWidth, GameConfig.ScreenHeight), Color.Black * (0.55f * _open));

        // Hafifçe aşağıdan kayarak açılır
        var panel = Panel;
        panel.Y += (int)((1f - _open) * 30f);
        float a = _open;
        UiDraw.FancyPanel(sb, panel, Hud.Amber * a);

        int left = panel.X + 28;
        int right = panel.Right - 28;

        GameText.Draw(sb, _title, new Vector2(left, panel.Y + 18), Hud.Amber * a, 3);
        DrawCredits(sb, right, panel.Y + 22, a);

        // Durum satırı: kargo, yakıt ve sağlık
        int statusY = panel.Y + 64;
        string cargo = Loc.F("trade.cargo", inventory.Cargo.Total, inventory.Cargo.Capacity);
        GameText.Draw(sb, cargo, new Vector2(left, statusY), (inventory.Cargo.IsFull ? Hud.Danger : Hud.Label) * a);
        DrawMiniBar(sb, Loc.T("cabin.fuel"), left + 300, statusY, train.FuelRatio, new Color(220, 160, 70), a);
        DrawMiniBar(sb, Loc.T("cabin.health"), left + 590, statusY, train.Health / train.MaxHealth, CabinPanel.Good, a);

        sb.FillRect(new Rectangle(left, statusY + 30, right - left, 2), Hud.PanelEdge * (0.6f * a));

        if (_phase == Phase.Pumping)
        {
            DrawPump(sb, left, right, statusY + 60);
        }
        else
        {
            DrawTabs(sb, left, statusY + 42, a);
            DrawRows(sb, left, right, statusY + 90, inventory, cabin, a);
            GameText.Draw(sb, Loc.T("trade.hint"), new Vector2(left, panel.Bottom - 34), Hud.Label * (0.8f * a));
        }

        // Bildirim: sekmelerin sağında, listeyle çakışmadan
        if (_message != null && _messageTimer > 0f)
            GameText.DrawRight(sb, _message, right, statusY + 50, _messageColor * MathHelper.Clamp(_messageTimer, 0f, 1f));
    }

    /// <summary>Sağ üstte Kredi bakiyesi ve madeni para simgesi.</summary>
    public static void DrawCredits(SpriteBatch sb, float right, float y, float alpha = 1f)
    {
        string text = Loc.F("credits.count", SaveData.Current.Credits);
        float width = GameText.MeasureLine(text, 3);
        GameText.Draw(sb, text, new Vector2(right - width, y - 4), new Color(255, 215, 110) * alpha, 3);
        UiDraw.CreditIcon(sb, new Point((int)(right - width - 20), (int)y + 12), alpha);
    }

    private static void DrawMiniBar(SpriteBatch sb, string label, int x, int y, float ratio, Color color, float alpha)
    {
        GameText.Draw(sb, label, new Vector2(x, y), Hud.Label * alpha);
        float labelWidth = GameText.MeasureLine(label);
        var bar = new Rectangle(x + (int)labelWidth + 12, y + 3, 150, 14);
        UiDraw.FancyBar(sb, bar, ratio, ratio, color);
    }

    private void DrawTabs(SpriteBatch sb, int left, int y, float alpha)
    {
        void TabButton(int x, string text, bool active)
        {
            float width = GameText.MeasureLine(text) + 40;
            var rect = new Rectangle(x, y, (int)width, 36);
            sb.FillRect(rect, (active ? Hud.Amber * 0.25f : new Color(10, 12, 20) * 0.8f) * alpha);
            sb.DrawBorder(rect, 2, (active ? Hud.Amber : Hud.PanelEdge) * alpha);
            GameText.DrawCentered(sb, text, rect.Center.X, y + 8, (active ? Color.White : Hud.Label) * alpha);
        }

        string sell = Loc.T("trade.tab.sell");
        TabButton(left, sell, _tab == Tab.Sell);
        TabButton(left + (int)GameText.MeasureLine(sell) + 56, Loc.T("trade.tab.buy"), _tab == Tab.Buy);
    }

    private void DrawRows(SpriteBatch sb, int left, int right, int top, Inventory inventory, Cabin cabin, float alpha)
    {
        int last = Math.Min(_rows.Count, _scroll + MaxVisibleRows);

        // Kaydırma okları: listenin üstünde/altında daha fazla satır var
        int mid = (left + right) / 2;
        if (_scroll > 0)
            for (int i = 0; i < 4; i++)
                sb.FillRect(new Rectangle(mid - 2 - i * 2, top - 12 + i * 2, 4 + i * 4, 2), Hud.Amber * alpha);
        if (last < _rows.Count)
        {
            int arrowY = top + MaxVisibleRows * RowHeight;
            for (int i = 0; i < 4; i++)
                sb.FillRect(new Rectangle(mid - 8 + i * 2, arrowY + i * 2, 16 - i * 4, 2), Hud.Amber * alpha);
        }

        for (int i = _scroll; i < last; i++)
        {
            var row = _rows[i];
            int y = top + (i - _scroll) * RowHeight;
            bool selected = i == _cursor;
            var rect = new Rectangle(left, y, right - left, RowHeight - 6);

            if (selected)
            {
                float pulse = 0.18f + 0.06f * MathF.Sin(_time * 6f);
                sb.FillRect(rect, Hud.Amber * (pulse * alpha));
                sb.DrawBorder(rect, 2, Hud.Amber * alpha);
                sb.FillRect(new Rectangle(rect.X - 10, rect.Center.Y - 5, 6, 10), Hud.Amber * alpha);
            }

            var (name, detail, rightText, enabled, icon) = Describe(row, inventory, cabin);
            Color nameColor = (enabled ? Hud.Value : Hud.Label * 0.6f) * alpha;

            icon?.Invoke(sb, new Point(rect.X + 26, rect.Center.Y));
            GameText.Draw(sb, name, new Vector2(rect.X + 56, rect.Y + 9), nameColor);
            if (detail.Text != null)
                GameText.Draw(sb, detail.Text, new Vector2(rect.X + 56 + GameText.MeasureLine(name) + 16, rect.Y + 9), detail.Color * alpha);
            if (rightText.Text != null)
                GameText.DrawRight(sb, rightText.Text, rect.Right - 14, rect.Y + 9, rightText.Color * alpha);
        }
    }

    private (string Name, (string Text, Color Color) Detail, (string Text, Color Color) Right, bool Enabled, Action<SpriteBatch, Point> Icon)
        Describe(Row row, Inventory inventory, Cabin cabin)
    {
        var gold = new Color(255, 215, 110);
        int price = RowPrice(row, cabin);
        bool affordable = SaveData.Current.Credits >= price;

        switch (row.Kind)
        {
            case RowKind.Loot:
            {
                int count = inventory.Cargo.Count(row.Loot);
                return (LootCatalog.Name(row.Loot),
                    (Loc.T(LootCatalog.RarityKey(row.Loot)), LootCatalog.Color(row.Loot)),
                    ($"X{count}   +{price}", count > 0 ? gold : Hud.Label * 0.5f),
                    count > 0,
                    (s, p) => LootCatalog.DrawIcon(s, row.Loot, p));
            }
            case RowKind.SellAll:
            {
                int total = 0;
                foreach (var loot in LootCatalog.All) total += inventory.Cargo.Count(loot) * SellPrice(loot, cabin);
                return (Loc.T("trade.sellAll"), ("[X]", Hud.Amber), ($"+{total}", total > 0 ? gold : Hud.Label * 0.5f), total > 0,
                    (s, p) => UiDraw.CreditIcon(s, p, 1f));
            }
            case RowKind.Fuel:
                return (Loc.T("trade.buyFuel"), (Loc.F("trade.upTo", (int)GameConfig.FuelPackageAmount), Hud.Label),
                    ($"-{price}", affordable ? gold : Hud.Danger), affordable,
                    (s, p) => s.FillRect(new Rectangle(p.X - 7, p.Y - 10, 14, 20), new Color(220, 160, 70)));
            case RowKind.Repair:
                return (Loc.T("trade.repair"), (Loc.F("sum.health", (int)GameConfig.RepairAmount).TrimStart('>', ' '), Hud.Label),
                    ($"-{price}", affordable ? gold : Hud.Danger), affordable,
                    (s, p) => InventoryPanel.DrawIcon(s, ItemType.SpareParts, p));
            case RowKind.Rations:
                return (Loc.F("rations.pack", GameConfig.RationPack),
                    (Loc.F("rations.count", inventory.Rations, GameConfig.MaxRations), Hud.Label),
                    ($"-{price}", affordable ? gold : Hud.Danger), affordable && inventory.Rations < GameConfig.MaxRations,
                    (s, p) => InventoryPanel.DrawRationIcon(s, p));
            case RowKind.Item:
                return (ItemCatalog.Name(row.Item), (Loc.F("trade.owned.count", inventory.CountOf(row.Item)), Hud.Label),
                    ($"-{price}", affordable ? gold : Hud.Danger), affordable,
                    (s, p) => InventoryPanel.DrawIcon(s, row.Item, p));
            case RowKind.Upgrade:
            {
                bool owned = SaveData.HasUpgrade(row.Upgrade);
                return (UpgradeCatalog.Name(row.Upgrade), (UpgradeCatalog.Description(row.Upgrade), Hud.Label),
                    owned ? (Loc.T("trade.owned"), CabinPanel.Good) : ($"-{price}", affordable ? gold : Hud.Danger),
                    !owned && affordable,
                    (s, p) => UiDraw.UpgradeIcon(s, row.Upgrade, p));
            }
            default:
                return (Loc.T("trade.leave"), (null, Color.White), (null, Color.White), true,
                    (s, p) => s.FillRect(new Rectangle(p.X - 8, p.Y - 2, 16, 4), Hud.Amber));
        }
    }

    /// <summary>Pompa: basınç ibresi (yeşil bölge), depo doluluğu ve kalan süre.</summary>
    private void DrawPump(SpriteBatch sb, int left, int right, float y)
    {
        float centerX = (left + right) / 2f;
        GameText.DrawCentered(sb, Loc.T("pump.title"), centerX, y, Hud.Amber, 3);
        GameText.DrawCentered(sb, Loc.T("pump.instr"), centerX, y + 44, Hud.Value);

        int barX = left + 130;
        int barWidth = right - barX;
        GameText.Draw(sb, Loc.T("pump.pressure"), new Vector2(left, y + 96), Hud.Label);
        var gauge = new Rectangle(barX, (int)y + 92, barWidth, 26);
        sb.FillRect(gauge, new Color(40, 20, 20));
        int greenX = gauge.X + (int)(gauge.Width * GameConfig.PumpGreenMin);
        int greenW = (int)(gauge.Width * (GameConfig.PumpGreenMax - GameConfig.PumpGreenMin));
        sb.FillRect(new Rectangle(greenX, gauge.Y, greenW, gauge.Height), CabinPanel.Good * (_pump.InGreen ? 0.8f : 0.45f));
        sb.DrawBorder(gauge, 2, Hud.PanelEdge);
        int needleX = gauge.X + (int)(gauge.Width * MathHelper.Clamp(_pump.Pressure, 0f, 1f));
        sb.FillRect(new Rectangle(needleX - 2, gauge.Y - 6, 4, gauge.Height + 12), Color.White);

        GameText.Draw(sb, Loc.T("pump.tank"), new Vector2(left, y + 146), Hud.Label);
        var tank = new Rectangle(barX, (int)y + 148, barWidth, 18);
        UiDraw.FancyBar(sb, tank, _pump.Fill, _pump.Fill, new Color(220, 160, 70));

        if (_pump.Pressure < GameConfig.PumpGreenMin)
        {
            float pulse = 0.5f + 0.5f * MathF.Sin(_time * 14f);
            UiDraw.KeyCap(sb, new Rectangle((int)centerX - 60, (int)y + 196, 120, 34), "SPACE", Hud.Amber, pulse);
        }

        GameText.DrawRight(sb, _pump.TimeLeft.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + "s", right, y + 200, Hud.Label);
    }
}
