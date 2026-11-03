using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TheLastDelivery.Audio;
using TheLastDelivery.Core;
using TheLastDelivery.Graphics;
using TheLastDelivery.Localization;

namespace TheLastDelivery.UI;

/// <summary>DLC kilit ekranında seçilen eylem.</summary>
public enum DlcGateAction { None, OpenStore, MainMenu, Continue }

/// <summary>
/// Episode 2 Bölüm 2'nin sonunda (ya da kilitli bir Episode 2 bölümü seçilince) açılan sinematik
/// "EPISODE 2: PROJECT AURA DLC'SİNİ AÇ" paneli: hareketli parazit, AURA'nın gözü, içerik tanıtımı ve düğmeler.
/// DLC sahipliği doğrulanınca (mağazadan dönüş) "Devam Et" düğmesi belirir.
/// </summary>
public sealed class DlcGatePanel
{
    private static readonly Color AuraRed = new(255, 50, 60);
    private float _time;
    private int _cursor;
    private string _message;
    private float _messageTimer;

    public void Open()
    {
        _time = 0f;
        _cursor = 0;
        _message = null;
    }

    public void ShowMessage(string text)
    {
        _message = text;
        _messageTimer = 3f;
    }

    private static int ButtonCount => Platform.DLCManager.DLC_EPISODE2_UNLOCKED ? 3 : 2;

    public DlcGateAction Update(float dt, InputState input, ProceduralAudio audio)
    {
        _time += dt;
        _messageTimer -= dt;
        if (_time < 1.2f) return DlcGateAction.None; // Sinematik giriş bitmeden seçim yok

        int count = ButtonCount;
        if (input.AnyPressed(Keys.W, Keys.Up)) { _cursor = (_cursor - 1 + count) % count; audio.PlayClick(); }
        if (input.AnyPressed(Keys.S, Keys.Down)) { _cursor = (_cursor + 1) % count; audio.PlayClick(); }
        if (input.AnyPressed(Keys.D1, Keys.NumPad1)) _cursor = 0;
        else if (input.AnyPressed(Keys.D2, Keys.NumPad2)) _cursor = 1;
        else if (input.AnyPressed(Keys.D3, Keys.NumPad3) && count == 3) _cursor = 2;
        else if (!input.AnyPressed(Keys.Enter, Keys.Space)) return DlcGateAction.None;

        audio.PlayConfirm();
        return _cursor switch
        {
            0 => DlcGateAction.OpenStore,
            1 => DlcGateAction.MainMenu,
            _ => DlcGateAction.Continue
        };
    }

    public void Draw(SpriteBatch sb)
    {
        float t = _time;
        var screen = new Rectangle(0, 0, GameConfig.ScreenWidth, GameConfig.ScreenHeight);
        float fade = MathHelper.Clamp(t / 0.8f, 0f, 1f);
        sb.FillRect(screen, new Color(4, 2, 6) * (0.92f * fade));

        // Parazit çizgileri ve kayan kırmızı tarama
        for (int i = 0; i < 26; i++)
        {
            float y = (i * 53 + t * (40 + i * 7)) % GameConfig.ScreenHeight;
            sb.FillRect(new Rectangle(0, (int)y, GameConfig.ScreenWidth, 1), AuraRed * (0.06f * fade));
        }
        if (MathF.Sin(t * 19f) > 0.93f)
            for (int g = 0; g < 8; g++)
                sb.FillRect(new Rectangle((g * 173 + (int)(t * 300)) % 1280, (g * 97) % 720, 120, 3), Color.White * 0.25f);

        // AURA'nın gözü (solda) ve yörüngedeki uydular
        var eye = new Vector2(260, 330);
        float pulse = 0.6f + 0.4f * MathF.Sin(t * 2.2f);
        sb.DrawGlow(eye, 220f, AuraRed, 0.25f * fade * pulse);
        sb.Draw(Assets.Circle, new Rectangle((int)eye.X - 90, (int)eye.Y - 90, 180, 180), new Color(40, 6, 10) * fade);
        sb.Draw(Assets.Circle, new Rectangle((int)eye.X - 56, (int)eye.Y - 56, 112, 112), new Color(120, 14, 22) * fade);
        sb.Draw(Assets.Circle, new Rectangle((int)eye.X - 26, (int)eye.Y - 26, 52, 52), AuraRed * (fade * pulse));
        sb.Draw(Assets.Circle, new Rectangle((int)eye.X - 8, (int)eye.Y - 8, 16, 16), Color.White * (fade * pulse));
        for (int s = 0; s < 6; s++)
        {
            float a = t * 0.6f + s * MathHelper.TwoPi / 6f;
            var p = eye + new Vector2(MathF.Cos(a) * 150f, MathF.Sin(a) * 60f);
            sb.FillRect(new Rectangle((int)p.X - 6, (int)p.Y - 3, 12, 6), new Color(180, 186, 200) * fade);
            sb.FillRect(new Rectangle((int)p.X - 14, (int)p.Y - 1, 28, 2), new Color(90, 120, 180) * fade);
        }

        // Başlık ve içerik
        float textIn = MathHelper.Clamp((t - 0.4f) / 0.8f, 0f, 1f);
        int left = 500;
        GameText.Draw(sb, Loc.T("gate.episode"), new Vector2(left, 90), AuraRed * textIn, 2);
        GameText.Draw(sb, "PROJECT AURA", new Vector2(left + 3, 123), Color.Black * (0.6f * textIn), 6, Language.English);
        GameText.Draw(sb, "PROJECT AURA", new Vector2(left, 120), new Color(255, 230, 230) * textIn, 6, Language.English);
        GameText.Draw(sb, Hud.FitWidth(Loc.T("gate.title"), 740), new Vector2(left, 190), Hud.Amber * textIn, 2);

        string[] features = { "gate.f1", "gate.f2", "gate.f3", "gate.f4" };
        for (int i = 0; i < features.Length; i++)
        {
            float rowIn = MathHelper.Clamp((t - 0.7f - i * 0.18f) / 0.4f, 0f, 1f);
            int y = 240 + i * 46;
            sb.FillRect(new Rectangle(left, y + 6, 10, 10), AuraRed * rowIn);
            GameText.Draw(sb, Hud.FitWidth(Loc.T(features[i]), 720), new Vector2(left + 24 - (1f - rowIn) * 20f, y), Color.White * rowIn, 2);
        }

        // Merak uyandıran son söz: AURA, Arthur'a kutuptan sesleniyor (titreyerek belirir)
        float teaserIn = MathHelper.Clamp((t - 1.6f) / 0.6f, 0f, 1f);
        float jitter = MathF.Sin(t * 37f) > 0.92f ? 2f : 0f;
        GameText.Draw(sb, Hud.FitWidth(Loc.T("gate.teaser"), 740), new Vector2(left + jitter, 410), AuraRed * teaserIn, 2);

        // Düğmeler
        string[] buttons = { "gate.store", "gate.menu", "gate.continue" };
        for (int i = 0; i < ButtonCount; i++)
        {
            bool selected = i == _cursor;
            var rect = new Rectangle(left, 450 + i * 52, 460, 42);
            if (selected)
            {
                UiDraw.FancyPanel(sb, rect, (i == 2 ? new Color(140, 255, 160) : AuraRed) * textIn);
                sb.FillRect(rect, AuraRed * (0.12f * textIn));
            }
            else sb.DrawBorder(rect, 1, Hud.PanelEdge * textIn);
            GameText.Draw(sb, $"{i + 1}. {Loc.T(buttons[i])}", new Vector2(rect.X + 16, rect.Y + 10), (selected ? Color.White : Hud.Label) * textIn, 2);
        }

        if (_message != null && _messageTimer > 0f)
            GameText.Draw(sb, _message, new Vector2(left, 620), Hud.Amber * MathHelper.Clamp(_messageTimer, 0f, 1f));
        GameText.Draw(sb, Loc.T("gate.hint"), new Vector2(left, 660), Hud.Label * (0.7f * textIn));
    }
}

/// <summary>
/// Episode 1'in kapanış sinematiği: çekirdek yerine ulaşır, atmosfer temizlenir ve yirmi yıl sonra ilk kez güneş doğar.
/// Sonra telsize buzun altındaki askeri sığınaklardan parazitli bir yayın düşer: donma iklimsel değil, AURA uydularının
/// işidir ve ikinci dalga (Faz-2) hazırlanmaktadır. [BOŞLUK] ile hızlandırılır, sonunda ana menüye dönülür.
/// </summary>
public sealed class EpisodeOutro
{
    private const float Total = 17f;
    private float _time;

    public bool IsFinished { get; private set; }

    public void Start()
    {
        _time = 0f;
        IsFinished = false;
    }

    public void Update(float dt, InputState input, ProceduralAudio audio)
    {
        _time += dt;
        if (!input.Pressed(Keys.Space) && !input.Pressed(Keys.Enter)) return;
        audio.PlayClick();
        if (_time < Total) _time = _time < 6f ? 6f : _time < 11f ? 11f : Total; // Sahne sahne ilerle
        else IsFinished = true;
    }

    public void Draw(SpriteBatch sb)
    {
        float t = _time;
        int w = GameConfig.ScreenWidth, h = GameConfig.ScreenHeight;
        float sunrise = MathHelper.Clamp(t / 6f, 0f, 1f);

        // Gökyüzü: gece lacivertinden şafak turuncusuna
        for (int y = 0; y < h; y += 8)
        {
            float k = y / (float)h;
            var night = Color.Lerp(new Color(6, 8, 20), new Color(20, 24, 50), k);
            var dawn = Color.Lerp(new Color(70, 110, 170), new Color(255, 170, 110), k);
            sb.FillRect(new Rectangle(0, y, w, 8), Color.Lerp(night, dawn, sunrise));
        }

        // Güneş ufuktan yükselir
        float sunY = MathHelper.Lerp(560f, 330f, sunrise);
        sb.DrawGlow(new Vector2(w * 0.62f, sunY), 300f, new Color(255, 200, 120), 0.5f * sunrise);
        sb.Draw(Assets.Circle, new Rectangle((int)(w * 0.62f) - 60, (int)sunY - 60, 120, 120), new Color(255, 236, 190) * (0.3f + 0.7f * sunrise));

        // Karlı tepeler ve ufuktaki Sığınak-0
        for (int x = 0; x < w; x += 4)
        {
            float hill = 520 + MathF.Sin(x * 0.01f) * 20f + MathF.Sin(x * 0.027f) * 12f;
            sb.FillRect(new Rectangle(x, (int)hill, 4, h - (int)hill), Color.Lerp(new Color(40, 46, 70), new Color(230, 220, 215), sunrise * 0.8f));
        }
        sb.FillRect(new Rectangle(160, 470, 120, 60), new Color(50, 54, 70) * 0.9f);
        sb.FillRect(new Rectangle(204, 488, 32, 42), new Color(255, 210, 140) * (0.4f + 0.6f * sunrise));

        // Metin akışı
        TextLine(sb, Loc.T("outro.l1"), 120, t, 0.5f);
        TextLine(sb, Loc.T("outro.l2"), 160, t, 2.0f);
        TextLine(sb, Loc.T("outro.l3"), 200, t, 3.6f);

        // Telsiz paraziti: sahne kararır, yayın düşer
        if (t > 6.5f)
        {
            float glitch = MathHelper.Clamp((t - 6.5f) / 2f, 0f, 1f);
            sb.FillRect(new Rectangle(0, 0, w, h), new Color(6, 0, 4) * (0.55f * glitch));
            for (int i = 0; i < 18; i++)
            {
                float y = (i * 47 + t * 120f) % h;
                sb.FillRect(new Rectangle(0, (int)y, w, 2), new Color(255, 50, 60) * (0.15f * glitch));
            }
            var box = new Rectangle(220, 300, 840, 200);
            sb.FillRect(box, new Color(10, 4, 8) * (0.92f * glitch));
            sb.DrawBorder(box, 2, new Color(255, 50, 60) * glitch);
            GameText.Draw(sb, Loc.T("outro.radio"), new Vector2(box.X + 20, box.Y + 14), new Color(255, 90, 100) * glitch, 2);
            int chars = (int)MathHelper.Clamp((t - 7.5f) * 45f, 0f, 9999f);
            string body = Loc.T("outro.broadcast");
            if (chars < body.Length) body = body[..chars];
            float y0 = box.Y + 52;
            foreach (string line in GameText.Wrap(body, box.Width - 40, 2))
            {
                GameText.Draw(sb, line, new Vector2(box.X + 20, y0), Color.White * glitch, 2);
                y0 += GameText.LineHeight(2) + 4;
            }
        }

        // Faz-2 uyarısı ve Episode 2 başlığı
        if (t > 12f)
        {
            float a = MathHelper.Clamp((t - 12f) / 1.5f, 0f, 1f);
            float pulse = 0.6f + 0.4f * MathF.Sin(t * 5f);
            GameText.DrawCentered(sb, Loc.T("outro.phase2"), w / 2f, 540, new Color(255, 60, 70) * (a * pulse), 3);
            GameText.DrawCentered(sb, Loc.T("outro.next"), w / 2f, 600, new Color(255, 230, 230) * a, 2);
        }
        if (t >= Total)
            GameText.DrawCentered(sb, Loc.T("prompt.continue"), w / 2f, 660, Hud.Value * (0.6f + 0.4f * MathF.Sin(t * 4f)), 2);
    }

    private static void TextLine(SpriteBatch sb, string text, int y, float t, float start)
    {
        float a = MathHelper.Clamp((t - start) / 1f, 0f, 1f) * MathHelper.Clamp((7.5f - t) / 1f, 0f, 1f);
        if (a <= 0f) return;
        GameText.DrawCentered(sb, text, GameConfig.ScreenWidth / 2f + 2, y + 2, Color.Black * (0.5f * a), 2);
        GameText.DrawCentered(sb, text, GameConfig.ScreenWidth / 2f, y, Color.White * a, 2);
    }
}
