using System;
using System.Collections.Generic;
using TheLastDelivery.Localization;

namespace TheLastDelivery.Economy;

/// <summary>Kozmetik açan oyun içi görev: ilerleme SaveData istatistiklerinden okunur.</summary>
public sealed record Quest(string Id, int Target, Func<SaveData, int> Progress)
{
    public string Description => Loc.F($"quest.{Id}", Target);
    public bool IsMet(SaveData save) => Progress(save) >= Target;
}

/// <summary>
/// Mikro kozmetikleri ücretsiz açan görevler. Görev tamamlandığında ilgili kozmetik
/// SaveData.OwnedCosmetics'e eklenir (satın alınmış gibi).
/// </summary>
public static class QuestCatalog
{
    public static readonly Quest[] All =
    {
        new("sos3", 3, s => s.SosCompleted),                              // 3 imdat çağrısına yanıt ver
        new("hero", 1, s => s.EndingsSeen.Contains("hero") ? 1 : 0),      // Kahraman sonunu gör
        new("endings2", 2, s => s.EndingsSeen.Count),                     // 2 farklı son gör
    };

    public static Quest Get(string id) => Array.Find(All, q => q.Id == id);

    /// <summary>Tamamlanan görevlerin kozmetiklerini açar; yeni açılanları döner (bildirim için).</summary>
    public static List<CosmeticItem> Evaluate()
    {
        var unlocked = new List<CosmeticItem>();
        var save = SaveData.Current;
        foreach (var item in CosmeticCatalog.MicroItems)
        {
            if (item.Quest == null || save.OwnedCosmetics.Contains(item.Key)) continue;
            var quest = Get(item.Quest);
            if (quest == null || !quest.IsMet(save)) continue;
            save.OwnedCosmetics.Add(item.Key);
            unlocked.Add(item);
        }
        if (unlocked.Count > 0) SaveData.Save();
        return unlocked;
    }
}
