using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheLastDelivery.Graphics;

namespace TheLastDelivery.UI;

/// <summary>
/// Hikâye sinematiklerinin çizimleri: tamamen kodla, oyunun blok çizim diliyle çizilen sahneler
/// (kar ovası, depo, peron, kabin, çanak, tipi, tünel, ev, göl, kurtlar, telsiz, Sentinel, tüpler, rüya şehri, çekirdek...).
/// Her sahne hafifçe canlıdır: kar yağar, duman tüter, ışıklar titrer.
/// </summary>
public static class CinematicArt
{
    private static readonly Color Night = new(8, 12, 26), NightHigh = new(26, 34, 62);
    private static readonly Color SnowCol = new(214, 222, 236), Ink = new(10, 12, 18);
    private static readonly Color Warm = new(255, 196, 120), Cold = new(140, 200, 255), Red = new(255, 54, 60);

    private static SpriteBatch _sb;
    private static Rectangle _r;
    private static float _t, _a;

    private static void Box(int x, int y, int w, int h, Color c) => _sb.FillRect(new Rectangle(_r.X + x, _r.Y + y, w, h), c * _a);
    private static void Glow(int x, int y, float radius, Color c, float i) => _sb.DrawGlow(new Vector2(_r.X + x, _r.Y + y), radius, c, i * _a);
    private static void Disc(int x, int y, int d, Color c) => _sb.Draw(Assets.Circle, new Rectangle(_r.X + x - d / 2, _r.Y + y - d / 2, d, d), c * _a);

    private static void Sky(Color top, Color bottom, int horizon)
    {
        for (int i = 0; i < 12; i++)
            Box(0, horizon * i / 12, _r.Width, horizon / 12 + 1, Color.Lerp(top, bottom, i / 11f));
    }

    private static void Ground(int y, Color c) => Box(0, y, _r.Width, _r.Height - y, c);

    private static void Mountains(int baseY, Color c, int seed)
    {
        for (int i = 0; i < 9; i++)
        {
            int cx = (i * 173 + seed * 61) % (_r.Width + 200) - 100;
            int h = 60 + (i * 37 + seed * 13) % 90;
            for (int s = 0; s < h; s += 6) Box(cx - (h - s), baseY - s, (h - s) * 2, 6, c);
        }
    }

    private static void Snowfall(float density, float wind)
    {
        int count = (int)(140 * density);
        for (int i = 0; i < count; i++)
        {
            float speed = 30f + (i % 7) * 12f;
            float x = ((i * 97.3f) + _t * wind * speed * 0.6f) % _r.Width;
            float y = ((i * 53.1f) + _t * speed) % _r.Height;
            if (x < 0) x += _r.Width;
            int s = i % 5 == 0 ? 3 : 2;
            Box((int)x, (int)y, s, s, SnowCol * 0.8f);
        }
    }

    private static void Smoke(int x, int y, Color c)
    {
        for (int i = 0; i < 7; i++)
        {
            float k = (_t * 0.4f + i / 7f) % 1f;
            int d = (int)(14 + k * 50);
            Disc(x - (int)(k * 120), y - (int)(k * 90), d, c * (0.45f * (1f - k)));
        }
    }

    /// <summary>Lokomotif ve vagon silueti (ışıklı pencereler, far, duman).</summary>
    private static void Train(int x, int y, float scale, bool lit, Color body)
    {
        int S(float v) => (int)(v * scale);
        Box(x, y - S(60), S(180), S(50), body);                         // Vagon
        Box(x + S(190), y - S(80), S(70), S(70), body);                 // Kabin
        Box(x + S(260), y - S(56), S(130), S(46), body);                // Kazan
        Box(x + S(350), y - S(96), S(20), S(42), body);                 // Baca
        for (int i = 0; i < 4; i++) Disc(x + S(30 + i * 90), y - S(4), S(26), Ink);
        if (lit)
        {
            for (int i = 0; i < 4; i++) Box(x + S(14 + i * 42), y - S(50), S(26), S(18), Warm * 0.85f);
            Box(x + S(206), y - S(68), S(36), S(24), Warm);
            Glow(x + S(392), y - S(36), 140 * scale, Warm, 0.5f);
        }
        Smoke(x + S(360), y - S(100), new Color(170, 176, 190));
    }

    private static void Person(int x, int baseY, int h, Color c)
    {
        Box(x - h / 8, baseY - h, h / 4, h / 5, c);                      // Baş
        Box(x - h / 5, baseY - h * 4 / 5, h * 2 / 5, h * 3 / 5, c);       // Palto
        Box(x - h / 6, baseY - h / 5, h / 7, h / 5, c);
        Box(x + h / 20, baseY - h / 5, h / 7, h / 5, c);
    }

    public static void Draw(SpriteBatch sb, Rectangle r, string art, float time, float alpha)
    {
        _sb = sb; _r = r; _t = time; _a = alpha;
        int w = r.Width, h = r.Height, horizon = h * 2 / 3;
        float flicker = 0.85f + 0.15f * MathF.Sin(time * 7f);

        switch (art)
        {
            case "plain":
                Sky(Night, NightHigh, horizon);
                Disc(w * 3 / 4, h / 5, 60, new Color(210, 216, 230)); Glow(w * 3 / 4, h / 5, 200, Cold, 0.25f);
                Mountains(horizon, new Color(22, 28, 48), 1);
                Ground(horizon, new Color(150, 160, 182));
                Box(0, horizon + 34, w, 4, new Color(70, 74, 90));
                Train((int)(w * 0.18f + MathF.Sin(time * 0.2f) * 10), horizon + 34, 1f, true, Ink);
                Snowfall(0.6f, -0.6f);
                break;

            case "town":
                Sky(new Color(14, 16, 30), new Color(44, 40, 60), horizon);
                Mountains(horizon - 10, new Color(28, 30, 50), 3);
                Ground(horizon, new Color(140, 148, 168));
                for (int i = 0; i < 9; i++)
                {
                    int x = 60 + i * 135, hh = 70 + (i * 31) % 60;
                    Box(x, horizon - hh, 100, hh, new Color(36, 30, 32));
                    Box(x + 10, horizon - hh, 80, 6, SnowCol * 0.8f);
                    Box(x + 30, horizon - hh / 2, 18, 16, Warm * flicker);
                    Glow(x + 39, horizon - hh / 2 + 8, 60, Warm, 0.25f);
                    Smoke(x + 80, horizon - hh - 6, new Color(90, 90, 104));
                }
                Snowfall(0.5f, -0.3f);
                break;

            case "depot":
                Box(0, 0, w, h, new Color(26, 22, 22));
                for (int x = 0; x < w; x += 90) Box(x, 0, 6, h * 3 / 4, new Color(40, 34, 32));
                Box(0, h * 3 / 4, w, h / 4, new Color(36, 32, 30));
                for (int i = 0; i < 4; i++)
                {
                    int lx = 160 + i * 300;
                    Box(lx, 0, 2, 90, Ink); Box(lx - 14, 90, 30, 10, new Color(60, 56, 50));
                    Glow(lx + 1, 110, 180, Warm, 0.35f * flicker);
                }
                Box(w / 2 - 200, h * 3 / 4 - 50, 120, 50, new Color(70, 52, 36)); Box(w / 2 + 160, h * 3 / 4 - 70, 90, 70, new Color(60, 46, 32));
                Person(w / 2 - 60, h * 3 / 4, 150, Ink);
                Person(w / 2 + 60, h * 3 / 4, 170, Ink);
                Box(w / 2 - 30, h * 3 / 4 - 100, 60, 34, new Color(110, 86, 52));     // Paket
                Glow(w / 2, h * 3 / 4 - 83, 70, Cold, 0.4f + 0.2f * MathF.Sin(time * 2f));
                break;

            case "platform":
                Sky(new Color(20, 22, 34), new Color(50, 52, 70), horizon);
                Box(0, horizon - 10, w, 40, new Color(70, 70, 80));
                for (int i = 0; i < 80; i++)
                {
                    int x = (i * 37) % w, hh = 46 + (i * 13) % 26;
                    Person(x, horizon - 10, hh, new Color(18 + i % 20, 18, 26));
                }
                for (int i = 0; i < 6; i++)                                          // Geçen trenin hız çizgileri
                    Box(((int)(time * 1400) + i * 260) % (w + 300) - 300, horizon + 40 + i * 8, 260, 3, Warm * 0.6f);
                Box(0, horizon + 30, w, h - horizon - 30, new Color(16, 16, 22));
                Snowfall(0.4f, -1.5f);
                break;

            case "cab":
                Box(0, 0, w, h, new Color(30, 22, 18));
                Box(w / 2 - 260, 50, 520, 230, new Color(24, 34, 60));                // Ön cam
                for (int i = 0; i < 60; i++) Box(w / 2 - 260 + (int)((i * 83 + time * 60) % 520), 50 + (int)((i * 47 + time * 140) % 230), 2, 2, SnowCol);
                Box(w / 2 - 270, 40, 540, 12, new Color(60, 44, 32)); Box(w / 2 - 270, 278, 540, 12, new Color(60, 44, 32));
                for (int i = 0; i < 4; i++)
                {
                    int gx = 200 + i * 260, gy = 380;
                    Disc(gx, gy, 90, new Color(210, 200, 170)); Disc(gx, gy, 76, new Color(30, 26, 22));
                    float ang = time * (0.3f + i * 0.1f) + i;
                    Box(gx + (int)(MathF.Cos(ang) * 26), gy + (int)(MathF.Sin(ang) * 26), 6, 6, Red);
                    Glow(gx, gy, 80, Warm, 0.12f);
                }
                Box(w / 2 - 40, h - 160, 80, 160, new Color(70, 54, 40));                // Regülatör
                Box(w / 2 - 6, h - 220, 12, 70, new Color(140, 120, 90));
                Glow(w / 2, h - 60, 260, new Color(255, 120, 50), 0.25f * flicker);     // Ocak ışığı
                break;

            case "dish":
                Sky(new Color(6, 10, 24), new Color(20, 30, 54), horizon);
                for (int i = 0; i < 5; i++)                                          // Kuzey ışıkları
                {
                    float wave = MathF.Sin(time * 0.5f + i);
                    Box(0, 40 + i * 22 + (int)(wave * 10), w, 10, new Color(80, 255, 170) * (0.12f - i * 0.015f));
                }
                Ground(horizon, new Color(130, 140, 160));
                Box(w / 2 - 12, horizon - 220, 24, 220, new Color(40, 44, 56));
                Disc(w / 2, horizon - 240, 260, new Color(60, 66, 84)); Disc(w / 2 + 14, horizon - 246, 220, new Color(26, 30, 44));
                Glow(w / 2, horizon - 240, 140, Cold, 0.35f);
                Disc(w / 2, horizon - 240, 14, Red * (0.6f + 0.4f * MathF.Sin(time * 3f)));
                Snowfall(0.3f, -0.2f);
                break;

            case "storm":
                Box(0, 0, w, h, new Color(170, 176, 190));
                Snowfall(2.4f, -3f);
                Glow((int)(w * 0.3f), h / 2, 220, Warm, 0.35f * flicker);
                Box(0, 0, w, h, new Color(200, 206, 220) * 0.25f);
                break;

            case "tunnel":
                Box(0, 0, w, h, new Color(16, 16, 20));
                Disc(w / 2, h / 2 + 40, 520, new Color(6, 6, 8));
                for (int i = 0; i < 6; i++)
                {
                    int ex = w / 2 - 160 + i * 64, ey = h / 2 + 30 + (i % 2) * 20;
                    float blink = MathF.Sin(time * 2f + i * 1.7f) > -0.8f ? 1f : 0f;
                    Box(ex, ey, 6, 4, Red * blink); Box(ex + 12, ey, 6, 4, Red * blink);
                    Glow(ex + 9, ey + 2, 26, Red, 0.25f * blink);
                }
                Box(0, h - 60, w, 60, new Color(30, 28, 30));
                break;

            case "house":
                Sky(new Color(16, 18, 34), new Color(56, 60, 82), horizon);
                Ground(horizon, new Color(176, 184, 200));
                Box(w / 2 - 120, horizon - 150, 240, 150, new Color(54, 40, 34));
                for (int s = 0; s < 80; s += 8) Box(w / 2 - 140 + s * 3 / 2, horizon - 150 - s, 280 - s * 3, 8, new Color(70, 50, 40));
                Box(w / 2 - 140, horizon - 156, 280, 10, SnowCol);
                Box(w / 2 - 30, horizon - 80, 60, 80, new Color(30, 22, 18));
                Box(w / 2 + 60, horizon - 110, 40, 34, new Color(40, 50, 70));          // Karanlık pencere
                int swing = (int)(MathF.Sin(time * 1.2f) * 14);
                Box(w / 2 + 230, horizon - 160, 6, 160, Ink); Box(w / 2 + 330, horizon - 160, 6, 160, Ink); Box(w / 2 + 230, horizon - 164, 106, 6, Ink);
                Box(w / 2 + 270 + swing, horizon - 156, 2, 100, Ink); Box(w / 2 + 300 + swing, horizon - 156, 2, 100, Ink);
                Box(w / 2 + 262 + swing, horizon - 58, 46, 6, new Color(80, 60, 40));
                Snowfall(0.5f, -0.4f);
                break;

            case "lake":
                Sky(new Color(60, 70, 110), new Color(255, 190, 140), horizon);
                Glow(w / 2, horizon, 300, Warm, 0.4f);
                Box(0, horizon, w, h - horizon, new Color(150, 190, 220));
                for (int i = 0; i < 12; i++) Box(i * 110, horizon + 30 + (i % 3) * 20, 80, 2, Color.White * 0.5f);
                Box(0, horizon - 40, w, 10, new Color(60, 60, 70));                     // Köprü
                for (int i = 0; i < 14; i++) Box(i * 96, horizon - 30, 8, 30, new Color(60, 60, 70));
                Train(w / 2 - 300, horizon - 40, 0.9f, false, new Color(220, 230, 240));
                for (int i = 0; i < 4; i++) Box(w / 2 - 285 + i * 38, horizon - 85, 22, 16, Warm * 0.7f);
                break;

            case "wolves":
                Sky(Night, new Color(40, 40, 64), horizon);
                Disc(w / 2, h / 3, 160, new Color(230, 230, 240)); Glow(w / 2, h / 3, 260, Cold, 0.3f);
                for (int s = 0; s < 120; s += 6) Box(w / 2 - 400 + s, horizon - s / 2, w, 6, new Color(20, 22, 36));
                for (int i = 0; i < 7; i++)
                {
                    int x = 180 + i * 140, y = horizon - 30 - (i % 3) * 14;
                    Box(x, y, 48, 18, Ink); Box(x + 40, y - 12, 18, 16, Ink); Box(x + 52, y - 18, 4, 8, Ink);
                    Box(x + 4, y + 18, 6, 14, Ink); Box(x + 36, y + 18, 6, 14, Ink);
                    Box(x + 52, y - 8, 3, 2, Warm * flicker);
                }
                Snowfall(0.3f, -0.4f);
                break;

            case "radio":
                Box(0, 0, w, h, new Color(24, 18, 14));
                Box(w / 2 - 340, h / 2 - 150, 680, 300, new Color(70, 46, 30));
                Box(w / 2 - 300, h / 2 - 110, 600, 110, new Color(50, 36, 18));
                Box(w / 2 - 296, h / 2 - 106, 592, 102, Warm * 0.15f);
                for (int i = 0; i <= 30; i++) Box(w / 2 - 280 + i * 19, h / 2 - 40 - (i % 5 == 0 ? 20 : 8), 2, i % 5 == 0 ? 20 : 8, Warm * 0.6f);
                int needle = w / 2 - 280 + (int)((MathF.Sin(time * 0.4f) * 0.5f + 0.5f) * 560);
                Box(needle, h / 2 - 104, 3, 96, Red);
                for (int i = 0; i < 200; i++)
                {
                    float v = MathF.Sin(i * 0.2f + time * 4f) * MathF.Sin(i * 0.05f + time) * 30f;
                    Box(w / 2 - 300 + i * 3, h / 2 + 60 + (int)v, 3, 3, Cold * 0.8f);
                }
                Disc(w / 2 - 200, h / 2 + 110, 50, new Color(140, 110, 80)); Disc(w / 2 + 200, h / 2 + 110, 50, new Color(140, 110, 80));
                break;

            case "redeye":
                Sky(new Color(10, 8, 14), new Color(40, 20, 26), horizon);
                Ground(horizon, new Color(80, 80, 96));
                Box(0, horizon + 30, w, 4, new Color(50, 50, 60));
                int sx = (int)(w * 0.25f + (time * 30f) % 200);
                Box(sx, horizon - 70, 520, 100, new Color(18, 18, 22));
                for (int i = 0; i < 5; i++) Disc(sx + 40 + i * 110, horizon + 28, 34, Ink);
                Disc(sx + 520, horizon - 20, 40, Red * (0.7f + 0.3f * MathF.Sin(time * 4f)));
                Glow(sx + 520, horizon - 20, 220, Red, 0.45f);
                Box(sx + 520, horizon - 22, w, 3, Red * 0.25f);                       // Tarama ışını
                Snowfall(0.5f, -0.8f);
                break;

            case "tubes":
                Box(0, 0, w, h, new Color(10, 16, 26));
                for (int row = 0; row < 2; row++)
                    for (int i = 0; i < 9; i++)
                    {
                        int x = 50 + i * 136 + row * 40, y = 60 + row * 230, s = row == 0 ? 180 : 210;
                        Box(x, y, 70, s, new Color(120, 200, 240) * 0.25f);
                        Box(x + 6, y + 10, 4, s - 20, Color.White * 0.35f);
                        Box(x + 24, y + 40, 22, 60, new Color(30, 50, 76));
                        Glow(x + 35, y + s / 2, 80, Cold, 0.18f + 0.06f * MathF.Sin(time + i));
                    }
                break;

            case "city":
                Sky(new Color(150, 200, 240), new Color(240, 230, 210), horizon);
                for (int i = 0; i < 12; i++)
                {
                    int x = 20 + i * 104, hh = 140 + (i * 53) % 220;
                    Box(x, horizon - hh, 80, hh, new Color(200, 225, 240));
                    for (int k = 0; k < hh; k += 24) Box(x + 8, horizon - hh + k + 6, 64, 10, new Color(160, 200, 230));
                    Box(x, horizon - hh - 10, 80, 10, new Color(120, 190, 120));      // Çatı bahçeleri
                }
                Ground(horizon, new Color(220, 228, 232));
                for (int i = 0; i < 14; i++) Person(60 + i * 86, horizon + 60, 60, new Color(120, 130, 150) * 0.9f);
                Box(0, 0, w, h, Color.White * (0.12f + 0.05f * MathF.Sin(time * 0.7f)));
                break;

            case "core":
                Box(0, 0, w, h, new Color(6, 10, 20));
                for (int i = 0; i < 16; i++) Box(i * 84, 0, 60, h, new Color(20, 34, 54) * 0.6f);
                Box(w / 2 - 50, 0, 100, h, new Color(120, 200, 255) * 0.35f);
                Glow(w / 2, h / 2, 320, Cold, 0.5f + 0.15f * MathF.Sin(time * 1.5f));
                Disc(w / 2, h / 2, 120, new Color(200, 240, 255) * 0.8f);
                Disc(w / 2, h / 2, 30, Red);
                break;

            case "fire":
                Box(0, 0, w, h, new Color(8, 8, 14));
                Ground(h * 3 / 4, new Color(30, 28, 34));
                Glow(w / 2, h * 3 / 4 - 20, 340, new Color(255, 140, 60), 0.45f * flicker);
                for (int i = 0; i < 6; i++)
                {
                    int fx = w / 2 - 20 + (i * 9) % 40, fh = 30 + (int)(MathF.Sin(time * 8f + i) * 12) + i * 4;
                    Box(fx, h * 3 / 4 - fh, 8, fh, (i % 2 == 0 ? new Color(255, 160, 60) : new Color(255, 220, 120)) * flicker);
                }
                for (int i = 0; i < 6; i++)
                {
                    float ang = i / 6f * MathF.Tau;
                    Person(w / 2 + (int)(MathF.Cos(ang) * 220), h * 3 / 4 + (int)(MathF.Sin(ang) * 20), 90, new Color(30, 22, 18));
                }
                break;

            case "shelter":
                Sky(new Color(14, 16, 26), new Color(50, 54, 70), horizon);
                Ground(horizon, new Color(150, 156, 172));
                Box(0, horizon - 260, w, 260, new Color(60, 64, 76));
                for (int x = 0; x < w; x += 120) Box(x, horizon - 260, 8, 260, new Color(40, 44, 54));
                Box(w / 2 - 140, horizon - 200, 280, 200, new Color(30, 32, 40));
                Box(w / 2 - 4, horizon - 200, 8, 200, new Color(80, 84, 96));
                for (int i = 0; i < 5; i++) { Box(100 + i * 260, horizon - 240, 20, 10, Warm * (0.4f * flicker)); }
                Glow(w / 2, horizon - 100, 220, Warm, 0.12f);
                Snowfall(0.6f, -0.5f);
                break;

            case "ruins":
                Sky(new Color(18, 16, 22), new Color(52, 40, 46), horizon);
                Ground(horizon, new Color(130, 132, 146));
                for (int i = 0; i < 6; i++)
                {
                    int x = 80 + i * 200, hh = 120 + (i * 41) % 100;
                    Box(x, horizon - hh, 140, hh, new Color(46, 44, 50));
                    Box(x + 20, horizon - hh + 20, 30, 40, Ink); Box(x + 80, horizon - hh + 50, 30, 40, Ink);
                    Box(x + 100, horizon - hh - 10, 40, 10, new Color(46, 44, 50));
                }
                Box(w - 300, horizon - 260, 6, 260, Ink); Box(w - 330, horizon - 260, 66, 6, Ink);  // Devrik anten
                Snowfall(0.4f, -0.6f);
                break;

            case "mine":
                Sky(new Color(20, 18, 26), new Color(60, 46, 50), horizon);
                Mountains(horizon, new Color(50, 42, 46), 5);
                Ground(horizon, new Color(90, 84, 90));
                Box(w / 2 - 140, horizon - 230, 20, 230, Ink); Box(w / 2 + 120, horizon - 230, 20, 230, Ink);
                Box(w / 2 - 140, horizon - 240, 280, 16, Ink); Disc(w / 2, horizon - 236, 60, Ink);
                Disc(w / 2, horizon - 40, 140, new Color(6, 6, 8));
                Glow(w / 2, horizon - 40, 140, Cold, 0.3f + 0.1f * MathF.Sin(time * 1.3f));
                break;

            case "tower":
                Sky(new Color(10, 8, 16), new Color(46, 26, 36), horizon);
                Ground(horizon, new Color(120, 120, 136));
                for (int s = 0; s < 360; s += 12)
                {
                    int half = 60 - s / 7;
                    Box(w / 2 - half, horizon - s, 4, 12, Ink); Box(w / 2 + half, horizon - s, 4, 12, Ink);
                    if (s % 36 == 0) Box(w / 2 - half, horizon - s, half * 2, 3, Ink);
                }
                Disc(w / 2 + 2, horizon - 380, 40, Red * (0.6f + 0.4f * MathF.Sin(time * 3f)));
                Glow(w / 2 + 2, horizon - 380, 260, Red, 0.35f);
                Snowfall(0.4f, -0.5f);
                break;

            case "station":
                Sky(new Color(22, 24, 36), new Color(60, 62, 80), horizon);
                Ground(horizon, new Color(150, 156, 170));
                Box(120, horizon - 200, w - 240, 200, new Color(50, 46, 46));
                Box(100, horizon - 210, w - 200, 14, SnowCol);
                Disc(w / 2, horizon - 140, 90, new Color(220, 214, 196)); Disc(w / 2, horizon - 140, 78, new Color(240, 236, 220));
                Box(w / 2 - 2, horizon - 176, 4, 38, Ink); Box(w / 2, horizon - 142, 26, 4, Ink);   // Donmuş saat
                for (int i = 0; i < 5; i++) Box(170 + i * 200, horizon - 90, 60, 70, Ink);
                Snowfall(0.4f, -0.3f);
                break;

            case "map":
                Box(0, 0, w, h, new Color(200, 186, 150));
                for (int i = 0; i < 40; i++) Box((i * 97) % w, (i * 53) % h, 8 + i % 9, 5, new Color(170, 150, 110) * 0.4f);
                int prev = 0;
                for (int i = 0; i < 20; i++)
                {
                    int x = 80 + i * 58, y = h / 2 + (int)(MathF.Sin(i * 0.7f) * 120);
                    if (i > 0) Box(x - 58, Math.Min(prev, y), 58, Math.Abs(prev - y) + 4, new Color(90, 70, 50) * 0.5f);
                    Disc(x, y, 12, new Color(90, 60, 40)); prev = y;
                }
                Box(80, h / 2 - 2, (int)((w - 160) * ((MathF.Sin(time * 0.3f) * 0.5f) + 0.5f)), 4, Red * 0.8f);
                Box(0, 0, w, h, new Color(60, 40, 20) * 0.1f);
                break;

            default: // "black": yalnızca karanlık ve kar
                Box(0, 0, w, h, Color.Black);
                Snowfall(0.3f, -0.2f);
                break;
        }
    }
}
