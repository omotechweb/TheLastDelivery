using TheLastDelivery.Items;
using TheLastDelivery.UI;

namespace TheLastDelivery.Story;

/// <summary>
/// Sürüş sırasında, bölümün belirli bir noktasında açılan hikâye anı. Oyun zamanı diyalog boyunca durur.
/// Her an hikâye başına bir kez oynanır (ilk düğümü "görüldü" olunca).
/// </summary>
/// <param name="Chapter">Hikâye modu bölüm numarası.</param>
/// <param name="At">Bölüm içindeki konum (0 = başı, 1 = sonu).</param>
/// <param name="Node">İlk diyalog düğümü.</param>
/// <param name="StopTrain">Tren durur (barikat, karşılaşma); yoksa hızını korur.</param>
public sealed record StoryBeat(int Chapter, float At, string Node, bool StopTrain = false)
{
    public string RequireFlag { get; init; }
    public int MinPassengers { get; init; }

    /// <summary>Mahkeme anı: en şüpheli yolcu sanık olur (yoksa an atlanır).</summary>
    public bool Accused { get; init; }
}

public static class StoryBeats
{
    /// <summary>
    /// Episode 1'in sürüş bölümlerine serpiştirilmiş anlar. Kurt Baba (Komutan Morozov) yayından tanınmaz bir
    /// sesle başlar (Bölüm 7, telsiz), Bölüm 15'te tünelde yüz yüze gelir, Bölüm 34'te hesaplaşma olur.
    /// </summary>
    public static readonly StoryBeat[] Episode1 =
    {
        new(2, 0.55f, "pkg.1"),
        new(15, 0.35f, "wolf.p1.1", StopTrain: true),
        new(23, 0.45f, "bridge.1", StopTrain: true) { RequireFlag = Flags.CultDefied },
        new(29, 0.40f, "hunger.1") { MinPassengers = 2 },
        new(34, 0.12f, "wolf.c.1", StopTrain: true),
        new(35, 0.50f, "lull.1"),
    };

    /// <summary>
    /// Episode 2'nin sürüş anları. Her biri finalde AURA'ya söylenebilecek bir "umut argümanı" doğurur:
    /// AURA her şeyi hesaplar ama yolda yaşananları ölçemez. Bazıları Episode 1 kararlarına bağlıdır
    /// (köprüdeki çocuk, Kurt Ata'yla ateşkes ya da düşmanlık, Sofi'nin bulunması).
    /// </summary>
    public static readonly StoryBeat[] Episode2 =
    {
        new(1, 0.55f, "e2b.heart.1"),
        new(5, 0.45f, "e2b.name.1"),
        new(13, 0.40f, "e2b.convoy.1", StopTrain: true),
        new(18, 0.50f, "e2b.acolyte.1", StopTrain: true),
        new(23, 0.35f, "e2b.wolf.1") { RequireFlag = "ep1:" + Flags.WolfTruce },
        new(23, 0.35f, "e2b.wolfx.1") { RequireFlag = "ep1:" + Flags.WolfEnemy },
        new(29, 0.50f, "e2b.fire.1", StopTrain: true) { MinPassengers = 1 },
        new(33, 0.45f, "e2b.oracle.1", StopTrain: true) { RequireFlag = Flags.E2NullBroken },
        new(33, 0.45f, "e2b.trap.1") { RequireFlag = Flags.E2NullInside },
        new(35, 0.45f, "e2b.sofi.1") { RequireFlag = "ep1:" + Flags.SofiKnown },
        new(39, 0.30f, "e2b.door.1"),
    };

    /// <summary>Bölümün bütün anları: özgün anlar ve derin anlatı anları (hatıralar, dönüm noktaları, AURA'nın sorguları).</summary>
    private static System.Collections.Generic.IEnumerable<StoryBeat> Enumerate(int episode)
    {
        foreach (var beat in episode == 2 ? Episode2 : Episode1) yield return beat;
        foreach (var beat in episode == 2 ? DeepBeats.Episode2 : DeepBeats.Episode1) yield return beat;
    }

    /// <summary>Bölümde henüz yaşanmamış anların konumları (mini haritada "!" işareti için).</summary>
    public static System.Collections.Generic.IEnumerable<float> Upcoming(int episode, int chapter)
    {
        foreach (var beat in Enumerate(episode))
            if (beat.Chapter == chapter && !Narrative.Seen(beat.Node) && (beat.RequireFlag == null || Narrative.Has(beat.RequireFlag)))
                yield return beat.At;
    }

    /// <summary>Şu an açılması gereken an (yoksa null).</summary>
    public static StoryBeat Due(int episode, int chapter, float progress, int passengers)
    {
        foreach (var beat in Enumerate(episode))
        {
            if (beat.Chapter != chapter || progress < beat.At) continue;
            if (Narrative.Seen(beat.Node)) continue;
            if (beat.RequireFlag != null && !Narrative.Has(beat.RequireFlag)) continue;
            if (passengers < beat.MinPassengers) continue;
            return beat;
        }
        return null;
    }
}

public static partial class NarrativeData
{
    private static void BuildStoryBeats()
    {
        BuildPackageBeat();
        BuildWolfParley();
        BuildBridgeBeat();
        BuildHungerBeat();
        BuildWolfConfrontation();
        BuildLullabyBeat();
        BuildEpisode2Beats();
    }

    // ==================================================================
    // Episode 2 sürüş anları
    // ==================================================================

    private static void BuildEpisode2Beats()
    {
        const string aura = "spk.aura", wolf = "spk.wolf";

        // Bölüm 1: kargodaki Kalp, kara trenin kırmızı gözüyle aynı ritimde atıyor
        Look("e2b.heart.1",
            O("e2b.heart.touch", new NarrativeEffect { Sanity = -10, SetFlag = Flags.E2HeartHeard }),
            O("e2b.heart.wrap", new NarrativeEffect { Sanity = 5 }));
        Look("e2b.heart.touch");
        Look("e2b.heart.wrap");

        // Bölüm 5: AURA, Arthur'a Kuzeyhavn peronunun kaydını ve bir sayıyı okur
        Talk("e2b.name.1", aura, PortraitKind.Aura,
            O("e2b.name.answer", new NarrativeEffect { SetFlag = Flags.E2Answered, Karma = 5, Sanity = -5 }),
            O("e2b.name.cut", new NarrativeEffect { Sanity = 5 }),
            O("e2b.name.nova", new NarrativeEffect { Sanity = 10, Morale = 5 }, req: "aboard:nova"));
        Talk("e2b.name.answer", aura, PortraitKind.Aura);
        Look("e2b.name.cut");
        Talk("e2b.name.nova", "p.nova.name", PortraitKind.Nova);

        // Bölüm 13: kış uykusu konvoyunda raydan çıkmış bir vagon, çatlak bir tüp ve içeriden vuran küçük bir el
        Add(new NarrativeNode("e2b.convoy.1", null, PortraitKind.Arthur) { Next = "e2b.convoy.2" });
        Look("e2b.convoy.2",
            O("e2b.convoy.open", new NarrativeEffect { Rations = -1, Heat = -12f, Karma = 15, SetFlag = Flags.E2Child }),
            O("e2b.convoy.yusuf", new NarrativeEffect { Rations = -1, Karma = 15, SetFlag = Flags.E2Child }, req: "aboard:yusuf"),
            O("e2b.convoy.leave", new NarrativeEffect { Karma = -10, Sanity = -15 }));
        Look("e2b.convoy.open");
        Talk("e2b.convoy.yusuf", "p.yusuf.name", PortraitKind.Yusuf);
        Look("e2b.convoy.leave");

        // Bölüm 18: raylara diz çökmüş müritler. Episode 1'de köprüde kurtarılan çocuk (Ilka) aralarında.
        Look("e2b.acolyte.1",
            O("e2b.acolyte.2"),
            O("e2b.acolyte.push", new NarrativeEffect { Damage = 10f, Sanity = -10 }));
        Look("e2b.acolyte.2",
            O("e2b.acolyte.ilka", new NarrativeEffect { SetFlag = Flags.E2AcolyteFriend, Karma = 10, Sanity = 10 },
                req: "ep1:" + Flags.AcolyteSaved),
            O("e2b.acolyte.truth", new NarrativeEffect { SetFlag = Flags.E2AcolyteDoubt, Karma = 5 }),
            O("e2b.acolyte.go"));
        Talk("e2b.acolyte.ilka", "spk.ilka", PortraitKind.Acolyte);
        Look("e2b.acolyte.truth");
        Look("e2b.acolyte.go");
        Look("e2b.acolyte.push");

        // Bölüm 23 (Episode 1'de ateşkes): Morozov'un Kızıl Birlik'i filonun önüne atılır
        Talk("e2b.wolf.1", wolf, PortraitKind.WolfFather,
            O("e2b.wolf.accept", new NarrativeEffect { Repair = 25f, Fuel = 15f, SetFlag = Flags.E2WolfHelp }),
            O("e2b.wolf.refuse", new NarrativeEffect { Karma = 5 }));
        Talk("e2b.wolf.accept", wolf, PortraitKind.WolfFather);
        Talk("e2b.wolf.refuse", wolf, PortraitKind.WolfFather);

        // Bölüm 23 (Episode 1'de düşmanlık): Morozov'un sesi bir Sentinel'in hoparlöründen. AURA onu da öğrenmiş.
        Add(new NarrativeNode("e2b.wolfx.1", wolf, PortraitKind.WolfFather) { Next = "e2b.wolfx.2" });
        Add(new NarrativeNode("e2b.wolfx.2", aura, PortraitKind.Aura) { OnEnter = new NarrativeEffect { Sanity = -10 } });

        // Bölüm 29: son insan istasyonu, ateş başında yolcular "sonra ne olacak?" diye soruyor
        Add(new NarrativeNode("e2b.fire.1", null, PortraitKind.Arthur) { Next = "e2b.fire.2" });
        Look("e2b.fire.2",
            O("e2b.fire.promise", new NarrativeEffect { Morale = 20, SetFlag = Flags.E2Promise }),
            O("e2b.fire.truth", new NarrativeEffect { Sanity = 10, Karma = 5, SetFlag = Flags.E2Truth }),
            O("e2b.fire.ask", new NarrativeEffect { Morale = 10, Sanity = 10, SetFlag = Flags.E2Truth }));
        Look("e2b.fire.promise");
        Look("e2b.fire.truth");
        Look("e2b.fire.ask");

        // Bölüm 33 (tarikat dağıldıysa): Kâhin raylarda tek başına. AURA'nın yanıldığını kendi kulağıyla duymak istiyor.
        const string oracle = "spk.oracle";
        Add(new NarrativeNode("e2b.oracle.1", null, PortraitKind.Arthur) { Next = "e2b.oracle.2" });
        Talk("e2b.oracle.2", oracle, PortraitKind.Oracle,
            O("e2b.oracle.ride", new NarrativeEffect { SetFlag = Flags.E2OracleRides, Karma = 10 }),
            O("e2b.oracle.kiril", new NarrativeEffect { SetFlag = Flags.E2OracleRides, SetFlag2 = Flags.E2KirilFaced, Karma = 10, Sanity = 5 },
                req: "aboard:kiril"),
            O("e2b.oracle.walk", new NarrativeEffect { Karma = -5 }));
        Talk("e2b.oracle.ride", oracle, PortraitKind.Oracle);
        Talk("e2b.oracle.kiril", "p.kiril.name", PortraitKind.Kiril);
        Talk("e2b.oracle.walk", oracle, PortraitKind.Oracle);

        // Bölüm 33 (NULL'a katılmış gibi yapıldıysa): krom kart hem anahtar hem tasma çıkar
        Talk("e2b.trap.1", oracle, PortraitKind.Oracle,
            O("e2b.trap.kiril", new NarrativeEffect { SetFlag = Flags.E2KirilFaced, Karma = 5 }, req: "aboard:kiril"),
            O("e2b.trap.nova", new NarrativeEffect { Sanity = 5 }, req: "aboard:nova"),
            O("e2b.trap.ram", new NarrativeEffect { Damage = 20f, Heat = -8f }));
        Talk("e2b.trap.kiril", "p.kiril.name", PortraitKind.Kiril);
        Talk("e2b.trap.nova", "p.nova.name", PortraitKind.Nova);
        Look("e2b.trap.ram");

        // Bölüm 35 (Sofi bulunduysa): gerçek Sofi'nin sesi. Ayna odasındaki sahte sese karşı tek anahtar.
        Talk("e2b.sofi.1", "spk.sofi", PortraitKind.Sofi,
            O("e2b.sofi.love", new NarrativeEffect { Sanity = 15, SetFlag = Flags.E2SofiCall }),
            O("e2b.sofi.hide", new NarrativeEffect { Sanity = 5, Karma = 5, SetFlag = Flags.E2SofiCall }));
        Talk("e2b.sofi.love", "spk.sofi", PortraitKind.Sofi);
        Talk("e2b.sofi.hide", "spk.sofi", PortraitKind.Sofi);

        // Bölüm 39: son kapının önünde Arthur'un defteri
        Look("e2b.door.1",
            O("e2b.door.keep", new NarrativeEffect { Sanity = 10, SetFlag = Flags.E2LogKept }),
            O("e2b.door.burn", new NarrativeEffect { Heat = 15f, Sanity = 5 }));
        Look("e2b.door.keep");
        Look("e2b.door.burn");
    }

    // Bölüm 2: paket içeriden vuruyor. Erken bir gizem ve ilk ahlaki kural ("açma, sorma, durma").
    private static void BuildPackageBeat()
    {
        Look("pkg.1",
            O("pkg.open", new NarrativeEffect { SetFlag = Flags.PackagePeeked, Sanity = -10 }),
            O("pkg.leave", new NarrativeEffect { Sanity = 5 }));
        Look("pkg.open");
        Look("pkg.leave");
    }

    // Bölüm 15 "Derin Galeriler": Kurt Baba'nın barikatı. Henüz kim olduğunu söylemez; Arthur'u tanıdığını belli eder.
    private static void BuildWolfParley()
    {
        const string wolf = "spk.wolf";
        Add(new NarrativeNode("wolf.p1.1", null, PortraitKind.Arthur) { Next = "wolf.p1.2" });
        Talk("wolf.p1.2", wolf, PortraitKind.WolfFather,
            O("wolf.p1.3"),
            O("wolf.p1.4"),
            O("wolf.p1.bora", new NarrativeEffect { SetFlag = Flags.WolfBora, Morale = 10, Karma = 5 }, req: "aboard:bora"));
        Add(new NarrativeNode("wolf.p1.3", wolf, PortraitKind.WolfFather) { Next = "wolf.p1.5" });
        Add(new NarrativeNode("wolf.p1.4", wolf, PortraitKind.WolfFather) { Next = "wolf.p1.5" });
        Talk("wolf.p1.5", wolf, PortraitKind.WolfFather,
            O("wolf.p1.pay", new NarrativeEffect { Rations = -3, SetFlag = Flags.WolfPaid }),
            O("wolf.p1.pay", new NarrativeEffect { Credits = -60, SetFlag = Flags.WolfPaid }),
            O("wolf.p1.code", new NarrativeEffect { Karma = 5 }, req: Flags.HqCodebook),
            O("wolf.p1.ram", new NarrativeEffect { Damage = 25f, Heat = -10f, SetFlag = Flags.WolfRammed }));
        Talk("wolf.p1.pay", wolf, PortraitKind.WolfFather);
        Talk("wolf.p1.code", wolf, PortraitKind.WolfFather);
        Look("wolf.p1.ram");
        Talk("wolf.p1.bora", "p.bora.name", PortraitKind.Bora);
    }

    // Bölüm 23 "Buz Köprüsü" (tarikat reddedildiyse): civataları söken çocuk mürit.
    private static void BuildBridgeBeat()
    {
        Look("bridge.1",
            O("bridge.save", new NarrativeEffect { SetFlag = Flags.AcolyteSaved, Karma = 10, Heat = -12f }),
            O("bridge.whistle", new NarrativeEffect { Damage = 15f }));
        Look("bridge.save");
        Look("bridge.whistle");
    }

    // Bölüm 29 "Açlık": yolcular erzağın gerçeğini soruyor.
    private static void BuildHungerBeat()
    {
        Add(new NarrativeNode("hunger.1", null, PortraitKind.Arthur) { Next = "hunger.2" });
        Look("hunger.2",
            O("hunger.share", new NarrativeEffect { Rations = -2, Morale = 20, Karma = 5, SetFlag = Flags.HungerShared }),
            O("hunger.lock", new NarrativeEffect { Morale = -10 }),
            O("hunger.self", new NarrativeEffect { Sanity = -15, Morale = 15, Karma = 10, SetFlag = Flags.HungerShared }));
        Look("hunger.share");
        Look("hunger.lock");
        Look("hunger.self");
    }

    // Bölüm 34 "Son Pusu": hesaplaşma. Kurt Baba, karargahta günlüğü bulunan Komutan Morozov'dur;
    // Kuzey Liman'da listeyi Arthur'a veren subay odur. Kanıtlar, yolcular ve Sofi ipuçları yeni yollar açar.
    private static void BuildWolfConfrontation()
    {
        const string wolf = "spk.wolf";
        Add(new NarrativeNode("wolf.c.1", null, PortraitKind.Arthur) { Next = "wolf.c.2" });
        Talk("wolf.c.2", wolf, PortraitKind.WolfFather,
            O("wolf.c.who"),
            O("wolf.c.diary", req: "seen:hq.diary.3"),
            O("wolf.c.deal"));
        Add(new NarrativeNode("wolf.c.who", wolf, PortraitKind.WolfFather) { Next = "wolf.c.3" });
        Add(new NarrativeNode("wolf.c.diary", wolf, PortraitKind.WolfFather) { Next = "wolf.c.3" });
        Talk("wolf.c.3", wolf, PortraitKind.WolfFather,
            O("wolf.c.sofi", new NarrativeEffect { Sanity = 10 }, req: Flags.HomeLetter + "|" + Flags.GhostTape),
            O("wolf.c.proof", req: Flags.HqBroadcast),
            O("wolf.c.oskar", req: "aboard:oskar"),
            O("wolf.c.final"));
        Add(new NarrativeNode("wolf.c.sofi", wolf, PortraitKind.WolfFather) { Next = "wolf.c.final" });
        Add(new NarrativeNode("wolf.c.proof", wolf, PortraitKind.WolfFather) { Next = "wolf.c.final" });
        Add(new NarrativeNode("wolf.c.oskar", "p.oskar.name", PortraitKind.Oskar) { Next = "wolf.c.final" });
        Talk("wolf.c.final", wolf, PortraitKind.WolfFather,
            O("wolf.c.truce", new NarrativeEffect { SetFlag = Flags.WolfTruce, Karma = 20, Fuel = 20f }),
            O("wolf.c.deal"),
            O("wolf.c.enemy", new NarrativeEffect { SetFlag = Flags.WolfEnemy, Karma = 5 }));
        Talk("wolf.c.truce", wolf, PortraitKind.WolfFather);
        Talk("wolf.c.deal", wolf, PortraitKind.WolfFather,
            End(new NarrativeEffect { SetFlag = Flags.WolfDeal, Karma = -30, Fuel = 30f }),
            O("wolf.c.enemy", new NarrativeEffect { SetFlag = Flags.WolfEnemy }));
        Talk("wolf.c.enemy", wolf, PortraitKind.WolfFather);
    }

    // Bölüm 35 "Beyaz Sessizlik": fırtınanın içinden Marta'nın ninnisi (Bölüm 37'deki Kuzey Yıldızı sahnesine hazırlık).
    private static void BuildLullabyBeat()
    {
        Look("lull.1",
            O("lull.hum", new NarrativeEffect { Sanity = 20, SetFlag = Flags.LullabyHummed }),
            O("lull.off"));
        Look("lull.hum");
        Look("lull.off");
    }
}
