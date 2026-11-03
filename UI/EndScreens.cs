using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheLastDelivery.Core;
using TheLastDelivery.Graphics;
using TheLastDelivery.Localization;
using TheLastDelivery.Story;

namespace TheLastDelivery.UI;

/// <summary>
/// Üç final ekranı (İyi / Buruk / Trajik). Dünya arkada canlı kalır,
/// üstüne finalin tonuna göre renklenen bir perde yavaşça iner.
/// </summary>
public static class EndScreens
{
    private const float CenterX = GameConfig.ScreenWidth / 2f;

    private readonly record struct Palette(Color Curtain, Color Title, Color Name, Color Body);

    private static Palette PaletteFor(EndingType type) => type switch
    {
        EndingType.NewBeginning => new Palette(new Color(12, 9, 5), new Color(255, 205, 130), new Color(170, 225, 140), new Color(225, 218, 205)),
        EndingType.LoneWolf => new Palette(new Color(8, 8, 11), new Color(225, 205, 175), new Color(180, 185, 210), new Color(200, 200, 210)),
        EndingType.Merchant => new Palette(new Color(14, 4, 4), new Color(255, 90, 70), new Color(255, 200, 110), new Color(215, 190, 185)),
        EndingType.Sacrifice => new Palette(new Color(16, 8, 2), new Color(255, 170, 70), new Color(255, 235, 200), new Color(235, 220, 200)),
        EndingType.Thaw => new Palette(new Color(4, 14, 18), new Color(140, 230, 255), new Color(150, 255, 200), new Color(205, 225, 230)),
        EndingType.AuraFree => new Palette(new Color(10, 12, 8), new Color(255, 225, 150), new Color(170, 230, 140), new Color(230, 225, 210)),
        EndingType.AuraMachine => new Palette(new Color(4, 2, 8), new Color(200, 120, 255), new Color(255, 60, 80), new Color(210, 200, 230)),
        EndingType.AuraReset => new Palette(new Color(2, 4, 10), new Color(170, 200, 240), new Color(120, 140, 180), new Color(170, 180, 200)),
        _ => new Palette(new Color(4, 8, 18), new Color(170, 210, 255), new Color(255, 120, 100), new Color(160, 170, 195))
    };

    public static void Draw(SpriteBatch sb, Ending ending, float timeInState)
    {
        if (ending == null) return;

        var palette = PaletteFor(ending.Type);

        float fade = MathHelper.Clamp(timeInState / 1.5f, 0f, 1f);
        sb.FillRect(new Rectangle(0, 0, GameConfig.ScreenWidth, GameConfig.ScreenHeight), palette.Curtain * (0.85f * fade));

        if (timeInState < 0.8f) return; // Önce perde insin, sonra yazı belirsin
        float alpha = MathHelper.Clamp(timeInState - 0.8f, 0f, 1f);

        // Başlık sığmıyorsa bir boy küçült
        int titleScale = GameText.Measure(ending.Title, 6).X <= 1100 ? 6 : 5;
        GameText.DrawCentered(sb, ending.Title, CenterX, 80, palette.Title * alpha, titleScale);
        GameText.DrawCentered(sb, ending.Tagline, CenterX, 80 + titleScale * PixelFont.GlyphHeight + 14, palette.Body * (0.8f * alpha), 2);

        // Final adı, iki yanında ince çizgilerle
        float nameY = 190;
        float nameWidth = GameText.Measure(ending.Name, 3).X;
        GameText.DrawCentered(sb, ending.Name, CenterX, nameY, palette.Name * alpha, 3);
        sb.FillRect(new Rectangle((int)(CenterX - nameWidth / 2 - 70), (int)nameY + 14, 50, 2), palette.Name * (0.5f * alpha));
        sb.FillRect(new Rectangle((int)(CenterX + nameWidth / 2 + 20), (int)nameY + 14, 50, 2), palette.Name * (0.5f * alpha));

        // Hikaye metni, paragraf paragraf. Sonsöz ve yolcuların akıbeti alana sığmazsa
        // metin, okumaya yetecek bir beklemeden sonra jenerik gibi yavaşça yukarı kayar.
        const float textTop = 256f, textBottom = 604f, edgeFade = 26f;
        float step = GameText.LineHeight(2) + 6;
        var lines = GameText.Wrap(ending.Text, 960, 2);
        float overflow = MathF.Max(0f, lines.Count * step - (textBottom - textTop));
        float scroll = overflow > 0f ? MathHelper.Clamp((timeInState - 9f) * 16f, 0f, overflow) : 0f;

        float y = textTop - scroll;
        foreach (string line in lines)
        {
            if (y > textTop - step && y < textBottom)
            {
                float edge = overflow > 0f
                    ? MathHelper.Clamp((y - (textTop - step)) / edgeFade, 0f, 1f) * MathHelper.Clamp((textBottom - y) / edgeFade, 0f, 1f)
                    : 1f;
                GameText.DrawCentered(sb, line, CenterX, y, palette.Body * (alpha * edge), 2);
            }
            y += step;
        }

        // Kaybolmayan, nabız gibi atan komut
        float pulse = 0.6f + 0.4f * MathF.Sin(timeInState * 4f);
        GameText.DrawCentered(sb, Loc.T("end.menu"), CenterX, 630, Hud.Amber * (alpha * pulse), 3);
        GameText.DrawCentered(sb, Loc.T("end.quit"), CenterX, 672, Hud.Label * (0.6f * alpha), 2);
    }
}
