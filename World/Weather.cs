using System;
using Microsoft.Xna.Framework;

namespace TheLastDelivery.World;

/// <summary>
/// Fırtına yoğunluğu. Güneye ilerledikçe hava sertleşir: gökyüzü fırtınalı griye döner,
/// kar daha yoğun ve çapraz yağar, kazan daha hızlı soğur. Üstüne dalgalı rüzgar esintileri eklenir.
/// </summary>
public sealed class Weather
{
    private const float BaseIntensity = 0.1f;
    private float _time;

    /// <summary>0 = sakin gece, 1 = tam fırtına.</summary>
    public float Intensity { get; private set; } = BaseIntensity;

    /// <summary>0-1 arası anlık esinti gücü.</summary>
    public float Gust { get; private set; }

    /// <summary>Kar ve dumanı sola savuran rüzgarın hızı (piksel/sn).</summary>
    public float WindSpeed => 25f + 170f * Intensity * (0.6f + 0.4f * Gust);

    public void Reset()
    {
        Intensity = BaseIntensity;
        _time = 0f;
    }

    /// <param name="journeyProgress">Yolculuğun tamamlanan oranı (0-1).</param>
    /// <param name="maxIntensity">Bölümün izin verdiği en yüksek yoğunluk (tünellerde ve erimede kar azdır).</param>
    public void Update(float dt, float journeyProgress, float maxIntensity = 1f)
    {
        _time += dt;

        // Yoğunluk hedefe yavaşça yaklaşır; ani sıçrama olmaz
        float target = MathHelper.Clamp(BaseIntensity + journeyProgress * 1.05f, 0f, maxIntensity);
        Intensity = MathHelper.Lerp(Intensity, target, MathHelper.Clamp(dt * 0.5f, 0f, 1f));

        Gust = 0.5f + 0.5f * MathF.Sin(_time * 0.7f) * MathF.Sin(_time * 0.23f + 1f);
    }
}
