using System;

namespace TheLastDelivery.Core;

/// <summary>
/// Oyunun durum makinesi.
/// Driving ve Station iki ana durumdur; Arriving, sürüşten istasyona geçişteki
/// otomatik fren evresidir. GameOver ve Victory ise bitiş ekranlarıdır.
/// Duraklatma ayrı bir durum değil, herhangi bir oynanış durumunun üstüne binen bayraktır;
/// böylece devam edildiğinde oyun tam kaldığı yerden sürer.
/// </summary>
public enum GameState
{
    LanguageSelect, // Oyun açılırken dil seçimi
    MainMenu,       // Ana menü
    TutorialPrompt, // "Eğitimi oynamak ister misiniz?" sorusu
    Tutorial,       // Koca Thomas'ın rehberliğinde güvenli alıştırma
    ChapterIntro,   // Siyah fonda bölüm adı ve hikaye metni
    Driving,        // Oyuncu treni sürüyor
    Arriving,  // Tren istasyona yaklaşıyor, kontrol oyuncudan alındı ve otomatik fren devrede
    Station,   // Terk edilmiş istasyonda duruldu, diyalog açık
    Town,      // Kasabada yürüyüş modu (A/D ile yürü, E ile etkileşim)
    Depot,     // Buzul benzinliği: ticaret arayüzü ve pompa mini oyunu
    Chase,     // Mafya pususu: kaçış kovalamacası
    Mining,    // Terk edilmiş maden: kazı mini oyunu
    FinalChoice, // Sığınak-0 kapısında son karar (gizli karma ve mafya anlaşması seçenekleri açar)
    GameOver,  // Donarak ölüm
    Victory,   // Sığınak-0'a teslimat
    Sentinel,    // Episode 2: AURA'nın makinistsiz zırhlı Sentinel treniyle yan yana çatışma
    DlcGate,     // Episode 2 Bölüm 2 sonu: "Project AURA" DLC kilit ekranı
    EpisodeOutro // Episode 1 kapanışı: güneş doğar, telsizden AURA uyarısı (cliffhanger)
}

/// <summary>Yolculuğun türü: hikâye (20 bölüm), sonsuz kurye ya da zamanla yarış.</summary>
public enum GameMode
{
    Story,
    Endless,
    Speedrun,
    Wasteland, // DLC "Buzul Kıyameti": 5 ek bölümlük ayrı hat, asit fırtınaları
    Hardcore,  // DLC "Acımasız Kış Sınavı": tek canlı hikâye koşusu, kaynaklar yarıya iner, sabotaj iki kat
    Episode2   // Episode 2 "Project AURA": 40 bölümlük Kutup Hattı (Bölüm 1-2 ücretsiz, 3-40 DLC)
}

/// <summary>
/// Zorluk çarpanları. Normal oyunda 1; Acımasız Kış Sınavı (Permadeath) modunda kaynaklar yarıya iner
/// ve hain sabotajları iki kat sık olur. Mod başlarken ayarlanır, menüye dönünce sıfırlanır.
/// </summary>
public static class Difficulty
{
    /// <summary>Yakıt, yağma, erzak gibi kazanılan kaynakların çarpanı.</summary>
    public static float Resources { get; private set; } = 1f;

    /// <summary>Hain sabotaj sıklığı çarpanı (2 = iki kat sık).</summary>
    public static float SabotageFrequency { get; private set; } = 1f;

    public static bool IsHardcore => Resources < 1f;

    public static void SetNormal()
    {
        Resources = 1f;
        SabotageFrequency = 1f;
    }

    public static void SetHardcore()
    {
        Resources = GameConfig.HardcoreResourceMultiplier;
        SabotageFrequency = GameConfig.HardcoreSabotageMultiplier;
    }

    /// <summary>Bir kaynak miktarını zorluğa göre ölçekler (en az 1, eğer başta 1 ya da fazlaysa).</summary>
    public static int Scale(int amount) => amount <= 0 ? amount : Math.Max(1, (int)MathF.Round(amount * Resources));

    public static float Scale(float amount) => amount * Resources;
}

/// <summary>İstasyon durumunun alt evreleri.</summary>
public enum StationPhase
{
    Choosing,  // Senaryo anlatılıyor, oyuncu [1] veya [2] ile karar veriyor
    Outcome    // Kararın sonucu ve günlük notu gösteriliyor
}

/// <summary>Oyunun neden bittiğini belirtir (Game Over ekranındaki açıklama için).</summary>
public enum DeathReason
{
    OutOfFuel,
    BoilerFrozen,
    Derailed,
    PackageLost,
    TimeUp,     // Zamanla yarış: süre doldu
    Madness     // Akıl sağlığı sıfıra indi: Arthur karanlıkta donup kaldı
}

/// <summary>
/// Kabin ve hava durumunun trenin kaynak tüketimine uyguladığı çarpanlar.
/// </summary>
public readonly record struct TravelModifiers(float FuelMultiplier, float CoolingMultiplier, float HeatMultiplier)
{
    public static readonly TravelModifiers None = new(1f, 1f, 1f);
}
