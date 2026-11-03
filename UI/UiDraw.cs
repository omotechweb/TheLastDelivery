using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheLastDelivery.Graphics;

namespace TheLastDelivery.UI;

/// <summary>
/// Göstergenin ekranda yumuşak hareket etmesi için iki değer tutar:
///   Display — gerçek değere hızla yaklaşan görünen doluluk,
///   Trail   — kaybedilen miktarı kısa süre açık renkle gösteren "iz" (hasar/tüketim geri bildirimi).
/// </summary>
public sealed class SmoothedValue
{
    private bool _initialized;

    public float Display { get; private set; }
    public float Trail { get; private set; }

    public void Update(float target, float dt)
    {
        if (!_initialized)
        {
            Display = Trail = target;
            _initialized = true;
            return;
        }

        Display = MathHelper.Lerp(Display, target, 1f - MathF.Exp(-dt * 12f));
        Trail = target >= Trail ? target : MathF.Max(target, Trail - dt * 0.35f);
    }
}

/// <summary>Şık arayüz çizim yardımcıları: gradyanlı, çerçeveli, parlak barlar.</summary>
public static class UiDraw
{
    /// <summary>Rengi saydamlaştırmadan aydınlatır/koyulaştırır.</summary>
    public static Color Shade(Color color, float factor) =>
        new(Vector3.Clamp(color.ToVector3() * factor, Vector3.Zero, Vector3.One));

    /// <summary>
    /// Çerçeveli, dikey gradyanlı bar: koyu iç zemin, kayıp izi, üstte parlama çizgisi,
    /// dolu kısmın ucunda parlak kenar, %25'lik çentikler. <paramref name="alarm"/> > 0 ise kırmızı nabız.
    /// </summary>
    public static void FancyBar(SpriteBatch sb, Rectangle rect, float ratio, float trail, Color fill, float alarm = 0f)
    {
        ratio = MathHelper.Clamp(ratio, 0f, 1f);
        trail = MathHelper.Clamp(trail, ratio, 1f);

        // Dış gölge ve iç zemin
        sb.FillRect(new Rectangle(rect.X - 2, rect.Y - 2, rect.Width + 4, rect.Height + 4), new Color(3, 4, 8));
        sb.FillRect(rect, new Color(16, 18, 28));
        sb.FillRect(new Rectangle(rect.X, rect.Y, rect.Width, 2), new Color(8, 9, 14)); // İç gölge

        // Kayıp izi: yeni harcanan miktar kısa süre açık renkte kalır
        int fillWidth = (int)(rect.Width * ratio);
        int trailWidth = (int)(rect.Width * trail);
        if (trailWidth > fillWidth)
            sb.FillRect(new Rectangle(rect.X + fillWidth, rect.Y, trailWidth - fillWidth, rect.Height), new Color(255, 240, 220) * 0.35f);

        // Dikey gradyan: üst açık, alt koyu
        var top = Shade(fill, 1.35f);
        var bottom = Shade(fill, 0.6f);
        for (int row = 0; row < rect.Height; row++)
        {
            float t = rect.Height <= 1 ? 0f : row / (float)(rect.Height - 1);
            sb.FillRect(new Rectangle(rect.X, rect.Y + row, fillWidth, 1), Color.Lerp(top, bottom, t));
        }

        if (fillWidth > 2)
        {
            sb.FillRect(new Rectangle(rect.X, rect.Y + 1, fillWidth, 1), Color.White * 0.3f);       // Parlama
            sb.FillRect(new Rectangle(rect.X + fillWidth - 2, rect.Y, 2, rect.Height), Shade(fill, 1.7f)); // Uç kenar
        }

        // %25'lik çentikler
        for (int i = 1; i < 4; i++)
        {
            int x = rect.X + rect.Width * i / 4;
            sb.FillRect(new Rectangle(x, rect.Y + rect.Height - 4, 1, 4), new Color(0, 0, 0) * 0.5f);
        }

        if (alarm > 0f)
            sb.FillRect(rect, new Color(255, 60, 40) * (0.35f * alarm));

        // Çerçeve: üst/sol açık, alt/sağ koyu kenar (kabartma hissi)
        sb.FillRect(new Rectangle(rect.X - 1, rect.Y - 1, rect.Width + 2, 1), new Color(110, 120, 150));
        sb.FillRect(new Rectangle(rect.X - 1, rect.Y - 1, 1, rect.Height + 2), new Color(90, 100, 130));
        sb.FillRect(new Rectangle(rect.X - 1, rect.Bottom, rect.Width + 2, 1), new Color(40, 44, 60));
        sb.FillRect(new Rectangle(rect.Right, rect.Y - 1, 1, rect.Height + 2), new Color(40, 44, 60));
    }

    /// <summary>Tek bir bölmeli gösterge hücresi: gradyanlı dolgu, parlama ve kabartma çerçeve.</summary>
    public static void FancyCell(SpriteBatch sb, Rectangle cell, float fill, Color color, float alarm = 0f)
    {
        sb.FillRect(cell, new Color(16, 18, 28));
        int width = (int)(cell.Width * MathHelper.Clamp(fill, 0f, 1f));
        if (width > 0)
        {
            var top = Shade(color, 1.35f);
            var bottom = Shade(color, 0.6f);
            for (int row = 0; row < cell.Height; row++)
            {
                float t = row / (float)Math.Max(1, cell.Height - 1);
                sb.FillRect(new Rectangle(cell.X, cell.Y + row, width, 1), Color.Lerp(top, bottom, t));
            }
            sb.FillRect(new Rectangle(cell.X, cell.Y + 1, width, 1), Color.White * 0.3f);
        }
        if (alarm > 0f)
            sb.FillRect(cell, new Color(255, 60, 40) * (0.3f * alarm));

        sb.FillRect(new Rectangle(cell.X, cell.Y, cell.Width, 1), new Color(100, 110, 140));
        sb.FillRect(new Rectangle(cell.X, cell.Bottom - 1, cell.Width, 1), new Color(30, 34, 48));
    }

    /// <summary>Panel arka planı: hafif dikey gradyan, iç kenar parlaması ve köşe süsleri.</summary>
    public static void FancyPanel(SpriteBatch sb, Rectangle rect, Color edge)
    {
        var top = new Color(14, 17, 30) * 0.86f;
        var bottom = new Color(5, 6, 12) * 0.86f;
        const int bands = 8;
        for (int i = 0; i < bands; i++)
        {
            int y0 = rect.Y + rect.Height * i / bands;
            int y1 = rect.Y + rect.Height * (i + 1) / bands;
            sb.FillRect(new Rectangle(rect.X, y0, rect.Width, y1 - y0), Color.Lerp(top, bottom, i / (float)(bands - 1)));
        }

        sb.DrawBorder(rect, 2, edge);
        sb.FillRect(new Rectangle(rect.X + 2, rect.Y + 2, rect.Width - 4, 1), Color.White * 0.06f);

        // Köşe süsleri
        var corner = Shade(edge, 1.5f);
        foreach (var (x, y) in new[] { (rect.X, rect.Y), (rect.Right - 6, rect.Y), (rect.X, rect.Bottom - 6), (rect.Right - 6, rect.Bottom - 6) })
            sb.FillRect(new Rectangle(x, y, 6, 6), corner);
    }

    /// <summary>Kredi simgesi: kenarı koyu, ortası "K" kabartmalı altın para.</summary>
    public static void CreditIcon(SpriteBatch sb, Point center, float alpha)
    {
        sb.Draw(Assets.Circle, new Rectangle(center.X - 11, center.Y - 11, 22, 22), new Color(150, 100, 30) * alpha);
        sb.Draw(Assets.Circle, new Rectangle(center.X - 9, center.Y - 9, 18, 18), new Color(255, 205, 90) * alpha);
        sb.FillRect(new Rectangle(center.X - 4, center.Y - 5, 2, 10), new Color(170, 110, 30) * alpha);
        sb.FillRect(new Rectangle(center.X - 2, center.Y - 1, 2, 2), new Color(170, 110, 30) * alpha);
        sb.FillRect(new Rectangle(center.X, center.Y - 5, 2, 4), new Color(170, 110, 30) * alpha);
        sb.FillRect(new Rectangle(center.X, center.Y + 1, 2, 4), new Color(170, 110, 30) * alpha);
        sb.FillRect(new Rectangle(center.X - 5, center.Y - 7, 3, 2), Color.White * (0.6f * alpha));
    }

    /// <summary>Tren geliştirmesi simgeleri.</summary>
    public static void UpgradeIcon(SpriteBatch sb, Economy.UpgradeType type, Point c)
    {
        void Box(int x, int y, int w, int h, Color col) => sb.FillRect(new Rectangle(c.X + x, c.Y + y, w, h), col);
        switch (type)
        {
            case Economy.UpgradeType.FuelTank:
                Box(-12, -8, 24, 16, new Color(90, 96, 110));
                Box(-12, -8, 24, 3, new Color(170, 176, 190));
                Box(-8, -4, 16, 8, new Color(220, 160, 70));
                break;
            case Economy.UpgradeType.Heater:
                Box(-10, -10, 20, 20, new Color(120, 60, 40));
                for (int x = -8; x < 10; x += 5) Box(x, -8, 3, 16, new Color(255, 140, 60));
                sb.DrawGlow(c.ToVector2(), 18f, new Color(255, 130, 50), 0.4f);
                break;
            case Economy.UpgradeType.CargoBay:
                Box(-12, -8, 24, 16, new Color(110, 80, 50));
                Box(-12, -8, 24, 2, new Color(150, 110, 70));
                Box(-1, -8, 2, 16, new Color(70, 50, 34));
                Box(-12, -1, 24, 2, new Color(70, 50, 34));
                break;
            case Economy.UpgradeType.BattleArmor: // Perçinli kalkan, ortasında pençe izi
                Box(-12, -12, 24, 22, new Color(90, 96, 110));
                Box(-9, 10, 18, 3, new Color(90, 96, 110));
                Box(-10, -10, 20, 3, new Color(170, 176, 190));
                for (int i = 0; i < 3; i++)
                    Box(-6 + i * 5, -5 + i, 2, 10, new Color(200, 60, 50));
                Box(-11, -11, 3, 3, new Color(220, 220, 230));
                Box(8, -11, 3, 3, new Color(220, 220, 230));
                break;
            case Economy.UpgradeType.Lantern: // Işık saçan fener
                sb.DrawGlow(c.ToVector2(), 22f, new Color(255, 220, 140), 0.5f);
                Box(-3, -14, 6, 3, new Color(60, 56, 50));
                Box(-7, -11, 14, 3, new Color(80, 76, 70));
                Box(-6, -8, 12, 14, new Color(255, 215, 120));
                Box(-7, 6, 14, 4, new Color(80, 76, 70));
                Box(-1, -8, 2, 14, new Color(255, 245, 210));
                break;
            case Economy.UpgradeType.CargoWagon: // Sandıklı platform vagonu
                Box(-14, 2, 28, 5, new Color(110, 80, 50));
                Box(-12, -8, 11, 10, new Color(150, 110, 70));
                Box(1, -12, 11, 14, new Color(130, 92, 56));
                Box(-6, -8, 2, 10, new Color(90, 64, 40));
                Box(-10, 7, 5, 5, new Color(40, 40, 46));
                Box(5, 7, 5, 5, new Color(40, 40, 46));
                break;
            case Economy.UpgradeType.PassengerWagon: // Pencereleri yanan yolcu vagonu
                Box(-15, -12, 30, 3, new Color(40, 40, 50));
                Box(-14, -9, 28, 15, new Color(110, 60, 46));
                for (int i = 0; i < 3; i++) Box(-11 + i * 9, -6, 6, 6, new Color(255, 190, 110));
                Box(-10, 7, 5, 5, new Color(40, 40, 46));
                Box(5, 7, 5, 5, new Color(40, 40, 46));
                break;
            case Economy.UpgradeType.ArmoredWagon: // Perçinli zırhlı vagon ve namlu
                Box(-15, -8, 30, 15, new Color(70, 74, 84));
                Box(-6, -13, 12, 5, new Color(52, 56, 64));
                Box(6, -12, 10, 2, new Color(140, 146, 160));
                for (int i = 0; i < 4; i++) Box(-12 + i * 7, -5, 2, 2, new Color(190, 196, 210));
                Box(-10, 7, 5, 5, new Color(40, 40, 46));
                Box(5, 7, 5, 5, new Color(40, 40, 46));
                break;
            default:
                Box(-10, -11, 20, 20, new Color(110, 116, 130));
                Box(-7, 9, 14, 3, new Color(110, 116, 130));
                Box(-6, -8, 12, 14, new Color(150, 156, 170));
                Box(-2, -6, 4, 10, new Color(90, 96, 110));
                break;
        }
    }

    /// <summary>
    /// Destekçi Rozeti (Supporter Pack): koyu zemin üzerinde altın yıldız ve "DESTEKÇİ" yazısı.
    /// </summary>
    /// <returns>Rozetin genişliği (yanına yerleşim için).</returns>
    public static int SupporterBadge(SpriteBatch sb, Point topLeft, float alpha, float time)
    {
        string label = Localization.Loc.T("badge.supporter");
        int width = (int)GameText.MeasureLine(label) + 34;
        var rect = new Rectangle(topLeft.X, topLeft.Y, width, 24);
        var gold = new Color(255, 205, 90);
        float shine = 0.75f + 0.25f * MathF.Sin(time * 3f);

        sb.FillRect(rect, new Color(40, 28, 10) * (0.9f * alpha));
        sb.DrawBorder(rect, 1, gold * (alpha * shine));
        // Beş köşeli yıldız yerine piksel yıldız: artı + köşegen noktalar
        int cx = rect.X + 13, cy = rect.Center.Y;
        sb.FillRect(new Rectangle(cx - 1, cy - 7, 3, 14), gold * alpha);
        sb.FillRect(new Rectangle(cx - 7, cy - 1, 14, 3), gold * alpha);
        sb.FillRect(new Rectangle(cx - 4, cy - 4, 9, 9), gold * alpha);
        sb.DrawGlow(new Vector2(cx, cy), 14f, gold, 0.4f * alpha * shine);
        GameText.Draw(sb, label, new Vector2(rect.X + 26, rect.Y + 2), gold * alpha);
        return width;
    }

    /// <summary>Basılabilir tuş görünümü: gölgeli, kabartmalı küçük kutu içinde harf.</summary>
    public static void KeyCap(SpriteBatch sb, Rectangle rect, string key, Color color, float pulse = 0f)
    {
        sb.FillRect(new Rectangle(rect.X, rect.Y + 3, rect.Width, rect.Height), new Color(0, 0, 0) * 0.6f);
        sb.FillRect(rect, Shade(color, 0.35f + 0.15f * pulse));
        sb.FillRect(new Rectangle(rect.X, rect.Y, rect.Width, 2), Shade(color, 1.2f));
        sb.DrawBorder(rect, 2, color);
        GameText.DrawCentered(sb, key, rect.Center.X, rect.Y + rect.Height / 2f - 11, Color.White, 2, Localization.Language.English);
    }
}
