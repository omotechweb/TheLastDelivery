using System;
using TheLastDelivery.Core;
using TheLastDelivery.Localization;

namespace TheLastDelivery.Story;

/// <summary>
/// Bölümün görsel ve oynanış teması. Tema; hikâye kartını, arka plan efektlerini ve
/// hangi yol olaylarının sık çıkacağını belirler.
/// </summary>
public enum ChapterTheme
{
    Plains,   // Karlı açık ova: yük vagonları sık
    Tunnels,  // Karanlık maden tünelleri: görüş az, pusu çok
    Thaw,     // Erken erime: arkadan gelen buzul çöküşü
    Blizzard, // Tipi: sıcaklık çakılır, çığ riski
    Wolves,   // Kurt toprakları: sürü saldırıları
    IceRails, // Buz köprüleri: çökmüş raylar ve makaslar
    Mining,   // Maden tepeleri: terk edilmiş maden girişleri
    Night,    // Kutup gecesi: uzun geceler, hainler iş başında
    AcidWaste // DLC Buzul Kıyameti: zehirli kar gövdeyi aşındırır, asit fırtınaları
}

/// <summary>Bölümün hangi oyun moduna ait olduğu (adı ve kalan mesafe gösterimi buna göre değişir).</summary>
public enum ChapterKind { Story, Endless, Speedrun, Wasteland, Episode2 }

/// <summary>Yol olaylarının göreli çıkma ağırlıkları (Acid: yalnızca Buzul Kıyameti'nde).</summary>
public readonly record struct EventWeights(
    float Cache, float Ice, float Valve, float Wolves, float Switch, float Blizzard, float Mine, float Acid = 0f)
{
    public float Total => Cache + Ice + Valve + Wolves + Switch + Blizzard + Mine + Acid;
}

/// <summary>
/// Yolculuğun bir bölümü. Km değerleri katedilen yola göredir (0 = başlangıç, 2000 = Sığınak-0).
/// Hikâye modunda 20 bölüm vardır; her biri 100 km'dir ve sırayla açılır.
/// Sonsuz ve zamanla yarış modları da aynı kayıt yapısını kullanır (Kind ile ayrılır).
/// Metinler: bölüm adı "chn.{N}", hikâye ve etki metni temaya göre ("ch1.*" / "cth.{tema}.*").
/// </summary>
public sealed record Chapter(
    int Number,
    float StartKm,
    float EndKm,
    ChapterTheme Theme,
    float CoolingMultiplier,       // Kazanın soğuma hızı çarpanı
    float EventCooldownMultiplier, // Yol olayları arası bekleme çarpanı (küçük = sık)
    int MaxAmbushes,               // Bu bölümdeki en fazla mafya pususu
    float AmbushFrequency,         // Pusu bekleme çarpanı (küçük = sık)
    float Darkness,                // 0 = açık hava, 1 = zifiri karanlık tünel
    float MaxStorm,                // Fırtına yoğunluğunun üst sınırı
    bool GlacierCollapse)          // Arkadan yaklaşan buzul çöküşü
{
    public ChapterKind Kind { get; init; } = ChapterKind.Story;

    /// <summary>Sonsuz modda aşama numarası (1'den başlar).</summary>
    public int Stage { get; init; }

    public string Name => Kind switch
    {
        ChapterKind.Endless => Loc.F("endless.stage", Stage, Loc.T($"cth.{ThemeId}.name")),
        ChapterKind.Speedrun => Loc.T("speedrun.name"),
        ChapterKind.Wasteland => Loc.T($"chw.{Number}"),
        ChapterKind.Episode2 => Loc.T($"e2n.{Number}"),
        _ => Loc.T($"chn.{Number}")
    };

    /// <summary>Episode 2: bu bölümde AURA'nın sinyal kuleleri hattın kenarında belirir (hack fırsatı).</summary>
    public bool HackTowers { get; init; }

    /// <summary>Episode 2: bu bölüm DLC gerektiriyor mu? (Bölüm 1-2 herkese açık.)</summary>
    public bool RequiresEpisode2Dlc => Kind == ChapterKind.Episode2 && Number > Chapters.Episode2FreeChapters;

    /// <summary>
    /// Kritik bölüm: hikâyenin gergin anı. Vana patlaması, kurt sürüsü ve mafya pususu yalnızca kritik
    /// bölümlerde ya da aşırı fırtınada tam sıklıkla çıkar. Bazı bölümler önceki seçimlere göre kritikleşir.
    /// </summary>
    public bool Critical { get; init; }

    /// <summary>"Yalnızca hikâye" bölümü: sürüş yok, bu sahnede keşif ya da diyalog oynanır.</summary>
    public TownArchetype? Scene { get; init; }

    public bool IsStoryScene => Scene != null;

    /// <summary>Önceki seçimler dahil, bu bölüm şu an kritik mi?</summary>
    public bool IsCritical => Kind == ChapterKind.Story
        ? (Critical || ChapterStory.ForcedCritical(Number)) && !ChapterStory.ForcedCalm(Number)
        : Critical;

    // Hikâye modunda her bölümün kendi anlatı metni (seçimlere göre değişebilir); diğer modlarda tema metni
    public string Story => Kind switch
    {
        ChapterKind.Story => Loc.T(ChapterStory.BeatKey(Number)),
        ChapterKind.Episode2 => Loc.T(ChapterStory.Episode2BeatKey(Number)),
        _ => Loc.T($"{StoryKey}.story")
    };

    public string Effect => Scene switch
    {
        null when HackTowers => Loc.T("e2.effect.hack"),
        null => Loc.T($"{StoryKey}.effect"),
        TownArchetype.BorderControl or TownArchetype.RadioMast or TownArchetype.ShelterGate
            or TownArchetype.NullChapel or TownArchetype.ServerHall or TownArchetype.MirrorRoom or TownArchetype.AuraCore
            => Loc.T("chs.effect.talk"),
        _ => Loc.T("chs.effect.explore")
    };

    /// <summary>Temanın kısa kimliği ("plains", "wolves"...).</summary>
    public string ThemeId => Theme.ToString().ToLowerInvariant();

    // İlk üç tema eski bölüm metinlerini kullanır; diğerleri "cth.{tema}" anahtarlarını
    private string StoryKey => Theme switch
    {
        ChapterTheme.Plains => "ch1",
        ChapterTheme.Tunnels => "ch2",
        ChapterTheme.Thaw => "ch3",
        _ => $"cth.{ThemeId}"
    };

    /// <summary>Kalan mesafe aralığı, örn. "2000 - 1900 KM".</summary>
    public string RemainingRange => Kind switch
    {
        ChapterKind.Endless => Loc.F("endless.range", (int)StartKm),
        ChapterKind.Speedrun => Loc.F("speedrun.range", (int)GameConfig.SpeedrunDistanceKm, (int)GameConfig.SpeedrunTimeLimit),
        ChapterKind.Wasteland => Loc.F("chapter.range", (int)(GameConfig.WastelandDistanceKm - StartKm), (int)(GameConfig.WastelandDistanceKm - EndKm)),
        _ => Loc.F("chapter.range", (int)(GameConfig.TotalDistanceKm - StartKm), (int)(GameConfig.TotalDistanceKm - EndKm))
    };

    public string HudTitle => Kind switch
    {
        ChapterKind.Story => Loc.F("chapter.hud", Number, Name),
        ChapterKind.Episode2 => Loc.F("e2.hud", Number, Name),
        ChapterKind.Wasteland => Loc.F("wasteland.hud", Number, Name),
        _ => Name
    };

    /// <summary>Bu temada hangi olayların ne sıklıkla çıkacağı.</summary>
    public EventWeights Weights => Chapters.WeightsFor(Theme);

    /// <summary>Gece ne kadar karanlık olur (Kutup Gecesi temasında daha koyu).</summary>
    public float NightStrength => Theme == ChapterTheme.Night ? 1.4f : 1f;
}

public static class Chapters
{
    private static Chapter C(int n, ChapterTheme theme, float cooling, float cooldown, int ambushes, float ambushFreq,
        float darkness, float storm, bool collapse = false)
    {
        float start = (n - 1) * GameConfig.ChapterLengthKm;
        return new Chapter(n, start, start + GameConfig.ChapterLengthKm, theme, cooling, cooldown, ambushes, ambushFreq,
            darkness, storm, collapse);
    }

    /// <summary>Hikâye sahnesi: sürüşsüz bölüm (keşif ya da diyalog). Kart görseli için bir tema da taşır.</summary>
    private static Chapter S(int n, ChapterTheme theme, TownArchetype scene) =>
        C(n, theme, 1f, 1f, 0, 1f, theme == ChapterTheme.Night ? 0.3f : 0f, 0.4f) with { Scene = scene };

    /// <summary>
    /// Hikâye modunun 40 kısa bölümü (her biri 50 km). Zorluk kademeli artar. Altı bölüm "yalnızca hikâye":
    /// 10 Sessiz Karargah, 17 Eski Ev, 25 Sınır Kapısı, 31 AURORA, 37 Kuzey Yıldızı, 39 Konsey'in Elçisi.
    /// Kritik bölümlerde pusu ve saldırılar tam sıklıktadır; diğerlerinde %60 seyrektir.
    /// </summary>
    public static readonly Chapter[] All =
    {
        C(1,  ChapterTheme.Plains,   1.00f, 0.90f, 0, 1.2f, 0f,    0.35f),
        C(2,  ChapterTheme.Plains,   1.00f, 0.90f, 1, 1.2f, 0f,    0.40f),
        C(3,  ChapterTheme.Wolves,   1.00f, 0.85f, 1, 1.1f, 0f,    0.45f) with { Critical = true },
        C(4,  ChapterTheme.Mining,   1.05f, 0.85f, 1, 1.0f, 0f,    0.45f),
        C(5,  ChapterTheme.IceRails, 1.05f, 0.85f, 1, 1.0f, 0f,    0.50f),
        C(6,  ChapterTheme.Plains,   1.05f, 0.80f, 1, 1.0f, 0f,    0.50f),
        C(7,  ChapterTheme.Tunnels,  1.10f, 0.85f, 2, 0.6f, 0.62f, 0.30f) with { Critical = true },
        C(8,  ChapterTheme.Blizzard, 1.15f, 0.80f, 1, 1.0f, 0.15f, 0.85f),
        C(9,  ChapterTheme.Night,    1.10f, 0.80f, 1, 0.8f, 0.25f, 0.50f),
        S(10, ChapterTheme.Night,    TownArchetype.Headquarters),
        C(11, ChapterTheme.Mining,   1.10f, 0.80f, 1, 0.9f, 0f,    0.50f),
        C(12, ChapterTheme.Wolves,   1.10f, 0.75f, 1, 0.9f, 0f,    0.55f),
        C(13, ChapterTheme.IceRails, 1.15f, 0.75f, 1, 0.9f, 0f,    0.55f),
        C(14, ChapterTheme.Night,    1.15f, 0.75f, 1, 0.8f, 0.3f,  0.55f),
        C(15, ChapterTheme.Tunnels,  1.15f, 0.80f, 2, 0.5f, 0.65f, 0.35f) with { Critical = true },
        C(16, ChapterTheme.Blizzard, 1.20f, 0.70f, 1, 0.9f, 0.2f,  0.90f),
        S(17, ChapterTheme.Plains,   TownArchetype.Homestead),
        C(18, ChapterTheme.Plains,   1.15f, 0.75f, 1, 0.9f, 0f,    0.55f),
        C(19, ChapterTheme.Wolves,   1.20f, 0.70f, 2, 0.7f, 0.1f,  0.65f) with { Critical = true },
        C(20, ChapterTheme.Night,    1.20f, 0.70f, 1, 0.7f, 0.3f,  0.60f),
        C(21, ChapterTheme.Mining,   1.20f, 0.70f, 2, 0.7f, 0f,    0.60f),
        C(22, ChapterTheme.Blizzard, 1.25f, 0.65f, 1, 0.9f, 0.2f,  0.90f),
        C(23, ChapterTheme.IceRails, 1.25f, 0.65f, 2, 0.8f, 0f,    0.65f),
        C(24, ChapterTheme.Tunnels,  1.25f, 0.70f, 3, 0.45f, 0.65f, 0.40f) with { Critical = true },
        S(25, ChapterTheme.Blizzard, TownArchetype.BorderControl),
        C(26, ChapterTheme.Plains,   1.20f, 0.70f, 1, 0.9f, 0f,    0.65f),
        C(27, ChapterTheme.Night,    1.25f, 0.65f, 2, 0.6f, 0.3f,  0.65f),
        C(28, ChapterTheme.Mining,   1.25f, 0.65f, 2, 0.7f, 0f,    0.65f),
        C(29, ChapterTheme.Wolves,   1.25f, 0.65f, 2, 0.7f, 0.1f,  0.70f),
        C(30, ChapterTheme.Blizzard, 1.30f, 0.60f, 2, 0.6f, 0.25f, 1.00f) with { Critical = true },
        S(31, ChapterTheme.IceRails, TownArchetype.Institute),
        C(32, ChapterTheme.IceRails, 1.25f, 0.60f, 2, 0.7f, 0f,    0.70f),
        C(33, ChapterTheme.Night,    1.30f, 0.60f, 2, 0.6f, 0.3f,  0.70f),
        C(34, ChapterTheme.Tunnels,  1.30f, 0.60f, 3, 0.4f, 0.65f, 0.45f) with { Critical = true },
        C(35, ChapterTheme.Blizzard, 1.35f, 0.60f, 2, 0.6f, 0.25f, 1.00f),
        C(36, ChapterTheme.Wolves,   1.30f, 0.55f, 2, 0.6f, 0.1f,  0.75f) with { Critical = true },
        S(37, ChapterTheme.Night,    TownArchetype.RadioMast),
        C(38, ChapterTheme.Thaw,     0.90f, 0.60f, 1, 1.0f, 0f,    0.45f, collapse: true),
        S(39, ChapterTheme.Thaw,     TownArchetype.ShelterGate),
        C(40, ChapterTheme.Thaw,     0.85f, 0.55f, 1, 1.0f, 0f,    0.40f, collapse: true) with { Critical = true },
    };

    public static int Count => All.Length;

    // ------------------------------------------------------------------
    // Episode 2 "Project AURA": Kutup Hattı, 40 bölüm (her biri 50 km)
    // ------------------------------------------------------------------

    /// <summary>Herkese açık "tanıtım" bölümleri; sonrası DLC_EPISODE2_UNLOCKED ister.</summary>
    public const int Episode2FreeChapters = 2;

    public static int Episode2Count => Episode2.Length;

    private static Chapter E(int n, ChapterTheme theme, float cooling, float cooldown, int ambushes, float ambushFreq,
        float darkness, float storm, bool critical = false, bool towers = false) =>
        C(n, theme, cooling, cooldown, ambushes, ambushFreq, darkness, storm) with
        {
            Kind = ChapterKind.Episode2, Critical = critical, HackTowers = towers
        };

    private static Chapter ES(int n, ChapterTheme theme, TownArchetype scene) => S(n, theme, scene) with { Kind = ChapterKind.Episode2 };

    /// <summary>
    /// Bölüm 1-2: tanıtım (Sentinel pususu, ilk frekans kulesi). 3-10: Sinyal Savaşı (hack kuleleri).
    /// 11-20: Dönüştürülmüş Şehirler. 21-30: Kutup Sınırı ve otonom tren savaşları. 31-39: Kutup Yeraltı Üssü.
    /// 40: AURA'nın çekirdeği. Kritik bölümlerde pusuyu AURA'nın Sentinel trenleri kurar.
    /// </summary>
    public static readonly Chapter[] Episode2 =
    {
        E(1,  ChapterTheme.Plains,   1.00f, 0.90f, 1, 1.0f, 0f,    0.30f, critical: true),
        ES(2, ChapterTheme.Night,    TownArchetype.SignalTower),
        E(3,  ChapterTheme.Plains,   1.05f, 0.85f, 1, 1.0f, 0f,    0.40f, towers: true),
        E(4,  ChapterTheme.IceRails, 1.05f, 0.85f, 1, 1.0f, 0f,    0.45f, towers: true),
        E(5,  ChapterTheme.Night,    1.10f, 0.80f, 1, 0.9f, 0.3f,  0.45f, critical: true, towers: true),
        E(6,  ChapterTheme.Wolves,   1.10f, 0.80f, 1, 0.9f, 0f,    0.50f, towers: true),
        E(7,  ChapterTheme.Blizzard, 1.15f, 0.80f, 1, 0.9f, 0.15f, 0.80f, towers: true),
        E(8,  ChapterTheme.Tunnels,  1.15f, 0.85f, 2, 0.6f, 0.62f, 0.30f, critical: true, towers: true),
        E(9,  ChapterTheme.Mining,   1.15f, 0.80f, 1, 0.9f, 0f,    0.50f, towers: true),
        ES(10, ChapterTheme.Night,   TownArchetype.RelayStation),
        E(11, ChapterTheme.Plains,   1.15f, 0.75f, 1, 0.9f, 0f,    0.55f),
        E(12, ChapterTheme.Mining,   1.20f, 0.75f, 1, 0.9f, 0f,    0.55f),
        E(13, ChapterTheme.Night,    1.20f, 0.75f, 2, 0.7f, 0.3f,  0.55f, critical: true),
        E(14, ChapterTheme.IceRails, 1.20f, 0.70f, 1, 0.9f, 0f,    0.60f),
        ES(15, ChapterTheme.Mining,  TownArchetype.SleepFactory),
        E(16, ChapterTheme.Blizzard, 1.25f, 0.70f, 1, 0.9f, 0.2f,  0.90f),
        E(17, ChapterTheme.Wolves,   1.25f, 0.70f, 2, 0.7f, 0.1f,  0.65f, critical: true),
        E(18, ChapterTheme.Tunnels,  1.25f, 0.75f, 2, 0.6f, 0.65f, 0.40f),
        E(19, ChapterTheme.Night,    1.25f, 0.70f, 1, 0.8f, 0.3f,  0.60f),
        ES(20, ChapterTheme.Night,   TownArchetype.NullChapel),
        E(21, ChapterTheme.IceRails, 1.25f, 0.65f, 2, 0.6f, 0f,    0.65f, critical: true),
        E(22, ChapterTheme.Blizzard, 1.30f, 0.65f, 2, 0.6f, 0.2f,  0.95f, critical: true),
        E(23, ChapterTheme.Plains,   1.25f, 0.65f, 2, 0.7f, 0f,    0.65f),
        E(24, ChapterTheme.Tunnels,  1.30f, 0.65f, 2, 0.5f, 0.65f, 0.40f, critical: true),
        E(25, ChapterTheme.Night,    1.30f, 0.65f, 3, 0.5f, 0.3f,  0.70f, critical: true),
        E(26, ChapterTheme.Wolves,   1.30f, 0.60f, 2, 0.6f, 0.1f,  0.70f),
        E(27, ChapterTheme.IceRails, 1.30f, 0.60f, 2, 0.6f, 0f,    0.70f, critical: true),
        E(28, ChapterTheme.Blizzard, 1.35f, 0.60f, 2, 0.6f, 0.25f, 1.00f, critical: true),
        E(29, ChapterTheme.Mining,   1.30f, 0.60f, 2, 0.6f, 0f,    0.70f),
        E(30, ChapterTheme.Night,    1.35f, 0.60f, 3, 0.4f, 0.35f, 0.75f, critical: true),
        ES(31, ChapterTheme.Tunnels, TownArchetype.PolarBase),
        E(32, ChapterTheme.Tunnels,  1.35f, 0.60f, 2, 0.5f, 0.70f, 0.40f, critical: true),
        E(33, ChapterTheme.Night,    1.35f, 0.60f, 2, 0.6f, 0.40f, 0.70f),
        ES(34, ChapterTheme.Night,   TownArchetype.ServerHall),
        E(35, ChapterTheme.Blizzard, 1.40f, 0.55f, 2, 0.6f, 0.30f, 1.00f, critical: true),
        E(36, ChapterTheme.Tunnels,  1.40f, 0.55f, 3, 0.4f, 0.70f, 0.45f, critical: true),
        ES(37, ChapterTheme.Night,   TownArchetype.MirrorRoom),
        E(38, ChapterTheme.Thaw,     0.90f, 0.55f, 2, 0.6f, 0.2f,  0.50f, critical: true),
        E(39, ChapterTheme.Tunnels,  1.40f, 0.55f, 3, 0.4f, 0.75f, 0.45f, critical: true),
        ES(40, ChapterTheme.Night,   TownArchetype.AuraCore),
    };

    /// <summary>Sonsuz modda temalar bu sırayla döner.</summary>
    private static readonly ChapterTheme[] EndlessThemes =
    {
        ChapterTheme.Plains, ChapterTheme.Wolves, ChapterTheme.Mining, ChapterTheme.IceRails,
        ChapterTheme.Blizzard, ChapterTheme.Night, ChapterTheme.Tunnels
    };

    /// <summary>
    /// Sonsuz kurye modunun aşaması: her aşamada fırtına sertleşir, olaylar sıklaşır, pusular çoğalır.
    /// </summary>
    public static Chapter ForEndless(int stage)
    {
        var theme = EndlessThemes[(stage - 1) % EndlessThemes.Length];
        float t = stage - 1;
        float start = t * GameConfig.EndlessStageKm;
        float darkness = theme switch { ChapterTheme.Tunnels => 0.62f, ChapterTheme.Night => 0.3f, ChapterTheme.Blizzard => 0.2f, _ => 0f };
        return new Chapter(stage, start, start + GameConfig.EndlessStageKm, theme,
            CoolingMultiplier: MathF.Min(1.7f, 1f + 0.06f * t),
            EventCooldownMultiplier: MathF.Max(0.35f, 0.9f - 0.06f * t),
            MaxAmbushes: 1 + (int)(t / 2f),
            AmbushFrequency: MathF.Max(0.4f, 1.1f - 0.07f * t),
            Darkness: darkness,
            MaxStorm: MathF.Min(1f, 0.4f + 0.07f * t + (theme == ChapterTheme.Blizzard ? 0.3f : 0f)),
            GlacierCollapse: false)
        {
            Kind = ChapterKind.Endless,
            Stage = stage
        };
    }

    /// <summary>Zamanla yarış: kısa, fırtınalı bir teslimat hattı.</summary>
    public static readonly Chapter Speedrun = new(0, 0f, GameConfig.SpeedrunDistanceKm, ChapterTheme.Blizzard,
        CoolingMultiplier: 1f, EventCooldownMultiplier: 0.8f, MaxAmbushes: 1, AmbushFrequency: 0.8f,
        Darkness: 0f, MaxStorm: 0.6f, GlacierCollapse: false)
    {
        Kind = ChapterKind.Speedrun
    };

    /// <summary>
    /// DLC "Buzul Kıyameti": ana oyundan sonraki 5 zorlu bölüm, kendi 500 km'lik hattında.
    /// Zehirli kar gövdeyi sürekli aşındırır, asit fırtınaları sık; zorluk bölümle artar.
    /// </summary>
    public static readonly Chapter[] Wasteland =
    {
        W(1, 1.30f, 0.65f, 2, 0.6f, 0.1f,  0.70f),
        W(2, 1.35f, 0.60f, 2, 0.55f, 0.15f, 0.80f),
        W(3, 1.40f, 0.55f, 3, 0.5f, 0.35f, 0.75f),
        W(4, 1.45f, 0.50f, 3, 0.45f, 0.2f,  0.90f),
        W(5, 1.55f, 0.45f, 3, 0.4f, 0.25f, 1.00f),
    };

    private static Chapter W(int n, float cooling, float cooldown, int ambushes, float ambushFreq, float darkness, float storm)
    {
        float start = (n - 1) * GameConfig.WastelandChapterKm;
        return new Chapter(n, start, start + GameConfig.WastelandChapterKm, ChapterTheme.AcidWaste, cooling, cooldown,
            ambushes, ambushFreq, darkness, storm, GlacierCollapse: false) { Kind = ChapterKind.Wasteland };
    }

    /// <summary>Temaya göre olay ağırlıkları: (yük vagonu, buz, vana, kurt, makas, tipi, maden, asit).</summary>
    public static EventWeights WeightsFor(ChapterTheme theme) => theme switch
    {
        ChapterTheme.Plains   => new(4.0f, 2.0f, 2.0f, 1.0f, 1.0f, 0.5f, 1.0f),
        ChapterTheme.Tunnels  => new(1.5f, 1.0f, 2.5f, 0.0f, 2.0f, 0.0f, 2.0f),
        ChapterTheme.Thaw     => new(1.0f, 2.0f, 2.0f, 0.5f, 2.5f, 0.5f, 0.5f),
        ChapterTheme.Blizzard => new(1.0f, 2.0f, 1.5f, 1.0f, 1.0f, 4.0f, 0.5f),
        ChapterTheme.Wolves   => new(1.5f, 1.0f, 1.5f, 4.0f, 1.0f, 1.0f, 1.0f),
        ChapterTheme.IceRails => new(1.0f, 3.0f, 1.5f, 0.5f, 4.0f, 0.5f, 0.5f),
        ChapterTheme.Mining   => new(1.5f, 1.0f, 2.0f, 1.0f, 1.0f, 0.5f, 4.0f),
        ChapterTheme.AcidWaste => new(1.0f, 1.5f, 1.5f, 2.0f, 1.5f, 1.5f, 0.5f, 4.0f),
        _                     => new(1.5f, 1.5f, 2.0f, 2.5f, 1.5f, 1.0f, 1.0f), // Kutup gecesi
    };
}
