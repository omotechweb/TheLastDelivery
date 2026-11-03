using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheLastDelivery.Economy;
using TheLastDelivery.Graphics;

namespace TheLastDelivery.UI;

/// <summary>
/// Kabin içi süslerin (Sığınak Kuryeleri kozmetik paketi) çizimi: duvardaki tablo, raftaki radyo
/// ve kostümlü Arthur. Hem kabin menüsünde hem de özelleştirme ekranının önizlemesinde kullanılır.
/// </summary>
public static class CabinDecor
{
    /// <summary>Tahta duvarlı, sobalı küçük kabin sahnesi: tablo, radyo ve Arthur bir arada.</summary>
    public static void DrawInterior(SpriteBatch sb, Rectangle r, CosmeticItem painting, CosmeticItem radio, CosmeticItem costume,
        float alpha, float time, PetKind pet = PetKind.None)
    {
        var interior = Cosmetics.Theme(SaveData.Current.EquippedTheme).Interior;
        if (interior != CabinInterior.Classic)
        {
            DrawBackdrop(sb, r, interior, alpha, time);
            if (interior != CabinInterior.Hologram) DrawPainting(sb, new Rectangle(r.X + 112, r.Y + 20, 76, 56), painting, alpha);
            DrawRadio(sb, new Point(r.X + 255, r.Y + 74), radio, alpha, time);
            DrawArthur(sb, new Point(r.X + r.Width / 2 + 60, r.Bottom - 26), costume, alpha);
            DrawPet(sb, new Point(r.X + r.Width / 2 + 10, r.Bottom - 26), pet, 3, time, alpha);
            return;
        }

        // Ahşap duvar ve döşeme
        sb.FillRect(r, new Color(58, 40, 30) * alpha);
        for (int x = r.X; x < r.Right; x += 26)
            sb.FillRect(new Rectangle(x, r.Y, 2, r.Height), new Color(44, 30, 22) * alpha);
        sb.FillRect(new Rectangle(r.X, r.Bottom - 26, r.Width, 26), new Color(40, 28, 22) * alpha);

        // Pencere: dışarıda kar
        var window = new Rectangle(r.X + 20, r.Y + 24, 70, 54);
        sb.FillRect(window, new Color(20, 26, 46) * alpha);
        for (int i = 0; i < 10; i++)
        {
            int sx = window.X + (int)((i * 23 + time * 12f) % window.Width);
            int sy = window.Y + (int)((i * 17 + time * 30f) % window.Height);
            sb.FillRect(new Rectangle(sx, sy, 2, 2), Color.White * (0.8f * alpha));
        }
        sb.DrawBorder(window, 3, new Color(90, 64, 44) * alpha);

        // Soba ve ateşi
        var stove = new Rectangle(r.Right - 70, r.Bottom - 70, 44, 44);
        sb.FillRect(stove, new Color(30, 30, 34) * alpha);
        sb.FillRect(new Rectangle(stove.X + 10, stove.Y + 18, 24, 14), new Color(255, 140, 50) * ((0.7f + 0.3f * MathF.Sin(time * 9f)) * alpha));
        sb.FillRect(new Rectangle(stove.Center.X - 4, r.Y, 8, stove.Y - r.Y), new Color(36, 36, 40) * alpha);
        sb.DrawGlow(new Vector2(stove.Center.X, stove.Center.Y + 6), 60f, new Color(255, 150, 70), 0.35f * alpha);

        DrawPainting(sb, new Rectangle(r.X + 112, r.Y + 20, 76, 56), painting, alpha);

        // Raf ve radyo
        sb.FillRect(new Rectangle(r.X + 210, r.Y + 74, 90, 6), new Color(100, 72, 48) * alpha);
        DrawRadio(sb, new Point(r.X + 255, r.Y + 74), radio, alpha, time);

        DrawArthur(sb, new Point(r.X + r.Width / 2 + 60, r.Bottom - 26), costume, alpha);
        DrawPet(sb, new Point(r.X + r.Width / 2 + 10, r.Bottom - 26), pet, 3, time, alpha); // Arthur'un yanında
    }

    /// <summary>
    /// Kabin iç tasarımları (yalnızca renk değil, mobilya ve ışık tamamen değişir):
    ///   Vintage: cilalı ahşap lambri, pirinç lamba, kadife koltuk, kitap rafı ve gramofon.
    ///   Command: askeri komuta odası; haritalı masa, çelik dolaplar, telsiz konsolu, kırmızı alarm lambası.
    ///   Hologram: siberpunk kokpit; karanlık panel, camgöbeği hologram harita ve dönen AURA uydusu, neon çizgiler.
    /// </summary>
    public static void DrawBackdrop(SpriteBatch sb, Rectangle r, CabinInterior interior, float alpha, float time)
    {
        void Fill(int x, int y, int w, int h, Color c) => sb.FillRect(new Rectangle(r.X + x, r.Y + y, w, h), c * alpha);
        int floor = r.Height - 26;
        switch (interior)
        {
            case CabinInterior.Vintage:
            {
                Fill(0, 0, r.Width, r.Height, new Color(86, 50, 30));
                for (int x = 0; x < r.Width; x += 40) Fill(x, 0, 3, floor, new Color(110, 66, 38));   // Lambri
                Fill(0, floor - 60, r.Width, 4, new Color(150, 100, 50));                              // Pervaz
                Fill(0, floor, r.Width, 26, new Color(60, 30, 26));
                for (int x = 0; x < r.Width; x += 30) Fill(x, floor + 8, 20, 4, new Color(120, 40, 40)); // Halı deseni
                Fill(20, 24, 70, 54, new Color(30, 30, 50));                                           // Pencere
                sb.DrawBorder(new Rectangle(r.X + 20, r.Y + 24, 70, 54), 4, new Color(170, 120, 60) * alpha);
                Fill(r.Width - 120, 10, 90, 70, new Color(70, 40, 24));                                // Kitap rafı
                for (int i = 0; i < 9; i++) Fill(r.Width - 114 + i * 9, 18, 7, 24, i % 3 == 0 ? new Color(140, 40, 40) : i % 3 == 1 ? new Color(50, 80, 60) : new Color(180, 150, 90));
                for (int i = 0; i < 9; i++) Fill(r.Width - 114 + i * 9, 48, 7, 24, i % 2 == 0 ? new Color(90, 60, 120) : new Color(160, 120, 70));
                Fill(30, floor - 46, 70, 46, new Color(130, 30, 40));                                  // Kadife koltuk
                Fill(26, floor - 60, 78, 16, new Color(110, 24, 34));
                float lamp = 0.8f + 0.2f * MathF.Sin(time * 3f);
                Fill(r.Width / 2 - 4, 0, 4, 30, new Color(150, 110, 50));                              // Pirinç tavan lambası
                Fill(r.Width / 2 - 14, 30, 24, 10, new Color(255, 210, 130) * lamp);
                sb.DrawGlow(new Vector2(r.X + r.Width / 2f, r.Y + 40), 120f, new Color(255, 200, 120), 0.3f * alpha * lamp);
                break;
            }
            case CabinInterior.Command:
            {
                Fill(0, 0, r.Width, r.Height, new Color(54, 62, 52));
                for (int y = 0; y < floor; y += 22) Fill(0, y, r.Width, 2, new Color(44, 50, 42));        // Çelik plakalar
                for (int x = 10; x < r.Width; x += 40) Fill(x, 6, 3, 3, new Color(100, 110, 96));         // Perçinler
                Fill(0, floor, r.Width, 26, new Color(36, 40, 34));
                for (int x = 0; x < r.Width; x += 20) Fill(x, floor + 4, 10, 3, new Color(220, 180, 50)); // Uyarı şeridi
                Fill(20, 20, 120, 70, new Color(190, 180, 140));                                         // Duvar haritası
                Fill(30, 30, 50, 4, new Color(140, 60, 50)); Fill(60, 50, 4, 30, new Color(140, 60, 50));
                Fill(90, 40, 30, 3, new Color(50, 80, 140));
                sb.DrawBorder(new Rectangle(r.X + 20, r.Y + 20, 120, 70), 3, new Color(80, 70, 50) * alpha);
                Fill(r.Width - 110, 20, 90, floor - 20, new Color(70, 78, 70));                          // Çelik dolap
                Fill(r.Width - 66, 30, 2, floor - 40, new Color(40, 46, 40));
                Fill(40, floor - 40, 160, 12, new Color(90, 70, 50));                                    // Harita masası
                Fill(50, floor - 28, 8, 28, new Color(60, 46, 34)); Fill(182, floor - 28, 8, 28, new Color(60, 46, 34));
                bool alarm = MathF.Sin(time * 4f) > 0.4f;
                Fill(r.Width / 2 - 8, 4, 16, 10, alarm ? new Color(255, 50, 40) : new Color(90, 30, 30));
                if (alarm) sb.DrawGlow(new Vector2(r.X + r.Width / 2f, r.Y + 9), 70f, new Color(255, 50, 40), 0.35f * alpha);
                break;
            }
            case CabinInterior.Hologram:
            {
                Fill(0, 0, r.Width, r.Height, new Color(10, 10, 20));
                for (int x = 0; x < r.Width; x += 24) Fill(x, 0, 1, floor, new Color(40, 30, 70));          // Panel ızgarası
                for (int y = 0; y < floor; y += 24) Fill(0, y, r.Width, 1, new Color(40, 30, 70));
                Fill(0, floor, r.Width, 26, new Color(16, 14, 30));
                Fill(0, floor, r.Width, 2, new Color(200, 80, 255));                                      // Neon zemin çizgisi
                var cyan = new Color(90, 230, 255);
                // Hologram harita: dönen uydu ve dünya küresi
                var center = new Vector2(r.X + 110, r.Y + 64);
                sb.Draw(Graphics.Assets.Circle, new Rectangle((int)center.X - 34, (int)center.Y - 34, 68, 68), cyan * (0.18f * alpha));
                for (int i = 0; i < 3; i++) sb.FillRect(new Rectangle((int)center.X - 34, (int)center.Y - 20 + i * 18, 68, 1), cyan * (0.4f * alpha));
                float a = time * 1.2f;
                var sat = center + new Vector2(MathF.Cos(a) * 50f, MathF.Sin(a) * 18f);
                sb.FillRect(new Rectangle((int)sat.X - 5, (int)sat.Y - 2, 10, 4), new Color(255, 60, 70) * alpha);
                sb.DrawGlow(center, 70f, cyan, 0.2f * alpha);
                // Konsol ve kayan veri
                Fill(r.Width - 170, floor - 50, 150, 50, new Color(24, 20, 40));
                for (int i = 0; i < 4; i++)
                {
                    int len = 30 + (int)((MathF.Sin(time * 2f + i) * 0.5f + 0.5f) * 90f);
                    Fill(r.Width - 160, floor - 44 + i * 10, len, 3, (i % 2 == 0 ? cyan : new Color(200, 80, 255)) * 0.8f);
                }
                if (MathF.Sin(time * 11f) > 0.9f) Fill(0, (int)(time * 90f % floor), r.Width, 3, new Color(200, 80, 255) * 0.4f); // Parazit
                break;
            }
        }
    }

    /// <summary>Çerçeveli tablo: dağ manzarası, eski dünya kenti ya da aile portresi (yoksa boş çivi).</summary>
    public static void DrawPainting(SpriteBatch sb, Rectangle r, CosmeticItem item, float alpha)
    {
        if (item == null || item.Value == 0)
        {
            sb.FillRect(new Rectangle(r.Center.X - 2, r.Y + 4, 4, 4), new Color(150, 150, 160) * alpha); // Boş çivi
            return;
        }

        var frame = new Color(170, 130, 60);
        sb.FillRect(new Rectangle(r.X - 4, r.Y - 4, r.Width + 8, r.Height + 8), frame * alpha);
        sb.FillRect(r, new Color(20, 20, 26) * alpha);
        void Box(float x, float y, float w, float h, Color c) =>
            sb.FillRect(new Rectangle(r.X + (int)(x * r.Width), r.Y + (int)(y * r.Height),
                Math.Max(1, (int)(w * r.Width)), Math.Max(1, (int)(h * r.Height))), c * alpha);

        switch (item.Value)
        {
            case 1: // Dağ manzarası: karlı zirveler ve gün batımı
                Box(0, 0, 1, 1, new Color(240, 170, 110));
                Box(0, 0.5f, 1, 0.5f, new Color(70, 90, 130));
                Box(0.1f, 0.25f, 0.35f, 0.45f, item.Primary);
                Box(0.18f, 0.25f, 0.12f, 0.12f, item.Secondary);
                Box(0.45f, 0.15f, 0.45f, 0.55f, UiDraw.Shade(item.Primary, 0.8f));
                Box(0.57f, 0.15f, 0.15f, 0.14f, item.Secondary);
                break;
            case 2: // Eski dünya kenti: gece ışıkları
                Box(0, 0, 1, 1, new Color(30, 30, 60));
                for (int i = 0; i < 6; i++)
                {
                    float h = 0.3f + (i * 37 % 5) * 0.1f;
                    Box(i / 6f + 0.01f, 1f - h, 0.14f, h, item.Secondary);
                    Box(i / 6f + 0.05f, 1f - h + 0.08f, 0.04f, 0.06f, item.Primary);
                }
                break;
            default: // Aile portresi: iki silüet
                Box(0, 0, 1, 1, new Color(120, 90, 70));
                Box(0.2f, 0.25f, 0.22f, 0.25f, item.Primary);
                Box(0.15f, 0.5f, 0.32f, 0.5f, item.Secondary);
                Box(0.58f, 0.3f, 0.2f, 0.22f, item.Primary);
                Box(0.53f, 0.52f, 0.3f, 0.48f, UiDraw.Shade(item.Secondary, 1.3f));
                break;
        }
    }

    /// <summary>Radyo modeli: ahşap lambalı radyo, askeri saha telsizi ya da gramofon. Ses dalgaları kıpırdar.</summary>
    public static void DrawRadio(SpriteBatch sb, Point bottomCenter, CosmeticItem item, float alpha, float time)
    {
        int x = bottomCenter.X, y = bottomCenter.Y;
        var body = item?.Primary ?? new Color(110, 80, 54);
        var glow = item?.Secondary ?? new Color(230, 200, 140);
        switch (item?.Value ?? 0)
        {
            case 1: // Askeri saha telsizi: haki kutu, anten, yeşil gösterge
                sb.FillRect(new Rectangle(x - 22, y - 26, 44, 26), body * alpha);
                sb.FillRect(new Rectangle(x + 14, y - 52, 2, 26), new Color(60, 60, 64) * alpha);
                sb.FillRect(new Rectangle(x - 16, y - 20, 18, 8), glow * ((0.6f + 0.4f * MathF.Sin(time * 5f)) * alpha));
                sb.FillRect(new Rectangle(x + 6, y - 20, 6, 6), new Color(40, 40, 40) * alpha);
                break;
            case 2: // Gramofon: plak ve pirinç boru
                sb.FillRect(new Rectangle(x - 20, y - 12, 40, 12), body * alpha);
                sb.FillRect(new Rectangle(x - 16, y - 16, 28, 4), new Color(20, 20, 22) * alpha);
                for (int i = 0; i < 4; i++)
                    sb.FillRect(new Rectangle(x + 2 + i * 4, y - 22 - i * 6, 6 + i * 4, 6), glow * alpha);
                break;
            default: // Ahşap lambalı radyo
                sb.FillRect(new Rectangle(x - 20, y - 30, 40, 30), body * alpha);
                sb.FillRect(new Rectangle(x - 14, y - 24, 28, 12), new Color(60, 44, 30) * alpha);
                sb.FillRect(new Rectangle(x - 12, y - 10, 24, 4), glow * ((0.6f + 0.4f * MathF.Sin(time * 4f)) * alpha));
                break;
        }

        // Çalarken yükselen nota/dalga çizgileri
        for (int i = 0; i < 3; i++)
        {
            float t = (time * 0.8f + i * 0.33f) % 1f;
            sb.FillRect(new Rectangle(x + 24 + (int)(t * 14), y - 30 - (int)(t * 30), 4, 2), glow * ((1f - t) * alpha));
        }
    }

    /// <summary>
    /// Oturan evcil hayvan (yan görünüş). Kutup köpeği: gri sırt, beyaz göğüs, kıvrık kuyruk;
    /// kedi: turuncu, sivri kulaklar, dik kuyruk. Kuyruk sallanır, gözler arada kırpılır.
    /// </summary>
    /// <param name="feet">Patilerin değdiği nokta.</param>
    /// <param name="facing">1 sağa, -1 sola bakar.</param>
    public static void DrawPet(SpriteBatch sb, Point feet, PetKind kind, int scale, float time, float alpha = 1f, int facing = 1)
    {
        if (kind == PetKind.None) return;
        void Px(int x, int y, int w, int h, Color c)
        {
            int px = facing >= 0 ? feet.X + x * scale : feet.X - (x + w) * scale;
            sb.FillRect(new Rectangle(px, feet.Y + y * scale, w * scale, h * scale), c * alpha);
        }

        int wag = MathF.Sin(time * 9f) > 0f ? 1 : 0;
        bool blink = MathF.Sin(time * 1.3f) > 0.97f;
        if (kind == PetKind.Husky)
        {
            var back = new Color(70, 76, 90);
            var fur = new Color(205, 210, 220);
            Px(-4, -11 - wag, 4, 3, fur);          // Kıvrık kuyruk
            Px(-2, -12 - wag, 3, 2, back);
            Px(0, -9, 12, 9, back);               // Gövde (oturuş)
            Px(2, -5, 9, 5, fur);                 // Göğüs ve karın
            Px(9, -2, 3, 2, fur);                 // Ön pati
            Px(8, -15, 8, 7, back);               // Baş
            Px(11, -12, 6, 4, fur);               // Yüz maskesi
            Px(15, -11, 3, 2, fur);               // Burun
            Px(17, -11, 1, 1, new Color(20, 20, 24));
            Px(9, -18, 2, 3, back);               // Kulaklar
            Px(13, -18, 2, 3, back);
            if (!blink) Px(13, -13, 1, 1, new Color(90, 170, 255)); // Mavi göz
        }
        else
        {
            var orange = new Color(230, 150, 70);
            var light = new Color(255, 225, 190);
            Px(-2, -16 + wag, 2, 12, orange);      // Dik kuyruk
            Px(0, -8, 9, 8, orange);              // Gövde
            Px(2, -4, 6, 4, light);
            Px(6, -13, 7, 6, orange);             // Baş
            Px(6, -15, 2, 2, orange);             // Sivri kulaklar
            Px(11, -15, 2, 2, orange);
            Px(9, -10, 3, 2, light);              // Ağız
            if (!blink) Px(10, -12, 1, 1, new Color(90, 200, 90)); // Yeşil göz
            Px(7, -11, 1, 1, new Color(160, 90, 40)); // Çizgi
        }
    }

    /// <summary>Kabinde ayakta duran Arthur (kostüm renkleriyle).</summary>
    public static void DrawArthur(SpriteBatch sb, Point feet, CosmeticItem costume, float alpha)
    {
        var coat = costume?.Primary ?? new Color(58, 46, 38);
        var scarf = costume?.Secondary ?? new Color(150, 42, 40);
        void Px(int dx, int dy, int w, int h, Color c) => sb.FillRect(new Rectangle(feet.X + dx * 2, feet.Y + dy * 2, w * 2, h * 2), c * alpha);

        Px(-6, -16, 5, 13, new Color(40, 36, 40));
        Px(1, -16, 5, 13, new Color(40, 36, 40));
        Px(-7, -4, 7, 4, new Color(28, 24, 22));
        Px(0, -4, 7, 4, new Color(28, 24, 22));
        Px(-9, -44, 18, 30, coat);
        Px(-1, -40, 2, 24, UiDraw.Shade(coat, 0.7f));
        Px(-8, -46, 16, 5, scarf);
        Px(-5, -56, 10, 10, new Color(200, 158, 128));
        Px(-5, -49, 10, 4, new Color(190, 190, 198));
        // Kostüme göre başlık: subay şapkası, kutup kaşifi kapüşonu ya da kürk şapka
        switch (costume?.Value ?? 0)
        {
            case 1:
                Px(-7, -62, 14, 6, coat);
                Px(-8, -57, 16, 2, UiDraw.Shade(coat, 0.6f));
                Px(-2, -61, 4, 3, scarf);              // Altın arma
                Px(-11, -44, 4, 3, scarf);             // Apoletler
                Px(7, -44, 4, 3, scarf);
                break;
            case 2:
                Px(-8, -64, 16, 10, coat);
                Px(-9, -58, 3, 12, new Color(235, 235, 240)); // Kürk kapüşon kenarı
                Px(6, -58, 3, 12, new Color(235, 235, 240));
                break;
            case 3: // Avcı: geniş kenarlı şapka ve kürk yaka
                Px(-11, -60, 22, 3, UiDraw.Shade(coat, 0.7f));
                Px(-6, -66, 12, 6, UiDraw.Shade(coat, 0.8f));
                Px(-10, -46, 20, 4, scarf);
                break;
            case 4: // İstasyon şefi: siperlikli şapka, altın şerit ve düdük
                Px(-7, -64, 14, 7, coat);
                Px(-7, -60, 14, 2, scarf);
                Px(-1, -63, 2, 2, scarf);
                Px(-8, -57, 18, 2, UiDraw.Shade(coat, 0.6f));
                Px(8, -36, 3, 6, scarf); // Düdük zinciri
                break;
            default:
                Px(-7, -63, 14, 8, new Color(92, 78, 62));
                break;
        }
    }
}
