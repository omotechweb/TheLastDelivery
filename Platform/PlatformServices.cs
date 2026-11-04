using System;
using System.Collections.Generic;

namespace TheLastDelivery.Platform;

/// <summary>
/// Steam gibi bir dağıtım platformunun oyuna sunduğu hizmetler.
/// Oyun yalnızca bu arayüzle konuşur; Steamworks.NET entegre edildiğinde
/// <c>SteamPlatform : IPlatformServices</c> yazılıp <see cref="AchievementManager.Initialize"/> ile takılır.
/// Steam kurulu değilken <see cref="OfflinePlatform"/> kullanılır: her şey yerel kayıtta tutulmaya devam eder.
/// </summary>
public interface IPlatformServices
{
    /// <summary>Gösterim ve kayıt için platform adı ("Steam", "Offline").</summary>
    string Name { get; }

    /// <summary>Platforma bağlı mı? (Değilse çağrılar yalnızca yerel olarak kaydedilir.)</summary>
    bool IsOnline { get; }

    /// <summary>Başarımı açar (Steam: SteamUserStats.SetAchievement). Zaten açıksa etkisizdir.</summary>
    void SetAchievement(string apiName);

    /// <summary>Bekleyen başarım ve istatistikleri sunucuya gönderir (Steam: StoreStats).</summary>
    void StoreStats();

    /// <summary>Liderlik tablosuna skor yükler. Ek veriler (km, aşama) tabloda skorla birlikte saklanır.</summary>
    void UploadLeaderboardScore(string board, int score, int[] details);

    /// <summary>
    /// DLC sahipliğini sorgular (Steam: SteamApps.BIsDlcInstalled). Şu an doğrulanamıyorsa false döner;
    /// o zaman DLCManager son doğrulanmış sahipliği (SaveData önbelleği) kullanır.
    /// </summary>
    bool TryGetDlcOwnership(uint appId, out bool owned);

    /// <summary>DLC'nin mağaza sayfasını açar (Steam: oyun içi kaplama). Açılamazsa false.</summary>
    bool OpenStorePage(uint appId);

    /// <summary>
    /// Oyun içi mikro ödeme (Steam: ISteamMicroTxn + oyun sunucusu). Ödeme onaylanırsa true.
    /// Gerçek Steam akışı eşzamansızdır; bu arayüz onay sonucunu tek çağrıda verir.
    /// </summary>
    bool TryPurchaseItem(string microId);

    /// <summary>Abone olunan Steam Workshop öğelerinin kurulu klasörleri (Steam: ISteamUGC). Yoksa boş.</summary>
    IReadOnlyList<string> GetWorkshopItemFolders();

    /// <summary>
    /// Fotoğraf modunda çekilen kareyi paylaşım kitaplığına ekler (Steam: ISteamScreens.AddScreenshotToLibrary;
    /// oradan topluluğa ve sosyal medyaya paylaşılabilir). Eklenemezse false.
    /// </summary>
    bool ShareScreenshot(string path, string caption);
}

/// <summary>
/// Steam olmadan çalışan sanal platform: çağrıları bellekte günlükler (hata ayıklama ve testler için).
/// Başarımlar ve liderlik tablosu zaten SaveData'da kalıcıdır; Steam bağlandığında
/// <see cref="AchievementManager.SyncAll"/> hepsini bir kerede gönderir.
/// </summary>
public sealed class OfflinePlatform : IPlatformServices
{
    private const int MaxLog = 64;
    private readonly List<string> _log = new();

    public string Name => "Offline";
    public bool IsOnline => false;

    /// <summary>Son platform çağrıları (en yeni sonda).</summary>
    public IReadOnlyList<string> Log => _log;

    public void SetAchievement(string apiName) => Write($"SetAchievement({apiName})");

    public void StoreStats() => Write("StoreStats()");

    public void UploadLeaderboardScore(string board, int score, int[] details) =>
        Write($"UploadLeaderboardScore({board}, {score}, [{string.Join(", ", details ?? Array.Empty<int>())}])");

    /// <summary>
    /// Steam olmadan DLC sahipliği doğrulanamaz: sürüm (Release) derlemede hiçbir DLC açık değildir.
    /// </summary>
    public bool TryGetDlcOwnership(uint appId, out bool owned)
    {
        owned = false;
        return true;
    }

    public bool OpenStorePage(uint appId)
    {
        Write($"OpenStorePage({appId})");
        return false; // Steam kaplaması yok: oyun "Steam'e bağlı değil" bildirimini gösterir
    }

    /// <summary>Steam olmadan ödeme alınamaz.</summary>
    public bool TryPurchaseItem(string microId)
    {
        Write($"TryPurchaseItem({microId})");
        return false;
    }

    public IReadOnlyList<string> GetWorkshopItemFolders() => Array.Empty<string>();

    /// <summary>Steam yok: fotoğraf yalnızca yerel Photos klasöründe kalır.</summary>
    public bool ShareScreenshot(string path, string caption)
    {
        Write($"ShareScreenshot({System.IO.Path.GetFileName(path)}, {caption})");
        return false;
    }

    private void Write(string entry)
    {
        _log.Add(entry);
        if (_log.Count > MaxLog) _log.RemoveAt(0);
    }
}
