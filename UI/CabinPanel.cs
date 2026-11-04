using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheLastDelivery.Core;
using TheLastDelivery.Graphics;
using TheLastDelivery.Localization;
using TheLastDelivery.Passengers;
using TheLastDelivery.World;

namespace TheLastDelivery.UI;

/// <summary>
/// Sağ üstteki "Kabin Panosu": tren sağlığı, yakıt ve paket bütünlüğü
/// (yeşil/sarı/kırmızı bölmeli göstergeler) ile kabindeki yolcuların listesi.
/// </summary>
public static class CabinPanel
{
    private const int Segments = 10;
    private const int SegmentWidth = 20;
    private const int SegmentGap = 3;
    private const int RowHeight = 32;

    // Yükseklik yatak sayısına göre hesaplanır (Yolcu Vagonu ile 6 yatak)
    private const int PanelX = 836, PanelY = 16, PanelWidth = 428;
    private static Rectangle Panel = new(PanelX, PanelY, PanelWidth, 350);

    /// <summary>AURA'nın sahte yakıt göstergesi (oran sapması; Game1 her kare ayarlar).</summary>
    public static float FuelLie { get; set; }

    /// <summary>Eğitimde vurgulanan yakıt satırı.</summary>
    public static readonly Rectangle FuelRow = new(PanelX + 8, PanelY + 74, PanelWidth - 16, 32);

    private static readonly Color HungerColor = new(235, 170, 80);
    private static readonly Color WarmthColor = new(255, 120, 70);
    private static readonly Color ColdColor = new(130, 190, 255);

    public static readonly Color Good = new(90, 200, 110);
    public static readonly Color Caution = new(230, 200, 70);
    public static readonly Color Critical = new(240, 75, 60);
    public static readonly Color Suspicious = new(200, 130, 255);

    // Animasyon durumu: değerler yumuşakça değişir, kayıplar kısa süre iz bırakır
    private static readonly SmoothedValue HealthBar = new();
    private static readonly SmoothedValue FuelBar = new();
    private static readonly SmoothedValue PackageBar = new();
    private static float _lastTime;

    public static void Draw(SpriteBatch sb, Train train, Cabin cabin, float time, int rations = -1)
    {
        // 3 yatakta tek sütun, 6 yatakta (Yolcu Vagonu) iki sütun: panel her durumda 3 satır yüksekliğinde kalır,
        // böylece sağa kayan lokomotifin ve far konisinin üstünü örtmez
        bool compact = cabin.Capacity > GameConfig.CabinCapacity;
        const int rowHeight = 56, listOffset = 228;
        int rows = compact ? (cabin.Capacity + 1) / 2 : cabin.Capacity;
        Panel = new Rectangle(PanelX, PanelY, PanelWidth, listOffset + rows * rowHeight + 6);

        int left = Panel.X + 16;
        float dt = Math.Clamp(time - _lastTime, 0f, 0.1f);
        _lastTime = time;
        HealthBar.Update(train.Health / train.MaxHealth, dt);
        float shownFuel = Microsoft.Xna.Framework.MathHelper.Clamp(train.FuelRatio + FuelLie, 0f, 1f); // AURA'nın sahte yakıt göstergesi
        FuelBar.Update(shownFuel, dt);
        PackageBar.Update(train.Package / GameConfig.MaxPackage, dt);

        UiDraw.FancyPanel(sb, Panel, Hud.PanelEdge);

        GameText.Draw(sb, Loc.T("cabin.title"), new Vector2(left, Panel.Y + 14), Hud.Amber);

        // Sağ üst: Kredi (kalıcı para birimi)
        string credits = Loc.F("credits.count", Economy.SaveData.Current.Credits);
        float creditsWidth = GameText.MeasureLine(credits);
        GameText.DrawRight(sb, credits, Panel.Right - 16, Panel.Y + 14, new Color(255, 215, 110));
        UiDraw.CreditIcon(sb, new Point((int)(Panel.Right - 16 - creditsWidth - 16), Panel.Y + 24), 1f);

        // Bölmeli çubuklar en uzun etiketin sağından başlar (etiket uzunluğu dile göre değişir)
        string health = Loc.T("cabin.health"), fuel = Loc.T("cabin.fuel"), package = Loc.T("cabin.package");
        float labelWidth = MathF.Max(GameText.MeasureLine(health), MathF.Max(GameText.MeasureLine(fuel), GameText.MeasureLine(package)));
        int barX = left + (int)MathF.Max(96f, labelWidth + 12f);

        int y = Panel.Y + 48;
        DrawSegmentedBar(sb, health, y, barX, HealthBar, train.Health / train.MaxHealth, time);
        DrawSegmentedBar(sb, fuel, y + RowHeight, barX, FuelBar, shownFuel, time);
        DrawSegmentedBar(sb, package, y + RowHeight * 2, barX, PackageBar, train.Package / GameConfig.MaxPackage, time);

        y += RowHeight * 3;
        sb.FillRect(new Rectangle(left, y, Panel.Width - 32, 2), Hud.PanelEdge * 0.6f);

        // Kabin sobası: sönerse yolcular üşür ([C] ile kömür atılır)
        float stove = cabin.Stove / 100f;
        Color stoveColor = stove > 0.4f ? WarmthColor : stove > 0.2f ? Caution : Critical;
        float stoveAlarm = stove < 0.2f && cabin.Count > 0 ? 0.5f + 0.5f * MathF.Sin(time * 6f) : 0f;
        GameText.Draw(sb, Loc.T("cabin.stove"), new Vector2(left, y + 10), Color.Lerp(Hud.Label, Critical, stoveAlarm * 0.7f));
        var stoveBar = new Rectangle(barX, y + 14, 150, 12);
        UiDraw.FancyBar(sb, stoveBar, stove, stove, stoveColor, stoveAlarm);
        UiDraw.KeyCap(sb, new Rectangle(stoveBar.Right + 10, y + 8, 22, 22), "C", Hud.Amber);
        GameText.DrawRight(sb, Loc.F("fmt.percent", (int)MathF.Round(stove * 100f)), Panel.Right - 14, y + 10, Hud.Value);

        y += 34;
        GameText.Draw(sb, Loc.F("cabin.passengers", cabin.Count, cabin.Capacity), new Vector2(left, y + 12), Hud.Label);

        // Sağda erzak çuvalı ve [G] paylaştır tuşu
        if (rations >= 0)
        {
            string count = $"X{rations}";
            float countWidth = GameText.MeasureLine(count);
            GameText.DrawRight(sb, count, Panel.Right - 16, y + 12, rations == 0 ? Critical : Hud.Value);
            InventoryPanel.DrawRationIcon(sb, new Point((int)(Panel.Right - 16 - countWidth - 14), y + 22));
            UiDraw.KeyCap(sb, new Rectangle((int)(Panel.Right - 16 - countWidth - 52), y + 10, 22, 22), "G", Hud.Amber);
        }

        int listTop = y + 42;
        int fullWidth = Panel.Width - 32;
        int cellWidth = compact ? (fullWidth - 12) / 2 : fullWidth;
        for (int i = 0; i < cabin.Capacity; i++)
        {
            // Sütun sütun doldurulur: 1-3 solda (ana vagon), 4-6 sağda (yolcu vagonu)
            int column = compact ? i / rows : 0;
            int cellX = left + column * (cellWidth + 12);
            int rowY = listTop + (compact ? i % rows : i) * rowHeight;
            int cellRight = cellX + cellWidth;

            if (i >= cabin.Count)
            {
                GameText.Draw(sb, Loc.T("cabin.empty"), new Vector2(cellX + 14, rowY + 14), Hud.Label * 0.4f);
                continue;
            }

            var p = cabin.Passengers[i];
            var conditionColor = ConditionColor(p.Condition);

            // Solda canlılık çubuğu: yolcunun yaşam gücü (dikey)
            var strip = new Rectangle(cellX, rowY + 2, 7, rowHeight - 12);
            sb.FillRect(new Rectangle(strip.X - 1, strip.Y - 1, strip.Width + 2, strip.Height + 2), new Color(3, 4, 8));
            sb.FillRect(strip, new Color(16, 18, 28));
            int fill = (int)(strip.Height * p.Vitality / 100f);
            float pulse = p.Vitality < GameConfig.CriticalVitality ? 0.6f + 0.4f * MathF.Sin(time * 6f) : 1f;
            for (int x = 0; x < strip.Width; x++) // Yatay gradyan: silindir hissi
            {
                float shade = 1.3f - MathF.Abs(x - strip.Width / 2f) / strip.Width;
                sb.FillRect(new Rectangle(strip.X + x, strip.Bottom - fill, 1, fill), UiDraw.Shade(conditionColor, shade) * pulse);
            }

            // Sağ üstte şüphe: suçüstü yakalanan hain kırmızı "HAİN", diğerleri şüphe yüzdesi
            string tag = p.IsRevealed ? Loc.T("cabin.traitor")
                : p.Suspicion >= 1f ? Loc.F(compact ? "fmt.percent" : "cabin.suspicion", (int)MathF.Round(p.Suspicion)) : null;
            float tagWidth = tag != null ? GameText.MeasureLine(tag) + 8 : 0f;
            if (tag != null)
            {
                var tagColor = p.IsRevealed ? Critical * (0.7f + 0.3f * MathF.Sin(time * 6f))
                    : Color.Lerp(Hud.Label, Suspicious, p.Suspicion / 100f);
                GameText.DrawRight(sb, tag, cellRight, rowY, tagColor);
            }

            // Ad (iki sütunda yalnızca ad, tek sütunda ad - meslek) ve durum
            string name = compact ? p.Name : $"{p.Name} - {p.Profession}";
            GameText.Draw(sb, Hud.FitWidth(name, cellWidth - 16 - tagWidth), new Vector2(cellX + 16, rowY), Hud.Value);
            GameText.Draw(sb, Hud.FitWidth($"({p.ConditionText})", cellWidth - 16), new Vector2(cellX + 16, rowY + 20), conditionColor);

            // Tek sütunda sağda yolcunun yeteneği
            if (!compact)
                GameText.DrawRight(sb, p.Profile.SkillTag, cellRight, rowY + 20, Hud.Amber * 0.8f);

            // Altta üç ince ihtiyaç çubuğu: tokluk (ekmek), vücut sıcaklığı (alev) ve moral (kalp)
            int needY = rowY + rowHeight - 12;
            int third = (cellWidth - 16) / 3;
            DrawNeed(sb, cellX + 16, needY, third - 6, p.Hunger / 100f, HungerColor, NeedIcon.Bread, time);
            DrawNeed(sb, cellX + 16 + third, needY, third - 6, p.Warmth / 100f,
                Color.Lerp(ColdColor, WarmthColor, p.Warmth / 100f), NeedIcon.Flame, time);
            DrawNeed(sb, cellX + 16 + third * 2, needY, third - 2, p.Morale / 100f, MoraleColor, NeedIcon.Heart, time);
        }

        // İki sütun arasında ince ayraç (ana vagon | yolcu vagonu)
        if (compact)
            sb.FillRect(new Rectangle(left + cellWidth + 5, listTop, 2, rows * rowHeight - 6), Hud.PanelEdge * 0.4f);
    }

    private enum NeedIcon { Bread, Flame, Heart }
    private static readonly Color MoraleColor = new(240, 120, 170);

    /// <summary>Simge + ince çubuk; değer düşükse çubuk yanıp söner.</summary>
    private static void DrawNeed(SpriteBatch sb, int x, int y, int width, float value, Color color, NeedIcon icon, float time)
    {
        value = MathHelper.Clamp(value, 0f, 1f);
        float alarm = value < 0.35f ? 0.5f + 0.5f * MathF.Sin(time * 7f) : 0f;
        if (icon == NeedIcon.Bread)
        {
            sb.FillRect(new Rectangle(x, y - 1, 9, 6), new Color(200, 140, 70));
            sb.FillRect(new Rectangle(x + 1, y - 2, 7, 2), new Color(235, 180, 100));
        }
        else if (icon == NeedIcon.Flame)
        {
            sb.FillRect(new Rectangle(x + 2, y - 3, 5, 8), color);
            sb.FillRect(new Rectangle(x + 3, y - 1, 3, 5), new Color(255, 230, 160));
        }
        else // Kalp
        {
            sb.FillRect(new Rectangle(x + 1, y - 2, 3, 3), color);
            sb.FillRect(new Rectangle(x + 5, y - 2, 3, 3), color);
            sb.FillRect(new Rectangle(x + 1, y, 7, 2), color);
            sb.FillRect(new Rectangle(x + 2, y + 2, 5, 2), color);
            sb.FillRect(new Rectangle(x + 4, y + 4, 1, 1), color);
        }

        var bar = new Rectangle(x + 14, y, width - 14, 5);
        sb.FillRect(bar, new Color(16, 18, 28));
        sb.FillRect(new Rectangle(bar.X, bar.Y, (int)(bar.Width * value), bar.Height),
            Color.Lerp(color, Critical, alarm * 0.8f));
    }

    /// <summary>
    /// 10 bölmeli gösterge: gradyanlı hücreler, renk değere göre yeşil → sarı → kırmızı.
    /// Kritik seviyede bölmeler yumuşak bir kırmızı nabızla uyarır; az önce kaybedilen kısım açık renkte söner.
    /// </summary>
    private static void DrawSegmentedBar(SpriteBatch sb, string label, int y, int barX, SmoothedValue value, float actual, float time)
    {
        float ratio = MathHelper.Clamp(value.Display, 0f, 1f);
        Color color = ratio > 0.6f ? Good : ratio > 0.3f ? Caution : Critical;
        float alarm = actual <= 0.3f ? 0.5f + 0.5f * MathF.Sin(time * 6f) : 0f;

        GameText.Draw(sb, label, new Vector2(Panel.X + 16, y), Color.Lerp(Hud.Label, Critical, alarm * 0.7f));

        float filled = ratio * Segments;
        float trail = MathHelper.Clamp(value.Trail, 0f, 1f) * Segments;
        for (int i = 0; i < Segments; i++)
        {
            var cell = new Rectangle(barX + i * (SegmentWidth + SegmentGap), y + 2, SegmentWidth, 16);

            // Son bölme kısmen dolu olabilir
            UiDraw.FancyCell(sb, cell, filled - i, color, alarm);

            // Kayıp izi
            float lost = MathHelper.Clamp(trail - i, 0f, 1f) - MathHelper.Clamp(filled - i, 0f, 1f);
            if (lost > 0.02f)
            {
                int start = cell.X + (int)(cell.Width * MathHelper.Clamp(filled - i, 0f, 1f));
                sb.FillRect(new Rectangle(start, cell.Y, Math.Max(1, (int)(cell.Width * lost)), cell.Height), Color.White * 0.35f);
            }
        }

        GameText.DrawRight(sb, Loc.F("fmt.percent", (int)MathF.Round(actual * 100f)), Panel.Right - 14, y, Hud.Value);
    }

    private static Color ConditionColor(PassengerCondition condition) => condition switch
    {
        PassengerCondition.Stable => Good,
        PassengerCondition.Warming => Caution,
        PassengerCondition.Freezing => new Color(140, 200, 255),
        PassengerCondition.Poisoned => new Color(170, 230, 90),
        _ => Critical // Hasta, Yaralı
    };
}
