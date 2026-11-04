namespace TheLastDelivery.Core;

/// <summary>
/// Oyun dengesini belirleyen tüm sabitler tek yerde.
/// Zorluk ayarı yapmak için yalnızca bu dosyayı düzenlemek yeterli.
/// </summary>
public static class GameConfig
{
    // --- Ekran ---
    public const int ScreenWidth = 1280;
    public const int ScreenHeight = 720;

    // --- Dünya yerleşimi (piksel) ---
    public const int GroundY = 560;           // Karlı zemin katmanının üst kenarı
    public const int RailY = GroundY + 15;    // Rayın üst yüzeyi (tekerlerin değdiği çizgi)
    public const int TrainX = 200;            // Trenin (arka vagonun) sol kenarı
    public const int StationStopScreenX = 900; // Tren durduğunda istasyon binasının ekrandaki merkezi

    // --- Mesafe ---
    public const float TotalDistanceKm = 2000f; // Hikâye güncellemesi: 40 kısa bölüm x 50 km
    public const float StationIntervalKm = 200f;
    public const float ChapterLengthKm = 50f;
    public static float ApproachKm => Near(6f); // Durağa bu kadar kala otomatik fren başlar

    // --- Tempo ---
    // Hikâye hatlarında (Episode 1, Acımasız Kış, Episode 2) tren aynı hızla görünür ama yol 13 kat yavaş
    // sayılır: 50 km'lik bir sürüş bölümü saf sürüşle ~4-6 dakika. Oyun bir hikâye oyunudur: bölümün öbür yarısını
    // açılış sinematiği (uzun anlatım sahnesi), sürüş içi hikâye anları, hatıralar, telsiz yayınları ve yolcu
    // diyalogları doldurur; bir bölüm toplamda ~10-14 dakika, kabaca %50 oynanış / %50 hikâyedir. Durağa yaklaşma gibi
    // "ekran mesafeleri" Near() ile, bekleme süreleri ve pasif tüketimler aşağıdaki çarpanlarla aynı oranda
    // ölçeklenir; böylece km başına yakıt, yemek ve akıl sağlığı dengesi değişmez.
    public const float StoryPace = 13f;
    public static float Pace { get; set; } = 1f;

    /// <summary>Ekranda görülen kısa mesafe (yanaşma, kule menzili): tempoyla küçülür, süresi aynı kalır.</summary>
    public static float Near(float km) => km / Pace;

    /// <summary>Pasif tüketim/yenilenme çarpanı (açlık, soba, akıl sağlığı): km başına değer sabit kalır.</summary>
    public static float NeedsScale => 1f / Pace;

    /// <summary>
    /// Yol olayları arası bekleme: hikâye hatlarında olaylar zamana göre eskisinden ~%35 seyrek; sürüş bir
    /// hayatta kalma testinden çok hikâyenin taşıyıcısıdır. Aradaki boşluğu telsiz, hatıra ve diyaloglar doldurur.
    /// </summary>
    public static float EventCooldownScale => Pace > 1f ? 9f : 1f;
    public static float AmbushCooldownScale => Pace > 1f ? 3f : 1f;

    /// <summary>Gün uzunluğu çarpanı: hikâye hatlarında bir gün ~2 bölüm sürer (tempoyla orantılı).</summary>
    public static float DayScale => Pace > 1f ? Pace / 3f : 1f;

    // --- Derin anlatı: zihin oyunları, güven sonuçları, hava cepheleri ---
    public const float MindGameSanity = 65f;        // AURA'nın gösterge manipülasyonu bu akıl sağlığının altında başlar
    public const float StabDamage = 16f;            // Arkadan bıçaklayan yolcu: tren hasarı (kavga, devrilen kazan)
    public const float StabSanity = 25f;
    public const float PlatingArmorMultiplier = 0.8f; // Sasha'nın zırh plakası
    public const float ReportAmbushDelay = 70f;     // İhbardan sonra pusunun kurulması (sürüş saniyesi)
    public const float WeatherFrontMin = 70f;       // Hava cephesi değişimleri arası (saniye)
    public const float WeatherFrontMax = 130f;

    // --- Hız ---
    public const float MaxSpeed = 120f;       // km/sa
    public const float StartSpeed = 30f;
    public const float Acceleration = 25f;    // km/sa / saniye
    public const float Deceleration = 40f;
    public const float CreepSpeed = 10f;      // Perona yanaşırken en düşük hız

    // Oyun zamanı ölçeği: 1 km/sa hız, saniyede bu kadar km yol demek.
    // (60 km/sa ile 200 km yaklaşık 100 saniye sürer.)
    public const float BaseKmPerSecondPerSpeed = 1f / 30f;
    public static float KmPerSecondPerSpeed => BaseKmPerSecondPerSpeed / Pace;

    // Ön katmanın (zeminin) her 1 km/sa için saniyede kaydığı piksel.
    public const float PixelsPerSpeedUnit = 6f;

    // Zemin hızıyla tutarlı olacak şekilde 1 km'nin ekrandaki karşılığı.
    public static float PixelsPerKm => PixelsPerSpeedUnit / KmPerSecondPerSpeed;

    // --- Yakıt ---
    public const float MaxFuel = 100f;
    public const float FuelBurnPerKmBase = 0.15f;   // Her km için sabit tüketim
    public const float FuelBurnPerKmSpeed = 0.15f;  // Yüksek hızda (karesiyle) eklenen tüketim
    public const float FuelIdleBurnPerSecond = 0.08f; // Kazan yanık kaldıkça harcanan yakıt
    public const float OverheatFuelMultiplier = 2f;
    public const float StationRefuel = 40f;

    // --- Kazan sıcaklığı ---
    public const float MaxTemperature = 100f;
    public const float StartTemperature = 60f;
    public const float HeatRate = 5f;   // Tam hızda saniyede kazanılan ısı
    public const float CoolRate = 2f;   // Dondurucu havanın saniyede çaldığı ısı
    public const float OverheatThreshold = 92f;
    public const float StationTemperatureBoost = 10f; // Arthur durakta kazanı yeniden körüklüyor

    // --- Tren sağlığı ---
    public const float MaxHealth = 100f;
    public const float DamagedSpeedFloor = 0.5f; // Sağlık 0'a yaklaştıkça azami hız %50'ye iner

    // --- Yolcular ---
    public const int CabinCapacity = 3;
    public const float PassengerCoolingReduction = 0.15f; // Her yolcu soğumayı %15 yavaşlatır (vücut ısısı)
    public const float PassengerFuelIncrease = 0.05f;     // Her yolcu km başı tüketimi %5 artırır (ağırlık)
    public const float StokerHeatBonus = 0.25f;           // Ateşçi kazanı %25 daha verimli besler
    public const float EngineerStationRepair = 15f;       // Mühendis her istasyonda treni onarır
    public const float EngineerDamageMultiplier = 0.5f;   // Mühendis varken buz hasarı yarıya iner

    // --- Fırtına ---
    public const float StormCoolingBonus = 0.4f;          // Tam fırtınada kazan %40 daha hızlı soğur

    // --- Rastgele yol olayları ---
    public const float FirstEventDelay = 15f;
    public const float EventCooldownMin = 18f;
    public const float EventCooldownMax = 35f;
    public static float EventSafetyMarginKm => Near(4f);  // Durağa bu kadar (+ApproachKm) kala yeni olay başlamaz
    public const float EventResultDisplay = 2.5f;

    public const float IceSafeSpeed = 40f;
    public const float IceDuration = 7f;
    public const float IceGracePeriod = 2.5f;             // Hasar başlamadan önce tepki süresi
    public const float RadioExtraGrace = 2f;              // Radyocu buzu erken haber verir
    public const float IceDamagePerSecond = 16f;

    public const float ValveReactionWindow = 4f;      // Basınç ibresi mini oyununun süresi
    public const float ValveGreenWidth = 0.16f;       // Yeşil bölgenin genişliği (gösterge oranı)
    public const float ValveBurstDamage = 8f;         // Yanlış zamanlama ya da süre aşımı: vana patlar
    public const float LeakDuration = 4f;
    public const float LeakHeatDrainPerSecond = 7f;

    public const float SupplyCacheSpeed = 30f;            // Yük vagonundan malzeme almak için hız sınırı
    public const float SupplyCacheWindow = 6f;

    // --- Envanter ve eşyalar ---
    public const int InventorySlots = 4;
    public const int MaxStackPerSlot = 3;
    public const float SparePartsRepair = 25f;
    public const float ThermalCoalFuel = 30f;
    public const float ThermalCoalHeat = 30f;

    // --- Yolcu sağlığı (canlılık) ---
    public const float ColdThreshold = 30f;               // Kazan bunun altındaysa kabin soğur, yolcular üşür
    public const float WarmThreshold = 45f;               // Kazan bunun üstündeyse yolcular toparlanır
    public const float VitalityRecoveryPerSecond = 1.5f;
    public const float CriticalVitality = 30f;            // Altında yolcu "Donmak Üzere" sayılır

    // --- Paket ---
    public const float MaxPackage = 100f;
    public const float PackageDamageShare = 0.6f;         // Trenin aldığı hasarın paketi de sarsan oranı (onarılamaz!)
    public const float LowPackageWarning = 40f;

    // --- Eğitim (Koca Thomas) ---
    public const float TutorialSpeedUpTarget = 50f;   // Adım 1: bu hıza çık
    public const float TutorialSlowDownTarget = 30f;  // Adım 2: bu hızın altına in

    // --- Finaller ---
    public const int GoodEndingMinPassengers = 2;

    // --- Rota: kasabalar ve buzul benzinlikleri (katedilen km) ---
    // İlk 4 istasyon hikâye senaryosudur; sonrakilerde peronda rastgele bir yabancı bekler.
    // Hikâye bölümlerinin (10, 17, 25, 31, 37, 39) kilometre aralıklarında durak yoktur: o bölümlerde tren sürülmez.
    public static readonly float[] StationKms = { 200f, 400f, 600f, 780f, 1000f, 1180f, 1400f, 1600f, 1780f };
    // Kasabalar sırasıyla: Ticaret Merkezi, Madenci Köyü, Hayalet İstasyon, Buzul Tarikatı, Terk Edilmiş Karakol, Askeri Sınır Kapısı
    public static readonly float[] TownKms = { 250f, 520f, 720f, 1080f, 1330f, 1620f };
    public static readonly float[] DepotKms = { 110f, 440f, 900f, 1150f, 1450f, 1880f };
    public static readonly float[] HubKms = { 300f, 700f, 1050f, 1360f, 1700f };      // Büyük ana istasyonlar (pazar + geliştirmeler)

    // --- Episode 2 "Project AURA": Kutup Hattı ---
    public static readonly float[] Episode2StationKms = { 200f, 560f, 820f, 1060f, 1320f, 1730f };
    public static readonly float[] Episode2TownKms = { 330f, 680f, 1265f };        // İnsansız karakol, uyku fabrikası, krom şapel
    public const int Episode2FirstTown = 6;                                           // TownCatalog'da Episode 2 kasabalarının ilk sırası
    public static readonly float[] Episode2DepotKms = { 130f, 420f, 880f, 1180f, 1440f, 1760f, 1900f };
    public static readonly float[] Episode2HubKms = { 260f, 640f, 1120f, 1600f };
    public const int Episode2ChapterReward = 12;

    // --- Episode 2: AURA sinyal hack'i ve Sentinel trenleri ---
    public const float HackTimeLimit = 14f;            // Dalga boyunu eşleştirmek için süre
    public const float HackLockTime = 1.4f;            // Eşleşmiş dalgayı bu kadar tutunca hack başarılı
    public const float HackFreqTolerance = 0.28f;      // Frekans farkı toleransı
    public const float HackAmpTolerance = 0.09f;       // Genlik farkı toleransı
    public const float HackFreqSpeed = 1.6f;           // Ok tuşuyla frekans değişim hızı (birim/sn)
    public const float HackAmpSpeed = 0.55f;
    public static float SignalTowerLeadKm => Near(8f); // Sinyal kulesi ekranın bu kadar önünde belirir
    public const float SignalTowerGapKm = 22f;         // Kuleler arası en az mesafe
    public const int TowerHackCredits = 20;
    public const float TowerJamDamage = 6f;            // Hacklenmeyen kule: parazit treni sarsar
    public const float TowerJamSanity = 8f;
    public const float SceneHackDifficulty = 0.15f;  // Episode 2 Bölüm 2: kulenin terminali (ilk hack, öğretici zorlukta)
    public const float SentinelDuration = 34f;         // Sentinel'i bu kadar atlatınca kaçılır
    public const float SentinelChargeTime = 4.2f;      // Taret ateş etmeden önce şarj olur (kırmızı çizgi uyarısı)
    public const float SentinelShotDamage = 9f;
    public const float SentinelLockDuration = 12f;     // Hack: silahlar kilitli kalır
    public const float SentinelBlindMiss = 0.7f;       // Hack: kör taretlerin ıskalama oranı
    public const int SentinelEscapeCredits = 35;
    public const float PanzerArmorMultiplier = 0.6f;   // Heavy Panzer Express: darbe hasarı %40 az

    // --- Hikâye güncellemesi: tehlike dengesi ---
    public const float CalmHazardScale = 0.3f;      // Kritik olmayan bölümde vana patlaması ve kurt sürüsü %70 seyrek
    public const float CalmAmbushScale = 0.25f;     // Kritik olmayan bölümde pusu sayısı %75 az
    public const float ExtremeStorm = 0.8f;         // Bu fırtına yoğunluğunun üstünde tehlikeler tam sıklıkta
    public const float CriticalAmbushDelayMin = 3f; // Kritik bölümde ilk pusu bölümün başında kurulur
    public const float CriticalAmbushDelayMax = 6f;
    public const float AmbushChapterLeadKm = 10f;   // Bölüm başından bu kadar km sonra pusu kurulabilir
    public const float CultSanityDrain = 0.7f;      // Tarikatın kutsaması: akıl sağlığı aşınması %30 yavaş
    public const float WorkersGiftFuel = 25f;       // Demirhan işçilerinin bıraktığı yakıt
    public const int QuestStepChapters = 2;         // Yolcu görevinin her adımı arasında geçen bölüm

    // --- Kredi ekonomisi (kalıcı para birimi, SaveData) ---
    public const int FuelPackagePrice = 15;         // Pompa ile en fazla FuelPackageAmount yakıt
    public const float FuelPackageAmount = 30f;
    public const int RepairPrice = 20;
    public const float RepairAmount = 30f;
    public const int CoalPrice = 18;
    public const int MedkitPrice = 22;
    public const int PartsPrice = 20;
    public const float TraderSellBonus = 1.25f;     // Tüccar yolcu: yağma %25 daha pahalıya satılır
    public const float TraderBuyDiscount = 0.8f;    // Tüccar yolcu: alışlar %20 ucuz
    public const int MechanicCreditCost = 25;
    public const float MechanicCreditRepair = 30f;
    public const float MechanicPartsRepair = 40f;   // Usta eliyle takılan parça, kendin taktığından iyi
    public const int CargoCapacity = 10;
    public const int StationLoot = 2;              // İstasyon deposunda bulunan yağma
    public const float HubRepair = 15f;             // Ana istasyon atölyesi treni ücretsiz biraz onarır
    public const int ChapterReward = 8;             // Her bölüm tamamlandığında (40 kısa bölüm)
    public const int ShelterReward = 120;           // Sığınak-0 teslimatı

    // --- Pompa mini oyunu (ritmik SPACE) ---
    public const float PumpDuration = 7f;
    public const float PumpTapBoost = 0.16f;
    public const float PumpDecayPerSecond = 0.38f;
    public const float PumpGreenMin = 0.45f;
    public const float PumpGreenMax = 0.75f;
    public const float PumpFillSeconds = 3.5f;      // İbre yeşildeyken depo bu sürede dolar
    public const float PumpMinimumYield = 0.3f;

    // --- Mafya pusuları ---
    public const float AmbushFirstKm = 120f;
    public const float AmbushCooldownMin = 55f;
    public const float AmbushCooldownMax = 100f;
    public static float AmbushSafetyKm => Near(15f); // Durağa bu kadar (+ApproachKm) kala pusu kurulmaz
    public const float ChaseIntroDuration = 1.8f;
    public const float ChaseDuration = 20f;
    public const float BanditStartDistance = 0.75f; // 1 = çok uzakta, 0 = trene yetişti
    public const float BanditBaseClose = 0.045f;
    public const float BanditSpeedClose = 0.10f;    // Yavaş giden tren daha çabuk yakalanır
    public const float PressurePerSecond = 0.14f;   // Tam gazda kazan basıncı birikimi
    public const float BlastThreshold = 0.5f;
    public const float BlastPush = 0.25f;
    public const float BlastHeatCost = 6f;
    public static readonly float[] ObstacleTimes = { 4f, 10f, 15f };
    public const int ComboLength = 3;
    public const float ComboWindow = 2.6f;
    public const float IntelExtraWindow = 1f;       // Taverna istihbaratı: daha uzun tepki süresi
    public const float IntelStartDistance = 0.9f;
    public const float ObstacleDamage = 12f;
    public const float TheftFuel = 25f;
    public const int TheftCargo = 3;                // Yakalanınca çalınan yağma sayısı
    public const float CaughtDamage = 10f;
    public const int EscapeLoot = 2;                // Kaçarken haydutların düşürdüğü yağma
    public const float GuardCloseMultiplier = 0.75f; // Asker yolcu: mafya daha yavaş yaklaşır, daha az çalar
    public const float ChaseFuelBurnPerSecond = 0.4f;

    // --- Bölüm 3: buzul çöküşü (zamanla yarış) ---
    public const float CollapseSpeed = 62f;         // Çöküş cephesinin hızı (km/sa karşılığı)
    public static float CollapseStartGapKm => Near(14f);
    public static float CollapseMaxGapKm => Near(20f);
    public static float CollapseMinGapAfterStop => Near(9f);
    public const float CollapseDamagePerSecond = 7f;

    // --- Kasaba: sığınak girişi ---
    public const float BunkerVitality = 40f;          // Sığınakta dinlenen yolcuların toparlanması
    public const float BunkerHeat = 20f;              // Sığınağın kazan dairesinden ısı

    // --- Geçişler ---
    public const float FadeHalfDuration = 0.5f;       // Siyaha kararma / açılma süresi

    // --- Kasaba (yürüyüş modu) ---
    public const int TownWidth = 3000;
    public const float WalkSpeed = 220f;
    public const float InteractRange = 70f;

    // --- Uyarı eşikleri ---
    public const float LowFuelWarning = 25f;
    public const float LowTemperatureWarning = 25f;
    public const float LowHealthWarning = 30f;

    // ==================================================================
    // Tam sürüm
    // ==================================================================

    // --- Oyun modları ---
    public const float EndlessStageKm = 250f;          // Sonsuz modda her aşama (tema + zorluk artışı)
    public const int EndlessCreditsPerKm = 25;         // Sonsuz mod: her 25 km için 1 Kredi
    public const float SpeedrunDistanceKm = 450f;
    public static readonly float[] SpeedrunDepotKms = { 120f, 240f, 360f };
    public const float SpeedrunTimeLimit = 210f;       // Saniye
    public const float SpeedrunFuelMultiplier = 1.5f;  // Yakıt hızlı tükenir
    public const float SpeedrunQuickRefuel = 25f;      // Benzinliğe varışta ücretsiz hızlı ikmal
    public const int SpeedrunBaseReward = 50;

    // --- Gece/gündüz döngüsü ---
    public const float DayLength = 160f;               // Bir tam gün (saniye)
    public const float NightStart = 0.55f;             // Günün bu oranından sonra gece başlar
    public const float NightEnd = 0.95f;
    public const float NightDarkness = 0.42f;

    // --- Kurt sürüsü ---
    public const float WolfDuration = 9f;
    public const float WolfBiteInterval = 1.4f;
    public const float WolfBiteDamage = 3.5f;
    public const float WolfBiteVitality = 6f;
    public const float WolfSmokeHeatCost = 6f;
    public const float WolfSmokeScare = 0.55f;
    public const float WolfOutrunSpeed = 90f;          // Bu hızın üstünde sürü yetişemez

    // --- Çökmüş ray makası ---
    public const float SwitchWindow = 4.5f;
    public const float SwitchSafeSpeed = 25f;          // Bu hızın altında ray kontrol edilir, kaza olmaz
    public const float SwitchCrashDamage = 18f;
    public const float SwitchHintDelay = 1.6f;         // Düşük görüşte kırık ray işareti geç seçilir

    // --- Tipi / çığ ---
    public const float BlizzardDuration = 8f;
    public const float BlizzardHeatDrop = 12f;
    public const float AvalancheSafeSpeed = 50f;
    public const float AvalancheRiskRate = 0.5f;
    public const float AvalancheDamage = 22f;
    public const float AvalancheHeatLoss = 10f;

    // --- Maden girişi ve mini oyun ---
    public const float MineEntranceWindow = 7f;
    public const float MineEnterSpeed = 30f;
    public const float MineDuration = 30f;
    public const float MineHitCooldown = 0.16f;

    // --- İhanet ---
    public const float SabotageCooldownMin = 16f;
    public const float SabotageCooldownMax = 28f;
    public const float SabotageFuel = 12f;
    public const float PoisonDrainPerSecond = 1.6f;
    public const float GuardCatchChance = 0.45f;

    // --- Kasaba riskleri ---
    public const float ThiefChance = 0.45f;
    public const float ThiefWalkSpeed = 70f;
    public const float ThiefStealTime = 4.5f;
    public const float PeddlerScamChance = 0.5f;
    public const int PeddlerFuelPrice = 8;
    public const float PeddlerFuelAmount = 30f;
    public const float PeddlerScamFuel = 5f;
    public const int PeddlerPartsPrice = 10;

    // --- Yeni eşyalar ve geliştirmeler ---
    public const int RareMedkitPrice = 55;
    public const int WolfBaitPrice = 8;
    public const float BattleArmorMultiplier = 0.5f;   // Mafya ve kurt hasarı yarıya iner
    public const float LanternExtraGrace = 1.5f;       // Gelişmiş fener: tehlike erken görülür

    // ================= Atmosfer, kabin yaşamı, vagonlar =================

    // --- Işık ve karanlık ---
    public const float MaxSceneDarkness = 0.9f;        // Zifiri gece/tipi: ekranın en fazla bu kadarı kararır
    public const float NightLightDarkness = 0.82f;     // Gece yarısı (fener konisi dışında) karanlık
    public const float StormDarkness = 0.45f;          // Tipi bastırdığında eklenen karanlık
    public const float SevereStormIntensity = 0.7f;    // Bu fırtına şiddetinin üstünde azami hız düşer
    public const float StormSpeedMultiplier = 0.7f;    // Şiddetli fırtına/tipide azami hız %30 düşer

    // --- Fener arızası ---
    public const float LampFailureWeight = 1.6f;       // Karanlıkta (gece, tünel, tipi) olay ağırlığı
    public const float LampFailureDuration = 18f;      // Bu sürede tamir edilmezse tren karanlıkta bir şeye çarpar
    public const int LampRepairHits = 4;               // Tamir için gereken [E] vuruşu
    public const float LampCrashDamage = 10f;

    // --- Kabin yaşamı: açlık, sıcaklık, soba, erzak ---
    public const float HungerPerSecond = 0.55f;        // Tok yolcu ~3 dakikada acıkır
    public const float RationFeed = 60f;               // Bir erzak açlığı bu kadar giderir
    public const int StartRations = 4;
    public const int MaxRations = 12;
    public const int RationPack = 3;                   // Tüccardan bir alımda gelen erzak
    public const int RationPackPrice = 9;
    public const float StoveBurnPerSecond = 0.9f;      // Soba ~2 dakikada söner
    public const float StoveStokeAmount = 45f;
    public const float StoveStokeFuel = 3f;            // Sobaya atılan kömür trenin yakıtından gelir
    public const float WarmthChangePerSecond = 1.4f;
    public const float NeedRequestThreshold = 35f;     // Bunun altında yolcu erzak/ısı ister
    public const float NeglectVitalityDrain = 0.5f;    // Aç ya da üşüyen yolcunun canlılık kaybı (sn başına)
    public const float SicknessDelay = 25f;            // Bu kadar uzun üşüyen yolcu hastalanır
    public const float DesertionDelay = 25f;           // Bu kadar uzun aç kalan yolcu treni terk eder (ölmeden önce)

    // --- Modüler vagonlar ---
    public const int CargoWagonBonus = 10;
    public const int PassengerWagonBeds = 3;
    public const float ArmoredWagonHealth = 50f;
    public const float ArmoredWagonAmbushMultiplier = 0.7f;

    // --- Ekran sallantısı ---
    public const float DamageShakePerPoint = 0.6f;     // Alınan hasarın her puanı için sarsıntı (piksel)
    public const float MaxScreenShake = 12f;

    // ================= DLC =================

    // --- Buzul Kıyameti: 5 bölüm × 100 km'lik ayrı hat ve asit fırtınası ---
    public const float WastelandChapterKm = 100f;
    public const float WastelandDistanceKm = 500f;
    public static readonly float[] WastelandStationKms = { 60f, 260f, 440f };
    public static readonly float[] WastelandTownKms = { 150f, 350f };
    public static readonly float[] WastelandDepotKms = { 95f, 210f, 310f, 400f };
    public static readonly float[] WastelandHubKms = { 240f, 470f };
    public const float AcidCorrosionPerSecond = 0.12f;  // Zehirli kar: tema boyunca gövde sürekli aşınır
    public const float AcidStormDrainPerSecond = 1.1f;  // Asit fırtınası sırasında aşınma
    public const float AcidStormDuration = 16f;
    public const float AcidEscapeSpeed = 75f;           // Bu hızın üstünde fırtınadan iki kat hızlı çıkılır
    public const float AcidVitalityDrain = 0.6f;        // Yolculara da sızar
    public const int WastelandChapterReward = 30;
    public const int WastelandClearReward = 250;

    // --- Acımasız Kış Sınavı (Permadeath) ---
    public const float HardcoreResourceMultiplier = 0.5f;
    public const float HardcoreSabotageMultiplier = 2f;

    // ================= Akıl sağlığı, telsiz, karma =================

    // --- Arthur'un akıl sağlığı ve yolcu morali (0-100) ---
    public const float HallucinationThreshold = 30f;   // Altında hayaletler, sahte pusular, kızıl illüzyonlar
    public const float SanityNightDrain = 0.22f;       // Gece sürüşü (sn başına)
    public const float SanityLampDrain = 0.9f;         // Arızalı farla zifiri karanlık
    public const float SanityColdDrain = 0.3f;         // Kazan soğukken
    public const float SanityHungerDrain = 0.2f;       // Kabinde aç yolcular varken
    public const float SanityDayRecovery = 0.06f;      // Gündüz yavaşça toparlanır
    public const float MoraleNightDrain = 0.12f;
    public const float MoraleNeedDrain = 0.35f;        // Aç ya da üşüyen yolcu
    public const float MoraleDesertion = 12f;          // Bunun altında uzun kalan yolcu treni terk eder
    public const float PetRegenPerSecond = 10f / 60f;     // Evcil hayvan: dakikada %10 akıl ve moral
    public const float MusicBoxDuration = 20f;
    public const float MusicBoxCooldown = 45f;
    public const float MusicBoxSanityPerSecond = 2.2f;
    public const float MusicBoxMoralePerSecond = 2.5f;
    public const int TavernSoupPrice = 6;
    public const float TavernSoupSanity = 35f;
    public const float TavernSoupMorale = 25f;
    public const float RadioMusicSanityPerSecond = 0.8f; // Eski dünya şarkıları

    // --- Telsiz ve imdat görevleri ---
    public const float FmMin = 88f, FmMax = 108f, FmStep = 0.1f;
    public const float AmMin = 530f, AmMax = 1600f, AmStep = 10f;
    public const float SosMinGapKm = 25f;              // Sıradaki durağa bu kadardan az kaldıysa imdat noktası kurulamaz
    public const float SosLeadMinKm = 12f, SosLeadMaxKm = 20f;

    // --- Karma (gizli) ---
    public const int SacrificeKarma = 60;              // Gizli Feda sonu için gereken karma
    public const int MerchantReward = 400;             // Paketi mafyaya satmanın bedeli

    // --- Başarımlar ---
    public const int MillionaireCredits = 1000;
    public const int LeaderboardSize = 10;
}
