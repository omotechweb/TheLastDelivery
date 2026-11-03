using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TheLastDelivery.Audio;
using TheLastDelivery.Core;
using TheLastDelivery.Graphics;
using TheLastDelivery.Localization;

namespace TheLastDelivery.UI;

/// <summary>Hack'in sonucu.</summary>
public enum HackResult { None, Success, Failed, Cancelled }

/// <summary>Sentinel trenine karşı başarılı bir hack'ten sonra seçilen eylem.</summary>
public enum HackAction { None, LockWeapons, BlindTurrets, SwitchRails }

/// <summary>
/// AURA Hack Arabirimi: telsiz frekansı AURA'nın frekansına getirilir. Osiloskopta AURA'nın kızıl dalgası ile
/// telsizin camgöbeği dalgası görünür; ←/→ frekansı, ↑/↓ genliği değiştirir. İki dalga örtüşünce kilit çubuğu dolar,
/// dolarsa hack başarılı olur. Süre biterse başarısız. Sentinel karşılaşmasında başarıdan sonra bir eylem seçilir:
/// [1] silahları kilitle, [2] taretleri kör et, [3] ray makasını değiştir (ancak silahlar susturulduktan sonra).
/// Telsizde Nova varsa tolerans genişler (korsan yardımı).
/// </summary>
public sealed class SignalHack
{
    private static readonly Rectangle Panel = new(290, 120, 700, 430);
    private static readonly Color AuraRed = new(255, 60, 70);
    private static readonly Color Cyan = new(90, 230, 255);

    private readonly Random _rng = new();
    private float _targetFreq, _targetAmp, _freq, _amp;
    private float _lock;
    private float _timeLeft;
    private float _time;
    private float _drift;          // Zor hack'lerde hedef dalga kayar
    private bool _choosing;        // Başarıdan sonra eylem seçimi
    private bool _allowSwitch;
    private float _resultTimer;
    private HackResult _result;
    private float _tolerance;

    public bool IsOpen { get; private set; }

    /// <summary>Eylem seçmeli mi (Sentinel karşılaşması) yoksa yalnızca başarılı/başarısız mı (sinyal kulesi)?</summary>
    public bool WithActions { get; private set; }

    public HackAction ChosenAction { get; private set; }

    /// <param name="difficulty">0 kolay - 1 zor: hedef dalga kayar, süre kısalır.</param>
    /// <param name="helper">Trende telsiz korsanı ya da telsizci varsa tolerans genişler.</param>
    public void Open(bool withActions, float difficulty, bool helper, bool allowSwitch)
    {
        IsOpen = true;
        WithActions = withActions;
        _allowSwitch = allowSwitch;
        _targetFreq = 1.5f + (float)_rng.NextDouble() * 3f;
        _targetAmp = 0.35f + (float)_rng.NextDouble() * 0.55f;
        _freq = _targetFreq + ((float)_rng.NextDouble() < 0.5f ? -1f : 1f) * (1.2f + (float)_rng.NextDouble());
        _freq = MathHelper.Clamp(_freq, 0.8f, 5.5f);
        _amp = MathHelper.Clamp(1.2f - _targetAmp, 0.2f, 1f);
        _lock = 0f;
        _timeLeft = GameConfig.HackTimeLimit * (1f - 0.25f * difficulty);
        _drift = difficulty;
        _choosing = false;
        _resultTimer = 0f;
        _result = HackResult.None;
        ChosenAction = HackAction.None;
        _tolerance = helper ? 1.5f : 1f;
    }

    public void Close() => IsOpen = false;

    private bool Matched =>
        MathF.Abs(_freq - _targetFreq) < GameConfig.HackFreqTolerance * _tolerance &&
        MathF.Abs(_amp - _targetAmp) < GameConfig.HackAmpTolerance * _tolerance;

    /// <returns>Hack bittiyse sonucu (sonuç penceresi kısa süre görünür, sonra kapanır).</returns>
    public HackResult Update(float dt, InputState input, ProceduralAudio audio)
    {
        if (!IsOpen) return HackResult.None;
        _time += dt;

        if (_resultTimer > 0f)
        {
            _resultTimer -= dt;
            if (_resultTimer <= 0f)
            {
                IsOpen = false;
                return _result;
            }
            return HackResult.None;
        }

        if (_choosing)
        {
            HackAction picked = HackAction.None;
            if (input.AnyPressed(Keys.D1, Keys.NumPad1)) picked = HackAction.LockWeapons;
            else if (input.AnyPressed(Keys.D2, Keys.NumPad2)) picked = HackAction.BlindTurrets;
            else if (input.AnyPressed(Keys.D3, Keys.NumPad3) && _allowSwitch) picked = HackAction.SwitchRails;
            else if (input.AnyPressed(Keys.D3, Keys.NumPad3)) audio.PlayError();
            if (picked != HackAction.None)
            {
                ChosenAction = picked;
                audio.PlayConfirm();
                Finish(HackResult.Success);
            }
            return HackResult.None;
        }

        if (input.Pressed(Keys.Escape) || input.Pressed(Keys.Back))
        {
            Finish(HackResult.Cancelled);
            return HackResult.None;
        }

        // Ok tuşları basılı tutuldukça dalga değişir
        if (input.Down(Keys.Left)) _freq -= GameConfig.HackFreqSpeed * dt;
        if (input.Down(Keys.Right)) _freq += GameConfig.HackFreqSpeed * dt;
        if (input.Down(Keys.Up)) _amp += GameConfig.HackAmpSpeed * dt;
        if (input.Down(Keys.Down)) _amp -= GameConfig.HackAmpSpeed * dt;
        _freq = MathHelper.Clamp(_freq, 0.6f, 6f);
        _amp = MathHelper.Clamp(_amp, 0.1f, 1.1f);

        // AURA karşı koyar: zor hack'lerde hedef yavaşça kayar
        _targetFreq = MathHelper.Clamp(_targetFreq + MathF.Sin(_time * 0.9f) * 0.25f * _drift * dt, 0.8f, 5.5f);

        _timeLeft -= dt;
        if (Matched)
        {
            _lock += dt / GameConfig.HackLockTime;
            if (_lock >= 1f)
            {
                audio.PlaySuccess();
                if (WithActions) _choosing = true;
                else Finish(HackResult.Success);
                return HackResult.None;
            }
        }
        else
        {
            _lock = MathF.Max(0f, _lock - dt * 0.8f);
        }

        if (_timeLeft <= 0f)
        {
            audio.PlayError();
            Finish(HackResult.Failed);
        }
        return HackResult.None;
    }

    private void Finish(HackResult result)
    {
        _result = result;
        _choosing = false;
        _resultTimer = result == HackResult.Cancelled ? 0.05f : 1.1f;
    }

    public void Draw(SpriteBatch sb)
    {
        if (!IsOpen) return;
        sb.FillRect(new Rectangle(0, 0, GameConfig.ScreenWidth, GameConfig.ScreenHeight), Color.Black * 0.55f);

        // Parazitli çerçeve
        var panel = Panel;
        if (MathF.Sin(_time * 23f) > 0.95f) panel.X += 3;
        sb.FillRect(panel, new Color(6, 8, 14) * 0.96f);
        sb.DrawBorder(panel, 2, AuraRed * 0.8f);
        for (int y = panel.Y; y < panel.Bottom; y += 4)
            sb.FillRect(new Rectangle(panel.X, y, panel.Width, 1), Color.Black * 0.25f);

        GameText.Draw(sb, Loc.T("hack.title"), new Vector2(panel.X + 24, panel.Y + 16), AuraRed, 3);
        GameText.DrawRight(sb, Loc.F("hack.time", MathF.Max(0f, _timeLeft).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)),
            panel.Right - 24, panel.Y + 22, _timeLeft < 4f ? Hud.Danger : Hud.Value);

        // Osiloskop
        var scope = new Rectangle(panel.X + 24, panel.Y + 64, panel.Width - 48, 220);
        sb.FillRect(scope, new Color(4, 14, 12));
        sb.DrawBorder(scope, 1, Cyan * 0.4f);
        for (int gx = scope.X; gx < scope.Right; gx += 40) sb.FillRect(new Rectangle(gx, scope.Y, 1, scope.Height), Cyan * 0.08f);
        for (int gy = scope.Y; gy < scope.Bottom; gy += 30) sb.FillRect(new Rectangle(scope.X, gy, scope.Width, 1), Cyan * 0.08f);
        DrawWave(sb, scope, _targetFreq, _targetAmp, AuraRed * 0.85f, 3);
        DrawWave(sb, scope, _freq, _amp, (Matched ? new Color(140, 255, 160) : Cyan), 2);

        // Değerler ve kilit çubuğu
        float y0 = scope.Bottom + 14;
        GameText.Draw(sb, Loc.F("hack.freq", _freq.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)), new Vector2(scope.X, y0), Cyan);
        GameText.Draw(sb, Loc.F("hack.amp", _amp.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)), new Vector2(scope.X + 250, y0), Cyan);
        var bar = new Rectangle(scope.X, (int)y0 + 32, scope.Width, 16);
        UiDraw.FancyBar(sb, bar, _lock, _lock, Matched ? new Color(140, 255, 160) : Cyan);
        GameText.Draw(sb, Loc.T(Matched ? "hack.locking" : "hack.search"), new Vector2(scope.X, bar.Bottom + 8), Matched ? new Color(140, 255, 160) : Hud.Label);

        if (_choosing)
        {
            var menu = new Rectangle(panel.X + 60, panel.Y + 110, panel.Width - 120, 170);
            sb.FillRect(menu, new Color(10, 4, 6) * 0.97f);
            sb.DrawBorder(menu, 2, new Color(140, 255, 160));
            GameText.DrawCentered(sb, Loc.T("hack.success"), menu.Center.X, menu.Y + 14, new Color(140, 255, 160), 2);
            GameText.Draw(sb, Loc.T("hack.opt.lock"), new Vector2(menu.X + 24, menu.Y + 56), Color.White);
            GameText.Draw(sb, Loc.T("hack.opt.blind"), new Vector2(menu.X + 24, menu.Y + 90), Color.White);
            GameText.Draw(sb, Loc.T(_allowSwitch ? "hack.opt.switch" : "hack.opt.switchLocked"), new Vector2(menu.X + 24, menu.Y + 124),
                _allowSwitch ? Color.White : Hud.Label * 0.6f);
        }
        else if (_resultTimer > 0f)
        {
            string text = Loc.T(_result switch
            {
                HackResult.Success => "hack.done",
                HackResult.Failed => "hack.failed",
                _ => "hack.cancel"
            });
            GameText.DrawCentered(sb, text, panel.Center.X, panel.Y + 170, _result == HackResult.Success ? new Color(140, 255, 160) : Hud.Danger, 4);
        }

        GameText.Draw(sb, Loc.T("hack.hint"), new Vector2(panel.X + 24, panel.Bottom - 30), Hud.Label * 0.8f);
    }

    /// <summary>Sinüs dalgasını küçük bloklarla çizer; dalga zamanla akar.</summary>
    private void DrawWave(SpriteBatch sb, Rectangle area, float freq, float amp, Color color, int size)
    {
        float mid = area.Center.Y;
        for (int x = 0; x < area.Width; x += 3)
        {
            float phase = x / (float)area.Width * MathHelper.TwoPi * freq - _time * 3f;
            float y = mid - MathF.Sin(phase) * amp * (area.Height / 2f - 8);
            sb.FillRect(new Rectangle(area.X + x, (int)y, size, size), color);
        }
    }
}
