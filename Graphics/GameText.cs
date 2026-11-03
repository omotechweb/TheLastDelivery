using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheLastDelivery.Localization;

namespace TheLastDelivery.Graphics;

/// <summary>
/// Oyundaki tüm yazıların tek giriş noktası. Aktif dile göre doğru çiziciyi seçer:
///   • Türkçe, İngilizce, İspanyolca → kodla üretilen piksel font
///   • Çince, Hintçe, Arapça         → Windows sistem fontu (SystemFontRenderer)
/// Satır kırma (Çincede her karakterden), ölçüm, hizalama ve daktilo efekti burada dile göre yapılır.
/// </summary>
public static class GameText
{
    private static PixelFont _pixel;
    private static GraphicsDevice _graphicsDevice;
    private static readonly Dictionary<Language, ILineRenderer> SystemRenderers = new();

    // Çincede satır başına gelmemesi gereken kapanış işaretleri
    private const string NoLineStart = "，。！？：；、）」』》%";

    public static void Initialize(GraphicsDevice graphicsDevice, PixelFont pixel)
    {
        _graphicsDevice = graphicsDevice;
        _pixel = pixel;
    }

    public static void Unload()
    {
        foreach (var renderer in SystemRenderers.Values)
            renderer?.Dispose();
        SystemRenderers.Clear();
    }

    /// <summary>Bu dil bu bilgisayarda gösterilebiliyor mu? (Gerekli font yüklü mü?)</summary>
    public static bool IsAvailable(Language language) =>
        Languages.Get(language).UsesPixelFont || GetSystemRenderer(language) != null;

    public static bool IsRightToLeft => Loc.Info.RightToLeft;

    /// <summary>Bir yazı satırının yüksekliği (tüm dillerde aynı satır aralığı).</summary>
    public static float LineHeight(int scale) => PixelFont.LineHeight * scale;

    // ------------------------------------------------------------------
    // Çizim
    // ------------------------------------------------------------------

    /// <summary>Metni sol üstten başlayarak çizer; '\n' yeni satıra geçer.</summary>
    public static void Draw(SpriteBatch sb, string text, Vector2 position, Color color, int scale = 2) =>
        Draw(sb, text, position, color, scale, Loc.Current);

    public static void Draw(SpriteBatch sb, string text, Vector2 position, Color color, int scale, Language language)
    {
        float y = position.Y;
        foreach (string line in text.Split('\n'))
        {
            DrawLine(sb, line, new Vector2(position.X, y), color, scale, language, int.MaxValue);
            y += LineHeight(scale);
        }
    }

    /// <summary>Her satırı verilen X etrafında ortalar.</summary>
    public static void DrawCentered(SpriteBatch sb, string text, float centerX, float y, Color color, int scale = 2) =>
        DrawCentered(sb, text, centerX, y, color, scale, Loc.Current);

    public static void DrawCentered(SpriteBatch sb, string text, float centerX, float y, Color color, int scale, Language language)
    {
        foreach (string line in text.Split('\n'))
        {
            float width = MeasureLine(line, scale, language);
            DrawLine(sb, line, new Vector2(MathF.Round(centerX - width / 2f), y), color, scale, language, int.MaxValue);
            y += LineHeight(scale);
        }
    }

    /// <summary>Metni sağ kenarı verilen X'e gelecek şekilde çizer.</summary>
    public static void DrawRight(SpriteBatch sb, string text, float rightX, float y, Color color, int scale = 2)
    {
        float width = MeasureLine(text, scale, Loc.Current);
        DrawLine(sb, text, new Vector2(rightX - width, y), color, scale, Loc.Current, int.MaxValue);
    }

    /// <summary>
    /// Daktilo efekti: tek satırın yalnızca ilk <paramref name="visibleChars"/> karakterini gösterir.
    /// Satır yerleşimi tam metne göre yapılır, böylece yazı akarken kaymaz.
    /// </summary>
    public static void DrawPartial(SpriteBatch sb, string line, int visibleChars, Vector2 position, Color color, int scale = 2) =>
        DrawLine(sb, line, position, color, scale, Loc.Current, visibleChars);

    // ------------------------------------------------------------------
    // Ölçüm ve satır kırma
    // ------------------------------------------------------------------

    public static Vector2 Measure(string text, int scale = 2) => Measure(text, scale, Loc.Current);

    public static Vector2 Measure(string text, int scale, Language language)
    {
        string[] lines = text.Split('\n');
        float width = 0f;
        foreach (string line in lines)
            width = MathF.Max(width, MeasureLine(line, scale, language));
        return new Vector2(width, lines.Length * LineHeight(scale));
    }

    public static float MeasureLine(string line, int scale = 2) => MeasureLine(line, scale, Loc.Current);

    /// <summary>
    /// Metni verilen genişliğe sığan satırlara böler. Sözcük sınırlarından kırar;
    /// Çincede boşluk olmadığından her karakterden kırılabilir (kapanış işaretleri satır başına gelmez).
    /// </summary>
    public static List<string> Wrap(string text, float maxWidth, int scale = 2)
    {
        var language = Loc.Current;
        bool breakAnywhere = Languages.Get(language).BreakAnywhere;
        var result = new List<string>();

        foreach (string paragraph in text.Split('\n'))
        {
            string line = string.Empty;
            foreach (string segment in Segment(paragraph, breakAnywhere))
            {
                if (segment == " ")
                {
                    if (line.Length > 0) line += " ";
                    continue;
                }

                string candidate = line + segment;
                bool mustStayOnLine = breakAnywhere && segment.Length == 1 && NoLineStart.Contains(segment[0]);
                if (line.Trim().Length > 0 && !mustStayOnLine && MeasureLine(candidate.TrimEnd(), scale, language) > maxWidth)
                {
                    result.Add(line.TrimEnd());
                    line = segment;
                }
                else
                {
                    line = candidate;
                }
            }
            result.Add(line.TrimEnd());
        }
        return result;
    }

    // ------------------------------------------------------------------
    // İç işleyiş
    // ------------------------------------------------------------------

    private static void DrawLine(SpriteBatch sb, string line, Vector2 position, Color color, int scale, Language language, int visibleChars)
    {
        var renderer = Languages.Get(language).UsesPixelFont ? null : GetSystemRenderer(language);
        if (renderer != null)
        {
            renderer.DrawLine(sb, line, position, color, scale, visibleChars);
            return;
        }

        string visible = visibleChars >= line.Length ? line : line.Substring(0, Math.Max(0, visibleChars));
        _pixel.Draw(sb, visible, position, color, scale, turkishCasing: language == Language.Turkish);
    }

    private static float MeasureLine(string line, int scale, Language language)
    {
        var renderer = Languages.Get(language).UsesPixelFont ? null : GetSystemRenderer(language);
        return renderer?.MeasureLine(line, scale) ?? PixelFont.MeasureWidth(line, scale);
    }

    private static ILineRenderer GetSystemRenderer(Language language)
    {
        if (SystemRenderers.TryGetValue(language, out var renderer)) return renderer;

        renderer = null;
        if (OperatingSystem.IsWindows() && _graphicsDevice != null)
            renderer = SystemFontRenderer.TryCreate(_graphicsDevice, _pixel, Languages.Get(language));

        SystemRenderers[language] = renderer; // null da önbelleğe alınır: font yoksa tekrar aranmaz
        return renderer;
    }

    /// <summary>Paragrafı kırılabilir parçalara böler: sözcükler, boşluklar ve (Çincede) tek tek karakterler.</summary>
    private static IEnumerable<string> Segment(string paragraph, bool breakAnywhere)
    {
        var word = new StringBuilder();
        foreach (char c in paragraph)
        {
            if (c == ' ')
            {
                if (word.Length > 0) { yield return word.ToString(); word.Clear(); }
                yield return " ";
            }
            else if (breakAnywhere && c >= '⺀')
            {
                if (word.Length > 0) { yield return word.ToString(); word.Clear(); }
                yield return c.ToString();
            }
            else
            {
                word.Append(c);
            }
        }
        if (word.Length > 0) yield return word.ToString();
    }
}
