using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheLastDelivery.Graphics;

namespace TheLastDelivery.World;

/// <summary>Tek bir parçacığın tüm durumu (duman, buhar, kıvılcım).</summary>
public struct Particle
{
    public Vector2 Position;
    public Vector2 Velocity;
    public float Age;
    public float Life;
    public float Size;          // Başlangıç yarıçapı (piksel)
    public float Growth;        // Ömrü boyunca boyutun kaç katı büyüyeceği
    public Color Color;
    public float Alpha;         // Başlangıç saydamlığı
    public float WindInfluence; // 0 = rüzgardan etkilenmez, 1 = tamamen sürüklenir
    public float Gravity;       // Pozitif değer aşağı çeker (kıvılcımlar)
    public bool Soft;           // Yumuşak daire mi, keskin piksel mi
    public bool Additive;       // Işık gibi parlasın mı
}

/// <summary>
/// Basit, genel amaçlı parçacık sistemi. Trenin geride bıraktığı hava akımı
/// (rüzgar hızı) parçacıkları sola doğru sürükler.
/// </summary>
public sealed class ParticleSystem
{
    private readonly List<Particle> _particles = new();

    public void Emit(in Particle particle) => _particles.Add(particle);

    public void Clear() => _particles.Clear();

    /// <param name="windVelocityX">Parçacıkların yaklaşmaya çalıştığı yatay hız (piksel/sn).</param>
    public void Update(float dt, float windVelocityX)
    {
        for (int i = _particles.Count - 1; i >= 0; i--)
        {
            var p = _particles[i];
            p.Age += dt;
            if (p.Age >= p.Life)
            {
                _particles.RemoveAt(i);
                continue;
            }

            if (p.WindInfluence > 0f)
            {
                p.Velocity.X = MathHelper.Lerp(p.Velocity.X, windVelocityX, MathHelper.Clamp(dt * 1.5f * p.WindInfluence, 0f, 1f));
                p.Velocity.Y *= 1f - dt * 0.6f; // Yükselen duman zamanla yavaşlar
            }
            p.Velocity.Y += p.Gravity * dt;
            p.Position += p.Velocity * dt;
            _particles[i] = p;
        }
    }

    public void Draw(SpriteBatch sb)
    {
        foreach (var p in _particles)
        {
            float t = p.Age / p.Life;
            float size = p.Size * (1f + t * p.Growth);
            float alpha = (1f - t) * p.Alpha;

            var color = p.Additive ? new Color(p.Color.R, p.Color.G, p.Color.B, (byte)0) : p.Color;
            var rect = new Rectangle((int)(p.Position.X - size), (int)(p.Position.Y - size),
                (int)MathHelper.Max(1f, size * 2f), (int)MathHelper.Max(1f, size * 2f));
            sb.Draw(p.Soft ? Assets.Circle : Assets.Pixel, rect, color * alpha);
        }
    }
}
