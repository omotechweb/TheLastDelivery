namespace TheLastDelivery.Story;

/// <summary>
/// Hikâye modunun bölüm anlatısı. Her bölümün kartında "chs.{N}" metni okunur; önceki seçimler
/// bazı bölümlerin metnini ("chs.{N}.b" / ".c") ve gerginliğini (kritik bölüm) değiştirir.
///
/// Anlatı adım adım şunları açığa çıkarır: Arthur'un geçmişi (Kuzeyhavn'ın makinisti, tahliye treni,
/// geride kalan Marta ve Sofi), Büyük Donma'nın gerçek sebebi (AURORA iklim deneyi) ve
/// Sığınak-0'ın sırrı (Konsey felaketi biliyordu; Kura ile insanları dışarı atıyor, Kızıl Kurtlar'ı besliyor).
/// </summary>
public static class ChapterStory
{
    /// <summary>Bölüm kartında gösterilecek metnin anahtarı.</summary>
    public static string BeatKey(int chapter) => chapter switch
    {
        12 when Narrative.Has(Flags.HqBroadcast) => "chs.12.b",
        18 when Narrative.Has(Flags.HomeLetter) => "chs.18.b",
        21 when Narrative.Has(Flags.MineOwner) => "chs.21.b",
        21 when Narrative.Has(Flags.MineWorkers) => "chs.21.c",
        23 when Narrative.Has(Flags.CultDefied) => "chs.23.b",
        26 when Narrative.Has(Flags.BorderRefugees) => "chs.26.b",
        26 when Narrative.Has(Flags.BorderBetrayed) => "chs.26.c",
        36 when Narrative.Has(Flags.WolfTruce) => "chs.36.b",
        36 when Narrative.Has(Flags.WolfEnemy) => "chs.36.c",
        38 when Narrative.Has(Flags.AuroraData) => "chs.38.b",
        40 when Narrative.Has(Flags.SofiTrust) => "chs.40.b",
        _ => $"chs.{chapter}"
    };

    /// <summary>Episode 2 bölüm kartının metni: Episode 1'den aktarılan kararlar ve Episode 2 seçimleri değiştirir.</summary>
    public static string Episode2BeatKey(int chapter) => chapter switch
    {
        1 when Narrative.Has("ep1:" + Flags.SofiKnown) => "e2s.1.b",
        11 when Narrative.Has("ep1:" + Flags.BorderRefugees) => "e2s.11.b",
        14 when Narrative.Has(Flags.E2Child) => "e2s.14.b",
        16 when Narrative.Has(Flags.E2Woke) => "e2s.16.b",
        16 when Narrative.Has(Flags.E2Sleep) => "e2s.16.c",
        20 when Narrative.Has(Flags.E2AcolyteDoubt) => "e2s.20.b",
        21 when Narrative.Has(Flags.E2NullInside) => "e2s.21.b",
        24 when Narrative.Has(Flags.E2WolfHelp) => "e2s.24.b",
        32 when Narrative.Has(Flags.E2Virus) => "e2s.32.b",
        36 when Narrative.Has(Flags.E2SofiCall) => "e2s.36.b",
        38 when Narrative.Has(Flags.E2ClearMind) => "e2s.38.b",
        _ => $"e2s.{chapter}"
    };

    /// <summary>Seçimlerin sonucu olarak kritikleşen bölümler (pusu ve saldırılar tam sıklıkta).</summary>
    public static bool ForcedCritical(int chapter) => chapter switch
    {
        12 => Narrative.Has(Flags.HqBroadcast),   // Karargah kayıtları yayınlandı: Kızıl Kurtlar öfkeli
        21 => Narrative.Has(Flags.MineOwner),     // Grevciler Kızıl Kurtlar'a katıldı
        23 => Narrative.Has(Flags.CultDefied),    // Tarikat rayları gevşetti
        36 => Narrative.Has(Flags.WolfEnemy),     // Kurt Baba aç sürüleri hatta sürüyor
        _ => false
    };

    /// <summary>
    /// Seçimlerin sonucu olarak sakinleşen bölümler: Kurt Baba'yla ateşkes yapıldıysa Kızıl Kurtlar
    /// son pususunu kurmaz ve sürüleri hattan uzak tutar.
    /// </summary>
    public static bool ForcedCalm(int chapter) => chapter switch
    {
        34 or 36 => Narrative.Has(Flags.WolfTruce),
        _ => false
    };
}

/// <summary>Hikâye bayraklarının adları (kayıtta bu metinlerle tutulur).</summary>
public static class Flags
{
    // Kasabalar
    public const string TradeMap = "trade.map";               // Kırağı Pazarı: Enstitü haritası satın alındı
    public const string TradeContraband = "trade.contraband"; // Orlov'un kaçak sandığı taşınıyor
    public const string MineWorkers = "mine.workers";         // Demirhan grevinde işçilerin yanında
    public const string MineOwner = "mine.owner";             // Demirhan grevinde ustabaşının yanında
    public const string GhostTape = "ghost.tape";             // Son Fener'de Sofi'nin kaydı dinlendi
    public const string CultBlessed = "cult.blessed";         // Tarikatın kutsamasını kabul etti
    public const string CultDefied = "cult.defied";           // Tarikatı reddetti
    public const string OutpostLedger = "outpost.ledger";     // Kızıl Birlik'in Konsey'den aldığı ödemeler görüldü
    public const string GarrisonAlly = "garrison.ally";       // Garnizon komutanı dost
    public const string PrisonerFreed = "mil.freed";

    // Hikâye bölümleri
    public const string HqDone = "hq.done";
    public const string HqCodebook = "hq.codebook";           // Karargahın kod kitabı alındı
    public const string HqBroadcast = "hq.broadcast";         // Karargah kayıtları her frekansta yayınlandı
    public const string HomeDone = "home.done";
    public const string HomeLetter = "home.letter";           // Marta'nın mektubu alındı
    public const string BorderCodes = "border.codes";
    public const string BorderRefugees = "border.refugees";   // Sınırda mültecileri sakladı
    public const string BorderBetrayed = "border.betrayed";   // Mültecileri teslim etti
    public const string LabDone = "lab.done";
    public const string AuroraData = "aurora.data";           // Ters Döngü verileri kopyalandı
    public const string AuroraDestroyed = "aurora.destroyed";
    public const string SofiKnown = "sofi.known";             // Kuzey Yıldızı'nın Sofi olduğu anlaşıldı
    public const string SofiTrust = "sofi.trust";
    public const string CouncilDeal = "council.deal";         // Konsey'in sessizlik anlaşması kabul edildi
    public const string CouncilRefused = "council.refused";
    public const string CouncilWitnessed = "council.witnessed"; // Yolcular Konsey'in önünde Arthur'a tanıklık etti

    // Yolcu görevleri
    public const string BoraIntel = "bora.intel";             // Bora'nın kardeşi pusuları haber veriyor
    public const string AnyaIntel = "anya.intel";
    public const string DmitriGuard = "dmitri.guard";

    // Sürüş sırasındaki hikâye anları (StoryBeats) ve Kurt Baba
    public const string PackagePeeked = "pkg.peeked";        // Bölüm 2: paketin dikişinden içine bakıldı
    public const string WolfPaid = "wolf.paid";              // Bölüm 15: tünel geçiş bedeli ödendi
    public const string WolfRammed = "wolf.rammed";          // Bölüm 15: barikat yarıldı
    public const string WolfBora = "wolf.bora";              // Bölüm 15: Bora kardeşini barikatta buldu
    public const string WolfTruce = "wolf.truce";            // Bölüm 34: Kurt Baba'yla ateşkes, Kızıl Kurtlar sürgünleri korur
    public const string WolfDeal = "wolf.deal";              // Bölüm 34: paket Kurt Baba'ya sözlendi (Tüccar sonu açılır)
    public const string WolfEnemy = "wolf.enemy";            // Bölüm 34: Kurt Baba reddedildi
    public const string AcolyteSaved = "bridge.saved";       // Bölüm 23: köprüdeki çocuk mürit kurtarıldı
    public const string HungerShared = "hunger.shared";      // Bölüm 29: erzak paylaşıldı
    public const string LullabyHummed = "lull.hummed";       // Bölüm 35: ninniye eşlik edildi

    // Episode 2 "Project AURA"
    public const string E2TowerDone = "e2.tower.done";       // Bölüm 2: frekans kulesi hacklendi, AURA'nın sunucusu bulundu
    public const string E2Nova = "e2.nova";                  // Telsiz korsanı Nova trende
    public const string E2RelayDone = "e2.relay.done";
    public const string E2Virus = "e2.virus";                // Nova'nın virüsü AURA'nın röle ağına yerleştirildi
    public const string E2FactoryDone = "e2.factory.done";
    public const string E2Woke = "e2.woke";                  // Uyku fabrikasındaki binlerce insan uyandırıldı
    public const string E2Sleep = "e2.sleep";                // Uyuyanlar tüplerde bırakıldı
    public const string E2NullEnemy = "e2.nullEnemy";        // NULL'un Kâhini reddedildi
    public const string E2NullInside = "e2.nullInside";      // NULL'a katılmış gibi yapıldı: üssün giriş kodu
    public const string E2NullBroken = "e2.nullBroken";      // Tarikat müritlerinin gözü açıldı
    public const string E2BaseDone = "e2.base.done";
    public const string E2Listened = "e2.listened";          // AURA'nın mantığı dinlendi
    public const string E2ClearMind = "e2.clearMind";        // AURA'nın zihin oyunları görüldü
    public const string E2EndFree = "e2.end.free";           // Son: İnsanlık Özgürdür
    public const string E2EndMachine = "e2.end.machine";     // Son: Makinelerin Mantığı
    public const string E2EndReset = "e2.end.reset";         // Son: Büyük Sıfırlama

    // Episode 2 sürüş anları (StoryBeats) ve çekirdekteki "umut" argümanları
    public const string E2HeartHeard = "e2.heartHeard";       // Bölüm 1: Kalp'in içindeki sesler dinlendi
    public const string E2Answered = "e2.answered";           // Bölüm 5: AURA'ya Kuzeyhavn'ın adlarıyla cevap verildi
    public const string E2Child = "e2.child";                 // Bölüm 13: konvoydaki çatlak tüpten Tali çıkarıldı
    public const string E2AcolyteFriend = "e2.acolyteFriend"; // Bölüm 18: Episode 1'in köprüdeki çocuğu (Ilka) tanındı
    public const string E2AcolyteDoubt = "e2.acolyteDoubt";   // Bölüm 18: müritlere uyuyanların gerçeği anlatıldı
    public const string E2WolfHelp = "e2.wolfHelp";           // Bölüm 23: Morozov'un Kızıl Birlik'i filoyu üstüne çekti
    public const string E2Promise = "e2.promise";             // Bölüm 29: ateş başında güneş sözü verildi
    public const string E2Truth = "e2.truth";                 // Bölüm 29: ateş başında "bilmiyorum" denildi
    public const string E2SofiCall = "e2.sofiCall";           // Bölüm 35: gerçek Sofi'nin sesi (yanlış söylenen ikinci kıta)
    public const string E2LogKept = "e2.logKept";             // Bölüm 39: Arthur'un defteri Sofi'ye bırakıldı
    public const string E2MaraDaughter = "e2.maraDaughter";   // Bölüm 15: Mara kızı Lena'yı tüpünde buldu
    public const string E2Brother = "e2.brother";             // Bölüm 31: Yusuf'un kardeşi Emre uyandırıldı
    public const string E2BrotherLeft = "e2.brotherLeft";     // Bölüm 31: Emre tüpünde bırakıldı
    public const string E2OracleRides = "e2.oracleRides";     // Bölüm 33: dağılan tarikatın Kâhini trene alındı
    public const string E2KirilFaced = "e2.kirilFaced";       // Bölüm 33: Kiril, kodu derlediği Kâhin'le yüzleşti
    public const string HopePrefix = "e2.hope.";              // Çekirdekte AURA'ya söylenen her umut argümanı
    public static readonly string[] HopeArguments = { "woke", "child", "fire", "heart", "names", "ilka", "oracle" };
}
