using System.Collections.Generic;
using TheLastDelivery.Economy;

namespace TheLastDelivery.Platform;

/// <summary>
/// Başarımların tek giriş noktası. Koşullar <see cref="Achievements"/> tablosunda, ilerleme SaveData
/// istatistiklerinde durur; bu sınıf yeni karşılanan başarımları bulur, yerel kayda işler
/// ve platforma (Steam) "ACH_&lt;ID&gt;" API adıyla bildirir.
///
/// 15 başarım: chapter10, escape5, delivered (Sığınak-0 Kurtarıcısı), wolves5, ore10, endless1000,
/// traitor3, betrayed1 (İlk İhanet), millionaire (Milyoner Kurye), speedrun1, fullHouse, baron,
/// lamp5, feed25, blizzard5.
/// </summary>
public static class AchievementManager
{
    public static IPlatformServices Platform { get; private set; } = new OfflinePlatform();

    public static int Total => Achievements.All.Length;
    public static int UnlockedCount => SaveData.Current.Achievements.Count;

    /// <summary>Oyun açılışında platformu takar ve yerelde açık olan başarımları platformla eşitler.</summary>
    public static void Initialize(IPlatformServices platform)
    {
        Platform = platform ?? new OfflinePlatform();
        SyncAll();
    }

    /// <summary>Steam'in Steam Cloud dışı açılan başarımları da tanıması için yerel listeyi gönderir.</summary>
    public static void SyncAll()
    {
        foreach (string id in SaveData.Current.Achievements)
            Platform.SetAchievement(ApiName(id));
        Platform.StoreStats();
    }

    /// <summary>Steamworks panelinde tanımlanacak başarım adı: "escape5" → "ACH_ESCAPE5".</summary>
    public static string ApiName(string id) => "ACH_" + id.ToUpperInvariant();

    /// <summary>
    /// İstatistik değiştikten sonra çağrılır: yeni karşılanan başarımları kaydeder, platforma bildirir
    /// ve oyun içi bildirim için döner.
    /// </summary>
    public static IReadOnlyList<Achievement> Evaluate()
    {
        var unlocked = Achievements.CollectNew();
        if (unlocked.Count == 0) return unlocked;

        foreach (var achievement in unlocked)
            Platform.SetAchievement(ApiName(achievement.Id));
        Platform.StoreStats();
        return unlocked;
    }

    public static bool IsUnlocked(string id) => SaveData.Current.Achievements.Contains(id);
}
