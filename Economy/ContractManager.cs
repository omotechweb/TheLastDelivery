using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TheLastDelivery.Core;
using TheLastDelivery.Localization;

namespace TheLastDelivery.Economy;

/// <summary>Görev hedeflerinin türleri (oyun olayları ContractManager.Report ile bildirir).</summary>
public enum ContractKind
{
    TravelKm,        // Toplam yol al
    Rescue,          // Trene yolcu al (istasyon, taverna, imdat)
    NoDamageChapter, // Hiç hasar almadan bir bölümü tamamla
    RepelWolves,     // Kurt sürüsü püskürt
    AnswerSos,       // İmdat çağrısına yanıt ver
    FeedPassengers,  // Yolculara erzak dağıt
    EscapeAmbush,    // Mafya pususundan kaç
    MineOre          // Madenden değerli cevher çıkar
}

/// <summary>Kayıtta tutulan tek bir görev.</summary>
public sealed class ContractState
{
    public string Kind { get; set; }
    public int Target { get; set; }
    public float Progress { get; set; }
    public bool Weekly { get; set; }
    public bool Completed { get; set; }
    public int RewardCredits { get; set; }
    public int RewardTickets { get; set; }

    public ContractKind KindValue => Enum.TryParse<ContractKind>(Kind, out var k) ? k : ContractKind.TravelKm;
    public string Description => Loc.F($"contract.{Kind}", Target);
    public int ProgressInt => Math.Min(Target, (int)MathF.Floor(Progress));
}

/// <summary>
/// Günlük ve haftalık görevler. Her gün 3 günlük, her hafta (ISO haftası) 3 haftalık görev tarihten
/// üretilir (aynı gün herkes aynı görevleri alır). Tamamlanan görevin ödülü (Kredi ya da kozmetik bileti)
/// anında verilir. İlerleme bellekte birikir, belirli aralıklarla ve durum değişimlerinde kaydedilir.
/// </summary>
public static class ContractManager
{
    private static bool _dirty;

    private static readonly (ContractKind Kind, int Daily, int Weekly)[] Pool =
    {
        (ContractKind.TravelKm, 300, 1000),
        (ContractKind.Rescue, 2, 6),
        (ContractKind.NoDamageChapter, 1, 3),
        (ContractKind.RepelWolves, 1, 4),
        (ContractKind.AnswerSos, 1, 3),
        (ContractKind.FeedPassengers, 3, 12),
        (ContractKind.EscapeAmbush, 1, 4),
        (ContractKind.MineOre, 2, 8),
    };

    public static IEnumerable<ContractState> Daily => SaveData.Current.Contracts.Where(c => !c.Weekly);
    public static IEnumerable<ContractState> Weekly => SaveData.Current.Contracts.Where(c => c.Weekly);

    public static string DayKey(DateTime d) => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    public static string WeekKey(DateTime d) => $"{ISOWeek.GetYear(d)}-W{ISOWeek.GetWeekOfYear(d):00}";

    /// <summary>Gün ya da hafta değiştiyse yeni görevleri üretir.</summary>
    public static void Refresh()
    {
        var save = SaveData.Current;
        var today = SeasonalEvents.Today;
        bool changed = false;

        if (save.ContractDay != DayKey(today))
        {
            save.Contracts.RemoveAll(c => !c.Weekly);
            save.Contracts.AddRange(Generate(today.DayOfYear + today.Year * 400, weekly: false));
            save.ContractDay = DayKey(today);
            changed = true;
        }
        if (save.ContractWeek != WeekKey(today))
        {
            save.Contracts.RemoveAll(c => c.Weekly);
            save.Contracts.AddRange(Generate(ISOWeek.GetWeekOfYear(today) * 97 + ISOWeek.GetYear(today), weekly: true));
            save.ContractWeek = WeekKey(today);
            changed = true;
        }
        if (changed) SaveData.Save();
    }

    private static IEnumerable<ContractState> Generate(int seed, bool weekly)
    {
        var rng = new Random(seed);
        var picks = Pool.OrderBy(_ => rng.Next()).Take(3);
        float bonus = SeasonalEvents.RewardMultiplier;
        foreach (var p in picks)
        {
            int target = weekly ? p.Weekly : p.Daily;
            yield return new ContractState
            {
                Kind = p.Kind.ToString(),
                Target = target,
                Weekly = weekly,
                RewardCredits = (int)MathF.Round((weekly ? 80 : 25 + rng.Next(3) * 5) * bonus),
                RewardTickets = weekly ? 1 : 0
            };
        }
    }

    /// <summary>Bir olayı bildirir; bu bildirimle tamamlanan görevleri (ödülleri verilmiş olarak) döner.</summary>
    public static List<ContractState> Report(ContractKind kind, float amount)
    {
        var done = new List<ContractState>();
        if (amount <= 0f) return done;
        string name = kind.ToString();
        foreach (var c in SaveData.Current.Contracts)
        {
            if (c.Completed || c.Kind != name) continue;
            c.Progress = MathF.Min(c.Target, c.Progress + amount);
            _dirty = true;
            if (c.Progress < c.Target) continue;

            c.Completed = true;
            SaveData.Current.CosmeticTickets += c.RewardTickets;
            SaveData.Current.ContractsCompleted++;
            if (c.RewardCredits > 0) SaveData.Earn(c.RewardCredits); // Earn kayıt da yapar
            done.Add(c);
        }
        if (done.Count > 0) Flush();
        return done;
    }

    /// <summary>Bellekteki ilerlemeyi kaydeder (her karede değil, durum değişimlerinde çağrılır).</summary>
    public static void Flush()
    {
        if (!_dirty) return;
        _dirty = false;
        SaveData.Save();
    }

    /// <summary>Sıradaki günlük ve haftalık yenilemeye kalan süre.</summary>
    public static TimeSpan UntilDailyReset => DateTime.Today.AddDays(1) - DateTime.Now;

    public static TimeSpan UntilWeeklyReset
    {
        get
        {
            int days = ((int)DayOfWeek.Monday - (int)DateTime.Today.DayOfWeek + 7) % 7;
            if (days == 0) days = 7;
            return DateTime.Today.AddDays(days) - DateTime.Now;
        }
    }
}
