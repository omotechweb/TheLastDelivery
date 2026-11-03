using System;
using System.Globalization;

namespace TheLastDelivery.Core;

/// <summary>Takvime bağlı canlı etkinlik.</summary>
public enum Season
{
    None,
    WinterFestival, // 15 Aralık - 6 Ocak: ışıklarla süslü trenler, görev kredileri +%50
    GhostWeek,      // 25 Ekim - 2 Kasım: balkabağı fenerleri, gece hayalet tren görülür
    StormDay        // Her hafta perşembe: fırtına daha sert esiyor, görev kredileri iki katı
}

/// <summary>
/// Sistem tarihine göre dönemsel etkinlikler. Test ve önizleme için TLD_DATE ortam değişkeni
/// (yyyy-MM-dd) tarihi değiştirebilir.
/// </summary>
public static class SeasonalEvents
{
    /// <summary>Etkinlik hesaplarında kullanılan bugünün tarihi.</summary>
    public static DateTime Today
    {
        get
        {
            string over = Environment.GetEnvironmentVariable("TLD_DATE");
            if (!string.IsNullOrWhiteSpace(over) &&
                DateTime.TryParseExact(over, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                return date;
            return DateTime.Now.Date;
        }
    }

    public static Season Current => For(Today);

    public static Season For(DateTime date)
    {
        if ((date.Month == 12 && date.Day >= 15) || (date.Month == 1 && date.Day <= 6)) return Season.WinterFestival;
        if ((date.Month == 10 && date.Day >= 25) || (date.Month == 11 && date.Day <= 2)) return Season.GhostWeek;
        if (date.DayOfWeek == DayOfWeek.Thursday) return Season.StormDay;
        return Season.None;
    }

    /// <summary>Görev kredi ödüllerinin çarpanı.</summary>
    public static float RewardMultiplier => Current switch
    {
        Season.WinterFestival => 1.5f,
        Season.StormDay => 2f,
        _ => 1f
    };

    /// <summary>Fırtına gününde bölümlerin fırtına üst sınırına eklenen şiddet.</summary>
    public static float ExtraStorm => Current == Season.StormDay ? 0.3f : 0f;

    /// <summary>Çeviri anahtarı ("season.winter" vb.); etkinlik yoksa null.</summary>
    public static string Key => Current switch
    {
        Season.WinterFestival => "season.winter",
        Season.GhostWeek => "season.ghost",
        Season.StormDay => "season.storm",
        _ => null
    };
}
