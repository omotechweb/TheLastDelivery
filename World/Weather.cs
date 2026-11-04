using System;
using Microsoft.Xna.Framework;

namespace TheLastDelivery.World;

/// <summary>Bölüm içinde değişen hava cephesi.</summary>
public enum WeatherFront { Calm, Snow, Squall, Fog, Clearing }

/// <summary>
/// Fırtına yoğunluğu. Güneye ilerledikçe hava sertleşir: gökyüzü fırtınalı griye döner,
/// kar daha yoğun ve çapraz yağar, kazan daha hızlı soğur. Üstüne dalgalı rüzgar esintileri eklenir.
/// Hikâye hatlarında bölüm boyunca hava cepheleri değişir (sakin, kar, sağanak tipi, sis, açılma):
/// yoğunluk cepheye göre yükselir ya da düşer, sis görüşü kapatır. Değişim bir kez bildirilir (FrontChanged).
/// </summary>
public sealed class Weather
{
    private const float BaseIntensity = 0.1f;
    private float _time;
    private Random _rng = new();
    private float _frontTimer = -1f;
    private float _frontScale = 1f, _frontScaleTarget = 1f;
    private float _fogTarget;
    private float _gapMin = 70f, _gapMax = 130f;

    /// <summary>0 = sakin gece, 1 = tam fırtına.</summary>
    public float Intensity { get; private set; } = BaseIntensity;

    /// <summary>0-1 arası anlık esinti gücü.</summary>
    public float Gust { get; private set; }

    /// <summary>0-1: sis yoğunluğu (görüşü kapatan gri perde).</summary>
    public float Fog { get; private set; }

    /// <summary>Şu anki hava cephesi.</summary>
    public WeatherFront Front { get; private set; } = WeatherFront.Snow;

    /// <summary>Bu karede başlayan yeni cephe (Game1 bir kez anons eder), yoksa null.</summary>
    public WeatherFront? FrontChanged { get; private set; }

    /// <summary>Kar ve dumanı sola savuran rüzgarın hızı (piksel/sn).</summary>
    public float WindSpeed => 25f + 170f * Intensity * (0.6f + 0.4f * Gust);

    public void Reset()
    {
        Intensity = BaseIntensity;
        _time = 0f;
        _frontTimer = -1f;
        _frontScale = _frontScaleTarget = 1f;
        Fog = _fogTarget = 0f;
        Front = WeatherFront.Snow;
        FrontChanged = null;
    }

    /// <summary>Yeni bölüm: cephe sırası bölüme göre tohumlanır. Cepheler kapalıysa (arcade modları) hava eski düzende kalır.</summary>
    public void StartChapter(int seed, bool fronts, float minGap, float maxGap)
    {
        _rng = new Random(seed * 131 + 7);
        _gapMin = minGap;
        _gapMax = maxGap;
        _frontTimer = fronts ? minGap * 0.6f + (float)_rng.NextDouble() * (maxGap - minGap) * 0.5f : -1f;
        if (!fronts)
        {
            _frontScaleTarget = 1f;
            _fogTarget = 0f;
            Front = WeatherFront.Snow;
        }
    }

    /// <param name="journeyProgress">Yolculuğun tamamlanan oranı (0-1).</param>
    /// <param name="maxIntensity">Bölümün izin verdiği en yüksek yoğunluk (tünellerde ve erimede kar azdır).</param>
    /// <param name="driving">Cepheler yalnızca tren yoldayken ilerler.</param>
    public void Update(float dt, float journeyProgress, float maxIntensity = 1f, bool driving = false)
    {
        _time += dt;
        FrontChanged = null;

        if (_frontTimer > 0f && driving)
        {
            _frontTimer -= dt;
            if (_frontTimer <= 0f) NextFront();
        }
        _frontScale += (_frontScaleTarget - _frontScale) * MathHelper.Clamp(dt * 0.25f, 0f, 1f);
        Fog += (_fogTarget - Fog) * MathHelper.Clamp(dt * 0.3f, 0f, 1f);

        // Yoğunluk hedefe yavaşça yaklaşır; ani sıçrama olmaz
        float target = MathHelper.Clamp((BaseIntensity + journeyProgress * 1.05f) * _frontScale, 0f, maxIntensity);
        Intensity = MathHelper.Lerp(Intensity, target, MathHelper.Clamp(dt * 0.5f, 0f, 1f));

        Gust = 0.5f + 0.5f * MathF.Sin(_time * 0.7f) * MathF.Sin(_time * 0.23f + 1f);
    }

    private void NextFront()
    {
        // Aynı cephe arka arkaya gelmez; sağanaktan sonra çoğunlukla açılma gelir
        WeatherFront next;
        do
        {
            double roll = _rng.NextDouble();
            next = Front == WeatherFront.Squall && roll < 0.6 ? WeatherFront.Clearing
                : roll < 0.22 ? WeatherFront.Calm
                : roll < 0.48 ? WeatherFront.Snow
                : roll < 0.70 ? WeatherFront.Squall
                : roll < 0.86 ? WeatherFront.Fog
                : WeatherFront.Clearing;
        } while (next == Front);

        Front = next;
        FrontChanged = next;
        (_frontScaleTarget, _fogTarget) = next switch
        {
            WeatherFront.Calm => (0.6f, 0f),
            WeatherFront.Squall => (1.55f, 0.1f),
            WeatherFront.Fog => (0.85f, 0.55f),
            WeatherFront.Clearing => (0.45f, 0f),
            _ => (1f, 0f)
        };
        _frontTimer = _gapMin + (float)_rng.NextDouble() * (_gapMax - _gapMin);
    }
}
