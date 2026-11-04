using System;
using System.Collections.Generic;
using TheLastDelivery.Economy;

namespace TheLastDelivery.Platform;

/// <summary>Steam'de satılan indirilebilir içerikler.</summary>
public enum DlcId
{
    Supporter,       // Geliştiriciye Destek Paketi: Altın Kaplama Lokomotif, Destekçi Rozeti, Bonuslar
    FrozenWasteland, // Buzul Kıyameti: 5 ek bölüm, Asit Fırtınası, Akıncı ve Askeri tren skinleri
    Couriers,        // Sığınak Kuryeleri Kozmetik Paketi: 4 tren rengi, 4 duman efekti, kabin süsleri, kostümler
    Hardcore,        // Acımasız Kış Sınavı: Permadeath (tek can) modu ve aşırı zorluk
    Episode2         // Episode 2 "Project AURA": 38 bölüm (3-40), hack mekaniği, Panzer ve Cyber-Glitch modelleri, 3 final
}

/// <summary>Bir DLC'nin mağaza bilgisi. Steam App ID'leri Steamworks'te DLC oluşturulunca güncellenmelidir.</summary>
public sealed record DlcInfo(DlcId Id, uint AppId, string Key)
{
    public string Name => Localization.Loc.T($"dlc.{Key}.name");
    public string Description => Localization.Loc.T($"dlc.{Key}.desc");
    public string Contents => Localization.Loc.T($"dlc.{Key}.contents");
}

/// <summary>
/// DLC sahipliğinin tek kaynağı. Sahiplik açılışta (ve DLC ekranına girildiğinde) platformdan bir kez sorgulanır,
/// bellekte tutulur ve SaveData'ya "son doğrulanan" olarak yazılır; oyun döngüsünde platforma hiç çağrı yapılmaz.
/// Platform yanıt veremezse (ör. Steam çevrimdışı) son doğrulanan sahiplik kullanılır; hiçbir hata oyunu durdurmaz.
/// Kilitli içerik (skin, duman, bölüm, mod) her zaman güvenli bir varsayılana düşer.
/// </summary>
public static class DLCManager
{
    // Yer tutucu App ID'ler: Steamworks panelinde DLC'ler açıldığında gerçek numaralarla değiştirilecek
    public static readonly DlcInfo[] All =
    {
        new(DlcId.Supporter, 3_100_001, "supporter"),
        new(DlcId.FrozenWasteland, 3_100_002, "wasteland"),
        new(DlcId.Couriers, 3_100_003, "couriers"),
        new(DlcId.Hardcore, 3_100_004, "hardcore"),
        new(DlcId.Episode2, 3_100_005, "episode2"),
    };

    /// <summary>
    /// Şimdilik bütün ücretli içerik (5 DLC ve mikro ödemeli kozmetikler) herkese ücretsiz. Satışa geri dönmek için
    /// false yapmak yeterli: sahiplik yeniden platformdan (Steam) okunur.
    /// </summary>
    public const bool AllContentFree = true;

    private static readonly HashSet<DlcId> Owned = new();

    public static DlcInfo Info(DlcId id) => Array.Find(All, d => d.Id == id);

    public static bool Owns(DlcId id) => Owned.Contains(id);

    /// <summary>İçerik bir DLC'ye bağlı değilse (null) her zaman kullanılabilir.</summary>
    public static bool Owns(DlcId? id) => id == null || Owned.Contains(id.Value);

    /// <summary>
    /// Steam DLC kontrol değişkeni: Episode 2 "Project AURA" açık mı? Bölüm 3-40, Panzer ve Cyber-Glitch modelleri
    /// ve Hologram kokpit buna bağlıdır. Sahiplik yalnızca Refresh() ile (açılışta, DLC ekranında, mağazadan dönüşte) güncellenir.
    /// </summary>
    public static bool DLC_EPISODE2_UNLOCKED => Owned.Contains(DlcId.Episode2);

    /// <summary>Platformdan sahipliği yeniden okur; sonuçları SaveData'ya işler ve DLC kozmetiklerini envantere ekler.</summary>
    public static void Refresh()
    {
        var platform = AchievementManager.Platform;
        var cached = SaveData.Current.OwnedDlc;
        bool changed = false;
        Owned.Clear();

        foreach (var dlc in All)
        {
            bool owned;
            try
            {
                if (!platform.TryGetDlcOwnership(dlc.AppId, out owned))
                    owned = cached.Contains(dlc.Key); // Doğrulanamadı: son bilinen durum
            }
            catch (Exception)
            {
                owned = cached.Contains(dlc.Key);     // Platform hatası oyunu durdurmaz
            }

            if (AllContentFree) owned = true;
            if (owned) Owned.Add(dlc.Id);
            if (owned != cached.Contains(dlc.Key))
            {
                if (owned) cached.Add(dlc.Key); else cached.Remove(dlc.Key);
                changed = true;
            }
        }

        changed |= GrantCosmetics();
        if (changed) SaveData.Save();
    }

    /// <summary>Sahip olunan DLC'lerin kozmetiklerini envantere işler (kayıtta "satın alınmış" görünür).</summary>
    private static bool GrantCosmetics()
    {
        bool changed = false;
        var save = SaveData.Current;
        foreach (var skin in Cosmetics.TrainSkins)
        {
            if (skin.Dlc == null || !Owns(skin.Dlc) || save.OwnedSkins.Contains(skin.Id)) continue;
            save.OwnedSkins.Add(skin.Id);
            changed = true;
        }
        foreach (var item in CosmeticCatalog.AllItems)
        {
            if (item.Dlc == null || !Owns(item.Dlc) || save.OwnedCosmetics.Contains(item.Key)) continue;
            save.OwnedCosmetics.Add(item.Key);
            changed = true;
        }
        return changed;
    }

    /// <summary>"Steam Mağazasında İncele": Steam kaplamasında açılır. Açılamazsa false (oyun bildirim gösterir).</summary>
    public static bool ViewInStore(DlcId id)
    {
        try
        {
            return AchievementManager.Platform.OpenStorePage(Info(id).AppId);
        }
        catch (Exception)
        {
            return false;
        }
    }

    // ------------------------------------------------------------------
    // İçerik yönlendirme: kilitli içerik güvenli varsayılana düşer
    // ------------------------------------------------------------------

    /// <summary>Destekçi rozeti: Supporter Pack sahibi ve rozeti gizlememiş.</summary>
    public static bool ShowSupporterBadge => !AllContentFree && Owns(DlcId.Supporter) && SaveData.Current.ShowSupporterBadge;

    /// <summary>Kuşanılan skin (DLC'si artık yoksa klasik).</summary>
    public static TrainSkin ActiveSkin(string id)
    {
        var skin = Cosmetics.Skin(id);
        return Owns(skin.Dlc) ? skin : Cosmetics.TrainSkins[0];
    }

    public static CosmeticItem Active(CosmeticSlot slot)
    {
        var item = CosmeticCatalog.Find(slot, SaveData.Current.Equipped(slot));
        return CosmeticCatalog.IsOwned(item) ? item : CosmeticCatalog.Default(slot); // DLC, mikro ödeme ya da görev
    }
}
