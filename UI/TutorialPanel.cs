using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheLastDelivery.Graphics;
using TheLastDelivery.Localization;
using TheLastDelivery.Story;

namespace TheLastDelivery.UI;

/// <summary>
/// Koca Thomas'ın konuşma paneli: piksel portre, daktilo metni, adım göstergesi
/// ve anlatılan arayüz bölgesinin etrafında nabız gibi atan vurgu çerçevesi.
/// </summary>
public static class TutorialPanel
{
    // Sol HUD'un altında, Kabin Panosu'na (x=836) değmeden
    private static readonly Rectangle Box = new(16, 250, 804, 170);
    private const int PortraitSize = 100;

    public static void Draw(SpriteBatch sb, Tutorial tutorial, float time)
    {
        var step = tutorial.Current;

        DrawHighlight(sb, step.Highlight, time);

        sb.FillRect(Box, new Color(8, 7, 10) * 0.92f);
        sb.DrawBorder(Box, 2, Hud.Amber * 0.7f);

        var portrait = new Rectangle(Box.X + 18, Box.Y + 22, PortraitSize, PortraitSize);
        DrawPortrait(sb, portrait);
        GameText.DrawCentered(sb, Loc.T("tut.short"), portrait.Center.X, portrait.Bottom + 8, Hud.Label * 0.8f, 2);

        int textX = Box.X + 136;
        int textRight = Box.Right - 24;
        // Üst satır: ad ve adım solda, atlama ipucu sağda (alt satır uzun hedef metinlerine kalsın)
        string name = Loc.T("tut.name");
        GameText.Draw(sb, name, new Vector2(textX, Box.Y + 16), Hud.Amber);
        GameText.Draw(sb, Loc.F("tut.step", tutorial.StepIndex + 1, Tutorial.Steps.Length),
            new Vector2(textX + GameText.MeasureLine(name) + 24, Box.Y + 16), Hud.Label);
        GameText.DrawRight(sb, Loc.T("tut.skip"), textRight, Box.Y + 16, Hud.Label * 0.6f);

        // Konuşma metni: tam metin satırlara bölünür, daktilo efektiyle karakter karakter açılır
        bool rtl = GameText.IsRightToLeft;
        int budget = tutorial.VisibleChars;
        float y = Box.Y + 46;
        foreach (string line in GameText.Wrap(step.Text, textRight - textX, 2))
        {
            if (budget <= 0) break;
            float x = rtl ? textRight - GameText.MeasureLine(line) : textX;
            GameText.DrawPartial(sb, line, budget, new Vector2(x, y), Hud.Value);
            budget -= line.Length;
            y += 28;
        }

        // Alt satır: hedef ya da kutlama
        var promptPos = new Vector2(textX, Box.Bottom - 30);
        if (tutorial.IsCelebrating)
            GameText.Draw(sb, Loc.T("tut.good"), promptPos, CabinPanel.Good);
        else if (tutorial.IsTextComplete)
            GameText.Draw(sb, step.Prompt, promptPos, Hud.Amber * (0.65f + 0.35f * MathF.Sin(time * 5f)));
    }

    private static void DrawHighlight(SpriteBatch sb, TutorialHighlight highlight, float time)
    {
        float pulse = 0.5f + 0.5f * MathF.Sin(time * 6f);
        var color = Hud.Amber * (0.4f + 0.6f * pulse);

        switch (highlight)
        {
            case TutorialHighlight.Speed:
                sb.DrawBorder(Hud.SpeedRow, 2, color);
                break;
            case TutorialHighlight.Resources:
                sb.DrawBorder(Hud.TemperatureRow, 2, color);
                sb.DrawBorder(CabinPanel.FuelRow, 2, color);
                break;
        }
    }

    /// <summary>Koca Thomas: makinist şapkası, gözlükleri ve gür beyaz bıyığıyla yaşlı bir makinist.</summary>
    private static void DrawPortrait(SpriteBatch sb, Rectangle frame)
    {
        const int u = PortraitSize / 20; // 20x20'lik piksel ızgarası

        void Px(int gx, int gy, int gw, int gh, Color c) =>
            sb.FillRect(new Rectangle(frame.X + gx * u, frame.Y + gy * u, gw * u, gh * u), c);

        var skin = new Color(205, 160, 130);
        var shade = new Color(175, 128, 104);
        var white = new Color(228, 228, 234);
        var grey = new Color(196, 196, 206);

        Px(0, 0, 20, 20, new Color(38, 28, 24));          // Arka plan (kabin ışığı)
        Px(1, 15, 18, 5, new Color(72, 46, 36));          // Ceket omuzları
        Px(8, 15, 4, 5, new Color(52, 34, 28));           // Yaka

        Px(5, 7, 10, 7, skin);                            // Yüz
        Px(4, 9, 1, 3, shade); Px(15, 9, 1, 3, shade);   // Kulaklar

        Px(4, 2, 12, 4, new Color(42, 52, 84));           // Makinist şapkası
        Px(3, 6, 14, 1, new Color(28, 34, 58));           // Siper
        Px(6, 3, 3, 2, new Color(255, 170, 80));          // Şapkadaki gözlükler
        Px(11, 3, 3, 2, new Color(255, 170, 80));
        Px(9, 3, 2, 1, new Color(90, 70, 50));

        Px(6, 8, 3, 1, white); Px(11, 8, 3, 1, white);   // Kaşlar
        Px(7, 9, 1, 1, new Color(30, 30, 34));            // Gözler
        Px(12, 9, 1, 1, new Color(30, 30, 34));
        Px(9, 10, 2, 2, shade);                           // Burun
        Px(14, 11, 1, 1, new Color(70, 60, 60));          // Yanakta kömür izi

        Px(5, 12, 10, 2, white);                          // Gür bıyık
        Px(6, 14, 8, 2, grey);                            // Sakal
        Px(7, 16, 6, 1, grey);
        Px(8, 17, 4, 1, grey);

        sb.DrawBorder(frame, 2, Hud.Amber * 0.6f);
    }
}
