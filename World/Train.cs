using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheLastDelivery.Core;
using TheLastDelivery.Graphics;

namespace TheLastDelivery.World;

/// <summary>
/// "Old Smokie": hız, yakıt, kazan sıcaklığı ve gövde sağlığını yöneten, kendini bloklarla çizen
/// termal lokomotif. Tren ekranda sabit durur; hareket hissi arka planın kaymasıyla verilir.
/// Lokomotif sağa bakar, paketin ve yolcuların taşındığı vagon arkada (solda) durur.
/// </summary>
public sealed class Train
{
    // Rayın üzerindeki referans noktası: ana vagonun sol alt köşesi.
    // Arkaya eklenen vagonlar ekrana sığsın diye tren gerektiğinde sağa kayar (LayoutOffsetX).
    private static Vector2 Origin => new(GameConfig.TrainX + LayoutOffsetX, GameConfig.RailY);

    private static int _layoutOffset;

    /// <summary>
    /// Ek vagonlar yüzünden trenin sağa kaydırıldığı mesafe (piksel). Kasaba sahnesinde kaydırma kapatılır:
    /// tren binaların önünde her zamanki yerinde durur, ek vagonlar sola, ekran dışına uzanır.
    /// </summary>
    public static int LayoutOffsetX => LayoutShiftEnabled ? _layoutOffset : 0;

    /// <summary>Kaydırma açık mı? (Game1 kasaba sahnesini çizerken kapatır.)</summary>
    public static bool LayoutShiftEnabled { get; set; } = true;

    /// <summary>Son vagonun en fazla bu kadarı ekranın soluna taşabilir (tren uzunluğu hissi, daha az kaydırma).</summary>
    private const int AllowedOverhang = 60;

    /// <summary>Ek vagonların toplam genişliği (ana vagonun arkasında, solda).</summary>
    private static int _extraWidth;

    /// <summary>Trenin en arka ucu (son vagonun sol kenarı) ve en ön ucu (inek mahmuzu), ekran koordinatında.</summary>
    public static int RearX => GameConfig.TrainX + LayoutOffsetX - _extraWidth;
    public static int FrontX => GameConfig.TrainX + LayoutOffsetX + 507;

    // Ek vagonlar (loko → ana vagon → yolcu → kargo → zırhlı); genişlikler ve aradaki kaplin
    private const int PassengerWagonWidth = 156, CargoWagonWidth = 124, ArmoredWagonWidth = 134, CouplerGap = 8;
    private static readonly Vector2 StovePipeTop = new(34, -106);

    // Teker merkezlerinin Origin'e göre konumları
    private static readonly Point[] SmallWheels = { new(30, -14), new(160, -14), new(245, -14), new(450, -14) };
    private static readonly Point[] DriveWheels = { new(330, -22), new(390, -22) };

    private static readonly Vector2 ChimneyTop = new(416, -143);
    private static readonly Vector2 ValvePosition = new(372, -62);

    // Renk paleti
    // Gövde renkleri seçili skinden gelir (Klasik, Siyah Ekspres, Altın Lokomotif...)
    public Economy.TrainSkin Skin { get; set; } = Economy.Cosmetics.TrainSkins[0];
    private Color WagonBody => Skin.WagonBody;
    private Color WagonPlank => Skin.WagonPlank;
    private Color Iron => Skin.Iron;
    private Color Boiler => Skin.Boiler;
    private Color BoilerBand => Skin.BoilerBand;
    private Color CabBody => Skin.CabBody;
    private Color Roof => Skin.Roof;
    private static readonly Color Snow = new(205, 215, 232);
    private static readonly Color LanternLight = new(255, 170, 80);
    private static readonly Color HeadlampLight = new(255, 232, 170);
    private static readonly Color PackageLight = new(90, 200, 210);

    private readonly ParticleSystem _smoke = new();   // Trenin arkasında çizilir
    private readonly ParticleSystem _effects = new(); // Buhar ve kıvılcımlar, trenin önünde
    private readonly Random _rng = new();
    private float _wheelDistance;  // Tekerin katettiği toplam piksel (dönüş açısı buradan hesaplanır)
    private float _smokeTimer;
    private float _steamTimer;
    private float _time;
    private float _stoveTimer;

    private int _steamLevel;   // 0 yok, 1 sızıntı (tıslama), 2 patlamış vana
    private bool _grinding;    // Buzlu rayda kızaklanma
    private int _passengerCount;

    public float Speed { get; private set; }        // km/sa
    public float Fuel { get; private set; }         // 0 - 100
    public float Temperature { get; private set; }  // 0 - 100 °C
    public float Health { get; private set; }       // 0 - MaxHealth
    public float Package { get; private set; }      // 0 - 100: gizemli paketin bütünlüğü (onarılamaz)

    public float SpeedRatio => Speed / GameConfig.MaxSpeed;
    public float ScrollSpeed => Speed * GameConfig.PixelsPerSpeedUnit; // Zeminin piksel/sn kayma hızı
    public bool IsOverheating => Temperature >= GameConfig.OverheatThreshold;
    public bool IsOutOfFuel => Fuel <= 0f;
    public bool IsFrozen => Temperature <= 0f;
    public bool IsWrecked => Health <= 0f;
    public bool IsPackageDestroyed => Package <= 0f;

    /// <summary>Gövdenin dayanabileceği en yüksek hasar (Zırhlı Vagon ile artar).</summary>
    public float MaxHealth
    {
        get => _maxHealth;
        set
        {
            _maxHealth = value;
            if (Health > value) Health = value; // Zırhlı vagon ayrılınca sağlık yeni sınıra iner
        }
    }
    private float _maxHealth = GameConfig.MaxHealth;

    /// <summary>Şiddetli fırtına ve tipide azami hız çarpanı (Game1 her karede ayarlar).</summary>
    public float SpeedCapMultiplier { get; set; } = 1f;

    /// <summary>
    /// Hasarlı tren tam hıza çıkamaz: sağlık düştükçe azami hız %50'ye kadar iner.
    /// Şiddetli fırtına azami hızı ayrıca düşürür.
    /// </summary>
    public float MaxAllowedSpeed =>
        GameConfig.MaxSpeed * MathHelper.Lerp(GameConfig.DamagedSpeedFloor, 1f, Health / MaxHealth) * SpeedCapMultiplier;

    /// <summary>Far arızası: koni söner, sürüş zifiri karanlıkta sürer ([E] ile tamir).</summary>
    public bool HeadlightBroken { get; set; }

    /// <summary>Kabin sobasının doluluğu (0-1): yanıyorsa ana vagonun bacasından ince duman çıkar.</summary>
    public float StoveLevel { get; set; }

    /// <summary>Kargo vagonundaki yükün doluluğu (0-1): sandık sayısı buna göre çizilir.</summary>
    public float CargoFill { get; set; }

    /// <summary>Bacadan çıkan dumanın stili (Sığınak Kuryeleri kozmetik paketi).</summary>
    public Economy.SmokeStyle SmokeStyle { get; set; } = Economy.SmokeStyle.Classic;

    /// <summary>Duman stiline göre bir duman parçacığının rengi, saydamlığı ve ışıma durumu.</summary>
    private (Color Color, float Alpha, bool Additive) SmokeLook(bool steam)
    {
        // Cyber-Glitch modeli klasik duman yerine neon mor buhar püskürtür
        if (Skin.Model == Economy.TrainModel.CyberGlitch && SmokeStyle == Economy.SmokeStyle.Classic)
            return (steam ? new Color(230, 170, 255) : new Color(170, 60, 255), 0.5f, true);
        switch (SmokeStyle)
        {
            case Economy.SmokeStyle.NeonBlue: // Mavi neon buhar: parlayan camgöbeği
                return (steam ? new Color(170, 240, 255) : new Color(50, 190, 255), 0.55f, true);
            case Economy.SmokeStyle.MilitaryBlack: // Siyah askeri duman: kalın, ağır, koyu
                return (steam ? new Color(70, 72, 60) : new Color(18, 18, 20), steam ? 0.6f : 0.7f, false);
            case Economy.SmokeStyle.RedFlame: // Kırmızı alev büyüsü: kor gibi parlayan kızıl bulut
                return (steam ? new Color(255, 200, 80) : new Color(255, 70, 30), 0.5f, true);
            case Economy.SmokeStyle.FlameSteam: // Alevli buhar: turuncu-sarı, alev dili gibi parlayan bulut
                return (steam ? new Color(255, 230, 150) : new Color(255, 120, 30), 0.45f, true);
            case Economy.SmokeStyle.VioletGala: // Mor gala dumanı: koyu mor bulut, pembe parıltılar
                return (steam ? new Color(255, 170, 240) : new Color(130, 60, 200), steam ? 0.5f : 0.55f, steam);
            case Economy.SmokeStyle.AuroraMist: // Kutup ışığı buharı: yeşil-mor dalgalanan ışıltı
                return (MathF.Sin(_time * 3f + _rng.Next(10)) > 0f ? new Color(90, 240, 170) : new Color(170, 110, 255), 0.5f, true);
            default:
                return (steam ? new Color(225, 230, 238) : new Color(78, 80, 90), steam ? 0.6f : 0.45f, false);
        }
    }

    /// <summary>Kabinde Arthur'un yanındaki evcil hayvan (makinist penceresinden görünür).</summary>
    public Economy.PetKind Pet { get; set; }

    /// <summary>Dönemsel süsler: Kış Şenliği ışıkları, Hayalet Haftası balkabakları.</summary>
    public Season Season { get; set; }

    private static readonly Color[] FestiveColors =
        { new(255, 70, 60), new(80, 230, 110), new(255, 210, 80), new(90, 160, 255) };

    /// <summary>Çatı boyunca ampullerin konumları (Origin'e göre).</summary>
    private static System.Collections.Generic.IEnumerable<Point> FestiveBulbs()
    {
        for (int x = -2; x <= 192; x += 13) yield return new Point(x, -93);   // Vagon çatısı
        for (int x = 204; x <= 304; x += 13) yield return new Point(x, -133); // Kabin çatısı
        for (int x = 304; x <= 450; x += 14) yield return new Point(x, -91);  // Kazan üstü
    }

    private Color BulbColor(int index) => FestiveColors[(index + (int)(_time * 2.5f)) % FestiveColors.Length];

    /// <summary>Kış Şenliği ampulleri ve Hayalet Haftası balkabağı (gövdenin üstüne).</summary>
    private void DrawSeasonal(Action<int, int, int, int, Color> box)
    {
        if (Season == Season.WinterFestival)
        {
            int i = 0;
            foreach (var p in FestiveBulbs())
            {
                box(p.X - 6, p.Y - 1, 13, 1, new Color(30, 40, 30)); // Kablo
                box(p.X - 1, p.Y, 3, 4, BulbColor(i++));
            }
            box(358, -121, 14, 4, new Color(40, 110, 50));             // Buhar domunda çelenk
            box(363, -125, 4, 4, new Color(220, 40, 40));
        }
        else if (Season == Season.GhostWeek)
        {
            var pumpkin = new Color(240, 130, 30);
            box(262, -142, 16, 10, pumpkin);                           // Kabin çatısında balkabağı fener
            box(268, -145, 3, 3, new Color(70, 110, 40));
            float flicker = 0.7f + 0.3f * MathF.Sin(_time * 11f);
            box(265, -139, 3, 2, new Color(255, 230, 100) * flicker);  // Gözler
            box(272, -139, 3, 2, new Color(255, 230, 100) * flicker);
            box(266, -135, 8, 1, new Color(255, 230, 100) * flicker);  // Ağız
            box(150, -101, 12, 8, pumpkin);                            // Vagonun çatısında küçük bir tane daha
            box(155, -103, 2, 2, new Color(70, 110, 40));
        }
    }

    /// <summary>Satın alınmış ek vagonlar.</summary>
    public bool HasPassengerWagon { get; private set; }
    public bool HasCargoWagon { get; private set; }
    public bool HasArmoredWagon { get; private set; }

    /// <summary>Ek vagonları bağlar ve treni ekrana sığacak şekilde konumlandırır.</summary>
    public void SetWagons(bool passenger, bool cargo, bool armored)
    {
        HasPassengerWagon = passenger;
        HasCargoWagon = cargo;
        HasArmoredWagon = armored;
        _extraWidth = (passenger ? PassengerWagonWidth + CouplerGap : 0)
                      + (cargo ? CargoWagonWidth + CouplerGap : 0)
                      + (armored ? ArmoredWagonWidth + CouplerGap : 0);
        _layoutOffset = Math.Max(0, _extraWidth - (GameConfig.TrainX - 12) - AllowedOverhang);
    }

    /// <summary>Farın ekrandaki konumu (ışık haritası için).</summary>
    public Vector2 HeadlampPosition => new(Origin.X + 476, Origin.Y - 102);

    /// <summary>Tam sağlıkta mı? (yedek parça ve tamirci gereksizse)</summary>
    public bool IsFullHealth => Health >= MaxHealth - 0.01f;

    public Train() => Reset();

    public void Reset()
    {
        Speed = GameConfig.StartSpeed;
        Fuel = FuelCapacity;
        Temperature = GameConfig.StartTemperature;
        Health = MaxHealth;
        HeadlightBroken = false;
        SpeedCapMultiplier = 1f;
        Package = GameConfig.MaxPackage;
        _wheelDistance = 0f;
        _smokeTimer = 0f;
        _steamTimer = 0f;
        _steamLevel = 0;
        _grinding = false;
        _passengerCount = 0;
        _smoke.Clear();
        _effects.Clear();
    }

    // ------------------------------------------------------------------
    // Sürüş ve kaynaklar
    // ------------------------------------------------------------------

    /// <param name="direction">+1 hızlan, -1 yavaşla.</param>
    public void Throttle(int direction, float dt)
    {
        if (direction > 0)
        {
            if (Speed < MaxAllowedSpeed)
                Speed = MathF.Min(Speed + GameConfig.Acceleration * dt, MaxAllowedSpeed);
        }
        else
        {
            Speed = MathF.Max(0f, Speed - GameConfig.Deceleration * dt);
        }
    }

    public void SetSpeed(float speed) => Speed = MathHelper.Clamp(speed, 0f, GameConfig.MaxSpeed);

    public void Stop() => Speed = 0f;

    /// <summary>Ölü trenin sürtünmeyle yavaşça durması (Game Over sırasında).</summary>
    public void Coast(float dt) => SetSpeed(Speed - 20f * dt);

    /// <summary>
    /// Yakıt yol ve zamanla tükenir; yüksek hız ve yolcu ağırlığı km başına daha çok yakar.
    /// Kazan yüksek hızda ısınır, düşük hızda dondurucu hava onu soğutur (yolcular bunu yavaşlatır).
    /// </summary>
    public void UpdateResources(float dt, float kmTravelled, TravelModifiers modifiers)
    {
        float burnPerKm = (GameConfig.FuelBurnPerKmBase + GameConfig.FuelBurnPerKmSpeed * SpeedRatio * SpeedRatio)
                          * modifiers.FuelMultiplier;
        if (IsOverheating)
            burnPerKm *= GameConfig.OverheatFuelMultiplier;

        Fuel -= burnPerKm * kmTravelled + GameConfig.FuelIdleBurnPerSecond * GameConfig.NeedsScale * dt;

        float heating = GameConfig.HeatRate * modifiers.HeatMultiplier * SpeedRatio;
        float cooling = GameConfig.CoolRate * modifiers.CoolingMultiplier;
        Temperature += (heating - cooling) * dt;

        // Hasar alan tren, yeni hız sınırına yavaşça iner
        if (Speed > MaxAllowedSpeed)
            Speed = MathF.Max(MaxAllowedSpeed, Speed - GameConfig.Deceleration * 0.5f * dt);

        Fuel = MathHelper.Clamp(Fuel, 0f, FuelCapacity);
        Temperature = MathHelper.Clamp(Temperature, 0f, GameConfig.MaxTemperature);
    }

    /// <summary>Kazanılan yakıt (Acımasız Kış modunda kaynaklar yarıya iner).</summary>
    public void Refuel(float amount) => Fuel = MathHelper.Clamp(Fuel + Difficulty.Scale(amount), 0f, FuelCapacity);

    /// <summary>Ek yakıt tüketimi (kovalamacada tam gaz) ya da mafyanın çaldığı yakıt.</summary>
    public void BurnFuel(float amount) => Fuel = MathHelper.Clamp(Fuel - amount, 0f, FuelCapacity);

    /// <summary>Yakıt deposu kapasitesi (Büyük Yakıt Deposu geliştirmesiyle artar).</summary>
    public float FuelCapacity { get; set; } = GameConfig.MaxFuel;

    /// <summary>Gövde hasarı çarpanı (Zırh Plakaları geliştirmesiyle azalır).</summary>
    public float ArmorMultiplier { get; set; } = 1f;

    /// <summary>Gelişmiş Fener geliştirmesi: far konisi büyür.</summary>
    public bool HasLantern { get; set; }

    /// <summary>0-1 arası yakıt doluluğu.</summary>
    public float FuelRatio => Fuel / FuelCapacity;

    public void StokeBoiler(float amount) =>
        Temperature = MathHelper.Clamp(Temperature + amount, 0f, GameConfig.MaxTemperature);

    public void DrainHeat(float amount) =>
        Temperature = MathHelper.Clamp(Temperature - amount, 0f, GameConfig.MaxTemperature);

    /// <summary>Gövde hasarı; sarsıntının bir kısmı arka vagondaki paketi de etkiler.</summary>
    public void Damage(float amount)
    {
        amount *= ArmorMultiplier;
        Health = MathHelper.Clamp(Health - amount, 0f, MaxHealth);
        Package = MathHelper.Clamp(Package - amount * GameConfig.PackageDamageShare, 0f, GameConfig.MaxPackage);
    }

    public void Repair(float amount) => Health = MathHelper.Clamp(Health + amount, 0f, MaxHealth);

    // ------------------------------------------------------------------
    // Görsel güncelleme
    // ------------------------------------------------------------------

    /// <summary>Bu karede gösterilecek olay efektlerini ve kabindeki yolcu sayısını ayarlar.</summary>
    /// <param name="steamLevel">0 yok, 1 sızdıran vana, 2 patlamış vana.</param>
    public void SetEffects(int steamLevel, bool grinding, int passengerCount)
    {
        _steamLevel = steamLevel;
        _grinding = grinding;
        _passengerCount = passengerCount;
    }

    /// <param name="engineAlive">Kazan sönmüşse bacadan duman çıkmaz.</param>
    /// <param name="windSpeed">Fırtına rüzgarının hızı (piksel/sn).</param>
    public void UpdateVisuals(float dt, bool engineAlive, float windSpeed)
    {
        _time += dt;
        _wheelDistance += ScrollSpeed * dt;
        _sharedWheelAngle = _wheelDistance / 14f;

        if (engineAlive)
        {
            // Hız arttıkça baca daha sık duman atar
            _smokeTimer -= dt;
            if (_smokeTimer <= 0f)
            {
                _smokeTimer = MathHelper.Lerp(0.22f, 0.04f, SpeedRatio);
                EmitSmoke();
            }

            if (_steamLevel > 0)
            {
                _steamTimer -= dt;
                if (_steamTimer <= 0f)
                {
                    _steamTimer = _steamLevel == 1 ? 0.07f : 0.02f;
                    EmitSteam(_steamLevel);
                }
            }
        }

        if (_grinding)
            EmitSparks();

        // Kabin sobası yanıyorsa ana vagonun küçük bacasından ince duman
        if (engineAlive && StoveLevel > 0.02f)
        {
            _stoveTimer -= dt;
            if (_stoveTimer <= 0f)
            {
                _stoveTimer = MathHelper.Lerp(0.5f, 0.18f, StoveLevel);
                _smoke.Emit(new Particle
                {
                    Position = Origin + StovePipeTop + new Vector2(_rng.Next(-2, 3), 0),
                    Velocity = new Vector2(_rng.Next(-6, 7), -_rng.Next(25, 45)),
                    Life = 1.4f + (float)_rng.NextDouble() * 0.8f,
                    Size = 4f + _rng.Next(3),
                    Growth = 3f,
                    Color = new Color(96, 96, 104),
                    Alpha = 0.3f + 0.2f * StoveLevel,
                    WindInfluence = 1f,
                    Soft = true
                });
            }
        }

        // Duman, trenin geride bıraktığı hava akımı ve fırtınayla sola sürüklenir
        float wind = -ScrollSpeed * 0.8f - windSpeed * 0.5f;
        _smoke.Update(dt, wind);
        _effects.Update(dt, wind);
    }

    /// <summary>
    /// Duman püskürtme ya da kazana kömür atma: bacadan hızla yükselip genişleyen yoğun duman ve buhar bulutu.
    /// </summary>
    /// <param name="power">0-1: bulutun yoğunluğu ve yüksekliği.</param>
    public void BurstSmoke(int count, float power)
    {
        for (int i = 0; i < count; i++)
        {
            bool steam = i % 3 == 0; // Aralarda beyaz buhar, çoğunlukla koyu duman
            float spread = (float)_rng.NextDouble() * 2f - 1f;
            var look = SmokeLook(steam);
            _smoke.Emit(new Particle
            {
                Position = Origin + ChimneyTop + new Vector2(_rng.Next(-6, 7), _rng.Next(-4, 2)),
                Velocity = new Vector2(spread * 70f * power - ScrollSpeed * 0.1f,
                    -(110f + (float)_rng.NextDouble() * 160f) * (0.5f + power)),
                Life = 1.4f + (float)_rng.NextDouble() * 1.4f,
                Size = 8f + _rng.Next(8),
                Growth = 4.5f + 2f * power,
                Color = look.Color,
                Alpha = look.Alpha,
                WindInfluence = 0.7f,
                Soft = true,
                Additive = look.Additive
            });
        }

        // Kazanın altından iki yana tıslayan beyaz buhar
        for (int i = 0; i < count / 2; i++)
        {
            _effects.Emit(new Particle
            {
                Position = Origin + new Vector2(300 + _rng.Next(0, 150), -28),
                Velocity = new Vector2(_rng.Next(-120, 121), -_rng.Next(20, 90)),
                Life = 0.5f + (float)_rng.NextDouble() * 0.6f,
                Size = 5f + _rng.Next(4),
                Growth = 3.5f,
                Color = new Color(230, 236, 245),
                Alpha = 0.55f,
                WindInfluence = 0.5f,
                Soft = true
            });
        }
    }

    private void EmitSmoke()
    {
        // Hızlıyken duman daha kısa yaşar: geriye doğru hızla akıp kaybolur
        float lifeScale = 1f - 0.55f * SpeedRatio;
        var look = SmokeLook(steam: false);
        _smoke.Emit(new Particle
        {
            Position = Origin + SmokeOrigin() + new Vector2(_rng.Next(-4, 5), 0),
            Velocity = new Vector2(-ScrollSpeed * 0.15f + _rng.Next(-10, 11), -_rng.Next(40, 75)),
            Life = (2.2f + (float)_rng.NextDouble() * 1.2f) * lifeScale,
            Size = 10f + _rng.Next(6),
            Growth = 3.5f,
            Color = look.Color,
            Alpha = look.Additive ? 0.35f : (SmokeStyle == Economy.SmokeStyle.MilitaryBlack ? 0.6f : 0.45f),
            WindInfluence = 1f,
            Soft = true,
            Additive = look.Additive
        });

        // Kırmızı alev büyüsü ve alevli buhar: dumanın içinden yükselen kıvılcımlar; mor gala: pembe parıltılar
        if (SmokeStyle == Economy.SmokeStyle.VioletGala && _rng.Next(3) == 0)
        {
            _effects.Emit(new Particle
            {
                Position = Origin + ChimneyTop + new Vector2(_rng.Next(-14, 15), -_rng.Next(4, 30)),
                Velocity = new Vector2(-ScrollSpeed * 0.15f + _rng.Next(-20, 21), -_rng.Next(20, 60)),
                Life = 0.6f + (float)_rng.NextDouble() * 0.6f,
                Size = 1.5f,
                Color = new Color(255, 200, 255),
                Alpha = 1f,
                WindInfluence = 0.8f,
                Additive = true
            });
        }
        if ((SmokeStyle == Economy.SmokeStyle.RedFlame || SmokeStyle == Economy.SmokeStyle.FlameSteam) && _rng.Next(2) == 0)
        {
            _effects.Emit(new Particle
            {
                Position = Origin + ChimneyTop + new Vector2(_rng.Next(-6, 7), -4),
                Velocity = new Vector2(-ScrollSpeed * 0.1f + _rng.Next(-30, 31), -_rng.Next(60, 130)),
                Life = 0.5f + (float)_rng.NextDouble() * 0.5f,
                Size = 1.5f,
                Color = new Color(255, 180, 60),
                Alpha = 1f,
                WindInfluence = 0.6f,
                Additive = true
            });
        }
    }

    private void EmitSteam(int level)
    {
        _effects.Emit(new Particle
        {
            Position = Origin + ValvePosition,
            Velocity = new Vector2(_rng.Next(-40, 41), -_rng.Next(50, level == 1 ? 90 : 160)),
            Life = 0.5f + (float)_rng.NextDouble() * (level == 1 ? 0.4f : 0.8f),
            Size = level == 1 ? 4f : 7f,
            Growth = 2.5f,
            Color = new Color(225, 232, 242),
            Alpha = level == 1 ? 0.45f : 0.65f,
            WindInfluence = 0.6f,
            Soft = true
        });
    }

    private void EmitSparks()
    {
        foreach (var wheel in DriveWheels)
        {
            for (int i = 0; i < 2; i++)
            {
                _effects.Emit(new Particle
                {
                    Position = Origin + new Vector2(wheel.X + _rng.Next(-8, 9), -1),
                    Velocity = new Vector2(-_rng.Next(80, 240), -_rng.Next(30, 140)),
                    Life = 0.25f + (float)_rng.NextDouble() * 0.25f,
                    Size = 1f,
                    Growth = 0f,
                    Color = new Color(255, 180, 80),
                    Alpha = 1f,
                    Gravity = 420f,
                    Additive = true
                });
            }
        }
    }

    // ------------------------------------------------------------------
    // Çizim
    // ------------------------------------------------------------------

    /// <summary>Tam sürüm skinlerinin ayırt edici parçaları (pulluk, dikenler, taç, kürk, lamba, parıltı).</summary>
    private void DrawStyleParts(Action<int, int, int, int, Color> box)
    {
        switch (Skin.Style)
        {
            case Economy.TrainStyle.Icebreaker: // Önde kademeli dev buz kıran pulluk
                for (int i = 0; i < 5; i++)
                    box(482 + i * 6, -58 + i * 9, 10, 58 - i * 9, i % 2 == 0 ? Skin.Shine : Skin.Trim);
                box(480, -60, 4, 46, Iron);
                break;

            case Economy.TrainStyle.Bandit: // Yamalı plakalar ve dikenler
                box(20, -76, 30, 18, Skin.BoilerBand);
                box(130, -52, 26, 14, Skin.BoilerBand);
                box(320, -84, 26, 16, Skin.Iron);
                for (int x = 0; x < 190; x += 18) box(x + 4, -98, 4, 6, Skin.Trim);  // Çatıda dikenler
                for (int x = 300; x < 450; x += 22) box(x + 6, -96, 4, 6, Skin.Trim);
                box(486, -40, 14, 4, Skin.Trim);
                break;

            case Economy.TrainStyle.Royal: // Altın taç, flama, süslü şeritler
                box(352, -115, 26, 6, Skin.Trim);
                box(352, -121, 4, 6, Skin.Trim); box(363, -123, 4, 8, Skin.Trim); box(374, -121, 4, 6, Skin.Trim);
                box(214, -150, 3, 22, Skin.Iron);                                       // Flama direği
                box(217, -150, 18, 10, Skin.Boiler);
                box(217, -146, 18, 2, Skin.Trim);
                box(0, -60, 190, 2, Skin.Trim);
                break;

            case Economy.TrainStyle.WolfHunter: // Kürk şeritleri ve kemik diş sırası
                box(0, -84, 190, 5, new Color(120, 110, 96));
                for (int x = 6; x < 186; x += 12) box(x, -80, 3, 5, new Color(120, 110, 96));
                for (int x = 300; x < 448; x += 10) box(x, -34, 4, 6, Skin.Trim);
                break;

            case Economy.TrainStyle.Miner: // Kabin üstünde maden lambası, sarı-siyah uyarı şeridi
                box(246, -140, 18, 8, new Color(40, 40, 44));
                box(250, -138, 10, 4, new Color(255, 220, 120));
                for (int x = 0; x < 190; x += 16) box(x, -32, 8, 4, Skin.Trim);
                box(150, -100, 34, 12, new Color(30, 30, 32));                        // Kömür sepeti
                box(152, -104, 30, 4, new Color(60, 56, 60));
                break;

            case Economy.TrainStyle.GoldPlated: // Altın kaplama: perçin sırası, parlayan kenarlar, kazanda ışıltı
                for (int x = 4; x < 190; x += 12) box(x, -82, 3, 3, Skin.Shine);
                for (int x = 304; x < 448; x += 12) box(x, -84, 3, 3, Skin.Shine);
                box(210, -122, 90, 3, Skin.Shine);
                {
                    int glint = (int)((_time * 90f) % 260f);
                    if (glint < 150) box(300 + glint, -88, 6, 24, Color.White * 0.35f); // Kazanın üstünde kayan parıltı
                }
                box(352, -112, 26, 4, Skin.Trim);                                         // Altın çan
                box(358, -118, 14, 6, Skin.Trim);
                break;

            case Economy.TrainStyle.Military: // Kamuflaj lekeleri, kum torbaları, beyaz yıldız
                var camo = new Color(46, 54, 36);
                box(20, -72, 34, 14, camo); box(110, -58, 40, 16, camo); box(150, -76, 24, 10, camo);
                box(312, -86, 30, 16, camo); box(380, -70, 40, 14, camo);
                box(230, -96, 20, 16, camo);
                box(256, -112, 18, 3, Skin.Trim); box(263, -119, 4, 17, Skin.Trim);       // Beyaz yıldız (artı biçiminde)
                for (int i = 0; i < 4; i++) box(10 + i * 22, -100, 20, 10, new Color(150, 136, 100)); // Kum torbaları
                break;

            case Economy.TrainStyle.Aurora: // Kazan üzerinde dalgalanan kutup ışığı şeridi
                for (int i = 0; i < 8; i++)
                {
                    float wave = MathF.Sin(_time * 2f + i * 0.8f);
                    var color = Color.Lerp(Skin.BoilerBand, Skin.Trim, 0.5f + 0.5f * wave) * 0.85f;
                    box(300 + i * 19, -76 + (int)(wave * 3f), 18, 3, color);
                }
                break;
        }
    }

    /// <summary>Duman bulutları; trenin arkasında kalması için gövdeden önce çizilir.</summary>
    public void DrawSmoke(SpriteBatch sb) => _smoke.Draw(sb);

    // ------------------------------------------------------------------
    // Özgün tren modelleri (renk değil, tamamen farklı 2D gövde)
    // ------------------------------------------------------------------

    /// <summary>Modele göre dumanın çıktığı nokta: Citadel'in üç bacası sırayla tüter, Panzer egzozdan, Cyber havalandırmadan.</summary>
    private Vector2 SmokeOrigin() => Skin.Model switch
    {
        Economy.TrainModel.Citadel => new Vector2(330 + _rng.Next(3) * 46, -172),
        Economy.TrainModel.Panzer => new Vector2(312, -150),
        Economy.TrainModel.CyberGlitch => new Vector2(414, -122),
        _ => ChimneyTop
    };

    private void DrawModelLocomotive(SpriteBatch sb, Action<int, int, int, int, Color> box, Point o)
    {
        switch (Skin.Model)
        {
            case Economy.TrainModel.CyberGlitch: DrawCyberGlitch(sb, box, o); break;
            case Economy.TrainModel.Panzer: DrawPanzer(sb, box, o); break;
            case Economy.TrainModel.Citadel: DrawCitadel(sb, box, o); break;
        }
    }

    /// <summary>
    /// Cyber-Glitch Locomotive: AURA teknolojisiyle birleşmiş açılı siyah gövde, mor neon şeritler,
    /// hologram kabin camı ve gövdeden kopup kayan dijital piksel parazitleri.
    /// </summary>
    private void DrawCyberGlitch(SpriteBatch sb, Action<int, int, int, int, Color> box, Point o)
    {
        var body = new Color(18, 14, 28);
        var panel = new Color(32, 26, 48);
        var neon = new Color(200, 80, 255);
        var cyan = new Color(80, 230, 255);
        float pulse = 0.7f + 0.3f * MathF.Sin(_time * 5f);

        box(205, -28, 290, 8, new Color(30, 30, 40));                 // Şasi
        box(208, -112, 250, 84, body);                                 // Ana gövde
        for (int i = 0; i < 6; i++)                                    // Açılı burun (basamaklı kama)
            box(458 + i * 7, -104 + i * 12, 7, 76 - i * 12, panel);
        box(220, -130, 120, 20, body);                                 // Alçak kabin kubbesi
        box(232, -124, 92, 12, cyan * (0.55f * pulse));                // Hologram cam
        box(232, -124, 92, 2, Color.White * 0.6f);
        for (int x = 230; x < 450; x += 24) box(x, -86, 14, 3, panel); // Panel çizgileri
        box(208, -60, 280, 3, neon * pulse);                           // Neon şerit
        box(208, -98, 250, 2, neon * (0.5f * pulse));
        box(404, -126, 22, 16, panel);                                 // Havalandırma (baca yerine)
        box(408, -122, 14, 4, neon * pulse);
        box(470, -100, 10, 10, cyan);                                  // Far yuvası
        box(462, -108, 14, 12, HeadlightBroken ? new Color(40, 38, 34) : HeadlampLight * HeadlampFlicker());
        box(486, -30, 22, 8, neon * 0.8f);                             // Alt neon kanat

        // Dijital parazit: gövdeden dilimler kayar, piksel kareleri saçılır
        if (MathF.Sin(_time * 13f) * MathF.Sin(_time * 4.1f) > 0.55f)
        {
            int slice = -110 + (int)((_time * 97f) % 80f);
            box(214 + _rng.Next(-8, 9), slice, 240, 6, neon * 0.45f);
            box(214 + _rng.Next(-12, 13), slice + 10, 180, 3, cyan * 0.4f);
        }
        for (int i = 0; i < 6; i++)
        {
            float a = _time * (1.3f + i * 0.4f) + i;
            int px = 220 + (int)((MathF.Sin(a) * 0.5f + 0.5f) * 260f);
            int py = -140 + (int)((MathF.Cos(a * 1.3f) * 0.5f + 0.5f) * 110f);
            if (((int)(_time * 8f) + i) % 3 == 0) box(px, py, 4, 4, (i % 2 == 0 ? neon : cyan) * 0.8f);
        }
        sb.DrawGlow(new Vector2(o.X + 340, o.Y - 60), 170f, neon, 0.12f * pulse);
        if (Pet != Economy.PetKind.None) UI.CabinDecor.DrawPet(sb, new Point(o.X + 250, o.Y - 112), Pet, 1, _time);
    }

    /// <summary>
    /// Heavy Panzer Express: zırh kaplı kutu gövde, perçinli plakalar, gözetleme yarığı, kısa egzozlar ve
    /// önde devasa çelik buz kıran koçbaşı. Tekerlek yerine paletler (DrawTreads). Darbe hasarını %40 azaltır.
    /// </summary>
    private void DrawPanzer(SpriteBatch sb, Action<int, int, int, int, Color> box, Point o)
    {
        var armor = Color.Lerp(Skin.Boiler, new Color(70, 76, 58), 0.6f);
        var dark = UI.UiDraw.Shade(armor, 0.75f);
        var steel = new Color(120, 124, 132);

        box(200, -150, 290, 116, armor);                               // Zırh gövde
        box(196, -156, 298, 8, dark);                                  // Üst zırh kenarı
        box(198, -160, 294, 4, Snow);
        for (int y = -140; y < -40; y += 24)                           // Plaka dikişleri
            box(200, y, 290, 2, dark);
        for (int x = 210; x < 486; x += 18)                            // Perçin sıraları
        {
            box(x, -146, 3, 3, steel);
            box(x, -44, 3, 3, steel);
        }
        box(236, -118, 70, 10, new Color(14, 14, 16));                 // Gözetleme yarığı
        float lantern = 0.75f + 0.25f * MathF.Sin(_time * 9f) * MathF.Sin(_time * 3.3f);
        box(240, -116, 62, 6, LanternLight * (0.6f * lantern));
        box(300, -168, 16, 18, dark);                                  // Egzozlar
        box(330, -164, 14, 14, dark);
        box(298, -172, 20, 5, new Color(40, 40, 44));
        box(340, -100, 110, 26, dark);                                 // Yan zırh paneli
        box(352, -96, 86, 4, Skin.Trim);
        box(462, -112, 14, 12, HeadlightBroken ? new Color(40, 38, 34) : HeadlampLight * HeadlampFlicker());
        box(458, -116, 22, 4, steel);                                  // Far siperi

        // Devasa buz kıran koçbaşı: öne doğru alçalan çelik kama ve dişler
        for (int i = 0; i < 7; i++)
            box(486 + i * 8, -120 + i * 15, 10, 120 - i * 15, i % 2 == 0 ? steel : UI.UiDraw.Shade(steel, 0.8f));
        for (int i = 0; i < 5; i++)
            box(536 + i * 2, -40 + i * 7, 8, 4, new Color(200, 210, 220));
        box(486, -124, 6, 96, new Color(60, 62, 70));
        if (Pet != Economy.PetKind.None) UI.CabinDecor.DrawPet(sb, new Point(o.X + 262, o.Y - 104), Pet, 1, _time);
    }

    /// <summary>Panzer'in paletleri: tekerlerin yerinde dönen palet zinciri ve yol tekerleri.</summary>
    private void DrawTreads(SpriteBatch sb)
    {
        int left = (int)Origin.X + 196, right = (int)Origin.X + 500, top = (int)Origin.Y - 34;
        var tread = new Color(30, 30, 34);
        sb.FillRect(new Rectangle(left, top, right - left, 34), tread);
        sb.Draw(Assets.Circle, new Rectangle(left - 16, top, 34, 34), tread);
        sb.Draw(Assets.Circle, new Rectangle(right - 18, top, 34, 34), tread);
        int shift = (int)(_wheelDistance % 16f);
        for (int x = left - shift; x < right; x += 16)                 // Dönen palet dişleri (üst ve alt)
        {
            if (x < left) continue;
            sb.FillRect(new Rectangle(x, top + 1, 8, 4), new Color(70, 70, 76));
            sb.FillRect(new Rectangle(right - (x - left) - 8, top + 29, 8, 4), new Color(70, 70, 76));
        }
        for (int i = 0; i < 6; i++)                                    // Yol tekerleri
        {
            int cx = left + 20 + i * 52;
            sb.Draw(Assets.Circle, new Rectangle(cx - 12, top + 5, 24, 24), new Color(64, 66, 72));
            sb.Draw(Assets.Circle, new Rectangle(cx - 5, top + 12, 10, 10), new Color(110, 112, 120));
        }
        // Wagon tekerleri standart kalır
        foreach (var w in SmallWheels)
            if (w.X < 200) DrawWheel(sb, Assets.WheelSmall, w, 14f);
    }

    /// <summary>
    /// Steam-Punk Citadel: pirinç kaplı kazan, üç dev baca, dışarıdan görünen dönen mekanik çarklar,
    /// bakır borular ve kemerli kabin camı.
    /// </summary>
    private void DrawCitadel(SpriteBatch sb, Action<int, int, int, int, Color> box, Point o)
    {
        var brass = new Color(190, 140, 60);
        var copper = new Color(170, 90, 50);
        var darkBrass = new Color(120, 84, 36);
        var iron = new Color(40, 36, 34);

        box(205, -30, 290, 10, iron);                                  // Şasi
        box(208, -128, 96, 100, new Color(90, 50, 34));                // Maun kabin
        box(200, -138, 112, 12, darkBrass);                            // Kubbeli kabin çatısı
        box(212, -146, 88, 8, brass);
        box(204, -150, 104, 4, Snow);
        box(226, -116, 56, 34, new Color(20, 14, 12));                 // Kemerli cam
        box(232, -122, 44, 6, new Color(20, 14, 12));
        float lantern = 0.75f + 0.25f * MathF.Sin(_time * 9f) * MathF.Sin(_time * 3.3f);
        box(228, -114, 52, 30, LanternLight * (0.55f * lantern));
        box(252, -122, 4, 40, darkBrass);                              // Cam çerçevesi

        box(304, -96, 160, 66, brass);                                 // Pirinç kazan
        box(304, -96, 160, 4, new Color(255, 230, 160));
        for (int x = 316; x < 464; x += 20) box(x, -96, 3, 66, darkBrass); // Kazan halkaları
        for (int x = 310; x < 460; x += 10) box(x, -40, 3, 3, new Color(255, 220, 140)); // Perçinler
        box(304, -66, 160, 3, copper);                                 // Bakır boru
        box(300, -78, 8, 50, copper);

        // Üç dev baca, alevli taçlar
        for (int i = 0; i < 3; i++)
        {
            int cx = 326 + i * 46;
            box(cx, -158, 18, 62, iron);
            box(cx - 6, -168, 30, 10, brass);
            box(cx - 3, -172, 24, 4, darkBrass);
        }
        box(464, -104, 24, 76, darkBrass);                             // Duman kutusu
        box(462, -112, 14, 12, HeadlightBroken ? new Color(40, 38, 34) : HeadlampLight * HeadlampFlicker());
        for (int i = 0; i < 3; i++) box(488 + i * 8, -30 + i * 6, 8, 14 - i * 4, brass); // Pirinç mahmuz

        // Dışarıdan görünen çarklar (kabinin yanında ve kazanın altında), dönen dişlerle
        DrawGear(sb, new Vector2(o.X + 268, o.Y - 56), 20f, _wheelDistance / 20f, brass);
        DrawGear(sb, new Vector2(o.X + 296, o.Y - 40), 12f, -_wheelDistance / 12f, copper);
        DrawGear(sb, new Vector2(o.X + 430, o.Y - 118), 14f, _time * 2f, brass);
        if (Pet != Economy.PetKind.None) UI.CabinDecor.DrawPet(sb, new Point(o.X + 236, o.Y - 84), Pet, 1, _time);
    }

    private static void DrawGear(SpriteBatch sb, Vector2 center, float radius, float angle, Color color)
    {
        sb.Draw(Assets.Circle, new Rectangle((int)(center.X - radius), (int)(center.Y - radius), (int)(radius * 2), (int)(radius * 2)), color);
        sb.Draw(Assets.Circle, new Rectangle((int)(center.X - radius * 0.45f), (int)(center.Y - radius * 0.45f), (int)(radius * 0.9f), (int)(radius * 0.9f)), UI.UiDraw.Shade(color, 0.6f));
        for (int i = 0; i < 8; i++)
        {
            float a = angle + i * MathHelper.TwoPi / 8f;
            var p = center + new Vector2(MathF.Cos(a), MathF.Sin(a)) * (radius + 2f);
            sb.FillRect(new Rectangle((int)p.X - 3, (int)p.Y - 3, 6, 6), color);
        }
    }

    public void Draw(SpriteBatch sb)
    {
        // Hızla artan hafif gövde titreşimi; buzda kızaklanırken daha sert sarsılır
        float shake = _grinding ? 2.5f : 1.3f;
        float bob = MathF.Sin(_time * (8f + 14f * SpeedRatio)) * shake * SpeedRatio;
        var o = new Point((int)Origin.X, (int)(Origin.Y + MathF.Round(bob)));

        void Box(int x, int y, int w, int h, Color c) => sb.FillRect(new Rectangle(o.X + x, o.Y + y, w, h), c);

        // ---------- Ek vagonlar (ana vagonun arkasında, solda) ----------
        DrawExtraWagons(sb, Box);

        // ---------- Yolcu ve paket vagonu ----------
        Box(30, -104, 8, 16, Iron);                // Kabin sobasının borusu
        Box(28, -106, 12, 3, BoilerBand);
        Box(6, -26, 178, 6, Iron);                 // Şasi
        Box(0, -80, 190, 56, WagonBody);           // Gövde
        for (int x = 22; x < 190; x += 24)
            Box(x, -78, 2, 52, WagonPlank);        // Tahta kaplama çizgileri
        Box(-4, -88, 198, 10, Roof);               // Çatı
        Box(-2, -92, 194, 5, Snow);                // Çatıdaki kar

        // Paket penceresi: içindeki gizemli paketin soluk ışığı (paket hasar gördükçe zayıflar ve titrer)
        Box(75, -70, 40, 14, new Color(14, 22, 26));
        Box(77, -68, 36, 10, PackageLight * (0.35f * PackagePulse()));

        // Yolcu pencereleri: her yolcu için bir pencere ısınıp yanar
        for (int i = 0; i < GameConfig.CabinCapacity; i++) // Ana vagonda 3 yatak; fazlası yolcu vagonunda
        {
            int wx = 12 + i * 20;
            if (i >= 1) wx = 125 + (i - 1) * 30; // Paket penceresinin iki yanına dağıt
            Box(wx, -70, 18, 14, new Color(14, 12, 12));
            if (i < _passengerCount)
                Box(wx + 2, -68, 14, 10, LanternLight * 0.5f);
        }

        Box(190, -36, 20, 6, Iron);                // Kaplin

        // ---------- Lokomotif ----------
        if (Skin.Model != Economy.TrainModel.Standard)
        {
            DrawModelLocomotive(sb, Box, o); // Özgün modeller: tamamen farklı gövde, baca ve teker düzeni
        }
        else
        {
            Box(205, -30, 280, 10, Iron);              // Ana şasi
            Box(210, -120, 90, 96, CabBody);           // Makinist kabini
            Box(202, -128, 106, 10, Roof);             // Kabin çatısı
            Box(204, -132, 102, 5, Snow);
            Box(226, -106, 50, 30, new Color(20, 14, 12)); // Kabin penceresi
            float lantern = 0.75f + 0.25f * MathF.Sin(_time * 9f) * MathF.Sin(_time * 3.3f);
            Box(228, -104, 46, 26, LanternLight * (0.55f * lantern)); // Arthur'un fener ışığı
            if (Pet != Economy.PetKind.None) // Evcil hayvan pencereden bakar
                UI.CabinDecor.DrawPet(sb, new Point(o.X + 232, o.Y - 78), Pet, 1, _time);

            Box(300, -90, 150, 60, Boiler);            // Kazan
            Box(300, -90, 150, 4, Skin.Shine);           // Ay ışığı yansıması
            Box(300, -62, 150, 3, Skin.Trim);           // Skin şeridi
            Box(0, -46, 190, 3, Skin.Trim);
            for (int x = 330; x < 450; x += 40)
                Box(x, -90, 5, 60, BoilerBand);        // Kazan çemberleri
            Box(350, -103, 30, 14, Boiler);            // Buhar domu
            Box(353, -107, 24, 4, Snow);
            Box(366, -66, 12, 8, _steamLevel > 0 ? new Color(200, 80, 50) : BoilerBand); // Termal vana
            Box(450, -95, 22, 70, Iron);               // Duman kutusu (ön yüz)

            Box(405, -135, 22, 45, Iron);              // Baca
            Box(399, -143, 34, 10, BoilerBand);        // Baca başlığı
            Box(440, -44, 30, 16, BoilerBand);         // Silindir

            Box(462, -108, 14, 12, HeadlightBroken ? new Color(40, 38, 34) : HeadlampLight * HeadlampFlicker()); // Far
            if (HeadlightBroken)
            {
                Box(465, -105, 3, 3, new Color(90, 86, 80)); // Kırık cam
                Box(470, -101, 3, 3, new Color(90, 86, 80));
            }

            // İnek mahmuzu (cowcatcher), basamak basamak alçalan üçgen
            Box(485, -30, 8, 14, BoilerBand);
            Box(493, -24, 8, 10, BoilerBand);
            Box(501, -18, 6, 6, BoilerBand);

            DrawStyleParts(Box);
        }
        DrawSeasonal(Box);

        // ---------- Tekerler (titreşimden bağımsız, raya sabit) ----------
        if (Skin.Model == Economy.TrainModel.Panzer)
        {
            DrawTreads(sb); // Paletli yürüyen aksam
        }
        else
        {
            foreach (var w in SmallWheels)
                DrawWheel(sb, Assets.WheelSmall, w, 14f);
            foreach (var w in DriveWheels)
                DrawWheel(sb, Assets.WheelLarge, w, 22f);

            // Tahrik tekerlerini bağlayan, tekerle birlikte dönen biyel kolu
            float angle = _wheelDistance / 22f;
            var pin = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 11f;
            int rodX = (int)(Origin.X + DriveWheels[0].X + pin.X);
            int rodY = (int)(Origin.Y + DriveWheels[0].Y + pin.Y);
            sb.FillRect(new Rectangle(rodX - 2, rodY - 2, DriveWheels[1].X - DriveWheels[0].X + 4, 4), new Color(110, 110, 120));
        }

        // Buhar ve kıvılcımlar gövdenin önünde
        _effects.Draw(sb);
    }

    /// <summary>Far konisi ve fener haleleri; diğer her şeyin üzerine eklenerek parlar.</summary>
    public void DrawLights(SpriteBatch sb)
    {
        var lamp = HeadlampPosition;
        float flicker = HeadlampFlicker();
        if (HeadlightBroken)
        {
            // Arızalı far: yalnızca ara sıra sönük bir kıvılcım
            if (MathF.Sin(_time * 37f) * MathF.Sin(_time * 11f) > 0.85f)
                sb.DrawGlow(lamp, 18f, HeadlampLight, 0.5f);
            sb.DrawGlow(new Vector2(Origin.X + 251, Origin.Y - 91), 60f, LanternLight, 0.25f);
            return;
        }

        // Gelişmiş Fener: daha uzun ve parlak bir koni
        float coneScale = HasLantern ? 1.6f : 1f;
        var coneTint = new Color(HeadlampLight.R, HeadlampLight.G, HeadlampLight.B, (byte)0) * ((HasLantern ? 0.4f : 0.28f) * flicker);
        sb.Draw(Assets.LightCone, lamp, null, coneTint, 0f, new Vector2(0, Assets.LightCone.Height / 2f),
            new Vector2(coneScale, HasLantern ? 1.35f : 1f), SpriteEffects.None, 0f);

        sb.DrawGlow(lamp, HasLantern ? 60f : 40f, HeadlampLight, 0.6f * flicker);
        sb.DrawGlow(new Vector2(Origin.X + 251, Origin.Y - 91), 60f, LanternLight, 0.25f);
        sb.DrawGlow(new Vector2(Origin.X + 95, Origin.Y - 63), 28f, PackageLight, 0.18f * PackagePulse());
        DrawSeasonalGlow(sb);
        if (_passengerCount > 0)
            sb.DrawGlow(new Vector2(Origin.X + 95, Origin.Y - 60), 90f, LanternLight, 0.06f * _passengerCount);
    }

    /// <summary>
    /// Işık haritasına (karanlık maskesi) trenin aydınlattığı alanları "deler":
    /// far konisi (arızalıysa yok), farın halesi, makinist feneri, dolu pencereler ve paketin ışığı.
    /// Maske silme karışımıyla çizildiği için renkler beyaz, şiddeti alfa belirler.
    /// </summary>
    public void DrawLightMask(SpriteBatch sb, int passengerCount)
    {
        float flicker = HeadlampFlicker();
        var lamp = HeadlampPosition;
        if (!HeadlightBroken)
        {
            // Görünen koniden daha uzun ve geniş: karanlıkta yolun bir parçası açılır
            var scale = HasLantern ? new Vector2(2.6f, 2.1f) : new Vector2(1.9f, 1.6f);
            sb.Draw(Assets.LightCone, lamp + new Vector2(-6, 0), null, Color.White * (0.95f * flicker), 0f,
                new Vector2(0, Assets.LightCone.Height / 2f), scale, SpriteEffects.None, 0f);
            DrawMaskGlow(sb, lamp, HasLantern ? 110f : 80f, 0.9f * flicker);
        }
        else if (MathF.Sin(_time * 37f) * MathF.Sin(_time * 11f) > 0.85f)
        {
            DrawMaskGlow(sb, lamp, 50f, 0.6f); // Arızalı farın kesik kıvılcımı
        }

        DrawMaskGlow(sb, new Vector2(Origin.X + 251, Origin.Y - 91), 95f, 0.75f);               // Makinist feneri
        DrawMaskGlow(sb, new Vector2(Origin.X + 95, Origin.Y - 63), 60f + 18f * passengerCount, 0.35f + 0.1f * passengerCount);
        if (HasPassengerWagon && passengerCount > GameConfig.CabinCapacity)
            DrawMaskGlow(sb, new Vector2(Origin.X - CouplerGap - PassengerWagonWidth / 2f, Origin.Y - 60), 80f, 0.45f);
        DrawMaskGlow(sb, new Vector2(Origin.X + 300, Origin.Y - 10), 160f, 0.25f); // Gövdenin kendi silüeti hafifçe seçilsin
        if (Season == Season.WinterFestival) // Şenlik ışıkları karanlıkta treni boydan boya aydınlatır
            foreach (var p in FestiveBulbs()) DrawMaskGlow(sb, Origin + new Vector2(p.X, p.Y + 2), 26f, 0.55f);
        else if (Season == Season.GhostWeek)
            DrawMaskGlow(sb, Origin + new Vector2(270, -137), 50f, 0.6f);
    }

    private static void DrawMaskGlow(SpriteBatch sb, Vector2 center, float radius, float strength)
    {
        var rect = new Rectangle((int)(center.X - radius), (int)(center.Y - radius), (int)(radius * 2), (int)(radius * 2));
        sb.Draw(Assets.Glow, rect, Color.White * strength);
    }

    // ------------------------------------------------------------------
    // Ek vagonlar
    // ------------------------------------------------------------------

    /// <summary>Satın alınan vagonlar: yolcu vagonu, kargo vagonu (yük doluluğuna göre sandıklar) ve zırhlı vagon.</summary>
    private void DrawExtraWagons(SpriteBatch sb, Action<int, int, int, int, Color> box)
    {
        int x = 0; // Ana vagonun sol kenarından sola doğru
        if (HasPassengerWagon)
        {
            x -= PassengerWagonWidth + CouplerGap;
            DrawPassengerWagon(box, x);
            DrawWagonWheels(sb, x, PassengerWagonWidth);
        }
        if (HasCargoWagon)
        {
            x -= CargoWagonWidth + CouplerGap;
            DrawCargoWagon(box, x);
            DrawWagonWheels(sb, x, CargoWagonWidth);
        }
        if (HasArmoredWagon)
        {
            x -= ArmoredWagonWidth + CouplerGap;
            DrawArmoredWagon(box, x);
            DrawWagonWheels(sb, x, ArmoredWagonWidth);
        }
    }

    private void DrawPassengerWagon(Action<int, int, int, int, Color> box, int x)
    {
        const int w = PassengerWagonWidth;
        box(x + w, -36, CouplerGap, 6, Iron);                     // Kaplin
        box(x + 6, -26, w - 12, 6, Iron);                         // Şasi
        box(x, -78, w, 54, WagonBody);
        for (int px = 18; px < w; px += 22) box(x + px, -76, 2, 50, WagonPlank);
        box(x - 4, -86, w + 8, 9, Roof);
        box(x - 2, -90, w + 4, 5, Snow);
        box(x, -46, w, 3, Skin.Trim);
        // Üç pencere: 4.-6. yolcular bu vagonda yolculuk eder
        for (int i = 0; i < GameConfig.PassengerWagonBeds; i++)
        {
            int wx = x + 16 + i * 48;
            box(wx, -70, 26, 16, new Color(14, 12, 12));
            if (_passengerCount > GameConfig.CabinCapacity + i)
                box(wx + 2, -68, 22, 12, LanternLight * 0.5f);
        }
    }

    private void DrawCargoWagon(Action<int, int, int, int, Color> box, int x)
    {
        const int w = CargoWagonWidth;
        box(x + w, -36, CouplerGap, 6, Iron);
        box(x + 4, -26, w - 8, 6, Iron);
        box(x, -42, w, 18, WagonPlank);                           // Açık platform
        box(x, -46, w, 4, Skin.Trim);
        box(x + 2, -64, 6, 22, Iron);                             // Ön-arka korkuluk
        box(x + w - 8, -64, 6, 22, Iron);

        // Yük doluluğuna göre üst üste sandıklar (en fazla 6)
        int crates = (int)MathF.Ceiling(MathHelper.Clamp(CargoFill, 0f, 1f) * 6f);
        for (int i = 0; i < crates; i++)
        {
            int col = i % 3, row = i / 3;
            int cx = x + 12 + col * 35, cy = -46 - 24 * (row + 1);
            box(cx, cy, 30, 22, new Color(120, 86, 52));
            box(cx, cy, 30, 3, new Color(150, 112, 70));
            box(cx + 13, cy, 4, 22, new Color(84, 60, 38));
            box(cx, cy - 3, 30, 3, Snow);
        }
        if (crates == 0)
            box(x + 20, -52, w - 40, 6, Snow); // Boş platformda kar
    }

    private void DrawArmoredWagon(Action<int, int, int, int, Color> box, int x)
    {
        const int w = ArmoredWagonWidth;
        var plate = new Color(58, 62, 70);
        var plateDark = new Color(40, 43, 50);
        box(x + w, -36, CouplerGap, 6, Iron);
        box(x + 4, -26, w - 8, 6, Iron);
        box(x, -84, w, 60, plate);
        box(x - 6, -84, 10, 50, plateDark);                       // Arkaya eğik zırh
        box(x + 8, -96, w - 16, 12, plateDark);                   // Taret
        box(x + w / 2 - 10, -104, 20, 8, plate);
        box(x + w / 2 + 6, -101, 34, 4, Iron);                    // Namlu
        for (int px = 8; px < w; px += 18)                        // Perçinler
        {
            box(x + px, -80, 3, 3, Skin.Shine);
            box(x + px, -32, 3, 3, Skin.Shine);
        }
        box(x + 16, -62, 36, 6, new Color(10, 10, 12));           // Atış delikleri
        box(x + 78, -62, 36, 6, new Color(10, 10, 12));
        box(x, -50, w, 3, Skin.Trim);
        box(x + 8, -100, w - 16, 4, Snow);
    }

    private static void DrawWagonWheels(SpriteBatch sb, int x, int width)
    {
        // Dönüş açısı paylaşılan tekerlerle aynı olsun diye örnek tekerin açısı kullanılır
        DrawStaticWheel(sb, new Point(x + 26, -14));
        DrawStaticWheel(sb, new Point(x + width - 26, -14));
    }

    private static float _sharedWheelAngle;

    private static void DrawStaticWheel(SpriteBatch sb, Point center)
    {
        var tex = Assets.WheelSmall;
        sb.Draw(tex, Origin + center.ToVector2(), null, Color.White, _sharedWheelAngle,
            new Vector2(tex.Width / 2f, tex.Height / 2f), 1f, SpriteEffects.None, 0f);
    }

    private void DrawSeasonalGlow(SpriteBatch sb)
    {
        if (Season == Season.WinterFestival)
        {
            int i = 0;
            foreach (var p in FestiveBulbs())
                sb.DrawGlow(Origin + new Vector2(p.X, p.Y + 2), 10f, BulbColor(i++), 0.5f);
        }
        else if (Season == Season.GhostWeek)
        {
            sb.DrawGlow(Origin + new Vector2(270, -137), 30f, new Color(255, 160, 50), 0.4f + 0.15f * MathF.Sin(_time * 11f));
        }
    }

    /// <summary>Paketin nabız gibi atan ışığı; bütünlük azaldıkça söner, kritikte kesik kesik yanar.</summary>
    private float PackagePulse()
    {
        float integrity = Package / GameConfig.MaxPackage;
        float pulse = (0.55f + 0.25f * MathF.Sin(_time * 1.7f)) * (0.3f + 0.7f * integrity);
        if (Package < GameConfig.LowPackageWarning && MathF.Sin(_time * 19f) * MathF.Sin(_time * 7f) > 0.2f)
            pulse *= 0.2f;
        return pulse;
    }

    /// <summary>Sağlıklı trende far hafifçe titrer; ağır hasarlı trende kesik kesik yanar.</summary>
    private float HeadlampFlicker()
    {
        if (Health < GameConfig.LowHealthWarning && MathF.Sin(_time * 31f) * MathF.Sin(_time * 17f) > 0.3f)
            return 0.25f;
        return 0.9f + 0.1f * MathF.Sin(_time * 23f);
    }

    private void DrawWheel(SpriteBatch sb, Texture2D texture, Point center, float radius)
    {
        float rotation = _wheelDistance / radius; // Kaymadan dönme: açı = yol / yarıçap
        var origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
        sb.Draw(texture, Origin + center.ToVector2(), null, Color.White, rotation, origin, 1f, SpriteEffects.None, 0f);
    }
}
