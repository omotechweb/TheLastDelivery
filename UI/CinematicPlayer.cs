using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TheLastDelivery.Audio;
using TheLastDelivery.Core;
using TheLastDelivery.Graphics;
using TheLastDelivery.Localization;
using TheLastDelivery.Story;

namespace TheLastDelivery.UI;

/// <summary>Sinematiğin bir sayfası: çizim ve metin anahtarı.</summary>
public sealed record CinematicPage(string Art, string TextKey);

/// <summary>
/// Uzun hikâye anlatım sahneleri (sinematikler). Her hikâye bölümü, sürüşten önce tam ekran, çizimli,
/// sayfa sayfa akan bir anlatımla açılır: "e1.{bölüm}" ve "e2.{bölüm}". Metinler "cs.{sahne}.{sayfa}".
/// Sahneler CinematicScript dosyalarında tanımlanır.
/// </summary>
public static class Cinematics
{
    private static readonly Dictionary<string, List<CinematicPage>> Scenes = new();

    public static void Add(string scene, string art)
    {
        if (!Scenes.TryGetValue(scene, out var pages)) Scenes[scene] = pages = new List<CinematicPage>();
        pages.Add(new CinematicPage(art, $"cs.{scene}.{pages.Count + 1}"));
    }

    public static IReadOnlyList<CinematicPage> Find(string scene)
    {
        CinematicScript.EnsureLoaded();
        return scene != null && Scenes.TryGetValue(scene, out var pages) ? pages : null;
    }

    public static IEnumerable<string> All
    {
        get
        {
            CinematicScript.EnsureLoaded();
            return Scenes.Keys;
        }
    }
}

/// <summary>
/// Sinematik oynatıcı: siyah sinema bantları, yavaşça kayan çizim, daktilo gibi akan metin.
///   BOŞLUK/ENTER: metni tamamla, sonra sonraki sayfa   BACKSPACE (basılı tut): sahneyi atla
/// Açıkken oyun zamanı durur.
/// </summary>
public sealed class CinematicPlayer
{
    private const float CharsPerSecond = 30f; // Okunabilir, sinematik bir akış
    private const float PageFade = 0.6f;
    private const float SkipHold = 1.2f;

    private IReadOnlyList<CinematicPage> _pages;
    private int _page;
    private float _pageTime, _typed, _time, _skipHeld, _closing = -1f;
    private string _title, _text;
    private Action _onDone;

    public bool IsOpen { get; private set; }

    /// <returns>Sahne varsa true (oynatılmaya başlar).</returns>
    public bool Open(string scene, string title, Action onDone)
    {
        var pages = Cinematics.Find(scene);
        if (pages == null || pages.Count == 0) return false;
        _pages = pages;
        _page = 0;
        _title = title;
        _onDone = onDone;
        _skipHeld = 0f;
        _closing = -1f;
        IsOpen = true;
        StartPage();
        return true;
    }

    private void StartPage()
    {
        _pageTime = 0f;
        _typed = 0f;
        _text = Loc.T(_pages[_page].TextKey);
    }

    public void Update(float dt, InputState input, ProceduralAudio audio)
    {
        if (!IsOpen) return;
        _time += dt;

        if (_closing >= 0f)
        {
            _closing += dt;
            if (_closing >= PageFade) Finish();
            return;
        }

        _pageTime += dt;
        if (_pageTime > PageFade * 0.5f) _typed = MathF.Min(_text.Length, _typed + dt * CharsPerSecond);

        // Basılı tutulan BACKSPACE bütün sahneyi atlar
        if (input.Down(Keys.Back))
        {
            _skipHeld += dt;
            if (_skipHeld >= SkipHold)
            {
                audio.PlayClick();
                _closing = 0f;
            }
            return;
        }
        _skipHeld = 0f;

        if (!input.AnyPressed(Keys.Space, Keys.Enter)) return;
        if (_typed < _text.Length)
        {
            _typed = _text.Length;
            return;
        }
        audio.PlayClick();
        if (_page + 1 < _pages.Count)
        {
            _page++;
            StartPage();
        }
        else
        {
            _closing = 0f;
        }
    }

    private void Finish()
    {
        IsOpen = false;
        var done = _onDone;
        _onDone = null;
        done?.Invoke();
    }

    public void Draw(SpriteBatch sb)
    {
        if (!IsOpen) return;
        int w = GameConfig.ScreenWidth, h = GameConfig.ScreenHeight;
        sb.FillRect(new Rectangle(0, 0, w, h), Color.Black);

        float fadeIn = MathHelper.Clamp(_pageTime / PageFade, 0f, 1f);
        float fadeOut = _closing >= 0f ? 1f - MathHelper.Clamp(_closing / PageFade, 0f, 1f) : 1f;
        float a = fadeIn * fadeOut;

        // Çizim: hafifçe kayan "kamera"
        int drift = (int)(MathF.Sin(_time * 0.15f) * 12f);
        var art = new Rectangle(drift - 20, 56, w + 40, 430);
        CinematicArt.Draw(sb, art, _pages[_page].Art, _time, a);
        sb.Draw(Assets.Vignette, new Rectangle(0, 56, w, 430), Color.Black * (0.55f * a));

        // Sinema bantları
        sb.FillRect(new Rectangle(0, 0, w, 56), Color.Black);
        sb.FillRect(new Rectangle(0, 486, w, h - 486), Color.Black);

        if (_page == 0 && _title != null)
            GameText.DrawCentered(sb, _title, w / 2f, 18, Hud.Amber * (fadeOut * MathHelper.Clamp(_pageTime, 0f, 1f)), 2);

        // Metin
        string visible = _text.Substring(0, Math.Min(_text.Length, (int)_typed));
        float y = 506;
        foreach (string line in GameText.Wrap(visible, w - 220, 2))
        {
            if (y > h - 52) break;
            GameText.Draw(sb, line, new Vector2(110, y), new Color(232, 226, 210) * fadeOut);
            y += GameText.LineHeight(2) + 6;
        }

        // Sayfa noktaları ve ipuçları
        int dotsX = w / 2 - _pages.Count * 9;
        for (int i = 0; i < _pages.Count; i++)
            sb.FillRect(new Rectangle(dotsX + i * 18, h - 30, 10, 4), (i == _page ? Hud.Amber : Hud.Label * 0.4f) * fadeOut);
        if (_typed >= _text.Length && _closing < 0f)
        {
            float pulse = 0.5f + 0.5f * MathF.Sin(_time * 4f);
            GameText.DrawRight(sb, Loc.T("cs.next"), w - 40, h - 40, Hud.Amber * (pulse * fadeOut));
        }
        GameText.Draw(sb, Loc.T("cs.skip"), new Vector2(40, h - 40), Hud.Label * (0.5f * fadeOut));
        if (_skipHeld > 0f)
            sb.FillRect(new Rectangle(40, h - 14, (int)(200 * _skipHeld / SkipHold), 3), Hud.Amber);
    }
}
