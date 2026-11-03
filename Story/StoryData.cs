using TheLastDelivery.Items;
using TheLastDelivery.Passengers;

namespace TheLastDelivery.Story;

/// <summary>
/// İstasyon senaryolarının oynanış verisi. Senaryolar sırayla, her GameConfig.StationIntervalKm
/// kilometrede bir karşımıza çıkar. Seçenek A her zaman birini trene almak, B ise onu geride
/// bırakıp kaynak almaktır. Tüm metinler Localization/Strings.*.cs tablolarındadır.
/// </summary>
public static class StoryData
{
    public static readonly StationScenario[] Scenarios =
    {
        new("s1",
            OptionA: new ChoiceOption("s1.a",
                Passenger: new PassengerProfile("viktor", PassengerCondition.Freezing, PassengerSkill.Engineer)),
            OptionB: new ChoiceOption("s1.b", Fuel: 20f)),

        new("s2",
            OptionA: new ChoiceOption("s2.a",
                Passenger: new PassengerProfile("elena", PassengerCondition.Freezing, PassengerSkill.RadioOperator),
                Items: new[] { ItemType.Medkit }),
            OptionB: new ChoiceOption("s2.b",
                Items: new[] { ItemType.SpareParts, ItemType.SpareParts })),

        new("s3",
            OptionA: new ChoiceOption("s3.a",
                Passenger: new PassengerProfile("mika", PassengerCondition.Sick, PassengerSkill.Child) { TraitorChance = 0f }),
            OptionB: new ChoiceOption("s3.b", Fuel: 25f, Items: new[] { ItemType.ThermalCoal })),

        new("s4",
            OptionA: new ChoiceOption("s4.a",
                Passenger: new PassengerProfile("bora", PassengerCondition.Injured, PassengerSkill.Stoker),
                Items: new[] { ItemType.ThermalCoal }),
            OptionB: new ChoiceOption("s4.b", Fuel: 20f)),
    };
}

/// <summary>
/// Hikâye senaryoları bittikten sonraki istasyonlarda (ve sonsuz modda) peronda bekleyen yabancılar.
/// Yabancıların gizli hain çıkma olasılığı yüksektir. Metinler: "stranger.*", "p.{id}.*".
/// </summary>
/// <summary>Telsizdeki imdat çağrılarından kurtarılabilen yolcular (hain çıkma olasılıkları düşük).</summary>
public static class SosProfiles
{
    public static readonly PassengerProfile Sergeant = new("dmitri", PassengerCondition.Injured, PassengerSkill.Guard) { TraitorChance = 0.05f };
    public static readonly PassengerProfile Medic = new("mila", PassengerCondition.Freezing, PassengerSkill.Medic) { TraitorChance = 0.05f };
}

/// <summary>Episode 2'nin yolcuları: telsiz korsanı, uyandırılan uyuyan, NULL kaçağı, makine karakolunun askeri, fabrika işçisi.</summary>
public static class Episode2Profiles
{
    public static readonly PassengerProfile Nova = new("nova", PassengerCondition.Stable, PassengerSkill.RadioOperator) { TraitorChance = 0f };
    public static readonly PassengerProfile Ada = new("ada", PassengerCondition.Sick, PassengerSkill.Medic) { TraitorChance = 0.05f };
    public static readonly PassengerProfile Kiril = new("kiril", PassengerCondition.Injured, PassengerSkill.Engineer) { TraitorChance = 0.3f };
    public static readonly PassengerProfile Yusuf = new("yusuf", PassengerCondition.Freezing, PassengerSkill.Guard) { TraitorChance = 0.1f };
    public static readonly PassengerProfile Mara = new("mara", PassengerCondition.Stable, PassengerSkill.Stoker) { TraitorChance = 0.15f };

    public static readonly PassengerProfile[] All = { Nova, Ada, Kiril, Yusuf, Mara };
}

/// <summary>Kimliğinden yolcu profili (Episode 1'den Episode 2'ye aktarılan yolcular için).</summary>
public static class PassengerRegistry
{
    public static PassengerProfile Find(string id)
    {
        foreach (var s in StoryData.Scenarios)
        {
            if (s.OptionA.Passenger?.Id == id) return s.OptionA.Passenger;
            if (s.OptionB.Passenger?.Id == id) return s.OptionB.Passenger;
        }
        foreach (var t in TownCatalog.All) if (t.Passenger?.Id == id) return t.Passenger;
        foreach (var p in StrangerCatalog.All) if (p.Id == id) return p;
        foreach (var p in Episode2Profiles.All) if (p.Id == id) return p;
        if (SosProfiles.Sergeant.Id == id) return SosProfiles.Sergeant;
        if (SosProfiles.Medic.Id == id) return SosProfiles.Medic;
        return null;
    }
}

public static class StrangerCatalog
{
    public static readonly PassengerProfile[] All =
    {
        new("grigor", PassengerCondition.Stable, PassengerSkill.Trader) { TraitorChance = 0.45f },
        new("vera", PassengerCondition.Freezing, PassengerSkill.Engineer) { TraitorChance = 0.35f },
        new("feliks", PassengerCondition.Sick, PassengerSkill.None) { TraitorChance = 0.5f },
        new("anya", PassengerCondition.Injured, PassengerSkill.Guard) { TraitorChance = 0.35f },
        new("pavel", PassengerCondition.Freezing, PassengerSkill.Stoker) { TraitorChance = 0.4f },
        new("lev", PassengerCondition.Stable, PassengerSkill.RadioOperator) { TraitorChance = 0.4f },
    };

    /// <summary>Trende olmayan rastgele bir yabancı (hepsi trendeyse null).</summary>
    public static PassengerProfile Pick(System.Random rng, Cabin cabin)
    {
        int offset = rng.Next(All.Length);
        for (int i = 0; i < All.Length; i++)
        {
            var profile = All[(offset + i) % All.Length];
            if (!cabin.HasProfile(profile.Id)) return profile;
        }
        return null;
    }
}
