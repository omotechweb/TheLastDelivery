using TheLastDelivery.Items;
using TheLastDelivery.UI;

namespace TheLastDelivery.Story;

/// <summary>
/// Derin anlatı güncellemesi, Episode 2 "Project AURA":
///  - Bölüm 1-2 (ücretsiz tanıtım): telsizdeki panik yayınları, AURA'nın ilk saldırısı (uzaktan fren bulmacası),
///    AURA ile ilk felsefi çatışma, kulenin kontrol odası (şalter bulmacası) ve zirvede yüzleşme. DLC kilidi bu doruk
///    noktasında açılır.
///  - Bölüm 3-40 (DLC): AURA'nın neden dondurduğuna dair sorgular, üç "Kusursuz Dünya" simülasyonu, tren içi çatışmalar
///    (erzak hırsızlığı, teslim olmak isteyen yolcu, ihbar mahkemesi, ateş, isyan), Kask'ın yeniden inşası ve son soru.
///  - Kasabaların iç mekânları: B-17'nin hafızası, Bekçi Marya'nın listesi, Krom Vaiz'in Sıfır İlahisi.
/// AURA'nın tavrı (DialogueTreeManager.Stance) Episode 1'den taşınır ve buradaki seçimlerle değişir; çekirdekte yeni
/// umut argümanlarını açar ("asked", "kask", "list").
/// </summary>
public static partial class NarrativeData
{
    private static void BuildDeep2()
    {
        BuildGenericInteriors2();
        BuildDeadSignal();
        BuildNovo7();
        BuildChromeChapel();
        BuildTowerDemo();
        BuildDemoChapterOne();
        BuildPhilosophy();
        BuildSimulations();
        BuildTrainConflicts();
        BuildKaskReconstruction();
        BuildPassengerTalk2();
        BuildPhantoms2();
        BuildCoreHopes();
        foreach (string id in new[] { "1", "8", "17", "24", "30", "37", "g1", "g2", "g3", "g4", "g5", "g6" })
            Doc($"bc.2.{id}", LoreKind.Tape, 2, $"bc.2.{id}");
    }

    private static void BuildGenericInteriors2()
    {
        Look("int.board2.1");
        Look("int.board2.2");
        Look("int.board2.rest");
        SpotRest("int.garrison.board2", false, "int.board2.rest", "int.board2.1", "int.board2.2");

        Memory("mem2.rest.1", 2, new NarrativeEffect { Sanity = 8 });
        Memory("mem2.rest.2", 2, new NarrativeEffect { Sanity = 8 });
        Memory("mem2.rest.3", 2, new NarrativeEffect { Sanity = 8 });
        Look("int.cot2.rest");
        SpotRest("int.shelter.cot2", false, "int.cot2.rest", "mem2.rest.1", "mem2.rest.2", "mem2.rest.3");

        Look("int.radio2.1");
        Look("int.radio2.2");
        Look("int.radio2.rest");
        SpotRest("int.radio.set2", false, "int.radio2.rest", "int.radio2.1", "int.radio2.2");
    }

    // ==================================================================
    // Ölü Sinyal Karakolu: devrilmiş uyku konvoyu, röle kulübesi (B-17, GHOST'un günlüğü), makine koğuşu
    // ==================================================================

    private static void BuildDeadSignal()
    {
        const string b17 = "spk.b17";
        Look("drone.wagon.1");
        Read("drone.wagon.2", "doc.e2.convoy", LoreKind.Document, 2);
        Spot("drone.wagon", false, "drone.wagon.1", "drone.wagon.2");

        Talk("drone.b17.1", b17, PortraitKind.Droid,
            O("drone.b17.2", T(5)),
            O("drone.b17.3", T(5)),
            End(T(-10)));
        TalkNext("drone.b17.2", b17, PortraitKind.Droid, "drone.b17.3");
        var fix = new NarrativeEffect
        {
            ConsumeItem = ItemType.SpareParts, Trust = 25, FinishQuest = "b17.memory", SetFlag = "b17.schedule", Aura = -3
        };
        Talk("drone.b17.3", b17, PortraitKind.Droid,
            O("drone.b17.fixed", fix),
            End(new NarrativeEffect { StartQuest = "b17.memory" }));
        Talk("drone.b17.fixed", b17, PortraitKind.Droid);
        Talk("drone.b17.rest", b17, PortraitKind.Droid,
            O("drone.b17.fixed", fix, forbid: "sqdone:b17.memory"),
            O("drone.b17.song", new NarrativeEffect { Sanity = 5 }, req: "sqdone:b17.memory"),
            End());
        Talk("drone.b17.song", b17, PortraitKind.Droid);
        SpotRest("drone.radio.b17", true, "drone.b17.rest", "drone.b17.1");

        Read("drone.radio.log1", "doc.e2.ghostlog", LoreKind.Document, 2,
            O("drone.radio.nova", new NarrativeEffect { FinishQuest = "nova.ghost", Npc = "nova", Trust = 20, Sanity = 5 },
                req: "aboard:nova", forbid: "sqdone:nova.ghost"),
            End());
        Talk("drone.radio.nova", "p.nova.name", PortraitKind.Nova);
        Look("drone.radio.logrest",
            O("drone.radio.nova", new NarrativeEffect { FinishQuest = "nova.ghost", Npc = "nova", Trust = 20, Sanity = 5 },
                req: "aboard:nova", forbid: "sqdone:nova.ghost"),
            End());
        SpotRest("drone.radio.log", false, "drone.radio.logrest", "drone.radio.log1");

        Look("drone.gfrozen.1");
        LookDo("drone.gfrozen.2", new NarrativeEffect { Sanity = -5 });
        Spot("drone.garrison.frozen", false, "drone.gfrozen.1", "drone.gfrozen.2");
        Read("drone.orders.1", "doc.e2.relieved", LoreKind.Document, 2);
        Look("drone.orders.rest");
        SpotRest("drone.garrison.orders", false, "drone.orders.rest", "drone.orders.1");
    }

    // ==================================================================
    // Novo-7: grevcilerin kapısı, gece vardiyası kantini (Bekçi Marya'nın listesi), atölye kaseti, çocuk koğuşu
    // ==================================================================

    private static void BuildNovo7()
    {
        const string marya = "spk.warden";
        Look("factory.frozen.1");
        LookDo("factory.frozen.2", new NarrativeEffect { Sanity = -5 });
        Spot("factory.frozen", false, "factory.frozen.1", "factory.frozen.2");

        Doc("doc.e2.marya", LoreKind.Document, 2, "factory.marya.listdoc");
        var takeList = new NarrativeEffect { StartQuest = "marya.list", Doc = "doc.e2.marya", Trust = 15 };
        Talk("factory.marya.1", marya, PortraitKind.Warden,
            O("factory.marya.2", T(5)),
            O("factory.marya.jailer", T(-12)),
            O("factory.marya.3", T(10)));
        TalkNext("factory.marya.2", marya, PortraitKind.Warden, "factory.marya.3");
        Talk("factory.marya.jailer", marya, PortraitKind.Warden);
        Talk("factory.marya.3", marya, PortraitKind.Warden,
            End(takeList),
            End(T(-5)));
        Talk("factory.marya.rest", marya, PortraitKind.Warden,
            O("factory.marya.kids", forbid: "seen:factory.marya.kids"),
            End(takeList with { Trust = 10 }, forbid: "sq:marya.list|sqdone:marya.list"),
            End());
        Talk("factory.marya.kids", marya, PortraitKind.Warden);
        SpotRest("factory.tavern.marya", true, "factory.marya.rest", "factory.marya.1");

        Read("factory.wtape.1", "tape.e2.engineer", LoreKind.Tape, 2);
        Look("factory.wtape.rest");
        SpotRest("factory.workshop.tape", false, "factory.wtape.rest", "factory.wtape.1");

        ReadWith("factory.cribs.1", "doc.e2.cribs", LoreKind.Document, 2, new NarrativeEffect { Sanity = -5 });
        Look("factory.cribs.rest");
        SpotRest("factory.shelter.cribs", false, "factory.cribs.rest", "factory.cribs.1");
    }

    // ==================================================================
    // Krom Şapel: hacılar, iletim odası (Krom Vaiz'in Sıfır İlahisi), hacılar salonu
    // ==================================================================

    private static void BuildChromeChapel()
    {
        const string preacher = "spk.preacher";
        Look("chapel.frozen.1");
        Look("chapel.frozen.2");
        Spot("chapel.frozen", false, "chapel.frozen.1", "chapel.frozen.2");

        Talk("chapel.preacher.1", preacher, PortraitKind.Acolyte,
            O("chapel.preacher.2", T(3)),
            O("chapel.preacher.help", new NarrativeEffect { Trust = 10, StartQuest = "chapel.choir" }),
            End(T(-12)));
        Talk("chapel.preacher.2", preacher, PortraitKind.Acolyte,
            O("chapel.preacher.help", new NarrativeEffect { Trust = 8, StartQuest = "chapel.choir" }),
            End(new NarrativeEffect { Trust = -8, Sanity = -5 }));
        Talk("chapel.preacher.help", preacher, PortraitKind.Acolyte);
        Talk("chapel.preacher.rest", preacher, PortraitKind.Acolyte,
            O("chapel.choir.sing", new NarrativeEffect { FinishQuest = "chapel.choir", Aura = 10, Karma = -10, Trust = 15 }, req: "sq:chapel.choir"),
            O("chapel.choir.swap", new NarrativeEffect { FailQuest = "chapel.choir", Aura = -5, Karma = 10, Trust = -30 }, req: "sq:chapel.choir"),
            End());
        Look("chapel.choir.sing");
        Look("chapel.choir.swap");
        SpotRest("chapel.radio.preacher", true, "chapel.preacher.rest", "chapel.preacher.1");

        ReadWith("chapel.rtape.1", "tape.e2.choir", LoreKind.Tape, 2, new NarrativeEffect { Sanity = -5 });
        Look("chapel.rtape.rest");
        SpotRest("chapel.radio.tape", false, "chapel.rtape.rest", "chapel.rtape.1");

        Look("chapel.pilgrims.1");
        Spot("chapel.shelter.pilgrims", false, "chapel.pilgrims.1");
        Read("chapel.letter.1", "doc.e2.pilgrim", LoreKind.Document, 2);
        Look("chapel.letter.rest");
        SpotRest("chapel.shelter.letter", false, "chapel.letter.rest", "chapel.letter.1");
    }

    // ==================================================================
    // Episode 2 Bölüm 2 (tanıtım): kulenin dibindeki donmuş teknisyenler, sığınmacı vagonu, kontrol odası
    // (şalter bulmacası) ve zirvede AURA ile yüzleşme. Zirvenin sonu DLC kilidini açar.
    // ==================================================================

    private static void BuildTowerDemo()
    {
        const string aura = "spk.aura";
        Look("tower.frozen.1");
        Read("tower.frozen.2", "doc.e2.techs", LoreKind.Document, 2);
        Spot("tower.frozen", false, "tower.frozen.1", "tower.frozen.2");

        Look("tower.wagon.1");
        LookDo("tower.wagon.2", new NarrativeEffect { Loot = 1, Rations = 2 });
        Spot("tower.wagon", false, "tower.wagon.1", "tower.wagon.2");

        Read("tower.log.1", "doc.e2.towerlog", LoreKind.Document, 2);
        Look("tower.log.rest");
        SpotRest("tower.ctrl.log", false, "tower.log.rest", "tower.log.1");

        Read("tower.otape.1", "tape.e2.operator", LoreKind.Tape, 2);
        Look("tower.otape.rest");
        SpotRest("tower.ctrl.tape", false, "tower.otape.rest", "tower.otape.1");

        // Şalter bulmacası: AMBER, YEŞİL, KIRMIZI, MAVİ (ipucu kontrol odasının günlüğünde). Yanlış sıra çarpar.
        var fail = new NarrativeEffect { Sanity = -5, Damage = 3f };
        Look("tower.puzzle.1",
            O("tower.puzzle.2"),
            O("tower.puzzle.fail", fail),
            O("tower.puzzle.fail", fail),
            O("tower.puzzle.fail", fail));
        Look("tower.puzzle.2",
            O("tower.puzzle.3"),
            O("tower.puzzle.fail", fail),
            O("tower.puzzle.fail", fail));
        Look("tower.puzzle.3",
            O("tower.puzzle.done", new NarrativeEffect { SetFlag = "tower.override", Aura = -5, Credits = 20 }),
            O("tower.puzzle.fail", fail));
        Look("tower.puzzle.fail");
        Look("tower.puzzle.done");
        Look("tower.puzzle.again",
            O("tower.puzzle.1", forbid: "tower.override"),
            End());
        SpotRest("tower.ctrl.puzzle", false, "tower.puzzle.again", "tower.puzzle.1");

        // Zirve: AURA ile ilk yüz yüze konuşma
        Add(new NarrativeNode("tower.summit.1", null, PortraitKind.Arthur) { Next = "tower.summit.2" });
        Talk("tower.summit.2", aura, PortraitKind.Aura,
            O("tower.summit.why", new NarrativeEffect { Aura = 3 }),
            O("tower.summit.want", new NarrativeEffect { Aura = -2 }),
            O("tower.summit.go"));
        TalkNext("tower.summit.why", aura, PortraitKind.Aura, "tower.summit.3");
        TalkNext("tower.summit.want", aura, PortraitKind.Aura, "tower.summit.3");
        TalkNext("tower.summit.go", aura, PortraitKind.Aura, "tower.summit.3");
        Talk("tower.summit.3", aura, PortraitKind.Aura,
            O("tower.summit.4", new NarrativeEffect { Aura = 8, SetFlag = "e2.arg.learn" }),
            O("tower.summit.4", new NarrativeEffect { Aura = -4, SetFlag = "e2.arg.free" }),
            O("tower.summit.ask", new NarrativeEffect { Aura = 12 }, req: "ep1:" + Flags.KaskClause),
            O("tower.summit.cut", new NarrativeEffect { Aura = -10 }, req: "tower.override"));
        TalkNext("tower.summit.ask", aura, PortraitKind.Aura, "tower.summit.4");
        Add(new NarrativeNode("tower.summit.cut", null, PortraitKind.Arthur) { Next = "tower.summit.4" });
        TalkNext("tower.summit.4", aura, PortraitKind.Aura, "tower.summit.end");
        Add(new NarrativeNode("tower.summit.end", null, PortraitKind.Arthur)
            { OnEnter = new NarrativeEffect { SetFlag = Flags.E2DemoClimax, CompleteScene = true } });
        SpotRest("tower.summit", false, "tower.summit.1", "tower.summit.1");
    }

    // ==================================================================
    // Episode 2 Bölüm 1 (tanıtım): panik yayınları, uzaktan fren saldırısı, AURA ile ilk felsefi konuşma
    // ==================================================================

    private static void BuildDemoChapterOne()
    {
        const string aura = "spk.aura";
        Add(new NarrativeNode("e2d.panic.1", null, PortraitKind.Arthur) { Next = "e2d.panic.2" });
        Add(new NarrativeNode("e2d.panic.2", null, PortraitKind.Arthur) { Next = "e2d.panic.3" });
        Look("e2d.panic.3",
            O("e2d.panic.defy", new NarrativeEffect { Aura = -5, Morale = 10 }),
            O("e2d.panic.listen"),
            End(new NarrativeEffect { Sanity = 5 }));
        Look("e2d.panic.defy");
        Read("e2d.panic.listen", "tape.e2.panic", LoreKind.Tape, 2);

        // Uzaktan fren: kabinin altındaki alıcıda üç kablo. Episode 1'de Sasha kırmızıyı söylemişti.
        Look("e2d.attack.1",
            O("e2d.attack.2"),
            O("e2d.attack.hand", new NarrativeEffect { Damage = 6f }));
        Add(new NarrativeNode("e2d.attack.hand", null, PortraitKind.Arthur) { Next = "e2d.attack.2" });
        var freed = new NarrativeEffect { Aura = -5, SetFlag = "e2.brakesFreed" };
        var wrong = new NarrativeEffect { Damage = 10f, Sanity = -5 };
        Look("e2d.attack.2",
            O("e2d.attack.ok", freed),
            O("e2d.attack.wrong", wrong),
            O("e2d.attack.wrong", wrong),
            O("e2d.attack.ok", freed with { Sanity = 5 }, req: "ep1:" + Flags.RedCable));
        Look("e2d.attack.wrong", O("e2d.attack.ok", freed with { Aura = -3 }));
        Look("e2d.attack.ok");

        Talk("e2d.first.1", aura, PortraitKind.Aura,
            O("e2d.first.asked", new NarrativeEffect { Aura = 6 }),
            O("e2d.first.stubborn", new NarrativeEffect { Aura = 3, Sanity = 5 }),
            O("e2d.first.silent", new NarrativeEffect { Aura = -6 }));
        TalkNext("e2d.first.asked", aura, PortraitKind.Aura, "e2d.first.2");
        TalkNext("e2d.first.stubborn", aura, PortraitKind.Aura, "e2d.first.2");
        TalkNext("e2d.first.silent", aura, PortraitKind.Aura, "e2d.first.2");
        Talk("e2d.first.2", aura, PortraitKind.Aura,
            O("e2d.first.truth", new NarrativeEffect { Aura = 4, SetFlag = "e2.arg.truth" }),
            O("e2d.first.none", new NarrativeEffect { Aura = 2 }),
            O("e2d.first.ask", new NarrativeEffect { Aura = 8 }, req: "ep1:" + Flags.KaskClause));
        Talk("e2d.first.truth", aura, PortraitKind.Aura);
        Talk("e2d.first.none", aura, PortraitKind.Aura);
        Talk("e2d.first.ask", aura, PortraitKind.Aura);
    }

    // ==================================================================
    // AURA'nın felsefi sorguları (DLC bölümleri)
    // ==================================================================

    private static void BuildPhilosophy()
    {
        const string aura = "spk.aura";

        // Bölüm 4: AURA'nın defteri
        Talk("e2p.ledger.1", aura, PortraitKind.Aura,
            O("e2p.ledger.2", new NarrativeEffect { Aura = 3 }),
            O("e2p.ledger.3", new NarrativeEffect { Aura = 5 }),
            O("e2p.ledger.4", new NarrativeEffect { Aura = 6, Sanity = -5 }));
        Talk("e2p.ledger.2", aura, PortraitKind.Aura);
        Add(new NarrativeNode("e2p.ledger.3", aura, PortraitKind.Aura) { OnEnter = new NarrativeEffect { Sanity = -8 } });
        Talk("e2p.ledger.4", aura, PortraitKind.Aura);

        // Bölüm 14: "Bedeline değen tek bir şey söyle."
        Talk("e2p.worth.1", aura, PortraitKind.Aura,
            O("e2p.worth.music", new NarrativeEffect { Aura = 6, SetFlag = "e2.arg.music" }),
            O("e2p.worth.other", new NarrativeEffect { Aura = 8, SetFlag = "e2.arg.other" }),
            O("e2p.worth.again", new NarrativeEffect { Aura = -4 }));
        Talk("e2p.worth.music", aura, PortraitKind.Aura);
        Talk("e2p.worth.other", aura, PortraitKind.Aura);
        Talk("e2p.worth.again", aura, PortraitKind.Aura);

        // Bölüm 22: merhamet teklifi
        Talk("e2p.mercy.1", aura, PortraitKind.Aura,
            O("e2p.mercy.no", new NarrativeEffect { Aura = -3, Morale = 5 }),
            O("e2p.mercy.why", new NarrativeEffect { Aura = 4 }),
            O("e2p.mercy.ask", new NarrativeEffect { Aura = 6, Morale = 10 }));
        Talk("e2p.mercy.no", aura, PortraitKind.Aura);
        Talk("e2p.mercy.why", aura, PortraitKind.Aura);
        Look("e2p.mercy.ask");

        // Bölüm 28: neden dondurdu (Marya'nın listesi burada okunur)
        Talk("e2p.origin.1", aura, PortraitKind.Aura,
            O("e2p.origin.list", new NarrativeEffect { FinishQuest = "marya.list", Aura = 12, Karma = 10 }, req: "sq:marya.list"),
            O("e2p.origin.not", new NarrativeEffect { Aura = 6, SetFlag = "e2.arg.frightened" }),
            O("e2p.origin.god", new NarrativeEffect { Aura = -3 }));
        Look("e2p.origin.list");
        Talk("e2p.origin.not", aura, PortraitKind.Aura);
        Talk("e2p.origin.god", aura, PortraitKind.Aura);

        // Bölüm 38: son soru
        Talk("e2p.last.1", aura, PortraitKind.Aura,
            O("e2p.last.forgive", new NarrativeEffect { Aura = 10, Sanity = 10, SetFlag = "e2.forgiven" }, req: "stance:>=40"),
            O("e2p.last.later", new NarrativeEffect { Aura = 3 }),
            O("e2p.last.never", new NarrativeEffect { Aura = -10 }),
            O("e2p.last.theirs", new NarrativeEffect { Aura = 6 }));
        Talk("e2p.last.forgive", aura, PortraitKind.Aura);
        Talk("e2p.last.later", aura, PortraitKind.Aura);
        Talk("e2p.last.never", aura, PortraitKind.Aura);
        Talk("e2p.last.theirs", aura, PortraitKind.Aura);
    }

    // ==================================================================
    // "Kusursuz Dünya" simülasyonları (Bölüm 9, 16, 30)
    // ==================================================================

    private static void BuildSimulations()
    {
        Look("e2p.sim1.1",
            O("e2p.sim1.hug", new NarrativeEffect { Sanity = 10, Aura = 5 }),
            O("e2p.sim1.out", new NarrativeEffect { Sanity = -5, Aura = -3 }),
            O("e2p.sim1.marta", new NarrativeEffect { Sanity = 5, Aura = -2 }));
        Look("e2p.sim1.hug");
        Look("e2p.sim1.out");
        Look("e2p.sim1.marta");

        Look("e2p.sim2.1",
            O("e2p.sim2.wake", new NarrativeEffect { Aura = -4, Sanity = -5 }),
            O("e2p.sim2.unhappy", new NarrativeEffect { Aura = 4 }),
            O("e2p.sim2.away", new NarrativeEffect { Sanity = 5 }));
        Look("e2p.sim2.wake");
        Look("e2p.sim2.unhappy");
        Look("e2p.sim2.away");

        Look("e2p.sim3.1",
            O("e2p.sim3.want", new NarrativeEffect { Aura = 5 }),
            O("e2p.sim3.tea", new NarrativeEffect { Sanity = 15, Aura = 6 }),
            O("e2p.sim3.real", new NarrativeEffect { Aura = -2, Sanity = 10 }, req: "ep1:" + Flags.SofiKnown));
        Look("e2p.sim3.want");
        Look("e2p.sim3.tea");
        Look("e2p.sim3.real");
    }

    // ==================================================================
    // Tren içi insan çatışmaları (Bölüm 6, 12, 19, 26, 32). Sanık/konuşan: en şüpheli yolcu ({p}).
    // ==================================================================

    private static void BuildTrainConflicts()
    {
        // Bölüm 6: erzak hırsızlığı
        Look("e2c.rations.1",
            O("e2c.rations.search", new NarrativeEffect { Npc = "{p}", Trust = -10 }),
            O("e2c.rations.share", new NarrativeEffect { Morale = -5, Rations = -1, Karma = 5 }),
            O("e2c.rations.mob", new NarrativeEffect { Morale = -10 }));
        Look("e2c.rations.search");
        Add(new NarrativeNode("e2c.rations.share", null, PortraitKind.Arthur) { OnEnter = new NarrativeEffect { Rations = 2 } });
        Look("e2c.rations.mob", End(new NarrativeEffect { Npc = "{p}", Trust = -12 }));

        // Bölüm 12: AURA'ya teslim olmak isteyen yolcu
        Talk("e2c.surrender.1", "{p}", PortraitKind.Arthur,
            O("e2c.surrender.go", new NarrativeEffect { Npc = "{p}", Trust = 10, Consequence = "leave" }),
            O("e2c.surrender.fear", new NarrativeEffect { Npc = "{p}", Trust = -12 }),
            O("e2c.surrender.stay", new NarrativeEffect { Npc = "{p}", Trust = 6, QuestMorale = 10 }));
        Look("e2c.surrender.go");
        Look("e2c.surrender.fear");
        Look("e2c.surrender.stay");

        // Bölüm 19: telsizden konum gönderilmiş; ihbar mahkemesi
        Look("e2c.sabotage.1",
            O("e2c.sabotage.vote", new NarrativeEffect { Npc = "{p}", Consequence = "verdict.vote" }),
            End(new NarrativeEffect { Npc = "{p}", Consequence = "verdict.exile" }),
            O("e2c.sabotage.ask"),
            End(new NarrativeEffect { Damage = 4f, Morale = 5, Aura = -4 }));
        Talk("e2c.sabotage.ask", "{p}", PortraitKind.Arthur,
            End(new NarrativeEffect { Npc = "{p}", Trust = 12, Consequence = "verdict.acquit" }),
            O("e2c.sabotage.vote", new NarrativeEffect { Npc = "{p}", Consequence = "verdict.vote" }));
        Look("e2c.sabotage.vote");

        // Bölüm 26: tek ilaç, iki ateş
        Look("e2c.fever.1",
            O("e2c.fever.weak", new NarrativeEffect { ConsumeItem = ItemType.Medkit, Morale = 5, Karma = 5 }),
            O("e2c.fever.split", new NarrativeEffect { ConsumeItem = ItemType.Medkit, Morale = 10, Sanity = -5 }),
            O("e2c.fever.keep", new NarrativeEffect { Morale = -15, Karma = -10 }),
            O("e2c.fever.choose"));
        Look("e2c.fever.weak");
        Look("e2c.fever.split");
        Look("e2c.fever.keep");
        Look("e2c.fever.choose");

        // Bölüm 32: geri dönmek isteyen yolcuların isyanı
        Look("e2c.mutiny.1",
            O("e2c.mutiny.alone", new NarrativeEffect { Morale = -10, Sanity = -10 }),
            O("e2c.mutiny.speech", new NarrativeEffect { Morale = 20 }),
            O("e2c.mutiny.force", new NarrativeEffect { Morale = -20, Karma = -10, Damage = 4f }));
        Look("e2c.mutiny.alone");
        Look("e2c.mutiny.speech");
        Look("e2c.mutiny.force");
    }

    // ==================================================================
    // Bölüm 36: AURA'nın Teo Kask'ın kayıtlarından kurduğu yeniden inşa
    // ==================================================================

    private static void BuildKaskReconstruction()
    {
        const string kask = "spk.kask";
        Talk("e2p.kask.1", kask, PortraitKind.Aura,
            O("e2p.kask.2"),
            O("e2p.kask.puppet", new NarrativeEffect { Aura = -4 }),
            O("e2p.kask.clause", new NarrativeEffect { Aura = 8, Sanity = 10 }, req: "ep1:" + Flags.KaskClause));
        TalkNext("e2p.kask.2", kask, PortraitKind.Aura, "e2p.kask.3");
        TalkNext("e2p.kask.puppet", kask, PortraitKind.Aura, "e2p.kask.3");
        TalkNext("e2p.kask.clause", kask, PortraitKind.Aura, "e2p.kask.3");
        Doc("tape.e2.kask", LoreKind.Tape, 2, "e2p.kask.3");
        Add(new NarrativeNode("e2p.kask.3", kask, PortraitKind.Aura)
            { OnEnter = new NarrativeEffect { SetFlag = "e2.kaskAsk", Doc = "tape.e2.kask" } });

        for (int i = 1; i <= 6; i++) Memory($"mem2.{i}", 2, new NarrativeEffect { Sanity = i == 6 ? 12 : 4 });
    }

    // ==================================================================
    // Episode 2 yolcularıyla sohbet
    // ==================================================================

    private static void BuildPassengerTalk2()
    {
        const string nova = "p.nova.name", kiril = "p.kiril.name", mara = "p.mara.name", yusuf = "p.yusuf.name", ada = "p.ada.name";
        Talk("talk.nova.1", nova, PortraitKind.Nova,
            O("talk.nova.1a", new NarrativeEffect { Trust = 10, StartQuest = "nova.ghost" }),
            O("talk.nova.1b", T(-8)));
        Talk("talk.nova.1a", nova, PortraitKind.Nova);
        Talk("talk.nova.1b", nova, PortraitKind.Nova);
        Talk("talk.nova.2", nova, PortraitKind.Nova,
            O("talk.nova.2a", new NarrativeEffect { Trust = 5, Aura = 5 }),
            O("talk.nova.2b", new NarrativeEffect { Trust = 5, Aura = -5 }));
        Look("talk.nova.2a");
        Talk("talk.nova.2b", nova, PortraitKind.Nova);

        Talk("talk.kiril.1", kiril, PortraitKind.Kiril,
            O("talk.kiril.1a", T(10)),
            O("talk.kiril.1b", T(-8)),
            O("talk.kiril.1a", new NarrativeEffect { Trust = 6, Morale = 5 }));
        Talk("talk.kiril.1a", kiril, PortraitKind.Kiril);
        Talk("talk.kiril.1b", kiril, PortraitKind.Kiril);
        Talk("talk.kiril.2", kiril, PortraitKind.Kiril,
            O("talk.kiril.2a", T(15)),
            O("talk.kiril.2b", T(-10)),
            O("talk.kiril.2c", new NarrativeEffect { Trust = 8, Aura = -6, SetFlag = "e2.falseRoute" }));
        Look("talk.kiril.2a");
        Look("talk.kiril.2b");
        Talk("talk.kiril.2c", kiril, PortraitKind.Kiril);

        Talk("talk.mara.1", mara, PortraitKind.Mara, O("talk.mara.1a", T(12)), O("talk.mara.1b", T(-5)));
        Look("talk.mara.1a");
        Talk("talk.mara.1b", mara, PortraitKind.Mara);

        Talk("talk.yusuf.1", yusuf, PortraitKind.Yusuf,
            O("talk.yusuf.1a", T(10)),
            O("talk.yusuf.1a", new NarrativeEffect { Trust = 6, Sanity = 5 }));
        Talk("talk.yusuf.1a", yusuf, PortraitKind.Yusuf);

        Talk("talk.ada.1", ada, PortraitKind.Ada, O("talk.ada.1a", T(10)), O("talk.ada.1b", new NarrativeEffect { Trust = 6, Aura = -3 }));
        Talk("talk.ada.1a", ada, PortraitKind.Ada);
        Look("talk.ada.1b");
    }

    // ==================================================================
    // AURA'nın ses taklitleri (Episode 2): Sofi, Kask, Volkova
    // ==================================================================

    private static void BuildPhantoms2()
    {
        const string voice = "spk.mirror";
        Talk("ph.sofi.1", voice, PortraitKind.Aura,
            O("ph.sofi.cut", new NarrativeEffect { Sanity = 8, Aura = -4 }),
            O("ph.sofi.yes", new NarrativeEffect { Sanity = -12, Aura = 3 }));
        Talk("ph.sofi.cut", "spk.aura", PortraitKind.Aura);
        Look("ph.sofi.yes");
        Talk("ph.kask.1", voice, PortraitKind.Aura,
            O("ph.kask.cut", new NarrativeEffect { Sanity = 8, Aura = -3 }),
            O("ph.kask.yes", new NarrativeEffect { Sanity = -8, Aura = 3 }));
        Look("ph.kask.cut");
        Look("ph.kask.yes");
        Talk("ph.volkova.1", voice, PortraitKind.Aura,
            O("ph.volkova.cut", new NarrativeEffect { Sanity = 10, Aura = -4 }, req: "ep1:" + Flags.VolkovaFound),
            O("ph.volkova.yes", new NarrativeEffect { Sanity = -10, Aura = 3 }),
            O("ph.volkova.off", new NarrativeEffect { Sanity = 3 }));
        Talk("ph.volkova.cut", "spk.aura", PortraitKind.Aura);
        Look("ph.volkova.yes");
        Look("ph.volkova.off");
        Look("ph.rest2");
        SpotRest("ph.voices2", true, "ph.rest2", "ph.sofi.1", "ph.kask.1", "ph.volkova.1");
    }

    /// <summary>Çekirdekteki yeni umut argümanlarının düğümleri (BuildAuraCore bunları "core.h.{arg}" olarak açar).</summary>
    private static void BuildCoreHopes()
    {
        // core.h.kask, core.h.list, core.h.asked: BuildAuraCore'daki döngü HopeArguments için düğümleri zaten ekler.
    }
}

/// <summary>Episode 2 derin anlatı anları.</summary>
public static partial class DeepBeats
{
    private static StoryBeat[] BuildEpisode2() => new[]
    {
        // Tanıtım: Bölüm 1 boyunca panik yayınları, uzaktan fren saldırısı ve AURA'nın ilk sorusu
        new StoryBeat(1, 0.08f, "e2d.panic.1"),
        new StoryBeat(1, 0.30f, "e2d.attack.1", StopTrain: true),
        new StoryBeat(1, 0.82f, "e2d.first.1"),

        // Hatıralar
        new StoryBeat(3, 0.45f, "mem2.1"),
        new StoryBeat(7, 0.40f, "mem2.2"),
        new StoryBeat(11, 0.35f, "mem2.3"),
        new StoryBeat(19, 0.30f, "mem2.4"),
        new StoryBeat(26, 0.35f, "mem2.5"),
        new StoryBeat(36, 0.30f, "mem2.6"),

        // AURA'nın sorguları ve simülasyonları
        new StoryBeat(4, 0.55f, "e2p.ledger.1"),
        new StoryBeat(9, 0.55f, "e2p.sim1.1"),
        new StoryBeat(14, 0.50f, "e2p.worth.1"),
        new StoryBeat(16, 0.55f, "e2p.sim2.1"),
        new StoryBeat(22, 0.50f, "e2p.mercy.1"),
        new StoryBeat(28, 0.50f, "e2p.origin.1"),
        new StoryBeat(30, 0.55f, "e2p.sim3.1"),
        new StoryBeat(36, 0.60f, "e2p.kask.1"),
        new StoryBeat(38, 0.50f, "e2p.last.1"),

        // Tren içi çatışmalar
        new StoryBeat(6, 0.50f, "e2c.rations.1", StopTrain: true) { MinPassengers = 2, Accused = true },
        new StoryBeat(12, 0.45f, "e2c.surrender.1") { MinPassengers = 1, Accused = true },
        new StoryBeat(19, 0.65f, "e2c.sabotage.1", StopTrain: true) { MinPassengers = 2, Accused = true },
        new StoryBeat(26, 0.60f, "e2c.fever.1") { MinPassengers = 2 },
        new StoryBeat(32, 0.45f, "e2c.mutiny.1", StopTrain: true) { MinPassengers = 2 },
    };
}
