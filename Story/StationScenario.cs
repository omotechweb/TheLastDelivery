using TheLastDelivery.Items;
using TheLastDelivery.Localization;
using TheLastDelivery.Passengers;

namespace TheLastDelivery.Story;

/// <summary>
/// Bir seçeneğin oyuna etkisi. Passenger doluysa seçim o kişiyi trene alır;
/// boşsa kişi geride bırakılır ve Arthur'un günlüğüne vicdan azabı düşer.
/// Items, envantere eklenecek eşyalardır (yer yoksa geride kalır).
/// Metinler çeviri tablosundan gelir: "{Key}.label" ve "{Key}.outcome".
/// </summary>
public sealed record ChoiceOption(
    string Key,
    PassengerProfile Passenger = null,
    float Fuel = 0f,
    float Health = 0f,
    ItemType[] Items = null,
    int Karma = 0,        // Gizli ahlak puanına etkisi (iyilik +, kötülük -)
    float Sanity = 0f,    // Arthur'un akıl sağlığına etkisi
    int Rations = 0)      // Erzak kazancı (+) ya da paylaşımı (-)
{
    public bool TakesPassenger => Passenger != null;
    public string Label => Loc.T($"{Key}.label");
    public string Outcome => Loc.T($"{Key}.outcome");
}

/// <summary>İstasyonda yaşanan, iki seçenekli ahlaki ikilem. Metinler: "{Id}.title", "{Id}.text".</summary>
public sealed record StationScenario(string Id, ChoiceOption OptionA, ChoiceOption OptionB)
{
    public string Title => Loc.T($"{Id}.title");
    public string Text => Loc.T($"{Id}.text");
}
