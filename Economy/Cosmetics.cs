using Microsoft.Xna.Framework;
using TheLastDelivery.Localization;

namespace TheLastDelivery.Economy;

/// <summary>Skinin trene eklediği ayırt edici parçalar (renklerin yanında).</summary>
public enum TrainStyle
{
    None,
    Icebreaker, // Önde dev buz kıran sac pulluk
    Bandit,     // Paslı dikenler ve yamalı plakalar
    Royal,      // Altın taç süsü ve flama
    WolfHunter, // Kurt postu şeritleri ve diş sırası
    Miner,      // Kabin üstünde maden lambası, kömür sepeti
    Aurora,     // Kazanın üstünde süzülen kutup ışığı parıltısı
    GoldPlated, // Destekçi: baştan sona altın kaplama, parlak perçin sırası ve ışıltı
    Military    // Askeri tren: kamuflaj lekeleri, kum torbaları ve beyaz yıldız
}

/// <summary>
/// Lokomotifin 2D sprite mimarisi. Standard dışındakiler yalnızca renk değil, tamamen farklı gövde,
/// baca ve yürüyen aksamla çizilir (World/Train.cs).
/// </summary>
public enum TrainModel
{
    Standard,
    CyberGlitch, // AURA teknolojisi: neon mor duman, dijital parazit (Episode 2 DLC)
    Panzer,      // Heavy Panzer Express: koçbaşı, zırh, paletler; darbe hasarı %40 az (Episode 2 DLC)
    Citadel      // Steam-Punk Citadel: pirinç kazan, üç dev baca, görünen çarklar (mağaza)
}

/// <summary>Kabinin iç tasarımı (garaj önizlemesi ve kabin sorgusu ekranı).</summary>
public enum CabinInterior { Classic, Vintage, Command, Hologram }

/// <summary>Lokomotif ve vagonun renk takımı.</summary>
public sealed record TrainSkin(
    string Id, int Price,
    Color WagonBody, Color WagonPlank, Color Iron, Color Boiler, Color BoilerBand,
    Color CabBody, Color Roof, Color Shine, Color Trim)
{
    public TrainStyle Style { get; init; } = TrainStyle.None;

    /// <summary>Özgün model (gövde mimarisi).</summary>
    public TrainModel Model { get; init; } = TrainModel.Standard;

    /// <summary>Yalnızca renk değiştiren eski skin: artık satılmaz, sahibine görünmeye devam eder.</summary>
    public bool Retired { get; init; }

    /// <summary>Satın alınabilmesi için gereken başarım (yoksa yalnızca fiyat).</summary>
    public string Achievement { get; init; }

    /// <summary>Bu skini içeren DLC (null = ana oyun). DLC sahibine ücretsiz açılır.</summary>
    public Platform.DlcId? Dlc { get; init; }

    public string Name => Loc.T($"skin.{Id}");
}

/// <summary>Kabin teması: arayüz panellerinin çerçeve ve vurgu renkleri.</summary>
public sealed record CabinTheme(string Id, int Price, Color Edge, Color Accent, Color Label)
{
    /// <summary>Kabin iç tasarımı (Classic: eski ahşap kabin).</summary>
    public CabinInterior Interior { get; init; } = CabinInterior.Classic;

    /// <summary>Bu kabini içeren DLC (null = ana oyun).</summary>
    public Platform.DlcId? Dlc { get; init; }

    public string Name => Loc.T($"theme.{Id}");
}

public static class Cosmetics
{
    public static TrainSkin[] TrainSkins { get; private set; } = new TrainSkin[]
    {
        // Klasik: yıpranmış pas kırmızısı ve demir
        new("classic", 0,
            new(66, 42, 34), new(48, 30, 25), new(38, 42, 50), new(54, 60, 72), new(30, 32, 38),
            new(70, 36, 30), new(24, 24, 30), new(80, 88, 104), new(54, 60, 72)),
        // ---- Özgün modeller: tamamen farklı gövde mimarisi ----

        // Steam-Punk Citadel: pirinç kaplama, üç dev baca, görünen çarklar
        new("citadel", 650,
            new(110, 64, 40), new(84, 48, 30), new(40, 36, 34), new(190, 140, 60), new(120, 84, 36),
            new(90, 50, 34), new(60, 40, 24), new(255, 230, 160), new(200, 150, 70))
        { Model = TrainModel.Citadel },
        // Episode 2: Cyber-Glitch Locomotive
        new("cyberglitch", 0,
            new(24, 20, 36), new(16, 12, 26), new(30, 30, 40), new(32, 26, 48), new(200, 80, 255),
            new(18, 14, 28), new(12, 10, 18), new(80, 230, 255), new(200, 80, 255))
        { Model = TrainModel.CyberGlitch, Dlc = Platform.DlcId.Episode2 },
        // Episode 2: Heavy Panzer Express
        new("panzer", 0,
            new(66, 72, 54), new(50, 56, 40), new(40, 42, 40), new(76, 84, 60), new(50, 56, 40),
            new(70, 76, 58), new(34, 38, 28), new(150, 160, 130), new(230, 190, 70))
        { Model = TrainModel.Panzer, Dlc = Platform.DlcId.Episode2 },

        // ---- Emekliye ayrılan renk skinleri (satılmaz; sahibinde kalır) ----
        new("black", 220,
            new(22, 20, 24), new(14, 12, 16), new(26, 24, 28), new(30, 28, 34), new(150, 26, 30),
            new(28, 24, 28), new(12, 10, 14), new(70, 64, 76), new(170, 30, 34)) { Retired = true },
        new("gold", 400,
            new(40, 46, 80), new(28, 32, 60), new(60, 52, 30), new(190, 150, 60), new(120, 86, 30),
            new(36, 42, 86), new(30, 26, 20), new(250, 220, 140), new(230, 190, 80)) { Retired = true },
        new("glacier", 260,
            new(140, 160, 180), new(110, 130, 150), new(56, 70, 90), new(90, 130, 170), new(46, 70, 100),
            new(70, 100, 140), new(40, 50, 66), new(200, 230, 250), new(160, 220, 255)) { Retired = true },

        // ---- Tam sürüm: başarımlarla açılan skinler ----

        // Buz Kıran Lokomotif: kalın beyaz sac, buz mavisi şerit, dev pulluk
        new("icebreaker", 300,
            new(190, 200, 214), new(150, 162, 180), new(60, 70, 86), new(210, 220, 232), new(90, 110, 140),
            new(170, 182, 200), new(60, 70, 86), new(255, 255, 255), new(80, 170, 230))
        { Style = TrainStyle.Icebreaker, Achievement = "chapter10" },
        // Paslı Haydut Treni: turuncu pas, yamalı plakalar, dikenler
        new("bandit", 250,
            new(110, 58, 30), new(80, 40, 22), new(50, 40, 34), new(130, 70, 36), new(70, 44, 30),
            new(96, 50, 28), new(40, 30, 26), new(170, 110, 70), new(200, 60, 40))
        { Style = TrainStyle.Bandit, Achievement = "escape5" },
        // Krallık Ekspresi: kraliyet moru, altın süslemeler
        new("royal", 500,
            new(70, 30, 90), new(50, 20, 66), new(70, 60, 30), new(110, 50, 140), new(200, 160, 60),
            new(84, 36, 104), new(36, 20, 44), new(240, 210, 255), new(240, 200, 80))
        { Style = TrainStyle.Royal, Achievement = "delivered" },
        // Kurt Avcısı: koyu orman yeşili, kemik beyazı dişler
        new("wolfhunter", 280,
            new(44, 58, 44), new(32, 44, 32), new(40, 40, 40), new(60, 70, 60), new(120, 110, 96),
            new(52, 66, 50), new(26, 30, 26), new(140, 150, 130), new(220, 210, 190))
        { Style = TrainStyle.WolfHunter, Achievement = "wolves5" },
        // Madenci Lokomotifi: kömür karası, sarı-siyah uyarı şeridi, alın lambası
        new("miner", 280,
            new(46, 42, 40), new(34, 30, 28), new(30, 30, 32), new(56, 52, 50), new(30, 28, 26),
            new(60, 54, 46), new(22, 20, 20), new(120, 110, 90), new(240, 190, 50))
        { Style = TrainStyle.Miner, Achievement = "ore10" },
        // Kutup Işığı: gece laciverti, yeşil-mor parıltı
        new("aurora", 450,
            new(20, 30, 60), new(14, 22, 46), new(30, 36, 56), new(30, 50, 90), new(80, 230, 170),
            new(26, 40, 76), new(12, 16, 30), new(160, 255, 220), new(170, 110, 255))
        { Style = TrainStyle.Aurora, Achievement = "endless1000" },

        // ---- DLC skinleri (DLC sahibine ücretsiz) ----

        // Supporter Pack: Altın Kaplama Lokomotif
        new("goldplated", 0,
            new(150, 112, 40), new(120, 88, 30), new(110, 84, 30), new(220, 175, 70), new(160, 120, 40),
            new(190, 150, 60), new(90, 66, 24), new(255, 240, 170), new(255, 220, 110))
        { Style = TrainStyle.GoldPlated, Dlc = Platform.DlcId.Supporter },
        // Buzul Kıyameti: Haydut Akıncı Treni ve Askeri Tren
        new("raider", 0,
            new(70, 60, 50), new(50, 42, 36), new(46, 40, 36), new(84, 70, 58), new(140, 40, 30),
            new(64, 54, 46), new(30, 26, 24), new(150, 130, 110), new(200, 50, 36))
        { Style = TrainStyle.Bandit, Dlc = Platform.DlcId.FrozenWasteland },
        new("military", 0,
            new(70, 80, 54), new(54, 62, 42), new(40, 44, 36), new(84, 96, 64), new(50, 56, 40),
            new(74, 84, 58), new(34, 38, 28), new(150, 160, 120), new(220, 220, 200))
        { Style = TrainStyle.Military, Dlc = Platform.DlcId.FrozenWasteland },
        // Sığınak Kuryeleri: 4 tren rengi
        new("neon", 0,
            new(20, 26, 40), new(14, 18, 30), new(30, 36, 50), new(28, 40, 64), new(40, 220, 255),
            new(24, 30, 50), new(10, 12, 20), new(140, 240, 255), new(60, 240, 255))
        { Dlc = Platform.DlcId.Couriers, Retired = true },
        new("midnight", 0,
            new(30, 22, 44), new(22, 16, 34), new(26, 22, 34), new(44, 30, 64), new(150, 90, 220),
            new(36, 26, 54), new(14, 10, 22), new(180, 150, 230), new(200, 120, 255))
        { Dlc = Platform.DlcId.Couriers, Retired = true },
        new("ember", 0,
            new(90, 30, 20), new(66, 22, 16), new(44, 30, 26), new(140, 46, 24), new(255, 140, 40),
            new(110, 36, 22), new(40, 18, 14), new(255, 190, 120), new(255, 110, 40))
        { Dlc = Platform.DlcId.Couriers, Retired = true },
        new("arctic", 0,
            new(214, 222, 232), new(180, 190, 204), new(90, 100, 116), new(230, 236, 244), new(120, 180, 220),
            new(200, 210, 224), new(70, 80, 96), new(255, 255, 255), new(100, 190, 240))
        { Dlc = Platform.DlcId.Couriers, Retired = true },
    };

    public static readonly CabinTheme[] CabinThemes =
    {
        new("classic", 0, new(70, 80, 105), new(235, 175, 90), new(150, 160, 185)),
        new("crimson", 100, new(120, 50, 56), new(255, 110, 90), new(190, 150, 155)),
        new("frost", 120, new(80, 130, 160), new(150, 220, 255), new(160, 190, 210)),
        new("emerald", 150, new(50, 110, 80), new(120, 230, 150), new(150, 190, 165)),
        // Tam sürüm renk paletleri
        new("royal", 180, new(96, 60, 130), new(240, 200, 80), new(180, 160, 205)),
        new("ash", 140, new(90, 88, 86), new(255, 140, 60), new(175, 170, 165)),
        // Kabin iç tasarımları: panellerin renginin yanında kabinin içi de tamamen değişir
        new("vintage", 220, new(120, 84, 50), new(240, 190, 110), new(200, 170, 130)) { Interior = CabinInterior.Vintage },
        new("command", 320, new(70, 90, 60), new(230, 200, 80), new(170, 185, 150)) { Interior = CabinInterior.Command },
        new("hologram", 0, new(70, 50, 120), new(120, 230, 255), new(180, 170, 220)) { Interior = CabinInterior.Hologram, Dlc = Platform.DlcId.Episode2 },
    };

    /// <summary>
    /// Skin ekranında listelenen trenler: emekli renk skinleri yalnızca sahibine görünür.
    /// (Kayıttaki kimlikler bozulmaz; Skin(id) bütün listede arar.)
    /// </summary>
    public static TrainSkin[] ShopSkins
    {
        get
        {
            // Menü her karede okur: sahip olunanlar değişmedikçe önbellekten döner
            var owned = SaveData.Current.OwnedSkins;
            if (_shopSkins == null || _shopOwned != owned.Count || _shopTotal != TrainSkins.Length)
            {
                _shopSkins = System.Array.FindAll(TrainSkins, s => !s.Retired || owned.Contains(s.Id));
                _shopOwned = owned.Count;
                _shopTotal = TrainSkins.Length;
            }
            return _shopSkins;
        }
    }

    private static TrainSkin[] _shopSkins;
    private static int _shopOwned = -1, _shopTotal = -1;

    /// <summary>Skin listesinin 2. sütunda kaydırmalı gösterimi için toplam sayı.</summary>
    public static int SkinCount => TrainSkins.Length;

    /// <summary>Modlardan gelen skinleri listenin sonuna ekler (kimlikleri "mod_" ile başlar).</summary>
    public static void AddModSkins(System.Collections.Generic.IReadOnlyList<TrainSkin> skins)
    {
        if (skins.Count == 0) return;
        var all = new TrainSkin[TrainSkins.Length + skins.Count];
        TrainSkins.CopyTo(all, 0);
        for (int i = 0; i < skins.Count; i++) all[TrainSkins.Length + i] = skins[i];
        TrainSkins = all;
    }

    public static TrainSkin Skin(string id)
    {
        foreach (var skin in TrainSkins) if (skin.Id == id) return skin;
        return TrainSkins[0];
    }

    public static CabinTheme Theme(string id)
    {
        foreach (var theme in CabinThemes) if (theme.Id == id) return theme;
        return CabinThemes[0];
    }
}
