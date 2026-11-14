using System;
using System.Globalization;
using System.IO;

namespace TheLastDelivery.Core;

/// <summary>
/// Oyuncu ayarları: ses düzeyi, ekran sarsıntısı, bloom, hasar parlaması, kontrol ipuçları ve titreşim.
/// %LocalAppData%\TheLastDelivery\settings.txt dosyasında "anahtar=değer" satırları olarak durur;
/// dosya yoksa ya da bozuksa varsayılanlar kullanılır.
/// </summary>
public static class Settings
{
    /// <summary>Ana ses düzeyi (0-100, 10'luk adımlarla).</summary>
    public static int Volume { get; set; } = 100;

    /// <summary>Ekran sarsıntısı şiddeti (0, 50 ya da 100).</summary>
    public static int Shake { get; set; } = 100;

    public static bool Bloom { get; set; } = true;

    /// <summary>Hasar anındaki kızıl çerçeve parlaması (ışığa duyarlı oyuncular kapatabilir).</summary>
    public static bool Flash { get; set; } = true;

    /// <summary>Sürüş ekranının altındaki tuş hatırlatma satırı.</summary>
    public static bool Hints { get; set; } = true;

    /// <summary>Oyun kolu titreşimi.</summary>
    public static bool Vibration { get; set; } = true;

    public static float VolumeScale => Volume / 100f;
    public static float ShakeScale => Shake / 100f;

    private static string FilePath => AppPaths.Combine("settings.txt");

    public static void Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return;
            foreach (string line in File.ReadAllLines(FilePath))
            {
                int split = line.IndexOf('=');
                if (split <= 0) continue;
                string key = line.Substring(0, split).Trim(), value = line.Substring(split + 1).Trim();
                switch (key)
                {
                    case "volume": Volume = Math.Clamp(ParseInt(value, Volume), 0, 100); break;
                    case "shake": Shake = Math.Clamp(ParseInt(value, Shake), 0, 100); break;
                    case "bloom": Bloom = value == "1"; break;
                    case "flash": Flash = value == "1"; break;
                    case "hints": Hints = value == "1"; break;
                    case "vibration": Vibration = value == "1"; break;
                }
            }
        }
        catch (Exception) { /* Ayarlar okunamadı: varsayılanlarla devam */ }
    }

    public static void Save()
    {
        try
        {
            Directory.CreateDirectory(AppPaths.DataDir);
            File.WriteAllLines(FilePath, new[]
            {
                "volume=" + Volume.ToString(CultureInfo.InvariantCulture),
                "shake=" + Shake.ToString(CultureInfo.InvariantCulture),
                "bloom=" + (Bloom ? "1" : "0"),
                "flash=" + (Flash ? "1" : "0"),
                "hints=" + (Hints ? "1" : "0"),
                "vibration=" + (Vibration ? "1" : "0")
            });
        }
        catch (Exception) { /* Ayarlar yazılamadı: yalnızca bu oturumda geçerli */ }
    }

    private static int ParseInt(string text, int fallback) =>
        int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) ? value : fallback;
}
