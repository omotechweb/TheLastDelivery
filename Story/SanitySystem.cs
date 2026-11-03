using System;
using TheLastDelivery.Core;

namespace TheLastDelivery.Story;

/// <summary>Akıl sağlığı düşükken görülen sanrı türleri.</summary>
public enum Hallucination
{
    GhostTrain, // Arka planda sessizce süzülen yarı saydam hayalet tren
    FakeAmbush, // Alarm çalar, "PUSU!" uyarısı belirir... ama ortada kimse yoktur
    RedVision   // Ekran kızıla boğulur, kenarlardan karanlık sızar
}

/// <summary>
/// Arthur'un akıl sağlığı (0-100). Gece sürüşü, soğuk, açlık ve fener arızası onu aşındırır;
/// gündüz yavaşça, müzik kutusu, telsizdeki eski şarkılar ve tavernadaki sıcak çorba hızla toparlar.
/// %30'un altında sanrılar başlar ve giderek sıklaşır.
/// </summary>
public sealed class SanitySystem
{
    private readonly Random _rng = new();
    private float _hallucinationTimer;

    public float Sanity { get; private set; } = 100f;

    public bool IsHallucinating => Sanity < GameConfig.HallucinationThreshold;

    /// <summary>0 (sağlıklı) - 1 (tamamen tükenmiş): sanrıların şiddeti.</summary>
    public float Madness => Math.Clamp(1f - Sanity / GameConfig.HallucinationThreshold, 0f, 1f);

    /// <summary>Müzik kutusu çalıyor mu (kalan süre) ve yeniden çalınabilmesi için bekleme.</summary>
    public float MusicBoxTime { get; private set; }
    public float MusicBoxCooldown { get; private set; }
    public bool MusicBoxPlaying => MusicBoxTime > 0f;

    /// <summary>Bu karede başlayan sanrı (yoksa null). Game1 görsel ve ses tepkisini verir.</summary>
    public Hallucination? Triggered { get; private set; }

    public void Reset()
    {
        Sanity = 100f;
        MusicBoxTime = 0f;
        MusicBoxCooldown = 0f;
        _hallucinationTimer = 10f;
    }

    public void Restore(float amount) => Sanity = Math.Clamp(Sanity + amount, 0f, 100f);

    /// <summary>Aşınma çarpanı (Buzul Tarikatı'nın kutsaması: 0.7). Game1 ayarlar.</summary>
    public float DrainMultiplier { get; set; } = 1f;

    /// <summary>Müzik kutusunu kurar. Bekleme sürüyorsa false.</summary>
    public bool StartMusicBox()
    {
        if (MusicBoxCooldown > 0f) return false;
        MusicBoxTime = GameConfig.MusicBoxDuration;
        MusicBoxCooldown = GameConfig.MusicBoxCooldown * GameConfig.Pace;
        return true;
    }

    /// <param name="driving">Tren yolda mı? (Duraklarda ve kasabada akıl sağlığı aşınmaz.)</param>
    /// <param name="hungryPassengers">Kabinde aç yolcu var mı?</param>
    public void Update(float dt, bool driving, float night, bool lampBroken, bool boilerCold, bool hungryPassengers, bool radioMusic)
    {
        Triggered = null;
        MusicBoxCooldown = MathF.Max(0f, MusicBoxCooldown - dt);
        if (MusicBoxTime > 0f)
        {
            MusicBoxTime = MathF.Max(0f, MusicBoxTime - dt);
            Restore(GameConfig.MusicBoxSanityPerSecond * dt);
        }
        if (radioMusic) Restore(GameConfig.RadioMusicSanityPerSecond * dt);
        if (!driving) return;

        float drain = night * GameConfig.SanityNightDrain;
        if (lampBroken) drain += GameConfig.SanityLampDrain;
        if (boilerCold) drain += GameConfig.SanityColdDrain;
        if (hungryPassengers) drain += GameConfig.SanityHungerDrain;
        drain *= DrainMultiplier;
        if (drain <= 0f && !MusicBoxPlaying) drain = -GameConfig.SanityDayRecovery; // Aydınlık ve sakin: toparlanır
        Sanity = Math.Clamp(Sanity - drain * GameConfig.NeedsScale * dt, 0f, 100f); // km baÅÄ±na aÅÄ±nma tempodan baÄÄ±msÄ±z

        if (!IsHallucinating)
        {
            _hallucinationTimer = MathF.Max(_hallucinationTimer, 6f);
            return;
        }

        // Akıl sağlığı düştükçe sanrılar sıklaşır (15 sn'den 5 sn'ye)
        _hallucinationTimer -= dt;
        if (_hallucinationTimer > 0f) return;
        _hallucinationTimer = MathHelper(15f, 5f, Madness) + (float)_rng.NextDouble() * 4f;
        Triggered = (Hallucination)_rng.Next(3);
    }

    private static float MathHelper(float from, float to, float t) => from + (to - from) * t;
}
