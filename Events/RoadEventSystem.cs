using System;
using System.Collections.Generic;
using TheLastDelivery.Core;
using TheLastDelivery.Items;
using TheLastDelivery.Localization;
using TheLastDelivery.Passengers;
using TheLastDelivery.Story;
using TheLastDelivery.World;

namespace TheLastDelivery.Events;

/// <summary>Bu karede olaylara verilen tepkiler (tuşlar Game1'de okunur).</summary>
public readonly record struct EventInput(bool Repair, bool Left, bool Right, bool Bait, bool Smoke);

/// <summary>
/// Sürüş sırasında rastgele aralıklarla olay başlatır, oyuncunun tepkisini değerlendirir
/// ve sonuçlarını (hasar, ısı kaybı, eşya) uygular.
/// Hangi olayın ne sıklıkla çıkacağını bölümün teması belirler (Weights).
/// Ses ve görsel tepkiler için Started / Finished olaylarını yayınlar.
/// </summary>
public sealed class RoadEventSystem
{
    private readonly Random _rng = new();
    private float _cooldown;
    private float _leakTimer;
    private float _smokeCooldown;

    /// <summary>Yeni bir olay başladığında tetiklenir.</summary>
    public event Action<RoadEvent> Started;

    /// <summary>Olay bittiğinde tetiklenir (true = oyuncu başarılı).</summary>
    public event Action<bool> Finished;

    /// <summary>Oyuncu maden girişinde durdu: mini oyun başlamalı.</summary>
    public event Action MineEntered;

    /// <summary>Kurt sürüsü kovuldu (başarım sayacı için).</summary>
    public event Action WolvesRepelled;

    /// <summary>Kurt sürüsüne duman püskürtüldü (görsel efekt için).</summary>
    public event Action SmokeBlasted;

    /// <summary>Arızalı fenere bir tamir vuruşu yapıldı (kıvılcım ve ses için). true = tamir tamamlandı.</summary>
    public event Action<bool> LampHit;

    public RoadEvent Active { get; private set; }

    /// <summary>Olaylar arası bekleme süresine uygulanan çarpan (küçük = sık).</summary>
    public float CooldownMultiplier { get; set; } = 1f;

    /// <summary>Olay türlerinin göreli ağırlıkları (bölüm temasından gelir).</summary>
    public EventWeights Weights { get; set; } = Chapters.WeightsFor(ChapterTheme.Plains);

    /// <summary>
    /// Vana patlaması ve kurt sürüsünün ağırlık çarpanı. Sakin bölümlerde 0.4 (%60 seyrek);
    /// kritik bölümde ya da aşırı fırtınada 1. Game1 her karede ayarlar.
    /// </summary>
    public float HazardScale { get; set; } = 1f;

    /// <summary>Gelişmiş Fener: tehlikeler daha erken görülür.</summary>
    public bool HasLantern { get; set; }

    /// <summary>Gece, tipi ya da tünel: kırık ray işareti fenersiz geç seçilir.</summary>
    public bool LowVisibility { get; set; }

    /// <summary>Kurt ve mafya saldırılarına uygulanan hasar çarpanı (Zırh Kaplama ile 0.5).</summary>
    public float AttackDamageMultiplier { get; set; } = 1f;

    /// <summary>Yük vagonundan alınan yağmalar (oyuncuya bildirim ve ses için).</summary>
    public event Action<IReadOnlyList<Economy.LootType>> LootCollected;

    /// <summary>Son olayın sonucu (birkaç saniye ekranda kalır).</summary>
    public string ResultMessage { get; private set; }
    public bool ResultSuccess { get; private set; }
    public float ResultTimer { get; private set; }

    /// <summary>Bu karede buzlu rayda hasar alınıyor mu? (kıvılcım efekti için)</summary>
    public bool IsGrinding { get; private set; }

    /// <summary>Patlamış vana hâlâ ısı kaçırıyor mu?</summary>
    public bool IsLeaking => _leakTimer > 0f;

    public bool IsOnIce => Active?.Type == RoadEventType.IcyRails;
    public bool IsValvePending => Active?.Type == RoadEventType.ValveLeak;
    public bool IsBlizzard => Active?.Type == RoadEventType.Blizzard;
    public bool IsAcidStorm => Active?.Type == RoadEventType.AcidStorm;

    public void Reset()
    {
        Active = null;
        _leakTimer = 0f;
        ResultTimer = 0f;
        IsGrinding = false;
        CooldownMultiplier = 1f;
        _cooldown = GameConfig.FirstEventDelay;
    }

    /// <summary>İstasyonda duruldu: aktif olay iptal, yeni olay için biraz bekle.</summary>
    public void Clear()
    {
        Active = null;
        _leakTimer = 0f;
        IsGrinding = false;
        _cooldown = MathF.Max(_cooldown, 8f);
    }

    /// <summary>Belirli bir olayı hemen başlatır (örn. hainin vanayı sabote etmesi). Başka olay sürüyorsa false.</summary>
    public bool ForceStart(RoadEventType type, Cabin cabin)
    {
        if (Active != null) return false;
        Begin(type, cabin);
        return true;
    }

    private bool _baitQueued;

    /// <summary>Envanterden '5' ile et atıldı: bir sonraki güncellemede [F] gibi işlenir.</summary>
    public void ThrowBait() => _baitQueued = true;

    /// <param name="allowNewEvents">Yeni olay başlatılabilir mi? (yanaşırken kapalı)</param>
    public void Update(float dt, Train train, Cabin cabin, Inventory inventory, EventInput input, bool allowNewEvents)
    {
        IsGrinding = false;
        ResultTimer = MathF.Max(0f, ResultTimer - dt);
        _smokeCooldown = MathF.Max(0f, _smokeCooldown - dt);

        // Patlamış vananın süregelen etkisi
        if (_leakTimer > 0f)
        {
            train.DrainHeat(GameConfig.LeakHeatDrainPerSecond * dt);
            _leakTimer -= dt;
        }

        if (Active == null)
        {
            if (!allowNewEvents) return;
            _cooldown -= dt;
            if (_cooldown <= 0f) StartRandomEvent(cabin);
            return;
        }

        if (_baitQueued)
        {
            input = input with { Bait = true };
            _baitQueued = false;
        }

        Active.Age += dt;
        switch (Active.Type)
        {
            case RoadEventType.IcyRails:     UpdateIcyRails(train, cabin, dt); break;
            case RoadEventType.ValveLeak:    UpdateValveLeak(input.Repair, train); break;
            case RoadEventType.SupplyCache:  UpdateSupplyCache(train, inventory); break;
            case RoadEventType.WolfPack:     UpdateWolfPack(dt, input, train, cabin, inventory); break;
            case RoadEventType.BrokenSwitch: UpdateBrokenSwitch(input, train); break;
            case RoadEventType.Blizzard:     UpdateBlizzard(dt, train); break;
            case RoadEventType.MineEntrance: UpdateMineEntrance(input.Repair, train); break;
            case RoadEventType.LampFailure:  UpdateLampFailure(input.Repair, train); break;
            case RoadEventType.AcidStorm:    UpdateAcidStorm(dt, train, cabin); break;
        }
    }

    private void StartRandomEvent(Cabin cabin)
    {
        var w = Weights with { Valve = Weights.Valve * HazardScale, Wolves = Weights.Wolves * HazardScale };
        // Fener arızası yalnızca karanlıkta (gece, tünel, tipi) anlamlıdır
        float lamp = LowVisibility ? GameConfig.LampFailureWeight : 0f;
        float roll = (float)_rng.NextDouble() * MathF.Max(0.001f, w.Total + lamp);
        RoadEventType type;
        if ((roll -= lamp) < 0f) type = RoadEventType.LampFailure;
        else if ((roll -= w.Cache) < 0f) type = RoadEventType.SupplyCache;
        else if ((roll -= w.Ice) < 0f) type = RoadEventType.IcyRails;
        else if ((roll -= w.Valve) < 0f) type = RoadEventType.ValveLeak;
        else if ((roll -= w.Wolves) < 0f) type = RoadEventType.WolfPack;
        else if ((roll -= w.Switch) < 0f) type = RoadEventType.BrokenSwitch;
        else if ((roll -= w.Blizzard) < 0f) type = RoadEventType.Blizzard;
        else if ((roll -= w.Acid) < 0f) type = RoadEventType.AcidStorm;
        else type = RoadEventType.MineEntrance;

        Begin(type, cabin);
    }

    private void Begin(RoadEventType type, Cabin cabin)
    {
        float lantern = HasLantern ? GameConfig.LanternExtraGrace : 0f;
        var radio = cabin.FindBySkill(PassengerSkill.RadioOperator);

        switch (type)
        {
            case RoadEventType.IcyRails:
            {
                // Radyocu buzlu bölgeyi telsizden önceden haber verir: daha uzun tepki süresi
                float extra = (radio != null ? GameConfig.RadioExtraGrace : 0f) + lantern;
                Active = new RoadEvent(type, GameConfig.IceDuration + extra, GameConfig.IceGracePeriod + extra, radio?.Name);
                break;
            }
            case RoadEventType.ValveLeak:
                // Yeşil bölge her arızada göstergenin orta-sağ kısmında farklı bir yerde
                Active = new RoadEvent(type, GameConfig.ValveReactionWindow, 0f, null)
                {
                    GreenStart = 0.42f + (float)_rng.NextDouble() * 0.2f
                };
                break;
            case RoadEventType.WolfPack:
                Active = new RoadEvent(type, GameConfig.WolfDuration, 0f, null) { BiteTimer = GameConfig.WolfBiteInterval };
                break;
            case RoadEventType.BrokenSwitch:
            {
                float extra = (radio != null ? GameConfig.RadioExtraGrace * 0.5f : 0f) + lantern;
                Active = new RoadEvent(type, GameConfig.SwitchWindow + extra, 0f, radio?.Name)
                {
                    BrokenBranch = _rng.Next(2),
                    HintDelay = LowVisibility && !HasLantern ? GameConfig.SwitchHintDelay : 0f
                };
                break;
            }
            case RoadEventType.Blizzard:
                Active = new RoadEvent(type, GameConfig.BlizzardDuration, 0f, radio?.Name);
                break;
            case RoadEventType.MineEntrance:
                Active = new RoadEvent(type, GameConfig.MineEntranceWindow + lantern, 0f, null);
                break;
            case RoadEventType.LampFailure:
                Active = new RoadEvent(type, GameConfig.LampFailureDuration, 0f, null);
                break;
            case RoadEventType.AcidStorm:
                Active = new RoadEvent(type, GameConfig.AcidStormDuration, 0f, radio?.Name);
                break;
            default:
                Active = new RoadEvent(type, GameConfig.SupplyCacheWindow + lantern, 0f, null);
                break;
        }

        _cooldown = (GameConfig.EventCooldownMin + (float)_rng.NextDouble() * (GameConfig.EventCooldownMax - GameConfig.EventCooldownMin))
                    * CooldownMultiplier * GameConfig.EventCooldownScale;
        Started?.Invoke(Active);
    }

    private void UpdateIcyRails(Train train, Cabin cabin, float dt)
    {
        if (!Active.InGracePeriod && train.Speed > GameConfig.IceSafeSpeed)
        {
            // Güvenli hızın ne kadar üstündeysek o kadar hasar
            float severity = (train.Speed - GameConfig.IceSafeSpeed) / (GameConfig.MaxSpeed - GameConfig.IceSafeSpeed);
            float damage = GameConfig.IceDamagePerSecond * (0.4f + 0.6f * severity) * cabin.DamageMultiplier;
            train.Damage(damage * dt);
            Active.TookDamage = true;
            IsGrinding = true;
        }

        if (Active.Age >= Active.Duration)
        {
            Finish(!Active.TookDamage, Loc.T(Active.TookDamage ? "res.ice.damaged" : "res.ice.clean"));
        }
    }

    /// <summary>
    /// Basınç ibresi mini oyunu: ibre yeşildeyken basılırsa vana kapanır.
    /// Yanlış anda basmak ya da süreyi kaçırmak vanayı patlatır: tren hasar alır ve kazan ısı kaçırır.
    /// </summary>
    private void UpdateValveLeak(bool repairPressed, Train train)
    {
        if (repairPressed)
        {
            if (Active.NeedleInGreen)
                Finish(true, Loc.T("res.valve.fixed"));
            else
                BurstValve(train);
        }
        else if (Active.Age >= Active.Duration)
        {
            BurstValve(train);
        }
    }

    private void BurstValve(Train train)
    {
        train.Damage(GameConfig.ValveBurstDamage);
        Finish(false, Loc.F("res.valve.burst", (int)GameConfig.ValveBurstDamage));
        _leakTimer = GameConfig.LeakDuration;
    }

    private void UpdateSupplyCache(Train train, Inventory inventory)
    {
        if (train.Speed < GameConfig.SupplyCacheSpeed)
        {
            // Vagon yağmalanır: 2-3 yağma eşyası kargoya (yer kalmadıysa geride kalır)
            var collected = new List<Economy.LootType>();
            int count = Difficulty.Scale(2 + _rng.Next(2)); // Acımasız Kış: yarı yarıya
            for (int i = 0; i < count; i++)
            {
                var loot = Economy.LootCatalog.Roll(_rng);
                if (inventory.Cargo.TryAdd(loot)) collected.Add(loot);
            }

            // Üçte bir ihtimalle kullanılabilir bir eşya da çıkar (nadir kit hariç)
            if (_rng.NextDouble() < 0.33 * Difficulty.Resources)
            {
                var items = new[] { ItemType.SpareParts, ItemType.ThermalCoal, ItemType.Medkit, ItemType.WolfBait };
                inventory.TryAdd(items[_rng.Next(items.Length)]);
            }

            if (collected.Count == 0)
            {
                Finish(false, Loc.T("res.cache.full"));
                return;
            }

            LootCollected?.Invoke(collected);
            Finish(true, Loc.F("res.cache.loot", collected.Count));
        }
        else if (Active.Age >= Active.Duration)
        {
            Finish(false, Loc.T("res.cache.missed"));
        }
    }

    // ------------------------------------------------------------------
    // Kurt sürüsü
    // ------------------------------------------------------------------

    /// <summary>
    /// Sürü trenin yanında koşar ve aralıklarla ısırır (gövde ve yolcular hasar alır).
    /// [F] et atmak sürüyü anında uzaklaştırır; [SHIFT] duman püskürtmek kazandan ısı harcayıp
    /// sürüyü korkutur; çok hızlı giden trene sürü yetişemez.
    /// </summary>
    private void UpdateWolfPack(float dt, EventInput input, Train train, Cabin cabin, Inventory inventory)
    {
        if (input.Bait)
        {
            if (inventory.TryConsume(ItemType.WolfBait))
            {
                Active.Aggression = 0f;
                WolvesRepelled?.Invoke();
                Finish(true, Loc.T("res.wolf.bait"));
                return;
            }
            ResultMessage = Loc.T("res.wolf.noBait");
            ResultSuccess = false;
            ResultTimer = 1.2f;
        }

        if (input.Smoke && _smokeCooldown <= 0f)
        {
            _smokeCooldown = 0.8f;
            if (train.Temperature >= GameConfig.WolfSmokeHeatCost * 2f)
            {
                train.DrainHeat(GameConfig.WolfSmokeHeatCost);
                Active.Aggression -= GameConfig.WolfSmokeScare;
                SmokeBlasted?.Invoke();
            }
            else
            {
                ResultMessage = Loc.T("res.wolf.cold");
                ResultSuccess = false;
                ResultTimer = 1.2f;
            }
        }

        // Hızlı giden trene sürü yetişemez
        if (train.Speed >= GameConfig.WolfOutrunSpeed)
            Active.Aggression -= 0.18f * dt;

        if (Active.Aggression <= 0f)
        {
            WolvesRepelled?.Invoke();
            Finish(true, Loc.T(train.Speed >= GameConfig.WolfOutrunSpeed ? "res.wolf.outrun" : "res.wolf.smoke"));
            return;
        }

        Active.BiteTimer -= dt;
        if (Active.BiteTimer <= 0f)
        {
            Active.BiteTimer = GameConfig.WolfBiteInterval;
            train.Damage(GameConfig.WolfBiteDamage * AttackDamageMultiplier);
            if (cabin.Count > 0)
                cabin.Passengers[_rng.Next(cabin.Count)].LoseVitality(GameConfig.WolfBiteVitality * AttackDamageMultiplier);
            Active.TookDamage = true;
        }

        if (Active.Age >= Active.Duration)
            Finish(!Active.TookDamage, Loc.T(Active.TookDamage ? "res.wolf.bitten" : "res.wolf.gone"));
    }

    // ------------------------------------------------------------------
    // Çökmüş ray makası
    // ------------------------------------------------------------------

    /// <summary>
    /// Hat ikiye ayrılır, kollardan biri çökmüştür. [A]/[←] sol, [D]/[→] sağ kola makas atar.
    /// Süre dolduğunda tren seçilen (seçilmediyse sol) kola girer; kırık kolsa ve tren yavaş değilse kaza.
    /// </summary>
    private void UpdateBrokenSwitch(EventInput input, Train train)
    {
        if (input.Left) Active.ChosenBranch = 0;
        if (input.Right) Active.ChosenBranch = 1;

        if (Active.Age < Active.Duration) return;

        int taken = Active.ChosenBranch < 0 ? 0 : Active.ChosenBranch;
        if (taken != Active.BrokenBranch)
        {
            Finish(true, Loc.T("res.switch.safe"));
        }
        else if (train.Speed <= GameConfig.SwitchSafeSpeed)
        {
            Finish(true, Loc.T("res.switch.slow"));
        }
        else
        {
            train.Damage(GameConfig.SwitchCrashDamage);
            train.SetSpeed(GameConfig.SwitchSafeSpeed);
            Finish(false, Loc.F("res.switch.crash", (int)GameConfig.SwitchCrashDamage));
        }
    }

    // ------------------------------------------------------------------
    // Tipi ve çığ
    // ------------------------------------------------------------------

    /// <summary>
    /// Tipi başlarken kazan ani soğuk yer (Game1, Started'da uygular). Tren güvenli hızın üstündeyken
    /// çığ riski birikir; risk dolarsa çığ treni gömer: ağır hasar, tren durur, kazan soğur.
    /// </summary>
    private void UpdateBlizzard(float dt, Train train)
    {
        if (train.Speed > GameConfig.AvalancheSafeSpeed)
        {
            float over = (train.Speed - GameConfig.AvalancheSafeSpeed) / (GameConfig.MaxSpeed - GameConfig.AvalancheSafeSpeed);
            Active.AvalancheRisk += GameConfig.AvalancheRiskRate * (0.35f + over) * dt;
        }
        else
        {
            Active.AvalancheRisk = MathF.Max(0f, Active.AvalancheRisk - 0.2f * dt);
        }

        if (Active.AvalancheRisk >= 1f)
        {
            train.Damage(GameConfig.AvalancheDamage);
            train.DrainHeat(GameConfig.AvalancheHeatLoss);
            train.Stop();
            Finish(false, Loc.F("res.blizzard.avalanche", (int)GameConfig.AvalancheDamage));
            return;
        }

        if (Active.Age >= Active.Duration)
            Finish(true, Loc.T("res.blizzard.passed"));
    }

    // ------------------------------------------------------------------
    // Maden girişi
    // ------------------------------------------------------------------

    private void UpdateMineEntrance(bool enterPressed, Train train)
    {
        if (enterPressed)
        {
            if (train.Speed <= GameConfig.MineEnterSpeed)
            {
                train.Stop();
                Finish(true, Loc.T("res.mine.enter"));
                MineEntered?.Invoke();
                return;
            }
            ResultMessage = Loc.F("res.mine.fast", (int)GameConfig.MineEnterSpeed);
            ResultSuccess = false;
            ResultTimer = 1.2f;
        }

        if (Active.Age >= Active.Duration)
            Finish(false, Loc.T("res.mine.missed"));
    }

    // ------------------------------------------------------------------
    // Fener arızası
    // ------------------------------------------------------------------

    /// <summary>
    /// Ampul patladı: far söner. Arthur [E] ile birkaç vuruşta yeni ampulü takar.
    /// Süre dolarsa tren zifiri karanlıkta yoldaki bir enkaza çarpar (hasar), ardından far yeniden yakılır.
    /// </summary>
    private void UpdateLampFailure(bool repairPressed, Train train)
    {
        train.HeadlightBroken = true;

        if (repairPressed)
        {
            Active.RepairHits++;
            bool done = Active.RepairHits >= GameConfig.LampRepairHits;
            LampHit?.Invoke(done);
            if (done)
            {
                train.HeadlightBroken = false;
                Finish(true, Loc.T("res.lamp.fixed"));
                return;
            }
        }

        if (Active.Age >= Active.Duration)
        {
            train.Damage(GameConfig.LampCrashDamage * AttackDamageMultiplier);
            train.HeadlightBroken = false;
            Finish(false, Loc.F("res.lamp.crash", (int)GameConfig.LampCrashDamage));
        }
    }

    // ------------------------------------------------------------------
    // Asit fırtınası (DLC Buzul Kıyameti)
    // ------------------------------------------------------------------

    /// <summary>
    /// Zehirli kar gövdeyi ve yolcuları aşındırır. Fırtınadan çıkmanın yolu hızdır:
    /// kaçış hızının üstünde fırtına iki kat hızlı geride kalır. Zırh hasarı azaltır.
    /// </summary>
    private void UpdateAcidStorm(float dt, Train train, Cabin cabin)
    {
        if (train.Speed >= GameConfig.AcidEscapeSpeed)
            Active.Age += dt; // Hızlı giden tren fırtına bulutundan daha çabuk çıkar

        train.Damage(GameConfig.AcidStormDrainPerSecond * AttackDamageMultiplier * dt);
        foreach (var p in cabin.Passengers)
            p.LoseVitality(GameConfig.AcidVitalityDrain * dt);
        Active.TookDamage = true;

        if (Active.Age >= Active.Duration)
            Finish(true, Loc.T("res.acid.passed"));
    }

    private void Finish(bool success, string message)
    {
        Active = null;
        ResultSuccess = success;
        ResultMessage = message;
        ResultTimer = GameConfig.EventResultDisplay;
        Finished?.Invoke(success);
    }
}
