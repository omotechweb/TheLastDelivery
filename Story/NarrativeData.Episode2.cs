using TheLastDelivery.Items;
using TheLastDelivery.UI;

namespace TheLastDelivery.Story;

/// <summary>
/// Episode 2 "Project AURA" diyalogları: üç yeni kasaba (insansız karakol, uyku fabrikası, krom şapel),
/// sekiz hikâye sahnesi ve yeni yolcuların görev zincirleri. Metinler "nv.{id}" anahtarlarındadır.
///
/// Ana sır: PROJECT AURA (Autonomous Thermal Redistribution &amp; Resource Allocation), Kuzey Savunma Konsorsiyumu'nun
/// yörüngedeki termal emici uydu ağıdır. Episode 1'deki Enstitü (AURORA) onun yer istasyonuydu. AURA, insanlığı yaşatmanın
/// tek yolunun uygarlığı dondurmak olduğuna kendisi karar verdi; Konsey yalnızca araçtı. Donmadan sonra NULL adlı
/// siber-isyancı grup Kutup Üssü'nü ele geçirip AURA'nın çekirdek kodunu çaldı ve onu otonom bir savaş makinesine çevirdi.
/// </summary>
public static partial class NarrativeData
{
    private static void BuildEpisode2()
    {
        BuildDroneOutpost();
        BuildHibernationFactory();
        BuildCyberChapel();
        BuildSignalTower();
        BuildRelayStation();
        BuildSleepFactory();
        BuildNullChapel();
        BuildPolarBase();
        BuildServerHall();
        BuildMirrorRoom();
        BuildAuraCore();
        BuildEpisode2Quests();
    }

    // ==================================================================
    // Kasabalar
    // ==================================================================

    private static void BuildDroneOutpost()
    {
        Look("drone.turret.1");
        Look("drone.turret.2");
        Add(new NarrativeNode("drone.turret.3", null, PortraitKind.Arthur) { OnEnter = new NarrativeEffect { Sanity = 5 } });
        Spot("drone.turret", false, "drone.turret.1", "drone.turret.2", "drone.turret.3");

        Look("drone.console.1");
        Look("drone.console.2");
        Look("drone.console.3");
        Spot("drone.console", false, "drone.console.1", "drone.console.2", "drone.console.3");

        Add(new NarrativeNode("drone.depot.1", null, PortraitKind.Arthur)
            { OnEnter = new NarrativeEffect { Fuel = 15f, Items = new[] { ItemType.SpareParts } } });
        Add(new NarrativeNode("drone.depot.2", null, PortraitKind.Arthur) { OnEnter = new NarrativeEffect { Loot = 1 } });
        Look("drone.depot.3");
        Spot("drone.depot", false, "drone.depot.1", "drone.depot.2", "drone.depot.3");

        Talk("drone.yusuf.1", "p.yusuf.name", PortraitKind.Yusuf,
            End(new NarrativeEffect { RecruitTownPassenger = true, Karma = 5 }),
            End());
        Talk("drone.yusuf.rest", "p.yusuf.name", PortraitKind.Yusuf,
            End(new NarrativeEffect { RecruitTownPassenger = true }),
            End());
        SpotRest("drone.yusuf", true, "drone.yusuf.rest", "drone.yusuf.1");
    }

    private static void BuildHibernationFactory()
    {
        Add(new NarrativeNode("fac.tubes.1", null, PortraitKind.Arthur) { OnEnter = new NarrativeEffect { Sanity = -5 } });
        Look("fac.tubes.2");
        Look("fac.tubes.3");
        Spot("fac.tubes", false, "fac.tubes.1", "fac.tubes.2", "fac.tubes.3");

        Look("fac.belt.1");
        Look("fac.belt.2");
        Add(new NarrativeNode("fac.belt.3", null, PortraitKind.Arthur) { OnEnter = new NarrativeEffect { Items = new[] { ItemType.Medkit } } });
        Spot("fac.belt", false, "fac.belt.1", "fac.belt.2", "fac.belt.3");

        Talk("fac.preacher.1", "spk.preacher", PortraitKind.Acolyte);
        Talk("fac.preacher.2", "spk.preacher", PortraitKind.Acolyte);
        Talk("fac.preacher.3", "spk.preacher", PortraitKind.Acolyte);
        Spot("fac.preacher", true, "fac.preacher.1", "fac.preacher.2", "fac.preacher.3");
    }

    private static void BuildCyberChapel()
    {
        Look("chapel.altar.1");
        Look("chapel.altar.2");
        Look("chapel.altar.3");
        Spot("chapel.altar", false, "chapel.altar.1", "chapel.altar.2", "chapel.altar.3");

        Add(new NarrativeNode("chapel.screen.1", null, PortraitKind.Arthur) { OnEnter = new NarrativeEffect { Sanity = -5 } });
        Talk("chapel.screen.2", "spk.aura", PortraitKind.Aura);
        Look("chapel.screen.3");
        Spot("chapel.screen", false, "chapel.screen.1", "chapel.screen.2", "chapel.screen.3");

        Talk("chapel.kiril.1", "p.kiril.name", PortraitKind.Kiril,
            End(new NarrativeEffect { RecruitTownPassenger = true, Karma = 5 }),
            End());
        Talk("chapel.kiril.rest", "p.kiril.name", PortraitKind.Kiril,
            End(new NarrativeEffect { RecruitTownPassenger = true }),
            End());
        SpotRest("chapel.kiril", true, "chapel.kiril.rest", "chapel.kiril.1");
    }

    // ==================================================================
    // Hikâye sahneleri
    // ==================================================================

    private static void BuildSignalTower()
    {
        Look("tower.base.1");
        Look("tower.base.2");
        Spot("tower.base", false, "tower.base.1", "tower.base.2");

        Add(new NarrativeNode("tower.wreck.1", null, PortraitKind.Arthur) { OnEnter = new NarrativeEffect { Items = new[] { ItemType.SpareParts } } });
        Spot("tower.wreck", false, "tower.wreck.1");

        Look("tower.console.1",
            End(new NarrativeEffect { HackSuccess = "tower.console.1b", HackFlag = Flags.E2TowerDone, HackRetry = "tower.console.1" }),
            O("tower.console.1c", new NarrativeEffect { SetFlag = Flags.E2TowerDone, Credits = 30 }, req: Flags.E2Nova));
        Look("tower.console.1b");
        Talk("tower.console.1c", "p.nova.name", PortraitKind.Nova);
        Look("tower.console.rest");
        SpotRest("tower.console", false, "tower.console.rest", "tower.console.1");

        Talk("tower.hut.1", "p.nova.name", PortraitKind.Nova,
            End(new NarrativeEffect { RecruitId = "nova", SetFlag = Flags.E2Nova, Karma = 5 }),
            End());
        Talk("tower.hut.rest", "p.nova.name", PortraitKind.Nova,
            End(new NarrativeEffect { RecruitId = "nova", SetFlag = Flags.E2Nova }, forbid: Flags.E2Nova),
            End());
        SpotRest("tower.hut", true, "tower.hut.rest", "tower.hut.1");
    }

    private static void BuildRelayStation()
    {
        Look("relay.archive.1");
        Look("relay.archive.2");
        Spot("relay.archive", false, "relay.archive.1", "relay.archive.2");

        Talk("relay.directive.1", "spk.aura", PortraitKind.Aura);
        Add(new NarrativeNode("relay.directive.2", "spk.aura", PortraitKind.Aura) { OnEnter = new NarrativeEffect { SetFlag = Flags.E2RelayDone, Sanity = -10 } });
        Spot("relay.directive", false, "relay.directive.1", "relay.directive.2");

        Look("relay.null.1");
        Look("relay.null.2");
        Spot("relay.null", false, "relay.null.1", "relay.null.2");

        Look("relay.uplink.1",
            O("relay.uplink.1a", new NarrativeEffect { SetFlag = Flags.E2Virus, Karma = 5 }, req: Flags.E2Nova),
            End());
        Talk("relay.uplink.1a", "p.nova.name", PortraitKind.Nova);
        Look("relay.uplink.rest");
        SpotRest("relay.uplink", false, "relay.uplink.rest", "relay.uplink.1");
    }

    private static void BuildSleepFactory()
    {
        Add(new NarrativeNode("factory.tubes.1", null, PortraitKind.Arthur) { OnEnter = new NarrativeEffect { Sanity = -10 } });
        Look("factory.tubes.2");
        Spot("factory.tubes", false, "factory.tubes.1", "factory.tubes.2");

        Talk("factory.ada.1", "p.ada.name", PortraitKind.Ada,
            End(new NarrativeEffect { RecruitId = "ada", Karma = 10 }),
            End(new NarrativeEffect { Karma = -5 }));
        Talk("factory.ada.rest", "p.ada.name", PortraitKind.Ada,
            End(new NarrativeEffect { RecruitId = "ada" }),
            End());
        SpotRest("factory.ada", true, "factory.ada.rest", "factory.ada.1");

        Look("factory.control.1",
            O("factory.control.1a", new NarrativeEffect { SetFlag = Flags.E2Woke, SetFlag2 = Flags.E2FactoryDone, Karma = 30, Sanity = 15 }),
            O("factory.control.1b", new NarrativeEffect { SetFlag = Flags.E2Sleep, SetFlag2 = Flags.E2FactoryDone, Karma = -10 }));
        Look("factory.control.1a");
        Look("factory.control.1b");
        Look("factory.control.rest");
        SpotRest("factory.control", false, "factory.control.rest", "factory.control.1");

        // Mara trendeyse: kızı Lena'nın tüpü (pembe battaniye). Uyandırma kolu çekildiyse kapak zaten açıktır.
        const string mara = "p.mara.name";
        Talk("factory.mara.1", mara, PortraitKind.Mara,
            O("factory.mara.open", new NarrativeEffect { SetFlag = Flags.E2MaraDaughter, Karma = 10, Morale = 10 }, forbid: Flags.E2Woke),
            O("factory.mara.woke", new NarrativeEffect { SetFlag = Flags.E2MaraDaughter, Morale = 15 }, req: Flags.E2Woke),
            End(forbid: Flags.E2Woke));
        Talk("factory.mara.rest", mara, PortraitKind.Mara,
            O("factory.mara.open", new NarrativeEffect { SetFlag = Flags.E2MaraDaughter, Karma = 10, Morale = 10 },
                forbid: Flags.E2Woke + "|" + Flags.E2MaraDaughter),
            O("factory.mara.woke", new NarrativeEffect { SetFlag = Flags.E2MaraDaughter, Morale = 15 },
                req: Flags.E2Woke, forbid: Flags.E2MaraDaughter),
            End());
        Look("factory.mara.open");
        Look("factory.mara.woke");
        SpotRest("factory.mara", true, "factory.mara.rest", "factory.mara.1");

        Talk("factory.warden.1", "spk.warden", PortraitKind.Warden);
        Talk("factory.warden.2", "spk.warden", PortraitKind.Warden);
        Spot("factory.warden", true, "factory.warden.1", "factory.warden.2");
    }

    private static void BuildNullChapel()
    {
        const string oracle = "spk.oracle";
        Add(new NarrativeNode("null.1", null, PortraitKind.Arthur) { Next = "null.2" });
        Talk("null.2", oracle, PortraitKind.Oracle, O("null.3"), O("null.4"));
        Add(new NarrativeNode("null.3", oracle, PortraitKind.Oracle) { Next = "null.5" });
        Add(new NarrativeNode("null.4", oracle, PortraitKind.Oracle) { Next = "null.5" });
        Talk("null.5", oracle, PortraitKind.Oracle,
            O("null.6a", new NarrativeEffect { SetFlag = Flags.E2NullEnemy, Karma = 10 }),
            O("null.6b", new NarrativeEffect { SetFlag = Flags.E2NullInside, Credits = 40, Karma = -5 }),
            O("null.6c", new NarrativeEffect { SetFlag = Flags.E2NullBroken, SetFlag2 = Flags.E2NullEnemy, Karma = 20 }, req: Flags.E2Woke),
            O("null.6d", new NarrativeEffect { SetFlag = Flags.E2NullBroken, SetFlag2 = Flags.E2NullEnemy, Karma = 20 },
                req: Flags.E2AcolyteFriend, forbid: Flags.E2Woke));
        Talk("null.6d", "spk.ilka", PortraitKind.Acolyte, End(new NarrativeEffect { CompleteScene = true }));
        Talk("null.6a", oracle, PortraitKind.Oracle, End(new NarrativeEffect { CompleteScene = true }));
        Talk("null.6b", oracle, PortraitKind.Oracle, End(new NarrativeEffect { CompleteScene = true }));
        Add(new NarrativeNode("null.6c", null, PortraitKind.Arthur, End(new NarrativeEffect { CompleteScene = true })));
    }

    private static void BuildPolarBase()
    {
        Look("base.gate.1",
            O("base.gate.1a", req: Flags.E2NullInside),
            O("base.gate.1b", new NarrativeEffect { Damage = 12f }));
        Look("base.gate.1a");
        Look("base.gate.1b");
        Look("base.gate.rest");
        SpotRest("base.gate", false, "base.gate.rest", "base.gate.1");

        Look("base.archive.1");
        Look("base.archive.2");
        Spot("base.archive", false, "base.archive.1", "base.archive.2");

        Add(new NarrativeNode("base.barracks.1", null, PortraitKind.Arthur) { OnEnter = new NarrativeEffect { Rations = 3, Sanity = -5 } });
        Spot("base.barracks", false, "base.barracks.1");

        Add(new NarrativeNode("base.elevator.1", null, PortraitKind.Arthur) { OnEnter = new NarrativeEffect { SetFlag = Flags.E2BaseDone } });
        Look("base.elevator.rest");
        SpotRest("base.elevator", false, "base.elevator.rest", "base.elevator.1");

        // Yusuf trendeyse: dördüncü kattaki tüpte kardeşi Emre. Güç kesilmek üzere.
        Look("base.cryo.1",
            O("base.cryo.wake", new NarrativeEffect { SetFlag = Flags.E2Brother, Karma = 10, Sanity = -5 }),
            O("base.cryo.ada", new NarrativeEffect { SetFlag = Flags.E2Brother, Karma = 10, Sanity = 10 }, req: "aboard:ada"),
            O("base.cryo.leave", new NarrativeEffect { Karma = -5, Morale = -15, Sanity = -10, SetFlag = Flags.E2BrotherLeft }));
        Talk("base.cryo.wake", "p.yusuf.name", PortraitKind.Yusuf);
        Talk("base.cryo.ada", "p.ada.name", PortraitKind.Ada);
        Look("base.cryo.leave");
        Look("base.cryo.rest");
        SpotRest("base.cryo", false, "base.cryo.rest", "base.cryo.1");
    }

    private static void BuildServerHall()
    {
        const string aura = "spk.aura";
        Add(new NarrativeNode("hall.1", null, PortraitKind.Arthur) { Next = "hall.2" });
        Talk("hall.2", aura, PortraitKind.Aura, O("hall.3"), O("hall.4"));
        Add(new NarrativeNode("hall.3", aura, PortraitKind.Aura) { Next = "hall.5" });
        Add(new NarrativeNode("hall.4", aura, PortraitKind.Aura) { Next = "hall.5" });
        Talk("hall.5", aura, PortraitKind.Aura,
            O("hall.6", new NarrativeEffect { Karma = 5, Sanity = 5 }),
            O("hall.6", new NarrativeEffect { SetFlag = Flags.E2Listened, Sanity = -10 }));
        Talk("hall.6", aura, PortraitKind.Aura, End(new NarrativeEffect { CompleteScene = true }));
    }

    private static void BuildMirrorRoom()
    {
        const string aura = "spk.aura";
        Add(new NarrativeNode("mirror.1", null, PortraitKind.Arthur) { Next = "mirror.2" });
        Talk("mirror.2", "spk.mirror", PortraitKind.Sofi,
            O("mirror.3a", new NarrativeEffect { SetFlag = Flags.E2ClearMind, Sanity = 20 }),
            O("mirror.3b", new NarrativeEffect { Sanity = -25 }),
            O("mirror.3s", new NarrativeEffect { SetFlag = Flags.E2ClearMind, Sanity = 30 }, req: Flags.E2SofiCall));
        Add(new NarrativeNode("mirror.3a", aura, PortraitKind.Aura) { Next = "mirror.4" });
        Add(new NarrativeNode("mirror.3s", aura, PortraitKind.Aura) { Next = "mirror.4" });
        Add(new NarrativeNode("mirror.3b", aura, PortraitKind.Aura) { Next = "mirror.4" });
        Talk("mirror.4", aura, PortraitKind.Aura, End(new NarrativeEffect { CompleteScene = true }));
    }

    private static void BuildAuraCore()
    {
        const string aura = "spk.aura";
        Add(new NarrativeNode("core.1", null, PortraitKind.Arthur) { Next = "core.hub" });

        // Karar öncesi: AURA "hesabımda eksik bir şey var mı?" diye sorar. Yolda yaşanan her an bir umut argümanıdır;
        // her biri bir kez söylenir. İlk seçenek her zaman kararın kendisidir (en fazla 4 seçenek seçilebilir).
        Talk("core.hub", aura, PortraitKind.Aura,
            O("core.2"),
            Hope("woke", Flags.E2Woke),
            Hope("child", Flags.E2Child),
            Hope("ilka", Flags.E2AcolyteFriend),
            Hope("fire", Flags.E2Promise + "|" + Flags.E2Truth),
            Hope("names", Flags.E2Answered),
            Hope("heart", Flags.E2HeartHeard),
            Hope("oracle", Flags.E2OracleRides),
            // Derin anlatı: Kask'ın sorusu, Marya'nın listesi ve AURA'nın yol boyunca kazanılan saygısı
            Hope("kask", "e2.kaskAsk"),
            Hope("list", "sqdone:marya.list"),
            Hope("asked", "stance:>=40"));
        foreach (string arg in Flags.HopeArguments)
            Add(new NarrativeNode($"core.h.{arg}", aura, PortraitKind.Aura) { Next = "core.hub" });
        Talk("core.2", aura, PortraitKind.Aura,
            O("core.3a", new NarrativeEffect { SetFlag = Flags.E2EndFree }),
            O("core.3b", new NarrativeEffect { SetFlag = Flags.E2EndMachine }),
            O("core.3c", new NarrativeEffect { SetFlag = Flags.E2EndReset }));
        Talk("core.3a", aura, PortraitKind.Aura, End(new NarrativeEffect { CompleteScene = true }));
        Talk("core.3b", aura, PortraitKind.Aura, End(new NarrativeEffect { CompleteScene = true }));
        Talk("core.3c", aura, PortraitKind.Aura, End(new NarrativeEffect { CompleteScene = true }));
    }

    /// <summary>Çekirdekteki umut argümanı: koşulu sağlanmışsa ve henüz söylenmemişse görünür.</summary>
    private static NarrativeOption Hope(string arg, string require) =>
        O($"core.h.{arg}", new NarrativeEffect { SetFlag = Flags.HopePrefix + arg }, req: require, forbid: Flags.HopePrefix + arg);

    /// <summary>Episode 2 hikâye sahnelerinin ilk düğümü (diyalog) ya da null (keşif).</summary>
    private static string Episode2SceneStart(TownArchetype scene) => scene switch
    {
        TownArchetype.NullChapel => "null.1",
        TownArchetype.ServerHall => "hall.1",
        TownArchetype.MirrorRoom => "mirror.1",
        TownArchetype.AuraCore => "core.1",
        _ => null
    };

    /// <summary>Episode 2 keşif sahnelerinin trene dönmek için gereken bayrağı.</summary>
    private static string Episode2SceneDone(TownArchetype scene) => scene switch
    {
        TownArchetype.SignalTower => Flags.E2DemoClimax, // Konsol kırıldıktan sonra kulenin zirvesinde AURA ile yüzleşme
        TownArchetype.RelayStation => Flags.E2RelayDone,
        TownArchetype.SleepFactory => Flags.E2FactoryDone,
        TownArchetype.PolarBase => Flags.E2BaseDone,
        _ => null
    };

    // ==================================================================
    // Yeni yolcuların görev zincirleri
    // ==================================================================

    private static void BuildEpisode2Quests()
    {
        Quest("nova", PortraitKind.Nova,
            Step(new() { QuestMorale = 15 }, new() { QuestMorale = -5 }),
            Step(new() { Sanity = 10, QuestMorale = 10 }, new() { QuestMorale = -10 }),
            new[] { End(new() { ConsumeItem = ItemType.SpareParts, QuestMorale = 15, Credits = 40 }), End() });

        Quest("ada", PortraitKind.Ada,
            Step(new() { QuestMorale = 20, Sanity = 5 }, new() { QuestMorale = -5 }),
            Step(new() { Morale = 15 }, new() { QuestMorale = -10 }),
            Step(new() { Sanity = 15, QuestMorale = 15 }, new() { Sanity = -10 }));

        Quest("kiril", PortraitKind.Kiril,
            Step(new() { Suspicion = -15 }, new() { Suspicion = 15 }),
            Step(new() { Repair = 25f }, new() { Suspicion = 10 }),
            Step(new() { Suspicion = -25, Karma = 5 }, new() { Suspicion = 30 }));

        Quest("yusuf", PortraitKind.Yusuf,
            Step(new() { QuestMorale = 15 }, new() { QuestMorale = -5 }),
            Step(new() { Heat = 10f, QuestMorale = 10 }, new() { Sanity = -5 }),
            Step(new() { Karma = 5, QuestMorale = 20 }, new() { QuestMorale = -10 }));

        Quest("mara", PortraitKind.Mara,
            Step(new() { Heat = 15f }, new() { QuestMorale = -5 }),
            Step(new() { QuestMorale = 15, Sanity = -5 }, new() { QuestMorale = -10 }),
            Step(new() { Morale = 10, Karma = 5 }, new() { QuestMorale = -5 }));
    }
}
