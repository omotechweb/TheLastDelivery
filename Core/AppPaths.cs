using System;
using System.IO;

namespace TheLastDelivery.Core;

/// <summary>
/// Oyunun kullanıcı verisi klasörü (kayıt, dil, fotoğraflar, modlar): %LocalAppData%\TheLastDelivery.
/// TLD_DATA_DIR ortam değişkeni başka bir klasör gösterirse o kullanılır (testler gerçek kaydı bozmasın diye).
/// </summary>
public static class AppPaths
{
    public static string DataDir { get; } = Resolve();

    public static string Combine(params string[] parts) => Path.Combine(DataDir, Path.Combine(parts));

    private static string Resolve()
    {
        string custom = Environment.GetEnvironmentVariable("TLD_DATA_DIR");
        return !string.IsNullOrWhiteSpace(custom)
            ? Path.GetFullPath(custom)
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TheLastDelivery");
    }
}
