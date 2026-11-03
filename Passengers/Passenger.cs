using System;
using TheLastDelivery.Core;
using TheLastDelivery.Localization;

namespace TheLastDelivery.Passengers;

/// <summary>Yolcunun trene kattığı özel yetenek.</summary>
public enum PassengerSkill
{
    None,
    Engineer,       // İstasyonlarda treni onarır, buz hasarını azaltır
    RadioOperator,  // Buzlu rayları telsizden erken haber verir
    Stoker,         // Kazanı daha verimli besler
    Child,          // Mekanik etkisi yok; hikayenin sonunu değiştirir
    Medic,          // Yolcular soğukta yarı hızda zayıflar
    Guard,          // Mafya kovalamacasında haydutlar daha yavaş yaklaşır, daha az çalar
    Trader          // Benzinliklerde daha iyi fiyatlar
}

/// <summary>Yolcunun sağlık durumu (dilden bağımsız; ekranda çevirisi gösterilir).</summary>
public enum PassengerCondition
{
    Freezing,  // Donmak üzere
    Sick,      // Hasta
    Injured,   // Yaralı
    Warming,   // Isınıyor
    Stable,    // Stabil
    Poisoned   // Zehirlenmiş (hain sabotajı): yalnızca Nadir Sağlık Kiti iyileştirir
}

/// <summary>
/// Senaryolarda tanımlanan, değişmeyen yolcu bilgisi.
/// Ad ve meslek dile göre çeviri tablosundan gelir ("p.viktor.name" vb.).
/// </summary>
public sealed record PassengerProfile(string Id, PassengerCondition InitialCondition, PassengerSkill Skill)
{
    /// <summary>Trene binerken gizli hain/sabotajcı çıkma olasılığı (çocuk asla, yabancılar sık).</summary>
    public float TraitorChance { get; init; } = 0.1f;

    public string Name => Loc.T($"p.{Id}.name");
    public string Profession => Loc.T($"p.{Id}.prof");

    public string SkillTag => Skill switch
    {
        PassengerSkill.Engineer => Loc.T("skill.engineer"),
        PassengerSkill.RadioOperator => Loc.T("skill.radio"),
        PassengerSkill.Stoker => Loc.T("skill.stoker"),
        PassengerSkill.Child => Loc.T("skill.child"),
        PassengerSkill.Medic => Loc.T("skill.medic"),
        PassengerSkill.Guard => Loc.T("skill.guard"),
        PassengerSkill.Trader => Loc.T("skill.trader"),
        _ => string.Empty
    };
}

/// <summary>
/// Trendeki bir yolcu.
/// Durumu yol boyunca iyileşir (ilk durum → Isınıyor → Stabil), ama kazan
/// soğuduğunda canlılığı (Vitality) düşer; sıfıra inerse yolcu ölür.
/// </summary>
public sealed class Passenger
{
    private static readonly Random Rng = new();
    private int _segmentsRidden;

    public Passenger(PassengerProfile profile)
    {
        Profile = profile;
        IsTraitor = Rng.NextDouble() < profile.TraitorChance;
        Vitality = profile.InitialCondition switch
        {
            PassengerCondition.Freezing => 45f,
            PassengerCondition.Sick => 55f,
            _ => 60f
        };
    }

    public PassengerProfile Profile { get; }
    public string Name => Profile.Name;
    public string Profession => Profile.Profession;
    public PassengerSkill Skill => Profile.Skill;

    /// <summary>0-100 arası yaşam gücü.</summary>
    public float Vitality { get; private set; }

    public bool IsDead => Vitality <= 0f;

    // ------------------------------------------------------------------
    // Kabin yaşamı: açlık ve sıcaklık
    // ------------------------------------------------------------------

    /// <summary>0-100 tokluk (100 = tok). Zamanla azalır; erzakla dolar.</summary>
    public float Hunger { get; private set; } = 85f;

    /// <summary>0-100 vücut sıcaklığı. Kabin sobası ve kazan ısısına doğru yavaşça kayar.</summary>
    public float Warmth { get; private set; } = 70f;

    /// <summary>Uzun süre üşüyen yolcu hastalanır (tıbbi malzemeyle iyileşir).</summary>
    public bool IsSick { get; private set; }

    /// <summary>0-100 moral: gece, açlık ve soğuk düşürür; müzik kutusu, erzak ve taverna yükseltir.</summary>
    public float Morale { get; private set; } = 70f;

    /// <summary>Uzun süre aç ya da umutsuz kalan yolcu bir sonraki fırsatta treni terk eder.</summary>
    public bool WantsToLeave => _starvingTime >= GameConfig.DesertionDelay || _despairTime >= GameConfig.DesertionDelay;

    private float _despairTime;

    public void LiftMorale(float amount) => Morale = Math.Clamp(Morale + amount, 0f, 100f);

    /// <summary>Erzak / ısı isteği bildirildi mi? (aynı istek tekrar tekrar bildirilmesin)</summary>
    public bool HungerRequested { get; set; }

    private float _starvingTime;
    private float _freezingTime;

    /// <summary>Bir erzak paylaştırıldı.</summary>
    public void Feed(float amount)
    {
        Hunger = MathF.Min(100f, Hunger + amount);
        _starvingTime = 0f;
        HungerRequested = false;
        LiftMorale(8f); // Karnı doyan yolcunun yüzü güler
    }

    /// <summary>
    /// Açlık ve sıcaklığı günceller. Aç ya da üşüyen yolcunun canlılığı ayrıca düşer;
    /// uzun süre üşüyen hastalanır, uzun süre aç kalan treni terk etmek ister.
    /// </summary>
    /// <param name="cabinHeat">0-100: soba ve kazandan gelen kabin ısısı.</param>
    /// <param name="night">0-1 gecenin koyuluğu (karanlık moral bozar).</param>
    /// <param name="musicBox">Müzik kutusu çalıyor mu?</param>
    public void UpdateNeeds(float dt, float cabinHeat, float decayMultiplier, float night = 0f, bool musicBox = false)
    {
        // Moral: gece ve karşılanmayan ihtiyaçlar düşürür, müzik kutusu yükseltir
        float moraleDrain = night * GameConfig.MoraleNightDrain
                            + (Hunger < 30f ? GameConfig.MoraleNeedDrain : 0f)
                            + (Warmth < 30f ? GameConfig.MoraleNeedDrain : 0f);
        moraleDrain *= decayMultiplier * GameConfig.NeedsScale; // Uzun bölümlerde km başına aşınma aynı kalır
        if (musicBox) moraleDrain -= GameConfig.MusicBoxMoralePerSecond;
        Morale = Math.Clamp(Morale - moraleDrain * dt, 0f, 100f);
        if (Morale < GameConfig.MoraleDesertion) _despairTime += dt;
        else _despairTime = MathF.Max(0f, _despairTime - dt * 2f);

        Hunger = MathF.Max(0f, Hunger - GameConfig.HungerPerSecond * GameConfig.NeedsScale * dt);
        Warmth += Math.Sign(cabinHeat - Warmth) * MathF.Min(MathF.Abs(cabinHeat - Warmth), GameConfig.WarmthChangePerSecond * dt);

        float neglect = 0f;
        if (Hunger <= 0f) { _starvingTime += dt; neglect += 1f; }
        else _starvingTime = MathF.Max(0f, _starvingTime - dt * 2f);

        if (Warmth < 25f) { _freezingTime += dt; neglect += (25f - Warmth) / 25f; }
        else _freezingTime = MathF.Max(0f, _freezingTime - dt);

        if (_freezingTime >= GameConfig.SicknessDelay) IsSick = true;
        if (neglect > 0f)
            Vitality = MathF.Max(0f, Vitality - GameConfig.NeglectVitalityDrain * neglect * decayMultiplier * GameConfig.NeedsScale * dt);
    }

    // ------------------------------------------------------------------
    // İhanet: gizli hain, şüphe ve zehir
    // ------------------------------------------------------------------

    /// <summary>Gizli sabotajcı mı? Oyuncu bunu yalnızca ipuçlarından (şüphe) ya da suçüstünden öğrenir.</summary>
    public bool IsTraitor { get; }

    /// <summary>Hain suçüstü yakalandı ve bağlandı: artık sabotaj yapamaz, kabin panosunda "HAİN" yazar.</summary>
    public bool IsRevealed { get; private set; }

    /// <summary>0-100 arası şüphe: sabotajlardan sonra artar, zamanla söner.</summary>
    public float Suspicion { get; private set; }

    public bool IsPoisoned { get; private set; }

    public void AddSuspicion(float amount) => Suspicion = Math.Clamp(Suspicion + amount, 0f, 100f);

    public void Reveal()
    {
        IsRevealed = true;
        Suspicion = 100f;
    }

    public void Poison() => IsPoisoned = true;

    /// <summary>Nadir Sağlık Kiti: zehir ve hastalık dahil her şeyi tamamen iyileştirir.</summary>
    public void CureCompletely()
    {
        IsPoisoned = false;
        Heal();
        Warmth = MathF.Max(Warmth, 60f);
    }

    /// <summary>Canlılık kritik eşiğin altındaysa hangi aşamada olursa olsun donmak üzeredir.</summary>
    public PassengerCondition Condition
    {
        get
        {
            if (IsPoisoned) return PassengerCondition.Poisoned;
            if (Vitality < GameConfig.CriticalVitality || Warmth < 15f) return PassengerCondition.Freezing;
            if (IsSick) return PassengerCondition.Sick;
            return _segmentsRidden switch
            {
                0 => Profile.InitialCondition,
                1 => PassengerCondition.Warming,
                _ => PassengerCondition.Stable
            };
        }
    }

    /// <summary>Durumun aktif dildeki adı.</summary>
    public string ConditionText => Loc.T(Condition switch
    {
        PassengerCondition.Freezing => "cond.freezing",
        PassengerCondition.Sick => "cond.sick",
        PassengerCondition.Injured => "cond.injured",
        PassengerCondition.Warming => "cond.warming",
        PassengerCondition.Poisoned => "cond.poisoned",
        _ => "cond.stable"
    });

    /// <summary>Tıbbi malzemeyle iyileştirilmeye ihtiyacı var mı? (hasta, yaralı, donmak üzere ya da zayıf düşmüş)</summary>
    public bool NeedsCare => Condition != PassengerCondition.Stable || Vitality < 70f;

    /// <summary>Bir sonraki istasyona sağ salim varıldığında çağrılır.</summary>
    public void CompleteSegment() => _segmentsRidden++;

    /// <summary>Sığınakta dinlenmek: canlılık toparlanır ve durum bir kademe iyileşir.</summary>
    public void Rest(float amount)
    {
        Vitality = MathF.Min(100f, Vitality + amount);
        _segmentsRidden++;
    }

    /// <summary>Benzinlikte battaniyesini takas eden yolcu üşür.</summary>
    public void LoseVitality(float amount) => Vitality = MathF.Max(1f, Vitality - amount);

    /// <summary>Tıbbi malzeme: yolcuyu tamamen iyileştirir.</summary>
    public void Heal()
    {
        _segmentsRidden = Math.Max(_segmentsRidden, 2);
        Vitality = 100f;
        IsSick = false;
        _freezingTime = 0f;
    }

    /// <summary>Soğukta canlılık kaybı (henüz toparlanmamış yolcular daha hızlı üşür), sıcakta yavaş iyileşme.</summary>
    /// <param name="decayMultiplier">Hemşire trendeyken yolcular daha yavaş zayıflar.</param>
    public void UpdateVitality(float dt, float boilerTemperature, float decayMultiplier = 1f)
    {
        if (IsPoisoned)
            Vitality -= GameConfig.PoisonDrainPerSecond * decayMultiplier * dt;
        Suspicion = MathF.Max(0f, Suspicion - (IsRevealed ? 0f : 0.25f * dt)); // Şüphe zamanla söner

        float coldness = Math.Clamp((GameConfig.ColdThreshold - boilerTemperature) / GameConfig.ColdThreshold, 0f, 1f);
        if (coldness > 0f)
        {
            float rate = _segmentsRidden switch { 0 => 6f, 1 => 3f, _ => 1.5f };
            Vitality -= rate * coldness * decayMultiplier * GameConfig.NeedsScale * dt;
        }
        else if (boilerTemperature >= GameConfig.WarmThreshold)
        {
            Vitality = MathF.Min(100f, Vitality + GameConfig.VitalityRecoveryPerSecond * GameConfig.NeedsScale * dt);
        }
        Vitality = MathF.Max(0f, Vitality);
    }

    /// <summary>Örn: "Elena - Eski Radyocu (Donmak Üzere)"</summary>
    public override string ToString() => $"{Name} - {Profession} ({ConditionText})";
}
