using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TheLastDelivery.Graphics;

/// <summary>
/// Content Pipeline / SpriteFont gerektirmeyen, tamamen kodla üretilen 5x7 piksel font.
/// Türkçe karakterleri (Ç, Ğ, İ, Ö, Ş, Ü) destekler. Tüm metin büyük harfe çevrilerek çizilir;
/// bu da oyuna eski bir telgraf/terminal havası verir.
///
/// Her glif 5x10 boyutunda ayrı bir Texture2D'dir:
///   satır 0-1 : üst aksan (nokta, iki nokta, ğ şapkası)
///   satır 2-8 : harf gövdesi (5x7)
///   satır 9   : alt çengel (Ç, Ş)
/// </summary>
public sealed class PixelFont : IDisposable
{
    public const int GlyphWidth = 5;
    public const int GlyphHeight = 10;
    public const int Advance = 6;      // Harf + 1 piksel boşluk
    public const int LineHeight = 12;  // Glif + 2 piksel satır arası

    private enum Accent { None, Dot, Dots, Breve, Cedilla, Acute, Tilde }

    private readonly Dictionary<char, Texture2D> _glyphs = new();
    private readonly Texture2D _fallback;

    public PixelFont(GraphicsDevice graphicsDevice)
    {
        foreach (var pair in BaseGlyphs)
            _glyphs[pair.Key] = BuildGlyph(graphicsDevice, pair.Value, Accent.None);

        foreach (var pair in AccentedGlyphs)
            _glyphs[pair.Key] = BuildGlyph(graphicsDevice, BaseGlyphs[pair.Value.Base], pair.Value.Accent);

        _fallback = _glyphs['?'];
    }

    // ------------------------------------------------------------------
    // Çizim ve ölçüm
    // ------------------------------------------------------------------

    /// <summary>
    /// Tek satırlık metni verilen konumdan başlayarak çizer.
    /// Çok dilli düzen, satır kırma ve hizalama GameText'tedir; burası yalnızca glifleri basar.
    /// </summary>
    /// <param name="turkishCasing">Türkçede i→İ, diğer dillerde i→I büyütülür.</param>
    public void Draw(SpriteBatch spriteBatch, string text, Vector2 position, Color color, int scale, bool turkishCasing)
    {
        float x = position.X;

        foreach (char raw in text)
        {
            char c = Normalize(raw, turkishCasing);
            if (c != ' ')
            {
                spriteBatch.Draw(GetGlyph(c), new Vector2(MathF.Round(x), MathF.Round(position.Y)), null, color,
                    0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            }
            x += Advance * scale;
        }
    }

    /// <summary>Tek satırın piksel genişliği (her harf aynı genişlikte).</summary>
    public static float MeasureWidth(string text, int scale) =>
        text.Length == 0 ? 0f : (text.Length * Advance - 1) * scale;

    /// <summary>Bu karakter piksel fontla çizilebiliyor mu? (Çizilemeyenler sistem fontuna gider.)</summary>
    public bool CanRender(char c) => c == ' ' || _glyphs.ContainsKey(Normalize(c, turkishCasing: false));

    public void Dispose()
    {
        foreach (var texture in _glyphs.Values)
            texture.Dispose();
        _glyphs.Clear();
    }

    // ------------------------------------------------------------------
    // İç yardımcılar
    // ------------------------------------------------------------------

    private Texture2D GetGlyph(char c) => _glyphs.TryGetValue(c, out var tex) ? tex : _fallback;

    /// <summary>Dilin kurallarına uygun büyük harfe çevirir ve tipografik işaretleri sadeleştirir.</summary>
    private static char Normalize(char c, bool turkishCasing) => c switch
    {
        'i' => turkishCasing ? 'İ' : 'I',
        'ı' => 'I',
        'â' or 'Â' => 'A',
        'î' or 'Î' => 'İ',
        'û' or 'Û' => 'U',
        '’' or '‘' => '\'',
        '“' or '”' => '"',
        '—' or '–' => '-',
        '…' => '.',
        _ => char.ToUpperInvariant(c) // ç→Ç, ş→Ş, ğ→Ğ, ü→Ü, ö→Ö, ñ→Ñ, á→Á ...
    };

    private static Texture2D BuildGlyph(GraphicsDevice graphicsDevice, string[] body, Accent accent)
    {
        var data = new Color[GlyphWidth * GlyphHeight];

        void Set(int x, int y) => data[y * GlyphWidth + x] = Color.White;

        // Gövde: 2. satırdan başlar
        for (int row = 0; row < body.Length; row++)
            for (int col = 0; col < GlyphWidth; col++)
                if (body[row][col] == '#')
                    Set(col, row + 2);

        switch (accent)
        {
            case Accent.Dot:      Set(2, 0); break;
            case Accent.Dots:     Set(1, 0); Set(3, 0); break;
            case Accent.Breve:    Set(1, 0); Set(2, 0); Set(3, 0); break;
            case Accent.Cedilla:  Set(2, 9); break;
            case Accent.Acute:    Set(2, 0); Set(3, 0); break;           // Á É Í Ó Ú
            case Accent.Tilde:    Set(1, 0); Set(2, 0); Set(4, 0); Set(0, 1); Set(3, 1); break; // Ñ
        }

        var texture = new Texture2D(graphicsDevice, GlyphWidth, GlyphHeight);
        texture.SetData(data);
        return texture;
    }

    // ------------------------------------------------------------------
    // Glif tablosu (5x7, '#' dolu piksel)
    // ------------------------------------------------------------------

    private static readonly Dictionary<char, (char Base, Accent Accent)> AccentedGlyphs = new()
    {
        // İspanyolca
        ['Á'] = ('A', Accent.Acute),
        ['É'] = ('E', Accent.Acute),
        ['Í'] = ('I', Accent.Acute),
        ['Ó'] = ('O', Accent.Acute),
        ['Ú'] = ('U', Accent.Acute),
        ['Ñ'] = ('N', Accent.Tilde),
        // Türkçe
        ['Ç'] = ('C', Accent.Cedilla),
        ['Ş'] = ('S', Accent.Cedilla),
        ['Ğ'] = ('G', Accent.Breve),
        ['Ü'] = ('U', Accent.Dots),
        ['Ö'] = ('O', Accent.Dots),
        ['İ'] = ('I', Accent.Dot),
    };

    private static readonly Dictionary<char, string[]> BaseGlyphs = new()
    {
        ['A'] = new[] { ".###.", "#...#", "#...#", "#####", "#...#", "#...#", "#...#" },
        ['B'] = new[] { "####.", "#...#", "#...#", "####.", "#...#", "#...#", "####." },
        ['C'] = new[] { ".###.", "#...#", "#....", "#....", "#....", "#...#", ".###." },
        ['D'] = new[] { "####.", "#...#", "#...#", "#...#", "#...#", "#...#", "####." },
        ['E'] = new[] { "#####", "#....", "#....", "####.", "#....", "#....", "#####" },
        ['F'] = new[] { "#####", "#....", "#....", "####.", "#....", "#....", "#...." },
        ['G'] = new[] { ".###.", "#...#", "#....", "#.###", "#...#", "#...#", ".####" },
        ['H'] = new[] { "#...#", "#...#", "#...#", "#####", "#...#", "#...#", "#...#" },
        ['I'] = new[] { ".###.", "..#..", "..#..", "..#..", "..#..", "..#..", ".###." },
        ['J'] = new[] { "..###", "...#.", "...#.", "...#.", "...#.", "#..#.", ".##.." },
        ['K'] = new[] { "#...#", "#..#.", "#.#..", "##...", "#.#..", "#..#.", "#...#" },
        ['L'] = new[] { "#....", "#....", "#....", "#....", "#....", "#....", "#####" },
        ['M'] = new[] { "#...#", "##.##", "#.#.#", "#.#.#", "#...#", "#...#", "#...#" },
        ['N'] = new[] { "#...#", "#...#", "##..#", "#.#.#", "#..##", "#...#", "#...#" },
        ['O'] = new[] { ".###.", "#...#", "#...#", "#...#", "#...#", "#...#", ".###." },
        ['P'] = new[] { "####.", "#...#", "#...#", "####.", "#....", "#....", "#...." },
        ['Q'] = new[] { ".###.", "#...#", "#...#", "#...#", "#.#.#", "#..#.", ".##.#" },
        ['R'] = new[] { "####.", "#...#", "#...#", "####.", "#.#..", "#..#.", "#...#" },
        ['S'] = new[] { ".####", "#....", "#....", ".###.", "....#", "....#", "####." },
        ['T'] = new[] { "#####", "..#..", "..#..", "..#..", "..#..", "..#..", "..#.." },
        ['U'] = new[] { "#...#", "#...#", "#...#", "#...#", "#...#", "#...#", ".###." },
        ['V'] = new[] { "#...#", "#...#", "#...#", "#...#", "#...#", ".#.#.", "..#.." },
        ['W'] = new[] { "#...#", "#...#", "#...#", "#.#.#", "#.#.#", "#.#.#", ".#.#." },
        ['X'] = new[] { "#...#", "#...#", ".#.#.", "..#..", ".#.#.", "#...#", "#...#" },
        ['Y'] = new[] { "#...#", "#...#", ".#.#.", "..#..", "..#..", "..#..", "..#.." },
        ['Z'] = new[] { "#####", "....#", "...#.", "..#..", ".#...", "#....", "#####" },

        ['0'] = new[] { ".###.", "#...#", "#..##", "#.#.#", "##..#", "#...#", ".###." },
        ['1'] = new[] { "..#..", ".##..", "..#..", "..#..", "..#..", "..#..", ".###." },
        ['2'] = new[] { ".###.", "#...#", "....#", "...#.", "..#..", ".#...", "#####" },
        ['3'] = new[] { "####.", "....#", "....#", ".###.", "....#", "....#", "####." },
        ['4'] = new[] { "...#.", "..##.", ".#.#.", "#..#.", "#####", "...#.", "...#." },
        ['5'] = new[] { "#####", "#....", "####.", "....#", "....#", "#...#", ".###." },
        ['6'] = new[] { ".###.", "#....", "#....", "####.", "#...#", "#...#", ".###." },
        ['7'] = new[] { "#####", "....#", "...#.", "..#..", ".#...", ".#...", ".#..." },
        ['8'] = new[] { ".###.", "#...#", "#...#", ".###.", "#...#", "#...#", ".###." },
        ['9'] = new[] { ".###.", "#...#", "#...#", ".####", "....#", "....#", ".###." },

        ['.'] = new[] { ".....", ".....", ".....", ".....", ".....", ".##..", ".##.." },
        [','] = new[] { ".....", ".....", ".....", ".....", ".##..", "..#..", ".#..." },
        [':'] = new[] { ".....", ".##..", ".##..", ".....", ".##..", ".##..", "....." },
        [';'] = new[] { ".....", ".##..", ".##..", ".....", ".##..", "..#..", ".#..." },
        ['!'] = new[] { "..#..", "..#..", "..#..", "..#..", "..#..", ".....", "..#.." },
        ['?'] = new[] { ".###.", "#...#", "....#", "...#.", "..#..", ".....", "..#.." },
        ['¿'] = new[] { "..#..", ".....", "..#..", ".#...", "#....", "#...#", ".###." },
        ['¡'] = new[] { "..#..", ".....", "..#..", "..#..", "..#..", "..#..", "..#.." },
        ['-'] = new[] { ".....", ".....", ".....", ".###.", ".....", ".....", "....." },
        ['_'] = new[] { ".....", ".....", ".....", ".....", ".....", ".....", "#####" },
        ['\''] = new[] { "..#..", "..#..", ".#...", ".....", ".....", ".....", "....." },
        ['"'] = new[] { ".#.#.", ".#.#.", ".....", ".....", ".....", ".....", "....." },
        ['('] = new[] { "...#.", "..#..", ".#...", ".#...", ".#...", "..#..", "...#." },
        [')'] = new[] { ".#...", "..#..", "...#.", "...#.", "...#.", "..#..", ".#..." },
        ['['] = new[] { ".###.", ".#...", ".#...", ".#...", ".#...", ".#...", ".###." },
        [']'] = new[] { ".###.", "...#.", "...#.", "...#.", "...#.", "...#.", ".###." },
        ['/'] = new[] { "....#", "...#.", "...#.", "..#..", ".#...", ".#...", "#...." },
        ['%'] = new[] { "##..#", "##.#.", "...#.", "..#..", ".#...", ".#.##", "#..##" },
        ['+'] = new[] { ".....", "..#..", "..#..", "#####", "..#..", "..#..", "....." },
        ['='] = new[] { ".....", ".....", "#####", ".....", "#####", ".....", "....." },
        ['>'] = new[] { ".#...", "..#..", "...#.", "....#", "...#.", "..#..", ".#..." },
        ['<'] = new[] { "...#.", "..#..", ".#...", "#....", ".#...", "..#..", "...#." },
        ['*'] = new[] { ".....", "#.#.#", ".###.", "#####", ".###.", "#.#.#", "....." },
        ['°'] = new[] { ".##..", "#..#.", ".##..", ".....", ".....", ".....", "....." },
    };
}
