using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using TheLastDelivery.Core;
using TheLastDelivery.Localization;
using TheLastDelivery.Passengers;

namespace TheLastDelivery.Story;

public enum EndingType
{
    NewBeginning,   // İyi final: Sığınak-0 + en az 2 yolcu hayatta
    LoneWolf,       // Buruk final: Sığınak-0 + yolcu yok (ya da tek kişi)
    LostInDarkness, // Trajik final: yolda yakıt/sıcaklık/sağlık/paket tükendi
    Merchant,       // Karanlık / Tüccar sonu: paket Kızıl Kurtlar'a satıldı
    Sacrifice,      // Gizli Feda sonu: Arthur insanlık için treni patlatır
    Thaw,           // Gizli Erime sonu: çekirdek AURORA kulesine götürülür, kış geri çevrilir
    AuraFree,       // Episode 2: İnsanlık Özgürdür (AURA yok edildi)
    AuraMachine,    // Episode 2: Makinelerin Mantığı (Arthur AURA ile birleşti)
    AuraReset       // Episode 2: Büyük Sıfırlama (insanlık uykuya)
}

/// <summary>Hikâyenin dört sonu (kayıtta görülen sonlar bu kimliklerle tutulur).</summary>
public static class EndingIds
{
    public const string Hero = "hero", LoneWolf = "lonewolf", Merchant = "merchant", Sacrifice = "sacrifice", Thaw = "thaw";
    public static readonly string[] All = { Hero, LoneWolf, Merchant, Sacrifice, Thaw };

    // Episode 2 "Project AURA"
    public const string AuraFree = "aura.free", AuraMachine = "aura.machine", AuraReset = "aura.reset";
    public static readonly string[] Episode2 = { AuraFree, AuraMachine, AuraReset };
}

/// <summary>Final ekranında gösterilecek her şey (aktif dilde).</summary>
public sealed record Ending(EndingType Type, string Title, string Tagline, string Name, string Text);

/// <summary>Oyuncunun yolculuk boyunca verdiği kararlardan finali belirler.</summary>
public static class Endings
{
    /// <summary>Sığınak-0'a ulaşıldığında: final hayatta kalan yolculara bağlı.</summary>
    public static Ending ForArrival(IReadOnlyList<Passenger> survivors, int abandonedCount, int lostCount)
    {
        string title = Loc.T("end.success.title");
        string tagline = Loc.T("end.success.tagline");
        var text = new StringBuilder();

        if (survivors.Count >= GameConfig.GoodEndingMinPassengers)
        {
            text.Append(Loc.T("end.good.text"));
            text.Append('\n').Append(Loc.F("end.good.together", JoinNames(survivors)));
            if (survivors.Any(p => p.Skill == PassengerSkill.Child))
                text.Append('\n').Append(Loc.T("end.good.child"));
            AppendRemembrance(text, abandonedCount, lostCount);
            return new Ending(EndingType.NewBeginning, title, tagline, Loc.T("end.good.name"), text.ToString());
        }

        text.Append(Loc.T("end.bitter.text"));
        text.Append('\n').Append(survivors.Count == 0
            ? Loc.T("end.bitter.none")
            : Loc.F("end.bitter.one", survivors[0].Name));
        AppendRemembrance(text, abandonedCount, lostCount);
        return new Ending(EndingType.LoneWolf, title, tagline, Loc.T("end.bitter.name"), text.ToString());
    }

    /// <summary>
    /// Sığınak-0'ın kapısından girildi. Gizli karma ve hayatta kalanlar sonu belirler:
    /// en az iki yolcu ve iyi (ya da gri) karma → Kahraman; aksi halde → Yalnız Kurt.
    /// </summary>
    public static (Ending Ending, string Id) ForGate(IReadOnlyList<Passenger> survivors, int abandonedCount, int lostCount, int karma)
    {
        bool hero = survivors.Count >= GameConfig.GoodEndingMinPassengers && karma >= 0;
        var arrival = ForArrival(survivors, abandonedCount, lostCount);
        var text = new StringBuilder(hero ? Loc.T("end.good.text") : Loc.T("end.bitter.text"));
        if (hero)
        {
            text.Append('\n').Append(Loc.F("end.good.together", JoinNames(survivors)));
            text.Append('\n').Append(Loc.T("end.hero.line"));
        }
        else
        {
            text.Append('\n').Append(survivors.Count == 0 ? Loc.T("end.bitter.none") : Loc.T("end.lonewolf.line"));
        }
        AppendRemembrance(text, abandonedCount, lostCount);
        return hero
            ? (arrival with { Type = EndingType.NewBeginning, Name = Loc.T("end.hero.name"), Text = text.ToString() }, EndingIds.Hero)
            : (arrival with { Type = EndingType.LoneWolf, Name = Loc.T("end.lonewolf.name"), Text = text.ToString() }, EndingIds.LoneWolf);
    }

    /// <summary>Karanlık / Tüccar sonu: paket Kızıl Kurtlar'a satıldı, Arthur zengin ama Sığınak-0 karanlıkta.</summary>
    public static Ending ForMerchant(int credits, int survivors)
    {
        var text = new StringBuilder(Loc.T("end.merchant.text"));
        text.Append('\n').Append(Loc.F("end.merchant.reward", credits));
        if (survivors > 0) text.Append('\n').Append(Loc.F("end.merchant.passengers", survivors));
        return new Ending(EndingType.Merchant, Loc.T("end.merchant.title"), Loc.T("end.merchant.tagline"),
            Loc.T("end.merchant.name"), text.ToString());
    }

    /// <summary>Gizli Feda sonu: Arthur, mafyanın zırhlı trenini köprüyle birlikte havaya uçurur; yolcular kapıya yürür.</summary>
    public static Ending ForSacrifice(IReadOnlyList<Passenger> survivors)
    {
        // Kurt Baba'yla ateşkes yapıldıysa köprüye gelen zırhlı tren Konsey'indir
        var text = new StringBuilder(Loc.T(Narrative.Has(Flags.WolfTruce) ? "end.sacrifice.text.council" : "end.sacrifice.text"));
        if (survivors.Count > 0)
            text.Append('\n').Append(Loc.F("end.sacrifice.passengers", JoinNames(survivors)));
        text.Append('\n').Append(Loc.T("end.sacrifice.last"));
        return new Ending(EndingType.Sacrifice, Loc.T("end.sacrifice.title"), Loc.T("end.sacrifice.tagline"),
            Loc.T("end.sacrifice.name"), text.ToString());
    }

    /// <summary>
    /// Gizli Erime sonu: Arthur kapıdan girmez; çekirdeği Sofi'nin beklediği Enstitü kulesine götürür
    /// ve AURORA'yı tersine çalıştırır. Koşul: Ters Döngü verileri, Sofi'ye güven, Konsey'le anlaşma yok.
    /// </summary>
    public static Ending ForThaw(IReadOnlyList<Passenger> survivors)
    {
        var text = new StringBuilder(Loc.T("end.thaw.text"));
        if (survivors.Count > 0)
            text.Append('\n').Append(Loc.F("end.thaw.passengers", JoinNames(survivors)));
        text.Append('\n').Append(Loc.T("end.thaw.last"));
        return new Ending(EndingType.Thaw, Loc.T("end.thaw.title"), Loc.T("end.thaw.tagline"),
            Loc.T("end.thaw.name"), text.ToString());
    }

    /// <summary>
    /// Yolculuktaki kararların sonsözü: sınırdaki aile, karargah kayıtları, Konsey anlaşması, Sofi.
    /// Her sonun metnine eklenir (seçimlerin hikâyeye etkisi).
    /// </summary>
    public static string Epilogue(string endingId, IReadOnlyList<Passenger> survivors = null)
    {
        var lines = new List<string>();

        // Kurt Baba'nın ve Kızıl Kurtlar'ın akıbeti
        if (Narrative.Has(Flags.WolfTruce)) lines.Add(Loc.T("epi.wolf.truce"));
        else if (Narrative.Has(Flags.WolfEnemy)) lines.Add(Loc.T("epi.wolf.enemy"));
        else if (Narrative.Has(Flags.WolfDeal) && endingId != EndingIds.Merchant) lines.Add(Loc.T("epi.wolf.broken"));
        if (Narrative.Has(Flags.WolfBora)) lines.Add(Loc.T("epi.wolf.bora"));
        if (Narrative.Has(Flags.AcolyteSaved)) lines.Add(Loc.T("epi.acolyte"));

        if (Narrative.Has(Flags.BorderRefugees)) lines.Add(Loc.T("epi.refugees"));
        if (Narrative.Has(Flags.BorderBetrayed)) lines.Add(Loc.T("epi.betrayed"));
        if (Narrative.Has(Flags.HqBroadcast)) lines.Add(Loc.T("epi.broadcast"));
        if (Narrative.Has(Flags.MineWorkers)) lines.Add(Loc.T("epi.workers"));
        if (Narrative.Has(Flags.MineOwner)) lines.Add(Loc.T("epi.owner"));
        if (Narrative.Has(Flags.CouncilDeal) && endingId is EndingIds.Hero or EndingIds.LoneWolf) lines.Add(Loc.T("epi.deal"));
        if (Narrative.Has(Flags.CouncilWitnessed)) lines.Add(Loc.F("epi.witness", Narrative.CountFlags(NarrativeData.WitnessPrefix)));
        if (Narrative.Has(Flags.SofiKnown) && endingId != EndingIds.Thaw) lines.Add(Loc.T("epi.sofi"));
        if (Narrative.Has(Flags.AuroraDestroyed)) lines.Add(Loc.T("epi.destroyed"));

        // Yolcuların her biri için "sonra ne oldu" satırı (Tüccar sonunda kapı kimseye açılmadı)
        if (survivors != null && survivors.Count > 0 && endingId != EndingIds.Merchant)
        {
            lines.Add("");
            lines.Add(Loc.T("epi.after"));
            foreach (var p in survivors)
            {
                // En özelden en genele: görevin sonu + final → görevin sonu → yolcunun satırı → genel satır
                string id = p.Profile.Id, end = QuestEnd(id), line = null;
                foreach (string key in new[] { end != null ? $"epi.p.{id}.{end}.{endingId}" : null, end != null ? $"epi.p.{id}.{end}" : null, $"epi.p.{id}" })
                {
                    if (key == null) continue;
                    string text = Loc.T(key);
                    if (text != key) { line = text; break; }
                }
                lines.Add(line ?? Loc.F("epi.p.generic", p.Name));
            }
        }
        return string.Join("\n", lines);
    }

    /// <summary>
    /// Episode 2'nin üç sonu: AURA'nın çekirdeğinde verilen karar. Yolcular, Episode 2 kararları (uyandırılan
    /// uyuyanlar, NULL tarikatının dağılması, virüs) ve Episode 1'den gelen Sofi bağı sonsözü değiştirir.
    /// </summary>
    public static (Ending Ending, string Id) ForAura(IReadOnlyList<Passenger> survivors)
    {
        string key, id;
        EndingType type;
        if (Narrative.Has(Flags.E2EndMachine)) { key = "machine"; id = EndingIds.AuraMachine; type = EndingType.AuraMachine; }
        else if (Narrative.Has(Flags.E2EndReset)) { key = "reset"; id = EndingIds.AuraReset; type = EndingType.AuraReset; }
        else { key = "free"; id = EndingIds.AuraFree; type = EndingType.AuraFree; }

        var text = new StringBuilder(Loc.T($"end.aura.{key}.text"));
        if (survivors.Count > 0)
            text.Append('\n').Append(Loc.F($"end.aura.{key}.passengers", JoinNames(survivors)));
        if (Narrative.Has(Flags.E2Woke)) text.Append('\n').Append(Loc.T($"epi2.woke.{key}"));
        if (Narrative.Has(Flags.E2NullBroken)) text.Append('\n').Append(Loc.T("epi2.nullBroken"));
        if (Narrative.Has(Flags.E2Virus) && key == "free") text.Append('\n').Append(Loc.T("epi2.virus"));
        if (Narrative.Has("ep1:" + Flags.SofiKnown)) text.Append('\n').Append(Loc.T($"epi2.sofi.{key}"));
        return (new Ending(type, Loc.T($"end.aura.{key}.title"), Loc.T($"end.aura.{key}.tagline"),
            Loc.T($"end.aura.{key}.name"), text.ToString()), id);
    }

    /// <summary>Yolcunun görev zincirinin son adımında verilen karar ("end1" / "end2") ya da null (görev bitmedi).</summary>
    private static string QuestEnd(string id)
    {
        if (Narrative.Has(NarrativeData.QuestEndFlag(id, 1))) return "end1";
        if (Narrative.Has(NarrativeData.QuestEndFlag(id, 2))) return "end2";
        return null;
    }

    /// <summary>Yolcunun kendi hikâyesinin sonucu (sonsözde ona özel satırı seçer) ya da null.</summary>
    private static string PassengerVariant(string id) => id switch
    {
        "yusuf" => Narrative.Has(Flags.E2Brother) ? null : Narrative.Has(Flags.E2BrotherLeft) ? "left" : "alone",
        "mara" => Narrative.Has(Flags.E2MaraDaughter) ? "lena" : null,
        "kiril" => Narrative.Has(Flags.E2KirilFaced) ? "faced" : null,
        _ => QuestEnd(id)
    };

    /// <summary>Çekirdekte AURA'ya kaç umut argümanı söylendi (yolculukta yaşanan anlardan doğar).</summary>
    public static int HopeCount()
    {
        int count = 0;
        foreach (string arg in Flags.HopeArguments)
            if (Narrative.Has(Flags.HopePrefix + arg)) count++;
        return count;
    }

    /// <summary>
    /// Episode 2'nin sonsözü: sürüşte verilen kararlar (Tali, Ilka, Kızıl Birlik, ateş başındaki söz, defter),
    /// AURA'ya söylenen umut argümanları ve her yolcunun "sonra ne oldu" satırı. Büyük Sıfırlama'da herkes uyur.
    /// </summary>
    public static string Epilogue2(string endingId, IReadOnlyList<Passenger> survivors)
    {
        string key = endingId switch { EndingIds.AuraMachine => "machine", EndingIds.AuraReset => "reset", _ => "free" };
        var lines = new List<string>();

        int hope = HopeCount();
        if (hope >= 3) lines.Add(Loc.T($"epi2.hope.many.{key}"));
        else if (hope >= 1) lines.Add(Loc.T($"epi2.hope.some.{key}"));

        if (Narrative.Has(Flags.E2Child)) lines.Add(Loc.T($"epi2.tali.{key}"));
        if (Narrative.Has(Flags.E2AcolyteFriend)) lines.Add(Loc.T("epi2.ilka"));
        if (Narrative.Has(Flags.E2WolfHelp)) lines.Add(Loc.T("epi2.wolf"));
        if (Narrative.Has(Flags.E2Promise)) lines.Add(Loc.T($"epi2.promise.{key}"));
        else if (Narrative.Has(Flags.E2Truth)) lines.Add(Loc.T("epi2.truth"));
        if (Narrative.Has(Flags.E2LogKept)) lines.Add(Loc.T("epi2.log"));
        if (Narrative.Has(Flags.E2OracleRides)) lines.Add(Loc.T($"epi2.oracle.{key}"));

        if (survivors != null && survivors.Count > 0)
        {
            lines.Add("");
            if (key == "reset")
            {
                lines.Add(Loc.F("epi2.after.reset", JoinNames(survivors)));
            }
            else
            {
                lines.Add(Loc.T("epi.after"));
                foreach (var p in survivors)
                {
                    string id = p.Profile.Id, line = null;
                    // En özelden en genele: kişisel karar → sona özel → yolcunun satırı → genel satır
                    foreach (string pk in new[] { PassengerVariant(id) is { } v ? $"epi2.p.{id}.{v}" : null, $"epi2.p.{id}.{key}", $"epi2.p.{id}" })
                    {
                        if (pk == null) continue;
                        string text = Loc.T(pk);
                        if (text != pk) { line = text; break; }
                    }
                    lines.Add(line ?? Loc.F($"epi2.p.generic.{key}", p.Name));
                }
            }
        }
        return string.Join("\n", lines);
    }

    /// <summary>Yolda kalındığında: her ölüm nedeni kendi açılış cümlesiyle trajik finale bağlanır.</summary>
    public static Ending ForDeath(DeathReason reason, int passengersAboard)
    {
        var text = new StringBuilder(Loc.T(reason switch
        {
            DeathReason.OutOfFuel => "death.fuel",
            DeathReason.Derailed => "death.derail",
            DeathReason.PackageLost => "death.package",
            DeathReason.TimeUp => "death.timeup",
            DeathReason.Madness => "death.madness",
            _ => "death.frost"
        }));

        // Süre dolması bir donma ölümü değildir: kendi başlığı ve kapanış cümlesi var
        bool timeUp = reason == DeathReason.TimeUp;
        text.Append('\n').Append(Loc.T(timeUp ? "end.timeup.text" : "end.tragic.text"));
        if (passengersAboard > 0)
            text.Append('\n').Append(Loc.F("end.tragic.passengers", passengersAboard));

        return new Ending(EndingType.LostInDarkness, Loc.T(timeUp ? "end.timeup.title" : "end.tragic.title"),
            Loc.T("end.tragic.tagline"), Loc.T(timeUp ? "end.timeup.name" : "end.tragic.name"), text.ToString());
    }

    /// <summary>Sonsuz kurye modunun sonu: katedilen yol, aşama, kazanılan Kredi ve rekor.</summary>
    public static Ending ForEndless(DeathReason reason, int km, int stage, int credits, bool newRecord, int bestKm,
        int rank, int score)
    {
        var text = new StringBuilder(Loc.T(reason switch
        {
            DeathReason.OutOfFuel => "death.fuel",
            DeathReason.Derailed => "death.derail",
            DeathReason.PackageLost => "death.package",
            _ => "death.frost"
        }));
        text.Append('\n').Append(Loc.F("end.endless.text", km, stage, credits));
        text.Append('\n').Append(newRecord ? Loc.F("end.endless.record", km) : Loc.F("end.endless.best", bestKm));
        text.Append('\n').Append(rank > 0
            ? Loc.F("end.endless.rank", rank, score)       // Liderlik tablosuna girdi
            : Loc.F("end.endless.unranked", score));
        return new Ending(EndingType.LostInDarkness, Loc.T("end.endless.title"), Loc.F("end.endless.tagline", km),
            Loc.T("end.endless.name"), text.ToString());
    }

    /// <summary>Zamanla yarışın başarılı sonu: süre, ödül ve rekor.</summary>
    public static Ending ForSpeedrun(float seconds, int credits, bool newRecord, float bestSeconds)
    {
        var text = new StringBuilder(Loc.F("end.speedrun.text", FormatTime(seconds), credits));
        text.Append('\n').Append(newRecord ? Loc.T("end.speedrun.record") : Loc.F("end.speedrun.best", FormatTime(bestSeconds)));
        return new Ending(EndingType.NewBeginning, Loc.T("end.speedrun.title"), Loc.F("end.speedrun.tagline", FormatTime(seconds)),
            Loc.T("end.speedrun.name"), text.ToString());
    }

    /// <summary>DLC Buzul Kıyameti'nin sonu: zehirli çorak topraklar aşıldı, Sığınak-0'ın dış kapısına varıldı.</summary>
    public static Ending ForWasteland(int survivors, int reward)
    {
        var text = new StringBuilder(Loc.T("end.wasteland.text"));
        text.Append('\n').Append(Loc.F("end.wasteland.survivors", survivors));
        if (reward > 0) text.Append('\n').Append(Loc.F("end.wasteland.reward", reward));
        return new Ending(EndingType.NewBeginning, Loc.T("end.wasteland.title"), Loc.T("end.wasteland.tagline"),
            Loc.T("end.wasteland.name"), text.ToString());
    }

    /// <summary>"2:47.3" biçiminde süre.</summary>
    public static string FormatTime(float seconds)
    {
        seconds = MathF.Max(0f, seconds);
        int minutes = (int)(seconds / 60f);
        float rest = seconds - minutes * 60f;
        return string.Create(CultureInfo.InvariantCulture, $"{minutes}:{rest:00.0}");
    }

    private static void AppendRemembrance(StringBuilder text, int abandonedCount, int lostCount)
    {
        if (abandonedCount > 0)
            text.Append('\n').Append(Loc.F("end.remember.abandoned", abandonedCount));
        if (lostCount > 0)
            text.Append('\n').Append(Loc.F("end.remember.lost", lostCount));
    }

    /// <summary>"Viktor, Elena ve Mika" — ayraç ve bağlaç dile göre.</summary>
    private static string JoinNames(IReadOnlyList<Passenger> passengers)
    {
        var names = passengers.Select(p => p.Name).ToList();
        return names.Count == 1
            ? names[0]
            : string.Join(Loc.T("list.sep"), names.Take(names.Count - 1)) + Loc.T("list.and") + names[^1];
    }
}
