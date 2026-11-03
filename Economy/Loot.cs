using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheLastDelivery.Graphics;
using TheLastDelivery.Localization;

namespace TheLastDelivery.Economy;

/// <summary>Yolda toplanan, tüccara satılan yağma türleri (kullanılabilir eşyalardan ayrıdır).</summary>
public enum LootType
{
    ScrapMetal,   // Hurda Metal: temel, çok sık
    OldMedicine,  // Eski Dünya Tıbbi Malzemesi
    Radiator,     // Termal Radyatör Parçası: kıymetli
    MusicBox,     // Antika Müzik Kutusu: nadir
    GoldNugget,   // Altın Parçası: yalnızca madenlerde
    RareOre       // Nadir Cevher: madenlerin en derin damarlarında
}

/// <summary>Yağma eşyalarının adı, değeri, nadirliği, rengi ve ikonu.</summary>
public static class LootCatalog
{
    /// <summary>Değere göre artan sırada (mafya en sondan, yani en değerliden başlayarak çalar).</summary>
    public static readonly LootType[] All =
    {
        LootType.ScrapMetal, LootType.OldMedicine, LootType.Radiator, LootType.GoldNugget, LootType.MusicBox, LootType.RareOre
    };

    public static string Name(LootType type) => Loc.T(type switch
    {
        LootType.ScrapMetal => "loot.scrap",
        LootType.OldMedicine => "loot.medicine",
        LootType.Radiator => "loot.radiator",
        LootType.GoldNugget => "loot.gold",
        LootType.RareOre => "loot.ore",
        _ => "loot.musicbox"
    });

    /// <summary>Tüccarın ödediği Kredi.</summary>
    public static int Price(LootType type) => type switch
    {
        LootType.ScrapMetal => 5,
        LootType.OldMedicine => 15,
        LootType.Radiator => 25,
        LootType.GoldNugget => 35,
        LootType.RareOre => 60,
        _ => 45
    };

    /// <summary>Rastgele düşme ağırlığı: hurda çok sık, müzik kutusu nadir. Cevherler yalnızca madenden çıkar.</summary>
    private static int Weight(LootType type) => type switch
    {
        LootType.ScrapMetal => 55,
        LootType.OldMedicine => 22,
        LootType.Radiator => 16,
        LootType.GoldNugget or LootType.RareOre => 0,
        _ => 7
    };

    public static Color Color(LootType type) => type switch
    {
        LootType.ScrapMetal => new Color(160, 150, 140),
        LootType.OldMedicine => new Color(120, 200, 190),
        LootType.Radiator => new Color(255, 150, 70),
        LootType.GoldNugget => new Color(255, 205, 60),
        LootType.RareOre => new Color(190, 120, 255),
        _ => new Color(240, 200, 90)
    };

    /// <summary>Nadirlik etiketi (renk ile birlikte ticaret ekranında gösterilir).</summary>
    public static string RarityKey(LootType type) => type switch
    {
        LootType.ScrapMetal => "rarity.common",
        LootType.OldMedicine => "rarity.uncommon",
        LootType.Radiator or LootType.GoldNugget => "rarity.valuable",
        _ => "rarity.rare"
    };

    public static LootType Roll(Random rng)
    {
        int total = All.Sum(Weight);
        int roll = rng.Next(total);
        foreach (var type in All)
        {
            roll -= Weight(type);
            if (roll < 0) return type;
        }
        return LootType.ScrapMetal;
    }

    /// <summary>Küçük piksel ikonları.</summary>
    public static void DrawIcon(SpriteBatch sb, LootType type, Point c)
    {
        void Box(int x, int y, int w, int h, Color col) => sb.FillRect(new Rectangle(c.X + x, c.Y + y, w, h), col);

        switch (type)
        {
            case LootType.ScrapMetal:
                Box(-10, 2, 20, 7, new Color(120, 84, 60));
                Box(-7, -4, 15, 7, new Color(150, 156, 170));
                Box(-9, -9, 9, 6, new Color(170, 110, 70));
                Box(3, -10, 5, 5, new Color(200, 205, 215));
                break;
            case LootType.OldMedicine:
                Box(-6, -11, 12, 4, new Color(200, 205, 215));               // Kapak
                Box(-8, -7, 16, 18, new Color(110, 180, 170));              // Cam şişe
                Box(-5, -2, 10, 8, new Color(235, 235, 220));               // Etiket
                Box(-1, -1, 2, 6, new Color(210, 50, 50));
                Box(-3, 1, 6, 2, new Color(210, 50, 50));
                break;
            case LootType.Radiator:
                Box(-12, -8, 24, 18, new Color(150, 80, 40));
                for (int x = -10; x < 12; x += 4)
                    Box(x, -6, 2, 14, new Color(255, 160, 80));             // Kanatçıklar
                Box(-14, -2, 3, 4, new Color(110, 110, 120));               // Boru bağlantısı
                Box(11, -2, 3, 4, new Color(110, 110, 120));
                break;
            case LootType.GoldNugget:
                Box(-9, -5, 18, 12, new Color(200, 150, 30));
                Box(-6, -9, 12, 6, new Color(255, 205, 60));
                Box(-4, -7, 5, 3, new Color(255, 245, 190));                // Parıltı
                Box(5, 1, 3, 3, new Color(255, 235, 140));
                break;
            case LootType.RareOre:
                sb.DrawGlow(c.ToVector2(), 20f, new Color(190, 120, 255), 0.4f);
                Box(-10, -2, 20, 12, new Color(70, 60, 74));                 // Kaya
                Box(-6, -11, 6, 12, new Color(170, 100, 250));               // Kristaller
                Box(1, -8, 5, 10, new Color(210, 160, 255));
                Box(-2, -13, 3, 4, new Color(240, 220, 255));
                break;
            default:
                Box(-11, -2, 22, 13, new Color(120, 70, 40));               // Ahşap kutu
                Box(-11, -8, 22, 6, new Color(150, 90, 50));                // Açık kapak
                Box(-9, 1, 18, 2, new Color(240, 200, 90));                 // Altın şerit
                Box(-2, -12, 4, 4, new Color(240, 200, 90));                // Balerin
                Box(-1, -15, 2, 3, new Color(250, 230, 200));
                break;
        }
    }
}

/// <summary>
/// Kargo vagonu: toplanan yağmalar burada taşınır. Her eşya 1 birim yer kaplar;
/// kapasite dolunca yeni yağma alınamaz (Kargo Vagonu geliştirmesi kapasiteyi artırır).
/// </summary>
public sealed class Cargo
{
    private readonly Dictionary<LootType, int> _counts = new();

    public int Capacity { get; set; } = 10;

    public int Total => _counts.Values.Sum();
    public bool IsFull => Total >= Capacity;

    public int Count(LootType type) => _counts.TryGetValue(type, out int n) ? n : 0;

    public void Clear() => _counts.Clear();

    public bool TryAdd(LootType type)
    {
        if (IsFull) return false;
        _counts[type] = Count(type) + 1;
        return true;
    }

    public bool TryRemove(LootType type)
    {
        int n = Count(type);
        if (n <= 0) return false;
        _counts[type] = n - 1;
        return true;
    }

    /// <summary>Toplam satış değeri.</summary>
    public int Value => LootCatalog.All.Sum(t => Count(t) * LootCatalog.Price(t));

    /// <summary>Mafya yağması: en değerliden başlayarak en fazla <paramref name="amount"/> eşya alır.</summary>
    public List<LootType> Steal(int amount)
    {
        var stolen = new List<LootType>();
        foreach (var type in LootCatalog.All.Reverse())
        {
            while (stolen.Count < amount && TryRemove(type))
                stolen.Add(type);
        }
        return stolen;
    }
}
