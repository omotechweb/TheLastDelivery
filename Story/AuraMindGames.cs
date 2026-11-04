using System;
using TheLastDelivery.Core;

namespace TheLastDelivery.Story;

/// <summary>
/// AURA'nın zihin oyunları (psikolojik korku katmanı). AURA yalnızca otonom tren yollamaz:
///  - Akıl sağlığı düştükçe kabindeki göstergeleri manipüle eder (kazan, hız ve yakıt ibreleri sapar, titrer).
///  - Mini haritaya sahte duraklar, telsize sahte imdat ve "ölü yakınların" sesini taklit eden yayınlar düşürür.
///  - Sürüşün arasına kendi fısıltılarını serpiştirir.
/// Episode 2'nin her sürüş bölümünde, Episode 1'de ise 30. bölümden sonra ("bilinmeyen sinyal") etkindir.
/// Müzik kutusu ya da telsizdeki şarkılar akıl sağlığını toparladıkça etki zayıflar.
/// </summary>
public sealed class AuraMindGames
{
    private readonly Random _rng = new();
    private float _tempLie, _speedLie, _fuelLie;
    private float _tempTarget, _speedTarget, _fuelTarget;
    private float _retarget;
    private float _phantomLife;
    private float _whisperTimer;
    private float _radioTimer;
    private int _whispers;

    /// <summary>0-1: manipülasyonun gücü (akıl sağlığı ve bölüme göre).</summary>
    public float Strength { get; private set; }

    public bool Active => Strength > 0.01f;

    /// <summary>Mini haritadaki sahte durağın konumu (yoksa -1).</summary>
    public float PhantomKm { get; private set; } = -1f;

    /// <summary>Telsizde AURA'nın ses taklidi yayını var mı? (Game1 kanalı ekler/kaldırır.)</summary>
    public bool PhantomRadio { get; private set; }

    /// <summary>Bu karede doğan olay (fısıltı ya da gösterge uyarısı); Game1 bir kez gösterir. Yoksa null.</summary>
    public string Event { get; private set; }

    public float TemperatureLie => _tempLie;
    public float SpeedLie => _speedLie;
    public float FuelLie => _fuelLie;

    public void Reset()
    {
        _tempLie = _speedLie = _fuelLie = 0f;
        _tempTarget = _speedTarget = _fuelTarget = 0f;
        _retarget = 0f;
        PhantomKm = -1f;
        _phantomLife = 0f;
        PhantomRadio = false;
        _whisperTimer = 40f;
        _radioTimer = 30f;
        _whispers = 0;
        Strength = 0f;
        Event = null;
    }

    /// <summary>Yeni bölüm: sahte durak ve sahte yayın silinir, fısıltı sayacı sıfırlanır.</summary>
    public void StartChapter()
    {
        PhantomKm = -1f;
        PhantomRadio = false;
        _whispers = 0;
        _whisperTimer = 35f + (float)_rng.NextDouble() * 25f;
        _radioTimer = 25f + (float)_rng.NextDouble() * 20f;
    }

    /// <summary>Bölümde AURA'nın elinin uzandığı yer: Episode 2'nin tamamı, Episode 1'in son on bölümü.</summary>
    public static bool Reaches(int episode, int chapter) => episode == 2 || chapter >= 30;

    /// <param name="driving">Tren yolda mı (manipülasyon yalnızca sürüşte).</param>
    /// <param name="sanity">Arthur'un akıl sağlığı (0-100).</param>
    /// <param name="stance">AURA'nın tavrı: düşmanca davrandıkça oyunları sertleşir.</param>
    public void Update(float dt, bool driving, int episode, int chapter, float sanity, float km, float stance)
    {
        Event = null;
        bool reach = Reaches(episode, chapter);
        float target = 0f;
        if (reach && driving)
        {
            float pressure = (GameConfig.MindGameSanity - sanity) / GameConfig.MindGameSanity;   // 65'in altında başlar
            float hostility = Math.Clamp(-stance / 100f, 0f, 1f) * 0.25f;                      // Düşman AURA daha acımasız
            target = Math.Clamp(pressure + hostility, 0f, 1f);
            if (episode == 1) target *= 0.6f;                                                  // Episode 1'de yalnızca yankı
        }
        Strength += (target - Strength) * Math.Clamp(dt * 0.8f, 0f, 1f);

        // Göstergeler: yeni sapma hedefleri, yumuşak kayma ve sanrı anlarında titreme
        _retarget -= dt;
        if (_retarget <= 0f)
        {
            _retarget = 3f + (float)_rng.NextDouble() * 5f;
            float s = Strength;
            _tempTarget = s < 0.15f ? 0f : (float)(_rng.NextDouble() * 2 - 1) * 32f * s;
            _speedTarget = s < 0.25f ? 0f : (float)(_rng.NextDouble() * 2 - 1) * 24f * s;
            _fuelTarget = s < 0.3f ? 0f : (float)(_rng.NextDouble() * 2 - 1) * 0.28f * s;
            if (s > 0.35f && _rng.NextDouble() < 0.3) Event = "mind.gauge";
        }
        float follow = Math.Clamp(dt * 1.5f, 0f, 1f);
        float jitter = Strength > 0.6f ? MathF.Sin(km * 900f) * 3f * Strength : 0f;
        _tempLie += (_tempTarget - _tempLie) * follow;
        _speedLie += (_speedTarget - _speedLie) * follow + jitter * 0.2f;
        _fuelLie += (_fuelTarget - _fuelLie) * follow;
        if (!driving)
        {
            _tempLie *= 0.9f;
            _speedLie *= 0.9f;
            _fuelLie *= 0.9f;
        }

        // Sahte durak: mini haritada ileride bir "istasyon" belirir, yaklaşınca kaybolur
        if (PhantomKm > 0f)
        {
            _phantomLife -= dt;
            if (_phantomLife <= 0f || km >= PhantomKm - GameConfig.Near(1f))
            {
                PhantomKm = -1f;
                if (driving && Strength > 0.2f) Event = "mind.phantomGone";
            }
        }
        else if (driving && Strength > 0.4f && _rng.NextDouble() < dt * 0.02 * Strength)
        {
            PhantomKm = km + GameConfig.Near(12f + (float)_rng.NextDouble() * 18f);
            _phantomLife = 30f;
        }

        // Ses taklidi yayını: akıl sağlığı düştükçe telsizde ölülerin sesleri belirir
        _radioTimer -= dt;
        if (_radioTimer <= 0f)
        {
            _radioTimer = 35f + (float)_rng.NextDouble() * 30f;
            bool want = driving && Strength > 0.3f;
            if (want && !PhantomRadio) Event = "mind.radio";
            PhantomRadio = want;
        }

        // Fısıltılar: bölüm başına en fazla üç
        if (driving && Strength > 0.2f && _whispers < 3)
        {
            _whisperTimer -= dt * (0.5f + Strength);
            if (_whisperTimer <= 0f)
            {
                _whisperTimer = 45f + (float)_rng.NextDouble() * 40f;
                _whispers++;
                Event = "mind.whisper";
            }
        }
    }

    /// <summary>Telsizdeki ses taklidi yanıtlandı: yayın susar (bir sonraki fırsata kadar).</summary>
    public void SilenceRadio()
    {
        PhantomRadio = false;
        _radioTimer = 60f;
    }
}
