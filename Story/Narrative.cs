using System;
using System.Collections.Generic;
using TheLastDelivery.Economy;
using TheLastDelivery.Items;
using TheLastDelivery.Localization;
using TheLastDelivery.UI;

namespace TheLastDelivery.Story;

/// <summary>
/// Yürünebilen yerleşimlerin mimari şablonu. İlk altısı yol üstündeki kasabalar, diğerleri
/// "yalnızca hikâye" bölümlerinin keşif/diyalog sahneleri.
/// </summary>
public enum TownArchetype
{
    TradeHub,          // Ticaret Merkezi: büyük pazar, zengin tüccarlar
    MiningVillage,     // Madenci Köyü: işçiler, grev
    GhostStation,      // Hayalet İstasyon: fısıltılar, eski ses kayıtları
    IceCult,           // Buzul Tarikatı: ayinler, kehanetler
    AbandonedOutpost,  // Terk Edilmiş Karakol: yalnızca yağma
    MilitaryGate,      // Askeri Sınır Kapısı: kimlik kontrolü
    // Hikâye bölümleri
    Headquarters,      // Bölüm 10: Sessiz Karargah
    Homestead,         // Bölüm 17: Arthur'un eski evi
    BorderControl,     // Bölüm 25: Sınır Kapısı (sorgu)
    Institute,         // Bölüm 31: AURORA Enstitüsü
    RadioMast,         // Bölüm 37: Kuzey Yıldızı
    ShelterGate,       // Bölüm 39: Konsey'in elçisi

    // ---- Episode 2 "Project AURA" ----
    DroneOutpost,      // Kasaba: AURA'nın insansız askeri karakolu (yalnızca makineler)
    HibernationFactory,// Kasaba: insanların kış uykusu tüplerinde tutulduğu dev fabrika
    CyberChapel,       // Kasaba: NULL tarikatının krom şapeli
    SignalTower,       // E2 Bölüm 2: AURA'nın ilk frekans kulesi ve telsiz korsanı
    RelayStation,      // E2 Bölüm 10: Röle İstasyonu Sigma (AURA'nın doğuşu)
    SleepFactory,      // E2 Bölüm 15: Uyku Fabrikası Novo-7'nin içi
    NullChapel,        // E2 Bölüm 20: NULL'un Kâhini ile yüzleşme (diyalog)
    PolarBase,         // E2 Bölüm 31: Kutup Askeri Yeraltı Üssü'nün girişi
    ServerHall,        // E2 Bölüm 34: Sunucu salonu, AURA ile ilk konuşma (diyalog)
    MirrorRoom,        // E2 Bölüm 37: AURA'nın zihin oyunları (diyalog)
    AuraCore           // E2 Bölüm 40: AURA'nın ana çekirdeği, üç son (diyalog)
}

/// <summary>Bir seçimin oyuna etkisi. Tüm alanlar isteğe bağlıdır.</summary>
public sealed record NarrativeEffect
{
    public int Karma { get; init; }
    public int Credits { get; init; }          // + kazanç, - harcama (harcanamazsa seçenek kilitli)
    public float Fuel { get; init; }
    public float Repair { get; init; }
    public float Damage { get; init; }
    public float Heat { get; init; }
    public float Sanity { get; init; }         // + toparlar, - aşındırır
    public float Morale { get; init; }         // Tüm yolcuların morali
    public float QuestMorale { get; init; }    // Yalnızca konuşulan yolcunun morali
    public float Suspicion { get; init; }      // Konuşulan yolcuya şüphe (+) ya da güven (-)
    public int Rations { get; init; }          // + kazanç, - paylaşım
    public ItemType[] Items { get; init; }
    public ItemType? ConsumeItem { get; init; } // Seçenek bu eşyayı harcar (yoksa kilitli)
    public int Loot { get; init; }             // Kargoya yağma
    public string SetFlag { get; init; }
    public string SetFlag2 { get; init; }
    public bool RecruitTownPassenger { get; init; } // Kasabanın yolcu adayı trene katılır
    public string RecruitId { get; init; }                 // Bu kimlikteki yolcu trene katılır (Episode 2 hikâye sahneleri)
    public bool CompleteScene { get; init; }        // Diyalog sahnesi (hikâye bölümü) biter
    public string HackSuccess { get; init; }        // Diyalog kapanınca AURA hack'i açılır; başarıda bu düğüm gelir
    public string HackFlag { get; init; }           // Hack başarılıysa konan bayrak
    public string HackRetry { get; init; }          // Hack başarısızsa yeniden açılan düğüm
}

/// <summary>Düğümdeki bir seçenek. Etiket: "nv.{düğüm}.o{sıra}".</summary>
public sealed record NarrativeOption(
    string Next = null,
    NarrativeEffect Effect = null,
    string RequireFlag = null,
    string ForbidFlag = null)
{
    public string LabelKey { get; init; }
}

/// <summary>
/// Diyalog/inceleme düğümü. Metin: "nv.{Id}". Seçeneği yoksa [BOŞLUK] ile Next'e geçilir (Next yoksa kapanır).
/// Konuşan: SpeakerKey çeviri anahtarı (null: anlatıcı, başlık sahnenin adı).
/// </summary>
public sealed record NarrativeNode(string Id, string SpeakerKey, PortraitKind Portrait, params NarrativeOption[] Options)
{
    public string Next { get; init; }
    public NarrativeEffect OnEnter { get; init; }     // Düğüm açıldığında bir kez uygulanır
    public string Title { get; init; }                 // Pencere başlığı anahtarı (yoksa konuşanın adı)
    public string Text => Loc.T($"nv.{Id}");
    public string OptionLabel(int index) => Loc.T(Options[index].LabelKey ?? $"nv.{Id}.o{index + 1}");
}

/// <summary>
/// Kasabadaki inceleme noktası: her etkileşimde sıradaki (görülmemiş) düğüm açılır, replikler tekrar etmez.
/// Hepsi görüldüğünde "Rest" düğümü (yoksa son düğüm) gösterilir.
/// </summary>
public sealed record SpotDef(string Id, bool Talk, params string[] Entries)
{
    public string Rest { get; init; }
    public string Name => Loc.T($"spot.{Id}");
}

/// <summary>
/// Hikâye durumu: seçim bayrakları, görülen düğümler ve yolcu görev zincirleri.
/// SaveData.Story içinde kalıcıdır; Bölüm 1'den yeni bir hikâye başlarken sıfırlanır.
/// </summary>
public sealed class StoryState
{
    public Dictionary<string, int> Flags { get; set; } = new();
    public List<string> Seen { get; set; } = new();
    public Dictionary<string, int> Quests { get; set; } = new();   // Yolcu kimliği → tamamlanan adım sayısı
    public List<string> Lore { get; set; } = new();                // Açığa çıkan sırlar (günlük için)
}

/// <summary>
/// Episode 1'den Episode 2'ye aktarılanlar: gizli karma, hayatta kalan yolcular, verilen kararlar ve görülen son.
/// Episode 1 her bittiğinde (Sığınak-0 finali) yeniden yazılır.
/// </summary>
public sealed class EpisodeCarry
{
    public int Karma { get; set; }
    public List<string> Survivors { get; set; } = new();   // Yolcu kimlikleri ("viktor", "nadia"...)
    public List<string> Flags { get; set; } = new();       // Episode 1 hikâye bayrakları
    public string Ending { get; set; }                     // "hero", "thaw"...
}

/// <summary>
/// Hikâye durumuna kısa yollar. Episode 1 kararları SaveData.Story, Episode 2 kararları SaveData.Story2'de tutulur.
/// "ep1:" önekli bayraklar Episode 2'den Episode 1'in aktarılan kararlarını okur (örn. "ep1:sofi.trust").
/// Yolcu görev zincirleri her iki bölümde de ortaktır (Episode 1'den gelen yolcu hikâyesine kaldığı yerden devam eder).
/// </summary>
public static class Narrative
{
    private static int _episode = 1;
    private static StoryState State => _episode == 2 ? SaveData.Current.Story2 : SaveData.Current.Story;
    private static HashSet<string> _seen;

    /// <summary>Etkin bölüm (1 ya da 2). Game1 koşu başlarken ayarlar.</summary>
    public static int Episode
    {
        get => _episode;
        set
        {
            if (_episode == value) return;
            _episode = value;
            _seen = null;
        }
    }

    /// <summary>"aboard:{id}" koşulları için: o yolcu şu an trende mi? (Game1 ayarlar.)</summary>
    public static Func<string, bool> AboardCheck { get; set; }

    /// <summary>
    /// Bayrak koşulu. Ek biçimler: "a|b" (herhangi biri), "aboard:viktor" (yolcu trende),
    /// "seen:hq.diary.3" (düğüm okundu), "ep1:..." (Episode 1'den aktarılan karar),
    /// "a&amp;b" (hepsi; "|" ondan önce ayrılır), "count:council.w.:2" (bu önekle en az 2 bayrak).
    /// </summary>
    public static bool Has(string flag)
    {
        if (flag == null) return false;
        if (flag.Contains('|'))
        {
            foreach (string part in flag.Split('|'))
                if (Has(part)) return true;
            return false;
        }
        if (flag.Contains('&'))
        {
            foreach (string part in flag.Split('&'))
                if (!Has(part)) return false;
            return true;
        }
        if (flag.StartsWith("count:"))
        {
            int colon = flag.LastIndexOf(':');
            if (colon <= 6 || !int.TryParse(flag[(colon + 1)..], out int needed)) return false;
            return CountFlags(flag[6..colon]) >= needed;
        }
        if (flag.StartsWith("aboard:")) return AboardCheck?.Invoke(flag[7..]) == true;
        if (flag.StartsWith("seen:")) return Seen(flag[5..]);
        if (flag.StartsWith("ep1:"))
            return SaveData.Current.Carry?.Flags.Contains(flag[4..]) == true;
        return State.Flags.ContainsKey(flag);
    }

    /// <summary>Etkin bölümde bu önekle başlayan bayrak sayısı (ör. Konsey önünde tanıklık eden yolcular).</summary>
    public static int CountFlags(string prefix)
    {
        int count = 0;
        foreach (string key in State.Flags.Keys)
            if (key.StartsWith(prefix, StringComparison.Ordinal)) count++;
        return count;
    }

    public static void Set(string flag)
    {
        if (string.IsNullOrEmpty(flag)) return;
        State.Flags[flag] = 1;
    }

    public static bool Seen(string node)
    {
        _seen ??= new HashSet<string>(State.Seen);
        return _seen.Contains(node);
    }

    /// <summary>Düğümü yeniden "görülmemiş" yapar (başarısız hack'ten sonra terminal tekrar denenebilir).</summary>
    public static void Unsee(string node)
    {
        if (!Seen(node)) return;
        _seen.Remove(node);
        State.Seen.Remove(node);
    }

    public static void MarkSeen(string node)
    {
        if (Seen(node)) return;
        _seen.Add(node);
        State.Seen.Add(node);
    }

    private static Dictionary<string, int> Quests => SaveData.Current.Story.Quests;

    public static int QuestStep(string passengerId) => Quests.TryGetValue(passengerId, out int s) ? s : 0;

    public static void AdvanceQuest(string passengerId) => Quests[passengerId] = QuestStep(passengerId) + 1;

    /// <summary>Etkin bölümde yeni hikâye: o bölümün bütün seçimleri ve görülen replikleri silinir.</summary>
    public static void ResetForNewStory()
    {
        if (_episode == 2)
        {
            SaveData.Current.Story2 = new StoryState();
        }
        else
        {
            var quests = SaveData.Current.Story.Quests;
            SaveData.Current.Story = new StoryState();
            if (SaveData.Current.Episode1Complete) SaveData.Current.Story.Quests = quests; // Bitirilmiş hikâyede görevler korunur
        }
        _seen = null;
        SaveData.Save();
    }

    /// <summary>Kayıt yeniden yüklendiğinde önbellek tazelenir.</summary>
    public static void Invalidate() => _seen = null;

    /// <summary>İnceleme noktasında sıradaki düğüm: ilk görülmemiş giriş, hepsi görüldüyse dinlenme düğümü.</summary>
    public static string NextEntry(SpotDef spot)
    {
        foreach (string entry in spot.Entries)
            if (!Seen(entry)) return entry;
        return spot.Rest ?? spot.Entries[^1];
    }
}
