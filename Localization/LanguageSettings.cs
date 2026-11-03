using System;
using System.IO;

namespace TheLastDelivery.Localization;

/// <summary>
/// Son seçilen dili hatırlar; dil seçim ekranı açılışta bu dili işaretli gösterir.
/// Ayar dosyası: %LocalAppData%\TheLastDelivery\language.txt
/// Dosya okunamaz/yazılamazsa sessizce varsayılana döner.
/// </summary>
public static class LanguageSettings
{
    private static string FilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TheLastDelivery", "language.txt");

    public static Language LoadOrDefault(Language fallback)
    {
        try
        {
            if (File.Exists(FilePath) && Enum.TryParse(File.ReadAllText(FilePath).Trim(), out Language saved))
                return saved;
        }
        catch (Exception)
        {
            // Ayar okunamadı: varsayılan dil
        }
        return fallback;
    }

    public static void Save(Language language)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
            File.WriteAllText(FilePath, language.ToString());
        }
        catch (Exception)
        {
            // Ayar kaydedilemedi: bir sonraki açılışta yine seçim ekranı gelir, sorun değil
        }
    }
}
