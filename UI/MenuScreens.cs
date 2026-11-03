using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheLastDelivery.Core;
using TheLastDelivery.Graphics;
using TheLastDelivery.Localization;
using TheLastDelivery.Story;

namespace TheLastDelivery.UI;

/// <summary>Dil seçimi, ana menü, eğitim sorusu, bölüm kartı ve duraklatma ekranları. Hepsi canlı dünyanın üzerine çizilir.</summary>
public static class MenuScreens
{
    private const float CenterX = GameConfig.ScreenWidth / 2f;

    // Dil seçim ekranının başlığı her dilde sırayla belirir
    private static readonly (Language Language, string Text)[] SelectLanguageTitles =
    {
        (Language.Turkish, "DİL SEÇİN"),
        (Language.English, "SELECT LANGUAGE"),
        (Language.Spanish, "ELIGE TU IDIOMA"),
        (Language.Chinese, "选择语言"),
        (Language.Hindi, "भाषा चुनें"),
        (Language.Arabic, "اختر اللغة"),
    };

    // ------------------------------------------------------------------
    // Dil seçimi
    // ------------------------------------------------------------------

    public static void DrawLanguageSelect(SpriteBatch sb, int selectedIndex, float time, float timeInState)
    {
        float fade = MathHelper.Clamp(timeInState / 0.6f, 0f, 1f);
        sb.FillRect(new Rectangle(0, 0, GameConfig.ScreenWidth, GameConfig.ScreenHeight), new Color(4, 6, 14) * (0.7f * fade));

        GameText.DrawCentered(sb, "THE LAST DELIVERY", CenterX, 36, Hud.Amber * fade, 4, Language.English);

        // Başlık her 2 saniyede başka bir dile geçer (yumuşak geçişle)
        const float period = 2f;
        int titleIndex = (int)(time / period) % SelectLanguageTitles.Length;
        float phase = time % period;
        float titleAlpha = MathHelper.Clamp(MathF.Min(phase, period - phase) / 0.3f, 0f, 1f) * fade;
        var title = SelectLanguageTitles[titleIndex];
        GameText.DrawCentered(sb, title.Text, CenterX, 104, Color.White * titleAlpha, 3, title.Language);

        for (int i = 0; i < Languages.All.Length; i++)
        {
            var info = Languages.All[i];
            bool available = GameText.IsAvailable(info.Id);
            bool selected = i == selectedIndex;

            var row = new Rectangle(400, 168 + i * 66, 480, 54);
            Color edge = !available ? Hud.PanelEdge * 0.4f : selected ? Hud.Amber : Hud.PanelEdge;
            sb.FillRect(row, (selected ? Hud.Amber * 0.12f : new Color(6, 8, 16) * 0.85f) * fade);
            sb.DrawBorder(row, selected ? 3 : 1, edge * fade);

            // Sol: seçim tuşu
            var keyBox = new Rectangle(row.X + 14, row.Y + 12, 32, 30);
            sb.DrawBorder(keyBox, 2, edge * fade);
            GameText.DrawCentered(sb, (i + 1).ToString(), keyBox.Center.X, keyBox.Y + 5, edge * fade, 2, Language.English);

            // Orta: dilin kendi adı, kendi yazısıyla
            Color nameColor = !available ? Hud.Label * 0.35f : selected ? Color.White : Hud.Value;
            GameText.Draw(sb, info.NativeName, new Vector2(row.X + 66, row.Y + 9), nameColor * fade, 3, info.Id);

            // Sağ: İngilizce adı (font yoksa uyarı)
            string note = available ? info.EnglishName : "(FONT MISSING)";
            float noteWidth = GameText.Measure(note, 2, Language.English).X;
            GameText.Draw(sb, note, new Vector2(row.Right - 16 - noteWidth, row.Y + 18), Hud.Label * (0.7f * fade), 2, Language.English);

            if (selected && available)
                GameText.Draw(sb, ">", new Vector2(row.X - 28 + MathF.Sin(time * 6f) * 3f, row.Y + 12), Hud.Amber * fade, 3, Language.English);
        }

        GameText.DrawCentered(sb, "W/S  -  1-6  -  ENTER", CenterX, 580, Hud.Label * (0.8f * fade), 2, Language.English);
    }

    // ------------------------------------------------------------------
    // Eğitim sorusu
    // ------------------------------------------------------------------

    public static void DrawTutorialPrompt(SpriteBatch sb, float time, float timeInState)
    {
        float fade = MathHelper.Clamp(timeInState / 0.3f, 0f, 1f);

        sb.FillRect(new Rectangle(0, 0, GameConfig.ScreenWidth, GameConfig.ScreenHeight), new Color(4, 6, 14) * (0.6f * fade));

        var box = new Rectangle(270, 150, 740, 280);
        sb.FillRect(box, new Color(6, 8, 16) * (0.95f * fade));
        sb.DrawBorder(box, 2, Hud.PanelEdge * fade);

        GameText.DrawCentered(sb, Loc.T("tp.title"), CenterX, box.Y + 26, Hud.Amber * fade, 4);
        GameText.DrawCentered(sb, Loc.T("tp.question"), CenterX, box.Y + 84, Hud.Value * fade, 2);
        GameText.DrawCentered(sb, Loc.T("tp.desc"), CenterX, box.Y + 112, Hud.Label * fade, 2);

        DrawKeyOption(sb, "1", Loc.T("tp.yes"), box.X + 170, box.Y + 160, fade);
        DrawKeyOption(sb, "2", Loc.T("tp.no"), box.X + 170, box.Y + 204, fade);

        GameText.DrawCentered(sb, Loc.T("tp.back"), CenterX, box.Bottom + 16, Hud.Label * (0.5f * fade), 2);
    }

    // ------------------------------------------------------------------
    // Bölüm kartı
    // ------------------------------------------------------------------

    /// <summary>Bölüm geçiş kartı: siyah fonda bölüm adı, kalan mesafe ve kısa hikaye.</summary>
    /// <param name="logKey">Bölüm arası özet: "Arthur'un Defteri" satırının anahtarı (yoksa null).</param>
    public static void DrawChapterIntro(SpriteBatch sb, Chapter chapter, float timeInState, string logKey = null)
    {
        // Siyah perde hızla iner, yazılar sırayla belirir
        float curtain = MathHelper.Clamp(timeInState / 0.5f, 0f, 1f);
        float header = MathHelper.Clamp((timeInState - 0.4f) / 0.8f, 0f, 1f);
        float body = MathHelper.Clamp((timeInState - 1.0f) / 1.0f, 0f, 1f);

        sb.FillRect(new Rectangle(0, 0, GameConfig.ScreenWidth, GameConfig.ScreenHeight), Color.Black * curtain);

        GameText.DrawCentered(sb, Loc.F("chapter.label", chapter.Number), CenterX, 140, Hud.Amber * header, 3);
        int nameScale = GameText.Measure(chapter.Name, 6).X <= 1180 ? 6 : 4;
        GameText.DrawCentered(sb, chapter.Name, CenterX, 190, Color.White * header, nameScale);
        GameText.DrawCentered(sb, chapter.RemainingRange, CenterX, 276, Hud.Label * header, 2);
        sb.FillRect(new Rectangle((int)CenterX - 160, 310, 320, 2), Hud.Amber * (0.5f * header));

        float y = 336;
        foreach (string line in GameText.Wrap(chapter.Story, 900, 2))
        {
            GameText.DrawCentered(sb, line, CenterX, y, Hud.Value * body, 2);
            y += GameText.LineHeight(2) + 6;
        }
        GameText.DrawCentered(sb, chapter.Effect, CenterX, y + 14, Hud.Amber * body, 2);
        if (chapter.Kind == ChapterKind.Story && chapter.IsCritical && !chapter.IsStoryScene)
            GameText.DrawCentered(sb, Loc.T("chs.critical"), CenterX, y + 46, Hud.Danger * body, 2);

        // Arthur'un Defteri: geçmişe ve AURA'ya dair kısa, el yazısı gibi bir not (biraz geç belirir)
        string log = logKey != null ? Loc.T(logKey) : null;
        if (log != null && log != logKey)
        {
            float note = MathHelper.Clamp((timeInState - 1.6f) / 0.8f, 0f, 1f);
            float ly = MathF.Max(y + 86, 500);
            var noteColor = new Color(170, 200, 255);
            GameText.DrawCentered(sb, Loc.T("log.title"), CenterX, ly, noteColor * (0.75f * note), 2);
            ly += GameText.LineHeight(2) + 8;
            foreach (string line in GameText.Wrap(log, 860, 2))
            {
                GameText.DrawCentered(sb, line, CenterX, ly, noteColor * note, 2);
                ly += GameText.LineHeight(2) + 4;
            }
        }

        if (body >= 1f)
        {
            float pulse = 0.6f + 0.4f * MathF.Sin(timeInState * 4f);
            GameText.DrawCentered(sb, Loc.T("prompt.continue"), CenterX, 630, Hud.Value * pulse, 3);
        }
    }

    // ------------------------------------------------------------------
    // Duraklatma
    // ------------------------------------------------------------------

    public static void DrawPause(SpriteBatch sb, string journeySummary)
    {
        sb.FillRect(new Rectangle(0, 0, GameConfig.ScreenWidth, GameConfig.ScreenHeight), new Color(4, 6, 14) * 0.75f);

        var box = new Rectangle(200, 200, 880, 300);
        sb.FillRect(box, new Color(6, 8, 16) * 0.95f);
        sb.DrawBorder(box, 2, Hud.PanelEdge);

        GameText.DrawCentered(sb, Loc.T("pause.title"), CenterX, box.Y + 34, Hud.Amber, 5);
        sb.FillRect(new Rectangle(box.X + 60, box.Y + 104, box.Width - 120, 2), Hud.Amber * 0.4f);

        GameText.DrawCentered(sb, journeySummary, CenterX, box.Y + 126, Hud.Label, 2);
        GameText.DrawCentered(sb, Loc.T("pause.controls"), CenterX, box.Y + 166, Hud.Label * 0.8f, 2);

        GameText.DrawCentered(sb, Loc.T("pause.resume"), CenterX, box.Y + 220, Hud.Value, 3);
        GameText.DrawCentered(sb, Loc.T("pause.quit"), CenterX, box.Y + 256, Hud.Danger * 0.9f, 2);
    }

    private static void DrawKeyOption(SpriteBatch sb, string key, string label, int x, int y, float alpha)
    {
        var keyBox = new Rectangle(x, y - 4, 40, 30);
        sb.FillRect(keyBox, Hud.Amber * (0.15f * alpha));
        sb.DrawBorder(keyBox, 2, Hud.Amber * alpha);
        GameText.DrawCentered(sb, key, keyBox.Center.X, keyBox.Y + 5, Hud.Amber * alpha, 2);
        GameText.Draw(sb, label, new Vector2(x + 58, y), Hud.Value * alpha, 2);
    }
}
