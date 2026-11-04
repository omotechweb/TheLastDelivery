using System;
using System.Collections.Generic;
using TheLastDelivery.Economy;
using TheLastDelivery.Localization;
using TheLastDelivery.UI;

namespace TheLastDelivery.Story;

/// <summary>
/// Güveni dibe vuran bir NPC'nin nasıl tepki verdiği.
/// Steady: sessizce ayrılır. Coward: gece kaçar (erzakla). Hothead: arkadan bıçaklar. Informer: yerini ihbar eder.
/// </summary>
public enum Temper { Steady, Coward, Hothead, Informer }

/// <summary>Güven barı olan biri: yolcu ya da kasaba lideri.</summary>
public sealed record NpcDef(string Id, string NameKey, PortraitKind Portrait, int BaseTrust, Temper Temper, bool Passenger)
{
    public string Name => Loc.T(NameKey);
}

/// <summary>Diyalogdan doğan, Game1'in uygulaması gereken sonuç.</summary>
public readonly record struct PendingConsequence(string Npc, string Kind);

/// <summary>
/// Çok seçenekli diyalog ağlarının güven/şüphe katmanı. Her yolcunun ve kasaba liderinin 0-100 arası bir güveni
/// vardır (StoryState.Trust; etkin bölüme göre). Seçenekler güveni değiştirir; güven eşiklere göre seçenekleri açar
/// ("trust:viktor&gt;=60") ve <see cref="Hostile"/> altına düşerse NPC'nin mizacına göre bir sonuç doğar:
/// kaçma, bıçaklama, AURA'ya/Kızıl Birlik'e ihbar ya da kapıları kapatma. Seçimler günlüğe yazılır.
/// </summary>
public static class DialogueTreeManager
{
    public const int Hostile = 15;
    public const int Wary = 35;
    public const int Trusting = 65;
    public const int Loyal = 85;

    private static readonly Dictionary<string, NpcDef> Npcs = new();
    private static readonly List<PendingConsequence> Pending = new();

    static DialogueTreeManager()
    {
        // ---- Episode 1 yolcuları ----
        P("viktor", PortraitKind.Viktor, 55, Temper.Steady);
        P("elena", PortraitKind.Elena, 55, Temper.Steady);
        P("mika", PortraitKind.Mika, 60, Temper.Steady);
        P("bora", PortraitKind.Bora, 45, Temper.Hothead);
        P("nadia", PortraitKind.Nadia, 55, Temper.Steady);
        P("oskar", PortraitKind.Oskar, 50, Temper.Hothead);
        P("lina", PortraitKind.Lina, 45, Temper.Coward);
        P("ilya", PortraitKind.Ilya, 40, Temper.Informer);
        P("zoya", PortraitKind.Zoya, 50, Temper.Coward);
        P("taras", PortraitKind.Taras, 40, Temper.Hothead);
        P("grigor", PortraitKind.Grigor, 35, Temper.Informer);
        P("vera", PortraitKind.Vera, 45, Temper.Coward);
        P("feliks", PortraitKind.Feliks, 40, Temper.Coward);
        P("anya", PortraitKind.Anya, 40, Temper.Hothead);
        P("pavel", PortraitKind.Pavel, 40, Temper.Hothead);
        P("lev", PortraitKind.Lev, 35, Temper.Informer);
        P("dmitri", PortraitKind.Dmitri, 60, Temper.Steady);
        P("mila", PortraitKind.Mila, 60, Temper.Steady);
        // ---- Episode 2 yolcuları ----
        P("nova", PortraitKind.Nova, 55, Temper.Steady);
        P("ada", PortraitKind.Ada, 50, Temper.Coward);
        P("kiril", PortraitKind.Kiril, 35, Temper.Informer);
        P("yusuf", PortraitKind.Yusuf, 45, Temper.Hothead);
        P("mara", PortraitKind.Mara, 45, Temper.Hothead);

        // ---- Kasaba liderleri ve hikâye karakterleri (konuşma kimlikleri "spk.{id}") ----
        L("greta", PortraitKind.Innkeeper, 50, Temper.Steady);       // Kırağı Pazarı tavernacısı
        L("marshal", PortraitKind.Officer, 40, Temper.Informer);     // Kırağı Pazarı karakol şefi Brann
        L("yuri", PortraitKind.StrikeLeader, 45, Temper.Hothead, "spk.strike");
        L("gromov", PortraitKind.Foreman, 40, Temper.Informer, "spk.foreman");
        L("tinker", PortraitKind.Smith, 50, Temper.Steady);          // Demirhan tamircisi Sasha
        L("keeper", PortraitKind.LanternKeeper, 55, Temper.Coward);  // Son Fener'in fener bekçisi
        L("vesna", PortraitKind.Priestess, 40, Temper.Informer, "spk.priestess");
        L("hermit", PortraitKind.Hermit, 45, Temper.Steady);
        L("salt", PortraitKind.SaltWorker, 45, Temper.Coward);       // Tuzla'nın sığınak sorumlusu
        L("kaya", PortraitKind.Officer, 40, Temper.Hothead, "spk.officer");
        L("signal", PortraitKind.Quartermaster, 50, Temper.Steady);  // Güney Kapısı telsizcisi
        L("halden", PortraitKind.Inspector, 35, Temper.Informer, "spk.inspector");
        L("morrow", PortraitKind.Envoy, 30, Temper.Informer, "spk.envoy");
        L("wolf", PortraitKind.WolfFather, 25, Temper.Hothead, "spk.wolf");
        L("b17", PortraitKind.Droid, 50, Temper.Informer);           // Ölü Sinyal'in servis droidu
        L("marya", PortraitKind.Warden, 35, Temper.Informer, "spk.warden");
        L("preacher", PortraitKind.Acolyte, 30, Temper.Hothead, "spk.preacher");
        L("oracle", PortraitKind.Oracle, 30, Temper.Informer, "spk.oracle");
        L("aura", PortraitKind.Aura, 50, Temper.Informer, "spk.aura");
    }

    private static void P(string id, PortraitKind portrait, int trust, Temper temper) =>
        Npcs[id] = new NpcDef(id, $"p.{id}.name", portrait, trust, temper, true);

    private static void L(string id, PortraitKind portrait, int trust, Temper temper, string nameKey = null) =>
        Npcs[id] = new NpcDef(id, nameKey ?? $"spk.{id}", portrait, trust, temper, false);

    private static StoryState State => Narrative.Current;

    public static NpcDef Find(string id) => id != null && Npcs.TryGetValue(id, out var npc) ? npc : null;

    /// <summary>Konuşanın çeviri anahtarından NPC kimliği ("p.viktor.name" → viktor, "spk.strike" → yuri).</summary>
    public static string NpcForSpeaker(string speakerKey)
    {
        if (string.IsNullOrEmpty(speakerKey)) return null;
        if (speakerKey.StartsWith("p.") && speakerKey.EndsWith(".name"))
        {
            string id = speakerKey[2..^5];
            return Npcs.ContainsKey(id) ? id : null;
        }
        foreach (var npc in Npcs.Values)
            if (npc.NameKey == speakerKey) return npc.Id;
        return null;
    }

    /// <summary>NPC'nin şu anki güveni (hiç konuşulmadıysa başlangıç değeri; Episode 2'de Episode 1'den taşınan).</summary>
    public static int Trust(string id)
    {
        if (State.Trust.TryGetValue(id, out int value)) return value;
        if (Narrative.Episode == 2 && SaveData.Current.Carry?.Trust != null
            && SaveData.Current.Carry.Trust.TryGetValue(id, out int carried)) return carried;
        return Find(id)?.BaseTrust ?? 50;
    }

    /// <summary>Güven düzeyi 0-4: Düşman, Şüpheci, Temkinli, Güveniyor, Sadık.</summary>
    public static int Level(int trust) => trust <= Hostile ? 0 : trust < Wary ? 1 : trust < Trusting ? 2 : trust < Loyal ? 3 : 4;

    public static string LevelName(int trust) => Loc.T($"trust.lv.{Level(trust)}");

    /// <summary>
    /// Güveni değiştirir. <see cref="Hostile"/> eşiğinin altına ilk kez inildiğinde NPC'nin mizacına göre
    /// bir sonuç sıraya girer (Game1 diyalog kapanınca uygular). Değişimin özet satırını döndürür.
    /// </summary>
    public static string Adjust(string id, int delta)
    {
        var npc = Find(id);
        if (npc == null || delta == 0) return null;
        int before = Trust(id);
        int after = Math.Clamp(before + delta, 0, 100);
        State.Trust[id] = after;

        if (before > Hostile && after <= Hostile)
            Queue(id, npc.Temper switch
            {
                Temper.Coward => "flee",
                Temper.Hothead => "stab",
                Temper.Informer => "report",
                _ => npc.Passenger ? "leave" : "cold"
            });
        else if (before < Loyal && after >= Loyal)
            Narrative.Set($"loyal:{id}");

        return Loc.F(delta > 0 ? "sum.trustUp" : "sum.trustDown", npc.Name, LevelName(after));
    }

    /// <summary>Doğrudan sonuç (ör. mahkemede verilen hüküm). Aynı sonuç aynı kişide bir kez olur.</summary>
    public static void Queue(string id, string kind)
    {
        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(kind)) return;
        string key = $"{kind}:{id}";
        if (State.Consequences.Contains(key)) return;
        State.Consequences.Add(key);
        Narrative.Set(key); // İçerik "stab:bora" gibi bayraklarla sonuca tepki verebilir
        Pending.Add(new PendingConsequence(id, kind));
    }

    /// <summary>Bekleyen sonuçları alır ve sırayı boşaltır.</summary>
    public static List<PendingConsequence> TakePending()
    {
        var list = new List<PendingConsequence>(Pending);
        Pending.Clear();
        return list;
    }

    public static bool HasPending => Pending.Count > 0;

    /// <summary>Açık diyalog düğümünde güveni izlenen NPC (StoryRunner ayarlar; Game1 etkileri ona uygular).</summary>
    public static string Speaking { get; set; }

    /// <summary>Yeni koşu: önceki yolculuktan kalan bekleyen sonuçlar silinir.</summary>
    public static void ResetRun() => Pending.Clear();

    /// <summary>Seçim günlüğü (kayıtta kalır; sonsöz ve AURA'nın Episode 2 tavrı bunu okuyabilir).</summary>
    public static void RecordChoice(string nodeId, int optionIndex)
    {
        string entry = $"{nodeId}#{optionIndex + 1}";
        if (State.Choices.Count > 0 && State.Choices[^1] == entry) return;
        State.Choices.Add(entry);
        if (State.Choices.Count > 600) State.Choices.RemoveRange(0, State.Choices.Count - 600); // Kayıt şişmesin
    }

    /// <summary>"viktor&gt;=60", "kiril&lt;30", "greta&gt;50" biçimindeki güven koşulu.</summary>
    public static bool Check(string expression)
    {
        if (!Split(expression, out string id, out string op, out int value)) return false;
        return Compare(Trust(id), op, value);
    }

    /// <summary>AURA'nın tavrı: "&gt;=20", "&lt;0".</summary>
    public static bool CheckStance(string expression)
    {
        if (!Split("x" + expression, out _, out string op, out int value)) return false;
        return Compare(Stance, op, value);
    }

    /// <summary>
    /// AURA'nın Arthur'a tavrı. Episode 1'de sona doğru şekillenir (Enstitü belgeleri, Kuzey Yıldızı, telsizdeki sesler)
    /// ve Episode 2'ye taşınır; Episode 2'de AURA'nın her konuşması buna göre değişir.
    /// </summary>
    public static int Stance
    {
        get
        {
            int carried = Narrative.Episode == 2 ? SaveData.Current.Carry?.AuraStance ?? 0 : 0;
            return Math.Clamp(carried + State.AuraStance, -100, 100);
        }
    }

    public static string AdjustStance(int delta)
    {
        if (delta == 0) return null;
        State.AuraStance = Math.Clamp(State.AuraStance + delta, -100, 100);
        return Loc.T(delta > 0 ? "sum.auraUp" : "sum.auraDown");
    }

    /// <summary>AURA'nın tavrının adı: Düşman, Soğuk, Meraklı, Saygılı.</summary>
    public static string StanceName => Loc.T(Stance <= -40 ? "stance.0" : Stance < 0 ? "stance.1" : Stance < 40 ? "stance.2" : "stance.3");

    private static bool Split(string expression, out string id, out string op, out int value)
    {
        id = null; op = null; value = 0;
        if (string.IsNullOrEmpty(expression)) return false;
        int i = expression.IndexOfAny(new[] { '<', '>', '=' });
        if (i <= 0) return false;
        id = expression[..i];
        int j = i;
        while (j < expression.Length && expression[j] is '<' or '>' or '=') j++;
        op = expression[i..j];
        return int.TryParse(expression[j..], out value);
    }

    private static bool Compare(int actual, string op, int value) => op switch
    {
        ">=" => actual >= value,
        "<=" => actual <= value,
        ">" => actual > value,
        "<" => actual < value,
        _ => actual == value
    };

    /// <summary>Güveni izlenen bütün NPC'ler (arşivdeki ilişkiler sekmesi için: yalnızca tanışılanlar).</summary>
    public static IEnumerable<NpcDef> Known()
    {
        foreach (var npc in Npcs.Values)
            if (State.Trust.ContainsKey(npc.Id)) yield return npc;
    }
}

/// <summary>Kasaba binalarında ve diyaloglarda açılan yan görevler. Metinler: "sq.{id}.t" başlık, "sq.{id}.d" açıklama.</summary>
public static class SideQuests
{
    public const int None = 0, Active = 1, Done = 2, Failed = 3;

    /// <summary>Bilinen bütün yan görevler (arşivde sırayla listelenir).</summary>
    public static readonly string[] All =
    {
        // Episode 1
        "greta.son", "tinker.parts", "keeper.tapes", "hermit.cache", "salt.medicine", "signal.code",
        // Episode 2
        "b17.memory", "marya.list", "nova.ghost", "chapel.choir"
    };

    public static int State(string id) => id != null && Narrative.Current.SideQuests.TryGetValue(id, out int s) ? s : None;

    public static string Title(string id) => Loc.T($"sq.{id}.t");
    public static string Description(string id) => Loc.T($"sq.{id}.d");

    public static string Start(string id)
    {
        if (State(id) != None) return null;
        Narrative.Current.SideQuests[id] = Active;
        return Loc.F("sum.questStart", Title(id));
    }

    public static string Finish(string id)
    {
        if (State(id) is Done or Failed) return null;
        Narrative.Current.SideQuests[id] = Done;
        return Loc.F("sum.questDone", Title(id));
    }

    public static string Fail(string id)
    {
        if (State(id) is Done or Failed) return null;
        Narrative.Current.SideQuests[id] = Failed;
        return Loc.F("sum.questFail", Title(id));
    }
}

/// <summary>Arşiv kaydının türü.</summary>
public enum LoreKind { Document, Tape, Memory }

/// <summary>Bulunabilen belge, ses kaseti ya da Arthur'un hatırası. Başlık "doc.{Id}", metin "nv.{Node}".</summary>
public sealed record LoreEntry(string Id, LoreKind Kind, int Episode, string Node)
{
    public string Title => Loc.T($"doc.{Id}");
    public string Body => Loc.T($"nv.{Node}");
}

/// <summary>
/// Çevresel hikâye anlatımının kaydı: oyuncunun kendi keşfettiği belgeler, kasetler ve hatıralar.
/// Etkin hikâyede bulunanlar StoryState.Documents'ta (içerik "doc:x" ile sorar), bir kez bulunan her şey
/// kalıcı arşivde (SaveData.Archive) kalır ve [L] arşiv ekranından yeniden okunabilir.
/// </summary>
public static class LoreArchive
{
    private static readonly Dictionary<string, LoreEntry> Entries = new();
    private static readonly List<LoreEntry> Ordered = new();

    public static void Register(LoreEntry entry)
    {
        if (Entries.ContainsKey(entry.Id)) return;
        Entries[entry.Id] = entry;
        Ordered.Add(entry);
    }

    public static LoreEntry Find(string id)
    {
        EnsureLoaded();
        return id != null && Entries.TryGetValue(id, out var e) ? e : null;
    }

    public static IReadOnlyList<LoreEntry> All
    {
        get
        {
            EnsureLoaded();
            return Ordered;
        }
    }

    // Kayıtlar NarrativeData'nın statik kurucusunda tanımlanır; arşiv ondan önce açılırsa önce o yüklensin
    private static void EnsureLoaded() => NarrativeData.Node(null);

    /// <summary>Etkin hikâyede bulundu mu?</summary>
    public static bool Has(string id) => Narrative.Current.Documents.Contains(id);

    /// <summary>Herhangi bir koşuda bulunup arşive girdi mi?</summary>
    public static bool InArchive(string id) => SaveData.Current.Archive.Contains(id);

    public static bool IsUnread(string id) => InArchive(id) && !SaveData.Current.ArchiveRead.Contains(id);

    public static int UnreadCount
    {
        get
        {
            int count = 0;
            foreach (string id in SaveData.Current.Archive)
                if (!SaveData.Current.ArchiveRead.Contains(id)) count++;
            return count;
        }
    }

    public static void MarkRead(string id)
    {
        if (!SaveData.Current.ArchiveRead.Contains(id)) SaveData.Current.ArchiveRead.Add(id);
    }

    /// <summary>Kaydı bulur. Etkin hikâyede yeniyse özet satırını döndürür (yoksa null).</summary>
    public static string Collect(string id)
    {
        var entry = Find(id);
        if (entry == null) return null;
        if (!SaveData.Current.Archive.Contains(id)) SaveData.Current.Archive.Add(id);
        if (Narrative.Current.Documents.Contains(id)) return null;
        Narrative.Current.Documents.Add(id);
        return Loc.F(entry.Kind switch
        {
            LoreKind.Tape => "sum.tape",
            LoreKind.Memory => "sum.memory",
            _ => "sum.doc"
        }, entry.Title);
    }

    /// <summary>Etkin hikâyede bulunan kayıt sayısı (türüne göre).</summary>
    public static int Count(LoreKind kind, int episode)
    {
        int count = 0;
        foreach (var entry in Ordered)
            if (entry.Kind == kind && entry.Episode == episode && Narrative.Current.Documents.Contains(entry.Id)) count++;
        return count;
    }
}
