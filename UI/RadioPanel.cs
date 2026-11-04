using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TheLastDelivery.Audio;
using TheLastDelivery.Core;
using TheLastDelivery.Graphics;
using TheLastDelivery.Localization;
using TheLastDelivery.World;

namespace TheLastDelivery.UI;

public enum RadioAction { None, AnswerSos, AcceptMafiaDeal, MafiaIntel, BroadcastHeard, AnswerPhantom, PhantomDrain }

/// <summary>
/// Kabin panosundaki TELSİZ ([T]): eski bir lambalı alıcı. Açıkken oyun zamanı durur.
///   A/D ya da ←/→: frekans (basılı tutunca hızlanır)   W/S: FM/AM bandı
///   ENTER: imdat çağrısına yanıt ver / mafya teklifini kabul et   T ya da BACKSPACE: kapat
/// Yakalanan yayının dökümü daktilo gibi akar; sinyal zayıfsa harfler parazitle bozulur.
/// </summary>
public sealed class RadioPanel
{
    private static readonly Rectangle Panel = new(240, 90, 800, 470);
    private static readonly Color Bakelite = new(70, 46, 30);
    private static readonly Color Glow = new(255, 190, 90);

    private readonly Random _rng = new();
    private float _open;
    private float _time;
    private float _holdTime;
    private float _typed;           // Dökümün görünen karakter sayısı
    private RadioChannel _lastChannel;
    private int _line;              // Yayının hangi cümlesi
    private float _soundTimer;      // Mors/konuşma seslerinin aralığı
    private float _listenTime;      // Mafya frekansında geçirilen süre (istihbarat için)
    private bool _intelGiven;

    private float _broadcastTime;
    private string _broadcastHeard;  // Bu bölümde arşive kaydedilen son yayın

    public bool IsOpen { get; private set; }

    /// <summary>Şu an tutulan yayın ve sinyal gücü (ses ve akıl sağlığı için Game1 okur).</summary>
    public RadioChannel Channel { get; private set; }
    public float Signal { get; private set; }

    public void Open()
    {
        IsOpen = true;
        _open = 0f;
        _typed = 0f;
        _intelGiven = false;
    }

    public void Close() => IsOpen = false;

    /// <summary>Yeni yolculuk: istihbarat yeniden alınabilir.</summary>
    public void Reset()
    {
        IsOpen = false;
        _lastChannel = null;
        _listenTime = 0f;
    }

    public RadioAction Update(float dt, InputState input, RadioSystem radio, ProceduralAudio audio)
    {
        _time += dt;
        _open = MathF.Min(1f, _open + dt * 5f);

        if (input.AnyPressed(Keys.T, Keys.Back))
        {
            Close();
            audio.PlayClick();
            return RadioAction.None;
        }

        if (input.AnyPressed(Keys.W, Keys.Up, Keys.S, Keys.Down))
        {
            radio.SwitchBand();
            audio.PlayClick();
        }

        // Kadran: tek basış bir adım, basılı tutunca giderek hızlanır
        int direction = input.Down(Keys.A) || input.Down(Keys.Left) ? -1 : input.Down(Keys.D) || input.Down(Keys.Right) ? 1 : 0;
        if (direction != 0)
        {
            bool first = input.AnyPressed(Keys.A, Keys.Left, Keys.D, Keys.Right);
            _holdTime += dt;
            int steps = first ? 1 : _holdTime > 0.25f ? (int)MathF.Ceiling(dt * (8f + _holdTime * 25f)) : 0;
            if (steps > 0) radio.Tune(direction * steps);
        }
        else
        {
            _holdTime = 0f;
        }

        Channel = radio.Tuned(out float signal);
        Signal = signal;
        bool clear = Channel != null && Signal > 0.6f;

        // Yeni yayın yakalandı: döküm baştan başlar
        if (Channel != _lastChannel)
        {
            _lastChannel = Channel;
            _typed = 0f;
            _line = _rng.Next(3);
        }
        if (clear) _typed += dt * 28f;

        // Yayına göre ses: SOS mors bipleri, mafya kesik konuşmaları
        _soundTimer -= dt;
        if (clear && _soundTimer <= 0f)
        {
            if (Channel.Kind == RadioChannelKind.Sos) { audio.PlayMorseSos(); _soundTimer = 3.2f; }
            else if (Channel.Kind == RadioChannelKind.Mafia) { audio.PlayRadioChatter(); _soundTimer = 2.4f; }
            else if (Channel.Kind == RadioChannelKind.Broadcast) { audio.PlayRadioChatter(); _soundTimer = 3.0f; }
            else if (Channel.Kind == RadioChannelKind.Phantom) { audio.PlayEerie(); _soundTimer = 5.5f; }
        }

        // Hikâye yayını: birkaç saniye net dinlenince kayıt arşive girer (kaset)
        if (clear && Channel.Kind == RadioChannelKind.Broadcast)
        {
            _broadcastTime += dt;
            if (_broadcastTime > 4f && _broadcastHeard != Channel.Key)
            {
                _broadcastHeard = Channel.Key;
                return RadioAction.BroadcastHeard;
            }
        }
        else _broadcastTime = 0f;

        // AURA'nın ses taklidi: dinledikçe akıl sağlığı aşınır (Game1 uygular); ENTER ile sese yanıt verilir
        if (clear && Channel.Kind == RadioChannelKind.Phantom)
        {
            if (input.AnyPressed(Keys.Enter, Keys.Space)) { audio.PlayEerie(); return RadioAction.AnswerPhantom; }
            return RadioAction.PhantomDrain;
        }

        // Mafya frekansını biraz dinlemek pusu istihbaratı verir
        if (clear && Channel.Kind == RadioChannelKind.Mafia)
        {
            _listenTime += dt;
            if (_listenTime > 2.5f && !_intelGiven)
            {
                _intelGiven = true;
                radio.MafiaOfferHeard = true;
                return RadioAction.MafiaIntel;
            }
        }

        if (clear && input.AnyPressed(Keys.Enter, Keys.Space))
        {
            if (Channel.Kind == RadioChannelKind.Sos) { audio.PlayConfirm(); return RadioAction.AnswerSos; }
            if (Channel.Kind == RadioChannelKind.Mafia && radio.MafiaOfferHeard) { audio.PlayAlarm(); return RadioAction.AcceptMafiaDeal; }
        }
        return RadioAction.None;
    }

    // ------------------------------------------------------------------
    // Çizim
    // ------------------------------------------------------------------

    public void Draw(SpriteBatch sb, RadioSystem radio, bool mafiaDealMade)
    {
        float a = _open;
        sb.FillRect(new Rectangle(0, 0, GameConfig.ScreenWidth, GameConfig.ScreenHeight), Color.Black * (0.55f * a));
        var panel = Panel;
        panel.Y += (int)((1f - a) * 24f);

        // Bakalit gövde
        sb.FillRect(panel, Bakelite * a);
        sb.DrawBorder(panel, 4, new Color(40, 26, 16) * a);
        sb.FillRect(new Rectangle(panel.X + 4, panel.Y + 4, panel.Width - 8, 3), new Color(120, 84, 56) * a);
        GameText.Draw(sb, Loc.T("radio.title"), new Vector2(panel.X + 28, panel.Y + 18), Glow * a, 3);

        // Bant seçici
        string[] bands = { "FM", "AM" };
        for (int i = 0; i < 2; i++)
        {
            bool active = (int)radio.Band == i;
            var rect = new Rectangle(panel.Right - 170 + i * 74, panel.Y + 18, 64, 30);
            sb.FillRect(rect, (active ? Glow * 0.35f : Color.Black * 0.4f) * a);
            sb.DrawBorder(rect, 2, (active ? Glow : new Color(110, 80, 50)) * a);
            GameText.DrawCentered(sb, bands[i], rect.Center.X, rect.Y + 5, (active ? Color.White : Hud.Label) * a, 2, Language.English);
        }

        // Kadran camı: ölçek, yayın işaretleri, ibre
        var dial = new Rectangle(panel.X + 28, panel.Y + 70, panel.Width - 56, 110);
        sb.FillRect(dial, new Color(40, 30, 14) * a);
        sb.FillRect(new Rectangle(dial.X + 4, dial.Y + 4, dial.Width - 8, dial.Height - 8), new Color(255, 200, 120) * (0.12f * a));
        sb.DrawBorder(dial, 2, new Color(150, 110, 60) * a);
        for (int i = 0; i <= 40; i++)
        {
            int x = dial.X + 16 + (int)((dial.Width - 32) * i / 40f);
            int h = i % 5 == 0 ? 22 : 10;
            sb.FillRect(new Rectangle(x, dial.Bottom - 16 - h, 2, h), Glow * (0.6f * a));
        }
        float min = radio.Band == RadioBand.FM ? GameConfig.FmMin : GameConfig.AmMin;
        float max = radio.Band == RadioBand.FM ? GameConfig.FmMax : GameConfig.AmMax;
        for (int i = 0; i <= 4; i++)
        {
            float value = min + (max - min) * i / 4f;
            int x = dial.X + 16 + (int)((dial.Width - 32) * i / 4f);
            GameText.DrawCentered(sb, value.ToString(radio.Band == RadioBand.FM ? "0" : "0", CultureInfo.InvariantCulture),
                x, dial.Y + 10, Glow * (0.8f * a), 2, Language.English);
        }
        int needleX = dial.X + 16 + (int)((dial.Width - 32) * radio.DialPosition);
        sb.FillRect(new Rectangle(needleX - 1, dial.Y + 6, 3, dial.Height - 12), new Color(255, 70, 50) * a);
        sb.DrawGlow(new Vector2(needleX, dial.Center.Y), 30f, new Color(255, 120, 80), 0.3f * a);

        // Frekans ve sinyal gücü
        string freq = radio.Band == RadioBand.FM
            ? radio.Frequency.ToString("0.0", CultureInfo.InvariantCulture) + " MHz"
            : radio.Frequency.ToString("0", CultureInfo.InvariantCulture) + " kHz";
        GameText.Draw(sb, freq, new Vector2(panel.X + 28, panel.Y + 196), Color.White * a, 3, Language.English);
        GameText.Draw(sb, Loc.T("radio.signal"), new Vector2(panel.X + 330, panel.Y + 200), Hud.Label * a);
        for (int i = 0; i < 10; i++)
        {
            bool lit = Signal * 10f > i;
            int h = 6 + i * 2;
            sb.FillRect(new Rectangle(panel.X + 470 + i * 14, panel.Y + 222 - h, 10, h),
                (lit ? Color.Lerp(new Color(255, 120, 60), new Color(120, 230, 120), i / 9f) : new Color(30, 22, 14)) * a);
        }

        // Döküm penceresi
        var box = new Rectangle(panel.X + 28, panel.Y + 240, panel.Width - 56, 160);
        sb.FillRect(box, new Color(10, 12, 10) * a);
        sb.DrawBorder(box, 2, new Color(90, 70, 40) * a);
        string header, body;
        Color headerColor;
        if (Channel == null || Signal < 0.15f)
        {
            header = Loc.T("radio.static");
            body = Loc.T("radio.staticHint");
            headerColor = Hud.Label;
        }
        else
        {
            (header, body, headerColor) = Channel.Kind switch
            {
                RadioChannelKind.Music => (Loc.T("radio.music"), Loc.T($"radio.music.{_line}"), new Color(150, 220, 255)),
                RadioChannelKind.Mafia => (Loc.T("radio.mafia"), Loc.T(mafiaDealMade ? "radio.mafia.dealt" : $"radio.mafia.{_line}"), Hud.Danger),
                RadioChannelKind.Broadcast => (Loc.T("radio.broadcast"), Loc.T(Channel.Key), new Color(200, 230, 255)),
                RadioChannelKind.Phantom => (Loc.T("radio.phantom"), Loc.T(Channel.Key), new Color(255, 90, 100)),
                _ => (Loc.T("radio.sos"), Loc.T(radio.SosType == SosMission.MilitaryTrain ? "radio.sos.military" : "radio.sos.shelter"), new Color(255, 210, 90))
            };
        }
        GameText.Draw(sb, header, new Vector2(box.X + 14, box.Y + 10), headerColor * a);

        // Zayıf sinyalde harfler parazitle bozulur; güçlüde daktilo gibi akar
        string visible = body.Substring(0, Math.Min(body.Length, Signal > 0.6f ? (int)_typed : body.Length));
        if (Signal < 0.6f) visible = Garble(visible, 1f - Signal);
        float y = box.Y + 40;
        foreach (string line in GameText.Wrap(visible, box.Width - 28, 2))
        {
            if (y > box.Bottom - 26) break;
            GameText.Draw(sb, line, new Vector2(box.X + 14, y), Hud.Value * a);
            y += GameText.LineHeight(2) + 4;
        }

        // Eylem ipucu
        string action = null;
        if (Channel != null && Signal > 0.6f)
        {
            if (Channel.Kind == RadioChannelKind.Sos) action = Loc.T("radio.answer");
            else if (Channel.Kind == RadioChannelKind.Mafia && radio.MafiaOfferHeard && !mafiaDealMade) action = Loc.T("radio.deal");
            else if (Channel.Kind == RadioChannelKind.Music) action = Loc.T("radio.musicCalm");
            else if (Channel.Kind == RadioChannelKind.Phantom) action = Loc.T("radio.answerPhantom");
            else if (Channel.Kind == RadioChannelKind.Broadcast) action = Loc.T(_broadcastHeard == Channel.Key ? "radio.recorded" : "radio.recording");
        }
        if (action != null)
        {
            float pulse = 0.6f + 0.4f * MathF.Sin(_time * 5f);
            GameText.DrawRight(sb, action, box.Right - 12, box.Bottom - 26, Glow * (a * pulse));
        }

        GameText.Draw(sb, Loc.T("radio.hint"), new Vector2(panel.X + 28, panel.Bottom - 40), Hud.Label * (0.85f * a));
    }

    /// <summary>Parazit: harflerin bir kısmı rastgele işaretlerle değişir (çizim başına sabit kalmaması cızırtı hissi verir).</summary>
    private string Garble(string text, float amount)
    {
        var chars = text.ToCharArray();
        const string noise = ".-:'/";
        for (int i = 0; i < chars.Length; i++)
            if (chars[i] != ' ' && _rng.NextDouble() < amount * 0.8f)
                chars[i] = noise[_rng.Next(noise.Length)];
        return new string(chars);
    }
}
