using System;
using TheLastDelivery.Core;

namespace TheLastDelivery.Chase;

/// <summary>
/// Mafya pusularının ne zaman kurulacağına karar verir: rastgele bekleme süresi,
/// bölüm başına en fazla Chapter.MaxAmbushes pusu (Bölüm 2'nin tünelleri pusu doludur).
/// </summary>
public sealed class AmbushScheduler
{
    private readonly Random _rng = new();
    private float _cooldown;
    private int _count;
    private int _limit = 1;

    public void Reset() => StartChapter(1, 1f);

    /// <summary>
    /// Yeni bölüm: sayaç sıfırlanır, ilk pusu bölüm sıklığına göre zamanlanır.
    /// Kritik bölümde pusu kesindir ve bölümün hemen başında kurulur.
    /// </summary>
    public void StartChapter(int maxAmbushes, float frequency, bool critical = false)
    {
        _count = 0;
        _limit = maxAmbushes;
        _cooldown = critical
            ? GameConfig.CriticalAmbushDelayMin + (float)_rng.NextDouble() * (GameConfig.CriticalAmbushDelayMax - GameConfig.CriticalAmbushDelayMin)
            : RandomCooldown(frequency) * 0.6f; // İlk pusu biraz daha erken
    }

    /// <param name="allowed">Şu an pusu kurulabilir mi? (sürüşte, durağa uzak, başka olay yokken)</param>
    /// <param name="frequencyMultiplier">Bekleme çarpanı (küçük → daha sık).</param>
    /// <returns>Bu karede pusu başlamalıysa true.</returns>
    public bool Update(float dt, bool allowed, float frequencyMultiplier)
    {
        if (_count >= _limit) return false;

        _cooldown -= dt;
        if (_cooldown > 0f || !allowed) return false;

        _count++;
        _cooldown = RandomCooldown(frequencyMultiplier);
        return true;
    }

    private float RandomCooldown(float multiplier) =>
        (GameConfig.AmbushCooldownMin + (float)_rng.NextDouble() * (GameConfig.AmbushCooldownMax - GameConfig.AmbushCooldownMin)) * multiplier * GameConfig.AmbushCooldownScale;
}
