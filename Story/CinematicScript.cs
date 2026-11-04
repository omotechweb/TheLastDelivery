using System.Collections.Generic;
using TheLastDelivery.UI;

namespace TheLastDelivery.Story;

/// <summary>
/// Uzun hikâye anlatım sahnelerinin (sinematiklerin) senaryosu. Her satır bir sayfadır: (sahne, çizim, İngilizce, Türkçe).
/// Episode 1'in ve Episode 2'nin 40'ar bölümünün her biri sürüşten önce böyle bir sahneyle açılır.
/// Sahne metinleri seçimlerden bağımsızdır (oyuncunun kararlarını varsaymaz); seçimlere bağlı anlatım
/// sürüş içindeki hikâye anlarında ve diyaloglardadır. Diğer diller İngilizceye düşer.
/// </summary>
public static partial class CinematicScript
{
    static CinematicScript()
    {
        foreach (var page in Episode1) Cinematics.Add(page.Scene, page.Art);
        foreach (var page in Episode2) Cinematics.Add(page.Scene, page.Art);
    }

    /// <summary>Statik kurucunun (sahne kaydı) çalıştığından emin olur.</summary>
    public static void EnsureLoaded() { }

    /// <summary>Sayfa metinlerini çeviri tablosuna ekler ("cs.{sahne}.{sayfa}").</summary>
    public static void AddTexts(Dictionary<string, string> table, bool english)
    {
        var counts = new Dictionary<string, int>();
        foreach (var list in new[] { Episode1, Episode2 })
            foreach (var page in list)
            {
                counts.TryGetValue(page.Scene, out int n);
                counts[page.Scene] = ++n;
                table[$"cs.{page.Scene}.{n}"] = english ? page.En : page.Tr;
            }
    }
}
