using System;
using System.Collections.Generic;
using TheLastDelivery.Core;
using TheLastDelivery.World;

namespace TheLastDelivery.Passengers;

/// <summary>
/// Old Smokie'nin yolcu kabini (en fazla 3 yatak).
/// Yolcular kabini vücut ısılarıyla ısıtır ama treni ağırlaştırır.
/// </summary>
public sealed class Cabin
{
    private static readonly IReadOnlyList<Passenger> NoLosses = Array.Empty<Passenger>();

    private readonly List<Passenger> _passengers = new();

    public IReadOnlyList<Passenger> Passengers => _passengers;
    public int Count => _passengers.Count;

    /// <summary>Yatak sayısı: ana vagonda 3, Yolcu Vagonu ile +3.</summary>
    public int Capacity { get; set; } = GameConfig.CabinCapacity;

    public bool IsFull => Count >= Capacity;

    /// <summary>0-100 kabin sobası: zamanla söner, [C] ile kömür atılır. Yolcuların sıcaklığı buna bağlı.</summary>
    public float Stove { get; private set; }

    public void Clear()
    {
        _passengers.Clear();
        Stove = 60f;
    }

    public void StokeStove(float amount) => Stove = MathF.Min(100f, Stove + amount);

    /// <summary>En aç yolcu (tokluğu eşiğin altındaysa); yoksa null.</summary>
    public Passenger HungriestPassenger(float threshold = 80f)
    {
        Passenger hungriest = null;
        foreach (var p in _passengers)
            if (p.Hunger < threshold && (hungriest == null || p.Hunger < hungriest.Hunger))
                hungriest = p;
        return hungriest;
    }

    /// <summary>
    /// Soba yanar, yolcuların açlık ve sıcaklığı güncellenir. Kabin ısısı sobadan ve kazandan gelir;
    /// sönük sobalı kabin, kazan sıcak olsa bile serinler.
    /// </summary>
    public void UpdateNeeds(float dt, float boilerTemperature, float night = 0f, bool musicBox = false)
    {
        Stove = MathF.Max(0f, Stove - GameConfig.StoveBurnPerSecond * GameConfig.NeedsScale * dt);
        if (Count == 0) return;

        float cabinHeat = Math.Clamp(Stove * 0.75f + boilerTemperature * 0.35f, 0f, 100f);
        float decay = Has(PassengerSkill.Medic) ? 0.5f : 1f;
        foreach (var p in _passengers)
            p.UpdateNeeds(dt, cabinHeat, decay, night, musicBox);
    }

    public bool TryBoard(PassengerProfile profile)
    {
        if (IsFull) return false;
        _passengers.Add(new Passenger(profile));
        return true;
    }

    /// <summary>Yolcuyu trenden indirir (kovma / geride bırakma).</summary>
    public bool Remove(Passenger passenger) => _passengers.Remove(passenger);

    /// <summary>Bu kişi zaten trende mi? (Sonsuz modda aynı kasabaya yeniden gelindiğinde çift binişi önler.)</summary>
    public bool HasProfile(string id) => _passengers.Exists(p => p.Profile.Id == id);

    /// <summary>Henüz yakalanmamış, sabotaj yapabilen hainler.</summary>
    public IEnumerable<Passenger> ActiveTraitors()
    {
        foreach (var p in _passengers)
            if (p.IsTraitor && !p.IsRevealed) yield return p;
    }

    public Passenger FindBySkill(PassengerSkill skill) => _passengers.Find(p => p.Skill == skill);

    public bool Has(PassengerSkill skill) => FindBySkill(skill) != null;

    /// <summary>Tıbbi malzemeye en çok ihtiyaç duyan (canlılığı en düşük) yolcu; yoksa null.</summary>
    public Passenger MostCriticalPatient()
    {
        Passenger worst = null;
        foreach (var p in _passengers)
            if (p.NeedsCare && (worst == null || p.Vitality < worst.Vitality))
                worst = p;
        return worst;
    }

    /// <summary>Buz hasarına uygulanan çarpan (mühendis treni korur).</summary>
    public float DamageMultiplier => Has(PassengerSkill.Engineer) ? GameConfig.EngineerDamageMultiplier : 1f;

    /// <summary>Yolcu sayısı, yetenekler ve fırtına şiddetinden kaynak çarpanlarını hesaplar.</summary>
    public TravelModifiers GetModifiers(float stormIntensity)
    {
        float fuel = 1f + GameConfig.PassengerFuelIncrease * Count;
        float cooling = (1f - GameConfig.PassengerCoolingReduction * Count)
                        * (1f + GameConfig.StormCoolingBonus * stormIntensity);
        float heat = Has(PassengerSkill.Stoker) ? 1f + GameConfig.StokerHeatBonus : 1f;
        return new TravelModifiers(fuel, cooling, heat);
    }

    /// <summary>
    /// Kazan sıcaklığına göre yolcuların canlılığını günceller.
    /// </summary>
    /// <returns>Bu karede soğuğa yenik düşüp kabinden çıkarılan yolcular.</returns>
    public IReadOnlyList<Passenger> UpdateVitality(float dt, float boilerTemperature)
    {
        List<Passenger> lost = null;
        float decay = Has(PassengerSkill.Medic) ? 0.5f : 1f; // Hemşire yolcuları kollar
        for (int i = _passengers.Count - 1; i >= 0; i--)
        {
            var p = _passengers[i];
            p.UpdateVitality(dt, boilerTemperature, decay);
            if (!p.IsDead) continue;

            (lost ??= new List<Passenger>()).Add(p);
            _passengers.RemoveAt(i);
        }
        return lost ?? NoLosses;
    }

    /// <summary>
    /// İstasyona varışta: yolcuların durumu bir kademe iyileşir, mühendis treni onarır.
    /// </summary>
    /// <returns>Mühendisin onardığı sağlık miktarı (mühendis yoksa 0).</returns>
    public float OnArrival(Train train)
    {
        foreach (var passenger in _passengers)
            passenger.CompleteSegment();

        if (!Has(PassengerSkill.Engineer)) return 0f;

        float before = train.Health;
        train.Repair(GameConfig.EngineerStationRepair);
        return train.Health - before;
    }
}
