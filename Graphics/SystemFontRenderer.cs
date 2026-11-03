using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheLastDelivery.Localization;
using Drawing = System.Drawing;

namespace TheLastDelivery.Graphics;

/// <summary>Piksel fontun çizemediği dillerin satır çizicisi (platformdan bağımsız arayüz).</summary>
public interface ILineRenderer : IDisposable
{
    float MeasureLine(string line, int scale);

    /// <param name="visibleChars">Daktilo efekti: mantıksal sırada ilk kaç karakter görünsün.</param>
    void DrawLine(SpriteBatch sb, string line, Vector2 position, Color color, int scale, int visibleChars);
}

/// <summary>
/// Çince, Hintçe ve Arapça metinleri Windows sistem fontlarıyla (GDI+) çizer.
///
/// Satır parçalara (token) ayrılır:
///   • Sistem parçası : piksel fontun çizemediği karakter dizisi (ör. "避难所", "القطار", "ट्रेन")
///                      GDI+ ile bütün halinde çizilir; Arapça harf bağlama ve Hintçe hece birleştirme korunur.
///                      Sonuç doku olarak önbelleğe alınır (aynı sözcük bir kez çizilir).
///   • Latin parçası  : harf/rakam dizisi (ör. "72", "W/S") — piksel fontla çizilir.
///                      Sürekli değişen sayılar böylece hiç yeni doku üretmez.
///   • Nötr karakter  : boşluk, noktalama, parantez — tek tek.
/// Arapçada (sağdan sola) parçaların görsel sırası ters çevrilir ve parantezler aynalanır;
/// sayılar ve Latin sözcükler kendi içinde soldan sağa kalır (basitleştirilmiş Unicode Bidi).
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class SystemFontRenderer : ILineRenderer
{
    private const int Pad = 4; // Taşan glifler (Hintçe üst işaretleri vb.) kesilmesin diye doku kenar boşluğu

    private enum TokenKind { System, Latin, Neutral }

    private readonly struct Token
    {
        public Token(string text, TokenKind kind, int start)
        {
            Text = text;
            Kind = kind;
            Start = start;
        }

        public string Text { get; }
        public TokenKind Kind { get; }
        public int Start { get; } // Satırdaki mantıksal başlangıç indeksi (daktilo efekti için)
    }

    private readonly GraphicsDevice _graphicsDevice;
    private readonly PixelFont _pixel;
    private readonly Drawing.FontFamily _family;
    private readonly LanguageInfo _info;
    private readonly Drawing.StringFormat _format;
    private readonly Drawing.Bitmap _measureBitmap;
    private readonly Drawing.Graphics _measureGraphics;

    private readonly Dictionary<int, Drawing.Font> _fonts = new();
    private readonly Dictionary<(string, int), Texture2D> _textures = new();
    private readonly Dictionary<(string, int), float> _widths = new();

    private SystemFontRenderer(GraphicsDevice graphicsDevice, PixelFont pixel, Drawing.FontFamily family, LanguageInfo info)
    {
        _graphicsDevice = graphicsDevice;
        _pixel = pixel;
        _family = family;
        _info = info;

        _format = (Drawing.StringFormat)Drawing.StringFormat.GenericTypographic.Clone();
        _format.FormatFlags |= Drawing.StringFormatFlags.MeasureTrailingSpaces | Drawing.StringFormatFlags.NoWrap;
        if (info.RightToLeft)
            _format.FormatFlags |= Drawing.StringFormatFlags.DirectionRightToLeft;

        _measureBitmap = new Drawing.Bitmap(1, 1);
        _measureGraphics = Drawing.Graphics.FromImage(_measureBitmap);
        _measureGraphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
    }

    /// <summary>Dilin font adaylarından ilk yüklü olanla bir çizici oluşturur; hiçbiri yoksa null.</summary>
    public static SystemFontRenderer TryCreate(GraphicsDevice graphicsDevice, PixelFont pixel, LanguageInfo info)
    {
        foreach (string name in info.FontFamilies)
        {
            try
            {
                return new SystemFontRenderer(graphicsDevice, pixel, new Drawing.FontFamily(name), info);
            }
            catch (ArgumentException)
            {
                // Bu font yüklü değil, bir sonrakini dene
            }
        }
        return null;
    }

    // ------------------------------------------------------------------
    // Ölçüm ve çizim
    // ------------------------------------------------------------------

    public float MeasureLine(string line, int scale)
    {
        float width = 0f;
        foreach (var token in Tokenize(line))
            width += TokenWidth(token, scale);
        return width;
    }

    public void DrawLine(SpriteBatch sb, string line, Vector2 position, Color color, int scale, int visibleChars)
    {
        var tokens = Tokenize(line);
        if (_info.RightToLeft)
            tokens.Reverse(); // Görsel sıra: sağdan sola okunacak şekilde

        float x = position.X;
        foreach (var token in tokens)
        {
            float width = TokenWidth(token, scale);
            int shown = Math.Clamp(visibleChars - token.Start, 0, token.Text.Length);

            if (shown > 0)
            {
                if (token.Kind == TokenKind.System)
                {
                    DrawRun(sb, token.Text, x, position.Y, scale, color, shown / (float)token.Text.Length);
                }
                else
                {
                    string text = _info.RightToLeft && token.Kind == TokenKind.Neutral ? Mirror(token.Text) : token.Text;
                    _pixel.Draw(sb, text.Substring(0, shown), new Vector2(x, position.Y), color, scale, turkishCasing: false);
                }
            }
            x += width;
        }
    }

    /// <summary>Sistem fontu parçasını çizer; daktilo efektinde yalnızca görünen kısmı (sağdan sola dillerde sağdan) gösterir.</summary>
    private void DrawRun(SpriteBatch sb, string text, float x, float y, int scale, Color color, float fraction)
    {
        var texture = GetTexture(text, PixelSize(scale));
        int visibleWidth = (int)MathF.Ceiling(texture.Width * fraction);
        var source = _info.RightToLeft
            ? new Rectangle(texture.Width - visibleWidth, 0, visibleWidth, texture.Height)
            : new Rectangle(0, 0, visibleWidth, texture.Height);
        float offsetX = _info.RightToLeft ? texture.Width - visibleWidth : 0f;

        // Dikeyde piksel fontun gövde ortasına (satırın 5.5 birim aşağısı) hizala
        float top = y + 5.5f * scale - texture.Height / 2f;
        sb.Draw(texture, new Vector2(MathF.Round(x - Pad + offsetX), MathF.Round(top)), source, color);
    }

    // ------------------------------------------------------------------
    // Parçalara ayırma
    // ------------------------------------------------------------------

    private List<Token> Tokenize(string line)
    {
        var tokens = new List<Token>();
        int i = 0;
        while (i < line.Length)
        {
            char c = line[i];
            int start = i;

            if (IsSystemChar(c))
            {
                // Sözcüğe yapışık noktalama (". , ! ?" vb.) sözcükle birlikte sistem fontuyla çizilir;
                // böylece Arapçada doğru tarafa düşer ve yazı stiliyle uyumlu olur
                while (i < line.Length && (IsSystemChar(line[i]) || IsAttachedPunctuation(line[i]))) i++;
                tokens.Add(new Token(line.Substring(start, i - start), TokenKind.System, start));
            }
            else if (IsStrongLatin(c))
            {
                // Harf/rakam dizisi; "2/3", "W/S", "12.5" gibi iç ayraçlar dizinin parçası sayılır
                i++;
                while (i < line.Length)
                {
                    if (IsStrongLatin(line[i])) { i++; continue; }
                    bool joiner = line[i] is '/' or '.' or ':' or ',';
                    if (joiner && i + 1 < line.Length && IsStrongLatin(line[i + 1])) { i += 2; continue; }
                    break;
                }
                tokens.Add(new Token(line.Substring(start, i - start), TokenKind.Latin, start));
            }
            else
            {
                i++;
                tokens.Add(new Token(c.ToString(), TokenKind.Neutral, start));
            }
        }
        return tokens;
    }

    private bool IsSystemChar(char c) => c != ' ' && !_pixel.CanRender(c);

    // "°" sayının parçası sayılır: "70°C" sağdan sola satırda ters dönmez
    private bool IsStrongLatin(char c) => !IsSystemChar(c) && (char.IsLetterOrDigit(c) || c == '°');

    private static bool IsAttachedPunctuation(char c) => c is '.' or ',' or '!' or '?' or ':' or ';';

    private static string Mirror(string text) => text switch
    {
        "(" => ")", ")" => "(",
        "[" => "]", "]" => "[",
        "<" => ">", ">" => "<",
        _ => text
    };

    private float TokenWidth(Token token, int scale) => token.Kind switch
    {
        TokenKind.System => MeasureRun(token.Text, PixelSize(scale)),
        _ when token.Text == " " => 4 * scale, // Sistem fontu sözcükleri arasında piksel fonttan dar boşluk
        _ => token.Text.Length * PixelFont.Advance * scale
    };

    // ------------------------------------------------------------------
    // GDI+ ile ölçüm ve dokuya çizim
    // ------------------------------------------------------------------

    private int PixelSize(int scale) => Math.Max(13, (int)MathF.Round(_info.FontSizeFactor * scale));

    private Drawing.Font GetFont(int pixelSize)
    {
        if (!_fonts.TryGetValue(pixelSize, out var font))
        {
            font = new Drawing.Font(_family, pixelSize, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Pixel);
            _fonts[pixelSize] = font;
        }
        return font;
    }

    private float MeasureRun(string text, int pixelSize)
    {
        if (_widths.TryGetValue((text, pixelSize), out float width)) return width;

        width = _measureGraphics.MeasureString(text, GetFont(pixelSize), Drawing.PointF.Empty, _format).Width;
        _widths[(text, pixelSize)] = width;
        return width;
    }

    private Texture2D GetTexture(string text, int pixelSize)
    {
        if (_textures.TryGetValue((text, pixelSize), out var cached)) return cached;

        var font = GetFont(pixelSize);
        int width = (int)MathF.Ceiling(MeasureRun(text, pixelSize)) + Pad * 2;
        int height = (int)MathF.Ceiling(font.GetHeight(_measureGraphics)) + Pad * 2;

        using var bitmap = new Drawing.Bitmap(width, height, PixelFormat.Format32bppArgb);
        using (var g = Drawing.Graphics.FromImage(bitmap))
        {
            g.Clear(Drawing.Color.Transparent);
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            g.DrawString(text, font, Drawing.Brushes.White, new Drawing.RectangleF(Pad, Pad, width - Pad * 2 + 1, height - Pad), _format);
        }

        // Beyaz metnin alfa kanalını XNA'nın premultiplied formatına aktar; renk çizimde verilir
        var data = bitmap.LockBits(new Drawing.Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        var raw = new int[width * height];
        for (int row = 0; row < height; row++)
            Marshal.Copy(data.Scan0 + row * data.Stride, raw, row * width, width);
        bitmap.UnlockBits(data);

        var colors = new Color[raw.Length];
        for (int i = 0; i < raw.Length; i++)
        {
            byte alpha = (byte)((raw[i] >> 24) & 0xFF);
            colors[i] = new Color(alpha, alpha, alpha, alpha);
        }

        var texture = new Texture2D(_graphicsDevice, width, height);
        texture.SetData(colors);
        _textures[(text, pixelSize)] = texture;
        return texture;
    }

    public void Dispose()
    {
        foreach (var texture in _textures.Values) texture.Dispose();
        foreach (var font in _fonts.Values) font.Dispose();
        _textures.Clear();
        _fonts.Clear();
        _format.Dispose();
        _measureGraphics.Dispose();
        _measureBitmap.Dispose();
        _family.Dispose();
    }
}
