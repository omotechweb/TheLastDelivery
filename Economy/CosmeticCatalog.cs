using System;
using Microsoft.Xna.Framework;
using TheLastDelivery.Localization;
using TheLastDelivery.Platform;

namespace TheLastDelivery.Economy;

/// <summary>Kozmetik yuvaları: tren dumanı ve kabin içi süsler.</summary>
public enum CosmeticSlot
{
    Smoke,    // Bacadan çıkan dumanın rengi ve davranışı
    Painting, // Kabin duvarına asılan tablo
    Radio,    // Kabindeki radyo (kabin menüsü açıkken çalan müziğin sesi)
    Costume,  // Arthur'un kostümü (kasaba ve portre)
    MusicBox, // Kabindeki müzik kutusunun şarkı paketi ([M] ile çalınır)
    Pet       // Kabindeki evcil hayvan: akıl/moral yeniler, hainlere ve hırsızlara havlar
}

/// <summary>Evcil hayvan türü (CosmeticItem.Value).</summary>
public enum PetKind { None, Husky, Cat }

/// <summary>Duman efektinin görünümü.</summary>
public enum SmokeStyle { Classic, NeonBlue, MilitaryBlack, RedFlame, AuroraMist, FlameSteam, VioletGala }

/// <summary>
/// Tek bir kozmetik. Renkler çizimde kullanılır; Value yuvaya özgü seçimi taşır
/// (duman: SmokeStyle, radyo: ses modeli 0-2, tablo ve kostüm: görsel varyant).
/// </summary>
public sealed record CosmeticItem(CosmeticSlot Slot, string Id, DlcId? Dlc, int Value, Color Primary, Color Secondary)
{
    /// <summary>Doğrudan mikro ödemeyle satın alınabiliyorsa Steam ürün kimliği (ör. "MTX_SMOKE_FLAME").</summary>
    public string MicroId { get; init; }

    /// <summary>Oyun içi görevle de açılabiliyorsa görev kimliği (QuestCatalog).</summary>
    public string Quest { get; init; }

    /// <summary>DLC'ye değil, mikro ödemeye ya da göreve bağlı tekil içerik mi?</summary>
    public bool IsMicro => MicroId != null || Quest != null;
    /// <summary>Kayıtta tutulan benzersiz anahtar, örn. "smoke.neonBlue".</summary>
    public string Key => $"{Slot.ToString().ToLowerInvariant()}.{Id}";
    public string Name => Loc.T($"cos.{Key}");
}

/// <summary>Kozmetik kataloğu. Her yuvanın ilk öğesi ücretsiz varsayılandır.</summary>
public static class CosmeticCatalog
{
    public static readonly CosmeticItem[] Smokes =
    {
        new(CosmeticSlot.Smoke, "classic", null, (int)SmokeStyle.Classic, new(78, 80, 90), new(225, 230, 238)),
        new(CosmeticSlot.Smoke, "neonBlue", DlcId.Couriers, (int)SmokeStyle.NeonBlue, new(60, 200, 255), new(170, 240, 255)),
        new(CosmeticSlot.Smoke, "military", DlcId.Couriers, (int)SmokeStyle.MilitaryBlack, new(24, 24, 26), new(70, 72, 60)),
        new(CosmeticSlot.Smoke, "redFlame", DlcId.Couriers, (int)SmokeStyle.RedFlame, new(255, 90, 40), new(255, 200, 80)),
        new(CosmeticSlot.Smoke, "auroraMist", DlcId.Couriers, (int)SmokeStyle.AuroraMist, new(90, 240, 170), new(170, 110, 255)),
        // Mikro içerikler
        new(CosmeticSlot.Smoke, "flameSteam", null, (int)SmokeStyle.FlameSteam, new(255, 140, 40), new(255, 230, 150)) { MicroId = "MTX_SMOKE_FLAME" },
        new(CosmeticSlot.Smoke, "violetGala", null, (int)SmokeStyle.VioletGala, new(170, 80, 230), new(255, 200, 255)) { MicroId = "MTX_SMOKE_VIOLET", Quest = "sos3" },
    };

    public static readonly CosmeticItem[] Paintings =
    {
        new(CosmeticSlot.Painting, "none", null, 0, Color.Transparent, Color.Transparent),
        new(CosmeticSlot.Painting, "mountains", DlcId.Couriers, 1, new(90, 120, 160), new(230, 236, 245)),
        new(CosmeticSlot.Painting, "oldCity", DlcId.Couriers, 2, new(230, 170, 90), new(60, 50, 70)),
        new(CosmeticSlot.Painting, "family", DlcId.Couriers, 3, new(170, 130, 100), new(90, 60, 50)),
    };

    public static readonly CosmeticItem[] Radios =
    {
        new(CosmeticSlot.Radio, "standard", null, 0, new(110, 80, 54), new(230, 200, 140)),
        new(CosmeticSlot.Radio, "field", DlcId.Couriers, 1, new(80, 92, 60), new(150, 220, 120)),
        new(CosmeticSlot.Radio, "gramophone", DlcId.Couriers, 2, new(150, 110, 50), new(230, 190, 90)),
    };

    public static readonly CosmeticItem[] Costumes =
    {
        new(CosmeticSlot.Costume, "classic", null, 0, new(58, 46, 38), new(150, 42, 40)),
        new(CosmeticSlot.Costume, "officer", DlcId.Couriers, 1, new(44, 56, 46), new(200, 170, 70)),
        new(CosmeticSlot.Costume, "polar", DlcId.Couriers, 2, new(200, 206, 214), new(60, 120, 190)),
        // Mikro içerikler: şapka ve kıyafetler
        new(CosmeticSlot.Costume, "trapper", null, 3, new(110, 80, 50), new(200, 170, 120)) { MicroId = "MTX_COSTUME_TRAPPER" },
        new(CosmeticSlot.Costume, "stationmaster", null, 4, new(30, 40, 70), new(220, 180, 70)) { MicroId = "MTX_COSTUME_STATIONMASTER", Quest = "hero" },
    };

    /// <summary>Müzik kutusu şarkı paketleri (Value = ProceduralAudio şarkı sırası).</summary>
    public static readonly CosmeticItem[] MusicBoxes =
    {
        new(CosmeticSlot.MusicBox, "lullaby", null, 0, new(230, 200, 140), new(150, 110, 70)),
        new(CosmeticSlot.MusicBox, "waltz", null, 1, new(180, 210, 255), new(90, 110, 170)) { MicroId = "MTX_SONG_WALTZ" },
        new(CosmeticSlot.MusicBox, "anthem", null, 2, new(255, 210, 110), new(170, 60, 50)) { MicroId = "MTX_SONG_ANTHEM", Quest = "endings2" },
    };

    /// <summary>Evcil hayvanlar: kutup köpeği mağazadan (ya da görev biletiyle), kedi Supporter Pack ile.</summary>
    public static readonly CosmeticItem[] Pets =
    {
        new(CosmeticSlot.Pet, "none", null, (int)PetKind.None, Color.Transparent, Color.Transparent),
        new(CosmeticSlot.Pet, "husky", null, (int)PetKind.Husky, new(200, 206, 216), new(70, 76, 90)) { MicroId = "MTX_PET_HUSKY" },
        new(CosmeticSlot.Pet, "cat", DlcId.Supporter, (int)PetKind.Cat, new(230, 150, 70), new(255, 230, 200)),
    };

    public static readonly CosmeticItem[] AllItems = Concat(Smokes, Paintings, Radios, Costumes, MusicBoxes, Pets);

    /// <summary>Kozmetik mağazasındaki tekil (DLC dışı) içerikler.</summary>
    public static readonly CosmeticItem[] MicroItems = Array.FindAll(AllItems, i => i.IsMicro);

    /// <summary>Oyuncu bu öğeye sahip mi? DLC → sahiplik; mikro/görev → kayıtlı envanter; varsayılanlar herkese açık.</summary>
    public static bool IsOwned(CosmeticItem item)
    {
        if (item == null) return false;
        if (item.Dlc != null) return DLCManager.Owns(item.Dlc);
        if (item.IsMicro) return SaveData.Current.OwnedCosmetics.Contains(item.Key);
        return true;
    }

    public static CosmeticItem[] For(CosmeticSlot slot) => slot switch
    {
        CosmeticSlot.Smoke => Smokes,
        CosmeticSlot.Painting => Paintings,
        CosmeticSlot.Radio => Radios,
        CosmeticSlot.Costume => Costumes,
        CosmeticSlot.Pet => Pets,
        _ => MusicBoxes
    };

    public static CosmeticItem Default(CosmeticSlot slot) => For(slot)[0];

    public static CosmeticItem Find(CosmeticSlot slot, string id) => Array.Find(For(slot), i => i.Id == id);

    private static CosmeticItem[] Concat(params CosmeticItem[][] groups)
    {
        int total = 0;
        foreach (var g in groups) total += g.Length;
        var all = new CosmeticItem[total];
        int index = 0;
        foreach (var g in groups)
        {
            g.CopyTo(all, index);
            index += g.Length;
        }
        return all;
    }
}
