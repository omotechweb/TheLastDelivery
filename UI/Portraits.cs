using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheLastDelivery.Graphics;

namespace TheLastDelivery.UI;

/// <summary>Diyalogda konuşan kişinin portresi.</summary>
public enum PortraitKind
{
    Arthur, Innkeeper, Smith, Quartermaster, BunkerGuard,
    Watchman, MinerKid, LanternKeeper,
    Viktor, Elena, Mika, Bora, Nadia, Oskar, Lina,
    // Tam sürüm
    Hermit, SaltWorker, GateKeeper, Peddler,
    Ilya, Zoya, Taras, Grigor, Vera, Feliks, Anya, Pavel, Lev,
    // Hikâye güncellemesi: kasaba karakterleri ve hikâye bölümleri
    Auctioneer, Merchant, StrikeLeader, Foreman, Priestess, Acolyte, Officer, Prisoner,
    Inspector, Refugee, Sofi, Envoy, Dmitri, Mila,
    // Episode 2 "Project AURA"
    Nova, Ada, Kiril, Yusuf, Mara, Oracle, Warden, Droid, Aura,
    // Hikâye derinleştirme: Kızıl Kurtlar'ın lideri (eski Komutan Morozov)
    WolfFather
}

/// <summary>Konuşan kişi: ekranda adı ve portresi gösterilir.</summary>
public sealed record Speaker(string Name, PortraitKind Portrait)
{
    /// <summary>Oyuncunun kendisi (Arthur) mi? Destekçi Rozeti yalnızca onun adının yanında görünür.</summary>
    public bool IsPlayer { get; init; }
}

/// <summary>
/// 20x20'lik piksel ızgarasında, renk bloklarıyla çizilen portreler.
/// Her karakter aynı yüz şablonunu farklı şapka, saç, sakal ve renklerle kullanır.
/// </summary>
public static class Portraits
{
    private enum Hat { None, Ushanka, Cap, Beanie, Hood, Helmet, NurseCap, Headscarf, Bald }

    private sealed record Look(Color Background, Color Skin, Color Coat, Color HatColor, Color Hair,
        Hat Hat, bool Beard, bool Mustache, bool Child = false, Color? Accent = null);

    private static Look LookFor(PortraitKind kind) => kind switch
    {
        PortraitKind.Arthur => new(new(40, 30, 26), new(196, 154, 124), new(58, 46, 38), new(92, 78, 62), new(190, 190, 198), Hat.Ushanka, true, true, Accent: new(150, 42, 40)),
        PortraitKind.Innkeeper => new(new(60, 40, 26), new(214, 168, 136), new(200, 190, 170), new(0, 0, 0), new(70, 50, 40), Hat.Bald, false, true),
        PortraitKind.Smith => new(new(70, 34, 18), new(170, 120, 96), new(70, 50, 40), new(50, 40, 34), new(30, 26, 26), Hat.Cap, true, false, Accent: new(255, 140, 60)),
        PortraitKind.Quartermaster => new(new(34, 40, 30), new(200, 160, 130), new(60, 70, 50), new(60, 110, 70), new(120, 90, 60), Hat.Beanie, false, true),
        PortraitKind.BunkerGuard => new(new(28, 34, 40), new(190, 150, 124), new(56, 62, 70), new(90, 96, 104), new(50, 50, 50), Hat.Helmet, false, false, Accent: new(230, 190, 70)),
        PortraitKind.Watchman => new(new(30, 32, 44), new(190, 150, 126), new(60, 64, 80), new(70, 74, 90), new(220, 220, 228), Hat.Hood, true, true),
        PortraitKind.MinerKid => new(new(40, 34, 30), new(210, 170, 140), new(80, 60, 50), new(180, 50, 50), new(60, 40, 30), Hat.Beanie, false, false, Child: true),
        PortraitKind.LanternKeeper => new(new(60, 46, 24), new(196, 156, 128), new(70, 56, 40), new(100, 80, 56), new(200, 200, 205), Hat.Hood, true, false, Accent: new(255, 200, 110)),
        PortraitKind.Viktor => new(new(30, 36, 50), new(196, 156, 128), new(50, 60, 84), new(40, 54, 90), new(160, 160, 166), Hat.Cap, false, true, Accent: new(255, 170, 80)),
        PortraitKind.Elena => new(new(40, 30, 46), new(214, 172, 146), new(70, 50, 80), new(110, 60, 120), new(90, 60, 40), Hat.Headscarf, false, false, Accent: new(150, 150, 160)),
        PortraitKind.Mika => new(new(36, 34, 40), new(220, 180, 150), new(90, 70, 60), new(70, 110, 160), new(110, 70, 40), Hat.Beanie, false, false, Child: true),
        PortraitKind.Bora => new(new(60, 30, 20), new(160, 116, 90), new(60, 46, 38), new(40, 34, 30), new(60, 56, 56), Hat.Cap, true, true, Accent: new(255, 130, 60)),
        PortraitKind.Nadia => new(new(36, 40, 48), new(212, 170, 142), new(220, 222, 228), new(235, 235, 240), new(120, 80, 50), Hat.NurseCap, false, false, Accent: new(210, 50, 50)),
        PortraitKind.Oskar => new(new(30, 36, 28), new(180, 140, 112), new(64, 74, 54), new(70, 84, 56), new(60, 50, 40), Hat.Helmet, true, false),
        PortraitKind.Lina => new(new(46, 36, 26), new(214, 170, 140), new(110, 70, 40), new(150, 120, 90), new(140, 60, 40), Hat.Hood, false, false, Accent: new(240, 200, 90)),
        PortraitKind.Hermit => new(new(24, 28, 40), new(186, 148, 120), new(40, 44, 56), new(30, 32, 40), new(210, 210, 215), Hat.Hood, true, true, Accent: new(170, 200, 255)),
        PortraitKind.SaltWorker => new(new(54, 54, 50), new(204, 162, 132), new(150, 146, 130), new(220, 220, 210), new(80, 60, 44), Hat.Beanie, false, true),
        PortraitKind.GateKeeper => new(new(50, 26, 22), new(176, 132, 104), new(70, 40, 34), new(90, 90, 96), new(40, 30, 26), Hat.Helmet, true, false, Accent: new(255, 140, 110)),
        PortraitKind.Peddler => new(new(52, 40, 22), new(200, 156, 122), new(120, 84, 40), new(160, 60, 40), new(60, 40, 30), Hat.Cap, false, true, Accent: new(255, 215, 110)),
        PortraitKind.Ilya => new(new(26, 34, 52), new(196, 156, 128), new(50, 64, 90), new(60, 76, 110), new(70, 50, 36), Hat.Ushanka, false, false, Accent: new(170, 200, 255)),
        PortraitKind.Zoya => new(new(40, 40, 36), new(214, 172, 146), new(200, 200, 190), new(230, 230, 220), new(150, 90, 50), Hat.NurseCap, false, false, Accent: new(60, 160, 90)),
        PortraitKind.Taras => new(new(44, 24, 20), new(170, 126, 98), new(80, 40, 30), new(60, 40, 34), new(30, 24, 20), Hat.Bald, true, false, Accent: new(255, 140, 110)),
        PortraitKind.Grigor => new(new(40, 32, 20), new(190, 150, 120), new(90, 64, 40), new(50, 40, 30), new(40, 30, 24), Hat.Cap, false, true, Accent: new(240, 200, 90)),
        PortraitKind.Vera => new(new(30, 36, 44), new(214, 170, 140), new(70, 80, 96), new(80, 90, 110), new(170, 110, 60), Hat.Beanie, false, false, Accent: new(255, 170, 80)),
        PortraitKind.Feliks => new(new(30, 26, 30), new(190, 160, 140), new(30, 28, 32), new(20, 20, 24), new(200, 200, 205), Hat.Hood, true, false, Accent: new(200, 60, 60)),
        PortraitKind.Anya => new(new(34, 40, 30), new(206, 164, 134), new(90, 70, 50), new(150, 140, 120), new(200, 170, 110), Hat.Ushanka, false, false, Accent: new(140, 110, 80)),
        PortraitKind.Pavel => new(new(56, 30, 20), new(166, 120, 94), new(60, 46, 38), new(44, 38, 34), new(90, 70, 50), Hat.Cap, true, false, Accent: new(255, 130, 60)),
        PortraitKind.Lev => new(new(34, 34, 46), new(200, 160, 130), new(60, 60, 80), new(90, 90, 110), new(60, 50, 40), Hat.Headscarf, false, true, Accent: new(150, 150, 160)),
        PortraitKind.Auctioneer => new(new(56, 36, 20), new(214, 170, 140), new(120, 40, 50), new(150, 50, 60), new(60, 30, 26), Hat.Headscarf, false, false, Accent: new(255, 215, 110)),
        PortraitKind.Merchant => new(new(44, 34, 18), new(196, 152, 120), new(90, 60, 30), new(110, 80, 40), new(30, 26, 22), Hat.Ushanka, false, true, Accent: new(255, 200, 80)),
        PortraitKind.StrikeLeader => new(new(50, 26, 22), new(176, 130, 102), new(70, 60, 54), new(150, 40, 40), new(40, 32, 28), Hat.Beanie, true, false, Accent: new(230, 70, 60)),
        PortraitKind.Foreman => new(new(36, 36, 30), new(200, 156, 126), new(60, 54, 40), new(200, 170, 60), new(70, 60, 50), Hat.Helmet, false, true, Accent: new(255, 220, 120)),
        PortraitKind.Priestess => new(new(24, 32, 48), new(226, 214, 210), new(220, 226, 236), new(236, 240, 248), new(230, 230, 240), Hat.Hood, false, false, Accent: new(150, 200, 255)),
        PortraitKind.Acolyte => new(new(20, 26, 40), new(214, 196, 186), new(150, 160, 180), new(170, 180, 200), new(120, 110, 100), Hat.Hood, false, false, Child: true, Accent: new(150, 200, 255)),
        PortraitKind.Officer => new(new(30, 36, 30), new(190, 148, 120), new(60, 70, 54), new(70, 84, 60), new(40, 36, 30), Hat.Cap, false, true, Accent: new(230, 190, 70)),
        PortraitKind.Prisoner => new(new(26, 26, 30), new(170, 134, 112), new(70, 66, 60), new(0, 0, 0), new(90, 86, 80), Hat.Bald, true, false),
        PortraitKind.Inspector => new(new(34, 40, 52), new(200, 160, 132), new(70, 80, 100), new(90, 100, 120), new(60, 50, 40), Hat.Helmet, false, false, Accent: new(140, 200, 255)),
        PortraitKind.Refugee => new(new(36, 32, 30), new(204, 162, 134), new(80, 70, 60), new(120, 100, 80), new(90, 60, 40), Hat.Headscarf, false, false),
        PortraitKind.Sofi => new(new(20, 30, 48), new(216, 176, 148), new(60, 70, 96), new(120, 50, 60), new(120, 70, 40), Hat.Headscarf, false, false, Accent: new(140, 220, 255)),
        PortraitKind.Envoy => new(new(40, 40, 46), new(224, 196, 176), new(230, 232, 236), new(240, 240, 244), new(200, 200, 205), Hat.Ushanka, false, false, Accent: new(200, 220, 255)),
        PortraitKind.Dmitri => new(new(30, 34, 28), new(184, 140, 110), new(66, 76, 56), new(76, 88, 60), new(50, 44, 36), Hat.Helmet, true, true, Accent: new(230, 190, 70)),
        PortraitKind.Mila => new(new(36, 40, 50), new(214, 172, 146), new(200, 206, 216), new(220, 224, 232), new(160, 110, 70), Hat.NurseCap, false, false, Accent: new(80, 170, 220)),
        PortraitKind.Nova => new(new(24, 18, 40), new(206, 168, 140), new(40, 34, 56), new(200, 60, 200), new(60, 220, 230), Hat.Beanie, false, false, Accent: new(200, 80, 255)),
        PortraitKind.Ada => new(new(30, 40, 52), new(230, 214, 210), new(190, 200, 214), new(0, 0, 0), new(220, 210, 200), Hat.None, false, false, Accent: new(120, 200, 255)),
        PortraitKind.Kiril => new(new(20, 24, 30), new(196, 170, 150), new(40, 44, 52), new(140, 150, 170), new(30, 30, 34), Hat.Bald, false, false, Accent: new(80, 255, 200)),
        PortraitKind.Yusuf => new(new(34, 36, 28), new(170, 128, 100), new(70, 74, 56), new(80, 86, 64), new(30, 26, 22), Hat.Helmet, true, true, Accent: new(230, 190, 70)),
        PortraitKind.Mara => new(new(52, 30, 20), new(196, 150, 120), new(90, 60, 44), new(170, 90, 50), new(120, 60, 30), Hat.Headscarf, false, false, Accent: new(255, 140, 60)),
        PortraitKind.Oracle => new(new(16, 14, 22), new(220, 210, 216), new(30, 30, 36), new(170, 176, 190), new(200, 200, 210), Hat.Hood, false, false, Accent: new(80, 255, 200)),
        PortraitKind.Warden => new(new(40, 44, 50), new(206, 170, 146), new(150, 160, 176), new(200, 210, 220), new(100, 80, 60), Hat.NurseCap, false, false, Accent: new(120, 200, 255)),
        PortraitKind.WolfFather => new(new(48, 18, 16), new(184, 146, 120), new(214, 216, 220), new(120, 112, 104), new(176, 176, 182), Hat.Hood, true, true, Accent: new(220, 60, 50)),
        _ => new(new(30, 30, 40), new(200, 160, 130), new(60, 60, 70), new(80, 80, 90), new(80, 60, 40), Hat.None, false, false)
    };

    /// <summary>Yolcu kimliğinden ("viktor", "elena"...) portre türü.</summary>
    public static PortraitKind ForPassenger(string id) => id switch
    {
        "viktor" => PortraitKind.Viktor,
        "elena" => PortraitKind.Elena,
        "mika" => PortraitKind.Mika,
        "bora" => PortraitKind.Bora,
        "nadia" => PortraitKind.Nadia,
        "oskar" => PortraitKind.Oskar,
        "lina" => PortraitKind.Lina,
        "ilya" => PortraitKind.Ilya,
        "zoya" => PortraitKind.Zoya,
        "taras" => PortraitKind.Taras,
        "grigor" => PortraitKind.Grigor,
        "vera" => PortraitKind.Vera,
        "feliks" => PortraitKind.Feliks,
        "anya" => PortraitKind.Anya,
        "pavel" => PortraitKind.Pavel,
        "lev" => PortraitKind.Lev,
        "dmitri" => PortraitKind.Dmitri,
        "mila" => PortraitKind.Mila,
        "nova" => PortraitKind.Nova,
        "ada" => PortraitKind.Ada,
        "kiril" => PortraitKind.Kiril,
        "yusuf" => PortraitKind.Yusuf,
        "mara" => PortraitKind.Mara,
        _ => PortraitKind.Watchman
    };

    public static void Draw(SpriteBatch sb, Rectangle frame, PortraitKind kind, float alpha = 1f)
    {
        if (kind is PortraitKind.Aura or PortraitKind.Droid)
        {
            DrawMachine(sb, frame, kind == PortraitKind.Aura, alpha);
            return;
        }
        var look = LookFor(kind);
        int u = frame.Width / 20;

        void Px(int gx, int gy, int gw, int gh, Color c) =>
            sb.FillRect(new Rectangle(frame.X + gx * u, frame.Y + gy * u, gw * u, gh * u), c * alpha);

        Px(0, 0, 20, 20, look.Background);
        if (look.Accent.HasValue)
            sb.DrawGlow(new Vector2(frame.X + frame.Width * 0.75f, frame.Y + frame.Height * 0.3f), frame.Width * 0.5f, look.Accent.Value, 0.15f * alpha);

        int shrink = look.Child ? 1 : 0; // Çocuk yüzü biraz daha küçük ve aşağıda
        var shade = UiDraw.Shade(look.Skin, 0.82f);

        // Omuzlar ve yaka
        Px(1, 15 + shrink, 18, 5, look.Coat);
        Px(8, 15 + shrink, 4, 5, UiDraw.Shade(look.Coat, 0.75f));

        // Saç (şapkanın altından görünen)
        if (look.Hat is Hat.None or Hat.Bald or Hat.NurseCap or Hat.Headscarf)
            Px(4 + shrink, 5 + shrink, 12 - shrink * 2, 4, look.Hair);

        // Yüz
        Px(5 + shrink, 7 + shrink, 10 - shrink * 2, 7, look.Skin);
        Px(4 + shrink, 9 + shrink, 1, 3, shade);
        Px(15 - shrink, 9 + shrink, 1, 3, shade);

        // Gözler, burun
        Px(7, 9 + shrink, 1, 1, new Color(30, 30, 34));
        Px(12 - shrink, 9 + shrink, 1, 1, new Color(30, 30, 34));
        Px(9, 10 + shrink, 2, 2, shade);

        // Sakal / bıyık
        if (look.Mustache) Px(6 + shrink, 12 + shrink, 8 - shrink * 2, 1, look.Hair);
        if (look.Beard)
        {
            Px(5 + shrink, 13 + shrink, 10 - shrink * 2, 2, look.Hair);
            Px(7, 15, 6, 1, look.Hair);
        }
        else
        {
            Px(8, 12 + shrink, 4, 1, UiDraw.Shade(look.Skin, 0.65f)); // Ağız
        }

        // Başlık
        switch (look.Hat)
        {
            case Hat.Ushanka:
                Px(4, 2, 12, 5, look.HatColor);
                Px(3, 6, 2, 6, look.HatColor);
                Px(15, 6, 2, 6, look.HatColor);
                break;
            case Hat.Cap:
                Px(4, 3, 12, 4, look.HatColor);
                Px(3, 6, 14, 1, UiDraw.Shade(look.HatColor, 0.7f));
                if (look.Accent.HasValue) { Px(6, 4, 3, 2, look.Accent.Value); Px(11, 4, 3, 2, look.Accent.Value); } // Gözlük
                break;
            case Hat.Beanie:
                Px(4 + shrink, 3 + shrink, 12 - shrink * 2, 4, look.HatColor);
                Px(9, 2 + shrink, 2, 1, Color.White);
                break;
            case Hat.Hood:
                Px(3, 2, 14, 4, look.HatColor);
                Px(3, 5, 2, 10, look.HatColor);
                Px(15, 5, 2, 10, look.HatColor);
                break;
            case Hat.Helmet:
                Px(4, 2, 12, 5, look.HatColor);
                Px(3, 6, 14, 1, UiDraw.Shade(look.HatColor, 0.7f));
                if (look.Accent.HasValue) Px(9, 3, 2, 2, look.Accent.Value);
                break;
            case Hat.NurseCap:
                Px(5, 2, 10, 3, look.HatColor);
                if (look.Accent.HasValue) { Px(9, 2, 2, 3, look.Accent.Value); Px(8, 3, 4, 1, look.Accent.Value); }
                break;
            case Hat.Headscarf:
                Px(3, 3, 14, 4, look.HatColor);
                Px(3, 6, 2, 8, look.HatColor);
                if (look.Accent.HasValue) Px(15, 7, 2, 4, look.Accent.Value); // Kulaklık
                break;
        }

        // Atkı (Arthur) ya da yakadaki renk
        if (kind == PortraitKind.Arthur && look.Accent.HasValue)
            Px(5, 15, 10, 2, look.Accent.Value);

        sb.DrawBorder(frame, 2, Hud.Amber * (0.7f * alpha));
    }

    /// <summary>
    /// Makine portreleri. AURA: siyah zeminde eş merkezli halkalarla kızıl bir göz ve tarama çizgileri.
    /// Droid: kare kafa, tek mavi optik, anten.
    /// </summary>
    private static void DrawMachine(SpriteBatch sb, Rectangle frame, bool aura, float alpha)
    {
        int u = frame.Width / 20;
        void Px(int gx, int gy, int gw, int gh, Color c) =>
            sb.FillRect(new Rectangle(frame.X + gx * u, frame.Y + gy * u, gw * u, gh * u), c * alpha);

        double time = System.Environment.TickCount64 / 1000.0;
        if (aura)
        {
            Px(0, 0, 20, 20, new Color(6, 4, 8));
            var red = new Color(255, 40, 50);
            Px(3, 9, 14, 2, red * 0.35f);   // Göz kapağı çizgisi
            Px(6, 6, 8, 8, new Color(60, 8, 12));
            Px(7, 7, 6, 6, new Color(140, 16, 24));
            float pulse = 0.6f + 0.4f * (float)Math.Sin(time * 3.0);
            Px(8, 8, 4, 4, red * pulse);
            Px(9, 9, 2, 2, Color.White * pulse);
            for (int ring = 0; ring < 2; ring++)
            {
                int r = 4 + ring * 3;
                Px(10 - r, 10, 1, 1, red * 0.5f); Px(9 + r, 9, 1, 1, red * 0.5f);
                Px(9, 10 - r, 1, 1, red * 0.5f); Px(10, 9 + r, 1, 1, red * 0.5f);
            }
            int scan = (int)(time * 12.0) % 20;                 // Tarama çizgisi
            Px(0, scan, 20, 1, red * 0.15f);
            sb.DrawGlow(new Vector2(frame.Center.X, frame.Center.Y), frame.Width * 0.6f, red, 0.25f * alpha);
            sb.DrawBorder(frame, 2, red * (0.8f * alpha));
            return;
        }

        Px(0, 0, 20, 20, new Color(20, 26, 34));
        var steel = new Color(110, 120, 134);
        Px(9, 1, 2, 3, steel);                                   // Anten
        Px(8, 0, 4, 1, new Color(255, 80, 60));
        Px(4, 4, 12, 10, steel);                                 // Kafa
        Px(5, 5, 10, 8, new Color(70, 78, 90));
        Px(7, 7, 6, 3, new Color(10, 14, 20));                   // Optik bandı
        float blink = (float)(0.6 + 0.4 * Math.Sin(time * 5.0));
        Px(9, 8, 2, 1, new Color(90, 200, 255) * blink);
        Px(6, 11, 8, 1, new Color(40, 46, 56));                  // Hoparlör ızgarası
        Px(3, 15, 14, 5, new Color(80, 88, 100));                // Gövde
        Px(8, 16, 4, 2, new Color(255, 200, 60));
        sb.DrawBorder(frame, 2, new Color(90, 200, 255) * (0.7f * alpha));
    }
}
