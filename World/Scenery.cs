using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheLastDelivery.Core;
using TheLastDelivery.Graphics;
using TheLastDelivery.Story;

namespace TheLastDelivery.World;

/// <summary>Manzaranın bu karede çizileceği koşullar.</summary>
public readonly record struct SceneryView(
    float Phase,            // Günün 0-1 ilerleyişi (DayCycle.Phase)
    float Storm,            // 0-1 fırtına yoğunluğu
    float Time,             // Oyun saati (animasyonlar için)
    ChapterTheme Theme,
    bool Bodies,            // Ay ve güneş çizilsin mi (menü başlığının arkasında kalmasın)
    bool Roadside,          // Telgraf direkleri ve hat kenarı yapıları
    float AuroraBoost);     // Kutup ışığı çarpanı (Episode 2'nin Kutup Hattı'nda güçlü)

/// <summary>
/// Yol sahnesinin bütün arka planı: günün saatine göre renk değiştiren gökyüzü, yıldızlar, ay, güneş, kutup
/// ışıkları, bulutlar, beş parallax katmanı (uzak sıradağ, karlı dağlar, tepeler, uzak orman, yakın orman),
/// karlı zemin ve hattın kenarındaki telgraf direkleri. Katmanlar gri tonlu dokulardır; renkleri her karede
/// gökyüzü paletinden türetilir, böylece şafak, gündüz, günbatımı ve gece aynı dokularla çizilir.
/// </summary>
public sealed class Scenery : IDisposable
{
    private struct Layer
    {
        public Texture2D Texture;
        public float Speed;   // Zemine göre kayma oranı (0 = sonsuz uzak, 1 = zemin)
        public int Y;
        public float Depth;   // 0 = ufuk rengi, 1 = en koyu ön plan
        public float Offset;
    }

    private readonly record struct Palette(Color Top, Color Horizon, Color Far, Color Near, Color Ambient, Color Cloud);

    // Günün dört yüzü. Dünya donmuş: gündüz bile soluk, kurşuni bir aydınlık.
    private static readonly Palette Night = new(new(3, 5, 14), new(22, 32, 64), new(52, 66, 104), new(7, 9, 18), new(186, 198, 224), new(34, 42, 70));
    private static readonly Palette Dawn = new(new(26, 32, 70), new(222, 138, 112), new(176, 128, 138), new(26, 20, 34), new(246, 212, 204), new(236, 150, 130));
    private static readonly Palette Day = new(new(46, 68, 108), new(146, 168, 198), new(150, 170, 200), new(30, 40, 58), new(255, 255, 255), new(190, 204, 224));
    private static readonly Palette Dusk = new(new(28, 24, 62), new(232, 122, 78), new(170, 110, 118), new(22, 16, 30), new(248, 204, 186), new(240, 130, 96));
    private static readonly (float At, Palette Look)[] Keys =
    {
        (0.00f, Dawn), (0.10f, Day), (0.40f, Day), (0.50f, Dusk), (0.585f, Night), (0.93f, Night), (1.00f, Dawn)
    };

    private readonly Layer[] _layers = new Layer[5];
    private Texture2D _ground, _fade, _ray, _moon, _clouds;
    private float _groundOffset, _cloudDrift, _poleScroll, _shootTimer = 9f, _shootAge = -1f;
    private Vector2 _shootFrom;
    private readonly (Vector2 Position, float Size, float Twinkle, float Warmth)[] _stars = new (Vector2, float, float, float)[170];
    private readonly Random _rng = new(11);

    private const int PoleSpacing = 380;

    public void Load(GraphicsDevice gd)
    {
        int g = GameConfig.GroundY;
        _layers[0] = new Layer { Texture = SceneryTextures.CreateRange(gd, 1024, 190, seed: 3, peaks: 5, detail: 0f), Speed = 0.035f, Y = g - 176, Depth = 0.06f };
        _layers[1] = new Layer { Texture = SceneryTextures.CreateRange(gd, 1536, 300, seed: 9, peaks: 4, detail: 1f), Speed = 0.09f, Y = g - 282, Depth = 0.2f };
        _layers[2] = new Layer { Texture = SceneryTextures.CreateHills(gd, 1024, 120, seed: 21), Speed = 0.2f, Y = g - 104, Depth = 0.36f };
        _layers[3] = new Layer { Texture = SceneryTextures.CreateForest(gd, 1024, 96, seed: 31, minH: 26, maxH: 64, gap: 16, deadRatio: 0.12f), Speed = 0.38f, Y = g - 88, Depth = 0.62f };
        _layers[4] = new Layer { Texture = SceneryTextures.CreateForest(gd, 1280, 220, seed: 47, minH: 70, maxH: 172, gap: 124, deadRatio: 0.34f), Speed = 0.64f, Y = g - 212, Depth = 0.92f };

        _ground = SceneryTextures.CreateGround(gd, 1024, GameConfig.ScreenHeight - g, GameConfig.RailY - g, seed: 42);
        _fade = SceneryTextures.CreateFade(gd, 256, 1.5f);
        _ray = SceneryTextures.CreateAuroraRay(gd, 96);
        _moon = SceneryTextures.CreateMoon(gd, 30);
        _clouds = SceneryTextures.CreateCloudBand(gd, 640, 96, seed: 13);

        for (int i = 0; i < _stars.Length; i++)
        {
            float y = MathF.Pow((float)_rng.NextDouble(), 1.5f) * 430f; // Tepeye doğru sıklaşır
            _stars[i] = (new Vector2(_rng.Next(GameConfig.ScreenWidth), y), _rng.NextDouble() < 0.2 ? 2f : 1f,
                (float)_rng.NextDouble() * MathHelper.TwoPi, (float)_rng.NextDouble());
        }
    }

    /// <param name="scroll">Zeminin bu karede kaydığı piksel.</param>
    public void Update(float scroll, float dt, float wind)
    {
        for (int i = 0; i < _layers.Length; i++)
            _layers[i].Offset = Wrap(_layers[i].Offset + scroll * _layers[i].Speed, _layers[i].Texture.Width);
        _groundOffset = Wrap(_groundOffset + scroll, _ground.Width);
        _poleScroll += scroll;
        _cloudDrift += (4f + wind * 0.05f) * dt + scroll * 0.012f;

        // Arada bir kayan yıldız
        if (_shootAge >= 0f)
        {
            _shootAge += dt;
            if (_shootAge > 0.9f) _shootAge = -1f;
        }
        else if ((_shootTimer -= dt) <= 0f)
        {
            _shootTimer = 14f + (float)_rng.NextDouble() * 30f;
            _shootAge = 0f;
            _shootFrom = new Vector2(_rng.Next(300, 1200), _rng.Next(30, 200));
        }
    }

    private static float Wrap(float value, float size) => (value % size + size) % size;

    // ------------------------------------------------------------------
    // Palet
    // ------------------------------------------------------------------

    private static Color Mix(Color a, Color b, float t) => Color.Lerp(a, b, MathHelper.Clamp(t, 0f, 1f));

    private static Palette Blend(Palette a, Palette b, float t) => new(
        Mix(a.Top, b.Top, t), Mix(a.Horizon, b.Horizon, t), Mix(a.Far, b.Far, t),
        Mix(a.Near, b.Near, t), Mix(a.Ambient, b.Ambient, t), Mix(a.Cloud, b.Cloud, t));

    /// <summary>Günün saatine, temaya ve fırtınaya göre bu karenin paleti.</summary>
    private static Palette Look(in SceneryView view)
    {
        float p = view.Phase - MathF.Floor(view.Phase);
        var look = Night;
        for (int i = 0; i < Keys.Length - 1; i++)
        {
            if (p > Keys[i + 1].At) continue;
            look = Blend(Keys[i].Look, Keys[i + 1].Look, (p - Keys[i].At) / (Keys[i + 1].At - Keys[i].At));
            break;
        }

        switch (view.Theme)
        {
            case ChapterTheme.Night: // Kutup gecesi: güneş ufkun üstüne çıkamaz, gündüz bile alacakaranlık
                look = Blend(look, Night, 0.72f);
                break;
            case ChapterTheme.Thaw: // Erime: sıcak, sarımsı bir ışık
                look = Blend(look, new Palette(new(58, 60, 92), new(236, 190, 140), new(190, 170, 160), new(40, 34, 40),
                    new(255, 244, 226), new(250, 214, 170)), 0.4f);
                break;
            case ChapterTheme.AcidWaste: // Zehirli kar: yeşile çalan pus
                look = Blend(look, new Palette(new(18, 30, 16), new(128, 150, 60), new(96, 120, 70), new(14, 20, 10),
                    new(220, 240, 190), new(120, 150, 70)), 0.55f);
                break;
            case ChapterTheme.Blizzard:
                look = Blend(look, new Palette(new(52, 58, 70), new(120, 128, 142), new(124, 132, 148), new(40, 44, 54),
                    new(226, 232, 244), new(150, 156, 170)), 0.45f);
                break;
        }

        // Fırtına her şeyi kurşuni bir griye çeker
        var storm = new Palette(new(20, 21, 26), new(66, 68, 78), new(70, 73, 84), new(18, 19, 24), look.Ambient, new(74, 76, 86));
        return Blend(look, storm, view.Storm * 0.85f);
    }

    /// <summary>0-1: gökyüzünün ne kadar gece olduğu (yıldızlar ve kutup ışıkları için).</summary>
    public static float NightAmount(in SceneryView view)
    {
        float p = view.Phase - MathF.Floor(view.Phase);
        float night = MathHelper.Clamp((p - 0.5f) / 0.085f, 0f, 1f) * MathHelper.Clamp((1f - p) / 0.07f, 0f, 1f);
        return view.Theme == ChapterTheme.Night ? MathF.Max(night, 0.7f) : night;
    }

    /// <summary>Zeminde duran nesnelerin (duraklar, vagonlar) bu karedeki ortam ışığı.</summary>
    public static Color AmbientFor(in SceneryView view) => Look(view).Ambient;

    // ------------------------------------------------------------------
    // Çizim
    // ------------------------------------------------------------------

    public void Draw(SpriteBatch sb, in SceneryView view)
    {
        var look = Look(view);
        float night = NightAmount(view);
        float clear = MathHelper.Clamp(1f - view.Storm * 1.25f, 0f, 1f);
        int w = GameConfig.ScreenWidth, g = GameConfig.GroundY;

        // Gökyüzü: tepe rengi, üstüne ufka doğru koyulaşan ufuk rengi
        sb.FillRect(new Rectangle(0, 0, w, GameConfig.ScreenHeight), look.Top);
        sb.Draw(_fade, new Rectangle(0, 0, w, g + 10), look.Horizon);

        if (night * clear > 0.02f) DrawStars(sb, view, night * clear);
        if (night * clear > 0.05f) DrawAurora(sb, view, night * clear);
        if (view.Bodies) DrawBodies(sb, view, look, clear);
        DrawClouds(sb, view, look);

        // Katmanlar: uzaktakiler ufuk rengine, yakındakiler koyu silüete yaklaşır
        var haze = Mix(look.Horizon, look.Top, 0.25f);
        for (int i = 0; i < _layers.Length; i++)
        {
            ref Layer layer = ref _layers[i];
            var tint = Mix(look.Far, look.Near, MathF.Pow(layer.Depth, 0.8f));
            if (i == 2) tint = Mix(tint, Mix(look.Far, look.Ambient, 0.35f), 0.5f); // Tepeler kar yüzeyi: biraz daha aydınlık
            DrawTiled(sb, layer.Texture, layer.Offset, layer.Y, tint);

            // Kurt toprakları: iki sıra sık orman
            if (i == 4 && view.Theme == ChapterTheme.Wolves)
                DrawTiled(sb, layer.Texture, layer.Offset * 1.28f + 517f, layer.Y + 22, Mix(tint, look.Near, 0.6f));
            if (i == 2 && view.Theme == ChapterTheme.Mining && view.Roadside) DrawHeadframes(sb, layer.Offset, look);

            // Katmanların arasına giren pus (hava perspektifi); fırtınada yoğunlaşır
            float mist = (i == 0 ? 0.42f : i == 1 ? 0.3f : i == 4 ? 0.26f : 0.16f) + view.Storm * 0.3f;
            int reach = i == 4 ? 150 : 250; // Yakın ormanın dibinde alçak bir sis: tren silüeti ağaçlardan ayrılır
            sb.Draw(_fade, new Rectangle(0, g - reach, w, reach + 12), haze * MathHelper.Clamp(mist, 0f, 0.85f));
        }

        if (view.Roadside && view.Theme == ChapterTheme.IceRails) DrawFrozenLake(sb, view, look);
        DrawTiled(sb, _ground, _groundOffset, g, look.Ambient);
        if (view.Roadside)
        {
            if (view.Theme == ChapterTheme.IceRails) DrawBridge(sb, look);
            else if (view.Theme != ChapterTheme.Tunnels) DrawPoles(sb, look);
        }
    }

    /// <summary>Işık haritasında gökyüzünü karanlıktan çıkarır: tepede tam, ufka doğru sönen bir aydınlık.</summary>
    public void DrawSkyLight(SpriteBatch sb, float strength)
    {
        if (strength <= 0.01f) return;
        const int solid = 330, fade = 215;
        sb.FillRect(new Rectangle(-20, -20, GameConfig.ScreenWidth + 40, solid + 20), Color.White * strength);
        sb.Draw(_fade, new Rectangle(-20, solid, GameConfig.ScreenWidth + 40, fade), null, Color.White * strength, 0f, Vector2.Zero, SpriteEffects.FlipVertically, 0f);
    }

    private static void DrawTiled(SpriteBatch sb, Texture2D texture, float offset, int y, Color tint)
    {
        int x = -(int)(offset % texture.Width);
        for (; x < GameConfig.ScreenWidth; x += texture.Width)
            sb.Draw(texture, new Vector2(x, y), tint);
    }

    private void DrawStars(SpriteBatch sb, in SceneryView view, float strength)
    {
        foreach (var star in _stars)
        {
            float twinkle = 0.55f + 0.45f * MathF.Sin(view.Time * (0.7f + star.Warmth * 1.8f) + star.Twinkle);
            float low = MathHelper.Clamp((440f - star.Position.Y) / 160f, 0f, 1f); // Ufka yakın yıldızlar pusta söner
            var color = star.Warmth > 0.85f ? new Color(255, 214, 170) : star.Warmth < 0.15f ? new Color(170, 200, 255) : new Color(230, 236, 255);
            float a = strength * twinkle * low * (star.Size > 1f ? 1f : 0.9f);
            sb.FillRect(new Rectangle((int)star.Position.X, (int)star.Position.Y, (int)star.Size, (int)star.Size), color * a);
            if (star.Size > 1f) sb.DrawGlow(star.Position + Vector2.One, 7f, color, 0.35f * a);
        }

        if (_shootAge >= 0f)
        {
            float t = _shootAge / 0.9f;
            var head = _shootFrom + new Vector2(-260f, 110f) * t;
            for (int i = 0; i < 14; i++)
            {
                var p = head + new Vector2(13f, -5.5f) * i * 0.6f;
                sb.FillRect(new Rectangle((int)p.X, (int)p.Y, 3, 1), Color.White * (strength * (1f - t) * (1f - i / 14f)));
            }
        }
    }

    /// <summary>İki kutup ışığı perdesi: yeşil ana perde ve arkasında mor bir kuşak; dalgalanır, nabız gibi atar.</summary>
    private void DrawAurora(SpriteBatch sb, in SceneryView view, float strength)
    {
        strength *= view.AuroraBoost;
        if (strength <= 0.02f) return;
        float t = view.Time;
        const int step = 6;
        for (int band = 0; band < 2; band++)
        {
            var low = band == 0 ? new Color(70, 255, 150) : new Color(150, 90, 255);
            var high = band == 0 ? new Color(60, 200, 255) : new Color(255, 80, 190);
            float shift = band * 2.7f;
            for (int x = -step; x < GameConfig.ScreenWidth + step; x += step)
            {
                float u = x * 0.0042f + shift;
                float baseY = (band == 0 ? 250f : 190f) + 58f * MathF.Sin(u * 2.1f + t * 0.11f) + 24f * MathF.Sin(u * 5.3f - t * 0.19f);
                float curtain = 0.5f + 0.5f * MathF.Sin(u * 9f + t * 0.5f + MathF.Sin(u * 3f - t * 0.2f) * 2f);
                float height = (band == 0 ? 120f : 150f) + 90f * curtain * (0.6f + 0.4f * MathF.Sin(u * 23f + t * 0.9f));
                float glow = (0.35f + 0.65f * MathF.Pow(0.5f + 0.5f * MathF.Sin(u * 3.7f + t * 0.27f), 2f)) * (0.55f + 0.45f * curtain);
                var color = Color.Lerp(low, high, 0.5f + 0.5f * MathF.Sin(u * 1.7f + t * 0.07f));
                var tint = new Color(color.R, color.G, color.B, (byte)0) * (strength * glow * (band == 0 ? 0.5f : 0.3f));
                sb.Draw(_ray, new Rectangle(x, (int)(baseY - height), step, (int)height), tint);
            }
        }
    }

    private void DrawBodies(SpriteBatch sb, in SceneryView view, Palette look, float clear)
    {
        float p = view.Phase - MathF.Floor(view.Phase);

        // Güneş: gün boyu ufkun hemen üstünde alçak bir yay çizer (donmuş dünyanın soluk güneşi)
        float dayT = p > 0.9f ? (p - 0.96f) / 0.58f : (p + 0.04f) / 0.58f;
        if (dayT > -0.05f && dayT < 1.05f && view.Theme != ChapterTheme.Night)
        {
            var sun = new Vector2(MathHelper.Lerp(1120f, 160f, dayT), 452f - 200f * MathF.Sin(MathHelper.Pi * MathHelper.Clamp(dayT, 0f, 1f)));
            float low = 1f - MathF.Sin(MathHelper.Pi * MathHelper.Clamp(dayT, 0f, 1f));         // 1 = ufukta
            var color = Color.Lerp(new Color(255, 244, 226), new Color(255, 150, 90), low);
            sb.DrawGlow(sun, 420f, color, (0.2f + 0.3f * low) * clear);
            sb.DrawGlow(sun, 110f, color, 0.5f * clear);
            sb.Draw(Assets.Circle, new Rectangle((int)sun.X - 20, (int)sun.Y - 20, 40, 40), color * (0.9f * clear));
        }

        // Ay: gece boyunca yükselir ve batar
        float nightT = (p - 0.5f) / 0.5f;
        if (view.Theme == ChapterTheme.Night && nightT < 0f) nightT = 0.5f; // Kutup gecesinde ay hep tepede
        if (nightT > 0f && nightT < 1f)
        {
            float rise = MathF.Sin(MathHelper.Pi * nightT);
            var moon = new Vector2(MathHelper.Lerp(1060f, 240f, nightT), 430f - 318f * rise);
            float a = clear * MathHelper.Clamp(rise * 3f, 0f, 1f);
            sb.DrawGlow(moon, 230f, new Color(120, 150, 215), 0.3f * a);
            sb.DrawGlow(moon, 70f, new Color(190, 206, 240), 0.35f * a);
            sb.Draw(_moon, new Rectangle((int)moon.X - 30, (int)moon.Y - 30, 60, 60), new Color(226, 232, 246) * a);
        }
    }

    private void DrawClouds(SpriteBatch sb, in SceneryView view, Palette look)
    {
        float cover = 0.3f + 0.6f * view.Storm;
        // Yüksek, ince bulutlar ve ufka yakın kalın bir kuşak
        DrawCloudRow(sb, _cloudDrift * 0.6f, 36, 2, look.Cloud * (cover * 0.6f));
        DrawCloudRow(sb, _cloudDrift + 311f, 210, 3, Mix(look.Cloud, look.Horizon, 0.35f) * cover);
    }

    private void DrawCloudRow(SpriteBatch sb, float drift, int y, int scale, Color tint)
    {
        int width = _clouds.Width * scale;
        for (int x = -(int)Wrap(drift, width); x < GameConfig.ScreenWidth; x += width)
            sb.Draw(_clouds, new Rectangle(x, y, width, _clouds.Height * scale), tint);
    }

    // ------------------------------------------------------------------
    // Hat kenarı
    // ------------------------------------------------------------------

    private static float PoleHash(int index)
    {
        unchecked
        {
            uint h = (uint)(index * 747796405 + 2891336453u);
            h = (h ^ (h >> 15)) * 2246822519u;
            return ((h ^ (h >> 13)) & 0xFFFF) / 65535f;
        }
    }

    /// <summary>Hat boyunca telgraf direkleri: aralarında sarkan teller, bazıları eğilmiş, bazılarının teli kopmuş.</summary>
    private void DrawPoles(SpriteBatch sb, Palette look)
    {
        var wood = Mix(look.Near, look.Far, 0.12f);
        var wire = Mix(look.Near, look.Far, 0.3f);
        int baseY = GameConfig.GroundY + 9;
        int first = (int)MathF.Floor(_poleScroll / PoleSpacing) - 1;
        for (int index = first; index < first + 6; index++)
        {
            float x = index * PoleSpacing - _poleScroll + 140f;
            float kind = PoleHash(index);
            int height = 128 + (int)(PoleHash(index + 9000) * 14f);
            int lean = kind > 0.84f ? (kind > 0.92f ? 7 : -6) : 0;                 // Fırtınanın eğdiği direkler
            float topX = x + lean;
            int topY = baseY - height;

            // Bir sonraki direğe uzanan iki tel (kopuksa yere sarkar)
            float nextKind = PoleHash(index + 1);
            int nextHeight = 128 + (int)(PoleHash(index + 9001) * 14f);
            float nextTopX = x + PoleSpacing + (nextKind > 0.84f ? (nextKind > 0.92f ? 7 : -6) : 0);
            bool broken = PoleHash(index + 500) > 0.86f;
            for (int line = 0; line < 2; line++)
            {
                const int segments = 19;
                for (int s = 0; s < segments; s++)
                {
                    float t = (s + 0.5f) / segments;
                    if (broken && t > 0.42f) break;
                    float sag = broken ? 150f * t * t : (26f + line * 5f) * 4f * t * (1f - t);
                    float y = MathHelper.Lerp(topY, baseY - nextHeight, t) + 8 + line * 10 + sag;
                    sb.FillRect(new Rectangle((int)MathHelper.Lerp(topX, nextTopX, s / (float)segments), (int)y, PoleSpacing / segments + 1, 1), wire);
                }
            }

            if (x < -40f || x > GameConfig.ScreenWidth + 40f) continue;
            // Gövde (eğikse basamak basamak kayar), travers kolları ve izolatörler
            for (int i = 0; i < 8; i++)
                sb.FillRect(new Rectangle((int)(x + lean * (1f - i / 7f)) - 2, topY + i * height / 8, 4, height / 8 + 1), wood);
            sb.FillRect(new Rectangle((int)topX - 15, topY + 6, 30, 3), wood);
            sb.FillRect(new Rectangle((int)topX - 11, topY + 16, 22, 3), wood);
            sb.FillRect(new Rectangle((int)topX - 15, topY + 2, 2, 4), wire);
            sb.FillRect(new Rectangle((int)topX + 13, topY + 2, 2, 4), wire);
            sb.FillRect(new Rectangle((int)topX - 3, topY - 3, 6, 3), Mix(look.Ambient, look.Far, 0.4f)); // Tepede kar
            sb.FillRect(new Rectangle((int)x - 6, baseY - 4, 12, 5), Mix(look.Ambient, look.Far, 0.3f));  // Dipte kar yığını
        }
    }

    /// <summary>Maden tepeleri: sırtlarda terk edilmiş kuyu kuleleri (kafes gövde, makara, eğik destek).</summary>
    private static void DrawHeadframes(SpriteBatch sb, float hillOffset, Palette look)
    {
        var steel = Mix(look.Far, look.Near, 0.55f);
        const int spacing = 1024;
        for (float x = 700f - hillOffset; x < GameConfig.ScreenWidth + 100; x += spacing)
        {
            if (x < -100f) continue;
            int baseY = GameConfig.GroundY - 44, top = baseY - 92;
            sb.FillRect(new Rectangle((int)x - 12, top, 3, 92), steel);
            sb.FillRect(new Rectangle((int)x + 9, top, 3, 92), steel);
            for (int y = top + 8; y < baseY; y += 18) sb.FillRect(new Rectangle((int)x - 12, y, 24, 2), steel);
            for (int i = 0; i < 12; i++) sb.FillRect(new Rectangle((int)x + 12 + i * 4, top + 6 + i * 7, 4, 3), steel); // Eğik destek
            sb.Draw(Assets.Circle, new Rectangle((int)x - 9, top - 14, 18, 18), steel);
            sb.FillRect(new Rectangle((int)x - 30, baseY - 16, 22, 16), steel);                                          // Makine dairesi
            sb.FillRect(new Rectangle((int)x - 32, baseY - 19, 26, 3), Mix(look.Ambient, look.Far, 0.5f));
        }
    }

    /// <summary>Buz köprüleri: ufka uzanan donmuş göl; üstünde gökyüzünün soluk yansıması ve çatlaklar.</summary>
    private void DrawFrozenLake(SpriteBatch sb, in SceneryView view, Palette look)
    {
        int g = GameConfig.GroundY;
        var ice = Mix(Mix(look.Horizon, look.Ambient, 0.3f), look.Far, 0.4f);
        sb.FillRect(new Rectangle(0, g - 46, GameConfig.ScreenWidth, 60), ice);
        sb.Draw(_fade, new Rectangle(0, g - 46, GameConfig.ScreenWidth, 60), Mix(look.Far, look.Near, 0.35f) * 0.6f);
        for (int i = 0; i < 26; i++) // Buzdaki çatlaklar ve parıltılar (uzak olan yavaş kayar)
        {
            float depth = PoleHash(i + 70);
            float x = ((PoleHash(i) * 1600f - _poleScroll * (0.25f + 0.5f * depth)) % 1600f + 1600f) % 1600f - 160f;
            int y = g - 42 + (int)(depth * 44f);
            float glint = 0.25f + 0.2f * MathF.Sin(view.Time * 1.3f + i);
            sb.FillRect(new Rectangle((int)x, y, 20 + (int)(depth * 70f), 1), Mix(look.Ambient, Color.White, 0.5f) * glint);
        }
    }

    /// <summary>Buz köprüleri: rayın arkasından akıp geçen çelik kafes kiriş korkulukları.</summary>
    private void DrawBridge(SpriteBatch sb, Palette look)
    {
        var steel = Mix(look.Near, look.Far, 0.22f);
        var frost = Mix(look.Ambient, look.Far, 0.45f);
        const int bay = 128;
        int baseY = GameConfig.RailY + 2, top = baseY - 78;
        sb.FillRect(new Rectangle(0, top, GameConfig.ScreenWidth, 5), steel);
        sb.FillRect(new Rectangle(0, top - 2, GameConfig.ScreenWidth, 2), frost);
        float start = -(_poleScroll % bay) - bay;
        for (float x = start; x < GameConfig.ScreenWidth + bay; x += bay)
        {
            sb.FillRect(new Rectangle((int)x - 3, top, 6, 80), steel);
            for (int i = 0; i < 16; i++) // Çapraz bağlantılar
            {
                float t = i / 16f;
                sb.FillRect(new Rectangle((int)(x + t * bay), top + 4 + (int)(t * 72f), bay / 16 + 1, 3), steel);
                sb.FillRect(new Rectangle((int)(x + bay - t * bay - bay / 16f), top + 4 + (int)(t * 72f), bay / 16 + 1, 3), steel);
            }
        }
    }

    public void Dispose()
    {
        foreach (var layer in _layers) layer.Texture?.Dispose();
        _ground?.Dispose();
        _fade?.Dispose();
        _ray?.Dispose();
        _moon?.Dispose();
        _clouds?.Dispose();
    }
}
