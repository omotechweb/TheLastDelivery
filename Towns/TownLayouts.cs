using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TheLastDelivery.Story;

namespace TheLastDelivery.Towns;

/// <summary>Kasabada çizilen yapı türleri.</summary>
public enum StructureKind
{
    House, Tavern, Store, Mechanic, Bunker,
    Stall, NoticeBoard, Headframe, MineCart, Platform, Loudspeaker, Gramophone, TicketBooth,
    Shrine, Totem, Tent, Ruin, Barracks, Watchtower, Barrier, Cage, Crate, Fence, Mailbox, Swing, Shed,
    Headquarters, RadioMast, Hut, LabDome, AuroraTower, Terminal, CoreCradle, ShelterWall, ArmoredCar, Searchlight,
    // Episode 2: AURA'nın makine dünyası
    DroneTurret, SleepTubes, ServerRack, ChromeSpire, AuraScreen, SignalTowerBig, SentinelWreck, BlastDoor, Elevator, SatelliteDish,
    // Derin anlatı: çevresel hikâye (dışarıda) ve iç mekân eşyaları
    FrozenFigure, WagonWreck,
    BarCounter, Fireplace, Shelf, Desk, Lockers, Workbench, Bunks, RadioConsole, MapBoard, Table, Boiler, TapeDeck
}

/// <summary>Bir yapının kasabadaki yeri. Variant: renk/biçim çeşidi (0 normal, 1-2 çeşit, 9 yıkık/karanlık).</summary>
public sealed record Placement(StructureKind Kind, float X, int Variant = 0);

/// <summary>Ayakta duran bir kişi (etkileşimli olsun olmasın).</summary>
public sealed record PersonSpot(float X, int Facing, Color Coat, Color Scarf);

/// <summary>
/// Bir kasaba şablonunun tamamı: yapılar, etkileşimler, insanlar, lambalar, satıcı/hırsız ve ortamın kasveti.
/// </summary>
public sealed record TownLayout(
    Placement[] Structures,
    Interactable[] Interactables,
    PersonSpot[] People,
    float[] Lamps,
    float PeddlerX = -1f,          // Seyyar satıcı (yoksa -1)
    bool Thief = false,            // Bu kasabada hırsız çıkabilir mi?
    float Gloom = 0f,              // 0-1: kasvet (karanlık perde)
    bool DistantLights = true)     // Arka plandaki uzak evlerin ışıkları
{
    /// <summary>Yürünebilen genişlik (iç mekânlar dar).</summary>
    public int Width { get; init; } = Core.GameConfig.TownWidth;

    /// <summary>İç mekânsa türü (yoksa dış mekân).</summary>
    public InteriorKind? Interior { get; init; }
}

/// <summary>
/// Altı kasaba mimarisi ve altı hikâye sahnesi. Her biri farklı binalar, farklı NPC'ler ve
/// kendine özgü inceleme noktalarıyla gelir (bkz. NarrativeData).
/// </summary>
public static class TownLayouts
{
    private static readonly Dictionary<TownArchetype, TownLayout> Cache = new();

    private static Interactable Train => new(InteractableKind.Train, 560f, 380f);
    private static Interactable Spot(string id, float x, float promptY = 400f) => new(InteractableKind.Spot, x, promptY) { SpotId = id };
    private static PersonSpot P(float x, int facing, Color coat, Color scarf) => new(x, facing, coat, scarf);

    public static TownLayout For(TownArchetype archetype)
    {
        if (!Cache.TryGetValue(archetype, out var layout))
            Cache[archetype] = layout = WithExtras(archetype, Build(archetype));
        return layout;
    }

    /// <summary>
    /// Derin anlatı eklemeleri: hizmet binaları (taverna, tamirci, sığınak) girilebilir kapılara dönüşür, ek binalar
    /// (karakol, telsiz kulesi) ve dışarıdaki çevresel hikâye noktaları (donmuş insanlar, vagon enkazları) eklenir.
    /// </summary>
    private static TownLayout WithExtras(TownArchetype archetype, TownLayout layout)
    {
        var structures = new List<Placement>(layout.Structures);
        structures.AddRange(InteriorCatalog.ExtraStructures(archetype));

        var interactables = new List<Interactable>();
        foreach (var item in layout.Interactables)
        {
            var interior = InteriorCatalog.ForService(item.Kind);
            interactables.Add(interior != null
                ? new Interactable(InteractableKind.Door, item.X, item.PromptY) { Interior = interior, Service = item.Kind, NameKey = item.NameKey }
                : item);
        }
        foreach (var (kind, x, promptY) in InteriorCatalog.ExtraDoors(archetype))
            interactables.Add(new Interactable(InteractableKind.Door, x, promptY) { Interior = kind });
        interactables.AddRange(InteriorCatalog.ExtraSpots(archetype));

        return layout with { Structures = structures.ToArray(), Interactables = interactables.ToArray() };
    }

    private static TownLayout Build(TownArchetype archetype) => archetype switch
    {
        // Ticaret Merkezi: renkli tenteli tezgâhlar, kalabalık, müzayedeci ve zengin tüccar
        TownArchetype.TradeHub => new(
            new[]
            {
                new Placement(StructureKind.House, 920f), new Placement(StructureKind.Tavern, 1180f),
                new Placement(StructureKind.Stall, 1440f), new Placement(StructureKind.Store, 1700f),
                new Placement(StructureKind.Stall, 1930f, 1), new Placement(StructureKind.Mechanic, 2180f),
                new Placement(StructureKind.Bunker, 2520f), new Placement(StructureKind.NoticeBoard, 2700f),
                new Placement(StructureKind.House, 2900f, 1)
            },
            new[]
            {
                Train, new(InteractableKind.Tavern, 1180f, 330f), Spot("trade.auction", 1440f, 410f),
                new(InteractableKind.Store, 1700f, 360f), Spot("trade.merchant", 1930f, 410f),
                new(InteractableKind.Mechanic, 2180f, 350f), new(InteractableKind.Npc, 2350f, 440f),
                new(InteractableKind.Bunker, 2520f, 370f), Spot("trade.board", 2700f, 420f),
                new(InteractableKind.Peddler, 2860f, 420f)
            },
            new[]
            {
                P(1440f, -1, new Color(120, 40, 50), new Color(230, 190, 80)), P(1930f, -1, new Color(90, 60, 30), new Color(200, 160, 60)),
                P(2350f, -1, new Color(70, 80, 96), new Color(120, 60, 50)), P(1570f, 1, new Color(60, 70, 60), new Color(150, 90, 60)),
                P(1600f, -1, new Color(80, 60, 70), new Color(90, 120, 160)), P(2050f, 1, new Color(70, 64, 50), new Color(160, 60, 60))
            },
            new[] { 1030f, 1310f, 1580f, 1820f, 2060f, 2350f, 2620f, 2790f },
            PeddlerX: 2840f, Thief: true),

        // Madenci Köyü: maden kulesi, vagonetler, grevci işçiler
        TownArchetype.MiningVillage => new(
            new[]
            {
                new Placement(StructureKind.House, 900f, 2), new Placement(StructureKind.Tavern, 1160f, 1),
                new Placement(StructureKind.Headframe, 1480f), new Placement(StructureKind.MineCart, 1650f),
                new Placement(StructureKind.Store, 1840f, 1), new Placement(StructureKind.Mechanic, 2120f),
                new Placement(StructureKind.House, 2440f, 2), new Placement(StructureKind.House, 2760f)
            },
            new[]
            {
                Train, new(InteractableKind.Tavern, 1160f, 330f), Spot("mine.shaft", 1480f, 340f),
                new(InteractableKind.Npc, 1640f, 440f), new(InteractableKind.Store, 1840f, 360f),
                Spot("mine.strike", 1990f, 420f), new(InteractableKind.Mechanic, 2120f, 350f),
                Spot("mine.foreman", 2290f, 420f)
            },
            new[]
            {
                P(1640f, -1, new Color(80, 60, 50), new Color(180, 50, 50)), P(1990f, -1, new Color(70, 60, 54), new Color(200, 50, 40)),
                P(2290f, -1, new Color(60, 54, 40), new Color(200, 170, 60)), P(2030f, 1, new Color(60, 56, 52), new Color(150, 40, 40)),
                P(1950f, 1, new Color(66, 60, 56), new Color(140, 60, 50))
            },
            new[] { 1040f, 1320f, 1740f, 1980f, 2300f, 2600f },
            Thief: true),

        // Hayalet İstasyon: uzun peron, durmuş saat, gramofon, hoparlör; kimse yok
        TownArchetype.GhostStation => new(
            new[]
            {
                new Placement(StructureKind.Platform, 1350f), new Placement(StructureKind.Loudspeaker, 1100f),
                new Placement(StructureKind.Gramophone, 1350f), new Placement(StructureKind.TicketBooth, 1660f),
                new Placement(StructureKind.House, 1980f, 9), new Placement(StructureKind.Ruin, 2600f, 1)
            },
            new[]
            {
                Train, Spot("ghost.speaker", 1100f, 340f), Spot("ghost.gramophone", 1350f, 420f),
                Spot("ghost.ticket", 1660f, 380f), Spot("ghost.waiting", 1980f, 380f),
                new(InteractableKind.Npc, 2280f, 440f)
            },
            new[] { P(2280f, -1, new Color(70, 56, 40), new Color(200, 160, 90)) },
            new[] { 2200f },
            Gloom: 0.35f, DistantLights: false),

        // Buzul Tarikatı: buz tapınağı, totem, çadırlar, beyaz cüppeli müritler
        TownArchetype.IceCult => new(
            new[]
            {
                new Placement(StructureKind.Tent, 950f), new Placement(StructureKind.Shrine, 1260f),
                new Placement(StructureKind.Totem, 1560f), new Placement(StructureKind.Store, 1820f, 2),
                new Placement(StructureKind.Tent, 2080f, 1), new Placement(StructureKind.Tent, 2560f),
                new Placement(StructureKind.House, 2820f, 9)
            },
            new[]
            {
                Train, Spot("cult.priestess", 1260f, 400f), Spot("cult.totem", 1560f, 330f),
                new(InteractableKind.Store, 1820f, 360f) { NameKey = "sign.cultStore" }, Spot("cult.ilya", 2080f, 420f),
                Spot("cult.acolyte", 2290f, 420f), new(InteractableKind.Npc, 2470f, 440f)
            },
            new[]
            {
                P(1260f, -1, new Color(220, 226, 236), new Color(150, 200, 255)), P(2080f, -1, new Color(50, 64, 90), new Color(170, 200, 255)),
                P(2290f, -1, new Color(150, 160, 180), new Color(200, 220, 255)), P(2470f, -1, new Color(40, 44, 56), new Color(170, 200, 255)),
                P(1340f, 1, new Color(160, 170, 190), new Color(200, 220, 255)), P(1180f, 1, new Color(160, 170, 190), new Color(200, 220, 255))
            },
            new[] { 1110f, 1420f, 1700f, 1950f, 2400f },
            Gloom: 0.15f),

        // Terk Edilmiş Karakol: yıkık barakalar, devrik kule; yalnızca yağma
        TownArchetype.AbandonedOutpost => new(
            new[]
            {
                new Placement(StructureKind.Ruin, 950f), new Placement(StructureKind.Barracks, 1260f, 9),
                new Placement(StructureKind.Watchtower, 1540f, 9), new Placement(StructureKind.Crate, 1720f),
                new Placement(StructureKind.House, 1980f, 9), new Placement(StructureKind.Barracks, 2320f, 9),
                new Placement(StructureKind.Ruin, 2680f, 1)
            },
            new[]
            {
                Train, Spot("outpost.armory", 1260f, 350f), Spot("outpost.locker", 1720f, 420f),
                Spot("outpost.office", 1980f, 360f), Spot("outpost.zoya", 2320f, 350f)
            },
            System.Array.Empty<PersonSpot>(),
            System.Array.Empty<float>(),
            Gloom: 0.3f, DistantLights: false),

        // Askeri Sınır Kapısı: gözetleme kuleleri, bariyer, dikenli tel, projektörler
        TownArchetype.MilitaryGate => new(
            new[]
            {
                new Placement(StructureKind.Watchtower, 900f), new Placement(StructureKind.Barracks, 1160f),
                new Placement(StructureKind.Tavern, 1450f, 2), new Placement(StructureKind.Store, 1730f, 3),
                new Placement(StructureKind.Barrier, 1980f), new Placement(StructureKind.Mechanic, 2240f, 1),
                new Placement(StructureKind.NoticeBoard, 2440f, 1), new Placement(StructureKind.Cage, 2620f),
                new Placement(StructureKind.Watchtower, 2860f), new Placement(StructureKind.Searchlight, 1300f),
                new Placement(StructureKind.Searchlight, 2740f)
            },
            new[]
            {
                Train, new(InteractableKind.Bunker, 1160f, 370f) { NameKey = "sign.barracks" },
                new(InteractableKind.Tavern, 1450f, 330f) { NameKey = "sign.canteen" },
                new(InteractableKind.Store, 1730f, 360f) { NameKey = "sign.quartermaster" },
                Spot("mil.officer", 1980f, 400f), new(InteractableKind.Npc, 2100f, 440f),
                new(InteractableKind.Mechanic, 2240f, 350f) { NameKey = "sign.armory" },
                Spot("mil.board", 2440f, 420f), Spot("mil.prisoner", 2620f, 410f)
            },
            new[]
            {
                P(1980f, -1, new Color(60, 70, 54), new Color(230, 190, 70)), P(2100f, -1, new Color(70, 40, 34), new Color(255, 140, 110)),
                P(2620f, -1, new Color(70, 66, 60), new Color(60, 56, 50)), P(1890f, 1, new Color(60, 70, 54), new Color(80, 90, 70)),
                P(2780f, -1, new Color(60, 70, 54), new Color(80, 90, 70))
            },
            new[] { 1040f, 1600f, 2340f, 2740f }),

        // ---- Hikâye bölümleri ----
        TownArchetype.Headquarters => new(
            new[]
            {
                new Placement(StructureKind.Ruin, 900f), new Placement(StructureKind.Headquarters, 1220f),
                new Placement(StructureKind.Barracks, 1530f, 9), new Placement(StructureKind.Hut, 1820f, 9),
                new Placement(StructureKind.RadioMast, 2110f, 9), new Placement(StructureKind.Crate, 2360f, 1),
                new Placement(StructureKind.Ruin, 2660f, 1)
            },
            new[]
            {
                Train, Spot("hq.gate", 1220f, 330f), Spot("hq.barracks", 1530f, 350f), Spot("hq.diary", 1820f, 370f),
                Spot("hq.radio", 2110f, 320f), Spot("hq.safe", 2360f, 420f), Spot("hq.map", 2660f, 370f)
            },
            System.Array.Empty<PersonSpot>(),
            System.Array.Empty<float>(),
            Gloom: 0.45f, DistantLights: false),

        TownArchetype.Homestead => new(
            new[]
            {
                new Placement(StructureKind.Fence, 980f), new Placement(StructureKind.Mailbox, 1120f),
                new Placement(StructureKind.Swing, 1320f), new Placement(StructureKind.House, 1660f, 3),
                new Placement(StructureKind.Shed, 2100f), new Placement(StructureKind.House, 2480f, 9),
                new Placement(StructureKind.House, 2820f, 9)
            },
            new[]
            {
                Train, Spot("home.gate", 980f, 430f), Spot("home.letter", 1120f, 440f), Spot("home.swing", 1320f, 400f),
                Spot("home.photo", 1570f, 360f), Spot("home.musicbox", 1760f, 360f), Spot("home.order", 2100f, 380f)
            },
            System.Array.Empty<PersonSpot>(),
            new[] { 1460f },
            Gloom: 0.25f, DistantLights: false),

        TownArchetype.BorderControl => new(
            new[]
            {
                new Placement(StructureKind.Watchtower, 1000f), new Placement(StructureKind.Barrier, 1300f),
                new Placement(StructureKind.ShelterWall, 1700f, 1), new Placement(StructureKind.Searchlight, 1150f),
                new Placement(StructureKind.Watchtower, 2300f), new Placement(StructureKind.Barracks, 2650f)
            },
            new[] { Train },
            new[]
            {
                P(1180f, -1, new Color(70, 80, 100), new Color(140, 200, 255)), P(1380f, -1, new Color(60, 70, 54), new Color(80, 90, 70)),
                P(980f, 1, new Color(60, 70, 54), new Color(80, 90, 70)), P(700f, 1, new Color(80, 70, 60), new Color(120, 100, 80))
            },
            new[] { 1120f, 1500f },
            Gloom: 0.2f, DistantLights: false),

        TownArchetype.Institute => new(
            new[]
            {
                new Placement(StructureKind.LabDome, 1120f), new Placement(StructureKind.AuroraTower, 1500f),
                new Placement(StructureKind.Terminal, 1800f), new Placement(StructureKind.Hut, 2060f, 1),
                new Placement(StructureKind.CoreCradle, 2320f), new Placement(StructureKind.Crate, 2620f, 2)
            },
            new[]
            {
                Train, Spot("lab.gate", 1120f, 330f), Spot("lab.tower", 1500f, 300f), Spot("lab.terminal", 1800f, 400f),
                Spot("lab.report", 2060f, 370f), Spot("lab.core", 2320f, 400f), Spot("lab.cache", 2620f, 420f) with { RequireFlag = Flags.TradeMap }
            },
            System.Array.Empty<PersonSpot>(),
            new[] { 1650f, 2200f },
            Gloom: 0.3f, DistantLights: false),

        TownArchetype.RadioMast => new(
            new[] { new Placement(StructureKind.Hut, 1100f), new Placement(StructureKind.RadioMast, 1320f) },
            new[] { Train },
            System.Array.Empty<PersonSpot>(),
            new[] { 1000f },
            Gloom: 0.4f, DistantLights: false),

        // ---- Episode 2 kasabaları ----

        // İnsansız askeri karakol: taretler, antenler, makine hurdası; tek "canlı" bir servis droidi
        TownArchetype.DroneOutpost => new(
            new[]
            {
                new Placement(StructureKind.DroneTurret, 950f), new Placement(StructureKind.Barracks, 1230f, 9),
                new Placement(StructureKind.SatelliteDish, 1520f), new Placement(StructureKind.Terminal, 1760f),
                new Placement(StructureKind.Crate, 1990f, 2), new Placement(StructureKind.DroneTurret, 2250f, 1),
                new Placement(StructureKind.Store, 2500f, 4), new Placement(StructureKind.Ruin, 2800f, 1)
            },
            new[]
            {
                Train, Spot("drone.turret", 950f, 360f), Spot("drone.yusuf", 1230f, 360f), Spot("drone.console", 1760f, 400f),
                Spot("drone.depot", 1990f, 420f), new(InteractableKind.Npc, 2160f, 440f),
                new(InteractableKind.Store, 2500f, 360f) { NameKey = "sign.vending" }
            },
            System.Array.Empty<PersonSpot>(),
            new[] { 1100f, 1650f, 2380f },
            Gloom: 0.3f, DistantLights: false),

        // Kış uykusu fabrikası: tüp sıraları, bacalar, işçi kantini, siber-tarikatçı vaiz
        TownArchetype.HibernationFactory => new(
            new[]
            {
                new Placement(StructureKind.SleepTubes, 980f), new Placement(StructureKind.Tavern, 1290f, 2),
                new Placement(StructureKind.SleepTubes, 1600f, 1), new Placement(StructureKind.Store, 1880f, 3),
                new Placement(StructureKind.Mechanic, 2160f, 1), new Placement(StructureKind.AuraScreen, 2480f),
                new Placement(StructureKind.SleepTubes, 2780f)
            },
            new[]
            {
                Train, Spot("fac.tubes", 980f, 380f), new(InteractableKind.Tavern, 1290f, 330f) { NameKey = "sign.canteen" },
                Spot("fac.belt", 1600f, 380f), new(InteractableKind.Store, 1880f, 360f) { NameKey = "sign.quartermaster" },
                new(InteractableKind.Npc, 2030f, 440f), new(InteractableKind.Mechanic, 2160f, 350f),
                Spot("fac.preacher", 2480f, 420f)
            },
            new[]
            {
                P(2030f, -1, new Color(150, 160, 176), new Color(120, 200, 255)), P(2480f, -1, new Color(40, 44, 52), new Color(80, 255, 200)),
                P(1420f, 1, new Color(70, 64, 60), new Color(170, 90, 50)), P(1460f, -1, new Color(66, 60, 56), new Color(140, 140, 150))
            },
            new[] { 1140f, 1450f, 1740f, 2320f, 2640f },
            Gloom: 0.15f),

        // NULL'un krom şapeli: sunucu sunağı, krom kuleler, dev AURA ekranı
        TownArchetype.CyberChapel => new(
            new[]
            {
                new Placement(StructureKind.ChromeSpire, 950f), new Placement(StructureKind.ServerRack, 1250f),
                new Placement(StructureKind.AuraScreen, 1560f, 1), new Placement(StructureKind.Tent, 1850f, 1),
                new Placement(StructureKind.Store, 2120f, 2), new Placement(StructureKind.ChromeSpire, 2450f, 1),
                new Placement(StructureKind.Hut, 2760f, 9)
            },
            new[]
            {
                Train, Spot("chapel.altar", 1250f, 380f), Spot("chapel.screen", 1560f, 330f), Spot("chapel.kiril", 1850f, 420f),
                new(InteractableKind.Store, 2120f, 360f) { NameKey = "sign.cultStore" }, new(InteractableKind.Npc, 2300f, 440f)
            },
            new[]
            {
                P(1850f, -1, new Color(40, 44, 52), new Color(80, 255, 200)), P(2300f, -1, new Color(150, 160, 180), new Color(80, 255, 200)),
                P(1180f, 1, new Color(150, 160, 180), new Color(80, 255, 200)), P(1330f, -1, new Color(150, 160, 180), new Color(80, 255, 200))
            },
            new[] { 1100f, 1700f, 2280f },
            Gloom: 0.2f),

        // ---- Episode 2 hikâye sahneleri ----
        TownArchetype.SignalTower => new(
            new[]
            {
                new Placement(StructureKind.SentinelWreck, 1050f), new Placement(StructureKind.SignalTowerBig, 1500f),
                new Placement(StructureKind.Terminal, 1820f), new Placement(StructureKind.Hut, 2150f), new Placement(StructureKind.RadioMast, 2330f)
            },
            new[]
            {
                Train, Spot("tower.wreck", 1050f, 380f), Spot("tower.base", 1500f, 300f),
                Spot("tower.console", 1820f, 400f), Spot("tower.hut", 2150f, 380f)
            },
            new[] { P(2190f, -1, new Color(40, 34, 56), new Color(200, 80, 255)) },
            new[] { 1300f },
            Gloom: 0.35f, DistantLights: false),

        TownArchetype.RelayStation => new(
            new[]
            {
                new Placement(StructureKind.SatelliteDish, 1000f), new Placement(StructureKind.ServerRack, 1350f, 1),
                new Placement(StructureKind.AuraScreen, 1700f), new Placement(StructureKind.Ruin, 2050f, 1),
                new Placement(StructureKind.Terminal, 2350f), new Placement(StructureKind.SignalTowerBig, 2650f, 1)
            },
            new[]
            {
                Train, Spot("relay.archive", 1350f, 380f), Spot("relay.directive", 1700f, 330f),
                Spot("relay.null", 2050f, 380f), Spot("relay.uplink", 2350f, 400f)
            },
            System.Array.Empty<PersonSpot>(),
            new[] { 1500f, 2200f },
            Gloom: 0.4f, DistantLights: false),

        TownArchetype.SleepFactory => new(
            new[]
            {
                new Placement(StructureKind.SleepTubes, 1000f), new Placement(StructureKind.SleepTubes, 1320f, 1),
                new Placement(StructureKind.Terminal, 1650f), new Placement(StructureKind.SleepTubes, 1950f),
                new Placement(StructureKind.AuraScreen, 2280f), new Placement(StructureKind.SleepTubes, 2600f, 1)
            },
            new[]
            {
                Train, Spot("factory.tubes", 1000f, 380f), Spot("factory.ada", 1320f, 380f),
                Spot("factory.control", 1650f, 400f), Spot("factory.warden", 1950f, 420f),
                Spot("factory.mara", 2600f, 400f) with { RequireFlag = "aboard:mara" } // Mara'nın kızı Lena'nın tüpü
            },
            new[] { P(1950f, -1, new Color(150, 160, 176), new Color(120, 200, 255)) },
            new[] { 1160f, 1800f, 2440f },
            Gloom: 0.3f, DistantLights: false),

        TownArchetype.NullChapel => new(
            new[]
            {
                new Placement(StructureKind.ChromeSpire, 1050f), new Placement(StructureKind.AuraScreen, 1350f, 1),
                new Placement(StructureKind.ServerRack, 1650f), new Placement(StructureKind.ChromeSpire, 1950f, 1)
            },
            new[] { Train },
            new[]
            {
                P(1250f, -1, new Color(30, 30, 36), new Color(80, 255, 200)), P(1430f, -1, new Color(150, 160, 180), new Color(80, 255, 200)),
                P(1500f, -1, new Color(150, 160, 180), new Color(80, 255, 200)), P(1560f, -1, new Color(150, 160, 180), new Color(80, 255, 200))
            },
            new[] { 1200f, 1800f },
            Gloom: 0.35f, DistantLights: false),

        TownArchetype.PolarBase => new(
            new[]
            {
                new Placement(StructureKind.BlastDoor, 1100f), new Placement(StructureKind.Barracks, 1450f, 9),
                new Placement(StructureKind.ServerRack, 1780f, 1), new Placement(StructureKind.Elevator, 2150f),
                new Placement(StructureKind.Searchlight, 1300f), new Placement(StructureKind.SleepTubes, 2500f, 1)
            },
            new[]
            {
                Train, Spot("base.gate", 1100f, 340f), Spot("base.barracks", 1450f, 360f),
                Spot("base.archive", 1780f, 380f), Spot("base.elevator", 2150f, 340f),
                Spot("base.cryo", 2500f, 380f) with { RequireFlag = "aboard:yusuf" } // Dördüncü kat: Yusuf'un kardeşi Emre
            },
            System.Array.Empty<PersonSpot>(),
            new[] { 1620f, 2000f },
            Gloom: 0.45f, DistantLights: false),

        TownArchetype.ServerHall => new(
            new[]
            {
                new Placement(StructureKind.ServerRack, 950f), new Placement(StructureKind.ServerRack, 1200f, 1),
                new Placement(StructureKind.AuraScreen, 1500f, 1), new Placement(StructureKind.ServerRack, 1800f),
                new Placement(StructureKind.ServerRack, 2050f, 1)
            },
            new[] { Train },
            System.Array.Empty<PersonSpot>(),
            new[] { 1350f, 1650f },
            Gloom: 0.45f, DistantLights: false),

        TownArchetype.MirrorRoom => new(
            new[]
            {
                new Placement(StructureKind.AuraScreen, 1000f), new Placement(StructureKind.AuraScreen, 1300f, 1),
                new Placement(StructureKind.AuraScreen, 1600f), new Placement(StructureKind.AuraScreen, 1900f, 1)
            },
            new[] { Train },
            System.Array.Empty<PersonSpot>(),
            System.Array.Empty<float>(),
            Gloom: 0.5f, DistantLights: false),

        TownArchetype.AuraCore => new(
            new[]
            {
                new Placement(StructureKind.ServerRack, 1000f, 1), new Placement(StructureKind.AuraScreen, 1300f, 1),
                new Placement(StructureKind.SignalTowerBig, 1600f, 2), new Placement(StructureKind.AuraScreen, 1900f, 1),
                new Placement(StructureKind.ServerRack, 2200f)
            },
            new[] { Train },
            System.Array.Empty<PersonSpot>(),
            System.Array.Empty<float>(),
            Gloom: 0.4f, DistantLights: false),

        _ => new( // ShelterGate: Sığınak-0'ın dış duvarı ve Konsey'in zırhlı aracı
            new[]
            {
                new Placement(StructureKind.ArmoredCar, 1120f), new Placement(StructureKind.ShelterWall, 1800f),
                new Placement(StructureKind.Searchlight, 1500f), new Placement(StructureKind.Searchlight, 2100f)
            },
            new[] { Train },
            new[]
            {
                P(1020f, -1, new Color(230, 232, 236), new Color(200, 220, 255)), P(1240f, -1, new Color(200, 204, 210), new Color(120, 140, 170)),
                P(1300f, -1, new Color(200, 204, 210), new Color(120, 140, 170))
            },
            new[] { 1400f },
            Gloom: 0.2f, DistantLights: false)
    };
}
