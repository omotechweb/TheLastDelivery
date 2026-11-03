using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TheLastDelivery.Audio;
using TheLastDelivery.Chase;
using TheLastDelivery.Core;
using TheLastDelivery.Economy;
using TheLastDelivery.Events;
using TheLastDelivery.Graphics;
using TheLastDelivery.Items;
using TheLastDelivery.Mining;
using TheLastDelivery.Modding;
using TheLastDelivery.Localization;
using TheLastDelivery.Passengers;
using TheLastDelivery.Platform;
using TheLastDelivery.Story;
using TheLastDelivery.Towns;
using TheLastDelivery.Trade;
using TheLastDelivery.UI;
using TheLastDelivery.World;

namespace TheLastDelivery;

/// <summary>
/// The Last Delivery — Büyük Donma'dan 20 yıl sonra, son hat kuryesi Arthur ve
/// lokomotifi Old Smokie'nin Sığınak-0'a uzanan 1000 kilometrelik yolculuğu.
///
/// Game1 yalnızca orkestra şefidir: durum makinesini yürütür, alt sistemleri günceller
/// ve çizim sırasını belirler. Asıl iş World/, Passengers/, Items/, Events/, Audio/, UI/
/// ve Graphics/ altındaki sınıflardadır.
/// </summary>
public class Game1 : Game, INarrativeHost
{
    private static readonly Color NightColor = new(6, 8, 20); // Fırtınalı gece laciverti
    private const float SupplyWagonLeadKm = 5f;                // Yük vagonu ekranın bu kadar önünden gelir

    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    // Dünya
    private ParallaxLayer _mountains;   // En arka: yavaş kayan dağlar
    private ParallaxLayer _pines;       // Orta: kurumuş çamlar
    private ParallaxLayer _ground;      // Ön: karlı zemin ve traversler
    private SnowSystem _snow;
    private Train _train;
    private readonly Weather _weather = new();

    // Oynanış sistemleri
    private readonly Cabin _cabin = new();
    private readonly Inventory _inventory = new();
    private readonly RoadEventSystem _events = new();
    private readonly DialogBox _dialog = new();
    private readonly ToastFeed _toasts = new();
    private readonly Tutorial _tutorial = new();
    private ProceduralAudio _audio;

    // Yerleşimler ve aksiyon
    private enum TownDialog { None, Tavern, Mechanic, Store, Bunker, Peddler, Result }

    private readonly TownScene _townScene = new();
    private readonly TraderScreen _trader = new();
    private readonly MetaMenu _meta = new();
    private readonly ChaseSequence _chase = new();
    private readonly AmbushScheduler _ambush = new();
    private TownDialog _townDialog;
    private bool[] _gossipHeard = new bool[TownCatalog.All.Length];     // Kasaba başına bir kez dedikodu
    private bool[] _passengerHired = new bool[TownCatalog.All.Length];  // Kasaba başına bir yolcu adayı
    private bool[] _bunkerUsed = new bool[TownCatalog.All.Length];      // Kasaba başına bir kez sığınakta dinlenme
    private bool[] _npcGiftGiven = new bool[TownCatalog.All.Length];    // Kasaba NPC'si bir kez yağma hediye eder

    // Bölüm temaları
    private float _collapseGapKm;       // Bölüm 3: buzul çöküşünün trenin ne kadar gerisinde olduğu
    private float _collapseShake;       // Çöküş yetiştiğinde ekran sarsıntısı
    private float _decorScroll;         // Tünel destekleri ve eriyik su birikintileri için kayma
    private readonly ParticleSystem _themeParticles = new(); // Düşen buz parçaları, damlayan su
    private readonly Random _rng = new();

    // Siyaha kararma geçişi: ilk yarıda kararır, ortada sahne değişir, ikinci yarıda açılır
    private float _fadeTimer = -1f;
    private Action _fadeMidpoint;
    private bool _fadeMidpointDone;
    private bool _ambushIntel;                                          // Tavernada duyulan istihbarat (sonraki pusuya hazırlık)

    // Oyun modu (hikâye, sonsuz kurye, zamanla yarış)
    private GameMode _mode = GameMode.Story;
    private readonly EndlessRoute _endlessRoute = new();
    private int _endlessStage = 1;
    private float _speedrunTime;        // Zamanla yarışta geçen süre (saniye)

    // Tam sürüm sistemleri
    private readonly BetrayalSystem _betrayal = new();
    private readonly CabinMenu _cabinMenu = new();
    private readonly MineMinigame _mine = new();
    private readonly DayCycle _dayCycle = new();
    private StationScenario _currentScenario;   // İstasyondaki senaryo (hikâye ya da peronda bekleyen yabancı)
    private float _mineKm = -1f;                // Yol kenarındaki maden girişinin konumu (yoksa -1)
    private float _wolfSmokeFlash;              // Kurtlara duman püskürtme anının görsel geri bildirimi

    // Atmosfer, kabin yaşamı ve cila
    private AudioManager _audioManager;         // Sahneye göre motor/ray/rüzgar/müzik karışımı
    private readonly LightMap _lightMap = new(); // Gece, tünel ve tipi karanlığı; far konisi aydınlatır
    private float _screenShake;                 // Hasar ve pusuda ekran sallantısı (piksel)
    private float _lastHealth;                  // Hasarı fark etmek için önceki karedeki sağlık
    private bool _stormSlowed;                  // Şiddetli fırtına hız sınırı bildirildi mi?
    private bool _stoveWarned;                  // "Soba sönüyor" uyarısı bildirildi mi?
    private RoadEventType? _lastEventType;      // Biten olayın türü (başarım sayaçları için)

    // Akıl sağlığı, telsiz, gizli karma ve sonlar
    private readonly SanitySystem _sanity = new();
    private readonly DriveChatter _chatter = new();             // Sürüşte kısa telsiz/yolcu/hatıra satırları
    private readonly RadioSystem _radio = new();
    private readonly RadioPanel _radioPanel = new();
    private int _karma;                         // Gizli ahlak puanı: iyilik +, kötülük - (oyuncuya gösterilmez)
    private bool _mafiaDeal;                    // Telsizde Kızıl Kurtlar'ın teklifi kabul edildi (Tüccar sonu)
    private float _sosKm = -1f;                 // Yanıtlanan imdat çağrısının yeri (yoksa -1)
    private SosMission _sosType;
    private bool _atSos;                        // Şu an bir imdat noktasında duruluyor
    private float _ghostTrainTimer, _redVisionTimer, _fakeAmbushTimer; // Sanrıların süreleri
    private readonly List<Action> _finalActions = new(); // Final seçimindeki seçeneklerin eylemleri

    // Canlı içerik: görevler, garaj, evcil hayvan, fotoğraf modu, dönemsel etkinlikler
    private readonly GarageScreen _garage = new();
    private readonly PhotoMode _photo = new();
    private bool _chapterHealthLost;            // Bu bölümde hasar alındı mı? ("Hasarsız bölüm" görevi)
    private float _contractFlushTimer;
    private PetKind Pet => (PetKind)DLCManager.Active(CosmeticSlot.Pet).Value;

    // Hikâye güncellemesi: diyalog düğümleri, keşif/diyalog bölümleri, yolcu görev zincirleri
    private readonly StoryRunner _story = new();
    private readonly Dictionary<string, int> _boardedAt = new(); // Yolcu kimliği → trene bindiği bölüm
    private string _questNotified;                               // Son haber verilen görev ("q.viktor.2")

    // Episode 2 "Project AURA": sinyal hack'i, Sentinel trenleri, DLC kilidi, Episode 1 kapanışı
    private readonly SignalHack _hack = new();
    private readonly SentinelEncounter _sentinel = new();
    private readonly DlcGatePanel _gate = new();
    private readonly EpisodeOutro _outro = new();
    private int _gateNextChapter = -1;                           // DLC açılınca devam edilecek bölüm (sıra)
    private float _gateRefreshTimer;
    private float _towerKm = -1f;                                // Hattın kenarındaki AURA sinyal kulesi (yoksa -1)
    private float _nextTowerKm;
    private bool _towerHacking;                                  // Açık hack bir sinyal kulesine mi ait?
    private NarrativeEffect _sceneHack;                          // Hikâye sahnesinde bekleyen/açık hack (yoksa null)
    private bool _sceneHacking;

    // Oyun durumu
    private GameState _state;
    private StationPhase _stationPhase;
    private bool _isPaused;
    private float _stateTimer;          // Mevcut durumda geçen süre (geçiş efektleri için)
    private float _travelledKm;
    private int _nextStopIndex;         // Sıradaki durağın Route.Stops içindeki sırası
    private int _chapterIndex;          // Aktif bölüm (Chapters.All içindeki sıra)
    private int _abandonedCount;        // Geride bırakılan insan sayısı
    private int _lostCount;             // Yolda soğuktan ölen yolcu sayısı
    private string _arrivalNote;        // Varışta yaşanan olay (örn. mühendisin onarımı)
    private float _supplyWagonKm = -1f; // Yol kenarındaki yük vagonunun konumu (yoksa -1)
    private Ending _ending;
    private float _time;

    // Dil seçimi
    private int _languageCursor;        // Dil seçim ekranında işaretli satır
    private bool _languageChosen;       // Bu oturumda dil seçildi mi? (ESC menüye mi döner, oyundan mı çıkar)

    private KeyboardState _previousKeys;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = GameConfig.ScreenWidth,
            PreferredBackBufferHeight = GameConfig.ScreenHeight,
            SynchronizeWithVerticalRetrace = true
        };
        IsFixedTimeStep = false;
        IsMouseVisible = true;
        Window.Title = "The Last Delivery";
        Window.TextInput += (_, e) => _meta.OnTextInput(e.Character); // DENEME KODU alanı için klavye girişi
    }

    // ==================================================================
    // Yükleme
    // ==================================================================

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        Assets.Load(GraphicsDevice);
        Narrative.AboardCheck = id => _cabin.HasProfile(id); // "aboard:oskar" gibi diyalog koşulları

        // Parallax katmanları: hız çarpanı 0.1 / 0.4 / 1.0
        _mountains = new ParallaxLayer(
            TextureFactory.CreateMountainLayer(GraphicsDevice, 1280, 280,
                color: new Color(16, 20, 34), rimColor: new Color(60, 70, 96), fogColor: new Color(30, 36, 58)),
            speedFactor: 0.1f, y: GameConfig.GroundY - 260);

        _pines = new ParallaxLayer(
            TextureFactory.CreateDeadPineLayer(GraphicsDevice, 1024, 200, new Color(9, 11, 19), seed: 7),
            speedFactor: 0.4f, y: GameConfig.GroundY - 180);

        _ground = new ParallaxLayer(
            TextureFactory.CreateSnowGround(GraphicsDevice, 1024, GameConfig.ScreenHeight - GameConfig.GroundY,
                railRow: GameConfig.RailY - GameConfig.GroundY, seed: 42),
            speedFactor: 1f, y: GameConfig.GroundY);

        _snow = new SnowSystem(650, GameConfig.ScreenWidth, GameConfig.ScreenHeight);
        _train = new Train();

        _audio = new ProceduralAudio();
        _audioManager = new AudioManager(_audio);
        _events.Started += OnRoadEventStarted;
        _events.LampHit += OnLampHit;
        _events.Finished += OnRoadEventFinished;
        _events.LootCollected += OnLootCollected;
        _events.MineEntered += EnterMine;
        _events.WolvesRepelled += () =>
        {
            SaveData.Record(s => s.WolvesRepelled++);
            ReportContract(ContractKind.RepelWolves, 1);
            CheckAchievements();
        };
        _events.SmokeBlasted += OnWolfSmoke;
        _trader.UpgradePurchased += ApplyProfile; // Ana istasyonda alınan geliştirme hemen trene işlenir

        SaveData.Load(); // Kredi, açılan bölümler, geliştirmeler, skinler

        // Steam entegrasyonu: Steamworks bağlandığında burada SteamPlatform takılır; şimdilik çevrimdışı sanal platform
        AchievementManager.Initialize(new OfflinePlatform());
        ModLoader.LoadAll();  // Topluluk modları ve Steam Workshop öğeleri (skin, diyalog, durak, metin)
        ContractManager.Refresh(); // Yeni gün/hafta: günlük ve haftalık görevler
        DLCManager.Refresh(); // DLC sahipliği açılışta bir kez sorgulanır (oyun döngüsünde platforma çağrı yok)
        ResetGame();
        OpenLanguageSelect(); // Oyun her açılışta dil seçimiyle başlar; son seçilen dil işaretli gelir
    }

    protected override void UnloadContent()
    {
        _events.Started -= OnRoadEventStarted;
        _events.Finished -= OnRoadEventFinished;
        _events.LampHit -= OnLampHit;
        ContractManager.Flush();
        _photo.Dispose();
        _audioManager?.Dispose();
        _lightMap.Dispose();
        _mountains?.Dispose();
        _pines?.Dispose();
        _ground?.Dispose();
        Assets.Unload();
        _spriteBatch?.Dispose();
        base.UnloadContent();
    }

    /// <summary>
    /// Tam sıfırlama: mesafe, kaynaklar, yolcular, envanter, olaylar, hava, bildirimler
    /// ve final bilgisi temizlenir; oyun Ana Menü'ye döner.
    /// </summary>
    private void ResetGame()
    {
        _travelledKm = 0f;
        _nextStopIndex = 0;
        _chapterIndex = 0;
        _abandonedCount = 0;
        _lostCount = 0;
        _arrivalNote = null;
        _supplyWagonKm = -1f;
        _ending = null;
        _isPaused = false;

        ApplyProfile(); // Kalıcı geliştirmeler ve skin trene uygulanır (Reset yakıtı yeni kapasiteye doldurur)
        _train.Reset();
        _cabin.Clear();
        _inventory.Clear();
        _events.Reset();
        _weather.Reset();
        _toasts.Clear();

        Difficulty.SetNormal(); // Acımasız Kış modu kendi çarpanlarını ResetGame'den sonra kurar
        GiveStartingKit();
        _screenShake = 0f;
        _lastHealth = _train.Health;
        _stormSlowed = false;
        _stoveWarned = false;
        _lastEventType = null;
        _sanity.Reset();
        _radioPanel.Reset();
        _radio.MafiaOfferHeard = false;
        _karma = 0;
        _mafiaDeal = false;
        _chatter.ResetRun();
        _sosKm = -1f;
        _atSos = false;
        _ghostTrainTimer = _redVisionTimer = _fakeAmbushTimer = 0f;
        _collapseGapKm = GameConfig.CollapseStartGapKm;
        _npcGiftGiven = new bool[TownCatalog.All.Length];
        _boardedAt.Clear();
        _questNotified = null;
        _sanity.DrainMultiplier = 1f;
        _events.HazardScale = 1f;
        Narrative.Episode = 1;
        _hack.Close();
        _towerKm = -1f;
        _nextTowerKm = 0f;
        _towerHacking = false;
        _sceneHack = null;
        _sceneHacking = false;
        _gateNextChapter = -1;

        // Tam sürüm: mod, ihanet, gece, maden
        SetMode(GameMode.Story);
        _endlessStage = 1;
        _speedrunTime = 0f;
        _betrayal.Reset();
        _cabinMenu.Close();
        _dayCycle.Reset();
        _currentScenario = null;
        _mineKm = -1f;
        _wolfSmokeFlash = 0f;

        // Kasabalar ve mafya
        _ambush.Reset();
        _ambushIntel = false;
        _townDialog = TownDialog.None;
        _gossipHeard = new bool[TownCatalog.All.Length];
        _passengerHired = new bool[TownCatalog.All.Length];
        _bunkerUsed = new bool[TownCatalog.All.Length];
        _fadeTimer = -1f;

        _meta.Open(MetaScreen.Main);
        ChangeState(GameState.MainMenu);
    }

    /// <summary>Başlangıç çantası: bir yedek parça, bir termal kömür ve birkaç erzak (zorluğa göre).</summary>
    private void GiveStartingKit()
    {
        _inventory.TryAdd(ItemType.SpareParts);
        _inventory.TryAdd(ItemType.ThermalCoal);
        _inventory.AddRations(Difficulty.Scale(GameConfig.StartRations));
    }

    /// <summary>SaveData profilindeki kalıcı geliştirmeleri ve kuşanılan skini trene ve kargoya uygular.</summary>
    private void ApplyProfile()
    {
        _train.Skin = DLCManager.ActiveSkin(SaveData.Current.EquippedSkin); // DLC'si olmayan skin klasiğe düşer
        _train.FuelCapacity = SaveData.HasUpgrade(UpgradeType.FuelTank) ? UpgradeCatalog.BigTankCapacity : GameConfig.MaxFuel;
        _train.ArmorMultiplier = SaveData.HasUpgrade(UpgradeType.Armor) ? UpgradeCatalog.ArmorDamageMultiplier : 1f;
        _inventory.Cargo.Capacity = GameConfig.CargoCapacity + (SaveData.HasUpgrade(UpgradeType.CargoBay) ? UpgradeCatalog.CargoBayBonus : 0);

        // Gelişmiş Fener: görüş ve erken uyarı; Zırh Kaplama: mafya ve kurt hasarı yarıya iner
        bool lantern = SaveData.HasUpgrade(UpgradeType.Lantern);
        float attack = SaveData.HasUpgrade(UpgradeType.BattleArmor) ? GameConfig.BattleArmorMultiplier : 1f;
        _train.HasLantern = lantern;
        _events.HasLantern = lantern;
        _events.AttackDamageMultiplier = attack;

        // Modüler vagonlar: trenin arkasına eklenir; kargo, yatak, gövde ve pusu zırhı
        // Garajda ayrılan vagonlar trene bağlanmaz
        bool Attached(UpgradeType wagon) => SaveData.HasUpgrade(wagon) && !SaveData.Current.DetachedWagons.Contains(wagon.ToString());
        bool cargoWagon = Attached(UpgradeType.CargoWagon);
        bool passengerWagon = Attached(UpgradeType.PassengerWagon);
        bool armoredWagon = Attached(UpgradeType.ArmoredWagon);
        _train.SetWagons(passengerWagon, cargoWagon, armoredWagon);
        _inventory.Cargo.Capacity += cargoWagon ? GameConfig.CargoWagonBonus : 0;
        _cabin.Capacity = GameConfig.CabinCapacity + (passengerWagon ? GameConfig.PassengerWagonBeds : 0);
        _train.MaxHealth = GameConfig.MaxHealth + (armoredWagon ? GameConfig.ArmoredWagonHealth : 0f);
        _chase.AttackMultiplier = attack * (armoredWagon ? GameConfig.ArmoredWagonAmbushMultiplier : 1f);

        // Heavy Panzer Express: zırh kaplı gövde ve koçbaşı darbe hasarını %40 azaltır
        if (_train.Skin.Model == TrainModel.Panzer)
        {
            _train.ArmorMultiplier *= GameConfig.PanzerArmorMultiplier;
            _chase.AttackMultiplier *= GameConfig.PanzerArmorMultiplier;
        }
        _sentinel.AttackMultiplier = _chase.AttackMultiplier;

        // Evcil hayvan ve dönemsel süsler
        _train.Pet = Pet;
        _townScene.Pet = Pet;
        _train.Season = SeasonalEvents.Current;
    }

    /// <summary>Görev ilerlemesini bildirir; tamamlanan görevin ödülünü duyurur.</summary>
    private void ReportContract(ContractKind kind, float amount)
    {
        foreach (var contract in ContractManager.Report(kind, amount))
        {
            string reward = contract.RewardTickets > 0
                ? Loc.F("contract.rewardBoth", contract.RewardCredits, contract.RewardTickets)
                : Loc.F("contract.rewardCredits", contract.RewardCredits);
            _toasts.Show(Loc.F("contract.done", contract.Description, reward), new Color(120, 220, 255));
            _audio.PlaySuccess();
        }
    }

    /// <summary>Evcil hayvanın sesi: köpek havlar, kedi miyavlar.</summary>
    private void PetCall()
    {
        if (Pet == PetKind.Cat) _audio.PlayMeow();
        else _audio.PlayBark();
    }

    /// <summary>Seçilen bölümden yeni bir hikâye yolculuğu başlatır. Bölüm 1 eğitim sorusuyla başlar.</summary>
    private void StartRun(int chapterNumber)
    {
        ResetGame();
        if (chapterNumber == 1) Narrative.ResetForNewStory(); // Baştan başlayan hikâye: eski seçimler silinir
        var chapter = Chapters.All[chapterNumber - 1];
        _travelledKm = chapter.StartKm;
        _nextStopIndex = Route.FirstStopAfter(chapter.StartKm);
        _chapterIndex = chapterNumber - 1;

        if (chapterNumber == 1)
            ChangeState(GameState.TutorialPrompt);
        else
            ShowChapter(chapterNumber - 1);
    }

    /// <summary>
    /// Episode 2 "Project AURA": Kutup Hattı'nın seçilen bölümünden başlar. Episode 1'den karma ve hayatta kalan
    /// yolcular aktarılır. Bölüm 3 ve sonrası DLC_EPISODE2_UNLOCKED ister; yoksa DLC kilit ekranı açılır.
    /// </summary>
    private void StartEpisode2(int chapterNumber)
    {
        ResetGame();
        SetMode(GameMode.Episode2);
        Narrative.Episode = 2;
        if (chapterNumber == 1) Narrative.ResetForNewStory(); // Episode 2 baştan: Episode 2 kararları silinir

        var chapter = Chapters.Episode2[chapterNumber - 1];
        _travelledKm = chapter.StartKm;
        _nextStopIndex = Route.FirstStopAfter(Route.Episode2Stops, chapter.StartKm);
        _nextTowerKm = chapter.StartKm + 6f;

        // Episode 1'den gelenler: gizli karma ve Sığınak-0'a ulaşan yolcular
        var carry = SaveData.Current.Carry;
        if (carry != null)
        {
            _karma = carry.Karma;
            foreach (string id in carry.Survivors)
            {
                var profile = PassengerRegistry.Find(id);
                if (profile != null && !_cabin.IsFull && !_cabin.HasProfile(id)) _cabin.TryBoard(profile);
            }
        }

        if (chapter.RequiresEpisode2Dlc && !DLCManager.DLC_EPISODE2_UNLOCKED)
            OpenDlcGate(chapterNumber - 1);
        else
            ShowChapter(chapterNumber - 1);
    }

    /// <summary>Episode 2 DLC kilit ekranı: sahibi değilse oyun burada durur.</summary>
    private void OpenDlcGate(int nextChapterIndex)
    {
        _gateNextChapter = nextChapterIndex;
        _train.Stop();
        _events.Clear();
        _gate.Open();
        _gateRefreshTimer = 0f;
        _audio.PlayAlarm();
        ChangeState(GameState.DlcGate);
    }

    private void UpdateDlcGate(float dt, InputState input)
    {
        // Mağaza kaplamasından dönülünce satın alma görülsün: sahiplik birkaç saniyede bir yeniden okunur
        _gateRefreshTimer += dt;
        if (_gateRefreshTimer > 3f)
        {
            _gateRefreshTimer = 0f;
            DLCManager.Refresh();
        }

        switch (_gate.Update(dt, input, _audio))
        {
            case DlcGateAction.OpenStore:
                if (!DLCManager.ViewInStore(DlcId.Episode2)) _gate.ShowMessage(Loc.T("dlc.storeOffline"));
                DLCManager.Refresh();
                break;
            case DlcGateAction.MainMenu:
                ResetGame();
                break;
            case DlcGateAction.Continue when DLCManager.DLC_EPISODE2_UNLOCKED && _gateNextChapter >= 0:
                ShowChapter(_gateNextChapter);
                break;
        }
    }

    /// <summary>Sonsuz kurye: sınırsız hat, her aşamada sertleşen fırtına ve tehlikeler.</summary>
    private void StartEndless()
    {
        ResetGame();
        SetMode(GameMode.Endless);
        _endlessRoute.Reset();
        _endlessStage = 1;
        ShowChapter(0);
    }

    /// <summary>DLC "Buzul Kıyameti": 5 bölümlük ayrı hattın seçilen bölümünden başlar.</summary>
    private void StartWasteland(int chapterNumber)
    {
        if (!DLCManager.Owns(DlcId.FrozenWasteland)) return; // Kilitli içerik asla başlamaz
        ResetGame();
        SetMode(GameMode.Wasteland);
        var chapter = Chapters.Wasteland[chapterNumber - 1];
        _travelledKm = chapter.StartKm;
        _nextStopIndex = Route.FirstStopAfter(Route.WastelandStops, chapter.StartKm);
        ShowChapter(chapterNumber - 1);
    }

    /// <summary>
    /// DLC "Acımasız Kış Sınavı": tek canlı hikâye koşusu. Her zaman 1. bölümden başlar, bölüm seçilemez;
    /// kaynaklar yarıya iner, hain sabotajları iki kat sık olur. Ölüm koşuyu tamamen bitirir.
    /// </summary>
    private void StartHardcore()
    {
        if (!DLCManager.Owns(DlcId.Hardcore)) return;
        ResetGame();
        SetMode(GameMode.Hardcore);
        Difficulty.SetHardcore();
        Narrative.ResetForNewStory(); // Tek canlı koşu her zaman yeni bir hikâyedir
        _inventory.Clear();
        GiveStartingKit(); // Başlangıç çantası da yarı yarıya
        SaveData.Record(s => s.HardcoreRuns++);
        _travelledKm = 0f;
        _nextStopIndex = Route.FirstStopAfter(0f);
        ShowChapter(0);
    }

    /// <summary>Zamanla yarış: kısa hat, hızlı tükenen yakıt ve süre sınırı.</summary>
    private void StartSpeedrun()
    {
        ResetGame();
        SetMode(GameMode.Speedrun);
        _speedrunTime = 0f;
        ShowChapter(0);
    }

    // ==================================================================
    // Dil seçimi
    // ==================================================================

    private void OpenLanguageSelect()
    {
        var saved = _languageChosen ? Loc.Current : LanguageSettings.LoadOrDefault(Language.Turkish);
        _languageCursor = GameText.IsAvailable(saved) ? (int)saved : 0;
        ChangeState(GameState.LanguageSelect);
    }

    /// <summary>W/S veya ok tuşlarıyla gezinilir, ENTER onaylar; 1-6 tuşları dili doğrudan seçer.</summary>
    private void UpdateLanguageSelect(KeyboardState keys)
    {
        if (WasPressed(keys, Keys.W) || WasPressed(keys, Keys.Up))
            MoveLanguageCursor(-1);
        if (WasPressed(keys, Keys.S) || WasPressed(keys, Keys.Down))
            MoveLanguageCursor(+1);

        for (int i = 0; i < Languages.All.Length; i++)
        {
            if (WasPressed(keys, (Keys)((int)Keys.D1 + i)) || WasPressed(keys, (Keys)((int)Keys.NumPad1 + i)))
            {
                _languageCursor = i;
                ConfirmLanguage();
                return;
            }
        }

        if (WasPressed(keys, Keys.Enter))
            ConfirmLanguage();
    }

    /// <summary>İmleci bir sonraki kullanılabilir dile taşır (fontu olmayan diller atlanır).</summary>
    private void MoveLanguageCursor(int direction)
    {
        int count = Languages.All.Length;
        for (int step = 1; step <= count; step++)
        {
            int index = ((_languageCursor + direction * step) % count + count) % count;
            if (GameText.IsAvailable(Languages.All[index].Id))
            {
                _languageCursor = index;
                _audio.PlayClick();
                return;
            }
        }
    }

    private void ConfirmLanguage()
    {
        var language = Languages.All[_languageCursor].Id;
        if (!GameText.IsAvailable(language))
        {
            _audio.PlayError();
            return;
        }

        Loc.SetLanguage(language);
        LanguageSettings.Save(language);
        _languageChosen = true;
        _audio.PlayConfirm();
        ChangeState(GameState.MainMenu);
    }

    // ==================================================================
    // Eğitim ve bölümler
    // ==================================================================

    /// <summary>Aktif bölüm: hikâyede 20 bölümden biri, sonsuzda aşama, zamanla yarışta tek hat.</summary>
    private Chapter CurrentChapter => _mode switch
    {
        GameMode.Endless => Chapters.ForEndless(_endlessStage),
        GameMode.Speedrun => Chapters.Speedrun,
        GameMode.Wasteland => Chapters.Wasteland[Math.Min(_chapterIndex, Chapters.Wasteland.Length - 1)],
        GameMode.Episode2 => Chapters.Episode2[Math.Min(_chapterIndex, Chapters.Episode2.Length - 1)],
        _ => Chapters.All[_chapterIndex] // Hikâye ve Acımasız Kış aynı 40 bölümlük hattı kullanır
    };

    /// <summary>Bölümlü modların bölüm listesi (Episode 1/acımasız kış: 40, Episode 2: 40, Buzul Kıyameti: 5).</summary>
    private Chapter[] ChapterList => _mode switch
    {
        GameMode.Wasteland => Chapters.Wasteland,
        GameMode.Episode2 => Chapters.Episode2,
        _ => Chapters.All
    };

    /// <summary>Bölüm geçişi: Episode 2'de DLC gerektiren bölüme gelinmişse önce kilit ekranı.</summary>
    private void AdvanceToChapter(int next)
    {
        if (ChapterList[next].RequiresEpisode2Dlc && !DLCManager.DLC_EPISODE2_UNLOCKED)
        {
            OpenDlcGate(next);
            return;
        }
        ShowChapter(next);
    }

    /// <summary>Aktif bölümün tamamlanma kaydı (Episode 1, Episode 2 ya da Buzul Kıyameti).</summary>
    private void CompleteCurrentChapter()
    {
        int number = CurrentChapter.Number;
        if (_mode == GameMode.Wasteland) CompleteWastelandChapter(number);
        else if (_mode == GameMode.Episode2) CompleteEpisode2Chapter(number);
        else CompleteStoryChapter(number);
    }

    /// <summary>Episode 2 bölümü bitti: sıradaki bölümün kilidi (Episode 2 hattında) açılır.</summary>
    private void CompleteEpisode2Chapter(int number)
    {
        int next = Math.Min(Chapters.Episode2Count, number + 1);
        if (next > SaveData.Current.Episode2Unlocked) SaveData.Record(s => s.Episode2Unlocked = next);
        SaveData.Earn(GameConfig.Episode2ChapterReward);
        _toasts.Show(Loc.F("hub.complete", number, GameConfig.Episode2ChapterReward), CabinPanel.Good);
        CheckAchievements();
    }

    private void StartTutorial()
    {
        _tutorial.Start();
        _audio.PlayConfirm();
        ChangeState(GameState.Tutorial);
    }

    /// <summary>Bölüm kartını gösterir ve bölümün oynanış etkilerini (olay sıklığı) uygular.</summary>
    private void ShowChapter(int index)
    {
        _chapterIndex = index;
        _events.Clear();
        _events.CooldownMultiplier = CurrentChapter.EventCooldownMultiplier;
        _events.Weights = CurrentChapter.Weights;
        // Hikâye modunda kritik olmayan bölümlerde pusu %60 seyrek; kritik bölümde pusu kesin ve erkendir
        bool critical = !IsStoryMode || CurrentChapter.IsCritical;
        int ambushes = critical ? CurrentChapter.MaxAmbushes : (int)MathF.Round(CurrentChapter.MaxAmbushes * GameConfig.CalmAmbushScale);
        _ambush.StartChapter(ambushes, CurrentChapter.AmbushFrequency, IsStoryMode && CurrentChapter.IsCritical && ambushes > 0);
        ApplyChapterConsequences();
        _chapterHealthLost = false;
        _collapseGapKm = GameConfig.CollapseStartGapKm;
        // Telsiz: her bölümde yayınlar yeni frekanslarda; süren bir imdat görevi yoksa yeni bir çağrı yayında
        _radio.Retune((int)_mode * 1000 + CurrentChapter.Number * 13 + _endlessStage, withSos: _sosKm < 0f);
        _chatter.StartChapter();
        _audio.PlayWhistle();
        ChangeState(GameState.ChapterIntro);
    }

    private void UpdateTutorialPrompt(KeyboardState keys)
    {
        if (WasPressed(keys, Keys.D1) || WasPressed(keys, Keys.NumPad1))
            StartTutorial();
        else if (WasPressed(keys, Keys.D2) || WasPressed(keys, Keys.NumPad2))
            ShowChapter(0);
    }

    /// <summary>
    /// Güvenli alıştırma: tren hızlanıp yavaşlar ve arka plan kayar, ama yol sayılmaz,
    /// kaynak harcanmaz ve olay çıkmaz.
    /// </summary>
    private void UpdateTutorial(float dt, KeyboardState keys)
    {
        if (keys.IsKeyDown(Keys.W) || keys.IsKeyDown(Keys.Up))
            _train.Throttle(+1, dt);
        if (keys.IsKeyDown(Keys.S) || keys.IsKeyDown(Keys.Down))
            _train.Throttle(-1, dt);

        if (_tutorial.Update(dt, _train, WasPressed(keys, Keys.Space)))
            _audio.PlayConfirm();

        if (_tutorial.IsFinished || WasPressed(keys, Keys.Tab))
            ShowChapter(0);
    }

    private void UpdateChapterIntro(KeyboardState keys)
    {
        // Yazılar belirmeden yanlışlıkla geçilmesin
        if (_stateTimer > 1.2f && WasPressed(keys, Keys.Space))
        {
            _audio.PlayClick();
            if (IsStoryMode && CurrentChapter.IsStoryScene)
                EnterStoryScene();
            else
                ChangeState(GameState.Driving);
        }
    }

    /// <summary>Anlatılı hatlar: Episode 1 (Hikâye ve Acımasız Kış) ve Episode 2.</summary>
    private bool IsStoryMode => _mode is GameMode.Story or GameMode.Hardcore or GameMode.Episode2;

    /// <summary>Oyun modunu seçer; hikâye hatlarında bölümler uzun soluklu tempoda (GameConfig.Pace) oynanır.</summary>
    private void SetMode(GameMode mode)
    {
        _mode = mode;
        GameConfig.Pace = IsStoryMode ? GameConfig.StoryPace : 1f;
    }

    /// <summary>
    /// Önceki seçimlerin bölüm başındaki sonuçları: Demirhan işçilerinin hediyesi, tarikatın kutsaması,
    /// yolcuların verdiği pusu istihbaratı.
    /// </summary>
    private void ApplyChapterConsequences()
    {
        _sanity.DrainMultiplier = IsStoryMode && Narrative.Has(Flags.CultBlessed) ? GameConfig.CultSanityDrain : 1f;
        if (!IsStoryMode) return;

        if (_mode != GameMode.Episode2 && CurrentChapter.Number == 21 && Narrative.Has(Flags.MineWorkers))
        {
            _train.Refuel(GameConfig.WorkersGiftFuel);
            _toasts.Show(Loc.F("story.workersGift", (int)GameConfig.WorkersGiftFuel), CabinPanel.Good);
        }

        // Yolcuların pusu bilgisi kritik bölümlerde işe yarar
        bool informant = Narrative.Has(Flags.BoraIntel) && _cabin.HasProfile("bora")
                         || Narrative.Has(Flags.AnyaIntel) && _cabin.HasProfile("anya")
                         || Narrative.Has(Flags.DmitriGuard) && _cabin.HasProfile("dmitri");
        if (informant && CurrentChapter.IsCritical && !CurrentChapter.IsStoryScene)
        {
            _ambushIntel = true;
            _toasts.Show(Loc.T("story.intel"), Hud.Amber);
        }
    }

    // ------------------------------------------------------------------
    // Hikâye bölümleri: keşif ve diyalog sahneleri (sürüş yok)
    // ------------------------------------------------------------------

    /// <summary>Hikâye bölümünün sahnesine iner: tren durur, Arthur yürüyerek keşfeder ya da diyalog başlar.</summary>
    private void EnterStoryScene()
    {
        var scene = CurrentChapter.Scene.Value;
        _train.Stop();
        _events.Clear();
        _supplyWagonKm = -1f;
        var accent = scene switch
        {
            TownArchetype.Headquarters => new Color(255, 110, 90),
            TownArchetype.Homestead => new Color(255, 190, 110),
            TownArchetype.Institute => new Color(120, 220, 255),
            TownArchetype.RadioMast => new Color(140, 220, 255),
            TownArchetype.ShelterGate => new Color(210, 220, 240),
            _ => new Color(170, 200, 255)
        };
        _townScene.Enter(TownInfo.ForScene(scene, accent), (int)_travelledKm);
        _townDialog = TownDialog.None;
        ChangeState(GameState.Town);

        string start = NarrativeData.SceneStart(scene);
        if (start != null)
            _story.Open(this, start, CurrentTown.Name);
    }

    /// <summary>
    /// Hikâye bölümü bitti: tren bölümün sonuna geçer ve sıradaki bölüm kartı açılır. Episode 2'nin son bölümü
    /// (AURA'nın çekirdeği) seçilen sonla biter; Bölüm 2'nin sonunda DLC yoksa kilit ekranı gelir.
    /// </summary>
    private void FinishStoryScene()
    {
        var chapter = CurrentChapter;
        CompleteCurrentChapter();
        if (_mode == GameMode.Episode2 && chapter.Number == Chapters.Episode2Count)
        {
            FinishAura();
            return;
        }
        _travelledKm = chapter.EndKm;
        _nextStopIndex = Route.FirstStopAfter(Stops, chapter.EndKm - 0.01f);
        _nextTowerKm = _travelledKm + 6f;
        if (_chapterIndex + 1 < ChapterList.Length)
            AdvanceToChapter(_chapterIndex + 1);
    }

    /// <summary>Episode 2 finali: AURA'nın çekirdeğinde verilen karar üç sondan birini belirler.</summary>
    private void FinishAura()
    {
        SaveData.Record(s => s.Episode2Complete = true);
        SaveData.Earn(GameConfig.ShelterReward);
        var (ending, id) = Endings.ForAura(_cabin.Passengers);
        _ending = ending;
        RecordEnding(id);
    }

    /// <summary>Kasabadaki ya da hikâye sahnesindeki inceleme noktası: sıradaki görülmemiş replik açılır.</summary>
    private void OpenSpot(string spotId)
    {
        var spot = NarrativeData.Spot(spotId);
        if (spot == null) return;
        _audio.PlayClick();
        _story.Open(this, Narrative.NextEntry(spot), spot.Name);
    }

    // INarrativeHost: diyalog sahnesi tamamlandı
    public void OnSceneComplete()
    {
        if (_state == GameState.Town && CurrentTown.IsStoryScene)
            FadeTo(FinishStoryScene);
    }

    /// <summary>Seçenek şu an seçilebilir mi? Kredi, eşya, erzak ve kabinde boş yer kontrolü.</summary>
    public bool CanApply(NarrativeEffect e, Passenger questPassenger)
    {
        if (e.Credits < 0 && !SaveData.Current.DevUnlockAll && SaveData.Current.Credits < -e.Credits) return false;
        if (e.ConsumeItem.HasValue && !_inventory.Has(e.ConsumeItem.Value)) return false;
        if (e.Rations < 0 && _inventory.Rations < -e.Rations) return false;
        if (e.RecruitTownPassenger)
        {
            var p = CurrentTown?.Passenger;
            if (p == null || _cabin.IsFull || _cabin.HasProfile(p.Id) || _passengerHired[CurrentTown.Number - 1]) return false;
        }
        if (e.RecruitId != null && (_cabin.IsFull || _cabin.HasProfile(e.RecruitId) || PassengerRegistry.Find(e.RecruitId) == null))
            return false;
        return true;
    }

    /// <summary>Seçimin oyuna etkisi; özet satırları diyalog penceresinde gösterilir. Karma gizlidir.</summary>
    public string Apply(NarrativeEffect e, Passenger questPassenger)
    {
        var lines = new List<string>();
        bool recruit = e.RecruitTownPassenger && CanApply(e, questPassenger);
        bool recruitId = e.RecruitId != null && CanApply(e, questPassenger);
        Narrative.Set(e.SetFlag);
        Narrative.Set(e.SetFlag2);
        _karma += e.Karma;
        if (e.HackSuccess != null) _sceneHack = e; // Diyalog kapanınca hack arabirimi açılır

        if (e.Credits > 0)
        {
            SaveData.Earn(e.Credits);
            lines.Add(Loc.F("sum.credits", e.Credits));
        }
        else if (e.Credits < 0 && SaveData.TrySpend(-e.Credits))
        {
            lines.Add(Loc.F("sum.creditsSpent", -e.Credits));
        }
        if (e.Fuel != 0f)
        {
            _train.Refuel(e.Fuel);
            lines.Add(e.Fuel > 0f ? Loc.F("sum.fuel", (int)e.Fuel) : Loc.F("sum.fuelLost", (int)-e.Fuel));
        }
        if (e.Repair > 0f)
        {
            _train.Repair(e.Repair);
            lines.Add(Loc.F("sum.health", (int)e.Repair));
        }
        if (e.Damage > 0f)
        {
            _train.Damage(e.Damage);
            lines.Add(Loc.F("sum.damage", (int)e.Damage));
        }
        if (e.Heat > 0f)
        {
            _train.StokeBoiler(e.Heat);
            lines.Add(Loc.F("sum.heat", (int)e.Heat));
        }
        else if (e.Heat < 0f)
        {
            _train.DrainHeat(-e.Heat);
            lines.Add(Loc.F("sum.cold", (int)-e.Heat));
        }
        if (e.Sanity != 0f)
        {
            _sanity.Restore(e.Sanity);
            lines.Add(e.Sanity > 0f ? Loc.F("sum.sanity", (int)e.Sanity) : Loc.F("sum.sanityLost", (int)-e.Sanity));
        }
        if (e.Morale != 0f && _cabin.Count > 0)
        {
            foreach (var p in _cabin.Passengers) p.LiftMorale(e.Morale);
            lines.Add(Loc.F(e.Morale > 0f ? "sum.morale" : "sum.moraleLost", (int)MathF.Abs(e.Morale)));
        }
        if (questPassenger != null && e.QuestMorale != 0f)
        {
            questPassenger.LiftMorale(e.QuestMorale);
            lines.Add(Loc.F(e.QuestMorale > 0f ? "sum.qmorale" : "sum.qmoraleLost", questPassenger.Name, (int)MathF.Abs(e.QuestMorale)));
        }
        if (questPassenger != null && e.Suspicion != 0f)
        {
            questPassenger.AddSuspicion(e.Suspicion);
            lines.Add(Loc.F(e.Suspicion > 0f ? "sum.suspicion" : "sum.trust", questPassenger.Name));
        }
        if (e.Rations > 0)
            lines.Add(Loc.F("rations.bought", _inventory.AddRations(Difficulty.Scale(e.Rations))));
        else if (e.Rations < 0)
        {
            for (int i = 0; i < -e.Rations; i++) _inventory.TryConsumeRation();
            lines.Add(Loc.F("sum.rationsGiven", -e.Rations));
        }
        if (e.ConsumeItem.HasValue && _inventory.TryConsume(e.ConsumeItem.Value))
            lines.Add(Loc.F("sum.itemSpent", ItemCatalog.Name(e.ConsumeItem.Value)));
        if (e.Items != null)
            foreach (var item in e.Items)
                lines.Add(_inventory.TryAdd(item) ? Loc.F("sum.item", ItemCatalog.Name(item)) : Loc.F("sum.itemLost", ItemCatalog.Name(item)));
        if (e.Loot > 0)
        {
            int looted = CollectLoot(e.Loot);
            if (looted > 0) lines.Add(Loc.F("sum.loot", looted));
        }
        if (recruit)
        {
            var passenger = CurrentTown.Passenger;
            _cabin.TryBoard(passenger);
            _passengerHired[CurrentTown.Number - 1] = true;
            ReportContract(ContractKind.Rescue, 1);
            _karma += 10;
            lines.Add(Loc.F("sum.passenger", passenger.Name, passenger.SkillTag));
        }
        if (recruitId)
        {
            var profile = PassengerRegistry.Find(e.RecruitId);
            _cabin.TryBoard(profile);
            ReportContract(ContractKind.Rescue, 1);
            _karma += 5;
            lines.Add(Loc.F("sum.passenger", profile.Name, profile.SkillTag));
        }
        if (e.SetFlag != null || e.SetFlag2 != null)
            _townScene.RefreshInteractables(); // Yeni açılan noktalar (zula vb.)
        return string.Join("\n", lines);
    }

    // ------------------------------------------------------------------
    // Yolcu görev zincirleri ([J] konuş)
    // ------------------------------------------------------------------

    /// <summary>Şu an konuşmak isteyen ilk yolcu ve görev adımının düğümü (yoksa null).</summary>
    private (Passenger Passenger, string Node) AvailableQuest()
    {
        if (!IsStoryMode) return (null, null);
        foreach (var p in _cabin.Passengers)
        {
            string id = p.Profile.Id;
            if (!NarrativeData.QuestLength.TryGetValue(id, out int length)) continue;
            int step = Narrative.QuestStep(id);
            if (step >= length || p.IsRevealed) continue;
            int boarded = _boardedAt.TryGetValue(id, out int b) ? b : CurrentChapter.Number;
            if (CurrentChapter.Number - boarded >= GameConfig.QuestStepChapters * step + 1)
                return (p, $"q.{id}.{step + 1}");
        }
        return (null, null);
    }

    /// <summary>Trene yeni binen yolcuların bölümünü not eder; konuşmak isteyen yolcuyu bir kez haber verir.</summary>
    private void TrackQuests()
    {
        foreach (var p in _cabin.Passengers)
            if (!_boardedAt.ContainsKey(p.Profile.Id)) _boardedAt[p.Profile.Id] = CurrentChapter.Number;

        var (passenger, node) = AvailableQuest();
        if (node == null || node == _questNotified) return;
        _questNotified = node;
        _toasts.Show(Loc.F("quest.ready", passenger.Name), new Color(140, 220, 255));
    }

    /// <summary>
    /// Sürüş bölümünün içindeki hikâye anı: bölümde belirli bir noktaya gelindiğinde diyalog açılır
    /// (oyun zamanı durur). Barikat ve hesaplaşma gibi anlarda tren durur.
    /// </summary>
    private bool TryOpenStoryBeat()
    {
        var chapter = CurrentChapter;
        int episode;
        if (_mode is GameMode.Story or GameMode.Hardcore && chapter.Kind == ChapterKind.Story) episode = 1;
        else if (_mode == GameMode.Episode2 && chapter.Kind == ChapterKind.Episode2) episode = 2;
        else return false;
        float progress = (_travelledKm - chapter.StartKm) / MathF.Max(1f, chapter.EndKm - chapter.StartKm);
        var beat = StoryBeats.Due(episode, chapter.Number, progress, _cabin.Count);
        if (beat == null) return false;

        if (beat.StopTrain) _train.Stop();
        _chatter.Clear();
        _audio.PlayRadioChatter();
        _story.Open(this, beat.Node, chapter.Name);
        return true;
    }

    /// <summary>[J]: konuşmak isteyen yolcuyla kişisel hikâyesinin sıradaki adımı.</summary>
    private bool TryOpenQuest()
    {
        var (passenger, node) = AvailableQuest();
        if (node == null)
        {
            _toasts.Show(Loc.T("quest.none"), Hud.Label);
            return false;
        }
        Narrative.AdvanceQuest(passenger.Profile.Id);
        _questNotified = null;
        _story.Open(this, node, passenger.Name, passenger);
        return true;
    }

    /// <summary>
    /// Katedilen yol bir sonraki bölümün (sonsuzda aşamanın) başlangıcını geçtiyse:
    /// hikâyede biten bölüm kaydedilir (sıradakinin kilidi açılır, ödül verilir), sonra bölüm kartı açılır.
    /// </summary>
    private bool CheckChapterTransition()
    {
        if (_mode == GameMode.Speedrun) return false;

        if (_mode == GameMode.Endless)
        {
            int stage = (int)(_travelledKm / GameConfig.EndlessStageKm) + 1;
            if (stage <= _endlessStage) return false;
            if (!_chapterHealthLost) ReportContract(ContractKind.NoDamageChapter, 1);
            _endlessStage = stage;
            ShowChapter(0);
            return true;
        }

        int next = _chapterIndex + 1;
        if (next >= ChapterList.Length || _travelledKm < ChapterList[next].StartKm)
            return false;

        CompleteCurrentChapter();
        if (!_chapterHealthLost) ReportContract(ContractKind.NoDamageChapter, 1);
        AdvanceToChapter(next);
        return true;
    }

    /// <summary>Buzul Kıyameti bölümü bitti: sıradaki ek bölümün kilidi açılır, daha yüksek ödül.</summary>
    private void CompleteWastelandChapter(int number)
    {
        if (number + 1 > SaveData.Current.WastelandUnlocked)
            SaveData.Record(s => s.WastelandUnlocked = Math.Min(Chapters.Wasteland.Length, number + 1));
        SaveData.Earn(GameConfig.WastelandChapterReward);
        _toasts.Show(Loc.F("hub.complete", number, GameConfig.WastelandChapterReward), CabinPanel.Good);
        CheckAchievements();
    }

    /// <summary>Hikâye bölümü bitti: kilit açılır, ödül verilir, başarımlar kontrol edilir.</summary>
    private void CompleteStoryChapter(int number)
    {
        SaveData.CompleteChapter(number);
        SaveData.Earn(GameConfig.ChapterReward);
        _toasts.Show(Loc.F("hub.complete", number, GameConfig.ChapterReward), CabinPanel.Good);
        CheckAchievements();
    }

    /// <summary>Yeni karşılanan başarımları bildirir (skin kilitleri bunlara bağlı).</summary>
    private void CheckAchievements()
    {
        foreach (var achievement in AchievementManager.Evaluate()) // Yerel kayıt + platforma (Steam) bildirim
        {
            _toasts.Show(Loc.F("ach.unlocked", achievement.Name), new Color(255, 215, 110));
            _audio.PlaySuccess();
        }
        foreach (var item in QuestCatalog.Evaluate()) // Görevle açılan mikro kozmetikler
        {
            _toasts.Show(Loc.F("cos.unlocked", item.Name), new Color(200, 160, 255));
            _audio.PlayCoin();
        }
    }

    private void ChangeState(GameState newState)
    {
        ContractManager.Flush(); // Görev ilerlemesi durum değişimlerinde kaydedilir
        _state = newState;
        _stateTimer = 0f;
    }

    // ==================================================================
    // Durak yardımcıları
    // ==================================================================

    /// <summary>Aktif modun durak listesi (hikâye hattı, zamanla yarış hattı ya da sonsuz hat).</summary>
    private IReadOnlyList<RouteStop> Stops => _mode switch
    {
        GameMode.Endless => _endlessRoute.Stops,
        GameMode.Speedrun => Route.SpeedrunStops,
        GameMode.Wasteland => Route.WastelandStops,
        GameMode.Episode2 => Route.Episode2Stops,
        _ => Route.Stops
    };

    /// <summary>Rotadaki sıradaki durak (istasyon, kasaba, benzinlik ya da Sığınak-0).</summary>
    private RouteStop NextStop => Stops[Math.Min(_nextStopIndex, Stops.Count - 1)];

    private bool NextStopIsShelter => NextStop.Kind == StopKind.Shelter;

    private float NextStopKm => NextStop.Km;

    /// <summary>HUD'daki büyük mesafe: hikâyede Sığınak-0'a, yarışta teslimata kalan; sonsuzda katedilen.</summary>
    private float RemainingKm => _mode switch
    {
        GameMode.Endless => _travelledKm,
        GameMode.Speedrun => GameConfig.SpeedrunDistanceKm - _travelledKm,
        GameMode.Wasteland => GameConfig.WastelandDistanceKm - _travelledKm,
        _ => GameConfig.TotalDistanceKm - _travelledKm
    };

    private float SpeedrunTimeLeft => GameConfig.SpeedrunTimeLimit - _speedrunTime;

    private bool IsTravelling => _state is GameState.Driving or GameState.Arriving;

    private bool IsInJourney => _state is GameState.Tutorial or GameState.ChapterIntro
        or GameState.Driving or GameState.Arriving or GameState.Station
        or GameState.Town or GameState.Depot or GameState.Chase or GameState.Sentinel or GameState.Mining or GameState.FinalChoice;

    // ==================================================================
    // Güncelleme
    // ==================================================================

    protected override void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        dt = MathF.Min(dt, 0.1f); // Pencere sürüklenirken oluşan dev sıçramaları engelle

        var keys = Keyboard.GetState();

        if (WasPressed(keys, Keys.Escape))
            HandleEscape();

        if (_isPaused)
        {
            // Duraklatılmışken dünya donar; yalnızca ESC (devam) ve Q (çıkış) dinlenir
            if (WasPressed(keys, Keys.Q))
                Exit();
            UpdateAudio();
            _previousKeys = keys;
            base.Update(gameTime);
            return;
        }

        // Fotoğraf modu: dünya donar, arayüz gizlenir
        if (_photo.IsActive)
        {
            if (_photo.Update(dt, new InputState(keys, _previousKeys)))
                SavePhoto();
            UpdateAudio();
            _previousKeys = keys;
            base.Update(gameTime);
            return;
        }
        if (WasPressed(keys, Keys.P) && CanTakePhoto)
        {
            _photo.Begin();
            _audio.PlayShutter();
            _previousKeys = keys;
            base.Update(gameTime);
            return;
        }

        _time += dt;
        _stateTimer += dt;
        var input = new InputState(keys, _previousKeys);

        // Görev ilerlemesi her 20 saniyede bir kaydedilir (yol alma her karede bildirilir)
        _contractFlushTimer += dt;
        if (_contractFlushTimer > 20f)
        {
            _contractFlushTimer = 0f;
            ContractManager.Flush();
        }

        // Garaj açıkken oyun zamanı durur; değişiklikler trene hemen yansır
        if (_garage.IsOpen)
        {
            if (_garage.Update(dt, input, _cabin, _audio))
                ApplyProfile();
            UpdateAudio();
            _previousKeys = keys;
            base.Update(gameTime);
            return;
        }

        // Geçiş sürerken oyun mantığı bekler; dünya (kar, duman) canlı kalır
        if (UpdateFade(dt))
        {
            UpdateWorld(dt);
            _toasts.Update(dt);
            UpdateAudio();
            _previousKeys = keys;
            base.Update(gameTime);
            return;
        }

        // Hikâye sahnesindeki terminal: diyalog kapanınca hack arabirimi açılır (Episode 2, Bölüm 2)
        if (_sceneHack != null && !_sceneHacking && !_story.IsOpen && !_hack.IsOpen)
        {
            _sceneHacking = true;
            _hack.Open(withActions: false, GameConfig.SceneHackDifficulty, HackHelper, allowSwitch: false);
            _audio.PlayRadioChatter();
        }

        // AURA hack arabirimi: sinyal kulesinde oyun durur; Sentinel karşılaşmasında karşılaşma kendi içinde işler
        if (_hack.IsOpen && _state != GameState.Sentinel)
        {
            var result = _hack.Update(dt, input, _audio);
            if (result != HackResult.None)
            {
                if (_sceneHacking) OnSceneHack(result);
                else OnTowerHack(result);
            }
            UpdateAudio();
            _previousKeys = keys;
            base.Update(gameTime);
            return;
        }

        // Hikâye diyaloğu açıkken oyun zamanı durur
        if (_story.IsOpen)
        {
            _story.Update(dt, input, _audio);
            UpdateAudio();
            _previousKeys = keys;
            base.Update(gameTime);
            return;
        }

        // Kabin sorgusu açıkken oyun zamanı durur (yalnızca menü dinlenir)
        if (_cabinMenu.IsOpen)
        {
            var ejected = _cabinMenu.Update(dt, input, _cabin, _audio);
            if (ejected != null) EjectPassenger(ejected);
            UpdateAudio();
            _previousKeys = keys;
            base.Update(gameTime);
            return;
        }

        // Telsiz açıkken de oyun zamanı durur; eski şarkılar akıl sağlığını toparlar
        if (_radioPanel.IsOpen)
        {
            HandleRadio(_radioPanel.Update(dt, input, _radio, _audio));
            if (_radioPanel.Channel?.Kind == RadioChannelKind.Music && _radioPanel.Signal > 0.6f)
            {
                _sanity.Restore(GameConfig.RadioMusicSanityPerSecond * dt);
                foreach (var p in _cabin.Passengers) p.LiftMorale(GameConfig.RadioMusicSanityPerSecond * 0.5f * dt);
            }
            UpdateAudio();
            _previousKeys = keys;
            base.Update(gameTime);
            return;
        }

        // Zamanla yarış: kronometre bütün yolculuk boyunca (duraklarda da) işler
        if (_mode == GameMode.Speedrun && IsInJourney && _state != GameState.ChapterIntro)
        {
            _speedrunTime += dt;
            if (SpeedrunTimeLeft <= 0f && _state is not (GameState.GameOver or GameState.Victory))
            {
                Die(DeathReason.TimeUp);
                _previousKeys = keys;
                base.Update(gameTime);
                return;
            }
        }

        switch (_state)
        {
            case GameState.Mining:   UpdateMining(dt, input); break;
            case GameState.FinalChoice: UpdateFinalChoice(dt, input); break;
            case GameState.Town:     UpdateTown(dt, input); break;
            case GameState.Depot:    UpdateDepot(dt, input); break;
            case GameState.Chase:    UpdateChase(dt, keys, input); break;
            case GameState.Sentinel: UpdateSentinel(dt, keys, input); break;
            case GameState.DlcGate:  UpdateDlcGate(dt, input); break;
            case GameState.EpisodeOutro:
                _outro.Update(dt, input, _audio);
                if (_outro.IsFinished) ResetGame();
                break;
            case GameState.LanguageSelect: UpdateLanguageSelect(keys); break;
            case GameState.MainMenu:       UpdateMainMenu(dt, keys); break;
            case GameState.TutorialPrompt: UpdateTutorialPrompt(keys); break;
            case GameState.Tutorial:       UpdateTutorial(dt, keys); break;
            case GameState.ChapterIntro:   UpdateChapterIntro(keys); break;
            case GameState.Driving:  UpdateDriving(dt, keys); break;
            case GameState.Arriving: UpdateArriving(dt, keys); break;
            case GameState.Station:  UpdateStation(dt, keys); break;
            case GameState.GameOver: UpdateEnding(dt, keys); break;
            case GameState.Victory:  UpdateEnding(dt, keys); break;
        }

        UpdateWorld(dt);
        _toasts.Update(dt);
        UpdateAudio();

        _previousKeys = keys;
        base.Update(gameTime);
    }

    /// <summary>
    /// ESC: yolculuk ve eğitim sırasında duraklatır/devam ettirir; eğitim sorusunda menüye döner;
    /// ana menüde ve finalde oyundan çıkar.
    /// </summary>
    /// <summary>Fotoğraf modu sürüşte ve kasabada, başka bir pencere açık değilken açılır.</summary>
    private bool CanTakePhoto =>
        (_state is GameState.Driving or GameState.Arriving || _state == GameState.Town && _townDialog == TownDialog.None)
        && !_cabinMenu.IsOpen && !_radioPanel.IsOpen && !_garage.IsOpen && _fadeTimer < 0f;

    /// <summary>Kareyi PNG olarak kaydeder ve platformun paylaşım kitaplığına ekler (Steam ekran görüntüleri).</summary>
    private void SavePhoto()
    {
        string path = _photo.Save(GraphicsDevice, _spriteBatch, DrawWatermark);
        _audio.PlayShutter();
        if (path == null) return;
        SaveData.Record(s => s.PhotosTaken++);
        string caption = "The Last Delivery - " + (_state == GameState.Town ? CurrentTown.Name : CurrentChapter.HudTitle);
        try { AchievementManager.Platform.ShareScreenshot(path, caption); }
        catch (Exception) { /* Paylaşılamadı: fotoğraf yine de diskte */ }
    }

    /// <summary>Kaydedilen fotoğrafın köşesindeki filigran: oyunun adı, tarih ve dönemsel etkinlik.</summary>
    private void DrawWatermark(SpriteBatch sb)
    {
        string season = SeasonalEvents.Key != null ? "  -  " + Loc.T(SeasonalEvents.Key) : "";
        string text = "THE LAST DELIVERY  " + DateTime.Now.ToString("yyyy.MM.dd", System.Globalization.CultureInfo.InvariantCulture) + season;
        float w = GameText.MeasureLine(text);
        sb.FillRect(new Rectangle((int)(GameConfig.ScreenWidth - w - 36), GameConfig.ScreenHeight - 44, (int)w + 24, 30), Color.Black * 0.5f);
        GameText.DrawRight(sb, text, GameConfig.ScreenWidth - 24, GameConfig.ScreenHeight - 40, Color.White * 0.9f);
    }

    private void HandleEscape()
    {
        if (_hack.IsOpen) // ESC hack'ten vazgeçer (kulede kaçırılmış sayılır)
        {
            _hack.Close();
            if (_sceneHacking) OnSceneHack(HackResult.Cancelled);
            else if (_towerHacking) OnTowerHack(HackResult.Cancelled);
            _audio.PlayClick();
            return;
        }
        if (_state == GameState.EpisodeOutro || _state == GameState.DlcGate)
        {
            _audio.PlayClick();
            ResetGame();
            return;
        }
        if (_photo.IsActive)
        {
            _photo.End();
            _audio.PlayClick();
            return;
        }
        if (_garage.IsOpen)
        {
            _garage.Close();
            _audio.PlayClick();
            return;
        }
        if (IsInJourney)
        {
            _isPaused = !_isPaused;
            _audio.PlayClick();
        }
        else if (_state == GameState.TutorialPrompt || (_state == GameState.LanguageSelect && _languageChosen))
        {
            _audio.PlayClick();
            ChangeState(GameState.MainMenu);
        }
        else if (_state == GameState.MainMenu && _meta.CodeFocused)
        {
            _meta.UnfocusCode(); // ESC kod alanını kapatır, oyundan çıkmaz
        }
        else if (_state == GameState.MainMenu && _meta.Back())
        {
            _audio.PlayClick(); // Alt ekrandan ana menüye
        }
        else
        {
            Exit();
        }
    }

    private void UpdateMainMenu(float dt, KeyboardState keys)
    {
        // Ana menü, bölüm seçimi, mağaza ve skinler MetaMenu'de
        // Skin ekranında sahnedeki tren canlı önizleme (kilitli DLC skinleri de vitrin olarak gösterilir)
        _train.Skin = _meta.Screen == MetaScreen.Skins
            ? Cosmetics.Skin(_meta.PreviewSkinId)
            : DLCManager.ActiveSkin(_meta.PreviewSkinId);
        _train.SmokeStyle = _meta.PreviewSmoke ?? (SmokeStyle)DLCManager.Active(CosmeticSlot.Smoke).Value;
        switch (_meta.Update(dt, new InputState(keys, _previousKeys), _audio))
        {
            case MetaAction.StartWasteland:
                StartWasteland(_meta.ChosenWastelandChapter);
                break;
            case MetaAction.StartHardcore:
                StartHardcore();
                break;
            case MetaAction.StartChapter:
                if (_meta.ChosenEpisode == 2) StartEpisode2(_meta.ChosenChapter);
                else StartRun(_meta.ChosenChapter);
                break;
            case MetaAction.StartEndless:
                StartEndless();
                break;
            case MetaAction.StartSpeedrun:
                StartSpeedrun();
                break;
            case MetaAction.OpenLanguage:
                OpenLanguageSelect();
                break;
            case MetaAction.Quit:
                Exit();
                break;
        }
    }

    /// <summary>Dünya her durumda canlı kalır; hız 0 ise arka plan da durur.</summary>
    private void UpdateWorld(float dt)
    {
        bool inRun = _state != GameState.MainMenu && _state != GameState.LanguageSelect;
        float progress = _mode switch
        {
            GameMode.Endless => MathF.Min(1f, _travelledKm / 1500f),
            GameMode.Speedrun => _travelledKm / GameConfig.SpeedrunDistanceKm,
            GameMode.Wasteland => 0.5f + 0.5f * _travelledKm / GameConfig.WastelandDistanceKm, // Kıyamet: fırtına baştan sert
            _ => _travelledKm / GameConfig.TotalDistanceKm
        };
        // Fırtına günü (dönemsel etkinlik): fırtına bölümün sınırından daha sert eser
        _weather.Update(dt, progress, inRun ? MathF.Min(1f, CurrentChapter.MaxStorm + SeasonalEvents.ExtraStorm) : 1f);
        if (IsTravelling) _dayCycle.Update(dt);
        else _chatter.Clear(); // Durakta, kovalamacada ya da kasabada yarım kalan satır silinir
        _wolfSmokeFlash = MathF.Max(0f, _wolfSmokeFlash - dt * 2f);

        // Görüş: gece, tünel ya da tipi (kırık ray işaretleri fenersiz geç seçilir)
        _events.LowVisibility = _dayCycle.IsNight || CurrentChapter.Darkness > 0.3f || _events.IsBlizzard;

        UpdateStormLimit(dt);
        UpdateScreenShake(dt);

        // Kasabada tren binaların önündeki her zamanki yerinde durur (ek vagonlar sola, ekran dışına uzanır)
        Train.LayoutShiftEnabled = _state != GameState.Town;

        // DLC kozmetikleri: kuşanılan duman (menüde önizleme MetaMenu'den gelir)
        if (_state != GameState.MainMenu)
            _train.SmokeStyle = (SmokeStyle)DLCManager.Active(CosmeticSlot.Smoke).Value;

        // Duraklarda olay iptal edilirse arızalı far da (Arthur'un durakta taktığı yedek ampulle) yanar
        if (_events.Active?.Type != RoadEventType.LampFailure)
            _train.HeadlightBroken = false;
        _train.StoveLevel = _cabin.Stove / 100f;
        _train.CargoFill = _inventory.Cargo.Capacity > 0 ? (float)_inventory.Cargo.Total / _inventory.Cargo.Capacity : 0f;

        UpdateSanity(dt);

        float scroll = _train.ScrollSpeed * dt;
        _mountains.Update(scroll);
        _pines.Update(scroll);
        _ground.Update(scroll);
        _decorScroll += scroll;
        UpdateThemeEffects(dt);
        _snow.Update(dt, _train.ScrollSpeed, _time, _weather);

        // Olay efektleri yalnızca yoldayken görünür
        int steam = !IsTravelling ? 0 : _events.IsValvePending ? 1 : _events.IsLeaking ? 2 : 0;
        _train.SetEffects(steam, IsTravelling && _events.IsGrinding, _cabin.Count);
        _train.UpdateVisuals(dt, engineAlive: _state != GameState.GameOver, _weather.WindSpeed);

        // Geride kalan yük vagonunu ve maden girişini unut
        if (_supplyWagonKm > 0f && StationRenderer.ScreenX(_supplyWagonKm, _travelledKm) < -400f)
            _supplyWagonKm = -1f;
        if (_mineKm > 0f && StationRenderer.ScreenX(_mineKm, _travelledKm) < -400f)
            _mineKm = -1f;
    }

    /// <summary>
    /// Arthur'un akıl sağlığı: gece, soğuk, açlık ve fener arızası aşındırır. %30'un altında sanrılar başlar:
    /// arka planda hayalet tren, sahte pusu alarmları ve kızıl görüntüler.
    /// </summary>
    private void UpdateSanity(float dt)
    {
        _ghostTrainTimer = MathF.Max(0f, _ghostTrainTimer - dt);

        // Evcil hayvan: kabinde Arthur'un yanında; dakikada %10 akıl ve moral yeniler
        if (Pet != PetKind.None && IsInJourney && _state != GameState.FinalChoice)
        {
            _sanity.Restore(GameConfig.PetRegenPerSecond * GameConfig.NeedsScale * dt);
            foreach (var p in _cabin.Passengers) p.LiftMorale(GameConfig.PetRegenPerSecond * GameConfig.NeedsScale * dt);
        }

        // Hayalet Haftası: gece uzak raylarda akıl sağlığından bağımsız hayalet tren görülebilir
        if (SeasonalEvents.Current == Season.GhostWeek && IsTravelling && _dayCycle.IsNight
            && _ghostTrainTimer <= 0f && _rng.NextDouble() < dt * 0.03)
            _ghostTrainTimer = 6f;
        _redVisionTimer = MathF.Max(0f, _redVisionTimer - dt);
        if (_fakeAmbushTimer > 0f)
        {
            _fakeAmbushTimer -= dt;
            if (_fakeAmbushTimer <= 0f) _toasts.Show(Loc.T("hallu.nothing"), Hud.Label); // ...ortada kimse yok
        }
        if (!IsInJourney || _state == GameState.FinalChoice) return;

        bool driving = IsTravelling || _state == GameState.Chase;
        _sanity.Update(dt, driving, MathHelper.Clamp(_dayCycle.Night * CurrentChapter.NightStrength, 0f, 1.4f),
            _train.HeadlightBroken, _train.Temperature < GameConfig.ColdThreshold,
            _cabin.HungriestPassenger(GameConfig.NeedRequestThreshold) != null, radioMusic: false);

        switch (_sanity.Triggered)
        {
            case Hallucination.GhostTrain:
                _ghostTrainTimer = 6f;
                _audio.PlayEerie();
                break;
            case Hallucination.FakeAmbush:
                _toasts.Show(Loc.T("hallu.ambush"), Hud.Danger);
                _audio.PlayAlarm();
                AddShake(5f);
                _fakeAmbushTimer = 2.5f;
                break;
            case Hallucination.RedVision:
                _redVisionTimer = 2.5f;
                _audio.PlayEerie();
                break;
        }
    }

    /// <summary>[M]: kabindeki müzik kutusu kurulur; akıl sağlığı ve yolcu morali toparlanır.</summary>
    private void PlayMusicBox()
    {
        if (!_sanity.StartMusicBox())
        {
            Reject(Loc.F("musicbox.cooldown", (int)MathF.Ceiling(_sanity.MusicBoxCooldown)));
            return;
        }
        var song = DLCManager.Active(CosmeticSlot.MusicBox);
        _audio.PlayMusicBox(song.Value);
        _toasts.Show(Loc.F("musicbox.playing", song.Name), new Color(255, 215, 150));
        SaveData.Record(s => s.MusicBoxPlays++);
    }

    /// <summary>Mod metnini biçimlendirir; hatalı süslü parantezler oyunu düşürmez, metin olduğu gibi gösterilir.</summary>
    private static string SafeFormat(string text, params object[] args)
    {
        try { return string.Format(System.Globalization.CultureInfo.InvariantCulture, text, args); }
        catch (FormatException) { return text; }
    }

    /// <summary>Telsizden gelen eylemler: imdat çağrısına yanıt, mafya istihbaratı ve anlaşması.</summary>
    private void HandleRadio(RadioAction action)
    {
        switch (action)
        {
            case RadioAction.MafiaIntel:
                _ambushIntel = true; // Sonraki pusuya hazırlıklı yakalanılır
                _toasts.Show(Loc.T("radio.intel"), Hud.Amber);
                break;

            case RadioAction.AcceptMafiaDeal:
                if (_mafiaDeal) return;
                _mafiaDeal = true;
                _karma -= 40;
                _toasts.Show(Loc.T("radio.dealMade"), Hud.Danger);
                break;

            case RadioAction.AnswerSos:
                if (_radio.SosAnswered || _sosKm > 0f) return;
                // İmdat noktası sıradaki durağa çok yakın düşmemeli
                if (NextStopKm - _travelledKm < GameConfig.SosMinGapKm)
                {
                    _toasts.Show(Loc.T("sos.tooClose"), Hud.Label);
                    return;
                }
                float lead = GameConfig.SosLeadMinKm + (float)_rng.NextDouble() * (GameConfig.SosLeadMaxKm - GameConfig.SosLeadMinKm);
                _sosKm = _travelledKm + lead;
                _sosType = _radio.SosType;
                _radio.SosAnswered = true;
                _karma += 5;
                ReportContract(ContractKind.AnswerSos, 1);
                _toasts.Show(Loc.F("sos.accepted", (int)lead), new Color(255, 210, 90));
                break;
        }
    }

    /// <summary>
    /// Şiddetli fırtına ya da tipide azami hız %30 düşer (yumuşak geçişle); başladığında bir kez bildirilir.
    /// </summary>
    private void UpdateStormLimit(float dt)
    {
        bool severe = IsTravelling && (_weather.Intensity >= GameConfig.SevereStormIntensity || _events.IsBlizzard);
        float target = severe ? GameConfig.StormSpeedMultiplier : 1f;
        _train.SpeedCapMultiplier = MathHelper.Lerp(_train.SpeedCapMultiplier, target, MathHelper.Clamp(dt * 1.5f, 0f, 1f));

        if (severe && !_stormSlowed)
            _toasts.Show(Loc.F("storm.slow", (int)MathF.Round((1f - GameConfig.StormSpeedMultiplier) * 100f)), new Color(170, 210, 255));
        _stormSlowed = severe;
    }

    /// <summary>Tren hasar aldığında hasarla orantılı, hızla sönen ekran sallantısı.</summary>
    private void UpdateScreenShake(float dt)
    {
        float lost = _lastHealth - _train.Health;
        if (lost > 0.05f && IsTravelling) _chapterHealthLost = true;
        if (lost > 0.05f && IsInJourney && _state is not (GameState.GameOver or GameState.Victory))
            AddShake(lost * GameConfig.DamageShakePerPoint);
        _lastHealth = _train.Health;
        _screenShake = MathF.Max(0f, _screenShake - (8f + _screenShake * 6f) * dt);
    }

    private void AddShake(float amount) => _screenShake = MathF.Min(GameConfig.MaxScreenShake, _screenShake + amount);

    /// <summary>Sürüşte olaylara verilen tepkiler: E/BOŞLUK onar-gir, A/D makas, F et at, SHIFT duman.</summary>
    private EventInput ReadEventInput(KeyboardState keys) => new(
        Repair: WasPressed(keys, Keys.E) || WasPressed(keys, Keys.Space),
        Left: WasPressed(keys, Keys.A) || WasPressed(keys, Keys.Left),
        Right: WasPressed(keys, Keys.D) || WasPressed(keys, Keys.Right),
        Bait: WasPressed(keys, Keys.F),
        Smoke: WasPressed(keys, Keys.LeftShift) || WasPressed(keys, Keys.RightShift));

    /// <summary>Sürüş sırasında gece hainleri ve [K] kabin sorgusu.</summary>
    private void UpdateBetrayal(float dt, KeyboardState keys)
    {
        if (WasPressed(keys, Keys.K))
        {
            _cabinMenu.Open();
            _audio.PlayClick();
            return;
        }

        var news = _betrayal.Update(dt, _dayCycle.IsNight, _cabin, _train, _inventory, _events);
        if (news == null) return;

        _toasts.Show(news.Value.Message, news.Value.Color);

        // Evcil hayvan sabotajcıya havlar: şüphe hemen ona yönelir
        if (!news.Value.TraitorCaught && news.Value.Saboteur != null && Pet != PetKind.None)
        {
            news.Value.Saboteur.AddSuspicion(35f);
            _toasts.Show(Loc.F(Pet == PetKind.Cat ? "pet.cat.traitor" : "pet.dog.traitor", news.Value.Saboteur.Name), new Color(255, 200, 120));
            PetCall();
        }
        _audio.PlayAlarm();
        if (news.Value.TraitorCaught)
            SaveData.Record(s => s.TraitorsCaught++);
        else
            SaveData.Record(s => s.TimesSabotaged++); // "İlk İhanet" başarımı
        CheckAchievements();
    }

    /// <summary>
    /// Yolcuyu trenden kov: hainse yakıtın bir kısmı geri alınır ve başarım sayacı artar;
    /// masumsa geride bırakılmış sayılır (vicdan azabı, finali etkiler).
    /// </summary>
    private void EjectPassenger(Passenger passenger)
    {
        _cabin.Remove(passenger);
        if (passenger.IsTraitor)
        {
            float fuel = _betrayal.RecoverFuel();
            _karma += 5;
            _train.Refuel(fuel);
            SaveData.Record(s => s.TraitorsCaught++);
            _chatter.QueueReaction("betray", passenger.Name);
            _toasts.Show(Loc.F("betray.ejectTraitor", passenger.Name, (int)fuel), CabinPanel.Good);
            _audio.PlaySuccess();
            CheckAchievements();
        }
        else
        {
            _abandonedCount++;
            _karma -= 15;
            _chatter.QueueReaction("left", passenger.Name);
            _toasts.Show(Loc.F("betray.ejectInnocent", passenger.Name), CabinPanel.Critical);
            _audio.PlayDeath();
        }
    }

    /// <summary>Ses sahnesini oyun durumundan seçer; AudioManager katmanlar arasında yumuşak geçiş yapar.</summary>
    private void UpdateAudio()
    {
        // Kabin menüsü açıkken kabindeki radyo çalar; Bonuslar ekranında seçilen soundtrack parçası
        _audioManager.RadioModel = DLCManager.Active(CosmeticSlot.Radio).Value;
        _audioManager.SoundtrackTrack = _state == GameState.MainMenu ? _meta.PlayingTrack : -1;
        _audioManager.RadioSignal = _radioPanel.Signal;
        _audioManager.RadioMusic = _radioPanel.Channel?.Kind == RadioChannelKind.Music;
        _audioManager.SetScene(_radioPanel.IsOpen ? AudioScene.Radio : _cabinMenu.IsOpen ? AudioScene.Cabin : _state switch
        {
            GameState.MainMenu when _meta.PlayingTrack >= 0 => AudioScene.Soundtrack,
            GameState.MainMenu or GameState.LanguageSelect or GameState.TutorialPrompt => AudioScene.Menu,
            GameState.Town => _townDialog is TownDialog.Tavern or TownDialog.Bunker ? AudioScene.Tavern : AudioScene.Town,
            GameState.Station or GameState.Depot => AudioScene.Station,
            GameState.FinalChoice => AudioScene.Ending,
            GameState.Mining => AudioScene.Mining,
            GameState.GameOver or GameState.Victory or GameState.EpisodeOutro or GameState.DlcGate => AudioScene.Ending,
            _ => AudioScene.Driving
        });

        bool engineRunning = _state != GameState.GameOver && _state != GameState.Victory;
        _audioManager.Update(_train.SpeedRatio, engineRunning, _weather.Intensity, _weather.Gust, _events.IsBlizzard,
            _isPaused || _photo.IsActive);
    }

    /// <summary>Oyuncu kontrolünde sürüş. Durağa yaklaşınca otomatik frene geçer.</summary>
    private void UpdateDriving(float dt, KeyboardState keys)
    {
        if (keys.IsKeyDown(Keys.W) || keys.IsKeyDown(Keys.Up))
            _train.Throttle(+1, dt);
        if (keys.IsKeyDown(Keys.S) || keys.IsKeyDown(Keys.Down))
            _train.Throttle(-1, dt);

        HandleItemKeys(keys);

        // Hikâye modunda vana patlaması ve kurt sürüsü yalnızca kritik bölümde ya da aşırı fırtınada tam sıklıkta
        _events.HazardScale = !IsStoryMode || CurrentChapter.IsCritical || _weather.Intensity >= GameConfig.ExtremeStorm
            ? 1f : GameConfig.CalmHazardScale;

        // [J]: konuşmak isteyen yolcu (olay yokken)
        TrackQuests();
        if (WasPressed(keys, Keys.J) && _events.Active == null && TryOpenQuest())
            return;

        float kmBefore = _travelledKm;
        AdvanceTrain(dt, _sosKm > 0f ? MathF.Min(NextStopKm, _sosKm) : NextStopKm); // İmdat noktasını geçme
        UpdateCollapse(dt, _travelledKm - kmBefore);
        UpdatePassengers(dt);

        // Buzul Kıyameti: zehirli kar gövdeyi sürekli aşındırır (zırh azaltır)
        if (CurrentChapter.Theme == ChapterTheme.AcidWaste)
            _train.Damage(GameConfig.AcidCorrosionPerSecond * dt);

        if (CheckChapterTransition()) return;

        // Durağa çok yakınken yeni olay başlamaz
        bool allowEvents = NextStopKm - _travelledKm > GameConfig.ApproachKm + GameConfig.EventSafetyMarginKm;

        // Bölümün içindeki hikâye anı (Kurt Baba'nın barikatı, açlık, ninni...): olay yokken, durağa uzakken
        if (allowEvents && _events.Active == null && TryOpenStoryBeat()) return;
        _events.Update(dt, _train, _cabin, _inventory, ReadEventInput(keys), allowEvents);
        if (_state != GameState.Driving) return; // Maden girişine girildi
        UpdateChatter(dt, canStart: _events.Active == null && _towerKm < 0f);
        UpdateBetrayal(dt, keys);
        if (_mode == GameMode.Endless) _endlessRoute.EnsureAhead(_travelledKm);

        // [T]: kabin panosundaki telsiz
        if (WasPressed(keys, Keys.T))
        {
            _radioPanel.Open();
            _audio.PlayClick();
            return;
        }

        // İmdat noktasına yaklaşırken tren kendiliğinden yavaşlar ve durur
        if (_sosKm > 0f)
        {
            float left = _sosKm - _travelledKm;
            if (left < GameConfig.Near(3f))
            {
                float desired = MathHelper.Clamp(left / GameConfig.Near(3f) * GameConfig.MaxSpeed, GameConfig.CreepSpeed, GameConfig.MaxSpeed);
                if (_train.Speed > desired) _train.SetSpeed(desired);
                else if (_train.Speed < GameConfig.CreepSpeed) _train.Throttle(+1, dt);
            }
            if (left <= GameConfig.Near(0.05f))
            {
                _train.Stop();
                ArriveAtSos();
                return;
            }
        }

        if (CheckDeath()) return;

        // Mafya pususu: sürüşte, durağa uzakken ve başka bir olay yokken
        // Hikâye modunda sakin bölümlerde pusu yalnızca aşırı fırtınada kurulur
        bool tense = !IsStoryMode || CurrentChapter.IsCritical || _weather.Intensity >= GameConfig.ExtremeStorm;
        bool allowAmbush = tense && _travelledKm >= CurrentChapter.StartKm + GameConfig.AmbushChapterLeadKm && _events.Active == null
                           && NextStopKm - _travelledKm > GameConfig.ApproachKm + GameConfig.AmbushSafetyKm;
        if (_ambush.Update(dt, allowAmbush, 1f))
        {
            // Episode 2'de pusuyu mafya değil AURA'nın makinistsiz Sentinel trenleri kurar
            if (_mode == GameMode.Episode2) StartSentinel();
            else StartChase();
            return;
        }

        if (UpdateSignalTowers()) return;

        if (NextStopKm - _travelledKm <= GameConfig.ApproachKm)
            ChangeState(GameState.Arriving);
    }

    /// <summary>
    /// Otomatik yanaşma: hedef hız kalan mesafeyle orantılı düşer, böylece tren
    /// tam istasyon önünde yumuşakça durur. Süren olaylar ve eşya kullanımı devam eder.
    /// </summary>
    private void UpdateArriving(float dt, KeyboardState keys)
    {
        float remaining = NextStopKm - _travelledKm;
        float desired = MathHelper.Clamp(remaining / GameConfig.ApproachKm * GameConfig.MaxSpeed,
            GameConfig.CreepSpeed, GameConfig.MaxSpeed);

        if (_train.Speed > desired)
            _train.SetSpeed(desired);
        else if (_train.Speed < GameConfig.CreepSpeed)
            _train.Throttle(+1, dt); // Çok yavaş geldiysek perona kadar sürün

        HandleItemKeys(keys);
        float kmBefore = _travelledKm;
        AdvanceTrain(dt);
        UpdateCollapse(dt, _travelledKm - kmBefore);
        UpdatePassengers(dt);
        _events.Update(dt, _train, _cabin, _inventory, ReadEventInput(keys), allowNewEvents: false);
        if (_state != GameState.Arriving) return;
        UpdateChatter(dt, canStart: false);

        if (CheckDeath()) return;

        if (_travelledKm >= NextStopKm)
        {
            _travelledKm = NextStopKm;
            _train.Stop();

            switch (NextStop.Kind)
            {
                case StopKind.Shelter: ReachShelter(); break;
                case StopKind.Station: ArriveAtStation(); break;
                case StopKind.Town:    FadeTo(ArriveAtTown); break; // Sürüşten kasabaya siyaha kararan geçişle
                case StopKind.Depot:   ArriveAtDepot(); break;
                case StopKind.Hub:     FadeTo(ArriveAtHub); break; // Bölüm sonu: ana istasyona kararan geçişle
            }
        }
    }

    /// <summary>
    /// Kabin yaşamı: kazan soğursa yolcular üşür, soba söner, yolcular acıkır.
    /// İhtiyaçlar bir kez bildirilir; bakımsız yolcu hastalanır, uzun süre aç kalan treni terk eder,
    /// canlılığı tükenen yolcu ölür.
    /// </summary>
    private void UpdatePassengers(float dt)
    {
        _cabin.UpdateNeeds(dt, _train.Temperature, _dayCycle.Night, _sanity.MusicBoxPlaying);

        foreach (var lost in _cabin.UpdateVitality(dt, _train.Temperature))
        {
            _lostCount++;
            _karma -= 5;
            _chatter.QueueReaction("lost", lost.Name);
            _toasts.Show(Loc.F("toast.lost", lost.Name), CabinPanel.Critical);
            _audio.PlayDeath();
        }

        for (int i = _cabin.Count - 1; i >= 0; i--)
        {
            var p = _cabin.Passengers[i];
            if (p.WantsToLeave)
            {
                _cabin.Remove(p);
                _abandonedCount++;
                _karma -= 10;
                _chatter.QueueReaction("left", p.Name);
                _toasts.Show(Loc.F(p.Hunger <= 0f ? "needs.deserted" : "needs.despair", p.Name), CabinPanel.Critical);
                _audio.PlayDeath();
                continue;
            }
            if (p.Hunger < GameConfig.NeedRequestThreshold && !p.HungerRequested)
            {
                p.HungerRequested = true;
                _toasts.Show(Loc.F("needs.hungry", p.Name), CabinPanel.Caution);
                _audio.PlayClick();
            }
        }

        // Soba sönmek üzere: kabin soğuyacak
        if (_cabin.Count > 0 && _cabin.Stove < 20f && !_stoveWarned)
        {
            _stoveWarned = true;
            _toasts.Show(Loc.T("needs.stove"), CabinPanel.Caution);
            _audio.PlayClick();
        }
        else if (_cabin.Stove > 40f)
        {
            _stoveWarned = false;
        }

        // "Dolu Kabin" başarımı: aynı anda taşınan en çok yolcu
        if (_cabin.Count > SaveData.Current.MaxPassengersAtOnce)
        {
            SaveData.Record(s => s.MaxPassengersAtOnce = _cabin.Count);
            CheckAchievements();
        }
    }

    /// <summary>[G]: en aç yolcuya bir erzak paylaştırılır.</summary>
    private void FeedPassenger()
    {
        var hungry = _cabin.HungriestPassenger();
        if (hungry == null) { Reject(Loc.T("needs.notHungry")); return; }
        if (!_inventory.TryConsumeRation()) { Reject(Loc.T("rations.none")); return; }

        hungry.Feed(GameConfig.RationFeed);
        _toasts.Show(Loc.F("needs.fed", hungry.Name, _inventory.Rations), CabinPanel.Good);
        _audio.PlayConfirm();
        SaveData.Record(s => s.PassengersFed++);
        ReportContract(ContractKind.FeedPassengers, 1);
        _karma += 2;
        CheckAchievements();
    }

    /// <summary>[C]: kabin sobasına kömür atılır (trenin yakıtından).</summary>
    private void StokeStove()
    {
        if (_cabin.Stove >= 90f) { Reject(Loc.T("stove.full")); return; }
        if (_train.Fuel <= GameConfig.StoveStokeFuel + 1f) { Reject(Loc.T("stove.noFuel")); return; }

        _train.BurnFuel(GameConfig.StoveStokeFuel);
        _cabin.StokeStove(GameConfig.StoveStokeAmount);
        _audio.PlayShovel();
        _toasts.Show(Loc.F("stove.stoked", (int)GameConfig.StoveStokeFuel), CabinPanel.Good);
    }

    // ------------------------------------------------------------------
    // Envanter
    // ------------------------------------------------------------------

    private void HandleItemKeys(KeyboardState keys)
    {
        // Kabin bakımı: [G] erzak paylaştır, [C] sobaya kömür at
        if (WasPressed(keys, Keys.G)) FeedPassenger();
        if (WasPressed(keys, Keys.C)) StokeStove();
        if (WasPressed(keys, Keys.M)) PlayMusicBox();

        foreach (var type in ItemCatalog.All)
        {
            var (main, numPad) = ItemCatalog.UseKeys(type);
            if (WasPressed(keys, main) || WasPressed(keys, numPad))
                UseItem(type);
        }
    }

    /// <summary>Eşyayı kullanır; işe yaramayacaksa (tren sağlam, hasta yok) harcamaz.</summary>
    private void UseItem(ItemType type)
    {
        string name = ItemCatalog.Name(type);
        if (!_inventory.Has(type))
        {
            Reject(Loc.F("item.none", name));
            return;
        }

        switch (type)
        {
            case ItemType.SpareParts:
                if (_train.IsFullHealth)
                {
                    Reject(Loc.T("item.trainOk"));
                    return;
                }
                _train.Repair(GameConfig.SparePartsRepair);
                Accept(type, Loc.F("item.parts.used", (int)GameConfig.SparePartsRepair));
                break;

            case ItemType.ThermalCoal:
                _train.Refuel(GameConfig.ThermalCoalFuel);
                _train.StokeBoiler(GameConfig.ThermalCoalHeat);
                _train.BurstSmoke(22, 0.7f); // Kazana kömür: bacadan kabaran kara duman
                _audio.PlayShovel();
                Accept(type, Loc.F("item.coal.used", (int)GameConfig.ThermalCoalFuel, (int)GameConfig.ThermalCoalHeat));
                break;

            case ItemType.Medkit:
                var patient = _cabin.MostCriticalPatient();
                if (patient == null)
                {
                    Reject(Loc.T("item.noPatient"));
                    return;
                }
                patient.Heal();
                Accept(type, Loc.F("item.healed", patient.Name));
                break;

            case ItemType.RareMedkit:
                // Nadir Sağlık Kiti: tüm yolcular %100 iyileşir, zehir ve sabotaj hasarı silinir
                if (_cabin.Count == 0)
                {
                    Reject(Loc.T("item.noPatient"));
                    return;
                }
                foreach (var p in _cabin.Passengers)
                    p.CureCompletely();
                Accept(type, Loc.T("item.rareMedkit.used"));
                break;

            case ItemType.WolfBait:
                // Et yalnızca kurt sürüsü saldırırken işe yarar (olay sistemi [F] ile de kullanır)
                if (_events.Active?.Type != RoadEventType.WolfPack)
                {
                    Reject(Loc.T("item.bait.noWolves"));
                    return;
                }
                _events.ThrowBait();
                break;
        }
    }

    private void Accept(ItemType type, string message)
    {
        _inventory.TryConsume(type);
        _toasts.Show(message, CabinPanel.Good);
        _audio.PlayConfirm();
    }

    private void Reject(string message)
    {
        _toasts.Show(message, Hud.Label);
        _audio.PlayError();
    }

    // ------------------------------------------------------------------
    // Yol olayları (ses ve dünya tepkileri)
    // ------------------------------------------------------------------

    private void OnRoadEventStarted(RoadEvent e)
    {
        _audio.PlayAlarm();
        _lastEventType = e.Type;
        switch (e.Type)
        {
            case RoadEventType.SupplyCache:
                _supplyWagonKm = _travelledKm + GameConfig.Near(SupplyWagonLeadKm);
                break;
            case RoadEventType.MineEntrance:
                _mineKm = _travelledKm + GameConfig.Near(SupplyWagonLeadKm);
                break;
            case RoadEventType.Blizzard:
                // Tipi başlarken kazan sert bir soğuk dalgası yer
                _train.DrainHeat(GameConfig.BlizzardHeatDrop);
                _toasts.Show(Loc.F("toast.blizzardCold", (int)GameConfig.BlizzardHeatDrop), new Color(170, 210, 255));
                break;
            case RoadEventType.LampFailure:
                // Ampul patlar: kısa bir cam çatlaması ve kıvılcım
                _train.HeadlightBroken = true;
                _audio.PlayError();
                EmitLampSparks(14);
                break;
        }
    }

    private void OnRoadEventFinished(bool success)
    {
        if (success) _audio.PlaySuccess();
        else _audio.PlayError();

        // "Tipiye Meydan Okuyan": çığa yakalanmadan atlatılan tipiler
        if (success && _lastEventType == RoadEventType.Blizzard)
        {
            SaveData.Record(s => s.BlizzardsSurvived++);
            CheckAchievements();
        }
        _lastEventType = null;
    }

    /// <summary>Arızalı fenere [E] vuruşu: metal sesi ve kıvılcım. Tamamlanırsa başarım sayacı artar.</summary>
    private void OnLampHit(bool repaired)
    {
        _audio.PlayTink();
        EmitLampSparks(repaired ? 18 : 6);
        if (!repaired) return;

        SaveData.Record(s => s.LampsFixed++);
        CheckAchievements();
    }

    private void EmitLampSparks(int count)
    {
        var lamp = _train.HeadlampPosition;
        for (int i = 0; i < count; i++)
        {
            _themeParticles.Emit(new Particle
            {
                Position = lamp + new Vector2(_rng.Next(-4, 5), _rng.Next(-4, 5)),
                Velocity = new Vector2(_rng.Next(-120, 160), -_rng.Next(40, 160)),
                Life = 0.3f + (float)_rng.NextDouble() * 0.4f,
                Size = 1.5f,
                Color = new Color(255, 220, 140),
                Alpha = 1f,
                Gravity = 380f,
                Additive = true
            });
        }
    }

    /// <summary>Kurtlara duman püskürtüldü: kısa bir beyaz bulut ve tıslama.</summary>
    private void OnWolfSmoke()
    {
        _wolfSmokeFlash = 1f;
        _audio.PlaySteamBlast();
        _train.BurstSmoke(30, 1f);
        for (int i = 0; i < 30; i++)
        {
            _themeParticles.Emit(new Particle
            {
                Position = new Vector2(Train.RearX + _rng.Next(0, Train.FrontX - Train.RearX), GameConfig.RailY - 10 - _rng.Next(0, 30)),
                Velocity = new Vector2(_rng.Next(-260, 60), -_rng.Next(20, 120)),
                Life = 0.7f + (float)_rng.NextDouble() * 0.5f,
                Size = 9f + _rng.Next(8),
                Growth = 3f,
                Color = new Color(220, 228, 240),
                Alpha = 0.7f,
                Soft = true
            });
        }
    }

    // ------------------------------------------------------------------
    // Terk edilmiş maden (mini oyun)
    // ------------------------------------------------------------------

    private void EnterMine()
    {
        _mine.Start(SaveData.HasUpgrade(UpgradeType.Lantern));
        _audio.PlayWhistle();
        FadeTo(() => ChangeState(GameState.Mining));
    }

    /// <summary>Maden bitince bulunanlar kargoya yüklenir; tren yola devam eder.</summary>
    private void UpdateMining(float dt, InputState input)
    {
        _mine.Update(dt, input, _audio);
        if (!_mine.IsDone) return;

        // Acımasız Kış: madenden çıkan yükün yarısı yolda kalır
        int carried = Difficulty.Scale(_mine.Found.Count);
        int loaded = 0, ores = 0;
        for (int i = 0; i < carried; i++)
        {
            var loot = _mine.Found[i];
            if (!_inventory.Cargo.TryAdd(loot)) break;
            loaded++;
            if (loot is LootType.GoldNugget or LootType.RareOre) ores++;
        }
        if (ores > 0) SaveData.Record(s => s.OresMined += ores);
        ReportContract(ContractKind.MineOre, ores);

        string message = loaded < carried
            ? Loc.F("mine.result.full", loaded, carried - loaded)
            : Loc.F("mine.result", loaded);
        _toasts.Show(message, loaded > 0 ? CabinPanel.Good : Hud.Label);
        if (loaded > 0) _audio.PlayCoin();
        CheckAchievements();
        FadeTo(() => ChangeState(GameState.Driving));
    }

    // ------------------------------------------------------------------
    // İstasyon ve ahlaki seçimler
    // ------------------------------------------------------------------

    private void ArriveAtStation()
    {
        _events.Clear();
        _supplyWagonKm = -1f;
        _audio.PlayWhistle();

        // Yolcular bir kademe iyileşir, varsa mühendis treni onarır
        float repaired = _cabin.OnArrival(_train);
        _arrivalNote = repaired > 0f
            ? Loc.F("sum.engineer", _cabin.FindBySkill(PassengerSkill.Engineer).Name, (int)MathF.Round(repaired))
            : null;

        // İlk dört istasyon hikâye senaryosu; sonrakilerde peronda bir yabancı bekler (hain olabilir)
        string text;
        if (NextStop.Index < StoryData.Scenarios.Length)
        {
            _currentScenario = StoryData.Scenarios[NextStop.Index];
            text = _currentScenario.Text;
        }
        else
        {
            var stranger = StrangerCatalog.Pick(_rng, _cabin);
            if (ModLoader.Dialogs.Count > 0 && _rng.Next(2) == 0)
            {
                // Topluluk modlarından gelen diyaloglar yabancılı istasyonlara karışır
                var (key, modDialog) = ModLoader.Dialogs[_rng.Next(ModLoader.Dialogs.Count)];
                _currentScenario = ModLoader.BuildScenario(key, modDialog, stranger);
                text = SafeFormat(_currentScenario.Text, stranger?.Name, stranger?.Profession); // Mod metni {0} ad, {1} meslek içerebilir
            }
            else
            {
                _currentScenario = new StationScenario("stranger",
                    stranger != null ? new ChoiceOption("stranger.a", Passenger: stranger) : new ChoiceOption("stranger.empty"),
                    new ChoiceOption("stranger.b", Fuel: 15f));
                text = stranger != null
                    ? Loc.F("stranger.text", stranger.Name, stranger.Profession)
                    : Loc.T("stranger.emptyText");
            }
        }

        _dialog.Open(_currentScenario.Title, text, new[]
        {
            BuildOption("1", _currentScenario.OptionA),
            BuildOption("2", _currentScenario.OptionB)
        }, speaker: ArthurSpeaker);

        _stationPhase = StationPhase.Choosing;
        ChangeState(GameState.Station);
    }

    /// <summary>Kabin doluysa yolcu alan seçenek kilitlenir.</summary>
    private DialogOption BuildOption(string key, ChoiceOption choice)
    {
        bool blocked = choice.TakesPassenger && _cabin.IsFull;
        string label = blocked ? Loc.F("opt.cabinFull", choice.Label) : choice.Label;
        return new DialogOption(key, label, !blocked);
    }

    /// <summary>
    /// Seçim evresi: [1]/[2] karar verir. Sonuç evresi: SPACE pencereyi kapatır.
    /// Her iki evrede de SPACE önce daktilo efektini atlar.
    /// </summary>
    private void UpdateStation(float dt, KeyboardState keys)
    {
        _dialog.Update(dt);

        if (_dialog.IsClosed)
        {
            Depart();
            return;
        }

        if (WasPressed(keys, Keys.Space) && !_dialog.IsComplete)
        {
            _dialog.Skip();
            return;
        }

        var scenario = _currentScenario;

        // İstasyonda da şüphelenilen yolcu kovulabilir
        if (WasPressed(keys, Keys.K) && _cabin.Count > 0)
        {
            _cabinMenu.Open();
            _audio.PlayClick();
            return;
        }

        if (_stationPhase == StationPhase.Choosing && _dialog.AcceptsChoice)
        {
            if (WasPressed(keys, Keys.D1) || WasPressed(keys, Keys.NumPad1))
                TryChoose(scenario, scenario.OptionA);
            else if (WasPressed(keys, Keys.D2) || WasPressed(keys, Keys.NumPad2))
                TryChoose(scenario, scenario.OptionB);
        }
        else if (_stationPhase == StationPhase.Outcome && _dialog.IsComplete && WasPressed(keys, Keys.Space))
        {
            _dialog.Close();
        }
    }

    private void TryChoose(StationScenario scenario, ChoiceOption choice)
    {
        if (choice.TakesPassenger && _cabin.IsFull)
        {
            _audio.PlayError(); // Kilitli seçenek
            return;
        }
        _audio.PlayConfirm();

        // İstasyon deposu her durumda biraz yakıt verir (imdat noktasında depo yok)
        var summary = new List<string>();
        if (!_atSos)
        {
            _train.Refuel(GameConfig.StationRefuel);
            int looted = CollectLoot(GameConfig.StationLoot); // Depoda kalan eski dünya eşyaları kargoya
            summary.Add(Loc.F("sum.depot", (int)GameConfig.StationRefuel));
            if (looted > 0) summary.Add(Loc.F("sum.loot", looted));
        }

        if (choice.TakesPassenger)
        {
            _cabin.TryBoard(choice.Passenger);
            _karma += 10; // Birine kapıyı açmak
            ReportContract(ContractKind.Rescue, 1);
            summary.Add(Loc.F("sum.passenger", choice.Passenger.Name, choice.Passenger.SkillTag));
        }
        else if (scenario.OptionA.TakesPassenger || scenario.OptionB.TakesPassenger)
        {
            _abandonedCount++; // Birini geride bırakmayı seçti
            _karma -= 10;
        }

        // Seçeneğe özgü karma, akıl sağlığı ve erzak etkileri (imdat görevleri, mod diyalogları)
        _karma += choice.Karma;
        if (choice.Sanity != 0f) _sanity.Restore(choice.Sanity);
        if (choice.Rations > 0)
            summary.Add(Loc.F("rations.bought", _inventory.AddRations(choice.Rations)));
        else
            for (int i = 0; i < -choice.Rations; i++) _inventory.TryConsumeRation();

        if (choice.Fuel > 0f)
        {
            _train.Refuel(choice.Fuel);
            summary.Add(Loc.F("sum.fuel", (int)choice.Fuel));
        }
        if (choice.Health > 0f)
        {
            _train.Repair(choice.Health);
            summary.Add(Loc.F("sum.health", (int)choice.Health));
        }
        if (choice.Items != null)
        {
            foreach (var item in choice.Items)
            {
                summary.Add(_inventory.TryAdd(item)
                    ? Loc.F("sum.item", ItemCatalog.Name(item))
                    : Loc.F("sum.itemLost", ItemCatalog.Name(item)));
            }
        }
        if (_arrivalNote != null)
            summary.Add(_arrivalNote);

        // Pencere sonuç metniyle yeniden, yumuşakça açılır
        _dialog.Open(scenario.Title, choice.Outcome + "\n\n" + string.Join("\n", summary), speaker: ArthurSpeaker);
        _stationPhase = StationPhase.Outcome;
    }

    private void Depart()
    {
        if (_atSos)
        {
            // İmdat noktası hattaki bir durak değildir: sıradaki durak aynı kalır
            _atSos = false;
            _audio.PlayWhistle();
            ChangeState(GameState.Driving);
            return;
        }
        _train.StokeBoiler(GameConfig.StationTemperatureBoost);
        _nextStopIndex++;
        _collapseGapKm = MathF.Max(_collapseGapKm, GameConfig.CollapseMinGapAfterStop); // Durakta soluklanan yarış biraz rahatlar
        _audio.PlayWhistle();
        ChangeState(GameState.Driving);
    }

    // ------------------------------------------------------------------
    // Kasaba (yürüyüş modu)
    // ------------------------------------------------------------------

    private TownInfo CurrentTown => _townScene.Town;

    private void ArriveAtTown()
    {
        _events.Clear();
        _supplyWagonKm = -1f;
        _audio.PlayWhistle();

        // Bazı kasabalarda bir hırsız, Arthur trenden uzaklaşınca kargoya sinsice yaklaşır
        float thiefDelay = _rng.NextDouble() < GameConfig.ThiefChance ? 6f + (float)_rng.NextDouble() * 10f : -1f;
        _townScene.Enter(TownCatalog.All[NextStop.Index], (int)NextStop.Km, thiefDelay);
        _townDialog = TownDialog.None;
        ChangeState(GameState.Town);
    }

    /// <summary>
    /// Arthur A/D ile yürür, kamera onu izler (arka plan parallax ile kayar).
    /// Kasabada tren park halindedir: kaynak harcanmaz, yolcular sıcakta dinlenir.
    /// Hırsız kargoya yaklaşırsa uyarı çıkar; vaktinde yakalanmazsa yağma çalınır.
    /// </summary>
    private void UpdateTown(float dt, InputState input)
    {
        float cameraDelta = _townScene.Update(dt, input, canMove: _townDialog == TownDialog.None);
        _mountains.Update(cameraDelta);
        _pines.Update(cameraDelta);
        _ground.Update(cameraDelta);

        if (_townScene.ConsumeThiefAlert())
        {
            if (Pet != PetKind.None)
            {
                _toasts.Show(Loc.T(Pet == PetKind.Cat ? "pet.cat.thief" : "pet.dog.thief"), new Color(255, 200, 120));
                PetCall();
            }
            _toasts.Show(Loc.T("thief.alert"), Hud.Danger);
            _audio.PlayAlarm();
        }
        if (_townScene.ConsumeTheft())
        {
            int stolen = _inventory.Cargo.Steal(2).Count;
            _toasts.Show(stolen > 0 ? Loc.F("thief.stole", stolen) : Loc.T("thief.nothing"), Hud.Danger);
            _audio.PlayError();
        }

        if (_townDialog != TownDialog.None)
        {
            UpdateTownDialog(dt, input);
            return;
        }

        TrackQuests();
        if (input.Pressed(Keys.J) && TryOpenQuest())
            return;

        if (input.Pressed(Keys.K) && _cabin.Count > 0)
        {
            _cabinMenu.Open();
            _audio.PlayClick();
            return;
        }

        if (input.Pressed(Keys.G) && !CurrentTown.IsStoryScene) // Tren deposu / garaj
        {
            _garage.Open();
            _audio.PlayClick();
            return;
        }

        if (input.Pressed(Keys.E) && _townScene.Nearest != null)
            Interact(_townScene.Nearest);
    }

    private void Interact(Interactable target)
    {
        switch (target.Kind)
        {
            case InteractableKind.Train when CurrentTown.IsStoryScene:
            {
                // Keşif bölümü: önemli bulgular okunmadan trene dönülmez
                string done = NarrativeData.SceneDoneFlag(CurrentTown.Archetype);
                if (done != null && !Narrative.Has(done))
                {
                    _audio.PlayError();
                    _toasts.Show(Loc.T($"scene.locked.{CurrentTown.Archetype}"), Hud.Amber);
                    break;
                }
                if (done == null)
                {
                    _story.Open(this, NarrativeData.SceneStart(CurrentTown.Archetype), CurrentTown.Name); // Diyalog yeniden
                    break;
                }
                _audio.PlayConfirm();
                FadeTo(FinishStoryScene);
                break;
            }

            case InteractableKind.Train:
                _audio.PlayConfirm();
                FadeTo(Depart); // Kasabadan sürüşe siyaha kararan geçişle
                break;

            case InteractableKind.Spot:
                OpenSpot(target.SpotId);
                break;

            case InteractableKind.Tavern:
                OpenTavern();
                break;

            case InteractableKind.Store:
                OpenStore();
                break;

            case InteractableKind.Mechanic:
                OpenMechanic();
                break;

            case InteractableKind.Bunker:
                OpenBunker();
                break;

            case InteractableKind.Peddler:
                OpenPeddler();
                break;

            case InteractableKind.Thief:
                _townScene.CatchThief();
                _toasts.Show(Loc.T("thief.caught"), CabinPanel.Good);
                _audio.PlaySuccess();
                break;

            case InteractableKind.Npc:
                _audio.PlayClick();
                ShowTownResult(CurrentTown.NpcName, CurrentTown.NpcLine + NpcGift(), new Speaker(CurrentTown.NpcName, CurrentTown.NpcPortrait));
                break;
        }
    }

    private static Speaker PeddlerSpeaker => new(Loc.T("speaker.peddler"), PortraitKind.Peddler);

    /// <summary>Seyyar satıcı: piyasadan ucuz yakıt ve parça... ama mallar her zaman gerçek değil.</summary>
    private void OpenPeddler()
    {
        _dialog.Open(Loc.T("peddler.title"), Loc.T("peddler.text"), new[]
        {
            new DialogOption("1", Loc.F("peddler.opt.fuel", GameConfig.PeddlerFuelPrice, (int)GameConfig.PeddlerFuelAmount),
                SaveData.Current.Credits >= GameConfig.PeddlerFuelPrice),
            new DialogOption("2", Loc.F("peddler.opt.parts", GameConfig.PeddlerPartsPrice),
                SaveData.Current.Credits >= GameConfig.PeddlerPartsPrice),
            new DialogOption("3", Loc.T("peddler.opt.leave"), true)
        }, speaker: PeddlerSpeaker);
        _townDialog = TownDialog.Peddler;
        _audio.PlayClick();
    }

    /// <summary>Satıcıdan alışveriş: yarı yarıya ihtimalle mal sahte çıkar (sulandırılmış yakıt, paslı hurda).</summary>
    private void BuyFromPeddler(bool fuel)
    {
        int price = fuel ? GameConfig.PeddlerFuelPrice : GameConfig.PeddlerPartsPrice;
        if (!SaveData.TrySpend(price))
        {
            Reject(Loc.T("trade.noCredits"));
            return;
        }

        bool scam = _rng.NextDouble() < GameConfig.PeddlerScamChance;
        string result;
        if (fuel)
        {
            float amount = scam ? GameConfig.PeddlerScamFuel : GameConfig.PeddlerFuelAmount;
            _train.Refuel(amount);
            result = scam ? Loc.F("peddler.scam.fuel", (int)amount) : Loc.F("peddler.ok.fuel", (int)amount);
        }
        else if (scam)
        {
            result = Loc.T("peddler.scam.parts");
        }
        else
        {
            result = _inventory.TryAdd(ItemType.SpareParts) ? Loc.T("peddler.ok.parts") : Loc.T("store.full");
        }

        if (scam) _audio.PlayError(); else _audio.PlayCoin();
        ShowTownResult(Loc.T("peddler.title"), result, PeddlerSpeaker);
    }

    // Diyaloglarda konuşan kişiler (ad + portre)
    private static Speaker ArthurSpeaker => new(Loc.T("speaker.arthur"), PortraitKind.Arthur) { IsPlayer = true };
    private static Speaker InnkeeperSpeaker => new(Loc.T("speaker.innkeeper"), PortraitKind.Innkeeper);
    private static Speaker SmithSpeaker => new(Loc.T("speaker.smith"), PortraitKind.Smith);
    private static Speaker BunkerSpeaker => new(Loc.T("speaker.bunker"), PortraitKind.BunkerGuard);
    private static Speaker PassengerSpeaker(PassengerProfile p) => new(p.Name, Portraits.ForPassenger(p.Id));

    private void OpenTavern()
    {
        int i = CurrentTown.Number - 1;
        var passenger = CurrentTown.Passenger;

        string gossip = Loc.T("tavern.opt.gossip");
        string meet = Loc.F("tavern.opt.passenger", passenger.Name, passenger.Profession);

        _dialog.Open(Loc.F("tavern.title", CurrentTown.Name), CurrentTown.TavernText, new[]
        {
            new DialogOption("1", _gossipHeard[i] ? Loc.F("opt.heard", gossip) : gossip, !_gossipHeard[i]),
            new DialogOption("2", _passengerHired[i] ? Loc.F("opt.aboard", meet) : meet, !_passengerHired[i]),
            new DialogOption("3", Loc.F("tavern.opt.soup", GameConfig.TavernSoupPrice), true),
            new DialogOption("4", Loc.T("tavern.opt.leave"), true)
        }, speaker: InnkeeperSpeaker);
        _townDialog = TownDialog.Tavern;
        _audio.PlayClick();
    }

    private void OpenMechanic()
    {
        bool canPay = SaveData.Current.Credits >= GameConfig.MechanicCreditCost;
        bool hasParts = _inventory.Has(ItemType.SpareParts);

        _dialog.Open(Loc.F("mech.title", CurrentTown.Name), Loc.T("mech.text"), new[]
        {
            new DialogOption("1", Loc.F("mech.opt.credits", GameConfig.MechanicCreditCost, (int)GameConfig.MechanicCreditRepair), canPay),
            new DialogOption("2", Loc.F("mech.opt.parts", (int)GameConfig.MechanicPartsRepair), hasParts),
            new DialogOption("3", Loc.T("mech.opt.leave"), true)
        }, speaker: SmithSpeaker);
        _townDialog = TownDialog.Mechanic;
        _audio.PlayClick();
    }

    /// <summary>Erzak deposu: kasabanın tüccarı (yağma sat, yakıt/tamir/eşya al).</summary>
    private void OpenStore()
    {
        _trader.Open(TraderKind.Town, Loc.F("trader.town", CurrentTown.Name));
        _townDialog = TownDialog.Store;
        _audio.PlayClick();
    }

    /// <summary>Sığınak girişi: yolcular bir kez dinlenir, kazan sığınağın kazan dairesinden ısınır.</summary>
    private void OpenBunker()
    {
        int i = CurrentTown.Number - 1;
        string rest = Loc.F("bunker.opt.rest", (int)GameConfig.BunkerVitality);

        _dialog.Open(Loc.F("bunker.title", CurrentTown.Name), Loc.T("bunker.text"), new[]
        {
            new DialogOption("1", _bunkerUsed[i] ? Loc.F("opt.heard", rest) : rest, !_bunkerUsed[i]),
            new DialogOption("2", Loc.T("bunker.opt.leave"), true)
        }, speaker: BunkerSpeaker);
        _townDialog = TownDialog.Bunker;
        _audio.PlayClick();
    }

    private void RestAtBunker()
    {
        int i = CurrentTown.Number - 1;
        if (_bunkerUsed[i])
        {
            _audio.PlayError();
            return;
        }

        _bunkerUsed[i] = true;
        foreach (var passenger in _cabin.Passengers)
            passenger.Rest(GameConfig.BunkerVitality);
        _train.StokeBoiler(GameConfig.BunkerHeat);
        _audio.PlayConfirm();

        string summary = Loc.F("sum.heat", (int)GameConfig.BunkerHeat);
        if (_cabin.Count > 0)
            summary = Loc.F("sum.vitality", (int)GameConfig.BunkerVitality) + "\n" + summary;
        ShowTownResult(Loc.F("bunker.title", CurrentTown.Name), Loc.T("bunker.rested") + "\n\n" + summary, BunkerSpeaker);
    }

    /// <summary>Seçeneksiz bilgi penceresi; SPACE ile kapanır ve yürüyüşe dönülür.</summary>
    private void ShowTownResult(string title, string text, Speaker speaker)
    {
        _dialog.Open(title, text, null, "prompt.continue", speaker);
        _townDialog = TownDialog.Result;
    }

    private void UpdateTownDialog(float dt, InputState input)
    {
        // Erzak deposu: tüccar ekranı açık
        if (_townDialog == TownDialog.Store)
        {
            _trader.Update(dt, input, _train, _inventory, _cabin, _audio);
            if (_trader.IsFinished) _townDialog = TownDialog.None;
            return;
        }

        _dialog.Update(dt);

        if (_dialog.IsClosed)
        {
            _townDialog = TownDialog.None;
            return;
        }

        if (input.Pressed(Keys.Space) && !_dialog.IsComplete)
        {
            _dialog.Skip();
            return;
        }

        bool one = input.AnyPressed(Keys.D1, Keys.NumPad1);
        bool two = input.AnyPressed(Keys.D2, Keys.NumPad2);
        bool three = input.AnyPressed(Keys.D3, Keys.NumPad3);
        bool four = input.AnyPressed(Keys.D4, Keys.NumPad4);

        switch (_townDialog)
        {
            case TownDialog.Peddler when _dialog.AcceptsChoice:
                if (one) BuyFromPeddler(fuel: true);
                else if (two) BuyFromPeddler(fuel: false);
                else if (three) _dialog.Close();
                break;

            case TownDialog.Bunker when _dialog.AcceptsChoice:
                if (one) RestAtBunker();
                else if (two) _dialog.Close();
                break;

            case TownDialog.Tavern when _dialog.AcceptsChoice:
                if (one) ChooseGossip();
                else if (two) ChooseTownPassenger();
                else if (three) BuySoup();
                else if (four) _dialog.Close();
                break;

            case TownDialog.Mechanic when _dialog.AcceptsChoice:
                if (one) RepairAtMechanic(useCredits: true);
                else if (two) RepairAtMechanic(useCredits: false);
                else if (three) _dialog.Close();
                break;

            case TownDialog.Result:
                if (_dialog.IsComplete && input.Pressed(Keys.Space))
                    _dialog.Close();
                break;
        }
    }

    /// <summary>Dedikodu: yol bilgisi ve sonraki mafya pususu için istihbarat.</summary>
    private void ChooseGossip()
    {
        int i = CurrentTown.Number - 1;
        if (_gossipHeard[i])
        {
            _audio.PlayError();
            return;
        }

        _gossipHeard[i] = true;
        _ambushIntel = true;
        _audio.PlayConfirm();
        ShowTownResult(Loc.F("tavern.title", CurrentTown.Name),
            CurrentTown.Gossip + "\n\n" + Loc.T("tavern.gossip.effect"), InnkeeperSpeaker);
    }

    /// <summary>Tavernada sıcak çorba ve bir kadeh: Arthur'un aklı ve yolcuların morali toparlanır.</summary>
    private void BuySoup()
    {
        if (!SaveData.TrySpend(GameConfig.TavernSoupPrice))
        {
            _audio.PlayError();
            _toasts.Show(Loc.T("trade.noCredits"), Hud.Danger);
            return;
        }
        _sanity.Restore(GameConfig.TavernSoupSanity);
        foreach (var p in _cabin.Passengers) p.LiftMorale(GameConfig.TavernSoupMorale);
        _audio.PlayCoin();
        ShowTownResult(Loc.F("tavern.title", CurrentTown.Name), Loc.T("tavern.soup.done"), InnkeeperSpeaker);
    }

    private void ChooseTownPassenger()
    {
        int i = CurrentTown.Number - 1;
        var passenger = CurrentTown.Passenger;
        if (_passengerHired[i])
        {
            _audio.PlayError();
            return;
        }

        if (_cabin.IsFull)
        {
            _audio.PlayError();
            ShowTownResult(Loc.F("tavern.title", CurrentTown.Name), Loc.F("tavern.full", passenger.Name), PassengerSpeaker(passenger));
            return;
        }

        _cabin.TryBoard(passenger);
        _passengerHired[i] = true;
        ReportContract(ContractKind.Rescue, 1);
        _karma += 10;
        _audio.PlayConfirm();
        ShowTownResult(Loc.F("tavern.title", CurrentTown.Name),
            CurrentTown.JoinText + "\n\n" + Loc.F("sum.passenger", passenger.Name, passenger.SkillTag), PassengerSpeaker(passenger));
    }

    /// <summary>Tamirci: hurdayla ya da getirilen yedek parçayla tren sağlığını onarır.</summary>
    private void RepairAtMechanic(bool useCredits)
    {
        if (_train.IsFullHealth)
        {
            Reject(Loc.T("item.trainOk"));
            return;
        }

        float repaired;
        string cost;
        if (useCredits)
        {
            if (!SaveData.TrySpend(GameConfig.MechanicCreditCost))
            {
                Reject(Loc.T("trade.noCredits"));
                return;
            }
            repaired = GameConfig.MechanicCreditRepair;
            cost = Loc.F("sum.creditsSpent", GameConfig.MechanicCreditCost);
        }
        else
        {
            if (!_inventory.TryConsume(ItemType.SpareParts))
            {
                Reject(Loc.T("mech.noParts"));
                return;
            }
            repaired = GameConfig.MechanicPartsRepair;
            cost = Loc.F("sum.itemSpent", ItemCatalog.Name(ItemType.SpareParts));
        }

        float before = _train.Health;
        _train.Repair(repaired);
        _audio.PlayExplosion(); // Örs ve çekiç gümbürtüsü
        ShowTownResult(Loc.F("mech.title", CurrentTown.Name),
            Loc.T("mech.done") + "\n\n" + Loc.F("sum.health", (int)MathF.Round(_train.Health - before)) + "\n" + cost, SmithSpeaker);
    }

    // ------------------------------------------------------------------
    // Buzul benzinliği
    // ------------------------------------------------------------------

    private void ArriveAtDepot()
    {
        _events.Clear();
        _supplyWagonKm = -1f;
        _audio.PlayWhistle();

        // Zamanla yarışta pazarlık yok: hızlı ikmal ve hemen yola devam
        if (_mode == GameMode.Speedrun)
        {
            _train.Refuel(GameConfig.SpeedrunQuickRefuel);
            _toasts.Show(Loc.F("speedrun.refuel", GameConfig.SpeedrunQuickRefuel), CabinPanel.Good);
            Depart();
            return;
        }

        _trader.Open(TraderKind.Depot, Loc.T("trader.depot"));
        ChangeState(GameState.Depot);
    }

    /// <summary>
    /// Bölüm sonu ana istasyonu: bölüm tamamlanır (sıradaki bölümün kilidi açılır, ödül Kredisi),
    /// atölye treni ücretsiz biraz onarır ve büyük pazar (tren geliştirmeleriyle) açılır.
    /// </summary>
    private void ArriveAtHub()
    {
        _events.Clear();
        _supplyWagonKm = -1f;
        _audio.PlayWhistle();
        _audio.PlaySuccess();

        _cabin.OnArrival(_train);
        _train.Repair(GameConfig.HubRepair);
        _trader.Open(TraderKind.Hub, Loc.F("trader.hub", NextStop.Index + 1));
        ChangeState(GameState.Depot);
    }

    private void UpdateDepot(float dt, InputState input)
    {
        if (input.Pressed(Keys.G)) // Tren deposu / garaj (benzinlik ve ana istasyon)
        {
            _garage.Open();
            _audio.PlayClick();
            return;
        }
        _trader.Update(dt, input, _train, _inventory, _cabin, _audio);
        if (_trader.IsFinished)
            Depart();
    }

    // ------------------------------------------------------------------
    // Yağma
    // ------------------------------------------------------------------

    /// <summary>Rastgele yağma toplar; kargo dolarsa kalanı geride kalır. Toplanan sayıyı döner.</summary>
    private int CollectLoot(int count)
    {
        int collected = 0;
        for (int i = 0; i < count; i++)
        {
            var loot = LootCatalog.Roll(_rng);
            if (!_inventory.Cargo.TryAdd(loot))
            {
                _toasts.Show(Loc.T("res.cache.full"), Hud.Danger);
                break;
            }
            collected++;
        }
        return collected;
    }

    /// <summary>Kasaba NPC'si ilk konuşmada kıymetli bir yağma hediye eder.</summary>
    private string NpcGift()
    {
        int i = CurrentTown.Number - 1;
        if (_npcGiftGiven[i]) return string.Empty;
        _npcGiftGiven[i] = true;

        var gift = _rng.Next(2) == 0 ? LootType.MusicBox : LootType.Radiator;
        if (!_inventory.Cargo.TryAdd(gift)) return "\n\n" + Loc.T("res.cache.full");
        _audio.PlayCoin();
        return "\n\n" + Loc.F("npc.gift", LootCatalog.Name(gift));
    }

    private void OnLootCollected(IReadOnlyList<LootType> loot)
    {
        foreach (var item in loot)
            _toasts.Show(Loc.F("toast.loot", LootCatalog.Name(item)), LootCatalog.Color(item));
        _audio.PlayCoin();
    }

    // ------------------------------------------------------------------
    // Bölüm temaları
    // ------------------------------------------------------------------

    /// <summary>
    /// Bölüm 3 — buzul çöküşü: çöküş cephesi sabit hızla ilerler; tren daha yavaş giderse aradaki
    /// mesafe azalır. Cephe yetişirse raylar çöker: tren her saniye hasar alır ve ekran sarsılır.
    /// </summary>
    private void UpdateCollapse(float dt, float kmMoved)
    {
        if (!CurrentChapter.GlacierCollapse) return;

        float frontKm = GameConfig.CollapseSpeed * GameConfig.KmPerSecondPerSpeed * dt;
        _collapseGapKm = MathHelper.Clamp(_collapseGapKm + kmMoved - frontKm, 0f, GameConfig.CollapseMaxGapKm);

        if (_collapseGapKm <= 0f)
        {
            _train.Damage(GameConfig.CollapseDamagePerSecond * dt);
            _collapseShake = 6f;
        }
    }

    /// <summary>Düşen buz parçaları (çöküş yaklaşırken) ve tünel tavanından damlayan su.</summary>
    private void UpdateThemeEffects(float dt)
    {
        _collapseShake = MathF.Max(0f, _collapseShake - dt * 10f);
        bool onRoute = _state is GameState.Driving or GameState.Arriving;

        if (onRoute && CurrentChapter.GlacierCollapse && _collapseGapKm < 6f && _rng.NextDouble() < dt * 30f)
        {
            float closeness = 1f - _collapseGapKm / 6f;
            _themeParticles.Emit(new Particle
            {
                Position = new Vector2(_rng.Next(0, (int)(80 + 300 * closeness)), -10),
                Velocity = new Vector2(-_rng.Next(20, 120), _rng.Next(200, 400)),
                Life = 1.6f,
                Size = 3f + _rng.Next(5),
                Color = new Color(200, 230, 250),
                Alpha = 0.9f,
                Gravity = 600f
            });
        }

        if (onRoute && CurrentChapter.Theme == ChapterTheme.Tunnels && _rng.NextDouble() < dt * 6f)
        {
            _themeParticles.Emit(new Particle
            {
                Position = new Vector2(_rng.Next(0, GameConfig.ScreenWidth), 120),
                Velocity = new Vector2(-_train.ScrollSpeed * 0.3f, 60),
                Life = 1.2f,
                Size = 1.5f,
                Color = new Color(150, 190, 220),
                Alpha = 0.7f,
                Gravity = 500f
            });
        }

        _themeParticles.Update(dt, -_train.ScrollSpeed * 0.5f);
    }

    // ------------------------------------------------------------------
    // Mafya pususu ve kovalamaca
    // ------------------------------------------------------------------

    private void StartChase()
    {
        _events.Clear();
        _supplyWagonKm = -1f;
        _chase.Start(_ambushIntel, _cabin.Has(PassengerSkill.Guard));
        _ambushIntel = false;
        _audio.PlayAlarm();
        _audio.PlayExplosion();
        AddShake(GameConfig.MaxScreenShake); // Pusu: sert bir sarsıntıyla başlar
        ChangeState(GameState.Chase);
    }

    /// <summary>
    /// Kaçış: [W] tam gaz, [SHIFT] buhar püskürtme, ok tuşlarıyla engel kombinasyonları.
    /// Yol devam eder ama sıradaki durağın yanaşma bölgesine kovalamaca sırasında girilmez.
    /// </summary>
    // ------------------------------------------------------------------
    // Episode 2: Sentinel trenleri ve AURA sinyal kuleleri
    // ------------------------------------------------------------------

    private void StartSentinel()
    {
        _events.Clear();
        _supplyWagonKm = -1f;
        _towerKm = -1f;
        _sentinel.AttackMultiplier = _chase.AttackMultiplier;
        _sentinel.Start();
        _audio.PlayAlarm();
        AddShake(10f);
        ChangeState(GameState.Sentinel);
    }

    /// <summary>Hack'e yardım eden yolcu: telsiz korsanı Nova ya da herhangi bir telsizci.</summary>
    private bool HackHelper => _cabin.HasProfile("nova") || _cabin.Has(PassengerSkill.RadioOperator);

    private float HackDifficulty => MathHelper.Clamp((CurrentChapter.Number - 1) / 39f, 0f, 1f);

    private void UpdateSentinel(float dt, KeyboardState keys, InputState input)
    {
        if (keys.IsKeyDown(Keys.W)) _train.Throttle(+1, dt);
        if (keys.IsKeyDown(Keys.S)) _train.Throttle(-1, dt);
        HandleItemKeys(keys);
        AdvanceTrain(dt, NextStopKm - GameConfig.ApproachKm - GameConfig.Near(0.5f));
        UpdatePassengers(dt);

        // [H]: AURA'nın frekansına gir. Hack açıkken Sentinel ateş etmeyi sürdürür (gerçek zamanlı).
        if (_hack.IsOpen)
        {
            var result = _hack.Update(dt, input, _audio);
            if (result == HackResult.Success) _sentinel.ApplyHack(_hack.ChosenAction, _audio);
            else if (result == HackResult.Failed) _toasts.Show(Loc.T("hack.failedToast"), Hud.Danger);
        }
        else if (!_sentinel.InIntro && _sentinel.Outcome == ChaseOutcome.None && WasPressed(keys, Keys.H))
        {
            _hack.Open(withActions: true, HackDifficulty, HackHelper, _sentinel.CanSwitchRails);
            _audio.PlayClick();
        }

        _sentinel.Update(dt, _train, _audio);
        if (CheckDeath()) return;

        if (_sentinel.IsFinished)
        {
            _hack.Close();
            SaveData.Earn(GameConfig.SentinelEscapeCredits);
            SaveData.Record(s => s.AmbushesEscaped++);
            ReportContract(ContractKind.EscapeAmbush, 1);
            _toasts.Show(_sentinel.ResultMessage, CabinPanel.Good);
            CheckAchievements();
            ChangeState(GameState.Driving);
        }
    }

    /// <summary>
    /// Sinyal Savaşı bölümlerinde (3-10) hattın kenarında AURA kuleleri belirir. Kule yanından geçerken [H] ile
    /// hacklenirse Kredi ve istihbarat; hacklenmezse ya da hack başarısızsa parazit treni sarsar, akıl sağlığı düşer.
    /// </summary>
    /// <returns>Hack açıldıysa true (bu kare sürüş durur).</returns>
    private bool UpdateSignalTowers()
    {
        if (_mode != GameMode.Episode2 || !CurrentChapter.HackTowers) return false;

        if (_towerKm < 0f)
        {
            bool room = NextStopKm - _travelledKm > GameConfig.ApproachKm + GameConfig.SignalTowerLeadKm + GameConfig.Near(4f);
            if (_travelledKm >= _nextTowerKm && room && _events.Active == null)
                _towerKm = _travelledKm + GameConfig.SignalTowerLeadKm;
            return false;
        }

        float delta = _towerKm - _travelledKm;
        if (delta < -GameConfig.Near(1.5f))
        {
            OnTowerHack(HackResult.Failed); // Kule geçildi, sinyal hacklenmedi
            return false;
        }
        if (delta < GameConfig.Near(3f) && WasPressed(Keyboard.GetState(), Keys.H))
        {
            _towerHacking = true;
            _hack.Open(withActions: false, HackDifficulty, HackHelper, allowSwitch: false);
            _audio.PlayClick();
            return true;
        }
        return false;
    }

    private bool TowerInRange => _towerKm > 0f && _towerKm - _travelledKm < GameConfig.Near(3f) && _towerKm - _travelledKm > -GameConfig.Near(1.5f);

    /// <summary>
    /// Hikâye sahnesindeki hack'in sonucu: başarıda bayrak konur ve AURA'nın sırrını açan düğüm gelir;
    /// başarısızlıkta terminal yeniden denenebilir (düğüm "görülmedi" sayılır).
    /// </summary>
    private void OnSceneHack(HackResult result)
    {
        var hack = _sceneHack;
        _sceneHack = null;
        _sceneHacking = false;
        if (hack == null) return;
        if (result == HackResult.Success)
        {
            Narrative.Set(hack.HackFlag);
            _audio.PlaySuccess();
            _story.Open(this, hack.HackSuccess, CurrentTown?.Name ?? "");
        }
        else
        {
            if (hack.HackRetry != null) Narrative.Unsee(hack.HackRetry);
            _sanity.Restore(-GameConfig.TowerJamSanity * 0.5f);
            AddShake(4f);
            _audio.PlayError();
            _toasts.Show(Loc.T(result == HackResult.Cancelled ? "hack.retry" : "hack.failedToast"), Hud.Danger);
        }
    }

    /// <summary>Sinyal kulesinin sonucu (hack başarılı, başarısız ya da kule kaçırıldı).</summary>
    private void OnTowerHack(HackResult result)
    {
        if (_towerKm < 0f) return;
        _towerHacking = false;
        _towerKm = -1f;
        _nextTowerKm = _travelledKm + GameConfig.SignalTowerGapKm;
        if (result == HackResult.Success)
        {
            SaveData.Earn(GameConfig.TowerHackCredits);
            _ambushIntel = true;
            _sanity.Restore(5f);
            _toasts.Show(Loc.F("tower.hacked", GameConfig.TowerHackCredits), new Color(140, 255, 160));
        }
        else
        {
            _train.Damage(GameConfig.TowerJamDamage);
            _sanity.Restore(-GameConfig.TowerJamSanity);
            AddShake(6f);
            _audio.PlayError();
            _toasts.Show(Loc.T("tower.jammed"), Hud.Danger);
        }
    }

    private void UpdateChase(float dt, KeyboardState keys, InputState input)
    {
        // Yukarı ok kombinasyonlara ayrıldığı için gaz yalnızca W/S ile
        if (keys.IsKeyDown(Keys.W)) _train.Throttle(+1, dt);
        if (keys.IsKeyDown(Keys.S)) _train.Throttle(-1, dt);

        HandleItemKeys(keys);
        AdvanceTrain(dt, NextStopKm - GameConfig.ApproachKm - GameConfig.Near(0.5f));
        _train.BurnFuel(GameConfig.ChaseFuelBurnPerSecond * _train.SpeedRatio * dt);
        UpdatePassengers(dt);

        _chase.Update(dt, input, _train, _inventory, _audio);

        if (CheckDeath()) return;

        if (_chase.IsFinished)
        {
            bool escaped = _chase.Outcome == ChaseOutcome.Escaped;
            _toasts.Show(_chase.ResultMessage, escaped ? CabinPanel.Good : CabinPanel.Critical);
            if (escaped)
            {
                SaveData.Record(s => s.AmbushesEscaped++);
                ReportContract(ContractKind.EscapeAmbush, 1);
                CheckAchievements();
            }
            ChangeState(GameState.Driving);
        }
    }

    // ------------------------------------------------------------------
    // Finaller
    // ------------------------------------------------------------------

    /// <summary>Sığınak-0'a varıldı: hikâyede final yolculara göre, zamanla yarışta süre rekoru.</summary>
    private void ReachShelter()
    {
        _audio.PlayWhistle();
        _audio.PlaySuccess();

        if (_mode == GameMode.Speedrun)
        {
            float best = SaveData.Current.SpeedrunBestSeconds;
            bool record = best <= 0f || _speedrunTime < best;
            if (record) SaveData.Record(s => s.SpeedrunBestSeconds = _speedrunTime);
            SaveData.Record(s => s.SpeedrunWins++);
            SaveData.Earn(GameConfig.SpeedrunBaseReward);
            _ending = Endings.ForSpeedrun(_speedrunTime, GameConfig.SpeedrunBaseReward, record, best);
            CheckAchievements();
            ChangeState(GameState.Victory);
            return;
        }

        if (_mode == GameMode.Wasteland)
        {
            // Buzul Kıyameti'nin sonu: Sığınak-0'ın dış kapısı
            CompleteWastelandChapter(Chapters.Wasteland.Length);
            bool first = !SaveData.Current.WastelandCleared;
            SaveData.Record(s => s.WastelandCleared = true);
            if (first) SaveData.Earn(GameConfig.WastelandClearReward);
            _ending = Endings.ForWasteland(_cabin.Passengers.Count, first ? GameConfig.WastelandClearReward : 0);
            CheckAchievements();
            ChangeState(GameState.Victory);
            return;
        }

        // Hikâye ve Acımasız Kış: Sığınak-0'ın kapısında son karar (dört sondan biri)
        OpenFinalChoice();
    }

    // ------------------------------------------------------------------
    // İmdat görevleri (telsizden)
    // ------------------------------------------------------------------

    /// <summary>
    /// İmdat noktasına varıldı: çökmüş askeri tren ya da kar altındaki sığınak. İki seçenek:
    /// hayatta kalanlara yardım (karma +) ya da yalnızca malzemeleri almak (karma -).
    /// </summary>
    private void ArriveAtSos()
    {
        _sosKm = -1f;
        _atSos = true;
        _events.Clear();
        _audio.PlayWhistle();
        SaveData.Record(s => s.SosCompleted++);
        CheckAchievements();

        bool military = _sosType == SosMission.MilitaryTrain;
        var survivor = military ? SosProfiles.Sergeant : SosProfiles.Medic;
        bool canBoard = !_cabin.IsFull && !_cabin.HasProfile(survivor.Id);
        string id = military ? "sos.military" : "sos.shelter";
        var help = military
            ? new ChoiceOption(canBoard ? "sos.military.a" : "sos.military.afull", Passenger: canBoard ? survivor : null, Karma: 20, Sanity: 10)
            : new ChoiceOption(canBoard ? "sos.shelter.a" : "sos.shelter.afull", Passenger: canBoard ? survivor : null, Karma: 20, Sanity: 15, Rations: -2);
        var take = military
            ? new ChoiceOption("sos.military.b", Fuel: 25f, Items: new[] { ItemType.SpareParts, ItemType.SpareParts }, Karma: -15)
            : new ChoiceOption("sos.shelter.b", Items: new[] { ItemType.Medkit }, Karma: -20, Rations: 4);
        _currentScenario = new StationScenario(id, help, take);

        _dialog.Open(_currentScenario.Title, _currentScenario.Text, new[]
        {
            BuildOption("1", _currentScenario.OptionA),
            BuildOption("2", _currentScenario.OptionB)
        }, speaker: ArthurSpeaker);
        _stationPhase = StationPhase.Choosing;
        ChangeState(GameState.Station);
    }

    // ------------------------------------------------------------------
    // Final: Sığınak-0'ın kapısı ve dört son
    // ------------------------------------------------------------------

    /// <summary>
    /// Son karar. Her zaman: kapıdan gir (Kahraman ya da Yalnız Kurt). Telsizde mafya anlaşması yapıldıysa:
    /// paketi sat (Karanlık/Tüccar). Gizli: karma yeterince yüksekse treni feda etme seçeneği belirir.
    /// </summary>
    private void OpenFinalChoice()
    {
        _finalActions.Clear();
        var options = new List<DialogOption>();
        void Add(string label, Action action)
        {
            _finalActions.Add(action);
            options.Add(new DialogOption((_finalActions.Count).ToString(), label, true));
        }

        // Gerçeği bilen, Sofi'ye güvenen ve Konsey'le anlaşmayan Arthur için beşinci yol: Erime
        bool thaw = Narrative.Has(Flags.AuroraData) && Narrative.Has(Flags.SofiTrust) && !Narrative.Has(Flags.CouncilDeal);

        // Kurt Baba'ya paket sözlendiyse (Bölüm 34) telsizdeki anlaşmayla aynı kapı açılır
        bool mafiaDeal = _mafiaDeal || Narrative.Has(Flags.WolfDeal);
        // Ateşkes yapıldıysa köprüdeki zırhlı tren Kızıl Kurtlar'ın değil, Konsey'indir
        bool truce = Narrative.Has(Flags.WolfTruce);

        Add(Loc.T("final.gate"), FinishAtGate);
        if (mafiaDeal) Add(Loc.T("final.sell"), FinishMerchant);
        if (_karma >= GameConfig.SacrificeKarma) Add(Loc.T("final.sacrifice"), FinishSacrifice);
        if (thaw) Add(Loc.T("final.thaw"), FinishThaw);

        string text = Loc.T("final.text");
        if (Narrative.Has(Flags.CouncilDeal)) text += "\n\n" + Loc.T("final.deal");
        if (mafiaDeal) text += "\n\n" + Loc.T(Narrative.Has(Flags.WolfDeal) ? "final.wolfWaits" : "final.mafiaWaits");
        if (truce) text += "\n\n" + Loc.T("final.wolfTruce");
        if (_karma >= GameConfig.SacrificeKarma) text += "\n\n" + Loc.T(truce ? "final.bridge.council" : "final.bridge");
        if (thaw) text += "\n\n" + Loc.T("final.sofi");
        _dialog.Open(Loc.T("final.title"), text, options.ToArray(), speaker: ArthurSpeaker);
        ChangeState(GameState.FinalChoice);
    }

    private void UpdateFinalChoice(float dt, InputState input)
    {
        _dialog.Update(dt);
        if (input.Pressed(Keys.Space) && !_dialog.IsComplete)
        {
            _dialog.Skip();
            return;
        }
        if (!_dialog.AcceptsChoice) return;

        for (int i = 0; i < _finalActions.Count; i++)
        {
            if (!input.AnyPressed((Keys)((int)Keys.D1 + i), (Keys)((int)Keys.NumPad1 + i))) continue;
            _audio.PlayConfirm();
            _finalActions[i]();
            return;
        }
    }

    /// <summary>Hikâyeyi tamamlamanın ortak kaydı (bölüm kilidi, ödül, Acımasız Kış zaferi).</summary>
    private void CompleteStory(bool delivered)
    {
        if (_mode == GameMode.Hardcore)
            SaveData.Record(s => { s.HardcoreWins++; s.HardcoreBestKm = (int)GameConfig.TotalDistanceKm; });
        SaveData.CompleteChapter(SaveData.MaxChapter);
        if (delivered)
        {
            SaveData.Earn(GameConfig.ShelterReward); // Teslimat ödülü
            SaveData.Record(s => s.Delivered = true);
        }
    }

    /// <summary>Gizli Erime sonu: çekirdek Enstitü kulesine götürülür, AURORA tersine çalıştırılır.</summary>
    private void FinishThaw()
    {
        CompleteStory(delivered: true);
        _ending = Endings.ForThaw(_cabin.Passengers);
        RecordEnding(EndingIds.Thaw);
    }

    private void RecordEnding(string id)
    {
        // Yolculuk boyunca verilen kararların ve yolcuların sonsözü
        string epilogue = _mode == GameMode.Episode2
            ? Endings.Epilogue2(id, _cabin.Passengers)
            : Endings.Epilogue(id, _cabin.Passengers);
        if (!string.IsNullOrEmpty(epilogue))
            _ending = _ending with { Text = _ending.Text + "\n" + epilogue };

        // Episode 1 bitti: karma, hayatta kalan yolcular ve kararlar Episode 2'ye aktarılır
        if (_mode is GameMode.Story or GameMode.Hardcore)
        {
            var carry = new EpisodeCarry { Karma = _karma, Ending = id };
            foreach (var p in _cabin.Passengers) carry.Survivors.Add(p.Profile.Id);
            carry.Flags.AddRange(SaveData.Current.Story.Flags.Keys);
            SaveData.Record(s => { s.Carry = carry; s.Episode1Complete = true; });
        }
        SaveData.Record(s => { if (!s.EndingsSeen.Contains(id)) s.EndingsSeen.Add(id); });
        if (_mode == GameMode.Hardcore)
            _ending = _ending with { Text = _ending.Text + "\n" + Loc.T("end.hardcore.win") };
        CheckAchievements();
        _audio.PlaySuccess();
        ChangeState(GameState.Victory);
    }

    /// <summary>Kapıdan girildi: karma ve hayatta kalanlara göre Kahraman ya da Yalnız Kurt sonu.</summary>
    private void FinishAtGate()
    {
        CompleteStory(delivered: true);
        var (ending, id) = Endings.ForGate(_cabin.Passengers, _abandonedCount, _lostCount, _karma);
        _ending = ending;
        RecordEnding(id);
    }

    /// <summary>Karanlık / Tüccar sonu: paket Kızıl Kurtlar'a satılır.</summary>
    private void FinishMerchant()
    {
        CompleteStory(delivered: false);
        SaveData.Earn(GameConfig.MerchantReward);
        _ending = Endings.ForMerchant(GameConfig.MerchantReward, _cabin.Count);
        RecordEnding(EndingIds.Merchant);
    }

    /// <summary>Gizli Feda sonu: Arthur mafyanın zırhlı trenini köprüyle birlikte havaya uçurur.</summary>
    private void FinishSacrifice()
    {
        CompleteStory(delivered: true);
        _audio.PlayExplosion();
        AddShake(GameConfig.MaxScreenShake);
        _ending = Endings.ForSacrifice(_cabin.Passengers);
        RecordEnding(EndingIds.Sacrifice);
    }

    /// <summary>Yolculuğun sonu: sonsuz modda katedilen yol ödüllendirilir ve rekor tutulur.</summary>
    private void Die(DeathReason reason)
    {
        if (_mode == GameMode.Endless)
        {
            int km = (int)_travelledKm;
            int best = SaveData.Current.EndlessBestKm;
            bool record = km > best;
            if (record) SaveData.Record(s => s.EndlessBestKm = km);
            int credits = km / GameConfig.EndlessCreditsPerKm;
            SaveData.Earn(credits);
            var (rank, score) = LeaderboardManager.SubmitEndless(km, _endlessStage, credits); // Sanal liderlik tablosu
            _ending = Endings.ForEndless(reason, km, _endlessStage, credits, record, best, rank, score);
            CheckAchievements();
        }
        else if (_mode == GameMode.Hardcore)
        {
            // Permadeath: koşu tamamen biter; yalnızca ulaşılan en uzak mesafe kalır
            int km = (int)_travelledKm;
            int best = SaveData.Current.HardcoreBestKm;
            if (km > best) SaveData.Record(s => s.HardcoreBestKm = km);
            var death = Endings.ForDeath(reason, _cabin.Count);
            _ending = death with
            {
                Title = Loc.T("end.hardcore.title"),
                Name = Loc.T("end.hardcore.name"),
                Text = death.Text + "\n" + Loc.F("end.hardcore.text", km, Math.Max(km, best))
            };
        }
        else
        {
            _ending = Endings.ForDeath(reason, _cabin.Count);
        }

        // Episode 1'in kalıcı ölümü (Freeze Death): Episode 1 henüz bitirilmediyse yolculuk en baştan başlar
        if (_mode == GameMode.Story && !SaveData.Current.Episode1Complete && !SaveData.Current.DevUnlockAll)
        {
            SaveData.Current.UnlockedChapter = 1;
            Narrative.ResetForNewStory();
            _ending = _ending with { Text = _ending.Text + "\n" + Loc.T("end.permadeath") };
        }

        Difficulty.SetNormal();
        _audio.PlayDeath();
        ChangeState(GameState.GameOver);
    }

    /// <summary>Yakıt, sıcaklık, sağlık veya paket tükendiyse yolculuk biter.</summary>
    private bool CheckDeath()
    {
        DeathReason reason;
        if (_train.IsOutOfFuel) reason = DeathReason.OutOfFuel;
        else if (_train.IsFrozen) reason = DeathReason.BoilerFrozen;
        else if (_train.IsWrecked) reason = DeathReason.Derailed;
        else if (_train.IsPackageDestroyed) reason = DeathReason.PackageLost;
        else if (IsStoryMode && _sanity.Sanity <= 0f) reason = DeathReason.Madness; // Akıl sağlığı tükendi
        else return false;

        Die(reason);
        return true;
    }

    /// <summary>Final ekranı: R tam sıfırlama yapıp Ana Menü'ye döner.</summary>
    private void UpdateEnding(float dt, KeyboardState keys)
    {
        if (_state == GameState.GameOver)
            _train.Coast(dt); // Ateşi sönen tren yavaşça durur

        if (WasPressed(keys, Keys.R))
        {
            _audio.PlayClick();
            // Episode 1 bittiyse kapanış sinematiği: güneş doğar, telsizde AURA'nın uyarısı
            if (_state == GameState.Victory && _mode is GameMode.Story or GameMode.Hardcore && _ending?.Type is not
                    (EndingType.AuraFree or EndingType.AuraMachine or EndingType.AuraReset))
            {
                _outro.Start();
                ChangeState(GameState.EpisodeOutro);
                return;
            }
            ResetGame();
        }
    }

    /// <summary>Treni hızına göre ilerletir ve kabin/fırtına/bölüm çarpanlarıyla kaynakları tüketir.</summary>
    private void AdvanceTrain(float dt) => AdvanceTrain(dt, NextStopKm); // Durağı asla geçme

    /// <param name="maxKm">Bu kareye kadar gidilebilecek en uzak nokta.</param>
    private void AdvanceTrain(float dt, float maxKm)
    {
        float km = _train.Speed * GameConfig.KmPerSecondPerSpeed * dt;
        float before = _travelledKm;
        _travelledKm = MathF.Min(_travelledKm + km, MathF.Max(_travelledKm, maxKm));
        ReportContract(ContractKind.TravelKm, _travelledKm - before);

        var modifiers = _cabin.GetModifiers(_weather.Intensity);
        modifiers = modifiers with { CoolingMultiplier = modifiers.CoolingMultiplier * CurrentChapter.CoolingMultiplier };
        if (_mode == GameMode.Speedrun) // Zamanla yarış: kazan yakıtı hızlı yutar
            modifiers = modifiers with { FuelMultiplier = modifiers.FuelMultiplier * GameConfig.SpeedrunFuelMultiplier };
        _train.UpdateResources(dt, km, modifiers);
    }

    /// <summary>
    /// Hikâye hatlarında sürüşün arasına serpiştirilen anlatım: AURA'nın cızırtılı repliği, yolcuların korkusu,
    /// Arthur'un hatıraları. Olay sürerken yeni satır başlamaz.
    /// </summary>
    private void UpdateChatter(float dt, bool canStart)
    {
        if (!IsStoryMode) return;
        var said = _chatter.Update(dt, canStart, _mode == GameMode.Episode2 ? 2 : 1, CurrentChapter.Number, _cabin.Passengers);
        if (said == ChatterKind.Radio) _audio.PlayRadioChatter();
        else if (said != null) _audio.PlayClick();
    }

    private bool WasPressed(KeyboardState keys, Keys key) => keys.IsKeyDown(key) && _previousKeys.IsKeyUp(key);

    // ------------------------------------------------------------------
    // Siyaha kararma geçişi
    // ------------------------------------------------------------------

    /// <summary>Ekranı 0.5 sn'de karartır, tam karanlıkta <paramref name="midpoint"/>'i çalıştırır, sonra 0.5 sn'de açar.</summary>
    private void FadeTo(Action midpoint)
    {
        if (_fadeTimer >= 0f) return; // Zaten bir geçiş sürüyor
        _fadeTimer = 0f;
        _fadeMidpoint = midpoint;
        _fadeMidpointDone = false;
    }

    /// <returns>Geçiş sürüyorsa true (bu karede oyun mantığı çalışmaz).</returns>
    private bool UpdateFade(float dt)
    {
        if (_fadeTimer < 0f) return false;

        _fadeTimer += dt;
        if (!_fadeMidpointDone && _fadeTimer >= GameConfig.FadeHalfDuration)
        {
            _fadeMidpointDone = true;
            _fadeMidpoint?.Invoke();
        }
        if (_fadeTimer >= GameConfig.FadeHalfDuration * 2f)
            _fadeTimer = -1f;
        return true;
    }

    private float FadeAlpha
    {
        get
        {
            if (_fadeTimer < 0f) return 0f;
            float half = GameConfig.FadeHalfDuration;
            float t = _fadeTimer < half ? _fadeTimer / half : 1f - (_fadeTimer - half) / half;
            return MathHelper.Clamp(t * t * (3f - 2f * t), 0f, 1f); // Yumuşak (smoothstep) eğri
        }
    }

    // ==================================================================
    // Çizim
    // ==================================================================

    protected override void Draw(GameTime gameTime)
    {
        // Sarsıntı bu karede bir kez hesaplanır: hem ışık haritası hem sahne aynı kaymayı kullanır
        _frameShake = RouteShake();

        // Işık haritası sahneden ÖNCE hazırlanır (ekran hedefi çizim ortasında değişmesin)
        if (_state != GameState.Town)
            PrepareLighting();

        if (_photo.IsActive)
        {
            DrawPhotoMode();
            base.Draw(gameTime);
            return;
        }

        GraphicsDevice.Clear(NightColor);

        // Dünya: kasabada kamera dönüşümüyle, yolda (kovalamacada sarsıntıyla)
        if (_state == GameState.Town)
            DrawTownScene();
        else
            DrawRouteScene();

        // Arayüz: sarsıntı ve kameradan bağımsız. PointClamp piksel dokuları keskin tutar.
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        DrawAtmosphereOverlay();
        DrawInterface();

        if (_isPaused)
        {
            string summary = Loc.F("pause.summary", CurrentChapter.Number, (int)_travelledKm, (int)GameConfig.TotalDistanceKm,
                _cabin.Count, _cabin.Capacity, _inventory.UsedSlots, GameConfig.InventorySlots);
            MenuScreens.DrawPause(_spriteBatch, summary);
        }

        // Sahne geçişi: her şeyin üstünde siyaha kararma
        float fade = FadeAlpha;
        if (fade > 0f)
            _spriteBatch.FillRect(new Rectangle(0, 0, GameConfig.ScreenWidth, GameConfig.ScreenHeight), Color.Black * fade);

        _spriteBatch.End();
        base.Draw(gameTime);
    }

    /// <summary>Yol sahnesi: arka plan, duraklar, ray, tren ve (varsa) mafya kovalamacası. Kovalamacada sarsılır.</summary>
    private Vector2 _frameShake;
    private float _stormFrost;  // Kenar buzlanmasının yumuşatılmış şiddeti

    /// <summary>Kovalamaca, çöküş ve hasar sarsıntılarının bu karedeki toplamı.</summary>
    private Vector2 RouteShake()
    {
        var shake = _state == GameState.Chase ? _chase.ShakeOffset : _state == GameState.Sentinel ? _sentinel.ShakeOffset : Vector2.Zero;
        float random = _collapseShake + (_isPaused ? 0f : _screenShake);
        if (random > 0f)
            shake += new Vector2(((float)_rng.NextDouble() * 2f - 1f) * random, ((float)_rng.NextDouble() * 2f - 1f) * random * 0.6f);
        return new Vector2(MathF.Round(shake.X), MathF.Round(shake.Y));
    }

    private Matrix ShakeTransform => Matrix.CreateTranslation(_frameShake.X, _frameShake.Y, 0f);

    /// <summary>
    /// Sahnenin karanlığı: bölümün kendi karanlığı (tünel), gece ve tipi birleşir.
    /// Gece yarısı ekranın büyük kısmı kararır; yalnızca far konisi ve lambalar aydınlatır.
    /// </summary>
    private float SceneDarkness()
    {
        if (!IsInJourney || _state is GameState.Mining or GameState.Tutorial) return 0f;
        var chapter = CurrentChapter;
        float night = _dayCycle.Night * GameConfig.NightLightDarkness * chapter.NightStrength * (_train.HasLantern ? 0.85f : 1f);
        float storm = (_events.IsBlizzard ? GameConfig.StormDarkness : 0f) + MathF.Max(0f, _weather.Intensity - 0.6f) * 0.5f;
        float darkness = 1f - (1f - chapter.Darkness) * (1f - MathHelper.Clamp(night, 0f, 0.9f)) * (1f - MathHelper.Clamp(storm, 0f, 0.6f));
        return MathF.Min(darkness, GameConfig.MaxSceneDarkness);
    }

    private void PrepareLighting()
    {
        // Tipide karanlık mavimsi gri, gece ve tünelde neredeyse siyah
        var tint = _events.IsBlizzard ? new Color(16, 20, 30) : new Color(2, 2, 6);
        _lightMap.Prepare(GraphicsDevice, _spriteBatch, SceneDarkness(), tint, ShakeTransform, DrawSceneLights);
    }

    /// <summary>Işık haritasına karanlığı delen kaynakları çizer: tren, duraklar, maden feneri, tünel lambaları.</summary>
    private void DrawSceneLights(SpriteBatch sb)
    {
        _train.DrawLightMask(sb, _cabin.Count);

        void Light(Vector2 center, float radius, float strength) =>
            sb.Draw(Assets.Glow, new Rectangle((int)(center.X - radius), (int)(center.Y - radius), (int)(radius * 2), (int)(radius * 2)),
                Color.White * strength);

        // Duraklar kendi lambalarıyla aydınlık: yaklaşırken karanlıkta bir ışık adası belirir
        foreach (var stop in Stops)
        {
            float x = StationRenderer.ScreenX(stop.Km, _travelledKm);
            if (!StationRenderer.IsVisible(x)) continue;
            Light(new Vector2(x, GameConfig.RailY - 110), 240f, 0.8f);
        }

        if (_mineKm > 0f)
        {
            float x = StationRenderer.ScreenX(_mineKm, _travelledKm);
            if (StationRenderer.IsVisible(x)) Light(new Vector2(x, GameConfig.GroundY - 90), 130f, 0.8f);
        }
        if (_sosKm > 0f)
        {
            float x = StationRenderer.ScreenX(_sosKm, _travelledKm);
            float flare = 0.6f + 0.4f * MathF.Sin(_time * 9f);
            if (StationRenderer.IsVisible(x)) Light(new Vector2(x + 40, GameConfig.GroundY - 70), 170f, 0.8f * flare);
        }

        if (CurrentChapter.Theme == ChapterTheme.Tunnels)
        {
            const float spacing = 520f;
            float offset = _decorScroll % spacing;
            for (float x = -offset + 98; x < GameConfig.ScreenWidth + spacing; x += spacing)
                Light(new Vector2(x, 190), 160f, 0.75f);
        }
    }

    private void DrawRouteScene()
    {
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: ShakeTransform);

        bool inRun = _state is not (GameState.MainMenu or GameState.LanguageSelect or GameState.TutorialPrompt);
        var chapter = CurrentChapter;

        bool tunnel = inRun && chapter.Theme == ChapterTheme.Tunnels;

        DrawBackdrop();
        if (inRun && _ghostTrainTimer > 0f) DrawGhostTrain();
        if (tunnel) DrawTunnelBack();
        if (inRun && chapter.GlacierCollapse) DrawThawGround();
        DrawStations();
        if (inRun) DrawMineEntrance();
        if (inRun && _sosKm > 0f) DrawSosSite();

        // Kovalamacada arka plandaki paralel rayda zırhlı mafya treni (bizim trenin arkasında kalır)
        if (_state == GameState.Chase)
            _chase.DrawBackground(_spriteBatch, _time);
        if (_state == GameState.Sentinel)
            _sentinel.DrawBackground(_spriteBatch, _time);
        if (inRun && _towerKm > 0f)
            DrawSignalTowerRoadside();

        DrawRails();
        if (inRun && IsTravelling) DrawSwitchFork();
        if (inRun && IsTravelling && _events.IsBlizzard) DrawAvalanche(_events.Active);
        if (inRun && chapter.GlacierCollapse && IsTravelling) DrawCollapseFront();

        // Tren: duman arkada, ışıklar önde
        _train.DrawSmoke(_spriteBatch);
        _train.Draw(_spriteBatch);

        if (_state == GameState.Chase)
            _chase.DrawWorld(_spriteBatch, _time);
        if (_state == GameState.Sentinel)
            _sentinel.DrawWorld(_spriteBatch);
        if (inRun && IsTravelling) DrawWolves();

        // Karanlık: hazırlanan ışık haritası sahnenin üstüne serilir (far konisi ve lambalar delik açar).
        // Maske ekran koordinatında olduğu için sarsıntısız bir çizimle serilir.
        if (inRun && _lightMap.Darkness > 0.01f)
        {
            _spriteBatch.End();
            _spriteBatch.Begin(blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);
            _lightMap.Draw(_spriteBatch);
            _spriteBatch.End();
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: ShakeTransform);
            if (IsTravelling) DrawWolfEyes(); // Karanlıkta yalnızca kurtların gözleri parlar
        }
        if (tunnel) DrawTunnelFront();

        _train.DrawLights(_spriteBatch);
        _themeParticles.Draw(_spriteBatch);
        _snow.Draw(_spriteBatch);
        if (inRun && IsTravelling && _events.IsBlizzard) DrawBlizzard(_events.Active);
        if (inRun && chapter.Theme == ChapterTheme.AcidWaste) DrawAcidWeather();
        _spriteBatch.End();
    }

    /// <summary>
    /// Buzul Kıyameti: zehirli kar. Ekranda hafif yeşil-sarı bir pus ve parlayan asit taneleri;
    /// asit fırtınasında pus koyulaşır, yağış yatay ve yoğun olur, gövdeden duman tüter.
    /// </summary>
    private void DrawAcidWeather()
    {
        bool storm = _events.IsAcidStorm && IsTravelling;
        var screen = new Rectangle(-20, -20, GameConfig.ScreenWidth + 40, GameConfig.ScreenHeight + 40);
        float pulse = 0.5f + 0.5f * MathF.Sin(_time * 1.7f);
        _spriteBatch.FillRect(screen, new Color(110, 150, 40) * ((storm ? 0.22f : 0.08f) + 0.03f * pulse));

        int count = storm ? 90 : 30;
        float slant = storm ? 1.6f : 0.4f;
        for (int i = 0; i < count; i++)
        {
            float speed = 260f + (i % 7) * 40f;
            float y = (i * 131 + _time * speed) % (GameConfig.ScreenHeight + 40) - 20;
            float x = ((i * 211 - _time * speed * slant) % 1400f + 1400f) % 1400f - 60f;
            var drop = new Color(190, 255, 90);
            _spriteBatch.FillRect(new Rectangle((int)x, (int)y, 2, storm ? 6 : 4), drop * 0.8f);
            if (i % 9 == 0) _spriteBatch.DrawGlow(new Vector2(x, y), 8f, drop, 0.35f);
        }

        // Fırtınada aşınan gövdeden yükselen sarımsı duman
        if (storm && _rng.NextDouble() < 0.4)
        {
            _themeParticles.Emit(new Particle
            {
                Position = new Vector2(Train.RearX + _rng.Next(0, Train.FrontX - Train.RearX), GameConfig.RailY - 30 - _rng.Next(0, 70)),
                Velocity = new Vector2(-_train.ScrollSpeed * 0.2f, -_rng.Next(20, 60)),
                Life = 0.8f + (float)_rng.NextDouble() * 0.6f,
                Size = 4f + _rng.Next(4),
                Growth = 2.5f,
                Color = new Color(170, 200, 90),
                Alpha = 0.35f,
                Soft = true
            });
        }
    }

    /// <summary>İmdat noktası: devrilmiş askeri vagon ya da kar altındaki sığınak kapağı; yanıp sönen kızıl işaret fişeği.</summary>
    private void DrawSosSite()
    {
        float x = StationRenderer.ScreenX(_sosKm, _travelledKm);
        if (!StationRenderer.IsVisible(x)) return;
        int cx = (int)x, baseY = GameConfig.GroundY + 4;
        void Box(int dx, int dy, int w, int h, Color c) => _spriteBatch.FillRect(new Rectangle(cx + dx, baseY + dy, w, h), c);

        if (_sosType == SosMission.MilitaryTrain)
        {
            // Raydan çıkmış, yan yatmış askeri vagon
            _spriteBatch.Draw(Assets.Pixel, new Vector2(cx - 120, baseY - 50), null, new Color(70, 80, 54), 0.18f,
                Vector2.Zero, new Vector2(170, 52), SpriteEffects.None, 0f);
            _spriteBatch.Draw(Assets.Pixel, new Vector2(cx - 116, baseY - 44), null, new Color(220, 220, 200), 0.18f,
                Vector2.Zero, new Vector2(10, 10), SpriteEffects.None, 0f);
            Box(-140, -8, 220, 8, new Color(200, 208, 222));                     // Üstünü örten kar
            for (int i = 0; i < 3; i++) Box(40 + i * 14, -20 - i * 8, 10, 4, new Color(90, 90, 96)); // Kopmuş ray
        }
        else
        {
            // Kar altındaki sığınak: beton kapak, anten, yarı gömülü merdiven
            Box(-60, -24, 120, 24, new Color(90, 92, 100));
            Box(-50, -32, 100, 10, new Color(120, 122, 130));
            Box(-20, -30, 40, 6, new Color(40, 40, 46));
            Box(50, -100, 3, 76, new Color(70, 70, 76));
            Box(42, -100, 18, 3, new Color(70, 70, 76));
            Box(-70, -6, 140, 6, new Color(210, 216, 228));
        }

        // Kızıl işaret fişeği ve dumanı
        float flare = 0.6f + 0.4f * MathF.Sin(_time * 9f) * MathF.Sin(_time * 3.1f);
        Box(36, -40, 4, 30, new Color(60, 50, 40));
        Box(34, -46, 8, 8, new Color(255, 70, 50) * flare);
        _spriteBatch.DrawGlow(new Vector2(cx + 38, baseY - 42), 70f, new Color(255, 60, 40), 0.5f * flare);
        for (int i = 0; i < 5; i++)
        {
            float t = (_time * 0.5f + i * 0.2f) % 1f;
            _spriteBatch.DrawGlow(new Vector2(cx + 38 - t * 30f, baseY - 50 - t * 120f), 14f + t * 20f, new Color(255, 120, 100), 0.18f * (1f - t));
        }
    }

    /// <summary>Sanrı: arka plandaki uzak rayda sessizce süzülen, yarı saydam hayalet tren.</summary>
    private void DrawGhostTrain()
    {
        float t = 1f - _ghostTrainTimer / 6f;
        float alpha = MathF.Sin(t * MathF.PI) * 0.35f;
        int x = (int)MathHelper.Lerp(GameConfig.ScreenWidth + 200, -700, t);
        int y = GameConfig.GroundY - 60;
        var ghost = new Color(190, 220, 255) * alpha;
        for (int i = 0; i < 4; i++) _spriteBatch.FillRect(new Rectangle(x + 170 + i * 150, y - 40, 140, 40), ghost * 0.8f); // Vagonlar
        _spriteBatch.FillRect(new Rectangle(x, y - 60, 160, 60), ghost);                    // Lokomotif
        _spriteBatch.FillRect(new Rectangle(x + 20, y - 90, 22, 30), ghost);                // Baca
        for (int i = 0; i < 6; i++) _spriteBatch.FillRect(new Rectangle(x + 10 + i * 100, y - 2, 18, 6), ghost * 0.6f);
        _spriteBatch.DrawGlow(new Vector2(x - 6, y - 40), 60f, new Color(200, 230, 255), alpha);   // Önünde soluk far
    }

    /// <summary>Yol kenarındaki terk edilmiş maden girişi: kaya yamacı, ahşap çerçeve, fener ve vagonet.</summary>
    private void DrawMineEntrance()
    {
        if (_mineKm <= 0f) return;
        float x = StationRenderer.ScreenX(_mineKm, _travelledKm);
        if (!StationRenderer.IsVisible(x)) return;

        int baseY = GameConfig.GroundY + 4;
        int cx = (int)x;
        void Box(int dx, int dy, int w, int h, Color c) => _spriteBatch.FillRect(new Rectangle(cx + dx, baseY + dy, w, h), c);

        // Kademeli kaya yamacı ve üstündeki kar
        var rock = new Color(54, 48, 46);
        for (int i = 0; i < 6; i++)
        {
            int w = 320 - i * 44;
            Box(-w / 2, -40 - i * 28, w, 40, Color.Lerp(rock, new Color(70, 62, 58), i / 6f));
            Box(-w / 2 + 6, -40 - i * 28, w - 12, 5, new Color(200, 208, 222));
        }

        // Maden ağzı ve ahşap destekler
        Box(-46, -110, 92, 110, new Color(6, 6, 8));
        var wood = new Color(96, 68, 42);
        Box(-56, -118, 112, 14, wood);
        Box(-56, -110, 12, 110, wood);
        Box(44, -110, 12, 110, wood);
        Box(-60, -122, 120, 4, new Color(210, 216, 228));

        // İçeriden sızan sıcak ışık ve asılı fener
        float flicker = 0.75f + 0.25f * MathF.Sin(_time * 7f + x * 0.01f);
        _spriteBatch.DrawGlow(new Vector2(cx, baseY - 50), 80f, new Color(255, 180, 90), 0.25f * flicker);
        Box(-3, -100, 6, 10, new Color(40, 36, 32));
        Box(-6, -90, 12, 10, new Color(255, 205, 120) * flicker);
        _spriteBatch.DrawGlow(new Vector2(cx, baseY - 85), 50f, new Color(255, 200, 120), 0.4f * flicker);

        // Ağızdan çıkan dar ray ve devrik vagonet
        Box(-46, -6, 160, 3, new Color(90, 92, 100));
        Box(70, -30, 40, 22, new Color(80, 70, 60));
        Box(66, -34, 48, 6, new Color(110, 96, 80));
        Box(74, -36, 30, 6, new Color(180, 150, 70)); // İçindeki cevher parıltısı
        Box(74, -8, 8, 8, new Color(30, 30, 34));
        Box(98, -8, 8, 8, new Color(30, 30, 34));
    }

    /// <summary>Çökmüş makas: trenin önünde ikiye ayrılan hat; görülebiliyorsa kırık kol kızıl parlar.</summary>
    private void DrawSwitchFork()
    {
        var e = _events.Active;
        if (e?.Type != RoadEventType.BrokenSwitch) return;

        float t = MathHelper.Clamp(e.Age / e.Duration, 0f, 1f);
        int x = (int)MathHelper.Lerp(GameConfig.ScreenWidth + 60, GameConfig.TrainX + 480, t);
        int y = GameConfig.RailY;
        var rail = new Color(95, 100, 115);
        var highlight = new Color(165, 172, 190);
        float pulse = 0.5f + 0.5f * MathF.Sin(_time * 8f);

        // Kollar: sol (uzak, yukarı kıvrılan) ve sağ (yakın, aşağı kıvrılan)
        for (int branch = 0; branch < 2; branch++)
        {
            bool broken = branch == e.BrokenBranch;
            bool chosen = branch == (e.ChosenBranch < 0 ? 0 : e.ChosenBranch);
            int slope = branch == 0 ? -3 : 3;
            var color = chosen ? Color.Lerp(rail, Hud.Amber, 0.6f) : rail;

            for (int i = 0; i < 18; i++)
            {
                int sx = x + i * 26;
                int sy = y + i * slope;
                if (broken && i >= 5)
                {
                    // Çökmüş kesim: boşluklar, eğilmiş ve düşmüş ray parçaları
                    if (i % 3 == 0) continue;
                    sy += (i - 4) * 3;
                }
                _spriteBatch.FillRect(new Rectangle(sx, sy, 26, 4), color);
                _spriteBatch.FillRect(new Rectangle(sx, sy, 26, 1), highlight);
                if (i % 2 == 0) _spriteBatch.FillRect(new Rectangle(sx + 4, sy + 4, 8, 4), new Color(60, 44, 32)); // Travers
            }

            if (broken && e.HintVisible)
                _spriteBatch.DrawGlow(new Vector2(x + 7 * 26, y + 7 * slope + 10), 70f, Hud.Danger, 0.3f * pulse);
        }

        // Makas kolu: seçilen tarafa yatar
        int lean = e.ChosenBranch == 1 ? 10 : -10;
        _spriteBatch.FillRect(new Rectangle(x - 40, y - 14, 22, 14), new Color(70, 60, 50));
        _spriteBatch.FillRect(new Rectangle(x - 30 + lean / 2, y - 40, 4, 28), new Color(150, 150, 160));
        _spriteBatch.FillRect(new Rectangle(x - 34 + lean, y - 46, 12, 8), Hud.Danger);
    }

    /// <summary>Kurt sürüsü: trenin arkasından ve yanından koşan, gözleri parlayan kurtlar. Duman onları geri iter.</summary>
    private void DrawWolves()
    {
        var e = _events.Active;
        if (e?.Type != RoadEventType.WolfPack) return;

        for (int i = 0; i < WolfCount(e); i++)
        {
            var paws = WolfPosition(e, i, out float run);
            DrawWolf(paws, run, i % 3 == 0 ? new Color(70, 70, 78) : new Color(52, 52, 60));
        }
    }

    private static int WolfCount(RoadEvent e) => 3 + (int)(MathHelper.Clamp(e.Aggression, 0f, 1.5f) * 2f);

    /// <summary>Kurdun pati konumu: trenin arkasından yetişir, duman onları geri iter.</summary>
    private Vector2 WolfPosition(RoadEvent e, int i, out float run)
    {
        float appear = MathHelper.Clamp(e.Age * 1.2f, 0f, 1f);
        float aggression = MathHelper.Clamp(e.Aggression, 0f, 1.5f);
        run = _time * 14f + i * 1.9f;
        float x = Train.RearX - 170 + i * 74 + MathF.Sin(_time * 1.1f + i * 2.3f) * 26f;
        x += aggression * 60f - (1f - appear) * 420f - _wolfSmokeFlash * 140f;
        float y = GameConfig.RailY - 2 - (i % 2) * 8 - MathF.Abs(MathF.Sin(run * 0.5f)) * 6f; // Sıçrayarak koşu
        return new Vector2(x, y);
    }

    /// <summary>Karanlıkta yalnızca kurtların sarı gözleri seçilir.</summary>
    private void DrawWolfEyes()
    {
        var e = _events.Active;
        if (e?.Type != RoadEventType.WolfPack) return;
        for (int i = 0; i < WolfCount(e); i++)
        {
            var paws = WolfPosition(e, i, out _);
            var eye = new Vector2(paws.X + 45, paws.Y - 29);
            _spriteBatch.FillRect(new Rectangle((int)eye.X - 1, (int)eye.Y, 3, 2), new Color(255, 220, 90));
            _spriteBatch.DrawGlow(eye, 12f, new Color(255, 200, 80), 0.8f);
        }
    }

    private void DrawWolf(Vector2 paws, float run, Color fur)
    {
        int px = (int)paws.X, py = (int)paws.Y;
        void Box(int x, int y, int w, int h, Color c) => _spriteBatch.FillRect(new Rectangle(px + x, py + y, w, h), c);

        var dark = UiDraw.Shade(fur, 0.7f);
        Box(0, -24, 40, 13, fur);                   // Gövde
        Box(2, -26, 30, 4, UiDraw.Shade(fur, 1.25f)); // Sırt tüyleri
        Box(36, -32, 14, 12, fur);                  // Baş
        Box(48, -27, 8, 6, dark);                   // Burun
        Box(38, -37, 4, 6, dark);                   // Kulak
        Box(44, -30, 3, 2, new Color(255, 210, 80)); // Parlayan göz
        _spriteBatch.DrawGlow(new Vector2(px + 45, py - 29), 10f, new Color(255, 200, 80), 0.5f);
        Box(-14, -24 - (int)(MathF.Sin(run) * 3f), 16, 5, dark); // Kuyruk

        // Koşan bacaklar (iki çift, ters fazda)
        int a = (int)(MathF.Sin(run) * 6f), b = (int)(MathF.Sin(run + MathF.PI) * 6f);
        Box(6 + a, -12, 4, 12, dark);
        Box(12 + b, -12, 4, 12, dark);
        Box(28 + b, -12, 4, 12, dark);
        Box(34 + a, -12, 4, 12, dark);
    }

    /// <summary>Tipi: beyaz perde ve yatay kar rüzgârı; çığ riski arttıkça yamaçtan inen kar kütlesi.</summary>
    private void DrawBlizzard(RoadEvent e)
    {
        var screen = new Rectangle(-20, -20, GameConfig.ScreenWidth + 40, GameConfig.ScreenHeight + 40);
        float gust = 0.5f + 0.5f * MathF.Sin(_time * 2.3f);
        float appear = MathHelper.Clamp(e.Age, 0f, 1f);
        _spriteBatch.FillRect(screen, new Color(215, 225, 240) * ((0.28f + 0.12f * gust) * appear));

        for (int i = 0; i < 60; i++)
        {
            float x = ((i * 211 - _time * (900f + i * 9f)) % 1400f + 1400f) % 1400f - 60f;
            float y = (i * 97 + MathF.Sin(_time * 3f + i) * 20f) % GameConfig.ScreenHeight;
            _spriteBatch.FillRect(new Rectangle((int)x, (int)y, 30 + i % 40, 2), Color.White * (0.5f * appear));
        }

    }

    /// <summary>Çığ: sağdaki dağ yamacından trenin arkasında aşağı kayan kar kütlesi (tren önde kalır).</summary>
    private void DrawAvalanche(RoadEvent e)
    {
        // Risk arttıkça kütle büyür ve raylara yaklaşır
        float risk = MathHelper.Clamp(e.AvalancheRisk, 0f, 1f);
        if (risk <= 0.05f) return;

        const int slopeLeft = 720, slopeRight = GameConfig.ScreenWidth + 20;
        static float SlopeY(float x) => GameConfig.GroundY - 20 - (x - slopeLeft) * 0.42f; // Yamaç çizgisi (sağa doğru yükselir)

        // Dağ yamacı: kayan karın üzerinde aktığı koyu kaya
        for (int x = slopeLeft; x < slopeRight; x += 8)
        {
            int top = (int)SlopeY(x);
            _spriteBatch.FillRect(new Rectangle(x, top, 8, GameConfig.GroundY - top), new Color(70, 76, 92) * 0.85f);
        }

        // Kar kütlesi: ön ucu risk arttıkça yamaçtan aşağı iner, kalınlığı artar
        float front = MathHelper.Lerp(slopeRight - 60, slopeLeft + 40, risk);
        float shake = risk > 0.6f ? MathF.Sin(_time * 30f) * 2f : 0f;
        for (int x = (int)front; x < slopeRight; x += 8)
        {
            float along = (x - front) / (slopeRight - front + 1f);
            float thick = (18f + 70f * risk) * MathF.Min(1f, along * 4f) + MathF.Sin(x * 0.07f + _time * 5f) * 5f;
            int baseY = (int)(SlopeY(x) + shake);
            _spriteBatch.FillRect(new Rectangle(x, baseY - (int)thick, 8, (int)thick + 4), new Color(236, 242, 250) * 0.95f);
            _spriteBatch.FillRect(new Rectangle(x, baseY - (int)thick, 8, 3), Color.White);
        }

        // Kütlenin önünden dökülen kar parçaları
        for (int i = 0; i < 10; i++)
        {
            float life = (_time * 1.4f + i * 0.37f) % 1f;
            float x = front - life * 90f - i * 6f;
            float y = SlopeY(front) - 10f + life * life * 90f;
            int size = 4 + i % 3 * 3;
            _spriteBatch.FillRect(new Rectangle((int)x, (int)y, size, size), new Color(220, 230, 245) * (risk * (1f - life)));
        }

        if (risk > 0.6f)
            _spriteBatch.DrawGlow(new Vector2(front, SlopeY(front)), 220f, Hud.Danger, 0.18f * (0.5f + 0.5f * MathF.Sin(_time * 10f)));
    }

    /// <summary>Tünelin arka duvarı: kaya katmanları ve ahşap destek çerçeveleri (zeminle birlikte kayar).</summary>
    private void DrawTunnelBack()
    {
        var rock = new Color(30, 26, 26);
        _spriteBatch.FillRect(new Rectangle(0, 0, GameConfig.ScreenWidth, GameConfig.GroundY), new Color(14, 12, 14) * 0.85f);
        for (int i = 0; i < 18; i++)
        {
            float x = ((i * 97 - _decorScroll * 0.6f) % 1600 + 1600) % 1600 - 160;
            _spriteBatch.FillRect(new Rectangle((int)x, 180 + (i * 53) % 260, 120 + (i * 31) % 80, 14), rock);
        }

        const float spacing = 520f;
        float offset = _decorScroll % spacing;
        for (float x = -offset; x < GameConfig.ScreenWidth + spacing; x += spacing)
        {
            var wood = new Color(70, 50, 34);
            _spriteBatch.FillRect(new Rectangle((int)x, 150, 16, GameConfig.RailY - 150), wood);
            _spriteBatch.FillRect(new Rectangle((int)x + 180, 150, 16, GameConfig.RailY - 150), wood);
            _spriteBatch.FillRect(new Rectangle((int)x - 20, 140, 236, 18), wood);
            _spriteBatch.FillRect(new Rectangle((int)x - 20, 140, 236, 3), new Color(100, 76, 52));
        }
    }

    /// <summary>Karanlığın üstünde kalan tünel ön planı: kaya tavan ve destek kirişlerindeki asılı lambalar.</summary>
    private void DrawTunnelFront()
    {
        var rock = new Color(14, 12, 14);
        _spriteBatch.FillRect(new Rectangle(0, 0, GameConfig.ScreenWidth, 70), rock);
        for (int i = 0; i < 40; i++)
        {
            float x = ((i * 41 - _decorScroll) % 1640 + 1640) % 1640 - 40;
            _spriteBatch.FillRect(new Rectangle((int)x, 70, 26, 10 + (i * 13) % 34), rock); // Sarkıtlar
        }

        const float spacing = 520f;
        float offset = _decorScroll % spacing;
        for (float x = -offset + 98; x < GameConfig.ScreenWidth + spacing; x += spacing)
        {
            float flicker = 0.8f + 0.2f * MathF.Sin(_time * 9f + x);
            _spriteBatch.FillRect(new Rectangle((int)x - 2, 158, 4, 18), new Color(40, 36, 32));
            _spriteBatch.FillRect(new Rectangle((int)x - 6, 176, 12, 10), new Color(255, 200, 120) * flicker);
            _spriteBatch.DrawGlow(new Vector2(x, 182), 110f, new Color(255, 190, 110), 0.35f * flicker);
        }
    }

    /// <summary>Bölüm 3 — erken erime: karlı zeminde parlayan eriyik su birikintileri.</summary>
    private void DrawThawGround()
    {
        const float spacing = 330f;
        float offset = _decorScroll % spacing;
        for (float x = -offset; x < GameConfig.ScreenWidth + spacing; x += spacing)
        {
            int w = 120 + (int)(MathF.Abs(MathF.Sin(x)) * 60);
            _spriteBatch.FillRect(new Rectangle((int)x, 640, w, 10), new Color(70, 120, 160) * 0.8f);
            _spriteBatch.FillRect(new Rectangle((int)x + 10, 642, w - 30, 2), new Color(170, 210, 240) * 0.8f);
            _spriteBatch.FillRect(new Rectangle((int)x + 60, 600, w / 2, 6), new Color(70, 120, 160) * 0.6f);
        }
    }

    /// <summary>Çöküş cephesi: ekranın solunda, yaklaştıkça büyüyen kırılan buz ve çöken raylar.</summary>
    private void DrawCollapseFront()
    {
        // Gap 0 → cephe trenin arka ucunda; 10 km ve üstü → ekran dışında
        float closeness = 1f - MathHelper.Clamp(_collapseGapKm / 10f, 0f, 1f);
        if (closeness <= 0f) return;

        int edge = (int)(closeness * (GameConfig.TrainX + 60));
        _spriteBatch.FillRect(new Rectangle(0, GameConfig.RailY - 4, edge, 160), new Color(20, 30, 46));        // Açılan yarık
        _spriteBatch.FillRect(new Rectangle(0, GameConfig.RailY + 20, edge, 140), new Color(40, 90, 130) * 0.8f); // Altındaki kara su

        // Yarığın kenarında devrilen buz blokları (rayların üstünde, envanter panelinin üzerinden görünür)
        for (int i = 0; i < 4; i++)
        {
            float tilt = MathF.Sin(_time * 2f + i * 1.7f) * 0.25f + 0.3f * i;
            int size = 34 - i * 5;
            var pos = new Vector2(edge - 20 - i * 34, GameConfig.RailY - 30 - i * 18 + MathF.Sin(_time * 3f + i) * 6f);
            _spriteBatch.Draw(Assets.Pixel, pos, null, new Color(190, 225, 245), tilt, new Vector2(0.5f, 0.5f), new Vector2(size, size * 0.7f), SpriteEffects.None, 0f);
            _spriteBatch.Draw(Assets.Pixel, pos + new Vector2(-3, -3), null, new Color(235, 248, 255), tilt, new Vector2(0.5f, 0.5f), new Vector2(size * 0.5f, 3f), SpriteEffects.None, 0f);
        }
        for (int i = 0; i < 8; i++)
        {
            int x = edge - 10 - i * 26;
            if (x < -30) break;
            int drop = (int)((MathF.Sin(_time * 3f + i) * 0.5f + 0.5f) * 10) + i * 4;
            _spriteBatch.FillRect(new Rectangle(x, GameConfig.RailY - 6 + drop, 26, 10), new Color(170, 210, 235));   // Kırılan buz levhaları
            _spriteBatch.FillRect(new Rectangle(x + 4, GameConfig.RailY - 2 + drop, 18, 3), new Color(95, 100, 115)); // Kopan ray parçası
        }
        float pulse = 0.5f + 0.5f * MathF.Sin(_time * 8f);
        _spriteBatch.DrawGlow(new Vector2(edge, GameConfig.RailY), 120f, new Color(140, 200, 255), 0.25f * closeness * pulse);
    }

    /// <summary>
    /// Kasaba sahnesi: gökyüzü ve parallax ekran koordinatında, kasaba (binalar, tren, Arthur)
    /// kamera kaydırmasıyla dünya koordinatında, kar yine ekran koordinatında çizilir.
    /// </summary>
    private void DrawTownScene()
    {
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        DrawBackdrop();
        _spriteBatch.End();

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp,
            transformMatrix: Matrix.CreateTranslation(-MathF.Round(_townScene.CameraX), 0f, 0f));
        _townScene.DrawWorld(_spriteBatch, _train);
        _spriteBatch.End();

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _snow.Draw(_spriteBatch);
        _spriteBatch.End();
    }

    /// <summary>Gökyüzü, ay ve parallax katmanları (dağlar, çamlar, karlı zemin).</summary>
    private void DrawBackdrop()
    {
        var screen = new Rectangle(0, 0, GameConfig.ScreenWidth, GameConfig.ScreenHeight);
        float storm = _weather.Intensity;

        // Gökyüzü: gece laciverti, fırtına büyüdükçe koyu griye döner
        _spriteBatch.Draw(Assets.Sky, screen, Color.White);
        _spriteBatch.Draw(Assets.StormSky, screen, Color.White * storm);

        // Bulutların ardındaki ay, fırtınada kaybolur (menü ve final başlıklarının arkasında kalmasın diye yalnızca yolculukta)
        var moon = new Vector2(640, 120);
        float moonVisibility = IsInJourney ? 1f - storm * 0.9f : 0f;
        _spriteBatch.DrawGlow(moon, 180f, new Color(120, 140, 190), 0.25f * moonVisibility);
        _spriteBatch.Draw(Assets.Circle, new Rectangle((int)moon.X - 26, (int)moon.Y - 26, 52, 52),
            new Color(190, 198, 215) * (0.55f * moonVisibility));

        // Parallax katmanları (arkadan öne); fırtına uzak katmanları sise gömer
        var haze = new Color(48, 50, 58);
        _mountains.Draw(_spriteBatch, GameConfig.ScreenWidth);
        _spriteBatch.FillRect(new Rectangle(0, 0, GameConfig.ScreenWidth, GameConfig.GroundY), haze * (0.35f * storm));
        _pines.Draw(_spriteBatch, GameConfig.ScreenWidth);
        _spriteBatch.FillRect(new Rectangle(0, 0, GameConfig.ScreenWidth, GameConfig.GroundY), haze * (0.15f * storm));
        _ground.Draw(_spriteBatch, GameConfig.ScreenWidth);
    }

    /// <summary>Ray; buzlu bölgede buz mavisine döner ve üstünde parıltılar belirir.</summary>
    private void DrawRails()
    {
        bool icy = IsTravelling && _events.IsOnIce;
        var railColor = icy ? new Color(150, 200, 235) : new Color(95, 100, 115);
        var highlight = icy ? Color.White : new Color(165, 172, 190);

        if (icy)
            _spriteBatch.FillRect(new Rectangle(0, GameConfig.RailY - 3, GameConfig.ScreenWidth, 12), new Color(170, 215, 255) * 0.18f);

        _spriteBatch.FillRect(new Rectangle(0, GameConfig.RailY, GameConfig.ScreenWidth, 4), railColor);
        _spriteBatch.FillRect(new Rectangle(0, GameConfig.RailY, GameConfig.ScreenWidth, 1), highlight);
    }

    /// <summary>Ekrana giren durakları (istasyon, kasaba girişi, benzinlik, Sığınak-0) ve yük vagonunu çizer.</summary>
    private void DrawStations()
    {
        foreach (var stop in Stops) // Aktif modun hattı (sonsuz ve zamanla yarış hatları dahil)
        {
            float x = StationRenderer.ScreenX(stop.Km, _travelledKm);
            if (!StationRenderer.IsVisible(x)) continue;

            switch (stop.Kind)
            {
                case StopKind.Station:
                    StationRenderer.DrawAbandonedStation(_spriteBatch, x, stop.Index + 1, _time);
                    break;
                case StopKind.Town:
                    var town = TownCatalog.All[stop.Index];
                    StationRenderer.DrawTownGate(_spriteBatch, x, town.Name, town.Accent, _time);
                    break;
                case StopKind.Depot:
                    StationRenderer.DrawDepot(_spriteBatch, x, _time);
                    break;
                case StopKind.Shelter:
                    StationRenderer.DrawShelter(_spriteBatch, x, _time);
                    break;
                case StopKind.Hub:
                    StationRenderer.DrawHubStation(_spriteBatch, x, _time);
                    break;
            }
        }

        if (_supplyWagonKm > 0f)
        {
            float x = StationRenderer.ScreenX(_supplyWagonKm, _travelledKm);
            if (StationRenderer.IsVisible(x))
                StationRenderer.DrawAbandonedWagon(_spriteBatch, x, _time);
        }
    }

    /// <summary>Hattın kenarında AURA'nın sinyal kulesi: kafes gövde, kızıl göz ve yayılan frekans halkaları.</summary>
    private void DrawSignalTowerRoadside()
    {
        float x = StationRenderer.ScreenX(_towerKm, _travelledKm);
        if (!StationRenderer.IsVisible(x)) return;
        int baseY = GameConfig.GroundY + 6;
        var steel = new Color(40, 42, 50);
        var red = new Color(255, 50, 60);
        _spriteBatch.FillRect(new Rectangle((int)x - 26, baseY - 260, 6, 260), steel);
        _spriteBatch.FillRect(new Rectangle((int)x + 20, baseY - 260, 6, 260), steel);
        for (int y = baseY - 260; y < baseY; y += 22)
            _spriteBatch.FillRect(new Rectangle((int)x - 26, y, 52, 3), steel);
        float pulse = 0.6f + 0.4f * MathF.Sin(_time * 4f);
        var eye = new Vector2(x, baseY - 280);
        _spriteBatch.Draw(Assets.Circle, new Rectangle((int)eye.X - 18, (int)eye.Y - 18, 36, 36), new Color(50, 8, 12));
        _spriteBatch.Draw(Assets.Circle, new Rectangle((int)eye.X - 9, (int)eye.Y - 9, 18, 18), red * pulse);
        _spriteBatch.DrawGlow(eye, 60f, red, 0.45f * pulse);
        for (int i = 1; i <= 3; i++)
        {
            float wave = (_time * 0.8f + i * 0.33f) % 1f;
            _spriteBatch.DrawGlow(eye, 40f + wave * 180f, red, 0.1f * (1f - wave));
        }
    }

    /// <summary>Bölüm 3: üst ortada buzul çöküşünün ne kadar geride olduğunu gösteren panel.</summary>
    private void DrawCollapsePanel()
    {
        var panel = new Rectangle(444, 16, 388, 76);
        float danger = 1f - MathHelper.Clamp(_collapseGapKm / GameConfig.CollapseMaxGapKm, 0f, 1f);
        float pulse = _collapseGapKm < 4f ? 0.5f + 0.5f * MathF.Sin(_time * 10f) : 0f;
        UiDraw.FancyPanel(_spriteBatch, panel, Color.Lerp(new Color(120, 180, 220), Hud.Danger, danger));

        GameText.Draw(_spriteBatch, Loc.T("collapse.title"), new Vector2(panel.X + 16, panel.Y + 10), new Color(160, 220, 255));
        GameText.DrawRight(_spriteBatch, Loc.F("collapse.gap", MathF.Round(_collapseGapKm, 1)), panel.Right - 16, panel.Y + 10, Hud.Value);
        var bar = new Rectangle(panel.X + 16, panel.Y + 44, panel.Width - 32, 16);
        float ratio = _collapseGapKm / GameConfig.CollapseMaxGapKm;
        UiDraw.FancyBar(_spriteBatch, bar, ratio, ratio, Color.Lerp(Hud.Danger, new Color(120, 200, 255), ratio), pulse);

        if (_collapseGapKm < 4f)
            Hud.DrawBanner(_spriteBatch, Loc.T("collapse.warning"), _time);
    }

    /// <summary>Üst ortada mod paneli: zamanla yarışta geri sayım, sonsuz modda katedilen yol ve aşama.</summary>
    private void DrawModePanel()
    {
        if (_mode is GameMode.Story or GameMode.Wasteland) return;
        var panel = new Rectangle(444, 16, 388, 76);

        if (_mode == GameMode.Hardcore)
        {
            // Acımasız Kış: tek can uyarısı ve zorluk çarpanları
            float pulse = 0.7f + 0.3f * MathF.Sin(_time * 3f);
            UiDraw.FancyPanel(_spriteBatch, panel, Hud.Danger * pulse);
            GameText.Draw(_spriteBatch, Loc.T("hardcore.hud"), new Vector2(panel.X + 16, panel.Y + 10), Hud.Danger);
            GameText.Draw(_spriteBatch, Hud.FitWidth(Loc.F("hardcore.hudRules",
                    (int)MathF.Round((1f - GameConfig.HardcoreResourceMultiplier) * 100f), (int)GameConfig.HardcoreSabotageMultiplier),
                panel.Width - 32), new Vector2(panel.X + 16, panel.Y + 44), Hud.Label);
            return;
        }

        if (_mode == GameMode.Speedrun)
        {
            float left = SpeedrunTimeLeft;
            bool urgent = left < 30f;
            float pulse = urgent ? 0.5f + 0.5f * MathF.Sin(_time * 10f) : 0f;
            UiDraw.FancyPanel(_spriteBatch, panel, urgent ? Hud.Danger : Hud.Amber);
            GameText.Draw(_spriteBatch, Loc.T("speedrun.timer"), new Vector2(panel.X + 16, panel.Y + 10), Hud.Amber);
            GameText.DrawRight(_spriteBatch, Endings.FormatTime(left), panel.Right - 16, panel.Y + 6,
                Color.Lerp(Hud.Value, Hud.Danger, urgent ? 0.5f + 0.5f * pulse : 0f), 3);
            var bar = new Rectangle(panel.X + 16, panel.Y + 46, panel.Width - 32, 14);
            float ratio = left / GameConfig.SpeedrunTimeLimit;
            UiDraw.FancyBar(_spriteBatch, bar, ratio, ratio, Color.Lerp(Hud.Danger, Hud.Amber, ratio), pulse);
            return;
        }

        UiDraw.FancyPanel(_spriteBatch, panel, new Color(150, 200, 255));
        GameText.Draw(_spriteBatch, Loc.F("endless.hudStage", _endlessStage), new Vector2(panel.X + 16, panel.Y + 10), new Color(160, 220, 255));
        GameText.DrawRight(_spriteBatch, Loc.F("endless.hudKm", (int)_travelledKm), panel.Right - 16, panel.Y + 10, Hud.Value);
        int best = SaveData.Current.EndlessBestKm;
        string record = (int)_travelledKm > best && best > 0 ? Loc.T("endless.hudRecord") : Loc.F("endless.hudBest", best);
        GameText.Draw(_spriteBatch, record, new Vector2(panel.X + 16, panel.Y + 44), new Color(255, 215, 110));
    }

    /// <summary>Yaklaşılan durağın türüne göre ekran ortasındaki bildirim.</summary>
    private string ArrivalBanner() => NextStop.Kind switch
    {
        StopKind.Shelter => Loc.T("banner.shelter"),
        StopKind.Town => Loc.F("banner.town", TownCatalog.All[NextStop.Index].Name),
        StopKind.Depot => Loc.T("banner.depot"),
        StopKind.Hub => Loc.T("banner.hub"),
        _ => Loc.T("banner.station")
    };

    /// <summary>Vinyet ve kazan soğudukça ekran kenarlarından sızan buz.</summary>
    private void DrawAtmosphereOverlay()
    {
        var screen = new Rectangle(0, 0, GameConfig.ScreenWidth, GameConfig.ScreenHeight);
        _spriteBatch.Draw(Assets.Vignette, screen, Color.Black * (0.8f + 0.15f * _weather.Intensity));

        if (!IsInJourney) return; // Menüde ve finalde buz efekti yok
        float frost = 1f - MathHelper.Clamp(_train.Temperature / GameConfig.LowTemperatureWarning, 0f, 1f);
        if (frost > 0f)
            _spriteBatch.Draw(Assets.Vignette, screen, new Color(170, 210, 255) * (0.55f * frost));

        // Tipi ve şiddetli fırtına: ekran kenarlarında yarı saydam beyaz-mavi buğu ve buz kristalleri
        float storm = _events.IsBlizzard ? 1f : MathHelper.Clamp((_weather.Intensity - 0.6f) / 0.4f, 0f, 1f);
        _stormFrost = MathHelper.Lerp(_stormFrost, IsTravelling || _state == GameState.Chase ? storm : 0f, 0.03f);
        // Akıl sağlığı düşükken kenarlardan kızıl bir karanlık sızar; kızıl sanrıda ekran kana bürünür
        if (IsInJourney && _sanity.IsHallucinating)
            _spriteBatch.Draw(Assets.Vignette, screen, new Color(140, 0, 0) * (0.35f * _sanity.Madness * (0.8f + 0.2f * MathF.Sin(_time * 2f))));
        if (_redVisionTimer > 0f)
        {
            float pulse = MathF.Sin(_redVisionTimer / 2.5f * MathF.PI);
            _spriteBatch.FillRect(screen, new Color(150, 0, 0) * (0.35f * pulse));
            _spriteBatch.Draw(Assets.Vignette, screen, Color.Black * (0.8f * pulse));
        }

        // Asit fırtınası: kenarlardan sızan zehirli yeşil hale
        if (_events.IsAcidStorm && IsTravelling)
            _spriteBatch.Draw(Assets.Vignette, screen, new Color(140, 210, 60) * (0.45f + 0.15f * MathF.Sin(_time * 4f)));

        if (_stormFrost > 0.01f)
        {
            float breathe = 0.85f + 0.15f * MathF.Sin(_time * 1.3f);
            _spriteBatch.Draw(Assets.Vignette, screen, new Color(200, 225, 255) * (0.5f * _stormFrost * breathe));
            _spriteBatch.Draw(Assets.FrostEdge, screen, new Color(225, 240, 255) * (0.85f * _stormFrost));
        }
    }

    private void DrawInterface()
    {
        switch (_state)
        {
            case GameState.LanguageSelect:
                MenuScreens.DrawLanguageSelect(_spriteBatch, _languageCursor, _time, _stateTimer);
                break;

            case GameState.MainMenu:
                _meta.Draw(_spriteBatch);
                break;

            case GameState.TutorialPrompt:
                MenuScreens.DrawTutorialPrompt(_spriteBatch, _time, _stateTimer);
                break;

            case GameState.Tutorial:
                Hud.Draw(_spriteBatch, Loc.T("hud.tutorial"), _train, RemainingKm, NextStopKm - _travelledKm, NextStopIsShelter, _time);
                CabinPanel.Draw(_spriteBatch, _train, _cabin, _time, _inventory.Rations);
                InventoryPanel.Draw(_spriteBatch, _inventory);
                Hud.DrawControlsHint(_spriteBatch);
                TutorialPanel.Draw(_spriteBatch, _tutorial, _time);
                break;

            case GameState.ChapterIntro:
                MenuScreens.DrawChapterIntro(_spriteBatch, CurrentChapter, _stateTimer, IsStoryMode ? ChatterData.LogKey(CurrentChapter) : null);
                break;

            case GameState.Sentinel:
                if (!_sentinel.InIntro)
                {
                    Hud.Draw(_spriteBatch, CurrentChapter.HudTitle, _train, RemainingKm, NextStopKm - _travelledKm, NextStopIsShelter, _time, _sanity.Sanity);
                    CabinPanel.Draw(_spriteBatch, _train, _cabin, _time, _inventory.Rations);
                }
                _sentinel.DrawInterface(_spriteBatch, _time, _hack.IsOpen);
                _toasts.Draw(_spriteBatch);
                break;

            case GameState.DlcGate:
                _gate.Draw(_spriteBatch);
                break;

            case GameState.EpisodeOutro:
                _outro.Draw(_spriteBatch);
                break;

            case GameState.Driving:
            case GameState.Arriving:
                Hud.Draw(_spriteBatch, CurrentChapter.HudTitle, _train, RemainingKm, NextStopKm - _travelledKm, NextStopIsShelter, _time, _sanity.Sanity);
                CabinPanel.Draw(_spriteBatch, _train, _cabin, _time, _inventory.Rations);
                InventoryPanel.Draw(_spriteBatch, _inventory);
                EventBanner.Draw(_spriteBatch, _events, _time);
                Hud.DrawControlsHint(_spriteBatch);
                if (CurrentChapter.GlacierCollapse && !_events.IsValvePending)
                    DrawCollapsePanel();
                DrawModePanel();
                if (_state == GameState.Arriving)
                    Hud.DrawBanner(_spriteBatch, ArrivalBanner(), _time, _stateTimer);
                _chatter.Draw(_spriteBatch, _time);
                _toasts.Draw(_spriteBatch);
                break;

            case GameState.Chase:
                // Pusu uyarısı sırasında paneller gizlenir; ekran yalnızca alarma odaklanır
                if (!_chase.InIntro)
                {
                    Hud.Draw(_spriteBatch, CurrentChapter.HudTitle, _train, RemainingKm, NextStopKm - _travelledKm, NextStopIsShelter, _time, _sanity.Sanity);
                    CabinPanel.Draw(_spriteBatch, _train, _cabin, _time, _inventory.Rations);
                    DrawModePanel();
                }
                // Envanter paneli gizlenir ki ön plandaki haydut kamyonu görünsün (1-3 tuşları yine çalışır)
                _chase.DrawInterface(_spriteBatch, _time);
                _toasts.Draw(_spriteBatch);
                break;

            case GameState.Town:
                Hud.Draw(_spriteBatch, CurrentTown.Name, _train, RemainingKm, NextStopKm - _travelledKm, NextStopIsShelter, _time, _sanity.Sanity);
                CabinPanel.Draw(_spriteBatch, _train, _cabin, _time, _inventory.Rations);
                InventoryPanel.Draw(_spriteBatch, _inventory);
                if (_townDialog == TownDialog.None)
                {
                    _townScene.DrawInterface(_spriteBatch, _stateTimer, showPrompt: true);
                }
                else if (_townDialog == TownDialog.Store)
                {
                    _trader.Draw(_spriteBatch, _train, _inventory, _cabin);
                }
                else
                {
                    _spriteBatch.FillRect(new Rectangle(0, 0, GameConfig.ScreenWidth, GameConfig.ScreenHeight), Color.Black * 0.45f);
                    _dialog.Draw(_spriteBatch, _time);
                }
                _toasts.Draw(_spriteBatch);
                break;

            case GameState.Depot:
                InventoryPanel.Draw(_spriteBatch, _inventory);
                if (!_garage.IsOpen) _trader.Draw(_spriteBatch, _train, _inventory, _cabin); // Garajda tren arkada görünsün
                _toasts.Draw(_spriteBatch);
                break;

            case GameState.Station:
            case GameState.FinalChoice:
                // Dünyayı karart, diyaloğu öne çıkar
                _spriteBatch.FillRect(new Rectangle(0, 0, GameConfig.ScreenWidth, GameConfig.ScreenHeight),
                    Color.Black * MathHelper.Clamp(_stateTimer * 1.2f, 0f, 0.5f));
                _dialog.Draw(_spriteBatch, _time);
                break;

            case GameState.Mining:
                _mine.Draw(_spriteBatch);
                _toasts.Draw(_spriteBatch);
                break;

            case GameState.GameOver:
            case GameState.Victory:
                EndScreens.Draw(_spriteBatch, _ending, _stateTimer);
                break;
        }

        // Kabin menüsü (K): yolcuları incele, şüpheliyi at
        if (_cabinMenu.IsOpen)
        {
            _spriteBatch.FillRect(new Rectangle(0, 0, GameConfig.ScreenWidth, GameConfig.ScreenHeight), Color.Black * 0.55f);
            _cabinMenu.Draw(_spriteBatch, _cabin);
        }

        // Telsiz (T)
        if (_radioPanel.IsOpen)
            _radioPanel.Draw(_spriteBatch, _radio, _mafiaDeal);

        // Sinyal kulesi menzilde: hack ipucu
        if (_state == GameState.Driving && TowerInRange && !_hack.IsOpen)
        {
            string hint = Loc.T("tower.prompt");
            float w = GameText.MeasureLine(hint, 2) + 28f;
            var rect = new Rectangle((int)(GameConfig.ScreenWidth / 2f - w / 2f), 330, (int)w, 40);
            float pulse = 0.6f + 0.4f * MathF.Sin(_time * 8f);
            _spriteBatch.FillRect(rect, new Color(20, 4, 8) * 0.85f);
            _spriteBatch.DrawBorder(rect, 2, new Color(255, 50, 60) * pulse);
            GameText.DrawCentered(_spriteBatch, hint, GameConfig.ScreenWidth / 2f, rect.Y + 6, new Color(255, 200, 200), 2);
        }

        // AURA hack arabirimi
        if (_hack.IsOpen)
            _hack.Draw(_spriteBatch);

        // Hikâye diyaloğu (inceleme noktaları, hikâye bölümleri, yolcu görevleri)
        if (_story.IsOpen)
        {
            _spriteBatch.FillRect(new Rectangle(0, 0, GameConfig.ScreenWidth, GameConfig.ScreenHeight), Color.Black * 0.45f);
            _story.Draw(_spriteBatch, _time);
        }
        else if (IsStoryMode && (_state is GameState.Driving || _state == GameState.Town && _townDialog == TownDialog.None)
                 && !_cabinMenu.IsOpen && !_radioPanel.IsOpen && !_garage.IsOpen && AvailableQuest().Node != null)
        {
            // Konuşmak isteyen yolcu: küçük bir hatırlatma
            string hint = Loc.F("quest.hint", AvailableQuest().Passenger.Name);
            float w = GameText.MeasureLine(hint) + 24f;
            var rect = new Rectangle((int)(GameConfig.ScreenWidth / 2f - w / 2f), 404, (int)w, 28);
            float pulse = 0.75f + 0.25f * MathF.Sin(_time * 4f);
            _spriteBatch.FillRect(rect, Color.Black * 0.6f);
            GameText.DrawCentered(_spriteBatch, hint, GameConfig.ScreenWidth / 2f, rect.Y + 3, new Color(140, 220, 255) * pulse);
        }

        // Mola yerlerinde garaj ipucu ve garaj ekranı
        bool atRest = (_state == GameState.Depot || _state == GameState.Town && _townDialog == TownDialog.None && !CurrentTown.IsStoryScene)
                      && !_story.IsOpen;
        if (atRest && !_garage.IsOpen && !_cabinMenu.IsOpen)
        {
            string hint = Loc.T("garage.open");
            float w = GameText.MeasureLine(hint) + 24f;
            var rect = new Rectangle((int)(GameConfig.ScreenWidth / 2f - w / 2f), _state == GameState.Depot ? 8 : 368, (int)w, 28);
            _spriteBatch.FillRect(rect, Color.Black * 0.6f);
            GameText.DrawCentered(_spriteBatch, hint, GameConfig.ScreenWidth / 2f, rect.Y + 3, Hud.Amber * 0.9f);
        }
        if (_garage.IsOpen)
            _garage.Draw(_spriteBatch);
    }

    /// <summary>Fotoğraf modu: ilk karede arayüzsüz sahne yakalanır; sonra filtreli, yakınlaştırılmış görüntü çizilir.</summary>
    private void DrawPhotoMode()
    {
        if (!_photo.Captured)
        {
            _photo.Capture(GraphicsDevice, () =>
            {
                GraphicsDevice.Clear(NightColor);
                if (_state == GameState.Town) DrawTownScene();
                else DrawRouteScene();
                _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                DrawAtmosphereOverlay();
                _spriteBatch.End();
            });
        }

        GraphicsDevice.Clear(Color.Black);
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _photo.Draw(_spriteBatch);

        // Vizör köşeleri
        var c = Color.White * 0.6f;
        const int m = 24, l = 40;
        foreach (var (x, y, dx, dy) in new[] { (m, m, 1, 1), (GameConfig.ScreenWidth - m, m, -1, 1),
                     (m, GameConfig.ScreenHeight - m, 1, -1), (GameConfig.ScreenWidth - m, GameConfig.ScreenHeight - m, -1, -1) })
        {
            _spriteBatch.FillRect(new Rectangle(dx > 0 ? x : x - l, dy > 0 ? y : y - 2, l, 2), c);
            _spriteBatch.FillRect(new Rectangle(dx > 0 ? x : x - 2, dy > 0 ? y : y - l, 2, l), c);
        }

        if (!_photo.HideHelp)
        {
            string info = Loc.F("photo.status", Loc.T($"photo.filter.{_photo.Filter.ToString().ToLowerInvariant()}"),
                _photo.Zoom.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture));
            _spriteBatch.FillRect(new Rectangle(0, 0, GameConfig.ScreenWidth, 34), Color.Black * 0.55f);
            GameText.DrawCentered(_spriteBatch, Loc.T("photo.title") + "   " + info, GameConfig.ScreenWidth / 2f, 6, Hud.Amber);
            _spriteBatch.FillRect(new Rectangle(0, GameConfig.ScreenHeight - 34, GameConfig.ScreenWidth, 34), Color.Black * 0.55f);
            GameText.DrawCentered(_spriteBatch, Loc.T("photo.hint"), GameConfig.ScreenWidth / 2f, GameConfig.ScreenHeight - 28, Hud.Label);
        }
        if (_photo.SavedTimer > 0f && _photo.LastSaved != null)
        {
            string saved = Loc.F("photo.saved", System.IO.Path.GetFileName(_photo.LastSaved));
            float w = GameText.MeasureLine(saved) + 30f;
            float alpha = MathHelper.Clamp(_photo.SavedTimer, 0f, 1f);
            _spriteBatch.FillRect(new Rectangle((int)(GameConfig.ScreenWidth / 2f - w / 2f), 300, (int)w, 34), Color.Black * (0.75f * alpha));
            GameText.DrawCentered(_spriteBatch, saved, GameConfig.ScreenWidth / 2f, 306, CabinPanel.Good * alpha);
        }
        _spriteBatch.End();
    }
}
