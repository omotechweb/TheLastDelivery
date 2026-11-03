namespace TheLastDelivery.Localization;

/// <summary>Oyunun desteklediği diller.</summary>
public enum Language
{
    Turkish,
    English,
    Spanish,
    Chinese,
    Hindi,
    Arabic
}

/// <summary>
/// Bir dilin görüntülenme bilgileri.
/// FontFamilies null ise dil kod içinde üretilen piksel fontla çizilir;
/// doluysa Windows sistem fontlarından ilk bulunan kullanılır (Çince, Hintçe, Arapça).
/// </summary>
public sealed record LanguageInfo(
    Language Id,
    string NativeName,
    string EnglishName,
    string[] FontFamilies,
    float FontSizeFactor,   // Sistem fontu piksel boyutu = ölçek x bu çarpan
    bool RightToLeft,       // Arapça: sağdan sola
    bool BreakAnywhere)     // Çince: boşluk olmadan her karakterden satır kırılabilir
{
    public bool UsesPixelFont => FontFamilies == null;
}

public static class Languages
{
    public static readonly LanguageInfo[] All =
    {
        new(Language.Turkish, "Türkçe", "Turkish", null, 0f, false, false),
        new(Language.English, "English", "English", null, 0f, false, false),
        new(Language.Spanish, "Español", "Spanish", null, 0f, false, false),
        new(Language.Chinese, "中文", "Chinese", new[] { "Microsoft YaHei", "SimHei", "SimSun" }, 9.5f, false, true),
        new(Language.Hindi, "हिन्दी", "Hindi", new[] { "Nirmala UI", "Mangal" }, 10f, false, false),
        new(Language.Arabic, "العربية", "Arabic", new[] { "Segoe UI", "Arial", "Tahoma" }, 10.5f, true, false),
    };

    public static LanguageInfo Get(Language language) => All[(int)language];
}
