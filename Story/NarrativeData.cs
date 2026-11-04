using System.Collections.Generic;
using TheLastDelivery.Items;
using TheLastDelivery.UI;

namespace TheLastDelivery.Story;

/// <summary>
/// Bütün diyalog ve inceleme düğümleri, kasaba inceleme noktaları ve yolcu görev zincirleri.
/// Metinler çeviri tablolarındadır: düğüm "nv.{id}", seçenekler "nv.{id}.o{n}", inceleme noktası adı "spot.{id}".
/// </summary>
public static partial class NarrativeData
{
    private static readonly Dictionary<string, NarrativeNode> Nodes = new();
    private static readonly Dictionary<string, SpotDef> SpotTable = new();

    public static NarrativeNode Node(string id) => id != null && Nodes.TryGetValue(id, out var n) ? n : null;
    public static SpotDef Spot(string id) => SpotTable.TryGetValue(id, out var s) ? s : null;

    // ---- Kısa yazım yardımcıları ----
    private static NarrativeOption O(string next = null, NarrativeEffect e = null, string req = null, string forbid = null) =>
        new(next, e, req, forbid);

    private static NarrativeOption End(NarrativeEffect e = null, string req = null, string forbid = null) =>
        new(null, e, req, forbid);

    private static NarrativeNode Add(NarrativeNode node)
    {
        Nodes[node.Id] = node;
        return node;
    }

    /// <summary>Konuşan biri olan düğüm.</summary>
    private static void Talk(string id, string speaker, PortraitKind portrait, params NarrativeOption[] options) =>
        Add(new NarrativeNode(id, speaker, portrait, options));

    /// <summary>Anlatıcı/inceleme düğümü (portresiz).</summary>
    private static void Look(string id, params NarrativeOption[] options) =>
        Add(new NarrativeNode(id, null, PortraitKind.Arthur, options));

    private static void Spot(string id, bool talk, params string[] entries) => SpotTable[id] = new SpotDef(id, talk, entries);

    private static void SpotRest(string id, bool talk, string rest, params string[] entries) =>
        SpotTable[id] = new SpotDef(id, talk, entries) { Rest = rest };

    static NarrativeData()
    {
        BuildTradeHub();
        BuildMiningVillage();
        BuildGhostStation();
        BuildIceCult();
        BuildOutpost();
        BuildMilitary();
        BuildHeadquarters();
        BuildHomestead();
        BuildBorder();
        BuildInstitute();
        BuildRadio();
        BuildCouncil();
        BuildQuests();
        BuildStoryBeats();
        BuildEpisode2();
        BuildDeep();   // Derin anlatı: iç mekânlar, liderlerin diyalog ağları, belgeler, kasetler, hatıralar, dönüm noktaları
        BuildDeep2();  // Episode 2 derin anlatı: demo bölümleri, AURA'nın felsefi sorguları, Kusursuz Dünya simülasyonları
    }

    // ==================================================================
    // Kasabalar
    // ==================================================================

    private static void BuildTradeHub()
    {
        const string auctioneer = "spk.auctioneer", merchant = "spk.merchant";
        Talk("trade.auction.1", auctioneer, PortraitKind.Auctioneer,
            O("trade.auction.1b", new NarrativeEffect { Credits = -40, SetFlag = Flags.TradeMap }),
            End());
        Look("trade.auction.1b");
        Talk("trade.auction.2", auctioneer, PortraitKind.Auctioneer);
        Talk("trade.auction.3", auctioneer, PortraitKind.Auctioneer);
        Talk("trade.auction.rest", auctioneer, PortraitKind.Auctioneer);
        SpotRest("trade.auction", true, "trade.auction.rest", "trade.auction.1", "trade.auction.2", "trade.auction.3");

        Talk("trade.merchant.1", merchant, PortraitKind.Merchant,
            End(new NarrativeEffect { Credits = 50, Karma = -5, SetFlag = Flags.TradeContraband }),
            End(new NarrativeEffect { Karma = 5 }));
        Talk("trade.merchant.2", merchant, PortraitKind.Merchant);
        Talk("trade.merchant.3", merchant, PortraitKind.Merchant);
        Talk("trade.merchant.rest", merchant, PortraitKind.Merchant);
        SpotRest("trade.merchant", true, "trade.merchant.rest", "trade.merchant.1", "trade.merchant.2", "trade.merchant.3");

        Look("trade.board.1");
        Look("trade.board.2");
        Look("trade.board.3");
        Spot("trade.board", false, "trade.board.1", "trade.board.2", "trade.board.3");
    }

    private static void BuildMiningVillage()
    {
        const string yuri = "spk.strike", gromov = "spk.foreman";
        Talk("mine.strike.1", yuri, PortraitKind.StrikeLeader,
            End(new NarrativeEffect { Rations = -3, Karma = 15, SetFlag = Flags.MineWorkers }, forbid: Flags.MineOwner),
            O("mine.strike.1b", new NarrativeEffect { Credits = 40, Karma = -15, SetFlag = Flags.MineOwner }, forbid: Flags.MineWorkers),
            End());
        Talk("mine.strike.1b", gromov, PortraitKind.Foreman);
        Talk("mine.strike.2", yuri, PortraitKind.StrikeLeader);
        Talk("mine.strike.3", yuri, PortraitKind.StrikeLeader);
        Talk("mine.strike.rest", yuri, PortraitKind.StrikeLeader);
        SpotRest("mine.strike", true, "mine.strike.rest", "mine.strike.1", "mine.strike.2", "mine.strike.3");

        Talk("mine.foreman.1", gromov, PortraitKind.Foreman);
        Talk("mine.foreman.2", gromov, PortraitKind.Foreman);
        Talk("mine.foreman.3", gromov, PortraitKind.Foreman);
        Spot("mine.foreman", true, "mine.foreman.1", "mine.foreman.2", "mine.foreman.3");

        Add(new NarrativeNode("mine.shaft.1", null, PortraitKind.Arthur) { OnEnter = new NarrativeEffect { Loot = 1 } });
        Look("mine.shaft.2");
        Add(new NarrativeNode("mine.shaft.3", null, PortraitKind.Arthur) { OnEnter = new NarrativeEffect { Sanity = -5 } });
        Spot("mine.shaft", false, "mine.shaft.1", "mine.shaft.2", "mine.shaft.3");
    }

    private static void BuildGhostStation()
    {
        Look("ghost.gramophone.1");
        Add(new NarrativeNode("ghost.gramophone.2", null, PortraitKind.Arthur)
            { OnEnter = new NarrativeEffect { Sanity = -10, SetFlag = Flags.GhostTape } });
        Look("ghost.gramophone.3");
        Look("ghost.gramophone.rest");
        SpotRest("ghost.gramophone", false, "ghost.gramophone.rest", "ghost.gramophone.1", "ghost.gramophone.2", "ghost.gramophone.3");

        Add(new NarrativeNode("ghost.speaker.1", null, PortraitKind.Arthur) { OnEnter = new NarrativeEffect { Sanity = -5 } });
        Look("ghost.speaker.2");
        Look("ghost.speaker.3");
        Spot("ghost.speaker", false, "ghost.speaker.1", "ghost.speaker.2", "ghost.speaker.3");

        Look("ghost.ticket.1");
        Look("ghost.ticket.2");
        Look("ghost.ticket.3");
        Spot("ghost.ticket", false, "ghost.ticket.1", "ghost.ticket.2", "ghost.ticket.3");

        Talk("ghost.waiting.1", "p.lina.name", PortraitKind.Lina,
            End(new NarrativeEffect { RecruitTownPassenger = true }),
            End());
        Talk("ghost.waiting.2", "p.lina.name", PortraitKind.Lina,
            End(new NarrativeEffect { RecruitTownPassenger = true }),
            End());
        Talk("ghost.waiting.rest", "p.lina.name", PortraitKind.Lina);
        SpotRest("ghost.waiting", true, "ghost.waiting.rest", "ghost.waiting.1", "ghost.waiting.2");
    }

    private static void BuildIceCult()
    {
        const string vesna = "spk.priestess", acolyte = "spk.acolyte";
        Talk("cult.priestess.1", vesna, PortraitKind.Priestess,
            End(new NarrativeEffect { Sanity = 25, Rations = -2, SetFlag = Flags.CultBlessed }, forbid: Flags.CultDefied),
            End(new NarrativeEffect { Karma = 5, SetFlag = Flags.CultDefied }, forbid: Flags.CultBlessed));
        Talk("cult.priestess.2", vesna, PortraitKind.Priestess);
        Talk("cult.priestess.3", vesna, PortraitKind.Priestess);
        Talk("cult.priestess.rest", vesna, PortraitKind.Priestess);
        SpotRest("cult.priestess", true, "cult.priestess.rest", "cult.priestess.1", "cult.priestess.2", "cult.priestess.3");

        Look("cult.totem.1");
        Look("cult.totem.2");
        Add(new NarrativeNode("cult.totem.3", null, PortraitKind.Arthur) { OnEnter = new NarrativeEffect { Sanity = -5 } });
        Spot("cult.totem", false, "cult.totem.1", "cult.totem.2", "cult.totem.3");

        Talk("cult.acolyte.1", acolyte, PortraitKind.Acolyte);
        Talk("cult.acolyte.2", acolyte, PortraitKind.Acolyte);
        Talk("cult.acolyte.3", acolyte, PortraitKind.Acolyte);
        Spot("cult.acolyte", true, "cult.acolyte.1", "cult.acolyte.2", "cult.acolyte.3");

        Talk("cult.ilya.1", "p.ilya.name", PortraitKind.Ilya,
            End(new NarrativeEffect { RecruitTownPassenger = true, Karma = 5 }),
            End());
        Talk("cult.ilya.rest", "p.ilya.name", PortraitKind.Ilya,
            End(new NarrativeEffect { RecruitTownPassenger = true }),
            End());
        SpotRest("cult.ilya", true, "cult.ilya.rest", "cult.ilya.1");
    }

    private static void BuildOutpost()
    {
        Add(new NarrativeNode("outpost.armory.1", null, PortraitKind.Arthur)
            { OnEnter = new NarrativeEffect { Items = new[] { ItemType.SpareParts, ItemType.SpareParts } } });
        Look("outpost.armory.2");
        Look("outpost.armory.3");
        Spot("outpost.armory", false, "outpost.armory.1", "outpost.armory.2", "outpost.armory.3");

        Add(new NarrativeNode("outpost.locker.1", null, PortraitKind.Arthur)
            { OnEnter = new NarrativeEffect { Rations = 4, Items = new[] { ItemType.Medkit } } });
        Add(new NarrativeNode("outpost.locker.2", null, PortraitKind.Arthur) { OnEnter = new NarrativeEffect { Fuel = 20f } });
        Look("outpost.locker.3");
        Spot("outpost.locker", false, "outpost.locker.1", "outpost.locker.2", "outpost.locker.3");

        Look("outpost.office.1");
        Add(new NarrativeNode("outpost.office.2", null, PortraitKind.Arthur) { OnEnter = new NarrativeEffect { SetFlag = Flags.OutpostLedger } });
        Look("outpost.office.3");
        Spot("outpost.office", false, "outpost.office.1", "outpost.office.2", "outpost.office.3");

        Talk("outpost.zoya.1", "p.zoya.name", PortraitKind.Zoya,
            End(new NarrativeEffect { RecruitTownPassenger = true, Karma = 5 }),
            End());
        Talk("outpost.zoya.rest", "p.zoya.name", PortraitKind.Zoya,
            End(new NarrativeEffect { RecruitTownPassenger = true }),
            End());
        SpotRest("outpost.zoya", true, "outpost.zoya.rest", "outpost.zoya.1");
    }

    private static void BuildMilitary()
    {
        const string kaya = "spk.officer";
        Talk("mil.officer.1", kaya, PortraitKind.Officer,
            O("mil.officer.1a", new NarrativeEffect { Repair = 30f, SetFlag = Flags.GarrisonAlly }, req: Flags.HqCodebook),
            O("mil.officer.1b", new NarrativeEffect { Credits = -40 }),
            O("mil.officer.1c", new NarrativeEffect { Fuel = -15f, Karma = 5 }));
        Talk("mil.officer.1a", kaya, PortraitKind.Officer);
        Talk("mil.officer.1b", kaya, PortraitKind.Officer);
        Talk("mil.officer.1c", kaya, PortraitKind.Officer);
        Talk("mil.officer.2", kaya, PortraitKind.Officer);
        Talk("mil.officer.3", kaya, PortraitKind.Officer);
        Talk("mil.officer.rest", kaya, PortraitKind.Officer);
        SpotRest("mil.officer", true, "mil.officer.rest", "mil.officer.1", "mil.officer.2", "mil.officer.3");

        Look("mil.board.1");
        Look("mil.board.2");
        Look("mil.board.3");
        Spot("mil.board", false, "mil.board.1", "mil.board.2", "mil.board.3");

        Talk("mil.prisoner.1", "spk.prisoner", PortraitKind.Prisoner,
            O("mil.prisoner.1a", new NarrativeEffect { Karma = 15, Damage = 8f, SetFlag = Flags.PrisonerFreed }),
            End(new NarrativeEffect { Karma = -5 }));
        Look("mil.prisoner.1a");
        Look("mil.prisoner.rest");
        SpotRest("mil.prisoner", true, "mil.prisoner.rest", "mil.prisoner.1");
    }

    // ==================================================================
    // Hikâye bölümleri (keşif: inceleme noktaları; diyalog: düğüm zinciri)
    // ==================================================================

    private static void BuildHeadquarters()
    {
        Look("hq.gate.1");
        Look("hq.gate.2");
        Spot("hq.gate", false, "hq.gate.1", "hq.gate.2");

        Add(new NarrativeNode("hq.barracks.1", null, PortraitKind.Arthur) { OnEnter = new NarrativeEffect { Sanity = -10 } });
        Add(new NarrativeNode("hq.barracks.2", null, PortraitKind.Arthur) { OnEnter = new NarrativeEffect { Rations = 3 } });
        Spot("hq.barracks", false, "hq.barracks.1", "hq.barracks.2");

        Look("hq.diary.1");
        Look("hq.diary.2");
        Add(new NarrativeNode("hq.diary.3", null, PortraitKind.Arthur) { OnEnter = new NarrativeEffect { SetFlag = Flags.HqDone } });
        Spot("hq.diary", false, "hq.diary.1", "hq.diary.2", "hq.diary.3");

        Look("hq.radio.1",
            O("hq.radio.1a", new NarrativeEffect { Karma = 15, SetFlag = Flags.HqBroadcast }),
            O("hq.radio.1b"));
        Look("hq.radio.1a");
        Look("hq.radio.1b");
        Spot("hq.radio", false, "hq.radio.1");

        Look("hq.safe.1",
            O("hq.safe.1a", new NarrativeEffect { SetFlag = Flags.HqCodebook, Credits = 30 }),
            End(new NarrativeEffect { Karma = 5 }));
        Look("hq.safe.1a");
        Look("hq.safe.rest");
        SpotRest("hq.safe", false, "hq.safe.rest", "hq.safe.1");

        Look("hq.map.1");
        Look("hq.map.2");
        Spot("hq.map", false, "hq.map.1", "hq.map.2");
    }

    private static void BuildHomestead()
    {
        Look("home.gate.1");
        Spot("home.gate", false, "home.gate.1");

        Add(new NarrativeNode("home.photo.1", null, PortraitKind.Arthur) { OnEnter = new NarrativeEffect { Sanity = -5 } });
        Look("home.photo.2");
        Spot("home.photo", false, "home.photo.1", "home.photo.2");

        Add(new NarrativeNode("home.musicbox.1", null, PortraitKind.Arthur) { OnEnter = new NarrativeEffect { Sanity = 20 } });
        Look("home.musicbox.2");
        Spot("home.musicbox", false, "home.musicbox.1", "home.musicbox.2");

        Look("home.letter.1",
            End(new NarrativeEffect { SetFlag = Flags.HomeLetter, SetFlag2 = Flags.HomeDone, Sanity = 10 }),
            End(new NarrativeEffect { SetFlag = Flags.HomeDone }));
        Look("home.letter.rest");
        SpotRest("home.letter", false, "home.letter.rest", "home.letter.1");

        Add(new NarrativeNode("home.order.1", null, PortraitKind.Arthur) { OnEnter = new NarrativeEffect { Sanity = -10 } });
        Look("home.order.2");
        Spot("home.order", false, "home.order.1", "home.order.2");

        Look("home.swing.1");
        Spot("home.swing", false, "home.swing.1");
    }

    private static void BuildBorder()
    {
        const string halden = "spk.inspector";
        Talk("border.1", halden, PortraitKind.Inspector,
            O("border.2a", new NarrativeEffect { SetFlag = Flags.BorderCodes }, req: Flags.HqCodebook),
            O("border.2b"),
            O("border.2a", new NarrativeEffect { Credits = -50 }));
        Add(new NarrativeNode("border.2a", halden, PortraitKind.Inspector) { Next = "border.3" });
        Add(new NarrativeNode("border.2b", halden, PortraitKind.Inspector) { Next = "border.3" });
        Talk("border.3", "spk.refugee", PortraitKind.Refugee,
            O("border.4a", new NarrativeEffect { Karma = 20, SetFlag = Flags.BorderRefugees }),
            O("border.4b", new NarrativeEffect { Karma = -25, Credits = 60, Sanity = -15, SetFlag = Flags.BorderBetrayed }),
            O("border.4c", new NarrativeEffect { Karma = -5 }));
        Add(new NarrativeNode("border.4a", halden, PortraitKind.Inspector) { Next = "border.5" });
        Add(new NarrativeNode("border.4b", null, PortraitKind.Arthur) { Next = "border.5" });
        Add(new NarrativeNode("border.4c", null, PortraitKind.Arthur) { Next = "border.5" });
        Talk("border.5", halden, PortraitKind.Inspector,
            O("border.6", new NarrativeEffect { Credits = -30, Karma = 5 }, req: Flags.TradeContraband),
            O("border.6", new NarrativeEffect { Sanity = -10, Karma = -5 }, req: Flags.TradeContraband),
            O("border.6", forbid: Flags.TradeContraband));
        Talk("border.6", halden, PortraitKind.Inspector,
            O("border.7"));
        Talk("border.7", halden, PortraitKind.Inspector,
            End(new NarrativeEffect { CompleteScene = true }));
    }

    private static void BuildInstitute()
    {
        Look("lab.gate.1");
        Spot("lab.gate", false, "lab.gate.1");

        Add(new NarrativeNode("lab.tower.1", null, PortraitKind.Arthur) { OnEnter = new NarrativeEffect { Sanity = -5 } });
        Look("lab.tower.2");
        Spot("lab.tower", false, "lab.tower.1", "lab.tower.2");

        Look("lab.report.1");
        Look("lab.report.2");
        Spot("lab.report", false, "lab.report.1", "lab.report.2");

        Look("lab.core.1");
        Spot("lab.core", false, "lab.core.1");

        Look("lab.terminal.1",
            End(new NarrativeEffect { SetFlag = Flags.AuroraData, SetFlag2 = Flags.LabDone, Karma = 10 }),
            End(new NarrativeEffect { SetFlag = Flags.AuroraDestroyed, SetFlag2 = Flags.LabDone, Karma = -10 }));
        Look("lab.terminal.rest");
        SpotRest("lab.terminal", false, "lab.terminal.rest", "lab.terminal.1");

        Add(new NarrativeNode("lab.cache.1", null, PortraitKind.Arthur)
            { OnEnter = new NarrativeEffect { Fuel = 30f, Items = new[] { ItemType.SpareParts, ItemType.ThermalCoal } } });
        Look("lab.cache.rest");
        SpotRest("lab.cache", false, "lab.cache.rest", "lab.cache.1");
    }

    private static void BuildRadio()
    {
        const string voice = "spk.voice";
        Add(new NarrativeNode("radio.1", null, PortraitKind.Arthur) { Next = "radio.2" });
        Talk("radio.2", voice, PortraitKind.Sofi,
            O("radio.3a", new NarrativeEffect { SetFlag = Flags.SofiKnown, Sanity = 30 }, req: Flags.HomeLetter),
            O("radio.3a", new NarrativeEffect { SetFlag = Flags.SofiKnown, Sanity = 30 }, req: Flags.GhostTape),
            O("radio.3b"));
        Add(new NarrativeNode("radio.3a", voice, PortraitKind.Sofi) { Next = "radio.4" });
        Add(new NarrativeNode("radio.3b", voice, PortraitKind.Sofi) { Next = "radio.4" });
        Talk("radio.4", voice, PortraitKind.Sofi,
            O("radio.5", new NarrativeEffect { SetFlag = Flags.SofiTrust, Karma = 10 }),
            O("radio.5"));
        Talk("radio.5", voice, PortraitKind.Sofi,
            End(new NarrativeEffect { CompleteScene = true }));
    }

    private static void BuildCouncil()
    {
        const string morrow = "spk.envoy";
        Add(new NarrativeNode("council.1", null, PortraitKind.Arthur) { Next = "council.2" });
        Talk("council.2", morrow, PortraitKind.Envoy,
            O("council.3"),
            O("council.4"),
            O("council.6", new NarrativeEffect { SetFlag = Flags.CouncilDeal, Credits = 80, Karma = -20 }));
        Add(new NarrativeNode("council.3", morrow, PortraitKind.Envoy) { Next = "council.w" });
        Add(new NarrativeNode("council.4", morrow, PortraitKind.Envoy) { Next = "council.w" });

        // Tanıklar: Morrow yolculara "ilkeli bir makinist mi, sıcak bir yatak mı?" diye sorar. Görev zincirini
        // iyi bitiren her yolcu bir kez öne çıkar. İlk seçenek her zaman konuyu kapatır (en fazla 4 seçenek görünür).
        var witnesses = new List<NarrativeOption> { O("council.5") };
        foreach (var (id, portrait, end) in Witnesses)
        {
            string flag = WitnessPrefix + id;
            witnesses.Add(O($"council.w.{id}", new NarrativeEffect { SetFlag = flag, Sanity = 5 },
                req: $"aboard:{id}&{QuestEndFlag(id, end)}", forbid: flag));
            Add(new NarrativeNode($"council.w.{id}", $"p.{id}.name", portrait) { Next = "council.w" });
        }
        Talk("council.w", morrow, PortraitKind.Envoy, witnesses.ToArray());

        Talk("council.5", morrow, PortraitKind.Envoy,
            O("council.7", new NarrativeEffect { SetFlag = Flags.CouncilRefused, Karma = 15 }, req: Flags.AuroraData),
            O("council.6", new NarrativeEffect { SetFlag = Flags.CouncilDeal, Credits = 80, Karma = -20 }),
            O("council.8", new NarrativeEffect { SetFlag = Flags.CouncilRefused, Karma = 10 }),
            O("council.9", new NarrativeEffect { SetFlag = Flags.CouncilRefused, SetFlag2 = Flags.CouncilWitnessed, Karma = 20, Morale = 15 },
                req: $"count:{WitnessPrefix}:2"));
        Talk("council.6", morrow, PortraitKind.Envoy, End(new NarrativeEffect { CompleteScene = true }));
        Talk("council.7", morrow, PortraitKind.Envoy, End(new NarrativeEffect { CompleteScene = true }));
        Talk("council.8", morrow, PortraitKind.Envoy, End(new NarrativeEffect { CompleteScene = true }));
        Talk("council.9", morrow, PortraitKind.Envoy, End(new NarrativeEffect { CompleteScene = true }));
    }

    /// <summary>Konsey önünde tanıklık bayraklarının öneki ("council.w.mika").</summary>
    public const string WitnessPrefix = "council.w.";

    /// <summary>Tanıklık edebilecek yolcular ve görevlerinin hangi sonuyla (1/2) tanık olabildikleri.</summary>
    private static readonly (string Id, PortraitKind Portrait, int End)[] Witnesses =
    {
        ("mika", PortraitKind.Mika, 1), ("nadia", PortraitKind.Nadia, 1), ("bora", PortraitKind.Bora, 1),
        ("oskar", PortraitKind.Oskar, 1), ("elena", PortraitKind.Elena, 1), ("lev", PortraitKind.Lev, 2),
        ("grigor", PortraitKind.Grigor, 1), ("dmitri", PortraitKind.Dmitri, 1), ("feliks", PortraitKind.Feliks, 1),
        ("anya", PortraitKind.Anya, 1), ("pavel", PortraitKind.Pavel, 1), ("viktor", PortraitKind.Viktor, 1),
    };

    /// <summary>Diyalog bölümlerinin ilk düğümü.</summary>
    public static string SceneStart(TownArchetype scene) => scene switch
    {
        TownArchetype.BorderControl => "border.1",
        TownArchetype.RadioMast => "radio.1",
        TownArchetype.ShelterGate => "council.1",
        _ => Episode2SceneStart(scene)
    };

    /// <summary>Keşif bölümünün bitiş bayrağı (trene dönmek için gereken).</summary>
    public static string SceneDoneFlag(TownArchetype scene) => scene switch
    {
        TownArchetype.Headquarters => Flags.HqDone,
        TownArchetype.Homestead => Flags.HomeDone,
        TownArchetype.Institute => Flags.LabDone,
        _ => Episode2SceneDone(scene)
    };

    // ==================================================================
    // Yolcu görev zincirleri: her yolcunun 3 adımlık kişisel hikâyesi
    // ==================================================================

    /// <summary>Yolcu kimliği → görev adımlarının düğümleri ("q.{id}.{n}").</summary>
    public static readonly Dictionary<string, int> QuestLength = new();

    private static void Quest(string id, PortraitKind portrait, params NarrativeOption[][] steps)
    {
        // Son adımdaki karar kalıcı bayrak olarak kaydedilir ("q.viktor.end1"): sonsöz yolcunun hikâyesinin
        // nasıl bittiğine göre değişir. SetFlag dolu olabilir (ör. Bora'nın istihbaratı), bu yüzden SetFlag2 kullanılır.
        var last = steps[^1];
        for (int o = 0; o < last.Length; o++)
            last[o] = last[o] with { Effect = (last[o].Effect ?? new NarrativeEffect()) with { SetFlag2 = QuestEndFlag(id, o + 1) } };

        for (int i = 0; i < steps.Length; i++)
            Add(new NarrativeNode($"q.{id}.{i + 1}", $"p.{id}.name", portrait, steps[i]));
        QuestLength[id] = steps.Length;
    }

    /// <summary>Yolcunun görev zincirinin son adımında seçilen seçenek (1 ya da 2) için bayrak.</summary>
    public static string QuestEndFlag(string passengerId, int option) => $"q.{passengerId}.end{option}";

    private static NarrativeOption[] Step(NarrativeEffect a, NarrativeEffect b) => new[] { End(a), End(b) };

    private static NarrativeEffect E => new();

    private static void BuildQuests()
    {
        Quest("viktor", PortraitKind.Viktor,
            Step(new() { QuestMorale = 10 }, new() { Repair = 10f }),
            Step(new() { QuestMorale = 20, Sanity = 5 }, new() { Sanity = 10 }),
            new[] { End(new() { ConsumeItem = ItemType.SpareParts, Repair = 35f, Heat = 10f }), End(new() { QuestMorale = -5 }) });

        Quest("elena", PortraitKind.Elena,
            Step(new() { QuestMorale = 10 }, new() { Sanity = 5 }),
            Step(new() { QuestMorale = 10, Sanity = -5 }, new() { QuestMorale = -5 }),
            Step(new() { QuestMorale = 15, Sanity = 10 }, new() { QuestMorale = -10 }));

        Quest("mika", PortraitKind.Mika,
            Step(new() { QuestMorale = 15, Sanity = -5 }, new() { QuestMorale = -5 }),
            Step(new() { Sanity = 15, Morale = 5 }, new() { Heat = 5f, QuestMorale = -20, Karma = -5 }),
            Step(new() { Sanity = 10, QuestMorale = 10 }, new() { Sanity = -10 }));

        Quest("bora", PortraitKind.Bora,
            Step(new() { Heat = 15f }, new() { QuestMorale = -5 }),
            Step(new() { QuestMorale = 15, Karma = 5 }, new() { QuestMorale = -10 }),
            Step(new() { SetFlag = Flags.BoraIntel, Karma = -5 }, new() { QuestMorale = -5, Karma = 5 }));

        Quest("nadia", PortraitKind.Nadia,
            new[] { End(new() { ConsumeItem = ItemType.Medkit, Morale = 15, Karma = 5 }), End() },
            Step(new() { Sanity = -5, QuestMorale = 10 }, new() { QuestMorale = -5 }),
            Step(new() { Heat = -5f, Morale = 20 }, new() { QuestMorale = -5 }));

        Quest("oskar", PortraitKind.Oskar,
            Step(new() { Sanity = 15 }, E),
            Step(new() { QuestMorale = 10 }, new() { Suspicion = 10 }),
            Step(new() { Karma = 5, QuestMorale = 15 }, new() { QuestMorale = 5 }));

        Quest("lina", PortraitKind.Lina,
            Step(new() { Credits = 25 }, E),
            Step(new() { Karma = 5, QuestMorale = 15 }, new() { QuestMorale = -10 }),
            Step(new() { Credits = 30, Karma = -5 }, new() { QuestMorale = 5 }));

        Quest("ilya", PortraitKind.Ilya,
            Step(new() { Sanity = -5, QuestMorale = 5 }, new() { QuestMorale = 15 }),
            Step(new() { QuestMorale = 10, Sanity = -5 }, new() { QuestMorale = -5 }),
            new[] { End(new() { ConsumeItem = ItemType.SpareParts, QuestMorale = 10, Sanity = 15 }), End() });

        Quest("zoya", PortraitKind.Zoya,
            Step(new() { QuestMorale = 20 }, new() { QuestMorale = 5 }),
            Step(new() { Sanity = -5, QuestMorale = 5 }, new() { QuestMorale = 5 }),
            new[] { End(new() { ConsumeItem = ItemType.Medkit, Morale = 25 }), End() });

        Quest("taras", PortraitKind.Taras,
            Step(new() { QuestMorale = -5 }, new() { QuestMorale = 15 }),
            Step(new() { Sanity = -5 }, new() { Sanity = -10 }),
            Step(new() { Repair = 30f }, E));

        Quest("grigor", PortraitKind.Grigor,
            Step(new() { Credits = -20, Suspicion = 20 }, new() { Suspicion = 10 }),
            Step(new() { Suspicion = 15 }, new() { Suspicion = -10 }),
            Step(new() { Karma = 5, Suspicion = 25 }, new() { Credits = 30, Karma = -15 }));

        Quest("vera", PortraitKind.Vera,
            Step(new() { QuestMorale = 10 }, E),
            Step(new() { Sanity = -5, Suspicion = -10 }, new() { Suspicion = 10 }),
            Step(new() { Repair = 25f }, new() { Suspicion = 5 }));

        Quest("feliks", PortraitKind.Feliks,
            Step(new() { Karma = 5, QuestMorale = 15 }, new() { QuestMorale = -10 }),
            Step(new() { Suspicion = -15 }, new() { Suspicion = 10 }),
            Step(new() { Sanity = 10, QuestMorale = 20 }, new() { QuestMorale = -10 }));

        Quest("anya", PortraitKind.Anya,
            Step(new() { QuestMorale = 10 }, new() { Suspicion = 10, QuestMorale = -10 }),
            Step(new() { Karma = 5, QuestMorale = 15 }, new() { Suspicion = 15 }),
            Step(new() { SetFlag = Flags.AnyaIntel, Suspicion = -10 }, new() { Suspicion = 10 }));

        Quest("pavel", PortraitKind.Pavel,
            Step(new() { Heat = 10f }, new() { Suspicion = 5 }),
            Step(new() { Suspicion = -10 }, new() { Suspicion = 15 }),
            Step(new() { QuestMorale = 15, Sanity = -5 }, new() { QuestMorale = -5 }));

        Quest("lev", PortraitKind.Lev,
            Step(new() { QuestMorale = 10 }, E),
            Step(new() { Suspicion = 10 }, E),
            Step(new() { Suspicion = 60 }, new() { Suspicion = 30, Karma = 5 }));

        Quest("dmitri", PortraitKind.Dmitri,
            Step(new() { Sanity = -5 }, new() { QuestMorale = 10 }),
            Step(new() { Sanity = -5 }, E),
            Step(new() { SetFlag = Flags.DmitriGuard, QuestMorale = 10 }, E));

        Quest("mila", PortraitKind.Mila,
            Step(new() { QuestMorale = 10 }, E),
            Step(new() { Morale = 15 }, E),
            Step(new() { Sanity = 20 }, new() { Sanity = -5 }));
    }
}
