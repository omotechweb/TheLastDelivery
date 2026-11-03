using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheLastDelivery.Audio;
using TheLastDelivery.Core;
using TheLastDelivery.Graphics;
using TheLastDelivery.Localization;
using TheLastDelivery.UI;
using TheLastDelivery.World;

namespace TheLastDelivery.Chase;

/// <summary>
/// Episode 2: AURA'nın Sentinel treni. Makinisti olmayan, siyah zırhlı, lazer ve füze taretli bir tren paralel rayda
/// Old Smokie'nin yanına sokulur. İki taret sırayla şarj olur (hedef çizgisi kızarır) ve ateş eder.
/// [H] ile AURA'nın frekansı hacklenir: silahları kilitle, taretleri kör et ya da (silahlar susunca) ray makasını
/// değiştirip Sentinel'i başka hatta sapla. Süre dolana kadar dayanılırsa Sentinel geride kalır.
/// </summary>
public sealed class SentinelEncounter
{
    private const int RailY = GameConfig.RailY - 30;     // Arka plandaki paralel ray (mafya trenininkiyle aynı)
    private const float IntroDuration = 2.4f;
    private const float OutroDuration = 2.6f;

    private readonly Random _rng = new();
    private readonly ParticleSystem _effects = new();
    private float _time;
    private float _intro;
    private float _outro;
    private float _offset;          // Sentinel'in ekrandaki konumu: 0 yan yana, 1 ekran dışı (sol)
    private float _lockTimer;       // Silahlar kilitli
    private bool _blind;            // Taretler kör
    private bool _diverted;         // Makas değişti: Sentinel başka hatta sapıyor
    private int _activeTurret;
    private float _charge;          // 0-1: taretin şarjı
    private float _shotFlash;
    private float _shake;
    private string _feedback;
    private float _feedbackTimer;

    public ChaseOutcome Outcome { get; private set; }
    public string ResultMessage { get; private set; }
    public bool IsFinished => Outcome != ChaseOutcome.None && _outro <= 0f;
    public bool InIntro => _intro > 0f;
    public float AttackMultiplier { get; set; } = 1f;
    public Vector2 ShakeOffset { get; private set; }

    /// <summary>Makas değiştirme ancak silahlar susturulduktan sonra (kilit ya da körlük) mümkündür.</summary>
    public bool CanSwitchRails => _lockTimer > 0f || _blind;

    public void Start()
    {
        _time = 0f;
        _intro = IntroDuration;
        _outro = 0f;
        _offset = 1.2f;
        _lockTimer = 0f;
        _blind = false;
        _diverted = false;
        _activeTurret = 0;
        _charge = 0f;
        _shotFlash = 0f;
        _shake = 14f;
        _feedback = null;
        Outcome = ChaseOutcome.None;
        ResultMessage = null;
        _effects.Clear();
    }

    /// <summary>Başarılı hack'in seçilen eylemi.</summary>
    public void ApplyHack(HackAction action, ProceduralAudio audio)
    {
        switch (action)
        {
            case HackAction.LockWeapons:
                _lockTimer = GameConfig.SentinelLockDuration;
                _charge = 0f;
                Feedback(Loc.T("sentinel.locked"));
                break;
            case HackAction.BlindTurrets:
                _blind = true;
                Feedback(Loc.T("sentinel.blind"));
                break;
            case HackAction.SwitchRails:
                _diverted = true;
                Feedback(Loc.T("sentinel.switch"));
                audio.PlayWhistle();
                break;
        }
    }

    private void Feedback(string text)
    {
        _feedback = text;
        _feedbackTimer = 2.2f;
    }

    public void Update(float dt, Train train, ProceduralAudio audio)
    {
        _effects.Update(dt, -60f);
        _shake = MathF.Max(0f, _shake - dt * 20f);
        ShakeOffset = _shake > 0f ? new Vector2((float)(_rng.NextDouble() - 0.5) * _shake, (float)(_rng.NextDouble() - 0.5) * _shake) : Vector2.Zero;
        _shotFlash = MathF.Max(0f, _shotFlash - dt * 3f);
        _feedbackTimer -= dt;

        if (Outcome != ChaseOutcome.None)
        {
            _outro -= dt;
            _offset = MathF.Min(1.4f, _offset + dt * 0.6f); // Sentinel geride kalır ya da sapar
            return;
        }

        if (_intro > 0f)
        {
            _intro -= dt;
            _offset = MathHelper.Lerp(0f, 1.2f, MathHelper.Clamp(_intro / IntroDuration, 0f, 1f));
            return;
        }

        _time += dt;
        _offset = MathF.Max(0f, _offset - dt);
        _lockTimer = MathF.Max(0f, _lockTimer - dt);

        if (_diverted)
        {
            Escape(audio, diverted: true);
            return;
        }

        // Taret şarjı ve atış (kilitliyken susar)
        if (_lockTimer <= 0f)
        {
            _charge += dt / GameConfig.SentinelChargeTime;
            if (_charge >= 1f)
            {
                _charge = 0f;
                Fire(train, audio);
                _activeTurret = 1 - _activeTurret;
            }
        }

        // Hızlı giden tren Sentinel'in nişanını zorlaştırır: süre daha çabuk dolar
        if (_time >= GameConfig.SentinelDuration * (1f - 0.2f * train.SpeedRatio))
            Escape(audio, diverted: false);
    }

    private void Fire(Train train, ProceduralAudio audio)
    {
        bool miss = _blind && _rng.NextDouble() < GameConfig.SentinelBlindMiss;
        var muzzle = TurretPosition(_activeTurret);
        var target = new Vector2(World.Train.FrontX - 200 + _rng.Next(-120, 121), GameConfig.RailY - 60);
        if (miss) target += new Vector2(0, -160);
        _shotFlash = 1f;
        audio.PlayExplosion();
        for (int i = 0; i < 26; i++)
        {
            float t = i / 26f;
            _effects.Emit(new Particle
            {
                Position = Vector2.Lerp(muzzle, target, t),
                Velocity = new Vector2(_rng.Next(-30, 31), _rng.Next(-30, 31)),
                Life = 0.25f + t * 0.2f,
                Size = 2f,
                Color = new Color(255, 60, 70),
                Alpha = 1f,
                Additive = true
            });
        }
        if (miss)
        {
            Feedback(Loc.T("sentinel.miss"));
            return;
        }
        train.Damage(GameConfig.SentinelShotDamage * AttackMultiplier);
        _shake = 12f;
        for (int i = 0; i < 18; i++)
        {
            _effects.Emit(new Particle
            {
                Position = target,
                Velocity = new Vector2(_rng.Next(-200, 201), -_rng.Next(40, 260)),
                Life = 0.4f + (float)_rng.NextDouble() * 0.3f,
                Size = 1.5f,
                Color = new Color(255, 180, 90),
                Alpha = 1f,
                Gravity = 500f,
                Additive = true
            });
        }
        Feedback(Loc.F("sentinel.hit", (int)(GameConfig.SentinelShotDamage * AttackMultiplier * train.ArmorMultiplier)));
    }

    private void Escape(ProceduralAudio audio, bool diverted)
    {
        audio.PlaySuccess();
        Outcome = ChaseOutcome.Escaped;
        ResultMessage = Loc.F(diverted ? "sentinel.diverted" : "sentinel.escaped", GameConfig.SentinelEscapeCredits);
        _outro = OutroDuration;
    }

    private float FrontX => World.Train.FrontX + 40f - _offset * 1400f;

    private Vector2 TurretPosition(int i) => new(FrontX - (i == 0 ? 170 : 430), RailY - 92);

    // ------------------------------------------------------------------
    // Çizim
    // ------------------------------------------------------------------

    /// <summary>Paralel ray ve Sentinel treni (bizim trenin arkasında).</summary>
    public void DrawBackground(SpriteBatch sb, float time)
    {
        sb.FillRect(new Rectangle(0, RailY, GameConfig.ScreenWidth, 3), new Color(60, 62, 74));
        for (int x = (int)(-time * 300f % 24f); x < GameConfig.ScreenWidth; x += 24)
            sb.FillRect(new Rectangle(x, RailY + 3, 10, 4), new Color(26, 24, 28));

        int front = (int)FrontX;
        if (front < -60) return;
        int rail = RailY + (_diverted ? (int)(MathF.Min(1f, _outro <= 0 ? 1f : 1f - _outro / OutroDuration) * 40f) : 0);
        void Box(int x, int y, int w, int h, Color c) => sb.FillRect(new Rectangle(front + x, rail + y, w, h), c);

        var black = new Color(12, 12, 16);
        var plate = new Color(26, 26, 32);
        var red = new Color(255, 40, 50);
        float pulse = 0.6f + 0.4f * MathF.Sin(time * 6f);

        // İki zırhlı vagon ve taretleri
        for (int car = 0; car < 2; car++)
        {
            int x0 = -560 + car * 260;
            Box(x0, -60, 230, 46, black);
            Box(x0, -34, 230, 3, red * 0.8f);
            for (int k = 0; k < 6; k++) Box(x0 + 10 + k * 36, -54, 20, 4, plate);
            int turret = car == 0 ? 1 : 0; // Arka vagon taret 1, ön vagon taret 0
            bool charging = _lockTimer <= 0f && _activeTurret == turret && Outcome == ChaseOutcome.None && _intro <= 0f;
            Box(x0 + 80, -84, 70, 24, plate);
            Box(x0 + 146, -78, 50, 8, new Color(50, 50, 58));                                   // Lazer namlusu
            Box(x0 + 92, -100, 16, 16, new Color(40, 40, 48));                                 // Füze yuvası
            var eye = _lockTimer > 0f ? new Color(80, 80, 90) : _blind ? new Color(120, 40, 160) : red * pulse;
            Box(x0 + 104, -78, 14, 8, eye);
            if (charging)
            {
                // Hedef çizgisi: şarj arttıkça kalınlaşır ve kızarır
                var muzzle = TurretPosition(turret);
                var target = new Vector2(World.Train.FrontX - 200, GameConfig.RailY - 60);
                int dots = 30;
                for (int d = 0; d < dots; d++)
                {
                    var p = Vector2.Lerp(muzzle, target, d / (float)dots);
                    sb.FillRect(new Rectangle((int)p.X, (int)p.Y, 2 + (int)(_charge * 3), 2 + (int)(_charge * 3)), red * (0.2f + 0.7f * _charge));
                }
                sb.DrawGlow(muzzle, 30f + _charge * 40f, red, 0.3f + 0.4f * _charge);
            }
            if (_lockTimer > 0f) // Kilitli taretlerde dönen kilit simgesi
            {
                Box(x0 + 98, -112, 26, 4, new Color(140, 255, 160));
                Box(x0 + 98, -112, 4, 14, new Color(140, 255, 160));
                Box(x0 + 120, -112, 4, 14, new Color(140, 255, 160));
            }
        }

        // Makinistsiz lokomotif: kabin yerine kızıl sensör
        Box(-290, -70, 290, 56, black);
        Box(-290, -110, 80, 42, plate);
        Box(-276, -98, 52, 10, red * pulse);                              // Kabin penceresi yerine sensör yarığı
        Box(-150, -96, 26, 30, plate);                                    // Soğutma kulesi (baca yok)
        for (int i = 0; i < 3; i++) Box(-146 + i * 8, -104, 4, 10, red * 0.6f);
        Box(-290, -30, 290, 4, red * 0.8f);
        for (int i = 0; i < 6; i++) Box(-20 + i * 5, -60 + i * 8, 22 - i * 3, 6, new Color(90, 90, 100)); // Kama burun
        Box(-14, -54, 10, 8, red);
        sb.DrawGlow(new Vector2(front - 10, rail - 50), 70f, red, 0.5f * pulse);
        if (MathF.Sin(time * 17f) > 0.9f) // Dijital parazit
            for (int g = 0; g < 5; g++)
                Box(-560 + _rng.Next(840), -100 + _rng.Next(90), 30, 3, red * 0.5f);

        foreach (int wx in new[] { -530, -400, -270, -140, -60 })
            sb.Draw(Assets.WheelSmall, new Vector2(front + wx, rail - 10), null, new Color(70, 70, 80), time * 12f,
                new Vector2(Assets.WheelSmall.Width / 2f, Assets.WheelSmall.Height / 2f), 0.8f, SpriteEffects.None, 0f);
    }

    public void DrawWorld(SpriteBatch sb) => _effects.Draw(sb);

    public void DrawInterface(SpriteBatch sb, float time, bool hackOpen)
    {
        var screen = new Rectangle(0, 0, GameConfig.ScreenWidth, GameConfig.ScreenHeight);
        if (_shotFlash > 0f) sb.FillRect(screen, new Color(255, 40, 50) * (0.18f * _shotFlash));

        if (InIntro)
        {
            sb.Draw(Assets.Vignette, screen, new Color(255, 30, 40) * (0.5f + 0.3f * MathF.Sin(time * 14f)));
            float pulse = 0.7f + 0.3f * MathF.Sin(time * 12f);
            GameText.DrawCentered(sb, Loc.T("sentinel.alert"), GameConfig.ScreenWidth / 2f + 3, 263, Color.Black * 0.7f, 4);
            GameText.DrawCentered(sb, Loc.T("sentinel.alert"), GameConfig.ScreenWidth / 2f, 260, new Color(255, 60, 70) * pulse, 4);
            GameText.DrawCentered(sb, Loc.T("sentinel.sub"), GameConfig.ScreenWidth / 2f, 320, Hud.Label, 2);
            return;
        }

        // Üst panel: Sentinel durumu
        var panel = new Rectangle(436, 16, 408, 100);
        sb.FillRect(panel, new Color(14, 4, 8) * 0.88f);
        sb.DrawBorder(panel, 2, new Color(255, 50, 60) * 0.8f);
        GameText.Draw(sb, Loc.T("sentinel.title"), new Vector2(panel.X + 16, panel.Y + 10), new Color(255, 80, 90));
        float secondsLeft = MathF.Max(0f, GameConfig.SentinelDuration - _time);
        GameText.DrawRight(sb, Loc.F("chase.time", (int)MathF.Ceiling(secondsLeft)), panel.Right - 16, panel.Y + 10, Hud.Value);
        var bar = new Rectangle(panel.X + 16, panel.Y + 42, panel.Width - 32, 14);
        UiDraw.FancyBar(sb, bar, _lockTimer > 0f ? _lockTimer / GameConfig.SentinelLockDuration : _charge,
            _charge, _lockTimer > 0f ? new Color(140, 255, 160) : new Color(255, 60, 70));
        string state = _lockTimer > 0f ? Loc.F("sentinel.state.locked", (int)MathF.Ceiling(_lockTimer))
            : _blind ? Loc.T("sentinel.state.blind") : Loc.T("sentinel.state.charging");
        GameText.Draw(sb, state, new Vector2(panel.X + 16, panel.Y + 66), Hud.Label);

        if (_feedback != null && _feedbackTimer > 0f)
            GameText.DrawCentered(sb, _feedback, GameConfig.ScreenWidth / 2f, 395,
                new Color(255, 200, 200) * MathHelper.Clamp(_feedbackTimer / 0.4f, 0f, 1f), 2);

        if (Outcome != ChaseOutcome.None && ResultMessage != null)
        {
            GameText.DrawCentered(sb, ResultMessage, GameConfig.ScreenWidth / 2f + 3, 293, Color.Black * 0.7f, 3);
            GameText.DrawCentered(sb, ResultMessage, GameConfig.ScreenWidth / 2f, 290, CabinPanel.Good, 3);
        }
        else if (!hackOpen)
        {
            float pulse = 0.6f + 0.4f * MathF.Sin(time * 6f);
            GameText.Draw(sb, Loc.T("sentinel.instr"), new Vector2(24, GameConfig.ScreenHeight - 32), new Color(255, 120, 130) * pulse);
        }
    }
}
