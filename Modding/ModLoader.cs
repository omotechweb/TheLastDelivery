using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using TheLastDelivery.Economy;
using TheLastDelivery.Localization;
using TheLastDelivery.Platform;
using TheLastDelivery.Story;
using TheLastDelivery.World;

namespace TheLastDelivery.Modding;

// ======================================================================
// mod.json şeması (ayrıntılı açıklama ve örnek: ModKit/README.md)
// ======================================================================

public sealed class ModManifest
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Author { get; set; }
    public string Version { get; set; }
    public string Description { get; set; }
    public List<ModSkin> Skins { get; set; } = new();
    public List<ModDialog> Dialogs { get; set; } = new();
    public List<ModStop> Stops { get; set; } = new();

    /// <summary>Dil kodu → (anahtar → metin). Oyunun herhangi bir metnini çevirir ya da değiştirir.</summary>
    public Dictionary<string, Dictionary<string, string>> Strings { get; set; } = new();
}

/// <summary>Yeni tren rengi. Renkler "#RRGGBB" biçiminde; verilmeyenler klasik skinden alınır.</summary>
public sealed class ModSkin
{
    public string Id { get; set; }
    public int Price { get; set; }
    public Dictionary<string, string> Name { get; set; } = new();
    public Dictionary<string, string> Colors { get; set; } = new();
    public string Style { get; set; }
}

/// <summary>İstasyonda peronda bekleyen yabancıyla yaşanan iki seçenekli yeni diyalog.</summary>
public sealed class ModDialog
{
    public string Id { get; set; }
    public Dictionary<string, string> Title { get; set; } = new();
    public Dictionary<string, string> Text { get; set; } = new();
    public ModOption A { get; set; }
    public ModOption B { get; set; }
}

public sealed class ModOption
{
    public Dictionary<string, string> Label { get; set; } = new();
    public Dictionary<string, string> Outcome { get; set; } = new();
    public float Fuel { get; set; }
    public float Health { get; set; }
    public int Karma { get; set; }
    public float Sanity { get; set; }
    public int Rations { get; set; }

    /// <summary>true: bu seçenek peronda bekleyen yabancıyı trene alır.</summary>
    public bool TakeStranger { get; set; }
}

/// <summary>Hikâye hattına eklenen durak: "depot" (benzinlik) ya da "station" (yabancılı istasyon).</summary>
public sealed class ModStop
{
    public float Km { get; set; }
    public string Kind { get; set; }
}

/// <summary>Yüklenen bir modun özeti (Modlar ekranı için).</summary>
public sealed class ModInfo
{
    public string Id { get; init; }
    public string Name { get; init; }
    public string Author { get; init; }
    public string Version { get; init; }
    public string Folder { get; init; }
    public bool FromWorkshop { get; init; }
    public int Skins { get; set; }
    public int Dialogs { get; set; }
    public int Stops { get; set; }
    public int Strings { get; set; }
    public List<string> Errors { get; } = new();
    public bool Loaded => Errors.Count == 0 || Skins + Dialogs + Stops + Strings > 0;
}

/// <summary>
/// Topluluk modları ve Steam Workshop altyapısı. Oyun açılışında şu klasörleri tarar:
///   %LocalAppData%\TheLastDelivery\Mods\&lt;mod&gt;\   (elle kurulan modlar)
///   &lt;oyun klasörü&gt;\Mods\&lt;mod&gt;\
///   Steam Workshop'ta abone olunan öğelerin klasörleri (platformdan)
/// Her mod klasöründe "mod.json" (skinler, diyaloglar, duraklar, metinler) ve isteğe bağlı
/// "strings.&lt;dil&gt;.txt" (anahtar = metin satırları) bulunur. Hatalı bir mod oyunu durdurmaz:
/// hatalar Modlar ekranında listelenir, geçerli kısımları yine yüklenir.
/// </summary>
public static class ModLoader
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };
    private static readonly Regex SafeId = new("^[A-Za-z0-9_-]{1,40}$");
    private static readonly List<ModInfo> LoadedMods = new();
    private static readonly List<(string Key, ModDialog Dialog)> DialogList = new();

    public static IReadOnlyList<ModInfo> Mods => LoadedMods;

    /// <summary>İstasyonlarda kullanılabilecek mod diyalogları.</summary>
    public static IReadOnlyList<(string Key, ModDialog Dialog)> Dialogs => DialogList;

    /// <summary>Elle kurulan modların klasörü (Modlar ekranında gösterilir).</summary>
    public static string UserModFolder => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TheLastDelivery", "Mods");

    public static void LoadAll()
    {
        LoadedMods.Clear();
        DialogList.Clear();
        var skins = new List<TrainSkin>();
        var stops = new List<RouteStop>();

        foreach (var (folder, workshop) in ModFolders())
        {
            var info = LoadMod(folder, workshop, skins, stops);
            if (info != null) LoadedMods.Add(info);
        }

        try
        {
            Cosmetics.AddModSkins(skins);
            Route.AddModStops(stops);
        }
        catch (Exception e)
        {
            LoadedMods.Add(new ModInfo { Id = "?", Name = "?", Folder = "", Errors = { e.Message } });
        }
    }

    private static IEnumerable<(string Folder, bool Workshop)> ModFolders()
    {
        var roots = new List<string> { UserModFolder, Path.Combine(AppContext.BaseDirectory, "Mods") };
        string extra = Environment.GetEnvironmentVariable("TLD_MODS_PATH"); // Testler ve mod geliştiricileri için
        if (!string.IsNullOrWhiteSpace(extra)) roots.Add(extra);

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (string root in roots)
        {
            string[] dirs;
            try { dirs = Directory.Exists(root) ? Directory.GetDirectories(root) : Array.Empty<string>(); }
            catch (Exception) { continue; }
            foreach (string dir in dirs)
                if (seen.Add(Path.GetFullPath(dir))) yield return (dir, false);
        }

        IReadOnlyList<string> workshop;
        try { workshop = AchievementManager.Platform.GetWorkshopItemFolders(); }
        catch (Exception) { workshop = Array.Empty<string>(); }
        foreach (string dir in workshop)
            if (seen.Add(Path.GetFullPath(dir))) yield return (dir, true);
    }

    private static ModInfo LoadMod(string folder, bool workshop, List<TrainSkin> skins, List<RouteStop> stops)
    {
        string manifestPath = Path.Combine(folder, "mod.json");
        bool hasTxt = Directory.Exists(folder) && Directory.GetFiles(folder, "strings.*.txt").Length > 0;
        if (!File.Exists(manifestPath) && !hasTxt) return null; // Mod değil

        ModManifest manifest = null;
        var errors = new List<string>();
        if (File.Exists(manifestPath))
        {
            try { manifest = JsonSerializer.Deserialize<ModManifest>(File.ReadAllText(manifestPath), Json); }
            catch (Exception e) { errors.Add($"mod.json: {e.Message}"); }
        }

        string id = manifest?.Id ?? Path.GetFileName(folder);
        if (!SafeId.IsMatch(id ?? ""))
        {
            errors.Add($"Invalid mod id '{id}'");
            id = "mod" + Math.Abs(folder.GetHashCode());
        }

        var info = new ModInfo
        {
            Id = id,
            Name = manifest?.Name ?? id,
            Author = manifest?.Author ?? "?",
            Version = manifest?.Version ?? "1.0",
            Folder = folder,
            FromWorkshop = workshop
        };
        info.Errors.AddRange(errors);
        if (manifest != null) ApplyManifest(manifest, info, skins, stops);
        LoadTextStrings(folder, info);
        return info;
    }

    private static void ApplyManifest(ModManifest m, ModInfo info, List<TrainSkin> skins, List<RouteStop> stops)
    {
        string prefix = $"mod_{info.Id}_";

        // Skinler
        foreach (var s in m.Skins ?? new())
        {
            if (!SafeId.IsMatch(s.Id ?? "")) { info.Errors.Add($"Skin: invalid id '{s.Id}'"); continue; }
            var baseSkin = Cosmetics.TrainSkins[0];
            string id = prefix + s.Id;
            Color C(string name, Color fallback) => s.Colors != null && s.Colors.TryGetValue(name, out var hex) && TryParseColor(hex, out var c) ? c : fallback;
            var skin = new TrainSkin(id, Math.Clamp(s.Price, 0, 9999),
                C("wagonBody", baseSkin.WagonBody), C("wagonPlank", baseSkin.WagonPlank), C("iron", baseSkin.Iron),
                C("boiler", baseSkin.Boiler), C("boilerBand", baseSkin.BoilerBand), C("cabBody", baseSkin.CabBody),
                C("roof", baseSkin.Roof), C("shine", baseSkin.Shine), C("trim", baseSkin.Trim))
            {
                Style = Enum.TryParse<TrainStyle>(s.Style ?? "", true, out var style) ? style : TrainStyle.None
            };
            skins.Add(skin);
            AddText($"skin.{id}", s.Name, s.Id);
            info.Skins++;
        }

        // Diyaloglar
        foreach (var d in m.Dialogs ?? new())
        {
            if (!SafeId.IsMatch(d.Id ?? "") || d.A == null || d.B == null) { info.Errors.Add($"Dialog: invalid '{d.Id}'"); continue; }
            string key = prefix + d.Id;
            AddText($"{key}.title", d.Title, d.Id);
            AddText($"{key}.text", d.Text, "");
            AddText($"{key}.a.label", d.A.Label, "A");
            AddText($"{key}.a.outcome", d.A.Outcome, "");
            AddText($"{key}.b.label", d.B.Label, "B");
            AddText($"{key}.b.outcome", d.B.Outcome, "");
            DialogList.Add((key, d));
            info.Dialogs++;
        }

        // Duraklar
        foreach (var s in m.Stops ?? new())
        {
            var kind = (s.Kind ?? "").ToLowerInvariant() switch
            {
                "depot" => StopKind.Depot,
                "station" => StopKind.Station,
                _ => (StopKind?)null
            };
            if (kind == null) { info.Errors.Add($"Stop: unknown kind '{s.Kind}'"); continue; }
            // Mod istasyonları her zaman peronda bir yabancı (ya da mod diyaloğu) gösterir
            stops.Add(new RouteStop(s.Km, kind.Value, kind == StopKind.Station ? StoryData.Scenarios.Length + 50 + info.Stops : 0));
            info.Stops++;
        }

        // Serbest metinler
        foreach (var (code, table) in m.Strings ?? new())
        {
            if (!Loc.TryParseCode(code, out var language)) { info.Errors.Add($"Strings: unknown language '{code}'"); continue; }
            foreach (var (k, v) in table) { Loc.AddOverride(language, k, v); info.Strings++; }
        }
    }

    /// <summary>"strings.tr.txt": her satır "anahtar = metin"; # ile başlayan satırlar yorumdur. \n satır sonu olur.</summary>
    private static void LoadTextStrings(string folder, ModInfo info)
    {
        string[] files;
        try { files = Directory.GetFiles(folder, "strings.*.txt"); }
        catch (Exception) { return; }
        foreach (string file in files)
        {
            string code = Path.GetFileNameWithoutExtension(file).Split('.').LastOrDefault();
            if (!Loc.TryParseCode(code, out var language)) { info.Errors.Add($"{Path.GetFileName(file)}: unknown language"); continue; }
            try
            {
                foreach (string raw in File.ReadAllLines(file))
                {
                    string line = raw.Trim();
                    if (line.Length == 0 || line.StartsWith('#')) continue;
                    int eq = line.IndexOf('=');
                    if (eq <= 0) continue;
                    Loc.AddOverride(language, line[..eq].Trim(), line[(eq + 1)..].Trim().Replace("\\n", "\n"));
                    info.Strings++;
                }
            }
            catch (Exception e)
            {
                info.Errors.Add($"{Path.GetFileName(file)}: {e.Message}");
            }
        }
    }

    /// <summary>Çok dilli metni kaydeder; hiç dil verilmemişse yedek metin İngilizce olarak eklenir.</summary>
    private static void AddText(string key, Dictionary<string, string> texts, string fallback)
    {
        bool any = false;
        foreach (var (code, value) in texts ?? new())
        {
            if (!Loc.TryParseCode(code, out var language)) continue;
            Loc.AddOverride(language, key, value);
            any = true;
        }
        if (!any) Loc.AddOverride(Language.English, key, fallback);
    }

    private static bool TryParseColor(string hex, out Color color)
    {
        color = Color.White;
        hex = hex?.Trim().TrimStart('#');
        if (hex == null || hex.Length != 6 || !uint.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint v))
            return false;
        color = new Color((int)(v >> 16) & 255, (int)(v >> 8) & 255, (int)v & 255);
        return true;
    }

    /// <summary>Mod diyaloğundan istasyon senaryosu kurar (yabancıyı alan seçenek varsa ona verilir).</summary>
    public static StationScenario BuildScenario(string key, ModDialog d, Passengers.PassengerProfile stranger)
    {
        ChoiceOption Option(string suffix, ModOption o) => new(
            $"{key}.{suffix}",
            Passenger: o.TakeStranger ? stranger : null,
            Fuel: Math.Clamp(o.Fuel, 0f, 60f),
            Health: Math.Clamp(o.Health, 0f, 60f),
            Karma: Math.Clamp(o.Karma, -50, 50),
            Sanity: Math.Clamp(o.Sanity, -50f, 50f),
            Rations: Math.Clamp(o.Rations, -5, 5));
        return new StationScenario(key, Option("a", d.A), Option("b", d.B));
    }
}
