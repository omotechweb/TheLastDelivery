using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheLastDelivery.Core;
using TheLastDelivery.Graphics;

namespace TheLastDelivery.Towns;

/// <summary>
/// İç mekânların çizimi (taverna, karakol, atölye, sığınak, telsiz odası) ve çevresel hikâye yapıları
/// (buza kesmiş insanlar, terk edilmiş vagonlar). Hepsi kasabanın blok çizim diliyle çizilir.
/// </summary>
public sealed partial class TownScene
{
    private const int WallTop = 118;

    private static (Color Wall, Color Trim, Color Floor, Color Light) Palette(InteriorKind kind) => kind switch
    {
        InteriorKind.Tavern => (new Color(70, 46, 32), new Color(48, 31, 22), new Color(58, 40, 28), new Color(255, 190, 110)),
        InteriorKind.Garrison => (new Color(56, 62, 56), new Color(40, 44, 40), new Color(46, 48, 52), new Color(230, 230, 200)),
        InteriorKind.Workshop => (new Color(80, 52, 42), new Color(52, 36, 30), new Color(50, 46, 42), new Color(255, 200, 120)),
        InteriorKind.Shelter => (new Color(52, 56, 64), new Color(38, 40, 48), new Color(40, 42, 48), new Color(200, 215, 240)),
        _ => (new Color(34, 40, 56), new Color(24, 28, 40), new Color(32, 34, 42), new Color(140, 210, 255))
    };

    /// <summary>İç mekân: duvar, pencereler, çıkış kapısı, asılı lambalar, eşyalar, insanlar ve Arthur.</summary>
    private void DrawInteriorWorld(SpriteBatch sb)
    {
        var kind = Interior ?? InteriorKind.Tavern;
        var (wall, trim, floor, light) = Palette(kind);
        int width = _layout.Width;

        // Tavan, duvar, lambri, zemin
        sb.FillRect(new Rectangle(-40, 0, width + 80, WallTop), new Color(14, 12, 14));
        sb.FillRect(new Rectangle(-40, WallTop, width + 80, BaseY - WallTop), wall);
        for (int x = 0; x < width; x += 64) sb.FillRect(new Rectangle(x, WallTop, 2, BaseY - WallTop), trim * 0.6f); // Tahta ya da panel çizgileri
        sb.FillRect(new Rectangle(-40, BaseY - 70, width + 80, 70), trim);
        sb.FillRect(new Rectangle(-40, BaseY - 72, width + 80, 3), Color.Lerp(trim, Color.White, 0.15f));
        sb.FillRect(new Rectangle(-40, BaseY, width + 80, GameConfig.ScreenHeight - BaseY + 10), floor);
        for (int x = -40; x < width + 40; x += 90) sb.FillRect(new Rectangle(x, BaseY, 2, 160), Color.Black * 0.25f);
        sb.FillRect(new Rectangle(-40, BaseY, width + 80, 4), Color.Black * 0.35f);
        for (int x = 0; x < width; x += 260) sb.FillRect(new Rectangle(x, WallTop - 6, 30, 22), new Color(30, 22, 18)); // Kirişler

        // Buzlu pencereler: dışarıda kar yağıyor
        for (int x = 560; x < width - 120; x += 520)
        {
            var glass = new Rectangle(x, 200, 120, 130);
            sb.FillRect(new Rectangle(glass.X - 8, glass.Y - 8, glass.Width + 16, glass.Height + 16), trim);
            sb.FillRect(glass, new Color(30, 44, 70));
            sb.FillRect(new Rectangle(glass.X, glass.Bottom - 24, glass.Width, 24), new Color(200, 210, 230) * 0.5f); // Pervazdaki kar
            for (int i = 0; i < 9; i++)
            {
                float fy = (_time * (30f + i * 7f) + i * 37f) % glass.Height;
                float fx = (i * 29 + MathF.Sin(_time + i) * 6f) % glass.Width;
                sb.FillRect(new Rectangle(glass.X + (int)fx, glass.Y + (int)fy, 2, 2), Color.White * 0.7f);
            }
            sb.FillRect(new Rectangle(glass.Center.X - 2, glass.Y, 4, glass.Height), trim);
            sb.FillRect(new Rectangle(glass.X, glass.Center.Y - 2, glass.Width, 4), trim);
            sb.DrawGlow(new Vector2(glass.Center.X, glass.Center.Y), 110f, new Color(120, 150, 220), 0.12f);
        }

        // Çıkış kapısı: aralığından soğuk mavi ışık sızar
        float doorX = InteriorCatalog.ExitX;
        sb.FillRect(new Rectangle((int)doorX - 44, BaseY - 176, 88, 176), trim);
        sb.FillRect(new Rectangle((int)doorX - 36, BaseY - 168, 72, 168), new Color(28, 22, 18));
        sb.FillRect(new Rectangle((int)doorX + 20, BaseY - 168, 16, 168), new Color(120, 160, 220) * 0.35f);
        sb.FillRect(new Rectangle((int)doorX + 10, BaseY - 90, 6, 6), new Color(200, 170, 90));
        sb.DrawGlow(new Vector2(doorX + 28, BaseY - 60), 90f, new Color(120, 170, 255), 0.15f);

        // Eşyalar
        foreach (var p in _layout.Structures) DrawStructure(sb, p);
        HighlightNearest(sb);

        // Asılı lambalar
        foreach (float x in _layout.Lamps)
        {
            float flicker = 0.85f + 0.15f * MathF.Sin(_time * 6f + x);
            if (kind == InteriorKind.Shelter) flicker *= MathF.Sin(_time * 11f + x) > -0.92f ? 1f : 0.2f; // Sığınağın lambası titrer
            sb.FillRect(new Rectangle((int)x, WallTop, 2, 60), new Color(20, 18, 18));
            sb.FillRect(new Rectangle((int)x - 9, WallTop + 60, 20, 8), new Color(40, 36, 32));
            sb.FillRect(new Rectangle((int)x - 4, WallTop + 68, 10, 8), light * flicker);
            sb.DrawGlow(new Vector2(x + 1, WallTop + 72), 150f, light, 0.18f * flicker);
            sb.DrawGlow(new Vector2(x + 1, BaseY + 6), 120f, light, 0.07f * flicker);
        }

        _particles.Draw(sb);
        for (int i = 0; i < _layout.People.Length; i++)
        {
            var person = _layout.People[i];
            DrawPerson(sb, person.X, person.Facing, person.Coat, person.Scarf, MathF.Sin(_time * 2f + i) * 1.5f, 0f, false);
        }
        DrawArthur(sb);
    }

    /// <summary>Derin anlatı yapıları (dış mekânda da kullanılanlar dahil).</summary>
    private void DrawDeepStructure(SpriteBatch sb, Placement p)
    {
        float cx = p.X;
        int v = p.Variant;
        int y = BaseY;
        switch (p.Kind)
        {
            case StructureKind.FrozenFigure: DrawFrozen(sb, cx, v); break;
            case StructureKind.WagonWreck: DrawWagonWreck(sb, cx, v); break;

            case StructureKind.BarCounter:
                // Arka raf ve şişeler
                Box(sb, cx, -110, y - 230, 220, 8, new Color(60, 40, 28));
                for (int i = 0; i < 9; i++)
                    Box(sb, cx, -100 + i * 24, y - 254 + (i % 3) * 2, 8, 22 - (i % 3) * 2,
                        i % 3 == 0 ? new Color(90, 140, 90) : i % 3 == 1 ? new Color(150, 90, 50) : new Color(200, 200, 210) * 0.7f);
                Box(sb, cx, -110, y - 64, 220, 64, new Color(84, 54, 34));
                Box(sb, cx, -116, y - 70, 232, 8, new Color(120, 82, 52));
                Box(sb, cx, -40, y - 82, 10, 12, new Color(200, 180, 120)); // Kupa
                Box(sb, cx, 30, y - 80, 8, 10, new Color(170, 170, 180));
                break;

            case StructureKind.Fireplace:
            {
                Box(sb, cx, -70, y - 130, 140, 130, new Color(90, 86, 82));
                Box(sb, cx, -78, y - 138, 156, 12, new Color(110, 104, 98));
                Box(sb, cx, -44, y - 80, 88, 80, new Color(20, 14, 12));
                float f = 0.7f + 0.3f * MathF.Sin(_time * 9f) * MathF.Sin(_time * 3.7f);
                Box(sb, cx, -30, y - 40, 60, 30, new Color(255, 140, 50) * f);
                Box(sb, cx, -18, y - 58, 36, 22, new Color(255, 200, 90) * f);
                sb.DrawGlow(new Vector2(cx, y - 40), 160f, new Color(255, 150, 70), 0.3f * f);
                if (v == 1) Box(sb, cx, -60, y - 170, 120, 26, new Color(160, 170, 190)); // Keşişin buz kristali rafı
                break;
            }

            case StructureKind.Shelf:
                Box(sb, cx, -60, y - 220, 120, 220, new Color(66, 44, 30));
                for (int r = 0; r < 4; r++)
                {
                    Box(sb, cx, -56, y - 210 + r * 52, 112, 4, new Color(90, 62, 40));
                    for (int b = 0; b < 8; b++)
                        Box(sb, cx, -52 + b * 13, y - 204 + r * 52 - (b % 2) * 3 + 6, 10, 38 + (b % 2) * 3 - 6,
                            v == 1 ? new Color(200, 210, 230) : new Color(110 + b * 12, 60 + r * 20, 50));
                }
                break;

            case StructureKind.Desk:
                Box(sb, cx, -70, y - 54, 140, 10, new Color(90, 64, 44));
                Box(sb, cx, -64, y - 44, 10, 44, new Color(70, 50, 34));
                Box(sb, cx, 54, y - 44, 10, 44, new Color(70, 50, 34));
                Box(sb, cx, -40, y - 60, 36, 6, new Color(230, 226, 210)); // Kâğıtlar
                Box(sb, cx, -30, y - 64, 30, 4, new Color(210, 206, 190));
                if (v == 1) Box(sb, cx, 6, y - 62, 28, 8, new Color(170, 40, 40)); // Kırmızı "GİZLİ" dosya
                if (v == 2)
                {
                    Box(sb, cx, 10, y - 96, 46, 36, new Color(40, 44, 50)); // Terminal
                    Box(sb, cx, 14, y - 92, 38, 26, new Color(80, 220, 160) * (0.6f + 0.2f * MathF.Sin(_time * 3f)));
                }
                else
                {
                    Box(sb, cx, 40, y - 90, 3, 30, new Color(40, 40, 40)); // Masa lambası
                    Box(sb, cx, 32, y - 96, 20, 8, new Color(60, 90, 60));
                    sb.DrawGlow(new Vector2(cx + 42, y - 70), 50f, new Color(255, 220, 140), 0.25f);
                }
                break;

            case StructureKind.Lockers:
                for (int i = 0; i < 3; i++)
                {
                    Box(sb, cx, -66 + i * 44, y - 140, 40, 140, v == 1 ? new Color(70, 80, 70) : new Color(84, 90, 100));
                    Box(sb, cx, -60 + i * 44, y - 126, 28, 3, Color.Black * 0.4f);
                    Box(sb, cx, -60 + i * 44, y - 118, 28, 3, Color.Black * 0.4f);
                    Box(sb, cx, -34 + i * 44, y - 80, 4, 10, new Color(200, 190, 140));
                }
                break;

            case StructureKind.Workbench:
                Box(sb, cx, -110, y - 200, 220, 110, new Color(60, 52, 44)); // Alet panosu
                for (int i = 0; i < 6; i++) Box(sb, cx, -96 + i * 34, y - 186 + (i % 2) * 10, 6, 50 - (i % 2) * 16, new Color(150, 150, 160));
                Box(sb, cx, -110, y - 60, 220, 12, new Color(100, 76, 52));
                Box(sb, cx, -104, y - 48, 12, 48, new Color(70, 54, 40));
                Box(sb, cx, 92, y - 48, 12, 48, new Color(70, 54, 40));
                Box(sb, cx, -60, y - 82, 30, 22, new Color(90, 96, 110)); // Mengene
                if (v == 1) Box(sb, cx, 10, y - 96, 70, 36, new Color(70, 74, 84)); // Söküldü: motor bloğu
                break;

            case StructureKind.Bunks:
                Box(sb, cx, -76, y - 130, 8, 130, new Color(80, 84, 92));
                Box(sb, cx, 68, y - 130, 8, 130, new Color(80, 84, 92));
                Box(sb, cx, -70, y - 120, 140, 12, v == 2 ? new Color(160, 180, 200) : new Color(110, 100, 86));
                Box(sb, cx, -70, y - 46, 140, 12, v == 2 ? new Color(160, 180, 200) : new Color(110, 100, 86));
                Box(sb, cx, -66, y - 132, 34, 12, new Color(200, 196, 186)); // Yastıklar
                Box(sb, cx, -66, y - 58, 34, 12, new Color(200, 196, 186));
                if (v == 1) Box(sb, cx, 20, y - 70, 40, 24, new Color(140, 160, 120)); // İlaç sandığı
                break;

            case StructureKind.RadioConsole:
            {
                Box(sb, cx, -80, y - 110, 160, 110, new Color(50, 54, 62));
                Box(sb, cx, -80, y - 116, 160, 8, new Color(70, 74, 84));
                var screen = v == 1 ? new Color(255, 60, 60) : new Color(110, 230, 160);
                float pulse = 0.55f + 0.3f * MathF.Sin(_time * (v == 1 ? 5f : 2f));
                Box(sb, cx, -64, y - 100, 60, 40, Color.Black);
                for (int i = 0; i < 6; i++)
                    Box(sb, cx, -60 + i * 9, y - 80 + (int)(MathF.Sin(_time * 4f + i) * 8f), 6, 3, screen * pulse);
                for (int i = 0; i < 4; i++) sb.Draw(Graphics.Assets.Circle, new Rectangle((int)cx + 6 + i * 18, y - 96, 12, 12), new Color(180, 170, 140));
                Box(sb, cx, 4, y - 62, 66, 6, new Color(30, 30, 34));
                Box(sb, cx, 60, y - 200, 3, 90, new Color(60, 60, 66)); // Anten kablosu
                sb.DrawGlow(new Vector2(cx - 34, y - 80), 70f, screen, 0.2f * pulse);
                break;
            }

            case StructureKind.MapBoard:
                Box(sb, cx, -80, y - 250, 160, 110, new Color(160, 140, 100));
                Box(sb, cx, -76, y - 246, 152, 102, v == 2 ? new Color(60, 70, 60) : new Color(210, 200, 170));
                Box(sb, cx, -60, y - 220, 50, 3, new Color(80, 90, 120));
                Box(sb, cx, -20, y - 200, 70, 3, new Color(80, 90, 120));
                Box(sb, cx, -40, y - 230, 3, 60, new Color(200, 40, 40)); // Kırmızı ip
                Box(sb, cx, -40, y - 200, 50, 3, new Color(200, 40, 40));
                Box(sb, cx, 8, y - 204, 6, 6, new Color(220, 50, 50));
                if (v == 2) // Fotoğraf: üç asker
                    for (int i = 0; i < 3; i++) Box(sb, cx, -50 + i * 34, y - 236, 22, 34, new Color(170, 170, 160));
                break;

            case StructureKind.Table:
                Box(sb, cx, -60, y - 50, 120, 8, new Color(100, 70, 46));
                Box(sb, cx, -6, y - 42, 12, 42, new Color(70, 50, 34));
                Box(sb, cx, -96, y - 40, 24, 6, new Color(80, 56, 38)); // Sandalyeler
                Box(sb, cx, 72, y - 40, 24, 6, new Color(80, 56, 38));
                Box(sb, cx, -30, y - 62, 10, 12, new Color(200, 180, 120));
                if (v == 2) Box(sb, cx, 10, y - 56, 30, 6, new Color(230, 226, 210)); // Mektuplar
                break;

            case StructureKind.Boiler:
            {
                Box(sb, cx, -60, y - 170, 120, 170, new Color(90, 70, 60));
                Box(sb, cx, -66, y - 176, 132, 10, new Color(120, 96, 80));
                sb.Draw(Graphics.Assets.Circle, new Rectangle((int)cx - 20, y - 150, 40, 40), new Color(220, 210, 180));
                Box(sb, cx, -1, y - 146, 2, 16, new Color(180, 40, 30));
                float glow = 0.6f + 0.3f * MathF.Sin(_time * 4f);
                Box(sb, cx, -30, y - 60, 60, 30, new Color(255, 120, 40) * glow);
                sb.DrawGlow(new Vector2(cx, y - 45), 110f, new Color(255, 130, 60), 0.25f * glow);
                break;
            }

            case StructureKind.TapeDeck:
            {
                Box(sb, cx, -46, y - 44, 92, 8, new Color(90, 64, 44));
                Box(sb, cx, -40, y - 36, 8, 36, new Color(70, 50, 34));
                Box(sb, cx, 32, y - 36, 8, 36, new Color(70, 50, 34));
                Box(sb, cx, -40, y - 96, 80, 52, v == 1 ? new Color(60, 64, 70) : new Color(150, 120, 80));
                for (int i = 0; i < 2; i++)
                {
                    var c = new Vector2(cx - 18 + i * 36, y - 76);
                    sb.Draw(Graphics.Assets.Circle, new Rectangle((int)c.X - 13, (int)c.Y - 13, 26, 26), new Color(30, 30, 34));
                    float a = _time * 3f + i;
                    sb.FillRect(new Rectangle((int)(c.X + MathF.Cos(a) * 7f) - 2, (int)(c.Y + MathF.Sin(a) * 7f) - 2, 4, 4), new Color(200, 200, 200));
                }
                Box(sb, cx, -30, y - 54, 60, 6, new Color(30, 30, 34));
                Box(sb, cx, 26, y - 56, 4, 4, new Color(255, 60, 40) * (0.5f + 0.5f * MathF.Sin(_time * 5f))); // Kayıt ışığı
                break;
            }
        }
    }

    /// <summary>
    /// Buza kesmiş insanlar: yarı saydam buz bloklarının içinde koyu siluetler.
    /// 0: tek kişi ayakta, 1: sobanın başında üç kişi, 2: bankta bilet tutan biri, 3: el ele bir yetişkin ve bir çocuk.
    /// </summary>
    private void DrawFrozen(SpriteBatch sb, float cx, int variant)
    {
        int y = BaseY + 26;
        var ice = new Color(170, 210, 240);
        var body = new Color(40, 56, 80);
        void Figure(float x, int height, bool sitting)
        {
            int h = sitting ? height * 2 / 3 : height;
            Box(sb, x, -16, y - h - 10, 32, h + 10, ice * 0.35f);              // Buz bloğu
            Box(sb, x, -16, y - h - 10, 32, 3, Color.White * 0.5f);             // Parlama
            Box(sb, x, -7, y - h, 14, 12, body);                                 // Baş
            Box(sb, x, -9, y - h + 12, 18, h - 12 - (sitting ? 6 : 16), body);   // Gövde
            if (!sitting)
            {
                Box(sb, x, -8, y - 16, 6, 16, body);
                Box(sb, x, 2, y - 16, 6, 16, body);
            }
            else Box(sb, x, -9, y - 10, 24, 6, body);
            Box(sb, x, 10, y - h + 4, 3, h / 2, Color.White * 0.25f);
        }

        switch (variant)
        {
            case 1:
                Box(sb, cx, -14, y - 30, 28, 30, new Color(60, 60, 66));          // Sönmüş soba
                Figure(cx - 50, 70, true);
                Figure(cx + 50, 70, true);
                Figure(cx, 74, false);
                break;
            case 2:
                Box(sb, cx, -60, y - 30, 120, 8, new Color(70, 56, 40));          // Bank
                Figure(cx - 26, 72, true);
                Figure(cx + 26, 50, true);
                Box(sb, cx, 30, y - 44, 10, 6, new Color(230, 210, 160));         // Bilet
                break;
            case 3:
                Figure(cx - 14, 76, false);
                Figure(cx + 18, 48, false);
                Box(sb, cx, -2, y - 40, 16, 4, body);                               // Tutuşan eller
                break;
            default:
                Figure(cx, 78, false);
                break;
        }
        sb.DrawGlow(new Vector2(cx, y - 40), 80f, ice, 0.12f);
    }

    /// <summary>Terk edilmiş vagonlar: 0 yan yatmış yük vagonu, 1 karanlık camlı yolcu vagonu, 2 çatlak tüplü uyku konvoyu.</summary>
    private void DrawWagonWreck(SpriteBatch sb, float cx, int variant)
    {
        int y = BaseY + 10;
        switch (variant)
        {
            case 1:
                Box(sb, cx, -130, y - 110, 260, 90, new Color(70, 40, 36));
                Box(sb, cx, -130, y - 118, 260, 10, new Color(90, 56, 50));
                for (int i = 0; i < 5; i++) Box(sb, cx, -114 + i * 50, y - 96, 34, 30, new Color(16, 18, 26));
                Box(sb, cx, -114 + 2 * 50 + 8, y - 90, 10, 14, new Color(40, 56, 80)); // Camda bir siluet
                sb.Draw(Graphics.Assets.Circle, new Rectangle((int)cx - 100, y - 26, 26, 26), new Color(30, 28, 28));
                sb.Draw(Graphics.Assets.Circle, new Rectangle((int)cx + 74, y - 26, 26, 26), new Color(30, 28, 28));
                Box(sb, cx, -130, y - 124, 260, 8, new Color(220, 226, 236)); // Çatıda kar
                break;
            case 2:
                Box(sb, cx, -120, y - 90, 240, 76, new Color(80, 90, 104));
                for (int i = 0; i < 4; i++)
                {
                    Box(sb, cx, -106 + i * 58, y - 82, 40, 58, new Color(120, 200, 240) * 0.35f);
                    Box(sb, cx, -100 + i * 58, y - 70, 3, 30, Color.White * 0.4f);
                }
                Box(sb, cx, -48, y - 82, 2, 58, new Color(30, 30, 40)); // Çatlak
                sb.DrawGlow(new Vector2(cx, y - 50), 120f, new Color(120, 200, 255), 0.15f);
                break;
            default:
                // Yan yatmış yük vagonu: kapısı açık, içi karla dolu
                Box(sb, cx, -140, y - 70, 280, 64, new Color(96, 60, 40));
                Box(sb, cx, -140, y - 76, 280, 6, new Color(120, 80, 56));
                Box(sb, cx, -40, y - 62, 70, 48, new Color(20, 16, 14));
                Box(sb, cx, -40, y - 30, 70, 16, new Color(220, 226, 236));
                Box(sb, cx, -150, y - 14, 300, 14, new Color(220, 226, 236)); // Kar yığını
                break;
        }
    }
}
