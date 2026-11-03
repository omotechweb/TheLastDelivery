using System;
using System.Collections.Generic;
using System.Linq;
using TheLastDelivery.Core;
using TheLastDelivery.Story;

namespace TheLastDelivery.World;

/// <summary>Rotadaki bir durağın türü.</summary>
public enum StopKind
{
    Station, // Terk edilmiş istasyon: ahlaki seçim (ilk dördü hikâye, sonrakiler peronda bekleyen bir yabancı)
    Town,    // Canlı kasaba: yürüyüş modu
    Depot,   // Buzul benzinliği: tüccar ve pompa
    Hub,     // Büyük ana istasyon: büyük tüccar, tren geliştirmeleri
    Shelter  // Sığınak-0 (zamanla yarışta teslimat noktası): final
}

/// <summary>Rotadaki tek bir durak. Index, türüne göre senaryo/kasaba/benzinlik sırasıdır.</summary>
public sealed record RouteStop(float Km, StopKind Kind, int Index);

/// <summary>
/// Hikâye modunun 2000 kilometrelik hattı, kilometreye göre sıralı:
/// istasyonlar, kasabalar, benzinlikler, ana istasyonlar ve Sığınak-0 (bkz. GameConfig).
/// Zamanla yarış hattı ve sonsuz modun ileriye doğru üretilen hattı da buradan alınır.
/// </summary>
public static class Route
{
    public static RouteStop[] Stops { get; private set; } = Build();

    /// <summary>Zamanla yarış: üç hızlı ikmal benzinliği ve teslimat noktası.</summary>
    public static readonly RouteStop[] SpeedrunStops = BuildSpeedrun();

    /// <summary>
    /// Episode 2 "Project AURA": Sığınak-0'dan Kutup Askeri Yeraltı Üssü'ne 2000 km'lik Kutup Hattı.
    /// Hikâye bölümlerinin (2, 10, 15, 20, 31, 34, 37, 40) km aralıklarında durak yoktur.
    /// </summary>
    public static readonly RouteStop[] Episode2Stops = BuildEpisode2();

    /// <summary>DLC "Buzul Kıyameti" hattı: 500 km, sonunda Sığınak-0'ın dış kapısı.</summary>
    public static readonly RouteStop[] WastelandStops = BuildWasteland();

    /// <summary>Verilen kilometreden SONRA gelen ilk durağın sırası (bölüm seçiminde başlangıç için).</summary>
    public static int FirstStopAfter(IReadOnlyList<RouteStop> stops, float km)
    {
        for (int i = 0; i < stops.Count; i++)
            if (stops[i].Km > km) return i;
        return stops.Count - 1;
    }

    public static int FirstStopAfter(float km) => FirstStopAfter(Stops, km);

    /// <summary>
    /// Modlardan gelen ek durakları (benzinlik ya da istasyon) hikâye hattına ekler.
    /// Geçersiz ya da var olan bir durağa 6 km'den yakın duraklar atlanır.
    /// </summary>
    /// <returns>Eklenen durak sayısı.</returns>
    public static int AddModStops(IEnumerable<RouteStop> extra)
    {
        var stops = new List<RouteStop>(Stops);
        int added = 0;
        foreach (var stop in extra)
        {
            if (stop.Km <= 10f || stop.Km >= GameConfig.TotalDistanceKm - 10f) continue;
            if (stop.Kind is not (StopKind.Depot or StopKind.Station)) continue;
            if (stops.Any(s => MathF.Abs(s.Km - stop.Km) < 6f)) continue;
            stops.Add(stop);
            added++;
        }
        Stops = stops.OrderBy(s => s.Km).ToArray();
        return added;
    }

    private static RouteStop[] Build()
    {
        var stops = new List<RouteStop>();

        for (int i = 0; i < GameConfig.StationKms.Length; i++)
            stops.Add(new RouteStop(GameConfig.StationKms[i], StopKind.Station, i));

        for (int i = 0; i < GameConfig.TownKms.Length; i++)
            stops.Add(new RouteStop(GameConfig.TownKms[i], StopKind.Town, i));

        for (int i = 0; i < GameConfig.DepotKms.Length; i++)
            stops.Add(new RouteStop(GameConfig.DepotKms[i], StopKind.Depot, i));

        for (int i = 0; i < GameConfig.HubKms.Length; i++)
            stops.Add(new RouteStop(GameConfig.HubKms[i], StopKind.Hub, i));

        stops.Add(new RouteStop(GameConfig.TotalDistanceKm, StopKind.Shelter, 0));

        return stops.Where(s => s.Km <= GameConfig.TotalDistanceKm).OrderBy(s => s.Km).ToArray();
    }

    private static RouteStop[] BuildWasteland()
    {
        var stops = new List<RouteStop>();
        // Yabancılar her zaman hain olabilir (hikâye senaryoları burada yok)
        for (int i = 0; i < GameConfig.WastelandStationKms.Length; i++)
            stops.Add(new RouteStop(GameConfig.WastelandStationKms[i], StopKind.Station, Story.StoryData.Scenarios.Length + i));
        for (int i = 0; i < GameConfig.WastelandTownKms.Length; i++)
            stops.Add(new RouteStop(GameConfig.WastelandTownKms[i], StopKind.Town, 3 + i)); // Kuzgun Geçidi ve Tuzla Ocağı
        for (int i = 0; i < GameConfig.WastelandDepotKms.Length; i++)
            stops.Add(new RouteStop(GameConfig.WastelandDepotKms[i], StopKind.Depot, i));
        for (int i = 0; i < GameConfig.WastelandHubKms.Length; i++)
            stops.Add(new RouteStop(GameConfig.WastelandHubKms[i], StopKind.Hub, i));
        stops.Add(new RouteStop(GameConfig.WastelandDistanceKm, StopKind.Shelter, 0));
        return stops.OrderBy(s => s.Km).ToArray();
    }

    private static RouteStop[] BuildEpisode2()
    {
        var stops = new List<RouteStop>();
        // İstasyonlarda her zaman yabancılar bekler (Episode 1'in hikâye senaryoları burada yok)
        for (int i = 0; i < GameConfig.Episode2StationKms.Length; i++)
            stops.Add(new RouteStop(GameConfig.Episode2StationKms[i], StopKind.Station, StoryData.Scenarios.Length + i));
        for (int i = 0; i < GameConfig.Episode2TownKms.Length; i++)
            stops.Add(new RouteStop(GameConfig.Episode2TownKms[i], StopKind.Town, GameConfig.Episode2FirstTown + i));
        for (int i = 0; i < GameConfig.Episode2DepotKms.Length; i++)
            stops.Add(new RouteStop(GameConfig.Episode2DepotKms[i], StopKind.Depot, i));
        for (int i = 0; i < GameConfig.Episode2HubKms.Length; i++)
            stops.Add(new RouteStop(GameConfig.Episode2HubKms[i], StopKind.Hub, i));
        stops.Add(new RouteStop(GameConfig.TotalDistanceKm, StopKind.Shelter, 0));
        return stops.OrderBy(s => s.Km).ToArray();
    }

    private static RouteStop[] BuildSpeedrun()
    {
        var stops = new List<RouteStop>();
        for (int i = 0; i < GameConfig.SpeedrunDepotKms.Length; i++)
            stops.Add(new RouteStop(GameConfig.SpeedrunDepotKms[i], StopKind.Depot, i));
        stops.Add(new RouteStop(GameConfig.SpeedrunDistanceKm, StopKind.Shelter, 0));
        return stops.ToArray();
    }
}

/// <summary>
/// Sonsuz kurye modunun hattı: tren ilerledikçe 250 km'lik bloklar halinde yeni duraklar üretilir.
/// Her blokta bir benzinlik, yabancılı bir istasyon ve bir kasaba; iki blokta bir ana istasyon vardır.
/// Hat hiç bitmez; Sığınak-0 yoktur.
/// </summary>
public sealed class EndlessRoute
{
    private readonly List<RouteStop> _stops = new();
    private int _blocks;

    public IReadOnlyList<RouteStop> Stops => _stops;

    public void Reset()
    {
        _stops.Clear();
        _blocks = 0;
        EnsureAhead(0f);
    }

    /// <summary>Verilen kilometrenin en az iki blok ilerisine kadar durak üretir.</summary>
    public void EnsureAhead(float km)
    {
        while (_blocks * GameConfig.EndlessStageKm < km + GameConfig.EndlessStageKm * 2f)
        {
            int b = _blocks++;
            float start = b * GameConfig.EndlessStageKm;
            _stops.Add(new RouteStop(start + 80f, StopKind.Depot, b));
            _stops.Add(new RouteStop(start + 150f, StopKind.Station, StoryData.Scenarios.Length + b)); // Her zaman yabancı
            _stops.Add(new RouteStop(start + 215f, StopKind.Town, b % TownCatalog.All.Length));
            if (b % 2 == 1)
                _stops.Add(new RouteStop(start + 242f, StopKind.Hub, b / 2));
        }
    }
}
