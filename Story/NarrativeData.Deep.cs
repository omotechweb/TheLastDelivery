using TheLastDelivery.Items;
using TheLastDelivery.UI;

namespace TheLastDelivery.Story;

/// <summary>
/// Derin anlatı güncellemesi, Episode 1: girilebilir binalar (taverna, karakol, atölye, sığınak, telsiz kulesi),
/// kasaba liderlerinin güven barlı diyalog ağları, yan görevler, belgeler, ses kasetleri, Arthur'un hatıraları,
/// her beş bölümde bir dönüm noktası ve AURA'nın ilk "yankıları". Metinler: "nv.{id}" (StringsEnDeep/StringsTrDeep).
///
/// Arthur'un geçmişi: Demiryolu Kolordusu 9. Müfreze'nin makinist çavuşuydu. Müfreze, AURORA yer istasyonuna kablo döşedi.
/// Komutanı Binbaşı Ines Volkova "Sefer 0" emrini verdi; sistem mühendisi Teğmen Teo Kask, makinenin bir insan hakkında
/// karar vermeden önce "sormasını" şart koşan Madde 7'yi yazdı. Madde 7, donmadan bir hafta önce Konsey Başkanı H. Rhee
/// tarafından silindi.
/// </summary>
public static partial class NarrativeData
{
    // ---- Yardımcılar ----

    private static void Doc(string id, LoreKind kind, int episode, string node) =>
        LoreArchive.Register(new LoreEntry(id, kind, episode, node));

    /// <summary>Belge/kaset düğümü: açılınca kayıt arşive girer (metni düğümün metnidir).</summary>
    private static void Read(string id, string doc, LoreKind kind, int episode, params NarrativeOption[] options)
    {
        Doc(doc, kind, episode, id);
        Add(new NarrativeNode(id, null, PortraitKind.Arthur, options) { OnEnter = new NarrativeEffect { Doc = doc } });
    }

    /// <summary>Belge düğümü + ek etki (akıl sağlığı, bayrak...).</summary>
    private static void ReadWith(string id, string doc, LoreKind kind, int episode, NarrativeEffect extra, params NarrativeOption[] options)
    {
        Doc(doc, kind, episode, id);
        Add(new NarrativeNode(id, null, PortraitKind.Arthur, options) { OnEnter = extra with { Doc = doc } });
    }

    /// <summary>Arthur'un hatırası: "HATIRA" başlıklı anlatı düğümü; arşive hatıra olarak girer.</summary>
    private static void Memory(string id, int episode, NarrativeEffect extra = null)
    {
        Doc(id, LoreKind.Memory, episode, id);
        Add(new NarrativeNode(id, null, PortraitKind.Arthur)
        {
            OnEnter = (extra ?? new NarrativeEffect()) with { Doc = id },
            Title = "mem.title"
        });
    }

    /// <summary>Etkili anlatı düğümü (seçeneksiz).</summary>
    private static void LookDo(string id, NarrativeEffect onEnter, string next = null) =>
        Add(new NarrativeNode(id, null, PortraitKind.Arthur) { OnEnter = onEnter, Next = next });

    private static void TalkNext(string id, string speaker, PortraitKind portrait, string next, NarrativeEffect onEnter = null) =>
        Add(new NarrativeNode(id, speaker, portrait) { Next = next, OnEnter = onEnter });

    private static NarrativeEffect T(int trust) => new() { Trust = trust };

    private static void BuildDeep()
    {
        BuildGenericInteriors();
        BuildFrostMarket();
        BuildIronhall();
        BuildLastLantern();
        BuildRavenPass();
        BuildTuzla();
        BuildSouthGate();
        BuildHqBunker();
        BuildInstituteLab();
        BuildDeepBeats1();
        BuildPassengerTalk1();
        BuildPhantoms1();
        BuildBroadcasts1();
    }

    // ==================================================================
    // Her iç mekânda bulunan genel noktalar
    // ==================================================================

    private static void BuildGenericInteriors()
    {
        LookDo("int.hearth.1", new NarrativeEffect { Sanity = 5 });
        LookDo("int.hearth.2", new NarrativeEffect { Sanity = 5 });
        LookDo("int.hearth.3", new NarrativeEffect { Sanity = 5 });
        Look("int.hearth.rest");
        SpotRest("int.tavern.hearth", false, "int.hearth.rest", "int.hearth.1", "int.hearth.2", "int.hearth.3");

        Look("int.board.1");
        Look("int.board.2");
        Look("int.board.rest");
        SpotRest("int.garrison.board", false, "int.board.rest", "int.board.1", "int.board.2");

        LookDo("int.bench.1", new NarrativeEffect { Repair = 8f });
        Look("int.bench.2");
        Look("int.bench.rest");
        SpotRest("int.workshop.bench", false, "int.bench.rest", "int.bench.1", "int.bench.2");

        // Sığınaktaki ranza: her uzanışta Arthur'un sıradaki hatırası
        Memory("mem.rest.1", 1, new NarrativeEffect { Sanity = 8 });
        Memory("mem.rest.2", 1, new NarrativeEffect { Sanity = 8 });
        Memory("mem.rest.3", 1, new NarrativeEffect { Sanity = 8 });
        Memory("mem.rest.4", 1, new NarrativeEffect { Sanity = 4 });
        Look("int.cot.rest");
        SpotRest("int.shelter.cot", false, "int.cot.rest", "mem.rest.1", "mem.rest.2", "mem.rest.3", "mem.rest.4");

        Look("int.radio.1");
        Look("int.radio.2");
        Look("int.radio.rest");
        SpotRest("int.radio.set", false, "int.radio.rest", "int.radio.1", "int.radio.2");
    }

    // ==================================================================
    // Kırağı Pazarı: Bakır Çaydanlık tavernası (Greta), Şerif Brann'ın karakolu, sığınak, donmuş satıcı
    // ==================================================================

    private static void BuildFrostMarket()
    {
        const string greta = "spk.greta", brann = "spk.marshal";

        Look("trade.frozen.1");
        Read("trade.frozen.2", "doc.vendor", LoreKind.Document, 1);
        Spot("trade.frozen", false, "trade.frozen.1", "trade.frozen.2");

        // Greta: oğlu Jonas üç kış önce Kızıl Birlik'e katıldı. Yan görev: Jonas'ın akıbeti (Karargah'ta, Bölüm 10)
        Talk("trade.greta.1", greta, PortraitKind.Innkeeper,
            O("trade.greta.2", T(5)),
            End(new NarrativeEffect { Credits = -10, Sanity = 10, Trust = 3 }),
            End(T(-6)));
        Talk("trade.greta.2", greta, PortraitKind.Innkeeper,
            O("trade.greta.3", new NarrativeEffect { Trust = 10, StartQuest = "greta.son" }),
            O("trade.greta.2b", T(-12)));
        Talk("trade.greta.2b", greta, PortraitKind.Innkeeper);
        Add(new NarrativeNode("trade.greta.3", greta, PortraitKind.Innkeeper) { OnEnter = new NarrativeEffect { Rations = 2 } });
        Talk("trade.greta.rest", greta, PortraitKind.Innkeeper,
            O("trade.greta.news", T(2), forbid: "seen:trade.greta.news"),
            O("trade.greta.marta", T(2), forbid: "seen:trade.greta.marta"),
            End());
        Talk("trade.greta.news", greta, PortraitKind.Innkeeper);
        Add(new NarrativeNode("trade.greta.marta", greta, PortraitKind.Innkeeper) { OnEnter = new NarrativeEffect { Sanity = -5 } });
        SpotRest("trade.tavern.greta", true, "trade.greta.rest", "trade.greta.1");

        Read("trade.tape.1", "tape.frost", LoreKind.Tape, 1);
        Look("trade.tape.rest");
        SpotRest("trade.tavern.tape", false, "trade.tape.rest", "trade.tape.1");

        // Şerif Brann: duvarda Arthur'un aranıyor ilanı. Yol vergisi ya da bir yolcunun başındaki ödül.
        Talk("trade.marshal.1", brann, PortraitKind.Officer,
            O("trade.marshal.2", T(-5)),
            O("trade.marshal.2b", new NarrativeEffect { Trust = 8, Sanity = -5 }),
            O("trade.marshal.3"));
        TalkNext("trade.marshal.2", brann, PortraitKind.Officer, "trade.marshal.3");
        TalkNext("trade.marshal.2b", brann, PortraitKind.Officer, "trade.marshal.3");
        Talk("trade.marshal.3", brann, PortraitKind.Officer,
            End(new NarrativeEffect { Credits = -40, Trust = 6, SetFlag = "trade.taxpaid" }),
            O("trade.marshal.4", new NarrativeEffect { Trust = -15, Karma = 10 }),
            O("trade.marshal.bora", req: "aboard:bora"));
        Talk("trade.marshal.4", brann, PortraitKind.Officer);
        Talk("trade.marshal.bora", brann, PortraitKind.Officer,
            End(new NarrativeEffect { Credits = 60, Karma = -25, Npc = "bora", Consequence = "handover" }),
            O("trade.marshal.protect", new NarrativeEffect { Trust = -10, Karma = 10 }));
        LookDo("trade.marshal.protect", new NarrativeEffect { Npc = "bora", Trust = 20, SetFlag = "bora.protected" });
        Talk("trade.marshal.rest", brann, PortraitKind.Officer,
            End(new NarrativeEffect { Credits = -40, Trust = 6, SetFlag = "trade.taxpaid" }, forbid: "trade.taxpaid"),
            End());
        SpotRest("trade.garrison.marshal", true, "trade.marshal.rest", "trade.marshal.1");

        Read("trade.files.1", "doc.poster", LoreKind.Document, 1);
        Read("trade.files.2", "doc.manifest", LoreKind.Document, 1);
        Spot("trade.garrison.files", false, "trade.files.1", "trade.files.2");

        Read("trade.diary.1", "doc.kidsdiary", LoreKind.Document, 1);
        Look("trade.diary.2");
        Spot("trade.shelter.diary", false, "trade.diary.1", "trade.diary.2");
    }

    // ==================================================================
    // Demirhan: Donmuş Vardiya, madenci kantini (Yuri), Sasha'nın atölyesi, maden sığınağı
    // ==================================================================

    private static void BuildIronhall()
    {
        const string yuri = "spk.strike", sasha = "spk.tinker";

        Look("mine.statues.1");
        LookDo("mine.statues.2", new NarrativeEffect { Sanity = -5 });
        Spot("mine.statues", false, "mine.statues.1", "mine.statues.2");

        Talk("mine.yuri.1", yuri, PortraitKind.StrikeLeader,
            O("mine.yuri.2", T(5)),
            O("mine.yuri.2b", T(10)),
            End(T(-8)));
        TalkNext("mine.yuri.2", yuri, PortraitKind.StrikeLeader, "mine.yuri.3");
        TalkNext("mine.yuri.2b", yuri, PortraitKind.StrikeLeader, "mine.yuri.3");
        Talk("mine.yuri.3", yuri, PortraitKind.StrikeLeader,
            End(new NarrativeEffect { Trust = 5, SetFlag = "mine.ledgerAsk" }),
            End(new NarrativeEffect { Trust = -3, Sanity = 5 }));
        Talk("mine.yuri.rest", yuri, PortraitKind.StrikeLeader,
            O("mine.yuri.name", new NarrativeEffect { Trust = 15, Karma = 5 }, req: "doc:doc.ledger", forbid: "seen:mine.yuri.name"),
            End());
        Add(new NarrativeNode("mine.yuri.name", yuri, PortraitKind.StrikeLeader) { OnEnter = new NarrativeEffect { Morale = 5 } });
        SpotRest("mine.tavern.yuri", true, "mine.yuri.rest", "mine.yuri.1");

        Read("mine.ledger.1", "doc.ledger", LoreKind.Document, 1);
        Look("mine.ledger.rest");
        SpotRest("mine.tavern.ledger", false, "mine.ledger.rest", "mine.ledger.1");

        // Sasha: eski Konsorsiyum fren teknisyeni. Yan görev: iki yedek parça → cevher treni zırhı + kırmızı kablo sırrı
        Talk("mine.sasha.1", sasha, PortraitKind.Smith,
            End(new NarrativeEffect { StartQuest = "tinker.parts", Trust = 5 }),
            O("mine.sasha.2", T(3)),
            End(T(-3)));
        Talk("mine.sasha.2", sasha, PortraitKind.Smith,
            End(new NarrativeEffect { StartQuest = "tinker.parts", Trust = 8 }));
        Talk("mine.sasha.rest", sasha, PortraitKind.Smith,
            O("mine.sasha.part2", new NarrativeEffect { ConsumeItem = ItemType.SpareParts }, req: "sq:tinker.parts"),
            O("mine.sasha.brakes", forbid: Flags.RedCable),
            End());
        Talk("mine.sasha.part2", sasha, PortraitKind.Smith,
            O("mine.sasha.done", new NarrativeEffect
            {
                ConsumeItem = ItemType.SpareParts, FinishQuest = "tinker.parts", Repair = 40f, SetFlag = Flags.ArmorPlated,
                SetFlag2 = Flags.RedCable, Trust = 10
            }),
            End());
        Talk("mine.sasha.done", sasha, PortraitKind.Smith);
        Add(new NarrativeNode("mine.sasha.brakes", sasha, PortraitKind.Smith) { OnEnter = new NarrativeEffect { SetFlag = Flags.RedCable, Trust = 3 } });
        SpotRest("mine.workshop.sasha", true, "mine.sasha.rest", "mine.sasha.1");

        Read("mine.blueprint.1", "doc.absorber", LoreKind.Document, 1);
        Look("mine.blueprint.rest");
        SpotRest("mine.workshop.blueprint", false, "mine.blueprint.rest", "mine.blueprint.1");

        Look("mine.sfrozen.1");
        Read("mine.sfrozen.2", "tape.miner", LoreKind.Tape, 1);
        Spot("mine.shelter.frozen", false, "mine.sfrozen.1", "mine.sfrozen.2");
    }

    // ==================================================================
    // Son Fener Durağı: yedek vagon, istasyon şefinin kulübesi (Edvard), bekleme salonu
    // ==================================================================

    private static void BuildLastLantern()
    {
        const string keeper = "spk.keeper";

        Look("ghost.wagon.1");
        LookDo("ghost.wagon.2", new NarrativeEffect { Loot = 1, Sanity = -5 });
        Spot("ghost.wagon", false, "ghost.wagon.1", "ghost.wagon.2");

        // Edvard: yirmi yıldır son tren için feneri yakıyor. Marta buradan geçti ve Arthur'a bir makara bıraktı.
        Talk("ghost.keeper.1", keeper, PortraitKind.LanternKeeper,
            O("ghost.keeper.2", T(10)),
            O("ghost.keeper.2b", T(-5)),
            O("ghost.keeper.marta", T(3)));
        TalkNext("ghost.keeper.2", keeper, PortraitKind.LanternKeeper, "ghost.keeper.3");
        TalkNext("ghost.keeper.2b", keeper, PortraitKind.LanternKeeper, "ghost.keeper.3");
        TalkNext("ghost.keeper.marta", keeper, PortraitKind.LanternKeeper, "ghost.keeper.3");
        Talk("ghost.keeper.3", keeper, PortraitKind.LanternKeeper,
            End(new NarrativeEffect { StartQuest = "keeper.tapes", Trust = 5 }),
            End(new NarrativeEffect { Sanity = 5, Trust = -5 }));
        Talk("ghost.keeper.rest", keeper, PortraitKind.LanternKeeper,
            O("ghost.keeper.play", new NarrativeEffect { FinishQuest = "keeper.tapes", Trust = 15 },
                req: "doc:tape.ghost&doc:doc.ticket", forbid: "sqdone:keeper.tapes"),
            O("ghost.keeper.night", forbid: "seen:ghost.keeper.night"),
            End());
        Doc("tape.marta", LoreKind.Tape, 1, "ghost.keeper.play");
        Add(new NarrativeNode("ghost.keeper.play", null, PortraitKind.Arthur)
        {
            OnEnter = new NarrativeEffect { Doc = "tape.marta", Sanity = 25, SetFlag = Flags.MartaReel, SetFlag2 = Flags.GhostTape }
        });
        Talk("ghost.keeper.night", keeper, PortraitKind.LanternKeeper);
        SpotRest("ghost.radio.keeper", true, "ghost.keeper.rest", "ghost.keeper.1");

        Read("ghost.atape.1", "tape.ghost", LoreKind.Tape, 1);
        Look("ghost.atape.rest");
        SpotRest("ghost.radio.tape", false, "ghost.atape.rest", "ghost.atape.1");

        LookDo("ghost.stat.1", new NarrativeEffect { Sanity = -5 });
        Look("ghost.stat.2");
        Spot("ghost.shelter.statues", false, "ghost.stat.1", "ghost.stat.2");

        Read("ghost.ticket2.1", "doc.ticket", LoreKind.Document, 1);
        Look("ghost.ticket2.rest");
        SpotRest("ghost.shelter.ticket", false, "ghost.ticket2.rest", "ghost.ticket2.1");
    }

    // ==================================================================
    // Kuzgun Geçidi: İlk Dinleyenler, yemekhane (Vesna'nın geçmişi), keşişin mağarası (Onbaşı Pell), gizli zula
    // ==================================================================

    private static void BuildRavenPass()
    {
        const string vesna = "spk.priestess", pell = "spk.hermit";

        Look("cult.frozen.1");
        Look("cult.frozen.2");
        Spot("cult.frozen", false, "cult.frozen.1", "cult.frozen.2");

        LookDo("cult.cache.1", new NarrativeEffect
        {
            Fuel = 40f, Repair = 15f, Items = new[] { ItemType.ThermalCoal, ItemType.SpareParts }, FinishQuest = "hermit.cache"
        });
        Look("cult.cache.rest");
        SpotRest("cult.cache", false, "cult.cache.rest", "cult.cache.1");

        Talk("cult.vesna.1", vesna, PortraitKind.Priestess,
            O("cult.vesna.2"),
            O("cult.vesna.3", T(8)),
            End(T(-15)));
        TalkNext("cult.vesna.2", vesna, PortraitKind.Priestess, "cult.vesna.3b");
        TalkNext("cult.vesna.3", vesna, PortraitKind.Priestess, "cult.vesna.3b");
        Talk("cult.vesna.3b", vesna, PortraitKind.Priestess,
            O("cult.vesna.4", new NarrativeEffect { Trust = 5, Aura = -3 }),
            O("cult.vesna.5", new NarrativeEffect { Aura = 5, Sanity = -5 }));
        Talk("cult.vesna.4", vesna, PortraitKind.Priestess);
        Talk("cult.vesna.5", vesna, PortraitKind.Priestess);
        Talk("cult.vesna.rest", vesna, PortraitKind.Priestess,
            O("cult.vesna.source", T(5), req: "doc:doc.scripture", forbid: "seen:cult.vesna.source"),
            End(new NarrativeEffect { Sanity = 5, SetFlag = "cult.bread" }, forbid: "cult.bread"),
            End());
        Talk("cult.vesna.source", vesna, PortraitKind.Priestess);
        SpotRest("cult.tavern.vesna", true, "cult.vesna.rest", "cult.vesna.1");

        Read("cult.script.1", "doc.scripture", LoreKind.Document, 1);
        Look("cult.script.rest");
        SpotRest("cult.tavern.scripture", false, "cult.script.rest", "cult.script.1");

        // Onbaşı Pell: 9. Müfreze'nin keşif eri. AURORA'nın açıldığı gece uplink kablosunu kesmeye gönderilmişti.
        Talk("cult.hermit.1", pell, PortraitKind.Hermit,
            O("cult.hermit.2", T(10)),
            O("cult.hermit.2b", T(-5)));
        TalkNext("cult.hermit.2", pell, PortraitKind.Hermit, "cult.hermit.3");
        TalkNext("cult.hermit.2b", pell, PortraitKind.Hermit, "cult.hermit.3");
        Talk("cult.hermit.3", pell, PortraitKind.Hermit,
            End(new NarrativeEffect { Trust = 8, Sanity = 5 }),
            End(T(-10)),
            O("cult.hermit.kask", T(4)));
        Talk("cult.hermit.kask", pell, PortraitKind.Hermit);
        Talk("cult.hermit.rest", pell, PortraitKind.Hermit,
            O("cult.hermit.map", new NarrativeEffect { StartQuest = "hermit.cache" },
                req: "trust:hermit>=60", forbid: "sq:hermit.cache|sqdone:hermit.cache"),
            O("cult.hermit.notyet", req: "trust:hermit<60"),
            O("cult.hermit.kask", forbid: "seen:cult.hermit.kask"),
            End());
        Talk("cult.hermit.map", pell, PortraitKind.Hermit);
        Talk("cult.hermit.notyet", pell, PortraitKind.Hermit);
        SpotRest("cult.shelter.hermit", true, "cult.hermit.rest", "cult.hermit.1");

        Read("cult.wall.1", "doc.det9", LoreKind.Document, 1);
        Look("cult.wall.rest");
        SpotRest("cult.shelter.wall", false, "cult.wall.rest", "cult.wall.1");
    }

    // ==================================================================
    // Tuzla Karakolu: donmuş vagon, 9. Müfreze komuta noktası (Madde 7), tuz sığınağı (Dunya), sinyal kulübesi (Kask'ın kaseti)
    // ==================================================================

    private static void BuildTuzla()
    {
        const string dunya = "spk.salt";

        Look("outpost.wagon.1");
        Read("outpost.wagon.2", "doc.conductor", LoreKind.Document, 1);
        Spot("outpost.wagon", false, "outpost.wagon.1", "outpost.wagon.2");

        Read("outpost.log.1", "outpost.log", LoreKind.Document, 1);
        ReadWith("outpost.log.2", "doc.clause", LoreKind.Document, 1, new NarrativeEffect { SetFlag = Flags.KaskClause });
        Spot("outpost.garrison.log", false, "outpost.log.1", "outpost.log.2");

        LookDo("outpost.armory2.1", new NarrativeEffect { Repair = 30f, Fuel = 25f, Items = new[] { ItemType.SpareParts, ItemType.ThermalCoal } });
        Look("outpost.armory2.rest");
        SpotRest("outpost.garrison.armory", false, "outpost.armory2.rest", "outpost.armory2.1");

        // Dunya: on iki kişi, bir soba ve ateşler içinde bir çocuk. Yan görev: ilaç
        Add(new NarrativeNode("outpost.salt.1", dunya, PortraitKind.SaltWorker,
            O("outpost.salt.give", new NarrativeEffect { ConsumeItem = ItemType.Medkit, Trust = 25, Karma = 15 }),
            O("outpost.salt.no", T(-8)),
            O("outpost.salt.trade", T(-3)))
        { OnEnter = new NarrativeEffect { StartQuest = "salt.medicine" } });
        Add(new NarrativeNode("outpost.salt.give", dunya, PortraitKind.SaltWorker)
            { OnEnter = new NarrativeEffect { Rations = 3, FinishQuest = "salt.medicine" } });
        Talk("outpost.salt.no", dunya, PortraitKind.SaltWorker);
        Talk("outpost.salt.trade", dunya, PortraitKind.SaltWorker,
            O("outpost.salt.give", new NarrativeEffect { ConsumeItem = ItemType.Medkit, Trust = 10, Karma = 5 }),
            End(T(-5)));
        Talk("outpost.salt.rest", dunya, PortraitKind.SaltWorker,
            O("outpost.salt.give", new NarrativeEffect { ConsumeItem = ItemType.Medkit, Trust = 20, Karma = 10 },
                req: "sq:salt.medicine"),
            O("outpost.salt.boy", req: "sqdone:salt.medicine"),
            End());
        Talk("outpost.salt.boy", dunya, PortraitKind.SaltWorker);
        SpotRest("outpost.shelter.salt", true, "outpost.salt.rest", "outpost.salt.1");

        Read("outpost.rtape.1", "tape.kask", LoreKind.Tape, 1);
        Look("outpost.rtape.rest");
        SpotRest("outpost.radio.tape", false, "outpost.rtape.rest", "outpost.rtape.1");
    }

    // ==================================================================
    // Güney Kapısı Garnizonu: Yüzbaşı Kaya'nın odası, sinyal kulesi (Asteğmen Petra'nın sayı istasyonu), kantin
    // ==================================================================

    private static void BuildSouthGate()
    {
        const string kaya = "spk.officer", petra = "spk.signal";

        Talk("mil.kaya.1", kaya, PortraitKind.Officer,
            O("mil.kaya.2", T(3)),
            O("mil.kaya.3", new NarrativeEffect { Trust = 10, Karma = 5 }),
            End(T(-15)));
        Talk("mil.kaya.2", kaya, PortraitKind.Officer, End(T(5)));
        Add(new NarrativeNode("mil.kaya.3", kaya, PortraitKind.Officer) { OnEnter = new NarrativeEffect { SetFlag = Flags.GarrisonAlly } });
        Talk("mil.kaya.rest", kaya, PortraitKind.Officer,
            O("mil.kaya.order", T(6), req: "doc:doc.order7", forbid: "seen:mil.kaya.order"),
            End());
        Talk("mil.kaya.order", kaya, PortraitKind.Officer,
            End(new NarrativeEffect { Trust = 15, Sanity = 10 }),
            End(T(3)));
        SpotRest("mil.garrison.kaya", true, "mil.kaya.rest", "mil.kaya.1");

        Read("mil.orders.1", "doc.order7", LoreKind.Document, 1);
        Look("mil.orders.rest");
        SpotRest("mil.garrison.orders", false, "mil.orders.rest", "mil.orders.1");

        // Sayı istasyonu bulmacası: "7-1-1-2-9" = Madde 7 ve perondaki 4.112 kişi. Belgeleri okuyan çözer.
        Talk("mil.petra.1", petra, PortraitKind.Quartermaster,
            O("mil.petra.puzzle", new NarrativeEffect { StartQuest = "signal.code" }),
            End(T(-5)));
        Talk("mil.petra.puzzle", petra, PortraitKind.Quartermaster,
            O("mil.petra.solved", new NarrativeEffect { FinishQuest = "signal.code", Trust = 15, SetFlag = Flags.HqCodebook, Aura = 5 },
                req: "doc:doc.clause|doc:doc.poster|doc:doc.manifest"),
            O("mil.petra.wrong", T(-3)),
            End());
        Talk("mil.petra.solved", petra, PortraitKind.Quartermaster);
        Talk("mil.petra.wrong", petra, PortraitKind.Quartermaster);
        Talk("mil.petra.rest", petra, PortraitKind.Quartermaster,
            O("mil.petra.puzzle", req: "sq:signal.code"),
            End());
        SpotRest("mil.radio.signal", true, "mil.petra.rest", "mil.petra.1");

        Read("mil.intercept.1", "tape.morrow", LoreKind.Tape, 1);
        Look("mil.intercept.rest");
        SpotRest("mil.radio.intercept", false, "mil.intercept.rest", "mil.intercept.1");

        Look("mil.letters.1");
        LookDo("mil.letters.2", new NarrativeEffect { Morale = 5 });
        Spot("mil.tavern.letters", false, "mil.letters.1", "mil.letters.2");
    }

    // ==================================================================
    // Bölüm 10 dönüm noktası: Sessiz Karargah'ın komuta sığınağı, AURA'nın ilk protokol belgeleri
    // ==================================================================

    private static void BuildHqBunker()
    {
        Read("hq.proto.1", "doc.proto0", LoreKind.Document, 1);
        Read("hq.proto.2", "doc.proto1", LoreKind.Document, 1);
        LookDo("hq.proto.3", new NarrativeEffect { SetFlag = Flags.ProtocolFound, Sanity = -5 });
        Spot("hq.bunker.protocol", false, "hq.proto.1", "hq.proto.2", "hq.proto.3");

        Read("hq.btape.1", "tape.morozov", LoreKind.Tape, 1);
        Look("hq.btape.rest");
        SpotRest("hq.bunker.tape", false, "hq.btape.rest", "hq.btape.1");

        Read("hq.tags.1", "hq.tags", LoreKind.Document, 1,
            End(new NarrativeEffect { FinishQuest = "greta.son", Karma = 10, Sanity = -5, Npc = "greta", Trust = 20 }, req: "sq:greta.son"),
            End(new NarrativeEffect { FinishQuest = "greta.son", Sanity = 5 }, req: "sq:greta.son"),
            End());
        Look("hq.tags.rest");
        SpotRest("hq.bunker.tags", false, "hq.tags.rest", "hq.tags.1");
    }

    // ==================================================================
    // Bölüm 31: AURORA Enstitüsü'nde müdürün laboratuvarı
    // ==================================================================

    private static void BuildInstituteLab()
    {
        Read("lab.rhee.1", "tape.rhee", LoreKind.Tape, 1);
        Look("lab.rhee.rest");
        SpotRest("lab.inner.rhee", false, "lab.rhee.rest", "lab.rhee.1");

        ReadWith("lab.kask.1", "doc.kaskmemo", LoreKind.Document, 1, new NarrativeEffect { SetFlag = Flags.KaskClause });
        Look("lab.kask.rest");
        SpotRest("lab.inner.kask", false, "lab.kask.rest", "lab.kask.1");

        ReadWith("lab.photo.1", "doc.labphoto", LoreKind.Document, 1, new NarrativeEffect { Sanity = -5 });
        Look("lab.photo.rest");
        SpotRest("lab.inner.photo", false, "lab.photo.rest", "lab.photo.1");
    }

    // ==================================================================
    // Sürüş anları: hatıralar ve dönüm noktaları (5 Cam Raylar, 20 Mahkeme, 30 Yankı, 35 Volkova)
    // ==================================================================

    private static void BuildDeepBeats1()
    {
        for (int i = 1; i <= 9; i++) Memory($"mem.{i}", 1, new NarrativeEffect { Sanity = i is 5 or 6 ? -8 : 4 });

        // Bölüm 5: Cam Raylar'ın köprüsünde yirmi yıldır duran yolcu treni
        Look("glass.1",
            O("glass.2"),
            End(new NarrativeEffect { Sanity = 5, Fuel = -8f }));
        Add(new NarrativeNode("glass.2", null, PortraitKind.Arthur,
            O("glass.3"),
            O("glass.4")) { OnEnter = new NarrativeEffect { Sanity = -10 } });
        Read("glass.3", "doc.glasslog", LoreKind.Document, 1,
            End(new NarrativeEffect { Damage = 8f, SetFlag = "glass.cleared" }),
            End(new NarrativeEffect { Fuel = -12f, Karma = 5 }));
        Add(new NarrativeNode("glass.4", null, PortraitKind.Arthur,
            End(new NarrativeEffect { Damage = 8f, SetFlag = "glass.cleared" }),
            End(new NarrativeEffect { Fuel = -12f, Karma = 5 })) { OnEnter = new NarrativeEffect { Loot = 1, Rations = 2 } });

        // Bölüm 20: Aysız Hat'ta fırtına ortasında mahkeme. Sanık: en şüpheli yolcu ({p}).
        Add(new NarrativeNode("trial.1", null, PortraitKind.Arthur,
            O("trial.2"),
            O("trial.calm", new NarrativeEffect { Morale = -10 })) { OnEnter = new NarrativeEffect { SetFlag = Flags.TrialHeld } });
        Look("trial.calm", End(new NarrativeEffect { Npc = "{p}", Trust = 10 }));
        Talk("trial.2", "{p}", PortraitKind.Arthur,
            O("trial.3", new NarrativeEffect { Npc = "{p}", Trust = -5 }),
            O("trial.3", new NarrativeEffect { Npc = "{p}", Trust = 5 }));
        Look("trial.3",
            O("trial.vote", new NarrativeEffect { Npc = "{p}", Consequence = "verdict.vote" }),
            End(new NarrativeEffect { Npc = "{p}", Consequence = "verdict.exile", Morale = 5 }),
            End(new NarrativeEffect { Npc = "{p}", Consequence = "verdict.execute", Karma = -15 }),
            End(new NarrativeEffect { Npc = "{p}", Consequence = "verdict.acquit", Morale = 5 }));
        Look("trial.vote");

        // Bölüm 30: Çığ Geçidi'nde telsiz kendiliğinden açılır. Marta'nın sesi: AURA'nın ilk ses taklidi.
        Add(new NarrativeNode("echo.1", null, PortraitKind.Arthur,
            O("echo.2", new NarrativeEffect { Sanity = -15, Aura = 5 }),
            O("echo.off", new NarrativeEffect { Sanity = -5 }),
            O("echo.3", new NarrativeEffect { Aura = 3 })) { OnEnter = new NarrativeEffect { SetFlag = Flags.EchoHeard } });
        var echoChoices = new[]
        {
            O("echo.4", new NarrativeEffect { Sanity = 15, Aura = -5 }, req: Flags.MartaReel + "|" + Flags.HomeLetter),
            O("echo.5", new NarrativeEffect { Sanity = -10, Aura = 5 }),
            O("echo.off", new NarrativeEffect { Damage = 5f, Sanity = 5 })
        };
        Talk("echo.2", "spk.mirror", PortraitKind.Aura, echoChoices);
        Talk("echo.3", "spk.mirror", PortraitKind.Aura, echoChoices);
        Look("echo.4");
        Talk("echo.5", "spk.mirror", PortraitKind.Aura);
        Look("echo.off");

        // Bölüm 35: Beyaz Sessizlik'te Binbaşı Volkova'nın donmuş komuta vagonu ve itirafı
        Look("volkova.1",
            O("volkova.2"),
            End(new NarrativeEffect { Sanity = -5 }));
        Add(new NarrativeNode("volkova.2", null, PortraitKind.Arthur, O("volkova.3"))
            { OnEnter = new NarrativeEffect { SetFlag = Flags.VolkovaFound, Sanity = -10 } });
        Read("volkova.3", "tape.volkova", LoreKind.Tape, 1,
            End(new NarrativeEffect { SetFlag = "volkova.map", Aura = -5, Karma = 5 }),
            End(new NarrativeEffect { Sanity = 10 }),
            End(new NarrativeEffect { Sanity = -5 }));
    }

    // ==================================================================
    // Yolcularla serbest sohbet ([J]): genel ağ ({p}) ve yolcuya özel konuşmalar
    // ==================================================================

    private static void BuildPassengerTalk1()
    {
        Talk("talk.hub", "{p}", PortraitKind.Arthur,
            O("talk.hub.care", new NarrativeEffect { Npc = "{p}", Trust = 4, QuestMorale = 5 }),
            O("talk.hub.share", new NarrativeEffect { Npc = "{p}", Trust = 9, QuestMorale = 10, Rations = -1 }),
            O("talk.hub.accuse", new NarrativeEffect { Npc = "{p}", Trust = -10, Suspicion = 10 }),
            O("talk.hub.threat", new NarrativeEffect { Npc = "{p}", Trust = -18, QuestMorale = -10 }));
        Talk("talk.hub.care", "{p}", PortraitKind.Arthur);
        Talk("talk.hub.share", "{p}", PortraitKind.Arthur);
        Talk("talk.hub.accuse", "{p}", PortraitKind.Arthur);
        Talk("talk.hub.threat", "{p}", PortraitKind.Arthur);

        const string viktor = "p.viktor.name", bora = "p.bora.name", elena = "p.elena.name", mika = "p.mika.name";
        Talk("talk.viktor.1", viktor, PortraitKind.Viktor, O("talk.viktor.1a", T(10)), O("talk.viktor.1b", T(-6)));
        Talk("talk.viktor.1a", viktor, PortraitKind.Viktor);
        Talk("talk.viktor.1b", viktor, PortraitKind.Viktor);
        Talk("talk.viktor.2", viktor, PortraitKind.Viktor,
            O("talk.viktor.2a", new NarrativeEffect { Trust = 5, SetFlag = "receiver.cut", Damage = 4f }),
            O("talk.viktor.2b", new NarrativeEffect { Trust = 3, Aura = 3 }));
        Talk("talk.viktor.2a", viktor, PortraitKind.Viktor);
        Talk("talk.viktor.2b", viktor, PortraitKind.Viktor);

        Talk("talk.bora.1", bora, PortraitKind.Bora, O("talk.bora.1a", T(8)), O("talk.bora.1b", T(4)), O("talk.bora.1c", T(-15)));
        Talk("talk.bora.1a", bora, PortraitKind.Bora);
        Talk("talk.bora.1b", bora, PortraitKind.Bora);
        Talk("talk.bora.1c", bora, PortraitKind.Bora);
        Talk("talk.bora.2", bora, PortraitKind.Bora,
            O("talk.bora.2a", new NarrativeEffect { Trust = 5, Aura = -2 }),
            O("talk.bora.2a", T(8)));
        Talk("talk.bora.2a", bora, PortraitKind.Bora);

        Talk("talk.elena.1", elena, PortraitKind.Elena,
            O("talk.elena.1a", new NarrativeEffect { Trust = 5, Sanity = 5 }),
            O("talk.elena.1b", new NarrativeEffect { Trust = 5, Aura = 3, Sanity = -5 }));
        Talk("talk.elena.1a", elena, PortraitKind.Elena);
        Talk("talk.elena.1b", elena, PortraitKind.Elena);

        Talk("talk.mika.1", mika, PortraitKind.Mika,
            O("talk.mika.1a", new NarrativeEffect { Trust = 8, QuestMorale = 10 }),
            O("talk.mika.1b", new NarrativeEffect { Trust = 4, Sanity = 5 }));
        Talk("talk.mika.1a", mika, PortraitKind.Mika);
        Talk("talk.mika.1b", mika, PortraitKind.Mika);
    }

    // ==================================================================
    // AURA'nın ses taklitleri (Episode 1, Bölüm 30 sonrası: "bilinmeyen sinyal")
    // ==================================================================

    private static void BuildPhantoms1()
    {
        const string voice = "spk.mirror";
        Talk("ph.marta.1", voice, PortraitKind.Aura,
            O("ph.marta.cut", new NarrativeEffect { Sanity = 8, Aura = -4 }),
            O("ph.marta.yes", new NarrativeEffect { Sanity = -12, Aura = 4 }));
        Look("ph.marta.cut");
        Talk("ph.marta.yes", voice, PortraitKind.Aura);
        Talk("ph.thomas.1", voice, PortraitKind.Aura,
            O("ph.thomas.cut", new NarrativeEffect { Sanity = 8, Aura = -3 }),
            O("ph.thomas.yes", new NarrativeEffect { Sanity = -8, Aura = 3 }));
        Look("ph.thomas.cut");
        Look("ph.thomas.yes");
        Look("ph.rest1");
        SpotRest("ph.voices1", true, "ph.rest1", "ph.marta.1", "ph.thomas.1");
    }

    /// <summary>Episode 1 telsiz yayınları: dinlenince arşive kaset olarak girer (metin "nv.bc.1.*").</summary>
    private static void BuildBroadcasts1()
    {
        foreach (string id in new[] { "1", "7", "13", "19", "28", "36", "g1", "g2", "g3", "g4", "g5", "g6" })
            Doc($"bc.1.{id}", LoreKind.Tape, 1, $"bc.1.{id}");
    }
}

/// <summary>Episode 1 derin anlatı anları.</summary>
public static partial class DeepBeats
{
    private static StoryBeat[] BuildEpisode1() => new[]
    {
        // Hatıralar: Arthur'un evliliği, 9. Müfreze, Sofi, Kask'ın Madde 7'si, Sefer 0 emri, Kuzeyhavn peronu...
        new StoryBeat(3, 0.30f, "mem.1"),
        new StoryBeat(6, 0.40f, "mem.2"),
        new StoryBeat(8, 0.35f, "mem.3"),
        new StoryBeat(12, 0.30f, "mem.4"),
        new StoryBeat(16, 0.45f, "mem.5"),
        new StoryBeat(22, 0.30f, "mem.6"),
        new StoryBeat(27, 0.40f, "mem.7"),
        new StoryBeat(33, 0.25f, "mem.8"),
        new StoryBeat(38, 0.40f, "mem.9"),

        // Dönüm noktaları (10, 15, 25 ve 40 kendi sahneleri/anlarıyla zaten birer dönüm noktası)
        new StoryBeat(5, 0.55f, "glass.1", StopTrain: true),
        new StoryBeat(20, 0.45f, "trial.1", StopTrain: true) { MinPassengers = 2, Accused = true },
        new StoryBeat(30, 0.50f, "echo.1"),
        new StoryBeat(35, 0.22f, "volkova.1", StopTrain: true),
    };
}
