using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheLastDelivery.Graphics;
using TheLastDelivery.Localization;

namespace TheLastDelivery.UI;

/// <summary>Diyalogda oyuncuya sunulan tek bir seçenek.</summary>
public sealed record DialogOption(string Key, string Label, bool Enabled);

/// <summary>
/// İstasyonlarda açılan karanlık günlük penceresi.
/// Pencere ortadan dikey olarak yumuşakça açılır/kapanır, metin daktilo efektiyle akar,
/// varsa seçenekler metin bittikten sonra kayarak belirir.
/// Sağdan sola yazılan dillerde (Arapça) gövde metni sağa hizalanır.
/// </summary>
public sealed class DialogBox
{
    private const float CharsPerSecond = 45f;
    private const float OpenDuration = 0.35f;
    private const float CloseDuration = 0.25f;
    private const float OptionsFadeDuration = 0.4f;
    private const int Padding = 30;
    private const int TextScale = 2;
    private const int OptionIndent = 54;

    private static readonly Rectangle Bounds = new(170, 50, 940, 440);
    private static readonly float LineStep = GameText.LineHeight(TextScale) + 6;

    private static readonly Color TextColor = new(210, 215, 228);
    private static readonly Color Muted = new(110, 116, 135);

    private string _title = string.Empty;
    private string _continueKey = "dlg.continue";
    private Speaker _speaker;
    private float _badgeTime;   // Destekçi rozetinin parıltı animasyonu

    // Konuşan kişi varsa portre kutusu metnin yanında yer kaplar
    private const int PortraitSize = 100;
    private const int PortraitSpace = 128;
    private int TextLeft => Bounds.X + Padding + (_speaker != null && !GameText.IsRightToLeft ? PortraitSpace : 0);
    private int TextRight => Bounds.Right - Padding - (_speaker != null && GameText.IsRightToLeft ? PortraitSpace : 0);
    private int BodyTop => Bounds.Y + 84 + (_speaker != null ? 34 : 0);
    private List<string> _lines = new();
    private readonly List<(DialogOption Option, List<string> Lines)> _options = new();
    private int _totalChars;
    private float _visibleChars;
    private float _openProgress;   // 0 = kapalı, 1 = tamamen açık
    private float _optionsFade;
    private bool _closing;

    /// <summary>Metnin tamamı ekrana yazıldı mı?</summary>
    public bool IsComplete => _visibleChars >= _totalChars;

    /// <summary>Seçenekler görünür ve seçilebilir durumda mı?</summary>
    public bool AcceptsChoice => IsComplete && _options.Count > 0 && _openProgress >= 1f && !_closing;

    /// <summary>Close() çağrıldıktan sonra kapanma animasyonu bitti mi?</summary>
    public bool IsClosed => _closing && _openProgress <= 0f;

    /// <param name="continueKey">Seçeneksiz pencerede gösterilecek devam komutunun çeviri anahtarı.</param>
    /// <param name="speaker">Konuşan kişi: adı ve piksel portresi metnin yanında gösterilir.</param>
    public void Open(string title, string text, IReadOnlyList<DialogOption> options = null,
        string continueKey = "dlg.continue", Speaker speaker = null)
    {
        _title = title;
        _continueKey = continueKey;
        _speaker = speaker;
        int textWidth = TextRight - TextLeft;
        _lines = GameText.Wrap(text, textWidth, TextScale);
        _totalChars = 0;
        foreach (string line in _lines)
            _totalChars += line.Length;

        _options.Clear();
        if (options != null)
        {
            foreach (var option in options)
                _options.Add((option, GameText.Wrap(option.Label, textWidth - OptionIndent, TextScale)));
        }

        _visibleChars = 0f;
        _openProgress = 0f;
        _optionsFade = 0f;
        _closing = false;
    }

    public void Close() => _closing = true;

    public void Update(float dt)
    {
        if (_closing)
        {
            _openProgress = MathF.Max(0f, _openProgress - dt / CloseDuration);
            return;
        }

        _openProgress = MathF.Min(1f, _openProgress + dt / OpenDuration);
        if (_openProgress < 1f) return; // Pencere açılmadan yazı başlamaz

        _visibleChars = MathF.Min(_totalChars, _visibleChars + CharsPerSecond * dt);
        if (IsComplete)
            _optionsFade = MathF.Min(1f, _optionsFade + dt / OptionsFadeDuration);
    }

    /// <summary>Açılış animasyonunu ve daktilo efektini atlayıp metnin tamamını gösterir.</summary>
    public void Skip()
    {
        _openProgress = 1f;
        _visibleChars = _totalChars;
    }

    public void Draw(SpriteBatch sb, float time)
    {
        // Kübik yumuşatma: hızlı başlar, yavaşlayarak oturur
        float eased = 1f - MathF.Pow(1f - _openProgress, 3f);
        if (eased <= 0.01f) return;

        int height = (int)(Bounds.Height * eased);
        var box = new Rectangle(Bounds.X, Bounds.Center.Y - height / 2, Bounds.Width, height);
        sb.FillRect(box, new Color(5, 6, 12) * (0.6f * eased));
        UiDraw.FancyPanel(sb, box, new Color(80, 90, 115) * eased);

        // İçerik, pencere neredeyse açıldığında belirir
        float alpha = MathHelper.Clamp((eased - 0.75f) / 0.25f, 0f, 1f);
        if (alpha <= 0f) return;

        int x = Bounds.X + Padding;
        if (GameText.IsRightToLeft)
            GameText.DrawRight(sb, _title, Bounds.Right - Padding, Bounds.Y + 22, Hud.Amber * alpha, 3);
        else
            GameText.Draw(sb, _title, new Vector2(x, Bounds.Y + 22), Hud.Amber * alpha, 3);
        sb.FillRect(new Rectangle(x, Bounds.Y + 62, Bounds.Width - Padding * 2, 2), Hud.Amber * (0.5f * alpha));

        if (_speaker != null)
            DrawSpeaker(sb, alpha);

        float y = DrawBody(sb, time, alpha);
        DrawOptions(sb, y + 12, alpha);
        DrawPrompt(sb, time, alpha);
    }

    /// <summary>
    /// Konuşan kişi: portre kutusu (okuma yönünün başında) ve metnin üstünde adı.
    /// Portre içerikle birlikte hafifçe kayarak belirir.
    /// </summary>
    private void DrawSpeaker(SpriteBatch sb, float alpha)
    {
        bool rtl = GameText.IsRightToLeft;
        int slide = (int)((1f - alpha) * 16f);
        int portraitX = rtl ? Bounds.Right - Padding - PortraitSize + slide : Bounds.X + Padding - slide;
        var frame = new Rectangle(portraitX, Bounds.Y + 84, PortraitSize, PortraitSize);

        sb.FillRect(new Rectangle(frame.X + 4, frame.Y + 4, frame.Width, frame.Height), Color.Black * (0.5f * alpha));
        Portraits.Draw(sb, frame, _speaker.Portrait, alpha);

        // Ad levhası: amber yazı ve altında ince çizgi
        float nameWidth = GameText.MeasureLine(_speaker.Name);
        float nameX = rtl ? TextRight - nameWidth : TextLeft;
        GameText.Draw(sb, _speaker.Name, new Vector2(nameX, Bounds.Y + 80), Hud.Amber * alpha);
        sb.FillRect(new Rectangle((int)nameX, Bounds.Y + 104, (int)nameWidth, 2), Hud.Amber * (0.4f * alpha));

        // Oyuncunun (Arthur) adının yanında Destekçi Rozeti — Supporter Pack sahiplerine
        if (_speaker.IsPlayer && Platform.DLCManager.ShowSupporterBadge)
        {
            int badgeX = rtl ? (int)nameX - 12 - ((int)GameText.MeasureLine(Localization.Loc.T("badge.supporter")) + 34)
                             : (int)(nameX + nameWidth) + 12;
            UiDraw.SupporterBadge(sb, new Point(badgeX, Bounds.Y + 78), alpha, _badgeTime += 1f / 60f);
        }
    }

    /// <summary>Gövde metnini görünür karakter bütçesi kadar çizer; sonraki boş satırın Y'sini döner.</summary>
    private float DrawBody(SpriteBatch sb, float time, float alpha)
    {
        bool rtl = GameText.IsRightToLeft;
        int budget = (int)_visibleChars;
        float y = BodyTop;
        Vector2 cursor = new(TextLeft, y);

        foreach (string line in _lines)
        {
            if (budget <= 0) break;
            int visible = Math.Min(line.Length, budget);
            budget -= line.Length;

            // "> " ile başlayan satırlar kaynak özetidir, vurgulu çizilir
            Color color = line.StartsWith(">") ? Hud.Amber : TextColor;
            float lineWidth = GameText.MeasureLine(line);
            float x = rtl ? TextRight - lineWidth : TextLeft;

            GameText.DrawPartial(sb, line, visible, new Vector2(x, y), color * alpha);
            if (!rtl)
                cursor = new Vector2(x + GameText.MeasureLine(line.Substring(0, visible)), y);
            y += LineStep;
        }

        // Yazı akarken yanıp sönen imleç (soldan sağa dillerde)
        if (!IsComplete && !rtl && MathF.Sin(time * 6f) > 0f)
            sb.FillRect(new Rectangle((int)cursor.X + 4, (int)cursor.Y + 4, 10, 16), TextColor * alpha);

        return IsComplete ? BodyTop + _lines.Count * LineStep : y;
    }

    private void DrawOptions(SpriteBatch sb, float y, float alpha)
    {
        if (_options.Count == 0 || _optionsFade <= 0f) return;

        float fade = _optionsFade * alpha;
        float slide = (1f - _optionsFade) * 24f; // Okuma yönünün başından kayarak gelir
        bool rtl = GameText.IsRightToLeft;

        foreach (var (option, lines) in _options)
        {
            Color keyColor = option.Enabled ? Hud.Amber : Muted;
            Color labelColor = option.Enabled ? Hud.Value : Muted;

            // Soldan sağa dillerde tuş solda; Arapçada tuş sağda, metin ona sağdan yaslı
            int keyX = rtl ? (int)(TextRight - 40 - slide) : (int)(TextLeft + slide);
            var keyBox = new Rectangle(keyX, (int)y - 2, 40, 28);
            sb.FillRect(keyBox, keyColor * (0.15f * fade));
            sb.DrawBorder(keyBox, 2, keyColor * fade);
            GameText.DrawCentered(sb, option.Key, keyBox.Center.X, keyBox.Y + 4, keyColor * fade);

            foreach (string line in lines)
            {
                if (rtl)
                    GameText.DrawRight(sb, line, keyBox.X - 14, y, labelColor * fade);
                else
                    GameText.Draw(sb, line, new Vector2(keyBox.X + OptionIndent, y), labelColor * fade);
                y += LineStep;
            }
            y += 8;
        }
    }

    private void DrawPrompt(SpriteBatch sb, float time, float alpha)
    {
        string prompt;
        Color color = Muted;

        if (!IsComplete) prompt = Loc.T("dlg.skip");
        else if (_options.Count > 0)
        {
            prompt = Loc.T(_options.Count switch { > 3 => "dlg.choose4", > 2 => "dlg.choose3", _ => "dlg.choose" });
            color = Hud.Amber;
        }
        else { prompt = Loc.T(_continueKey); color = Hud.Amber; }

        if (IsComplete && MathF.Sin(time * 6f) < 0f) return; // Bekleyen komut yanıp söner

        GameText.DrawRight(sb, prompt, Bounds.Right - Padding, Bounds.Bottom - 34, color * alpha);
    }
}
