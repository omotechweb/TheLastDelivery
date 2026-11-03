using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheLastDelivery.Core;
using TheLastDelivery.Graphics;
using TheLastDelivery.Localization;

namespace TheLastDelivery.World;

/// <summary>
/// Yol kenarındaki yapıları çizer: terk edilmiş istasyonlar ve hattın sonundaki Sığınak-0.
/// Yapılar dünyada sabittir; ekrandaki konumları trenin katettiği yola göre hesaplanır.
/// </summary>
public static class StationRenderer
{
    private const int BaseY = GameConfig.GroundY + 8; // Binaların oturduğu kar çizgisi

    /// <summary>Durak kilometresini, trenin konumuna göre ekrandaki X merkezine çevirir.</summary>
    public static float ScreenX(float stopKm, float travelledKm)
        => GameConfig.StationStopScreenX + Train.LayoutOffsetX + (stopKm - travelledKm) * GameConfig.PixelsPerKm; // Ek vagonlarla kayan trenle birlikte

    public static bool IsVisible(float screenX) => screenX > -400 && screenX < GameConfig.ScreenWidth + 400;

    /// <summary>Kırık pencereli, lambası titreyen terk edilmiş istasyon binası.</summary>
    public static void DrawAbandonedStation(SpriteBatch sb, float centerX, int number, float time)
    {
        int cx = (int)centerX;
        var wall = new Color(32, 34, 44);
        var roof = new Color(20, 20, 26);
        var snow = new Color(170, 180, 200);

        void Box(int x, int y, int w, int h, Color c) => sb.FillRect(new Rectangle(cx + x, y, w, h), c);

        // Duvar
        Box(-130, BaseY - 96, 260, 96, wall);

        // Kademeli çatı (üçgen silüet) ve üzerindeki kar
        for (int i = 0; i < 7; i++)
        {
            int w = 300 - i * 34;
            int y = BaseY - 104 - i * 7;
            Box(-w / 2, y, w, 8, roof);
            Box(-w / 2 + 2, y - 2, w - 4, 2, snow);
        }
        Box(40, BaseY - 124, 34, 12, wall); // Çökmüş çatı kısmı

        // Tahtalarla kapatılmış pencereler
        foreach (int wx in new[] { -100, -15, 70 })
        {
            Box(wx, BaseY - 76, 30, 36, new Color(10, 12, 18));
            Box(wx - 3, BaseY - 66, 36, 4, new Color(58, 44, 34));
            Box(wx - 3, BaseY - 52, 36, 4, new Color(58, 44, 34));
        }

        // İstasyon tabelası
        string label = Loc.F("station.sign", number);
        int signWidth = (int)GameText.Measure(label, 1).X + 16;
        Box(-signWidth / 2, BaseY - 96 + 4, signWidth, 16, new Color(52, 42, 32));
        GameText.DrawCentered(sb, label, cx, BaseY - 96 + 7, new Color(190, 170, 130), 1);

        // Titreyen lamba direği: arada bir tamamen söner
        float flicker = 0.55f + 0.45f * MathF.Sin(time * 13f) * MathF.Sin(time * 7.3f);
        if (MathF.Sin(time * 2.1f) > 0.93f) flicker = 0.05f;
        Box(160, BaseY - 130, 4, 130, new Color(26, 26, 30));
        Box(148, BaseY - 130, 16, 3, new Color(26, 26, 30));
        Box(146, BaseY - 127, 6, 6, new Color(255, 200, 120) * MathHelper.Clamp(flicker + 0.2f, 0f, 1f));
        sb.DrawGlow(new Vector2(cx + 149, BaseY - 124), 70f, new Color(255, 180, 100), 0.35f * flicker);
    }

    /// <summary>Buzul benzinliği: kulübe, paslı pompa, büyük yakıt tankı ve parlayan tabela.</summary>
    public static void DrawDepot(SpriteBatch sb, float centerX, float time)
    {
        int cx = (int)centerX;
        var metal = new Color(46, 50, 60);
        var rust = new Color(120, 60, 36);
        var snow = new Color(175, 185, 205);
        var glow = new Color(255, 170, 70);

        void Box(int x, int y, int w, int h, Color c) => sb.FillRect(new Rectangle(cx + x, y, w, h), c);

        // Kulübe
        Box(-150, BaseY - 84, 120, 84, metal);
        Box(-158, BaseY - 92, 136, 10, new Color(30, 32, 38));
        Box(-156, BaseY - 96, 132, 4, snow);
        Box(-130, BaseY - 64, 30, 24, glow * (0.7f + 0.1f * MathF.Sin(time * 3f)));
        sb.DrawGlow(new Vector2(cx - 115, BaseY - 52), 40f, glow, 0.25f);

        // Pompa ve hortum
        Box(-10, BaseY - 76, 28, 76, rust);
        Box(-6, BaseY - 68, 20, 14, new Color(255, 200, 120) * (0.6f + 0.2f * MathF.Sin(time * 5f)));
        Box(18, BaseY - 60, 14, 4, new Color(30, 30, 34));
        Box(28, BaseY - 60, 4, 40, new Color(30, 30, 34));

        // Büyük yakıt tankı (çemberli)
        Box(50, BaseY - 64, 120, 54, new Color(70, 74, 84));
        Box(50, BaseY - 68, 120, 6, snow);
        for (int x = 64; x < 170; x += 30)
            Box(x, BaseY - 64, 4, 54, new Color(50, 52, 60));
        Box(60, BaseY - 10, 10, 10, new Color(30, 30, 34));
        Box(150, BaseY - 10, 10, 10, new Color(30, 30, 34));

        // Direkte tabela
        string sign = Loc.T("sign.depot");
        int signWidth = (int)GameText.Measure(sign, 2).X + 20;
        Box(-80, BaseY - 150, 4, 150, new Color(30, 30, 34));
        Box(-80 - signWidth / 2, BaseY - 176, signWidth, 28, new Color(30, 22, 18));
        float flicker = MathF.Sin(time * 2.7f) > 0.95f ? 0.3f : 1f;
        GameText.DrawCentered(sb, sign, cx - 80, BaseY - 172, glow * flicker, 2);
        sb.DrawGlow(new Vector2(cx - 80, BaseY - 162), 70f, glow, 0.3f * flicker);
    }

    /// <summary>
    /// Bölüm sonu ana istasyonu: kemerli büyük gar binası, saat kulesi, ışıklı pazar tezgâhları.
    /// Terk edilmiş istasyonların aksine canlı ve aydınlıktır.
    /// </summary>
    public static void DrawHubStation(SpriteBatch sb, float centerX, float time)
    {
        int cx = (int)centerX;
        var stone = new Color(58, 54, 60);
        var roof = new Color(30, 28, 34);
        var snow = new Color(178, 188, 208);
        var warm = new Color(255, 196, 120);

        void Box(int x, int y, int w, int h, Color c) => sb.FillRect(new Rectangle(cx + x, y, w, h), c);

        // Ana bina ve kemerli pencereler
        Box(-240, BaseY - 150, 480, 150, stone);
        for (int i = 0; i < 5; i++)
        {
            int roofWidth = 520 - i * 70;
            Box(-roofWidth / 2, BaseY - 158 - i * 10, roofWidth, 10, roof);
            Box(-roofWidth / 2 + 3, BaseY - 161 - i * 10, roofWidth - 6, 3, snow);
        }
        foreach (int x in new[] { -200, -120, 80, 160 })
        {
            float flicker = 0.75f + 0.1f * MathF.Sin(time * 2f + x);
            Box(x, BaseY - 120, 40, 60, new Color(26, 20, 18));
            Box(x + 3, BaseY - 117, 34, 54, warm * flicker);
            Box(x + 3, BaseY - 124, 34, 6, warm * (flicker * 0.8f)); // Kemer
            sb.DrawGlow(new Vector2(cx + x + 20, BaseY - 90), 46f, warm, 0.25f);
        }

        // Saat kulesi
        Box(-34, BaseY - 260, 68, 110, stone);
        Box(-40, BaseY - 270, 80, 12, roof);
        Box(-38, BaseY - 273, 76, 3, snow);
        sb.Draw(Assets.Circle, new Rectangle(cx - 22, BaseY - 246, 44, 44), new Color(230, 220, 190));
        float hand = time * 0.4f;
        sb.Draw(Assets.Pixel, new Vector2(cx, BaseY - 224), null, new Color(40, 34, 30), hand, new Vector2(0f, 0.5f), new Vector2(16f, 3f), SpriteEffects.None, 0f);
        sb.Draw(Assets.Pixel, new Vector2(cx, BaseY - 224), null, new Color(40, 34, 30), hand * 12f, new Vector2(0f, 0.5f), new Vector2(11f, 2f), SpriteEffects.None, 0f);
        sb.DrawGlow(new Vector2(cx, BaseY - 224), 60f, warm, 0.2f);

        // Büyük kapı
        Box(-40, BaseY - 100, 80, 100, new Color(40, 30, 24));
        Box(-36, BaseY - 96, 72, 96, warm * 0.55f);
        sb.DrawGlow(new Vector2(cx, BaseY - 40), 90f, warm, 0.3f);

        // Pazar tezgâhları (kırmızı-beyaz tenteler)
        foreach (int x in new[] { -330, 270 })
        {
            for (int i = 0; i < 6; i++)
                Box(x + i * 12, BaseY - 62, 12, 10, i % 2 == 0 ? new Color(180, 50, 50) : new Color(220, 220, 220));
            Box(x + 4, BaseY - 52, 4, 52, new Color(60, 46, 34));
            Box(x + 62, BaseY - 52, 4, 52, new Color(60, 46, 34));
            Box(x + 6, BaseY - 26, 60, 10, new Color(90, 66, 44));
            Box(x + 14, BaseY - 34, 12, 8, new Color(240, 200, 90));
            Box(x + 34, BaseY - 34, 14, 8, new Color(255, 150, 70));
            sb.DrawGlow(new Vector2(cx + x + 36, BaseY - 40), 50f, warm, 0.2f);
        }

        // Tabela
        string sign = Loc.T("sign.hub");
        int signWidth = (int)GameText.Measure(sign, 2).X + 28;
        Box(-signWidth / 2, BaseY - 146, signWidth, 26, new Color(30, 24, 20));
        GameText.DrawCentered(sb, sign, cx, BaseY - 142, warm, 2);
    }

    /// <summary>Yaklaşırken görünen kasaba: ışıklı evler ve kasabanın adını taşıyan giriş kemeri.</summary>
    public static void DrawTownGate(SpriteBatch sb, float centerX, string name, Color accent, float time)
    {
        int cx = (int)centerX;
        var snow = new Color(175, 185, 205);

        void Box(int x, int y, int w, int h, Color c) => sb.FillRect(new Rectangle(cx + x, y, w, h), c);

        foreach (var (x, w, h) in new[] { (-230, 110, 90), (-90, 130, 120), (70, 100, 80), (190, 120, 100) })
        {
            Box(x, BaseY - h, w, h, new Color(40, 34, 34));
            for (int i = 0; i < 4; i++)
            {
                int rw = w + 16 - i * (w / 4);
                Box(x + w / 2 - rw / 2, BaseY - h - 8 - i * 6, rw, 7, new Color(26, 22, 24));
                Box(x + w / 2 - rw / 2 + 2, BaseY - h - 10 - i * 6, rw - 4, 2, snow);
            }
            Box(x + 16, BaseY - h + 24, 22, 18, accent * (0.7f + 0.1f * MathF.Sin(time * 3f + x)));
            sb.DrawGlow(new Vector2(cx + x + 27, BaseY - h + 33), 30f, accent, 0.25f);
        }

        // Giriş kemeri ve tabela
        int signWidth = (int)GameText.Measure(name, 2).X + 28;
        Box(-signWidth / 2 - 6, BaseY - 170, 6, 170, new Color(50, 36, 28));
        Box(signWidth / 2, BaseY - 170, 6, 170, new Color(50, 36, 28));
        Box(-signWidth / 2, BaseY - 176, signWidth, 30, new Color(46, 32, 24));
        Box(-signWidth / 2, BaseY - 180, signWidth, 4, snow);
        GameText.DrawCentered(sb, name, cx, BaseY - 170, accent, 2);
    }

    /// <summary>Yol kenarında terk edilmiş, kapısı aralık yük vagonu (malzeme olayı).</summary>
    public static void DrawAbandonedWagon(SpriteBatch sb, float centerX, float time)
    {
        int cx = (int)centerX;
        var rust = new Color(58, 36, 28);
        var dark = new Color(20, 18, 20);

        void Box(int x, int y, int w, int h, Color c) => sb.FillRect(new Rectangle(cx + x, y, w, h), c);

        // Hafifçe yana yatmış izlenimi için basamaklı gövde
        Box(-80, BaseY - 18, 160, 6, dark);                 // Şasi
        Box(-76, BaseY - 66, 152, 48, rust);                // Gövde
        Box(-78, BaseY - 72, 156, 6, dark);                 // Çatı
        Box(-76, BaseY - 76, 150, 4, new Color(175, 185, 205)); // Çatıdaki kar
        for (int x = -60; x < 76; x += 24)
            Box(x, BaseY - 64, 2, 44, new Color(44, 28, 22)); // Tahta çizgileri

        // Aralık kapıdan sızan malzeme ışığı
        Box(-14, BaseY - 60, 28, 40, dark);
        float flicker = 0.7f + 0.3f * MathF.Sin(time * 6f);
        Box(-4, BaseY - 58, 8, 36, new Color(255, 170, 90) * (0.5f * flicker));
        sb.DrawGlow(new Vector2(cx, BaseY - 40), 50f, new Color(255, 170, 90), 0.25f * flicker);

        // Gömülü tekerler
        Box(-60, BaseY - 14, 18, 10, dark);
        Box(42, BaseY - 14, 18, 10, dark);
    }

    /// <summary>Hattın sonu: ışıkları yanan, işaret feneri yanıp sönen Sığınak-0 bunkeri.</summary>
    public static void DrawShelter(SpriteBatch sb, float centerX, float time)
    {
        int cx = (int)centerX;
        var concrete = new Color(42, 46, 54);
        var dark = new Color(26, 28, 34);
        var warm = new Color(255, 190, 110);
        var snow = new Color(175, 185, 205);

        void Box(int x, int y, int w, int h, Color c) => sb.FillRect(new Rectangle(cx + x, y, w, h), c);

        // Basamaklı beton kubbe
        for (int i = 0; i < 5; i++)
        {
            int w = 380 - i * 56;
            int y = BaseY - 128 - i * 14;
            Box(-w / 2, y, w, 16, concrete);
            Box(-w / 2 + 3, y - 2, w - 6, 3, snow);
        }
        Box(-190, BaseY - 128, 380, 128, concrete);

        // Anten ve yanıp sönen kırmızı işaret ışığı
        Box(-2, BaseY - 230, 4, 50, dark);
        bool beaconOn = MathF.Sin(time * 3f) > 0f;
        Box(-4, BaseY - 236, 8, 6, beaconOn ? new Color(255, 60, 50) : new Color(70, 20, 20));
        if (beaconOn) sb.DrawGlow(new Vector2(cx, BaseY - 233), 40f, new Color(255, 60, 50), 0.6f);

        // Aralığından sıcak ışık sızan büyük kapı
        Box(-46, BaseY - 84, 92, 84, dark);
        Box(-2, BaseY - 84, 4, 84, warm * 0.8f);
        sb.DrawGlow(new Vector2(cx, BaseY - 40), 90f, warm, 0.3f);

        // Aydınlık pencereler
        foreach (int wx in new[] { -160, -120, 90, 130 })
        {
            Box(wx, BaseY - 100, 22, 14, warm * (0.75f + 0.1f * MathF.Sin(time * 2f + wx)));
            sb.DrawGlow(new Vector2(cx + wx + 11, BaseY - 93), 30f, warm, 0.2f);
        }

        // Tabela
        Box(-70, BaseY - 120, 140, 20, new Color(20, 22, 28));
        GameText.DrawCentered(sb, Loc.T("shelter.sign"), cx, BaseY - 116, warm, 2);
    }
}
