using System;
using TheLastDelivery.Economy;

namespace TheLastDelivery.Platform;

public enum PurchaseResult
{
    Purchased,    // Ödeme onaylandı, içerik envantere eklendi
    AlreadyOwned,
    QuestOnly,    // Satışta değil: yalnızca oyun içi görevle açılır
    Unavailable   // Mağazaya ulaşılamadı (Steam yok / ödeme iptal)
}

/// <summary>
/// Oyun içi mikro içerik mağazası. Ödeme platform üzerinden yapılır (Steam: ISteamMicroTxn);
/// onaylanan içerik SaveData.OwnedCosmetics'e yazılır, böylece çevrimdışıyken de kullanılabilir.
/// Hiçbir hata oyunu durdurmaz.
/// </summary>
public static class StoreManager
{
    public static PurchaseResult Buy(CosmeticItem item)
    {
        if (CosmeticCatalog.IsOwned(item)) return PurchaseResult.AlreadyOwned;
        if (item.MicroId == null) return PurchaseResult.QuestOnly;

        bool paid;
        try
        {
            paid = AchievementManager.Platform.TryPurchaseItem(item.MicroId);
        }
        catch (Exception)
        {
            paid = false;
        }
        if (!paid) return PurchaseResult.Unavailable;

        SaveData.Current.OwnedCosmetics.Add(item.Key);
        SaveData.Save();
        return PurchaseResult.Purchased;
    }
}
