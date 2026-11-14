using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheLastDelivery.Graphics;

namespace TheLastDelivery.World;

/// <summary>
/// Ekran boyunca yağan kar taneleri. Tren hızlandıkça taneler yatayda savrulur ve
/// hız çizgisi gibi uzar; fırtına büyüdükçe kar yoğunlaşır, hızlanır ve çapraz yağar.
/// </summary>
public sealed class SnowSystem
{
    private struct Flake
    {
        public Vector2 Position;
        public float FallSpeed;
        public float Size;
        public float Depth;  // 0 = uzak, 1 = kameraya yakın
        public float Phase;  // Salınım için rastgele faz
    }

    private const float CalmDensity = 0.4f; // Sakin havada tanelerin yalnızca %40'ı görünür

    private readonly Flake[] _flakes;
    private readonly Random _rng = new();
    private readonly int _width;
    private readonly int _height;
    private float _horizontalSpeed;
    private int _activeCount;

    public SnowSystem(int count, int width, int height)
    {
        _width = width;
        _height = height;
        _flakes = new Flake[count];
        _activeCount = (int)(count * CalmDensity);
        for (int i = 0; i < count; i++)
            Respawn(ref _flakes[i], anywhere: true);
    }

    /// <param name="scrollSpeed">Zeminin saniyedeki kayma hızı (piksel/sn).</param>
    public void Update(float dt, float scrollSpeed, float time, Weather weather)
    {
        _activeCount = (int)(_flakes.Length * MathHelper.Lerp(CalmDensity, 1f, weather.Intensity));
        _horizontalSpeed = scrollSpeed + weather.WindSpeed;
        float fallBoost = 1f + scrollSpeed / 1500f + weather.Intensity * 0.8f;

        for (int i = 0; i < _flakes.Length; i++)
        {
            ref Flake f = ref _flakes[i];

            // Trenin hızı + fırtına rüzgarı + yumuşak salınım
            float wind = -scrollSpeed * (0.25f + 0.75f * f.Depth)
                         - weather.WindSpeed * (0.6f + 0.4f * f.Depth)
                         + MathF.Sin(time * 1.3f + f.Phase) * 12f;
            f.Position.X += wind * dt;
            f.Position.Y += f.FallSpeed * fallBoost * dt;

            if (f.Position.Y > _height + 10)
                Respawn(ref f, anywhere: false);
            else if (f.Position.X < -20)
                f.Position.X += _width + 40; // Sol kenardan çıkan tane sağdan geri girer
        }
    }

    /// <param name="lamp">Farın konumu; <paramref name="lampReach"/> 0 ise far sönük.</param>
    public void Draw(SpriteBatch spriteBatch, Vector2 lamp = default, float lampReach = 0f)
    {
        // Yatay hız arttıkça taneler uzar (hareket bulanıklığı hissi)
        float stretch = _horizontalSpeed / 240f;
        var warm = new Color(255, 236, 190);

        for (int i = 0; i < _activeCount; i++)
        {
            ref Flake f = ref _flakes[i];
            float alpha = 0.3f + 0.55f * f.Depth;
            var color = Color.White;

            // Far konisine giren taneler ışıkta parlar
            float dx = f.Position.X - lamp.X, dy = MathF.Abs(f.Position.Y - lamp.Y);
            if (lampReach > 0f && dx > 0f && dx < lampReach && dy < dx * 0.42f + 4f)
            {
                float lit = (1f - dx / lampReach) * (1f - dy / (dx * 0.42f + 4f));
                color = Color.Lerp(color, warm, 0.7f);
                alpha = MathF.Min(1f, alpha + 0.9f * lit);
            }

            if (f.Depth > 0.955f)
            {
                // Kameraya en yakın birkaç tane: iri, yumuşak pullar
                int size = 4 + (int)((f.Depth - 0.955f) * 60f);
                spriteBatch.Draw(Assets.Circle, new Rectangle((int)f.Position.X, (int)f.Position.Y, size + (int)MathF.Min(6f, stretch * 3f), size), color * (alpha * 0.4f));
            }
            else
            {
                int width = (int)MathF.Max(1f, f.Size * (1f + stretch * f.Depth));
                spriteBatch.Draw(Assets.Pixel, new Rectangle((int)f.Position.X, (int)f.Position.Y, width, (int)f.Size), color * alpha);
            }
        }
    }

    private void Respawn(ref Flake f, bool anywhere)
    {
        f.Depth = (float)_rng.NextDouble();
        f.Size = 1f + MathF.Round(f.Depth * 2.5f);
        f.FallSpeed = 30f + f.Depth * 70f + (float)_rng.NextDouble() * 20f;
        f.Phase = (float)_rng.NextDouble() * MathHelper.TwoPi;

        if (anywhere)
        {
            f.Position = new Vector2(_rng.Next(_width), _rng.Next(_height));
        }
        else
        {
            // Yatay hız yüksekken taneler sağ dışarıdan da gelsin ki sağ taraf boş kalmasın
            float extra = _horizontalSpeed * 0.6f;
            f.Position = new Vector2((float)_rng.NextDouble() * (_width + extra), -10f - _rng.Next(40));
        }
    }
}
