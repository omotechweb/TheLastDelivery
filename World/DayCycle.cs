using System;
using Microsoft.Xna.Framework;
using TheLastDelivery.Core;

namespace TheLastDelivery.World;

/// <summary>
/// Gece/gündüz döngüsü. Her yolculuk sabah başlar; günün sonuna doğru gece çöker.
/// Gece ekran kararır (Gelişmiş Fener bunu azaltır) ve gizli hainler sabotaja girişir.
/// </summary>
public sealed class DayCycle
{
    private float _clock;

    /// <summary>Günün 0-1 arası ilerleyişi.</summary>
    public float Phase => _clock / GameConfig.DayLength % 1f;

    /// <summary>0 = gündüz, 1 = koyu gece (alacakaranlıkta yumuşak geçiş).</summary>
    public float Night
    {
        get
        {
            float p = Phase;
            const float fade = 0.06f;
            if (p < GameConfig.NightStart - fade || p > GameConfig.NightEnd + fade) return 0f;
            float rise = MathHelper.Clamp((p - (GameConfig.NightStart - fade)) / (2f * fade), 0f, 1f);
            float fall = MathHelper.Clamp(((GameConfig.NightEnd + fade) - p) / (2f * fade), 0f, 1f);
            return MathF.Min(rise, fall);
        }
    }

    public bool IsNight => Night > 0.5f;

    public void Reset() => _clock = GameConfig.DayLength * 0.1f; // Sabah

    public void Update(float dt) => _clock += dt / GameConfig.DayScale; // HikÃ¢ye hatlarÄ±nda gÃ¼n daha uzun sÃ¼rer
}
