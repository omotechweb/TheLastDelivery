using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TheLastDelivery.Audio;
using TheLastDelivery.Core;
using TheLastDelivery.Graphics;
using TheLastDelivery.Items;
using TheLastDelivery.Localization;
using TheLastDelivery.UI;
using TheLastDelivery.World;

namespace TheLastDelivery.Chase;

public enum ChaseOutcome { None, Escaped, Caught }

public enum ComboKey { Left, Right, Up }

/// <summary>
/// Mafya pususu ve kaçış kovalamacası.
///   • Arkadan zırhlı bir ray arabası ve kar kamyonu yaklaşır (mesafe 1 → 0).
///   • [W] tam gaz yaklaşmayı yavaşlatır; tam gaz kazan basıncını da biriktirir.
///   • [SHIFT] biriken basınçla buhar püskürtüp haydutları geri iter.
///   • Raylarda engel belirince ekrandaki ok tuşu kombinasyonu süresi içinde girilmelidir.
/// Haydutlar trene yetişirse yakıt ve hurda çalar; süre dolana kadar dayanılırsa kaçılır.
/// </summary>
public sealed class ChaseSequence
{
    private const float OutroDuration = 2.4f;
    private const float FeedbackDuration = 1.6f;
    private static int TrainBackX => World.Train.RearX;    // Son vagonun sol kenarı (ek vagonlar dahil)
    private static int TrainFrontX => World.Train.FrontX;  // İnek mahmuzunun ucu

    private struct Popup
    {
        public string Text;
        public Vector2 Position;
        public float Age;
        public float Life;
        public Color Color;
        public int Scale;
    }

    private readonly Random _rng = new();
    private readonly ParticleSystem _effects = new();
    private readonly ParticleSystem _backgroundSmoke = new(); // Mafya treninin bacası (bizim trenin arkasında)
    private readonly List<Popup> _popups = new();
    private readonly List<float> _shockwaves = new();        // Buhar püskürtme halkalarının yaşı
    private float _knockback;                                 // Püskürtmeyle haydutların geriye savrulma miktarı (piksel)
    private float _flash;                                     // Püskürtme anındaki beyaz ekran parlaması
    private float _mafiaSmokeTimer;

    private float _introTimer;
    private float _outroTimer;
    private float _time;
    private float _banditDistance;
    private float _pressure;
    private float _shake;
    private float _exhaustTimer;
    private float _vroomTimer;
    private float _wheelAngle;
    private bool _intel;
    private bool _guard;

    private int _nextObstacle;
    private bool _comboActive;
    private ComboKey[] _combo = Array.Empty<ComboKey>();
    private int _comboProgress;
    private float _comboTime;
    private float _comboWindow;

    private string _feedback;
    private Color _feedbackColor;
    private float _feedbackTimer;

    public ChaseOutcome Outcome { get; private set; }
    public string ResultMessage { get; private set; }

    /// <summary>Sonuç ekranda yeterince göründü, sürüşe dönülebilir.</summary>
    public bool IsFinished => Outcome != ChaseOutcome.None && _outroTimer <= 0f;

    public bool InIntro => _introTimer > 0f;

    /// <summary>Zırh kaplaması gibi geliştirmelerin hasar çarpanı (1 = tam hasar).</summary>
    public float AttackMultiplier { get; set; } = 1f;

    /// <summary>Dünya çiziminin kaydırılacağı sarsıntı miktarı.</summary>
    public Vector2 ShakeOffset { get; private set; }

    public void Start(bool intel, bool guard)
    {
        _intel = intel;
        _guard = guard;
        _introTimer = GameConfig.ChaseIntroDuration;
        _outroTimer = 0f;
        _time = 0f;
        _banditDistance = 1.25f; // Ekranın solundan girerler
        _pressure = 0.2f;
        _shake = 18f;            // Pusu anı: belirgin sarsıntı
        _knockback = 0f;
        _flash = 0f;
        _shockwaves.Clear();
        _backgroundSmoke.Clear();
        _nextObstacle = 0;
        _comboActive = false;
        _feedback = null;
        Outcome = ChaseOutcome.None;
        ResultMessage = null;
        _effects.Clear();
        _popups.Clear();
    }

    // ------------------------------------------------------------------
    // Güncelleme
    // ------------------------------------------------------------------

    public void Update(float dt, InputState input, Train train, Inventory inventory, ProceduralAudio audio)
    {
        UpdateEffects(dt, train);

        if (Outcome != ChaseOutcome.None)
        {
            _outroTimer -= dt;
            // Kaçıldıysa haydutlar geride kalır; yakalandıysa yağmayı bitirip uzaklaşır
            _banditDistance = MathF.Min(1.4f, _banditDistance + dt * 0.5f);
            return;
        }

        float startDistance = _intel ? GameConfig.IntelStartDistance : GameConfig.BanditStartDistance;
        if (_introTimer > 0f)
        {
            _introTimer -= dt;
            _banditDistance = MathHelper.Lerp(startDistance, 1.25f, MathHelper.Clamp(_introTimer / GameConfig.ChaseIntroDuration, 0f, 1f));
            return;
        }

        _time += dt;

        // Tam gaz kazan basıncını biriktirir
        _pressure = MathF.Min(1f, _pressure + GameConfig.PressurePerSecond * train.SpeedRatio * dt);
        if (input.AnyPressed(Keys.LeftShift, Keys.RightShift))
            TryBlast(train, audio);

        // Haydutlar yaklaşır; yavaş giden tren daha çabuk yakalanır
        float closing = (GameConfig.BanditBaseClose + GameConfig.BanditSpeedClose * (1f - train.SpeedRatio))
                        * (_guard ? GameConfig.GuardCloseMultiplier : 1f);
        _banditDistance -= closing * dt;

        UpdateObstacles(dt, input, train, audio);

        if (_banditDistance <= 0f)
            Caught(train, inventory, audio);
        else if (_time >= GameConfig.ChaseDuration && !_comboActive)
            Escaped(inventory, audio);
    }

    private void TryBlast(Train train, ProceduralAudio audio)
    {
        if (_pressure < GameConfig.BlastThreshold)
        {
            Feedback(Loc.T("chase.lowPressure"), Hud.Danger);
            audio.PlayError();
            return;
        }

        _banditDistance = MathF.Min(1f, _banditDistance + GameConfig.BlastPush);
        _pressure = 0f;
        train.DrainHeat(GameConfig.BlastHeatCost);
        train.BurstSmoke(28, 1f); // Bacadan da genişleyen yoğun duman
        _shake = 12f;
        _flash = 1f;
        _knockback = 90f;          // Haydutlar anında geriye savrulur, sonra yeni mesafelerine oturur
        _shockwaves.Add(0f);
        audio.PlaySteamBlast();
        audio.PlayExplosion();
        Feedback(Loc.T("chase.blast"), new Color(200, 230, 255));
        AddPopup("FSSSHHH!", new Vector2(TrainBackX - 30, GameConfig.RailY - 150), new Color(230, 240, 255), 5);

        var origin = new Vector2(TrainBackX, GameConfig.RailY - 50);

        // Devasa siyah duman: kazanın dibinden geriye fışkıran kurum
        for (int i = 0; i < 36; i++)
        {
            _effects.Emit(new Particle
            {
                Position = origin + new Vector2(_rng.Next(-10, 30), _rng.Next(-60, 30)),
                Velocity = new Vector2(-_rng.Next(300, 800), _rng.Next(-120, 60)),
                Life = 0.9f + (float)_rng.NextDouble() * 0.8f,
                Size = 14f + _rng.Next(12),
                Growth = 4f,
                Color = new Color(18, 18, 22),
                Alpha = 0.85f,
                Soft = true
            });
        }

        // Üstüne beyaz buhar
        for (int i = 0; i < 30; i++)
        {
            _effects.Emit(new Particle
            {
                Position = origin + new Vector2(_rng.Next(0, 40), _rng.Next(-50, 10)),
                Velocity = new Vector2(-_rng.Next(250, 650), _rng.Next(-60, 40)),
                Life = 0.6f + (float)_rng.NextDouble() * 0.6f,
                Size = 8f + _rng.Next(8),
                Growth = 3f,
                Color = new Color(225, 232, 242),
                Alpha = 0.7f,
                Soft = true
            });
        }

        // Haydutların önünde kıvılcım: darbe hissi
        Sparks(new Vector2(BanditFrontX(), GameConfig.RailY - 40), 24);
        Sparks(new Vector2(TruckFrontX(_time), 610), 16);
    }

    private void Sparks(Vector2 position, int count)
    {
        for (int i = 0; i < count; i++)
        {
            _effects.Emit(new Particle
            {
                Position = position,
                Velocity = new Vector2(-_rng.Next(100, 400), -_rng.Next(40, 240)),
                Life = 0.3f + (float)_rng.NextDouble() * 0.3f,
                Size = 1.5f,
                Color = new Color(255, 200, 110),
                Alpha = 1f,
                Gravity = 500f,
                Additive = true
            });
        }
    }

    private void UpdateObstacles(float dt, InputState input, Train train, ProceduralAudio audio)
    {
        if (!_comboActive)
        {
            if (_nextObstacle < GameConfig.ObstacleTimes.Length && _time >= GameConfig.ObstacleTimes[_nextObstacle])
                StartCombo(audio);
            return;
        }

        _comboTime += dt;

        ComboKey? pressed = input.Pressed(Keys.Left) ? ComboKey.Left
                          : input.Pressed(Keys.Right) ? ComboKey.Right
                          : input.Pressed(Keys.Up) ? ComboKey.Up
                          : null;

        if (pressed.HasValue)
        {
            if (pressed.Value == _combo[_comboProgress])
            {
                _comboProgress++;
                audio.PlayClick();
                if (_comboProgress >= _combo.Length)
                {
                    EndCombo();
                    audio.PlaySuccess();
                    Feedback(Loc.T("chase.dodged"), CabinPanel.Good);
                    AddPopup("WHOOSH!", new Vector2(TrainFrontX + 40, GameConfig.RailY - 150), new Color(170, 230, 255), 3);
                }
                return;
            }

            Crash(train, audio); // Yanlış tuş
            return;
        }

        if (_comboTime >= _comboWindow)
            Crash(train, audio); // Süre doldu
    }

    private void StartCombo(ProceduralAudio audio)
    {
        _combo = new ComboKey[GameConfig.ComboLength];
        for (int i = 0; i < _combo.Length; i++)
            _combo[i] = (ComboKey)_rng.Next(3);

        _comboProgress = 0;
        _comboTime = 0f;
        _comboWindow = GameConfig.ComboWindow + (_intel ? GameConfig.IntelExtraWindow : 0f);
        _comboActive = true;
        audio.PlayAlarm();
    }

    private void EndCombo()
    {
        _comboActive = false;
        _nextObstacle++;
    }

    private void Crash(Train train, ProceduralAudio audio)
    {
        EndCombo();
        train.Damage(GameConfig.ObstacleDamage * AttackMultiplier);
        _shake = 14f;
        audio.PlayExplosion();
        Explode(new Vector2(TrainFrontX, GameConfig.RailY - 30));
        AddPopup("BAM!", new Vector2(TrainFrontX, GameConfig.RailY - 140), new Color(255, 170, 60), 5);
        Feedback(Loc.F("chase.hit", (int)GameConfig.ObstacleDamage), Hud.Danger);
    }

    private void Caught(Train train, Inventory inventory, ProceduralAudio audio)
    {
        float theftScale = _guard ? 0.5f : 1f;
        int fuel = (int)(GameConfig.TheftFuel * theftScale);
        int stolen = inventory.Cargo.Steal((int)(GameConfig.TheftCargo * theftScale)).Count; // En değerli yağmalar gider

        train.BurnFuel(fuel);
        train.Damage(GameConfig.CaughtDamage * AttackMultiplier);

        _shake = 16f;
        audio.PlayExplosion();
        Explode(new Vector2(TrainBackX + 20, GameConfig.RailY - 50));
        AddPopup("KRAK!", new Vector2(TrainBackX, GameConfig.RailY - 150), Hud.Danger, 5);

        Outcome = ChaseOutcome.Caught;
        ResultMessage = Loc.F("chase.caught", fuel, stolen);
        _outroTimer = OutroDuration;
    }

    private void Escaped(Inventory inventory, ProceduralAudio audio)
    {
        // Kaçarken haydutların düşürdüğü yağma toplanır
        int looted = 0;
        for (int i = 0; i < GameConfig.EscapeLoot; i++)
            if (inventory.Cargo.TryAdd(Economy.LootCatalog.Roll(_rng))) looted++;

        audio.PlaySuccess();
        Outcome = ChaseOutcome.Escaped;
        ResultMessage = Loc.F("chase.escaped", looted);
        _outroTimer = OutroDuration;
    }

    private void Feedback(string text, Color color)
    {
        _feedback = text;
        _feedbackColor = color;
        _feedbackTimer = FeedbackDuration;
    }

    // ------------------------------------------------------------------
    // Efektler
    // ------------------------------------------------------------------

    private void UpdateEffects(float dt, Train train)
    {
        _shake = MathF.Max(0f, _shake - dt * 22f);
        _flash = MathF.Max(0f, _flash - dt * 4f);
        _knockback = MathF.Max(0f, _knockback - dt * 140f);
        for (int i = _shockwaves.Count - 1; i >= 0; i--)
        {
            _shockwaves[i] += dt;
            if (_shockwaves[i] > 0.5f) _shockwaves.RemoveAt(i);
        }

        // Giriş boyunca güçlü, kovalamaca boyunca hafif sürekli titreşim
        float baseShake = InIntro ? 4f : Outcome == ChaseOutcome.None ? 1.5f : 0f;

        // Arka plandaki mafya treninin bacasından siyah duman
        _mafiaSmokeTimer -= dt;
        float mafiaX = MafiaTrainFrontX();
        if (_mafiaSmokeTimer <= 0f && mafiaX > -100)
        {
            _mafiaSmokeTimer = 0.06f;
            _backgroundSmoke.Emit(new Particle
            {
                Position = new Vector2(mafiaX - 112, MafiaRailY - 96),
                Velocity = new Vector2(-train.ScrollSpeed * 0.3f - 40, -_rng.Next(40, 80)),
                Life = 1.6f,
                Size = 7f,
                Growth = 3.5f,
                Color = new Color(20, 18, 22),
                Alpha = 0.7f,
                WindInfluence = 0.6f,
                Soft = true
            });
        }
        _backgroundSmoke.Update(dt, -train.ScrollSpeed * 0.6f);
        float amount = _shake + baseShake;
        ShakeOffset = new Vector2(((float)_rng.NextDouble() * 2f - 1f) * amount, ((float)_rng.NextDouble() * 2f - 1f) * amount * 0.6f);

        _feedbackTimer -= dt;
        _wheelAngle += train.ScrollSpeed * dt / 10f;

        // Haydut egzozu ve motor kükremesini temsil eden yazılar (kar kamyonundan)
        float banditX = TruckFrontX(_time) + 60;
        if (banditX > -100)
        {
            _exhaustTimer -= dt;
            if (_exhaustTimer <= 0f)
            {
                _exhaustTimer = 0.05f;
                _effects.Emit(new Particle
                {
                    Position = new Vector2(banditX - 226, 586),
                    Velocity = new Vector2(-train.ScrollSpeed * 0.6f - 60, -_rng.Next(20, 60)),
                    Life = 0.9f,
                    Size = 6f,
                    Growth = 2.5f,
                    Color = new Color(40, 36, 40),
                    Alpha = 0.6f,
                    WindInfluence = 0.3f,
                    Soft = true
                });
            }

            _vroomTimer -= dt;
            if (_vroomTimer <= 0f && Outcome == ChaseOutcome.None)
            {
                _vroomTimer = 1.3f + (float)_rng.NextDouble();
                AddPopup(_rng.Next(2) == 0 ? "VRRRM!" : "BRRRAP!", new Vector2(banditX - 120, 540),
                    new Color(255, 120, 90), 3);
            }
        }

        _effects.Update(dt, -train.ScrollSpeed * 0.8f);

        for (int i = _popups.Count - 1; i >= 0; i--)
        {
            var popup = _popups[i];
            popup.Age += dt;
            popup.Position.Y -= 30f * dt;
            if (popup.Age >= popup.Life) _popups.RemoveAt(i);
            else _popups[i] = popup;
        }
    }

    private void Explode(Vector2 position)
    {
        for (int i = 0; i < 30; i++)
        {
            float angle = (float)_rng.NextDouble() * MathHelper.TwoPi;
            float speed = 80f + (float)_rng.NextDouble() * 260f;
            _effects.Emit(new Particle
            {
                Position = position,
                Velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * speed,
                Life = 0.4f + (float)_rng.NextDouble() * 0.4f,
                Size = 3f + _rng.Next(5),
                Growth = 1.5f,
                Color = i % 3 == 0 ? new Color(255, 230, 140) : new Color(255, 120, 40),
                Alpha = 1f,
                Gravity = 200f,
                Soft = true,
                Additive = true
            });
        }
        for (int i = 0; i < 12; i++)
        {
            _effects.Emit(new Particle
            {
                Position = position,
                Velocity = new Vector2(_rng.Next(-80, 81), -_rng.Next(40, 140)),
                Life = 1.4f,
                Size = 10f,
                Growth = 2.5f,
                Color = new Color(50, 46, 48),
                Alpha = 0.6f,
                WindInfluence = 0.5f,
                Soft = true
            });
        }
    }

    private void AddPopup(string text, Vector2 position, Color color, int scale) =>
        _popups.Add(new Popup { Text = text, Position = position, Life = 0.9f, Color = color, Scale = scale });

    /// <summary>
    /// Haydut ray arabasının ön (sağ) kenarı: mesafe 0'da vagona dayanır, 1'de ekranın sol kenarındadır.
    /// Mesafe 1'in üstü (giriş/çıkış) arabayı ekran dışına iter.
    /// </summary>
    private float BanditFrontX() =>
        MathHelper.Lerp(TrainBackX - 4, 30f, MathHelper.Clamp(_banditDistance, 0f, 1.6f)) - _knockback;

    /// <summary>Ön plandaki kar kamyonu: karda daha serbest olduğu için trenin yanına kadar sokulur.</summary>
    private float TruckFrontX(float time) =>
        MathHelper.Lerp(860f, 200f, MathHelper.Clamp(_banditDistance, 0f, 1.6f)) + MathF.Sin(time * 2.3f) * 24f - _knockback;

    /// <summary>Arka paralel raydaki zırhlı mafya treni: yaklaştıkça burnu bizim lokomotifin hizasına gelir.</summary>
    private float MafiaTrainFrontX() =>
        MathHelper.Lerp(TrainFrontX - 30, 60f, MathHelper.Clamp(_banditDistance, 0f, 1.6f)) - _knockback * 0.7f;

    private const int MafiaRailY = GameConfig.RailY - 30; // Arka plandaki paralel ray

    // ------------------------------------------------------------------
    // Çizim — dünya (sarsıntı dönüşümü altında)
    // ------------------------------------------------------------------

    /// <summary>
    /// Arka plan katmanı (bizim trenin arkasında çizilir): paralel ray ve siyah-kızıl zırhlı mafya treni.
    /// Yanımıza sokulduğunda bizim tren onu kısmen örter; derinlik hissi verir.
    /// </summary>
    public void DrawBackground(SpriteBatch sb, float time)
    {
        // Paralel ray
        sb.FillRect(new Rectangle(0, MafiaRailY, GameConfig.ScreenWidth, 3), new Color(60, 62, 74));
        for (int x = (int)(-time * 300f % 24f); x < GameConfig.ScreenWidth; x += 24)
            sb.FillRect(new Rectangle(x, MafiaRailY + 3, 10, 4), new Color(30, 26, 26));

        _backgroundSmoke.Draw(sb);

        int front = (int)MafiaTrainFrontX();
        if (front < -40) return;

        int rail = MafiaRailY;
        void Box(int x, int y, int w, int h, Color c) => sb.FillRect(new Rectangle(front + x, rail + y, w, h), c);

        var black = new Color(16, 14, 18);
        var plate = new Color(30, 26, 30);
        var red = new Color(170, 24, 30);
        float glow = 0.7f + 0.3f * MathF.Sin(time * 9f);

        // Zırhlı vagon (arkada) ve taret
        Box(-470, -52, 190, 40, black);
        Box(-470, -30, 190, 4, red);
        Box(-410, -70, 50, 18, plate);
        Box(-366, -64, 50, 5, new Color(60, 58, 66));             // Namlu
        for (int x = -462; x < -290; x += 22)
            Box(x, -48, 10, 4, new Color(255, 40, 30) * (0.6f * glow)); // Mazgal ışıkları

        // Lokomotif gövdesi
        Box(-270, -56, 230, 46, black);
        Box(-270, -86, 64, 32, plate);                             // Kabin
        Box(-260, -78, 44, 8, new Color(255, 40, 30) * glow);      // Kızıl göz yarığı
        Box(-120, -96, 18, 40, plate);                             // Baca
        Box(-126, -100, 30, 6, black);
        Box(-270, -26, 230, 4, red);                               // Kızıl şerit
        for (int i = 0; i < 5; i++)
            Box(-40 + i * 6, -50 + i * 8, 14 - i * 2, 6, new Color(120, 120, 130)); // Dikenli kar küreği
        Box(-30, -48, 8, 8, new Color(255, 60, 40));               // Kızıl far
        sb.DrawGlow(new Vector2(front - 26, rail - 44), 60f, new Color(255, 40, 30), 0.45f * glow);
        sb.DrawGlow(new Vector2(front - 238, rail - 74), 40f, new Color(255, 40, 30), 0.3f * glow);

        // Tekerler
        foreach (int wx in new[] { -440, -320, -240, -150, -70 })
        {
            sb.Draw(Assets.WheelSmall, new Vector2(front + wx, rail - 10), null, new Color(120, 110, 120), _wheelAngle,
                new Vector2(Assets.WheelSmall.Width / 2f, Assets.WheelSmall.Height / 2f), 0.75f, SpriteEffects.None, 0f);
        }
    }

    public void DrawWorld(SpriteBatch sb, float time)
    {
        // Buhar püskürtme şok dalgası: trenin arkasından genişleyen halka
        foreach (float age in _shockwaves)
        {
            float radius = 30f + age * 700f;
            float alpha = 1f - age / 0.5f;
            var center = new Vector2(TrainBackX, GameConfig.RailY - 50);
            for (int i = 0; i < 48; i++)
            {
                float angle = i / 48f * MathHelper.TwoPi;
                var p = center + new Vector2(MathF.Cos(angle), MathF.Sin(angle) * 0.6f) * radius;
                sb.FillRect(new Rectangle((int)p.X - 3, (int)p.Y - 3, 6, 6), new Color(230, 240, 255) * (0.7f * alpha));
            }
        }

        DrawBanditCart(sb, BanditFrontX(), time);
        DrawSnowTruck(sb, TruckFrontX(time), time);

        if (_comboActive)
        {
            // Engel sağdan yaklaşır; süre bittiğinde trenin burnuna varır
            float t = MathHelper.Clamp(_comboTime / _comboWindow, 0f, 1f);
            DrawBarricade(sb, MathHelper.Lerp(GameConfig.ScreenWidth + 60, TrainFrontX + 10, t));
        }

        _effects.Draw(sb);

        foreach (var popup in _popups)
        {
            float alpha = 1f - popup.Age / popup.Life;
            float pop = MathHelper.Clamp(popup.Age * 8f, 0f, 1f);
            int scale = Math.Max(1, (int)MathF.Round(popup.Scale * (0.6f + 0.4f * pop)));
            GameText.DrawCentered(sb, popup.Text, popup.Position.X + 3, popup.Position.Y + 3, Color.Black * (0.6f * alpha), scale, Language.English);
            GameText.DrawCentered(sb, popup.Text, popup.Position.X, popup.Position.Y, popup.Color * alpha, scale, Language.English);
        }
    }

    /// <summary>Zırhlı, dikenli mafya ray arabası (kızıl şeritli, kafatası bayraklı).</summary>
    private void DrawBanditCart(SpriteBatch sb, float frontX, float time)
    {
        int right = (int)frontX;
        int rail = GameConfig.RailY;
        void Box(int x, int y, int w, int h, Color c) => sb.FillRect(new Rectangle(right + x, rail + y, w, h), c);

        var armor = new Color(40, 32, 34);
        var red = new Color(150, 30, 34);

        Box(-150, -26, 146, 8, new Color(26, 24, 26));     // Şasi
        Box(-146, -74, 138, 50, armor);                    // Zırhlı gövde
        Box(-146, -56, 138, 6, red);                       // Kızıl şerit
        for (int x = -138; x < -10; x += 18)
            Box(x, -70, 3, 3, new Color(90, 84, 86));      // Perçinler
        Box(-100, -92, 44, 18, armor);                     // Taret
        Box(-56, -86, 46, 5, new Color(70, 70, 76));       // Namlu
        for (int i = 0; i < 4; i++)
            Box(-8 + i * 3, -66 + i * 9, 8 - i * 2, 6, new Color(150, 150, 160)); // Ön dikenler

        // Kafatası bayrağı
        Box(-140, -118, 3, 44, new Color(30, 28, 30));
        float wave = MathF.Sin(time * 8f) * 2f;
        Box(-137, (int)(-118 + wave), 26, 16, new Color(20, 18, 20));
        Box(-130, (int)(-114 + wave), 10, 7, new Color(220, 220, 220));

        // Kırmızı far
        Box(-14, -48, 8, 8, new Color(255, 70, 50));
        sb.DrawGlow(new Vector2(right - 10, rail - 44), 50f, new Color(255, 60, 40), 0.55f);

        foreach (int wx in new[] { -128, -40 })
        {
            var center = new Vector2(right + wx, rail - 14);
            sb.Draw(Assets.WheelSmall, center, null, Color.White, _wheelAngle, new Vector2(Assets.WheelSmall.Width / 2f, Assets.WheelSmall.Height / 2f), 1f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>Raylara paralel karda ilerleyen, kar küreme bıçaklı haydut kamyonu (ön planda).</summary>
    private void DrawSnowTruck(SpriteBatch sb, float frontX, float time)
    {
        int right = (int)frontX;
        int ground = 650;
        float bounce = MathF.Abs(MathF.Sin(time * 12f)) * 2f;
        void Box(int x, int y, int w, int h, Color c) => sb.FillRect(new Rectangle(right + x, (int)(ground + y - bounce), w, h), c);

        Box(-180, -14, 170, 14, new Color(24, 22, 24));          // Paletler
        for (int x = -176; x < -12; x += 12)
            Box(x + (int)(time * 60f % 12f), -12, 6, 10, new Color(60, 58, 60)); // Dönen palet dişleri
        Box(-176, -50, 120, 36, new Color(110, 56, 34));         // Kasa
        Box(-56, -66, 46, 52, new Color(90, 44, 30));            // Kabin
        Box(-48, -60, 30, 16, new Color(255, 170, 90) * 0.5f);   // Ön cam
        Box(-10, -46, 14, 40, new Color(150, 150, 160));         // Kar küreme bıçağı
        Box(-166, -64, 8, 14, new Color(40, 40, 44));            // Egzoz
        sb.DrawGlow(new Vector2(right + 4, ground - 40 - bounce), 40f, new Color(255, 220, 160), 0.4f);
    }

    /// <summary>Raylara kurulmuş buz bloklu barikat.</summary>
    private static void DrawBarricade(SpriteBatch sb, float x)
    {
        int left = (int)x;
        int rail = GameConfig.RailY;
        void Box(int dx, int dy, int w, int h, Color c) => sb.FillRect(new Rectangle(left + dx, rail + dy, w, h), c);

        Box(0, -38, 44, 38, new Color(160, 210, 240));
        Box(30, -60, 34, 60, new Color(140, 195, 230));
        Box(4, -34, 10, 6, new Color(230, 245, 255));
        for (int i = 0; i < 5; i++)
            Box(-12 + i * 18, -50, 18, 10, i % 2 == 0 ? new Color(200, 40, 40) : new Color(230, 230, 230)); // Kırmızı-beyaz kiriş
        sb.DrawGlow(new Vector2(left + 30, rail - 30), 50f, new Color(150, 210, 255), 0.25f);
    }

    // ------------------------------------------------------------------
    // Çizim — arayüz (sarsıntısız)
    // ------------------------------------------------------------------

    public void DrawInterface(SpriteBatch sb, float time)
    {
        var screen = new Rectangle(0, 0, GameConfig.ScreenWidth, GameConfig.ScreenHeight);

        // Buhar püskürtme anında kısa beyaz parlama
        if (_flash > 0f)
            sb.FillRect(screen, new Color(235, 242, 255) * (0.3f * _flash));

        // Kırmızı alarm: giriş boyunca ekran kenarları nabız gibi kızarır
        float alarm = InIntro ? 0.6f + 0.4f * MathF.Sin(time * 14f) : (Outcome == ChaseOutcome.None ? 0.25f : 0f);
        if (alarm > 0f)
            sb.Draw(Assets.Vignette, screen, new Color(255, 30, 20) * (0.55f * alarm));

        if (InIntro)
        {
            float flash = MathHelper.Clamp((_introTimer - GameConfig.ChaseIntroDuration + 0.25f) / 0.25f, 0f, 1f);
            sb.FillRect(screen, new Color(255, 40, 30) * (0.35f * flash));

            float pulse = 0.7f + 0.3f * MathF.Sin(time * 12f);
            GameText.DrawCentered(sb, Loc.T("chase.alert"), GameConfig.ScreenWidth / 2f + 3, 263, Color.Black * 0.7f, 4);
            GameText.DrawCentered(sb, Loc.T("chase.alert"), GameConfig.ScreenWidth / 2f, 260, Hud.Danger * pulse, 4);
            if (_intel)
                GameText.DrawCentered(sb, Loc.T("chase.intel"), GameConfig.ScreenWidth / 2f, 320, Hud.Amber, 2);
            return;
        }

        DrawStatusPanel(sb, time);

        if (_comboActive)
            DrawCombo(sb, time);

        if (_feedback != null && _feedbackTimer > 0f)
            GameText.DrawCentered(sb, _feedback, GameConfig.ScreenWidth / 2f, 395, _feedbackColor * MathHelper.Clamp(_feedbackTimer / 0.4f, 0f, 1f), 2);

        if (Outcome != ChaseOutcome.None && ResultMessage != null)
        {
            Color color = Outcome == ChaseOutcome.Escaped ? CabinPanel.Good : Hud.Danger;
            GameText.DrawCentered(sb, ResultMessage, GameConfig.ScreenWidth / 2f + 3, 293, Color.Black * 0.7f, 3);
            GameText.DrawCentered(sb, ResultMessage, GameConfig.ScreenWidth / 2f, 290, color, 3);
        }

        GameText.Draw(sb, Loc.T("chase.instr"), new Vector2(24, GameConfig.ScreenHeight - 32), Hud.Amber * 0.85f);
    }

    /// <summary>Üst ortadaki panel: mafya mesafesi, kazan basıncı ve kalan kaçış süresi.</summary>
    private void DrawStatusPanel(SpriteBatch sb, float time)
    {
        var panel = new Rectangle(436, 16, 392, 112);
        sb.FillRect(panel, new Color(20, 6, 8) * 0.85f);
        sb.DrawBorder(panel, 2, Hud.Danger * 0.8f);

        string distanceLabel = Loc.T("chase.distance");
        string pressureLabel = Loc.T("chase.pressure");
        float labelWidth = MathF.Max(GameText.MeasureLine(distanceLabel), GameText.MeasureLine(pressureLabel));
        int barX = panel.X + 16 + (int)labelWidth + 12;
        int barWidth = panel.Right - 16 - barX;

        // Mesafe: uzaklık yeşil, yaklaştıkça kırmızı
        float distance = MathHelper.Clamp(_banditDistance, 0f, 1f);
        GameText.Draw(sb, distanceLabel, new Vector2(panel.X + 16, panel.Y + 12), Hud.Label);
        var bar = new Rectangle(barX, panel.Y + 15, barWidth, 14);
        sb.FillRect(bar, new Color(30, 20, 22));
        sb.FillRect(new Rectangle(bar.X, bar.Y, (int)(bar.Width * distance), bar.Height),
            Color.Lerp(Hud.Danger, CabinPanel.Good, distance));
        sb.DrawBorder(bar, 1, Hud.PanelEdge);

        // Basınç: eşik çizgisi, hazırken yanıp sönen [SHIFT]
        GameText.Draw(sb, pressureLabel, new Vector2(panel.X + 16, panel.Y + 44), Hud.Label);
        var pressure = new Rectangle(barX, panel.Y + 47, barWidth, 14);
        sb.FillRect(pressure, new Color(20, 22, 32));
        bool ready = _pressure >= GameConfig.BlastThreshold;
        sb.FillRect(new Rectangle(pressure.X, pressure.Y, (int)(pressure.Width * _pressure), pressure.Height),
            ready ? new Color(200, 230, 255) : new Color(110, 140, 170));
        int thresholdX = pressure.X + (int)(pressure.Width * GameConfig.BlastThreshold);
        sb.FillRect(new Rectangle(thresholdX - 1, pressure.Y - 3, 2, pressure.Height + 6), Hud.Amber);
        sb.DrawBorder(pressure, 1, Hud.PanelEdge);

        float secondsLeft = MathF.Max(0f, GameConfig.ChaseDuration - _time);
        GameText.Draw(sb, Loc.F("chase.time", (int)MathF.Ceiling(secondsLeft)), new Vector2(panel.X + 16, panel.Y + 78), Hud.Value);
        if (ready && MathF.Sin(time * 10f) > -0.2f)
            GameText.DrawRight(sb, Loc.T("chase.ready"), panel.Right - 16, panel.Y + 78, new Color(200, 230, 255));
    }

    /// <summary>Engel uyarısı: sıradaki ok tuşları, ilerleme ve kalan süre çubuğu.</summary>
    private void DrawCombo(SpriteBatch sb, float time)
    {
        float centerX = GameConfig.ScreenWidth / 2f;
        GameText.DrawCentered(sb, Loc.T("chase.obstacle"), centerX, 262, Hud.Danger * (0.75f + 0.25f * MathF.Sin(time * 12f)), 2);

        const int size = 56;
        const int gap = 16;
        int totalWidth = _combo.Length * size + (_combo.Length - 1) * gap;
        int startX = (int)(centerX - totalWidth / 2f);

        for (int i = 0; i < _combo.Length; i++)
        {
            var box = new Rectangle(startX + i * (size + gap), 300, size, size);
            Color color = i < _comboProgress ? CabinPanel.Good
                        : i == _comboProgress ? Hud.Amber
                        : Hud.Label * 0.6f;
            float lift = i == _comboProgress ? MathF.Sin(time * 14f) * 3f : 0f;
            box.Y += (int)lift;

            sb.FillRect(box, new Color(6, 8, 16) * 0.9f);
            sb.FillRect(box, color * 0.12f);
            sb.DrawBorder(box, 3, color);
            DrawArrow(sb, box.Center, _combo[i], 26, color);
        }

        // Kalan süre
        var bar = new Rectangle(startX, 366, totalWidth, 6);
        sb.FillRect(bar, new Color(30, 34, 46));
        float left = 1f - MathHelper.Clamp(_comboTime / _comboWindow, 0f, 1f);
        sb.FillRect(new Rectangle(bar.X, bar.Y, (int)(bar.Width * left), bar.Height), Color.Lerp(Hud.Danger, Hud.Amber, left));
    }

    /// <summary>Piksel bloklarından yön oku (yazı tipine bağlı değil, her dilde aynı).</summary>
    private static void DrawArrow(SpriteBatch sb, Point center, ComboKey key, int size, Color color)
    {
        int half = size / 2;
        for (int i = 0; i <= half; i++)
        {
            switch (key)
            {
                case ComboKey.Up:
                    sb.FillRect(new Rectangle(center.X - i, center.Y - half + i, i * 2 + 1, 1), color);
                    break;
                case ComboKey.Left:
                    sb.FillRect(new Rectangle(center.X - half + i, center.Y - i, 1, i * 2 + 1), color);
                    break;
                case ComboKey.Right:
                    sb.FillRect(new Rectangle(center.X + half - i, center.Y - i, 1, i * 2 + 1), color);
                    break;
            }
        }

        // Okun gövdesi
        switch (key)
        {
            case ComboKey.Up: sb.FillRect(new Rectangle(center.X - 4, center.Y, 9, half), color); break;
            case ComboKey.Left: sb.FillRect(new Rectangle(center.X, center.Y - 4, half, 9), color); break;
            case ComboKey.Right: sb.FillRect(new Rectangle(center.X - half, center.Y - 4, half, 9), color); break;
        }
    }
}
