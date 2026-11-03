using System;
using System.Collections.Generic;
using TheLastDelivery.Localization;

namespace TheLastDelivery.Economy;

/// <summary>Bir başarım: koşulu SaveData istatistiklerinden okunur; özel skinlerin kilidini açar.</summary>
public sealed record Achievement(string Id, int Target, Func<SaveData, int> Progress)
{
    public string Name => Loc.T($"ach.{Id}.name");
    public string Description => Loc.F($"ach.{Id}.desc", Target);
    public bool IsMet(SaveData save) => Progress(save) >= Target;
}

/// <summary>
/// Başarımlar. Yeni karşılanan başarımlar SaveData.Achievements listesine yazılır ve
/// oyunda bildirim olarak gösterilir. Skin dükkânı başarımı açık olmayan skini satmaz.
/// </summary>
public static class Achievements
{
    public static readonly Achievement[] All =
    {
        new("chapter10", 10, s => s.UnlockedChapter),
        new("escape5", 5, s => s.AmbushesEscaped),
        new("delivered", 1, s => s.Delivered ? 1 : 0),
        new("wolves5", 5, s => s.WolvesRepelled),
        new("ore10", 10, s => s.OresMined),
        new("endless1000", 1000, s => s.EndlessBestKm),
        new("traitor3", 3, s => s.TraitorsCaught),

        // Atmosfer güncellemesi: toplam 15 başarım (Steam'e ACH_<ID> adıyla bildirilir)
        new("betrayed1", 1, s => s.TimesSabotaged),
        new("millionaire", Core.GameConfig.MillionaireCredits, s => s.PeakCredits),
        new("speedrun1", 1, s => s.SpeedrunWins),
        new("fullHouse", Core.GameConfig.CabinCapacity + Core.GameConfig.PassengerWagonBeds, s => s.MaxPassengersAtOnce),
        new("baron", 3, s => CountWagons(s)),
        new("lamp5", 5, s => s.LampsFixed),
        new("feed25", 25, s => s.PassengersFed),
        new("blizzard5", 5, s => s.BlizzardsSurvived),
    };

    private static int CountWagons(SaveData s)
    {
        int count = 0;
        foreach (var wagon in new[] { UpgradeType.CargoWagon, UpgradeType.PassengerWagon, UpgradeType.ArmoredWagon })
            if (s.Upgrades.Contains(wagon.ToString())) count++;
        return count;
    }

    public static Achievement Get(string id)
    {
        foreach (var a in All) if (a.Id == id) return a;
        return null;
    }

    /// <summary>Başarım şartı yoksa ya da karşılanmışsa true.</summary>
    public static bool IsUnlocked(string id)
    {
        if (string.IsNullOrEmpty(id)) return true;
        var achievement = Get(id);
        return achievement == null || achievement.IsMet(SaveData.Current);
    }

    /// <summary>Yeni karşılanan başarımları kaydeder ve döner (oyun içi bildirim için).</summary>
    public static List<Achievement> CollectNew()
    {
        var unlocked = new List<Achievement>();
        var save = SaveData.Current;
        foreach (var a in All)
        {
            if (save.Achievements.Contains(a.Id) || !a.IsMet(save)) continue;
            save.Achievements.Add(a.Id);
            unlocked.Add(a);
        }
        if (unlocked.Count > 0) SaveData.Save();
        return unlocked;
    }
}
