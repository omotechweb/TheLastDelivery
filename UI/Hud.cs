using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheLastDelivery.Core;
using TheLastDelivery.Graphics;
using TheLastDelivery.Localization;
using TheLastDelivery.World;

namespace TheLastDelivery.UI;

/// <summary>
/// Sol üstteki sürüş paneli (hız, kazan, mesafe) ve alttaki kontrol ipuçları.
/// Barlar gradyanlı ve çerçevelidir; değerler yumuşakça değişir, kayıplar kısa bir iz bırakır.
/// Hız değişirken değer parlar, kritik uyarılar yavaşça belirir.
/// </summary>
public static class Hud
{
    private static readonly Rectangle BasePanel = new(16, 16, 404, 218);
    private static readonly Rectangle SanityPanel = new(16, 16, 404, 252); // Akıl sağlığı satırıyla
    private static Rectangle Panel = BasePanel;
    private static readonly SmoothedValue SanityBar = new();
    private const int Left = 32;

    public static readonly Color PanelBack = new Color(6, 8, 16) * 0.78f;
    // Çerçeve, vurgu ve etiket renkleri kuşanılan kabin temasından gelir
    public static Color PanelEdge => Economy.Cosmetics.Theme(Economy.SaveData.Current.EquippedTheme).Edge;
    public static Color Label => Economy.Cosmetics.Theme(Economy.SaveData.Current.EquippedTheme).Label;
    public static readonly Color Value = new(225, 230, 240);
    public static Color Amber => Economy.Cosmetics.Theme(Economy.SaveData.Current.EquippedTheme).Accent;
    public static readonly Color Danger = new(255, 90, 70);

    /// <summary>Eğitimde vurgulanan satırlar (hız ve kazan göstergeleri).</summary>
    public static readonly Rectangle SpeedRow = new(24, 58, 388, 32);
    public static readonly Rectangle TemperatureRow = new(24, 90, 388, 32);

    // Animasyon durumu
    private static readonly SmoothedValue SpeedBar = new();
    private static readonly SmoothedValue TemperatureBar = new();
    private static float _lastTime;
    private static float _lastSpeed;
    private static float _speedFlash;
    private static float _warningAlpha;
    private static string _lastWarning;

    /// <param name="title">Panel başlığı: aktif bölümün ya da kasabanın adı.</param>
    /// <param name="sanity">Arthur'un akıl sağlığı (0-100); negatifse satır gösterilmez.</param>
    public static void Draw(SpriteBatch sb, string title, Train train, float remainingKm, float kmToNextStop,
        bool nextIsShelter, float time, float sanity = -1f)
    {
        float dt = Math.Clamp(time - _lastTime, 0f, 0.1f);
        _lastTime = time;
        Panel = sanity >= 0f ? SanityPanel : BasePanel;

        SpeedBar.Update(train.SpeedRatio, dt);
        TemperatureBar.Update(train.Temperature / GameConfig.MaxTemperature, dt);

        // Hız değişirken değer parlar, durunca yavaşça normale döner
        if (MathF.Abs(train.Speed - _lastSpeed) > 0.01f) _speedFlash = 1f;
        else _speedFlash = MathF.Max(0f, _speedFlash - dt * 2.5f);
        _lastSpeed = train.Speed;

        UiDraw.FancyPanel(sb, Panel, PanelEdge);
        GameText.Draw(sb, FitWidth(title, Panel.Width - 32), new Vector2(Left, 30), Amber);

        // Etiketlerin genişliği dile göre değişir; çubuklar en uzun etiketin sağından başlar
        string speedLabel = Loc.T("hud.speed");
        string boilerLabel = Loc.T("hud.boiler");
        float labelWidth = MathF.Max(MathF.Max(GameText.MeasureLine(speedLabel), GameText.MeasureLine(boilerLabel)),
            sanity >= 0f ? GameText.MeasureLine(Loc.T("hud.sanity")) : 0f);
        int barX = Left + (int)MathF.Max(72f, labelWidth + 10f);

        // Hız: hasarlı trende ulaşılamayan bölge kırmızı bir çentikle işaretlenir
        var speedColor = Color.Lerp(Value, Color.White, _speedFlash);
        var speedBar = DrawGauge(sb, speedLabel, 64, barX, SpeedBar, new Color(150, 180, 225),
            Loc.F("hud.kmh", (int)MathF.Round(train.Speed)), speedColor, 0f);
        if (train.MaxAllowedSpeed < GameConfig.MaxSpeed - 0.5f)
        {
            int limitX = speedBar.X + (int)(speedBar.Width * train.MaxAllowedSpeed / GameConfig.MaxSpeed);
            sb.FillRect(new Rectangle(limitX, speedBar.Y, speedBar.Right - limitX, speedBar.Height), Danger * 0.3f);
            sb.FillRect(new Rectangle(limitX, speedBar.Y - 3, 2, speedBar.Height + 6), Danger);
        }

        // Kazan sıcaklığı: soğukta mavi, idealde turuncu, aşırı ısınmada kırmızı; donmaya yakınken nabız
        float tempRatio = train.Temperature / GameConfig.MaxTemperature;
        float coldAlarm = train.Temperature < GameConfig.LowTemperatureWarning ? 0.5f + 0.5f * MathF.Sin(time * 6f) : 0f;
        DrawGauge(sb, boilerLabel, 96, barX, TemperatureBar, TemperatureColor(tempRatio),
            $"{(int)MathF.Round(train.Temperature)}°C", Value, coldAlarm);

        sb.FillRect(new Rectangle(Left, 128, 372, 2), PanelEdge * 0.6f);

        GameText.Draw(sb, Loc.F("hud.shelter", (int)MathF.Ceiling(remainingKm)), new Vector2(Left, 140), Value);
        string next = Loc.F(nextIsShelter ? "hud.target" : "hud.next", (int)MathF.Ceiling(kmToNextStop));
        GameText.Draw(sb, next, new Vector2(Left, 168), Label);

        // Akıl sağlığı: %30'un altında kızıl nabız (sanrılar başlar)
        int warningY = 200;
        if (sanity >= 0f)
        {
            SanityBar.Update(sanity / 100f, dt);
            float ratio = sanity / 100f;
            bool mad = sanity < GameConfig.HallucinationThreshold;
            float alarm = mad ? 0.5f + 0.5f * MathF.Sin(time * 4f) : 0f;
            var color = Color.Lerp(new Color(190, 90, 200), new Color(150, 200, 255), ratio);
            DrawGauge(sb, Loc.T("hud.sanity"), 198, barX, SanityBar, color, Loc.F("fmt.percent", (int)MathF.Round(sanity)),
                mad ? Danger : Value, alarm);
            warningY = 232;
        }

        // Kritik uyarı: aniden yanıp sönmek yerine yavaşça belirir ve hafifçe nabız atar
        string warning = GetWarning(train);
        if (warning != null) _lastWarning = warning;
        _warningAlpha = MathHelper.Clamp(_warningAlpha + (warning != null ? dt * 2.5f : -dt * 3f), 0f, 1f);
        if (_lastWarning != null && _warningAlpha > 0f)
        {
            float pulse = 0.75f + 0.25f * MathF.Sin(time * 5f);
            GameText.Draw(sb, _lastWarning, new Vector2(Left, warningY), Danger * (_warningAlpha * pulse));
        }
    }

    public static void DrawControlsHint(SpriteBatch sb)
    {
        GameText.Draw(sb, Loc.T("hud.controls"), new Vector2(24, GameConfig.ScreenHeight - 32), new Color(150, 160, 185) * 0.7f);
    }

    /// <summary>Ekranın ortasında yavaşça beliren bildirim (durağa yaklaşırken).</summary>
    public static void DrawBanner(SpriteBatch sb, string text, float time, float timeVisible = 10f)
    {
        float fadeIn = MathHelper.Clamp(timeVisible / 0.6f, 0f, 1f);
        float alpha = fadeIn * (0.7f + 0.3f * MathF.Sin(time * 4f));
        GameText.DrawCentered(sb, text, GameConfig.ScreenWidth / 2f + 2, 292, Color.Black * (0.6f * alpha), 3);
        GameText.DrawCentered(sb, text, GameConfig.ScreenWidth / 2f, 290, Amber * alpha, 3);
    }

    private static Rectangle DrawGauge(SpriteBatch sb, string label, int y, int barX, SmoothedValue value,
        Color fill, string text, Color textColor, float alarm)
    {
        GameText.Draw(sb, label, new Vector2(Left, y), Label);

        // Değer sütunu sabit; çubuk etiketle değer arasındaki boşluğu doldurur
        const int valueX = 286;
        var bar = new Rectangle(barX, y + 3, Math.Max(100, valueX - 14 - barX), 16);
        UiDraw.FancyBar(sb, bar, value.Display, value.Trail, fill, alarm);

        GameText.Draw(sb, text, new Vector2(bar.Right + 14, y), textColor);
        return bar;
    }

    /// <summary>Panele sığmayan başlığı kısaltır: "BÖLÜM 3: ERKEN ERİME VE BU..."</summary>
    public static string FitWidth(string text, float maxWidth)
    {
        if (GameText.MeasureLine(text) <= maxWidth) return text;
        for (int length = text.Length - 1; length > 0; length--)
        {
            string candidate = text.Substring(0, length).TrimEnd() + "...";
            if (GameText.MeasureLine(candidate) <= maxWidth) return candidate;
        }
        return text;
    }

    private static Color TemperatureColor(float ratio)
    {
        var cold = new Color(80, 150, 255);
        var warm = new Color(255, 150, 50);
        return ratio < 0.6f
            ? Color.Lerp(cold, warm, ratio / 0.6f)
            : Color.Lerp(warm, Danger, (ratio - 0.6f) / 0.4f);
    }

    private static string GetWarning(Train train)
    {
        if (train.Temperature < GameConfig.LowTemperatureWarning) return Loc.T("warn.freezing");
        if (train.Health < GameConfig.LowHealthWarning) return Loc.T("warn.damaged");
        if (train.Package < GameConfig.LowPackageWarning) return Loc.T("warn.package");
        if (train.Fuel < GameConfig.LowFuelWarning) return Loc.T("warn.fuel");
        if (train.IsOverheating) return Loc.T("warn.overheat");
        return null;
    }
}
