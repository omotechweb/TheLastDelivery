using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using TheLastDelivery.Platform;

namespace TheLastDelivery.Economy;

/// <summary>
/// Kalıcı oyuncu profili: Kredi, açılan bölümler, satın alınan geliştirmeler, skinler ve temalar.
/// %LocalAppData%\TheLastDelivery\save.json dosyasına JSON olarak yazılır.
/// Dosya okunamaz/yazılamazsa oyun varsayılan profille sorunsuz devam eder.
/// </summary>
public sealed class SaveData
{
    public int Credits { get; set; }
    public int UnlockedChapter { get; set; } = 1;
    public List<string> Upgrades { get; set; } = new();
    public List<string> OwnedSkins { get; set; } = new() { "classic" };
    public List<string> OwnedThemes { get; set; } = new() { "classic" };
    public string EquippedSkin { get; set; } = "classic";
    public string EquippedTheme { get; set; } = "classic";

    // --- Tam sürüm: rekorlar, istatistikler, başarımlar ---
    public int EndlessBestKm { get; set; }
    public float SpeedrunBestSeconds { get; set; }   // 0 = henüz tamamlanmadı
    public bool Delivered { get; set; }              // Hikâye modunda Sığınak-0'a teslimat yapıldı
    public int AmbushesEscaped { get; set; }
    public int WolvesRepelled { get; set; }
    public int TraitorsCaught { get; set; }
    public int OresMined { get; set; }
    public List<string> Achievements { get; set; } = new();

    // --- Atmosfer güncellemesi: yeni başarım sayaçları ve sonsuz mod liderlik tablosu ---
    public int TimesSabotaged { get; set; }          // Bir hain tarafından sabote edilme sayısı
    public int PeakCredits { get; set; }             // Aynı anda biriktirilen en yüksek Kredi
    public int SpeedrunWins { get; set; }
    public int MaxPassengersAtOnce { get; set; }
    public int LampsFixed { get; set; }
    public int PassengersFed { get; set; }
    public int BlizzardsSurvived { get; set; }
    public List<LeaderboardEntry> EndlessLeaderboard { get; set; } = new();

    // --- DLC ve kozmetikler ---
    public List<string> OwnedDlc { get; set; } = new();          // Son doğrulanan DLC sahiplikleri ("supporter", ...)
    public List<string> OwnedCosmetics { get; set; } = new();    // DLC'lerden gelen kozmetikler ("smoke.neonBlue", ...)
    public bool ShowSupporterBadge { get; set; } = true;
    public string EquippedSmoke { get; set; } = "classic";
    public string EquippedPainting { get; set; } = "none";
    public string EquippedRadio { get; set; } = "standard";
    public string EquippedCostume { get; set; } = "classic";
    public string EquippedMusicBox { get; set; } = "lullaby";
    public string EquippedPet { get; set; } = "none";

    // --- Canlı içerik: günlük/haftalık görevler, kozmetik biletleri, garaj, fotoğraflar ---
    public string ContractDay { get; set; }
    public string ContractWeek { get; set; }
    public List<ContractState> Contracts { get; set; } = new();
    public int ContractsCompleted { get; set; }
    public int CosmeticTickets { get; set; }                    // Görev ödülü: bir mikro kozmetiği ücretsiz açar
    public List<string> DetachedWagons { get; set; } = new();   // Garajda trenden ayrılan vagonlar
    public int PhotosTaken { get; set; }

    // --- Hikâye: görülen sonlar, imdat görevleri ---
    public List<string> EndingsSeen { get; set; } = new();    // "hero", "lonewolf", "merchant", "sacrifice"
    public int SosCompleted { get; set; }
    public int MusicBoxPlays { get; set; }

    // --- Buzul Kıyameti ve Acımasız Kış ---
    public int WastelandUnlocked { get; set; } = 1;
    public bool WastelandCleared { get; set; }
    public int HardcoreBestKm { get; set; }
    public int HardcoreWins { get; set; }
    public int HardcoreRuns { get; set; }

    // --- Hikâye güncellemesi: 40 bölüm, seçimler, diyaloglar, yolcu görevleri ---
    public int ChapterScheme { get; set; }                       // 0: eski 20 bölümlük kayıt, 2: 40 bölüm
    public TheLastDelivery.Story.StoryState Story { get; set; } = new();         // Seçim bayrakları, görülen replikler, görev adımları

    // --- Episode 2 "Project AURA" ---
    public bool Episode1Complete { get; set; }                   // Episode 1 bir kez bitirildi (Episode 2 açılır, kalıcı ölüm kalkar)
    public int Episode2Unlocked { get; set; } = 1;               // Episode 2'de açılan en ileri bölüm
    public bool Episode2Complete { get; set; }
    public TheLastDelivery.Story.StoryState Story2 { get; set; } = new(); // Episode 2 seçimleri (ayrı tutulur)
    public TheLastDelivery.Story.EpisodeCarry Carry { get; set; } // Episode 1'den aktarılanlar: karma, yolcular, seçimler

    // --- Derin anlatı: kalıcı arşiv (bütün koşularda bulunan belgeler, kasetler ve hatıralar) ---
    public List<string> Archive { get; set; } = new();          // Bir kez bulunan her kayıt arşivde kalır
    public List<string> ArchiveRead { get; set; } = new();      // Arşiv ekranında okunanlar ("YENİ" etiketi için)
    public bool MiniMapHidden { get; set; }                      // [N] ile gizlenen mini harita

    public string Equipped(CosmeticSlot slot) => slot switch
    {
        CosmeticSlot.Smoke => EquippedSmoke,
        CosmeticSlot.Painting => EquippedPainting,
        CosmeticSlot.Radio => EquippedRadio,
        CosmeticSlot.Costume => EquippedCostume,
        CosmeticSlot.Pet => EquippedPet,
        _ => EquippedMusicBox
    };

    /// <summary>Kozmetiği kuşanır. Varsayılanlar herkese açıktır; DLC öğeleri yalnızca sahibine.</summary>
    public static bool TryEquip(CosmeticItem item)
    {
        if (!CosmeticCatalog.IsOwned(item)) return false;
        switch (item.Slot)
        {
            case CosmeticSlot.Smoke: Current.EquippedSmoke = item.Id; break;
            case CosmeticSlot.Painting: Current.EquippedPainting = item.Id; break;
            case CosmeticSlot.Radio: Current.EquippedRadio = item.Id; break;
            case CosmeticSlot.Costume: Current.EquippedCostume = item.Id; break;
            case CosmeticSlot.Pet: Current.EquippedPet = item.Id; break;
            default: Current.EquippedMusicBox = item.Id; break;
        }
        Save();
        return true;
    }

    // ------------------------------------------------------------------
    // Tekil profil ve dosya işlemleri
    // ------------------------------------------------------------------

    private static string FilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TheLastDelivery", "save.json");

    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static SaveData Current { get; private set; } = new();

    public static void Load()
    {
        try
        {
            if (File.Exists(FilePath))
                Current = JsonSerializer.Deserialize<SaveData>(File.ReadAllText(FilePath)) ?? new SaveData();
        }
        catch (Exception)
        {
            Current = new SaveData(); // Bozuk kayıt: varsayılan profil
        }
        Current.Sanitize();
        TheLastDelivery.Story.Narrative.Invalidate();
    }

    public static void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
            File.WriteAllText(FilePath, JsonSerializer.Serialize(Current, Options));
        }
        catch (Exception)
        {
            // Kaydedilemedi: oyun devam eder, bir sonraki değişiklikte yeniden denenir
        }
    }

    private void Sanitize()
    {
        Credits = Math.Max(0, Credits);
        // Eski 20 bölümlük kayıt: açılan bölüm yeni 40 bölümlük hatta aynı kilometreye taşınır
        if (ChapterScheme < 2)
        {
            UnlockedChapter = (Math.Max(1, UnlockedChapter) - 1) * 2 + 1;
            ChapterScheme = 2;
        }
        UnlockedChapter = Math.Clamp(UnlockedChapter, 1, MaxChapter);
        Story ??= new();
        Story.Sanitize();
        Story2 ??= new();
        Story2.Sanitize();
        Archive ??= new();
        ArchiveRead ??= new();
        if (Carry != null)
        {
            Carry.Survivors ??= new();
            Carry.Flags ??= new();
            Carry.Trust ??= new();
            Carry.Documents ??= new();
        }
        if (Delivered) Episode1Complete = true;
        Episode2Unlocked = Math.Clamp(Episode2Unlocked, 1, TheLastDelivery.Story.Chapters.Episode2Count);
        Upgrades ??= new();
        OwnedSkins ??= new();
        OwnedThemes ??= new();
        Achievements ??= new();
        EndlessLeaderboard ??= new();
        OwnedDlc ??= new();
        OwnedCosmetics ??= new();
        EquippedSmoke ??= "classic";
        EquippedPainting ??= "none";
        EquippedRadio ??= "standard";
        EquippedCostume ??= "classic";
        EquippedMusicBox ??= "lullaby";
        EquippedPet ??= "none";
        Contracts ??= new();
        DetachedWagons ??= new();
        CosmeticTickets = Math.Max(0, CosmeticTickets);
        EndingsSeen ??= new();
        WastelandUnlocked = Math.Clamp(WastelandUnlocked, 1, TheLastDelivery.Story.Chapters.Wasteland.Length);
        PeakCredits = Math.Max(PeakCredits, Credits);
        EndlessBestKm = Math.Max(0, EndlessBestKm);
        SpeedrunBestSeconds = Math.Max(0f, SpeedrunBestSeconds);
        if (!OwnedSkins.Contains("classic")) OwnedSkins.Add("classic");
        if (!OwnedThemes.Contains("classic")) OwnedThemes.Add("classic");
        if (!OwnedSkins.Contains(EquippedSkin)) EquippedSkin = "classic";
        if (!OwnedThemes.Contains(EquippedTheme)) EquippedTheme = "classic";
    }

    // ------------------------------------------------------------------
    // Ekonomi işlemleri (her değişiklik hemen kaydedilir)
    // ------------------------------------------------------------------

    public static void Earn(int amount)
    {
        if (amount <= 0) return;
        Current.Credits += amount;
        Current.PeakCredits = Math.Max(Current.PeakCredits, Current.Credits);
        Save();
    }

    public static bool TrySpend(int amount)
    {
        if (Current.Credits < amount) return false;
        Current.Credits -= amount;
        Save();
        return true;
    }

    public static bool HasUpgrade(UpgradeType type) => Current.Upgrades.Contains(type.ToString());

    public static bool TryBuyUpgrade(UpgradeType type)
    {
        if (HasUpgrade(type) || !TrySpend(UpgradeCatalog.Price(type))) return false;
        Current.Upgrades.Add(type.ToString());
        Save();
        return true;
    }

    /// <summary>Skin: sahipse kuşanır; değilse (başarımı açıksa) parası yetiyorsa satın alıp kuşanır.</summary>
    public static bool TryBuyOrEquipSkin(TrainSkin skin)
    {
        if (!Platform.DLCManager.Owns(skin.Dlc)) return false; // DLC skini: yalnızca sahibine
        if (!Current.OwnedSkins.Contains(skin.Id))
        {
            if (!Economy.Achievements.IsUnlocked(skin.Achievement)) return false;
            if (!TrySpend(skin.Price)) return false;
            Current.OwnedSkins.Add(skin.Id);
        }
        Current.EquippedSkin = skin.Id;
        Save();
        return true;
    }

    public static bool TryBuyOrEquipTheme(CabinTheme theme)
    {
        if (!Current.OwnedThemes.Contains(theme.Id))
        {
            if (!TrySpend(theme.Price)) return false;
            Current.OwnedThemes.Add(theme.Id);
        }
        Current.EquippedTheme = theme.Id;
        Save();
        return true;
    }

    /// <summary>Hikâye modundaki bölüm sayısı (kilit sınırı).</summary>
    public const int MaxChapter = 40;

    /// <summary>Bölüm tamamlandı: sıradaki bölümün kilidi açılır. Yeni bir kilit açıldıysa true.</summary>
    public static bool CompleteChapter(int number)
    {
        int next = Math.Min(MaxChapter, number + 1);
        if (next <= Current.UnlockedChapter) return false;
        Current.UnlockedChapter = next;
        Save();
        return true;
    }

    /// <summary>İstatistik sayaçlarını artırıp kaydeder (başarımlar bunlara bakar).</summary>
    public static void Record(Action<SaveData> change)
    {
        change(Current);
        Save();
    }
}
