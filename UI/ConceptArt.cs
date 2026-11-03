using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheLastDelivery.Graphics;

namespace TheLastDelivery.UI;

/// <summary>
/// Supporter Pack "Bonuslar" galerisinin konsept çizimleri: eski, sararmış kâğıt üzerinde
/// kurşun kalem çizgileri, gölge taramaları ve kenar notları. Tamamen kodla çizilir.
///   0 Old Smokie lokomotif taslağı   1 Kırağı Köyü sokağı   2 Kızıl Kurtlar'ın zırhlı treni   3 Sığınak-0 kapısı
/// </summary>
public static class ConceptArt
{
    private static readonly Color Paper = new(222, 206, 170);
    private static readonly Color PaperDark = new(196, 176, 136);
    private static readonly Color Ink = new(70, 56, 44);
    private static readonly Color Pencil = new(120, 104, 86);

    public static void Draw(SpriteBatch sb, Rectangle r, int index, float alpha, float time)
    {
        // Sararmış kâğıt: kenarları koyu, lekeli
        sb.FillRect(r, Paper * alpha);
        for (int i = 0; i < 40; i++)
        {
            int x = r.X + (i * 97) % r.Width, y = r.Y + (i * 53) % r.Height;
            sb.FillRect(new Rectangle(x, y, 6 + i % 9, 4 + i % 5), PaperDark * (0.35f * alpha));
        }
        sb.Draw(Assets.Vignette, r, new Color(110, 80, 50) * (0.35f * alpha));

        int cx = r.Center.X, baseY = r.Bottom - 70;
        void Line(int x, int y, int w, int h) => sb.FillRect(new Rectangle(x, y, Math.Max(1, w), Math.Max(1, h)), Ink * alpha);
        void Hatch(int x, int y, int w, int h)
        {
            for (int i = 0; i < w + h; i += 6) // Çapraz tarama ile gölge
            {
                int x0 = x + Math.Min(i, w), y0 = y + Math.Max(0, i - w);
                int len = Math.Min(Math.Min(i, w), h - Math.Max(0, i - w));
                for (int k = 0; k < len; k += 2) sb.FillRect(new Rectangle(x0 - k, y0 + k, 2, 2), Pencil * (0.6f * alpha));
            }
        }
        void Frame(int x, int y, int w, int h) { Line(x, y, w, 2); Line(x, y + h - 2, w, 2); Line(x, y, 2, h); Line(x + w - 2, y, 2, h); }
        void Note(string text, int x, int y) => GameText.Draw(sb, text, new Vector2(x, y), Ink * (0.8f * alpha));

        // Zemin çizgisi ve ölçü işaretleri
        Line(r.X + 30, baseY, r.Width - 60, 2);
        for (int x = r.X + 40; x < r.Right - 40; x += 40) Line(x, baseY + 4, 1, 6);

        switch (index)
        {
            case 0: // Old Smokie lokomotif taslağı
                Frame(cx - 230, baseY - 110, 150, 90);           // Vagon
                Frame(cx - 70, baseY - 140, 80, 120);            // Makinist kabini
                Frame(cx + 10, baseY - 100, 170, 70);            // Kazan
                Frame(cx + 110, baseY - 170, 26, 70);            // Baca
                Frame(cx + 102, baseY - 182, 42, 14);
                for (int i = 0; i < 3; i++) Frame(cx + 30 + i * 50, baseY - 30, 40, 40);  // Tekerler
                Frame(cx - 200, baseY - 26, 30, 30); Frame(cx - 130, baseY - 26, 30, 30);
                Hatch(cx + 12, baseY - 98, 166, 30);
                for (int i = 0; i < 5; i++)                       // Duman spirali
                {
                    int s = 14 + i * 8;
                    Frame(cx + 100 - i * 30, baseY - 210 - i * 22, s, s);
                }
                Note("OLD SMOKIE - REV. 3", r.X + 24, r.Y + 20);
                Note("KAZAN: 4.2 M", cx + 20, baseY - 126);
                break;

            case 1: // Kırağı Köyü sokağı
                for (int i = 0; i < 4; i++)
                {
                    int x = r.X + 50 + i * 150, h = 120 + (i % 2) * 40;
                    Frame(x, baseY - h, 120, h);
                    for (int k = 0; k < 4; k++) Line(x + k * 12, baseY - h - 30 + k * 7, 60 - k * 6, 2); // Çatı
                    Frame(x + 20, baseY - h + 30, 28, 24);
                    Frame(x + 70, baseY - 50, 26, 50);
                    Hatch(x + 4, baseY - h + 4, 30, h - 8);
                }
                Frame(r.Right - 140, baseY - 150, 8, 150);       // Fener direği
                sb.DrawGlow(new Vector2(r.Right - 136, baseY - 156), 30f, new Color(160, 120, 60), 0.4f * alpha);
                Note("KIRAĞI KÖYÜ - SOKAK ÇALIŞMASI", r.X + 24, r.Y + 20);
                break;

            case 2: // Kızıl Kurtlar'ın zırhlı treni
                Frame(cx - 260, baseY - 120, 360, 100);
                for (int x = cx - 250; x < cx + 90; x += 30) Line(x, baseY - 116, 2, 92);   // Zırh plakaları
                Frame(cx + 100, baseY - 90, 110, 70);             // Koçbaşı
                for (int i = 0; i < 4; i++) Line(cx + 210 + i * 6, baseY - 80 + i * 14, 18 - i * 3, 3);
                Frame(cx - 160, baseY - 170, 90, 50);             // Taret
                Line(cx - 70, baseY - 152, 120, 6);               // Namlu
                Hatch(cx - 256, baseY - 116, 350, 40);
                Line(cx - 200, baseY - 160, 20, 20); Line(cx - 190, baseY - 170, 3, 40); // Kurt bayrağı
                Note("KIZIL KURTLAR - ZIRHLI TREN", r.X + 24, r.Y + 20);
                break;

            default: // Sığınak-0 kapısı
                Frame(cx - 220, baseY - 230, 440, 230);           // Kaya cephe
                Frame(cx - 110, baseY - 170, 220, 170);           // Kapı
                for (int i = 0; i < 6; i++) Line(cx - 108 + i * 36, baseY - 168, 2, 166);
                Frame(cx - 30, baseY - 220, 60, 30);              // "0" levhası
                Note("0", cx - 6, baseY - 214);
                for (int i = 0; i < 3; i++)                       // Kapı üstü lambalar
                {
                    float flicker = 0.4f + 0.2f * MathF.Sin(time * 3f + i);
                    sb.DrawGlow(new Vector2(cx - 80 + i * 80, baseY - 180), 26f, new Color(180, 140, 70), flicker * alpha);
                }
                Hatch(cx - 216, baseY - 226, 100, 220);
                Hatch(cx + 116, baseY - 226, 100, 220);
                Note("SIĞINAK-0 - GİRİŞ", r.X + 24, r.Y + 20);
                break;
        }

        // Köşede imza
        Note("T.L.D. 2026", r.Right - 150, r.Bottom - 30);
    }
}
