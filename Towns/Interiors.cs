using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TheLastDelivery.Story;

namespace TheLastDelivery.Towns;

/// <summary>Girilebilen bina türleri.</summary>
public enum InteriorKind { Tavern, Garrison, Workshop, Shelter, RadioRoom }

/// <summary>İç mekândaki bir nokta: inceleme/konuşma, çizilecek eşya ve (konuşmaysa) orada duran kişi.</summary>
public sealed record InteriorSpot(string SpotId, StructureKind Prop, int Variant = 0, PersonSpot Person = null)
{
    /// <summary>Yalnızca bu koşul sağlanınca görünür (ör. "doc:outpost.log" ya da "trust:hermit&gt;=60").</summary>
    public string RequireFlag { get; init; }
}

/// <summary>
/// Kasabaların ve hikâye sahnelerinin girilebilir binaları. Dışarıdaki Taverna/Tamirci/Sığınak hizmetleri kapıya dönüşür
/// (hizmet içerideki tezgâhta sürer); bazı yerleşimlere Karakol ve Telsiz Kulesi kapıları eklenir.
/// İç mekânlarda kasabanın liderleri (güven barlı diyalog ağları), belgeler, ses kasetleri ve buza kesmiş insanlar vardır.
/// Ayrıca dış mekâna çevresel hikâye noktaları (terk edilmiş vagonlar, donmuş heykeller) eklenir.
/// </summary>
public static class InteriorCatalog
{
    public const int Width = 1500;
    public const float ExitX = 160f;
    private static readonly float[] Slots = { 430f, 690f, 950f, 1210f };

    private static PersonSpot Who(Color coat, Color scarf) => new(0f, -1, coat, scarf);

    /// <summary>Yerleşimin içerik öneki ("trade", "mine", "hq"...).</summary>
    public static string TownKey(TownArchetype archetype) => archetype switch
    {
        TownArchetype.TradeHub => "trade",
        TownArchetype.MiningVillage => "mine",
        TownArchetype.GhostStation => "ghost",
        TownArchetype.IceCult => "cult",
        TownArchetype.AbandonedOutpost => "outpost",
        TownArchetype.MilitaryGate => "mil",
        TownArchetype.DroneOutpost => "drone",
        TownArchetype.HibernationFactory => "factory",
        TownArchetype.CyberChapel => "chapel",
        TownArchetype.Headquarters => "hq",
        TownArchetype.Institute => "lab",
        TownArchetype.SignalTower => "tower",
        _ => null
    };

    /// <summary>Dışarıdaki hizmet binasının iç mekânı (taverna → taverna, tamirci → atölye, sığınak → sığınak).</summary>
    public static InteriorKind? ForService(InteractableKind kind) => kind switch
    {
        InteractableKind.Tavern => InteriorKind.Tavern,
        InteractableKind.Mechanic => InteriorKind.Workshop,
        InteractableKind.Bunker => InteriorKind.Shelter,
        _ => null
    };

    /// <summary>Hizmet binası olmayan ek kapılar: (iç mekân, x, kart yüksekliği).</summary>
    public static IEnumerable<(InteriorKind Kind, float X, float PromptY)> ExtraDoors(TownArchetype archetype)
    {
        switch (archetype)
        {
            case TownArchetype.TradeHub: yield return (InteriorKind.Garrison, 920f, 350f); break;
            case TownArchetype.MiningVillage: yield return (InteriorKind.Shelter, 2760f, 350f); break;
            case TownArchetype.GhostStation:
                yield return (InteriorKind.Shelter, 2450f, 350f);
                yield return (InteriorKind.RadioRoom, 2760f, 340f);
                break;
            case TownArchetype.IceCult:
                yield return (InteriorKind.Tavern, 950f, 360f);
                yield return (InteriorKind.Shelter, 2560f, 360f);
                break;
            case TownArchetype.AbandonedOutpost:
                yield return (InteriorKind.Shelter, 950f, 360f);
                yield return (InteriorKind.Garrison, 1540f, 320f);
                yield return (InteriorKind.RadioRoom, 2680f, 360f);
                break;
            case TownArchetype.MilitaryGate:
                yield return (InteriorKind.Garrison, 900f, 320f);
                yield return (InteriorKind.RadioRoom, 2860f, 320f);
                break;
            case TownArchetype.DroneOutpost:
                yield return (InteriorKind.RadioRoom, 1500f, 350f);
                yield return (InteriorKind.Garrison, 2750f, 350f);
                break;
            case TownArchetype.HibernationFactory: yield return (InteriorKind.Shelter, 2750f, 350f); break;
            case TownArchetype.CyberChapel:
                yield return (InteriorKind.RadioRoom, 950f, 350f);
                yield return (InteriorKind.Shelter, 2700f, 350f);
                break;
            case TownArchetype.Headquarters: yield return (InteriorKind.Garrison, 1000f, 380f); break;
            case TownArchetype.Institute: yield return (InteriorKind.RadioRoom, 1300f, 360f); break;
            case TownArchetype.SignalTower: yield return (InteriorKind.RadioRoom, 2420f, 350f); break;
        }
    }

    /// <summary>Ek kapıların dışarıda çizilen binaları (yerleşimde zaten bina olmayan yerler için).</summary>
    public static IEnumerable<Placement> ExtraStructures(TownArchetype archetype)
    {
        switch (archetype)
        {
            case TownArchetype.GhostStation:
                yield return new Placement(StructureKind.Hut, 2450f, 1);
                yield return new Placement(StructureKind.Hut, 2760f);
                yield return new Placement(StructureKind.WagonWreck, 820f);
                break;
            case TownArchetype.TradeHub: yield return new Placement(StructureKind.FrozenFigure, 1565f); break;
            case TownArchetype.MiningVillage: yield return new Placement(StructureKind.FrozenFigure, 2600f, 1); break;
            case TownArchetype.IceCult: yield return new Placement(StructureKind.FrozenFigure, 2750f, 2); break;
            case TownArchetype.AbandonedOutpost: yield return new Placement(StructureKind.WagonWreck, 2120f, 1); break;
            case TownArchetype.DroneOutpost:
                yield return new Placement(StructureKind.Hut, 1500f, 1);
                yield return new Placement(StructureKind.Barracks, 2750f);
                yield return new Placement(StructureKind.WagonWreck, 2340f, 2);
                break;
            case TownArchetype.HibernationFactory:
                yield return new Placement(StructureKind.Barracks, 2750f, 1);
                yield return new Placement(StructureKind.FrozenFigure, 2340f, 3);
                break;
            case TownArchetype.CyberChapel:
                yield return new Placement(StructureKind.Hut, 950f, 1);
                yield return new Placement(StructureKind.Barracks, 2700f, 1);
                yield return new Placement(StructureKind.FrozenFigure, 2500f, 3);
                break;
            case TownArchetype.SignalTower:
                yield return new Placement(StructureKind.Hut, 2420f);
                yield return new Placement(StructureKind.FrozenFigure, 1270f, 3);
                yield return new Placement(StructureKind.WagonWreck, 2760f, 2);
                break;
        }
    }

    /// <summary>Dış mekândaki çevresel hikâye noktaları (donmuş insanlar, terk edilmiş vagonlar, AURA'nın zirvesi).</summary>
    public static IEnumerable<Interactable> ExtraSpots(TownArchetype archetype)
    {
        Interactable S(string id, float x, float y = 400f) => new(InteractableKind.Spot, x, y) { SpotId = id };
        switch (archetype)
        {
            case TownArchetype.TradeHub: yield return S("trade.frozen", 1565f, 420f); break;
            case TownArchetype.MiningVillage: yield return S("mine.statues", 2600f, 420f); break;
            case TownArchetype.GhostStation: yield return S("ghost.wagon", 820f, 380f); break;
            case TownArchetype.IceCult: yield return S("cult.frozen", 2750f, 420f); break;
            case TownArchetype.AbandonedOutpost: yield return S("outpost.wagon", 2120f, 380f); break;
            case TownArchetype.DroneOutpost: yield return S("drone.wagon", 2340f, 380f); break;
            case TownArchetype.HibernationFactory: yield return S("factory.frozen", 2340f, 420f); break;
            case TownArchetype.CyberChapel: yield return S("chapel.frozen", 2500f, 420f); break;
            case TownArchetype.SignalTower:
                yield return S("tower.frozen", 1270f, 420f);
                yield return S("tower.wagon", 2760f, 380f);
                yield return S("tower.summit", 1660f, 300f) with { RequireFlag = Flags.E2TowerDone };
                break;
        }
        // Gizli zula: keşişin haritasıyla bulunur (Kuzgun Geçidi)
        if (archetype == TownArchetype.IceCult) yield return S("cult.cache", 2380f, 420f) with { RequireFlag = "sq:hermit.cache|sqdone:hermit.cache" };
    }

    // ------------------------------------------------------------------
    // İç mekânların içeriği
    // ------------------------------------------------------------------

    private static readonly Color Brown = new(96, 70, 48), Grey = new(70, 74, 84), Olive = new(70, 76, 56), White = new(210, 214, 224);

    private static readonly Dictionary<(string, InteriorKind), InteriorSpot[]> Table = new()
    {
        // ---- Episode 1 ----
        [("trade", InteriorKind.Tavern)] = new[]
        {
            new InteriorSpot("trade.tavern.greta", StructureKind.BarCounter, 0, Who(new Color(120, 60, 50), new Color(230, 200, 140))),
            new InteriorSpot("trade.tavern.tape", StructureKind.TapeDeck)
        },
        [("trade", InteriorKind.Garrison)] = new[]
        {
            new InteriorSpot("trade.garrison.marshal", StructureKind.Desk, 0, Who(new Color(60, 66, 80), new Color(200, 170, 70))),
            new InteriorSpot("trade.garrison.files", StructureKind.NoticeBoard, 1)
        },
        [("trade", InteriorKind.Shelter)] = new[] { new InteriorSpot("trade.shelter.diary", StructureKind.Bunks) },

        [("mine", InteriorKind.Tavern)] = new[]
        {
            new InteriorSpot("mine.tavern.yuri", StructureKind.Table, 0, Who(new Color(70, 60, 54), new Color(200, 50, 40))),
            new InteriorSpot("mine.tavern.ledger", StructureKind.Desk, 1)
        },
        [("mine", InteriorKind.Workshop)] = new[]
        {
            new InteriorSpot("mine.workshop.sasha", StructureKind.Workbench, 0, Who(new Color(80, 66, 50), new Color(220, 140, 60))),
            new InteriorSpot("mine.workshop.blueprint", StructureKind.MapBoard)
        },
        [("mine", InteriorKind.Shelter)] = new[] { new InteriorSpot("mine.shelter.frozen", StructureKind.FrozenFigure, 1) },

        [("ghost", InteriorKind.RadioRoom)] = new[]
        {
            new InteriorSpot("ghost.radio.keeper", StructureKind.RadioConsole, 0, Who(new Color(70, 56, 40), new Color(200, 160, 90))),
            new InteriorSpot("ghost.radio.tape", StructureKind.TapeDeck)
        },
        [("ghost", InteriorKind.Shelter)] = new[]
        {
            new InteriorSpot("ghost.shelter.statues", StructureKind.FrozenFigure, 2),
            new InteriorSpot("ghost.shelter.ticket", StructureKind.Desk, 2)
        },

        [("cult", InteriorKind.Tavern)] = new[]
        {
            new InteriorSpot("cult.tavern.vesna", StructureKind.Table, 1, Who(new Color(220, 226, 236), new Color(150, 200, 255))),
            new InteriorSpot("cult.tavern.scripture", StructureKind.Shelf, 1)
        },
        [("cult", InteriorKind.Shelter)] = new[]
        {
            new InteriorSpot("cult.shelter.hermit", StructureKind.Fireplace, 1, Who(new Color(40, 44, 56), new Color(170, 200, 255))),
            new InteriorSpot("cult.shelter.wall", StructureKind.MapBoard, 1)
        },

        [("outpost", InteriorKind.Garrison)] = new[]
        {
            new InteriorSpot("outpost.garrison.log", StructureKind.Desk),
            new InteriorSpot("outpost.garrison.armory", StructureKind.Lockers) { RequireFlag = "doc:outpost.log" }
        },
        [("outpost", InteriorKind.Shelter)] = new[]
        {
            new InteriorSpot("outpost.shelter.salt", StructureKind.Bunks, 1, Who(new Color(200, 200, 180), new Color(120, 110, 90)))
        },
        [("outpost", InteriorKind.RadioRoom)] = new[] { new InteriorSpot("outpost.radio.tape", StructureKind.TapeDeck, 1) },

        [("mil", InteriorKind.Garrison)] = new[]
        {
            new InteriorSpot("mil.garrison.kaya", StructureKind.Desk, 0, Who(new Color(60, 70, 54), new Color(230, 190, 70))),
            new InteriorSpot("mil.garrison.orders", StructureKind.NoticeBoard, 1)
        },
        [("mil", InteriorKind.RadioRoom)] = new[]
        {
            new InteriorSpot("mil.radio.signal", StructureKind.RadioConsole, 0, Who(Olive, new Color(140, 200, 255))),
            new InteriorSpot("mil.radio.intercept", StructureKind.TapeDeck, 1)
        },
        [("mil", InteriorKind.Tavern)] = new[] { new InteriorSpot("mil.tavern.letters", StructureKind.Table, 2) },

        // ---- Hikâye sahneleri ----
        [("hq", InteriorKind.Garrison)] = new[]
        {
            new InteriorSpot("hq.bunker.protocol", StructureKind.Desk, 1),
            new InteriorSpot("hq.bunker.tape", StructureKind.TapeDeck),
            new InteriorSpot("hq.bunker.tags", StructureKind.Lockers, 1)
        },
        [("lab", InteriorKind.RadioRoom)] = new[]
        {
            new InteriorSpot("lab.inner.rhee", StructureKind.TapeDeck, 1),
            new InteriorSpot("lab.inner.kask", StructureKind.Desk, 2),
            new InteriorSpot("lab.inner.photo", StructureKind.MapBoard, 2)
        },
        [("tower", InteriorKind.RadioRoom)] = new[]
        {
            new InteriorSpot("tower.ctrl.log", StructureKind.Desk, 2),
            new InteriorSpot("tower.ctrl.puzzle", StructureKind.RadioConsole, 1),
            new InteriorSpot("tower.ctrl.tape", StructureKind.TapeDeck, 1)
        },

        // ---- Episode 2 ----
        [("drone", InteriorKind.RadioRoom)] = new[]
        {
            new InteriorSpot("drone.radio.b17", StructureKind.RadioConsole, 1, Who(new Color(150, 156, 170), new Color(110, 200, 255))),
            new InteriorSpot("drone.radio.log", StructureKind.Desk, 2)
        },
        [("drone", InteriorKind.Garrison)] = new[]
        {
            new InteriorSpot("drone.garrison.frozen", StructureKind.FrozenFigure, 3),
            new InteriorSpot("drone.garrison.orders", StructureKind.NoticeBoard, 1)
        },
        [("factory", InteriorKind.Tavern)] = new[]
        {
            new InteriorSpot("factory.tavern.marya", StructureKind.Table, 1, Who(new Color(70, 80, 100), new Color(140, 230, 255)))
        },
        [("factory", InteriorKind.Workshop)] = new[] { new InteriorSpot("factory.workshop.tape", StructureKind.TapeDeck, 1) },
        [("factory", InteriorKind.Shelter)] = new[] { new InteriorSpot("factory.shelter.cribs", StructureKind.Bunks, 2) },
        [("chapel", InteriorKind.RadioRoom)] = new[]
        {
            new InteriorSpot("chapel.radio.preacher", StructureKind.RadioConsole, 1, Who(new Color(150, 160, 180), new Color(80, 255, 200))),
            new InteriorSpot("chapel.radio.tape", StructureKind.TapeDeck, 1)
        },
        [("chapel", InteriorKind.Shelter)] = new[]
        {
            new InteriorSpot("chapel.shelter.pilgrims", StructureKind.FrozenFigure, 3),
            new InteriorSpot("chapel.shelter.letter", StructureKind.Desk, 2)
        },
    };

    /// <summary>Her iç mekânda bulunan genel nokta (sıcak ocak, aranan listesi, tezgâh, ranza: hatıralar, telsiz).</summary>
    /// <remarks>Episode 2 yerleşimlerinde ranza ve telsiz Episode 2'nin hatıralarını ve yayınlarını açar.</remarks>
    private static InteriorSpot Generic(InteriorKind kind, bool episode2) => kind switch
    {
        InteriorKind.Tavern => new InteriorSpot("int.tavern.hearth", StructureKind.Fireplace),
        InteriorKind.Garrison => new InteriorSpot(episode2 ? "int.garrison.board2" : "int.garrison.board", StructureKind.NoticeBoard),
        InteriorKind.Workshop => new InteriorSpot("int.workshop.bench", StructureKind.Workbench, 1),
        InteriorKind.Shelter => new InteriorSpot(episode2 ? "int.shelter.cot2" : "int.shelter.cot", StructureKind.Bunks),
        _ => new InteriorSpot(episode2 ? "int.radio.set2" : "int.radio.set", StructureKind.RadioConsole, episode2 ? 1 : 0)
    };

    private static bool IsEpisode2(TownArchetype archetype) => archetype >= TownArchetype.DroneOutpost;

    /// <summary>İç mekânın adı ("int.name.Tavern" ya da yerleşime özgü "int.name.trade.Tavern").</summary>
    public static string NameKey(InteriorKind kind, TownArchetype archetype)
    {
        string key = $"int.name.{TownKey(archetype)}.{kind}";
        return Localization.Loc.Has(key) ? key : $"int.name.{kind}";
    }

    /// <summary>
    /// İç mekânın yerleşimi: solda çıkış kapısı, ardından (varsa) hizmet tezgâhı, yerleşime özgü noktalar ve genel nokta.
    /// </summary>
    public static TownLayout Build(InteriorKind kind, TownArchetype archetype, InteractableKind? service)
    {
        var structures = new List<Placement>();
        var interactables = new List<Interactable>
        {
            new(InteractableKind.Exit, ExitX, 380f)
        };
        var people = new List<PersonSpot>();
        var lamps = new List<float>();
        int slot = 0;

        if (service != null)
        {
            float x = Slots[slot++];
            structures.Add(new Placement(kind switch
            {
                InteriorKind.Tavern => StructureKind.BarCounter,
                InteriorKind.Workshop => StructureKind.Workbench,
                _ => StructureKind.Boiler
            }, x, 1));
            interactables.Add(new Interactable(service.Value, x, 400f) { NameKey = $"int.counter.{kind}" });
            people.Add(new PersonSpot(x + 30f, -1, kind switch
            {
                InteriorKind.Tavern => Brown,
                InteriorKind.Workshop => new Color(80, 66, 50),
                _ => Grey
            }, White));
        }

        var spots = new List<InteriorSpot>();
        if (Table.TryGetValue((TownKey(archetype), kind), out var specific)) spots.AddRange(specific);
        spots.Add(Generic(kind, IsEpisode2(archetype)));

        foreach (var spot in spots)
        {
            if (slot >= Slots.Length) break;
            if (NarrativeData.Spot(spot.SpotId) == null) continue; // İçeriği tanımlanmamış nokta çizilmez
            float x = Slots[slot++];
            structures.Add(new Placement(spot.Prop, x, spot.Variant));
            interactables.Add(new Interactable(InteractableKind.Spot, x, spot.Person != null ? 410f : 390f)
                { SpotId = spot.SpotId, RequireFlag = spot.RequireFlag });
            if (spot.Person != null) people.Add(spot.Person with { X = x + 34f });
        }

        for (float x = 300f; x < Width - 100f; x += 300f) lamps.Add(x);
        return new TownLayout(structures.ToArray(), interactables.ToArray(), people.ToArray(), lamps.ToArray(),
            Gloom: kind == InteriorKind.Shelter ? 0.2f : 0.1f, DistantLights: false)
        {
            Width = Width,
            Interior = kind
        };
    }
}
