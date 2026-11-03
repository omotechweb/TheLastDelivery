using TheLastDelivery.Localization;
using TheLastDelivery.Passengers;

namespace TheLastDelivery.Story;

/// <summary>
/// Yol üstündeki canlı bir kasaba. Tavernasında bir yolcu adayı ve dedikodu,
/// sokağında konuşulabilecek bir NPC vardır. Metinler: "town{N}.*".
/// </summary>
public sealed record TownInfo(int Number, PassengerProfile Passenger, Microsoft.Xna.Framework.Color Accent,
    UI.PortraitKind NpcPortrait, TownArchetype Archetype = TownArchetype.TradeHub)
{
    /// <summary>Hikâye bölümünün sahnesi mi (kasaba değil)?</summary>
    public bool IsStoryScene => Number <= 0;

    /// <summary>Hikâye bölümleri için sahte "kasaba": adı "scene.{arketip}" anahtarından gelir.</summary>
    public static TownInfo ForScene(TownArchetype scene, Microsoft.Xna.Framework.Color accent) =>
        new(0, null, accent, UI.PortraitKind.Arthur, scene);

    public string Name => IsStoryScene ? Loc.T($"scene.{Archetype}") : Loc.T($"town{Number}.name");
    public string TavernText => Loc.T($"town{Number}.tavern");
    public string Gossip => Loc.T($"town{Number}.gossip");
    public string JoinText => Loc.T($"town{Number}.join");
    public string NpcName => Loc.T($"town{Number}.npc");
    public string NpcLine => Loc.T($"town{Number}.npcLine");
}

public static class TownCatalog
{
    public static readonly TownInfo[] All =
    {
        // Her kasaba farklı bir mimari şablon: binalar, NPC'ler, replikler ve görevler kasabaya özgü
        new(1, new PassengerProfile("nadia", PassengerCondition.Stable, PassengerSkill.Medic), new(255, 190, 120), UI.PortraitKind.Watchman, TownArchetype.TradeHub),          // Kırağı Pazarı
        new(2, new PassengerProfile("oskar", PassengerCondition.Stable, PassengerSkill.Guard), new(255, 160, 90), UI.PortraitKind.MinerKid, TownArchetype.MiningVillage),      // Demirhan
        new(3, new PassengerProfile("lina", PassengerCondition.Stable, PassengerSkill.Trader), new(150, 190, 210), UI.PortraitKind.LanternKeeper, TownArchetype.GhostStation), // Son Fener Durağı
        new(4, new PassengerProfile("ilya", PassengerCondition.Stable, PassengerSkill.RadioOperator) { TraitorChance = 0.25f }, new(150, 200, 255), UI.PortraitKind.Hermit, TownArchetype.IceCult),         // Kuzgun Geçidi
        new(5, new PassengerProfile("zoya", PassengerCondition.Stable, PassengerSkill.Medic) { TraitorChance = 0.25f }, new(200, 200, 180), UI.PortraitKind.SaltWorker, TownArchetype.AbandonedOutpost),    // Tuzla Karakolu
        new(6, new PassengerProfile("taras", PassengerCondition.Stable, PassengerSkill.Guard) { TraitorChance = 0.3f }, new(255, 140, 110), UI.PortraitKind.GateKeeper, TownArchetype.MilitaryGate),       // Güney Kapısı Garnizonu
        // Episode 2 "Project AURA": Kutup Hattı kasabaları
        new(7, Episode2Profiles.Yusuf, new(110, 200, 255), UI.PortraitKind.Droid, TownArchetype.DroneOutpost),         // Ölü Sinyal Karakolu
        new(8, Episode2Profiles.Mara, new(140, 230, 255), UI.PortraitKind.Warden, TownArchetype.HibernationFactory),   // Novo-7 Uyku Fabrikası
        new(9, Episode2Profiles.Kiril, new(90, 255, 200), UI.PortraitKind.Acolyte, TownArchetype.CyberChapel),        // Krom Şapel
    };
}
