using System.Collections.Generic;
using System.Globalization;

namespace TheLastDelivery.Localization;

/// <summary>
/// Çeviri sözlüğü. Tüm oyuncuya görünen metinler anahtarla buradan alınır:
///   Loc.T("hud.speed")              → "HIZ" / "SPEED" / "速度" ...
///   Loc.F("hud.kmh", 72)            → "72 KM/SA"
/// Bir dilde anahtar eksikse önce İngilizceye, sonra Türkçeye düşülür.
/// </summary>
public static class Loc
{
    // Derin anlatı güncellemesinin metinleri ayrı tablolardadır (StringsEnDeep, StringsTrDeep, StringsDeepUi) ve ana
    // tabloların üstüne birleştirilir: aynı anahtar varsa yenisi geçerli olur (ör. güncellenen sahne kilidi metni).
    private static readonly Dictionary<Language, IReadOnlyDictionary<string, string>> Tables = new()
    {
        [Language.Turkish] = Merge(StringsTr.Table, StringsDeepUi.For(Language.Turkish), StringsTrDeep.Table),
        [Language.English] = Merge(StringsEn.Table, StringsDeepUi.For(Language.English), StringsEnDeep.Table),
        [Language.Spanish] = Merge(StringsEs.Table, StringsDeepUi.For(Language.Spanish)),
        [Language.Chinese] = Merge(StringsZh.Table, StringsDeepUi.For(Language.Chinese)),
        [Language.Hindi] = Merge(StringsHi.Table, StringsDeepUi.For(Language.Hindi)),
        [Language.Arabic] = Merge(StringsAr.Table, StringsDeepUi.For(Language.Arabic)),
    };

    private static IReadOnlyDictionary<string, string> Merge(params IReadOnlyDictionary<string, string>[] tables)
    {
        var merged = new Dictionary<string, string>(tables[0]);
        for (int i = 1; i < tables.Length; i++)
            foreach (var pair in tables[i]) merged[pair.Key] = pair.Value;
        return merged;
    }

    /// <summary>Anahtarın İngilizce ya da Türkçe tabloda (ya da etkin dilde) karşılığı var mı?</summary>
    public static bool Has(string key) =>
        Tables[Current].ContainsKey(key) || Tables[Language.English].ContainsKey(key) || Tables[Language.Turkish].ContainsKey(key);

    public static Language Current { get; private set; } = Language.Turkish;

    public static LanguageInfo Info => Languages.Get(Current);

    public static void SetLanguage(Language language) => Current = language;

    /// <summary>Anahtarın aktif dildeki karşılığı.</summary>
    public static string T(string key) => T(key, Current);

    /// <summary>Modlardan gelen metinler: önce bunlara bakılır (yeni anahtar ekleyebilir ya da var olanı değiştirebilir).</summary>
    private static readonly Dictionary<Language, Dictionary<string, string>> Overrides = new();

    /// <summary>Dil kodu → dil ("tr", "en", "es", "zh", "hi", "ar"). Tanınmayan kod için false.</summary>
    public static bool TryParseCode(string code, out Language language)
    {
        switch (code?.Trim().ToLowerInvariant())
        {
            case "tr": language = Language.Turkish; return true;
            case "en": language = Language.English; return true;
            case "es": language = Language.Spanish; return true;
            case "zh": language = Language.Chinese; return true;
            case "hi": language = Language.Hindi; return true;
            case "ar": language = Language.Arabic; return true;
            default: language = Language.English; return false;
        }
    }

    /// <summary>Bir mod metni ekler ya da var olan metni değiştirir.</summary>
    public static void AddOverride(Language language, string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key) || value == null) return;
        if (!Overrides.TryGetValue(language, out var table))
            Overrides[language] = table = new Dictionary<string, string>();
        table[key] = value;
    }

    private static bool TryOverride(Language language, string key, out string text)
    {
        text = null;
        return Overrides.TryGetValue(language, out var table) && table.TryGetValue(key, out text);
    }

    public static string T(string key, Language language)
    {
        if (TryOverride(language, key, out var text)) return text;
        if (Tables[language].TryGetValue(key, out text)) return text;
        if (TryOverride(Language.English, key, out text)) return text;
        if (TryOverride(Language.Turkish, key, out text)) return text;
        if (Tables[Language.English].TryGetValue(key, out text)) return text;
        if (Tables[Language.Turkish].TryGetValue(key, out text)) return text;
        return key; // Eksik çeviri ekranda anahtar olarak görünür, kolayca fark edilir
    }

    /// <summary>Biçimlendirilmiş çeviri: {0}, {1} yer tutucuları doldurulur.</summary>
    public static string F(string key, params object[] args) =>
        string.Format(CultureInfo.InvariantCulture, T(key), args);
}
