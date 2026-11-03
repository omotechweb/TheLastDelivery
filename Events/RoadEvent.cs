using TheLastDelivery.Core;
using TheLastDelivery.Localization;

namespace TheLastDelivery.Events;

public enum RoadEventType
{
    IcyRails,      // Hız düşürülmezse tren (ve paket) hasar alır
    ValveLeak,     // Süre içinde 'E' basılmazsa kazan hızla soğur
    SupplyCache,   // Yeterince yavaşlanırsa terk edilmiş vagondan eşya alınır
    WolfPack,      // Kurt sürüsü: [F] et at, [SHIFT] duman püskürt ya da hızla kaç
    BrokenSwitch,  // Çökmüş ray: [A]/[D] ile sağlam hatta makas değiştir ya da yavaşla
    Blizzard,      // Tipi: sıcaklık çakılır; hızı düşürmezsen çığ treni gömer
    MineEntrance,  // Terk edilmiş maden girişi: yavaşla ve [E] ile içeri gir
    LampFailure,   // Fener arızası: far söner, [E] ile tamir edilene kadar zifiri karanlık
    AcidStorm      // DLC Buzul Kıyameti: zehirli kar fırtınası gövdeyi aşındırır; hızlı giden çabuk çıkar
}

/// <summary>Sürüş sırasında yaşanan, kısa süreli tek bir olay.</summary>
public sealed class RoadEvent
{
    public RoadEvent(RoadEventType type, float duration, float gracePeriod, string warnedBy)
    {
        Type = type;
        Duration = duration;
        GracePeriod = gracePeriod;
        WarnedBy = warnedBy;
    }

    public RoadEventType Type { get; }

    /// <summary>Buz: olayın toplam süresi. Diğerleri: tepki penceresi.</summary>
    public float Duration { get; }

    /// <summary>Buz: hasar başlamadan önce yavaşlamak için tanınan süre.</summary>
    public float GracePeriod { get; }

    /// <summary>Olayı telsizden önceden haber veren yolcunun adı (yoksa null).</summary>
    public string WarnedBy { get; }

    public float Age { get; set; }
    public bool TookDamage { get; set; }

    public bool InGracePeriod => Type == RoadEventType.IcyRails && Age < GracePeriod;

    // ------------------------------------------------------------------
    // Vana mini oyunu: basınç ibresi
    // ------------------------------------------------------------------

    /// <summary>Vana göstergesinin yeşil bölgesinin başlangıcı (0-1). Her arızada farklı yerdedir.</summary>
    public float GreenStart { get; init; } = 0.5f;

    public float GreenEnd => GreenStart + GameConfig.ValveGreenWidth;

    /// <summary>
    /// İbrenin konumu (0 = soğuk sol uç, 1 = kırmızı sağ uç). İbre soldan sağa, yeşilin
    /// üzerinden kırmızıya kayar, geri sekip yeniden süpürür; her geçişte biraz daha hızlanır.
    /// </summary>
    public float Needle
    {
        get
        {
            // Hızlanan üçgen dalga: faz = 0.55t + 0.2t²
            float phase = 0.55f * Age + 0.2f * Age * Age;
            float wave = (phase * 2f) % 2f;
            return 1f - System.MathF.Abs(wave - 1f);
        }
    }

    public bool NeedleInGreen => Needle >= GreenStart && Needle <= GreenEnd;

    // ------------------------------------------------------------------
    // Tam sürüm olaylarının durumu
    // ------------------------------------------------------------------

    /// <summary>Kurt sürüsünün saldırganlığı (1 = tam saldırı, 0 = kaçtı).</summary>
    public float Aggression { get; set; } = 1f;
    public float BiteTimer { get; set; }

    /// <summary>Çökmüş ray: kırık olan kol (0 = sol, 1 = sağ). Varsayılan makas sol koldadır.</summary>
    public int BrokenBranch { get; init; }

    /// <summary>Kırık rayın işareti bu süreden sonra okunur (gece/tipi/tünelde fenersiz geç görülür).</summary>
    public float HintDelay { get; init; }

    public bool HintVisible => Age >= HintDelay;

    /// <summary>Oyuncunun makası attığı kol (-1 = karar verilmedi).</summary>
    public int ChosenBranch { get; set; } = -1;

    /// <summary>Tipi: 0-1 çığ riski; 1'e ulaşırsa çığ düşer.</summary>
    public float AvalancheRisk { get; set; }

    /// <summary>Fener arızası: tamir için vurulan [E] sayısı.</summary>
    public int RepairHits { get; set; }

    public string Title => Loc.T(Type switch
    {
        RoadEventType.IcyRails => "ev.ice.title",
        RoadEventType.ValveLeak => "ev.valve.title",
        RoadEventType.WolfPack => "ev.wolf.title",
        RoadEventType.BrokenSwitch => "ev.switch.title",
        RoadEventType.Blizzard => "ev.blizzard.title",
        RoadEventType.MineEntrance => "ev.mine.title",
        RoadEventType.LampFailure => "ev.lamp.title",
        RoadEventType.AcidStorm => "ev.acid.title",
        _ => "ev.cache.title"
    });

    public string Instruction => Type switch
    {
        RoadEventType.IcyRails => Loc.F("ev.ice.instr", (int)GameConfig.IceSafeSpeed),
        RoadEventType.ValveLeak => Loc.T("ev.valve.instr"),
        RoadEventType.WolfPack => Loc.F("ev.wolf.instr", (int)GameConfig.WolfOutrunSpeed),
        RoadEventType.BrokenSwitch => SwitchInstruction,
        RoadEventType.Blizzard => Loc.F("ev.blizzard.instr", (int)GameConfig.AvalancheSafeSpeed),
        RoadEventType.MineEntrance => Loc.F("ev.mine.instr", (int)GameConfig.MineEnterSpeed),
        RoadEventType.LampFailure => Loc.F("ev.lamp.instr", RepairHits, GameConfig.LampRepairHits),
        RoadEventType.AcidStorm => Loc.F("ev.acid.instr", (int)GameConfig.AcidEscapeSpeed),
        _ => Loc.F("ev.cache.instr", (int)GameConfig.SupplyCacheSpeed)
    };

    private string SwitchInstruction
    {
        get
        {
            string broken = !HintVisible ? "???" : Loc.T(BrokenBranch == 0 ? "ev.switch.left" : "ev.switch.right");
            return Loc.F("ev.switch.instr", broken, (int)GameConfig.SwitchSafeSpeed);
        }
    }

    /// <summary>Zamanlayıcı çubuğu için 1'den 0'a inen oran.</summary>
    public float TimerRatio
    {
        get
        {
            if (Type == RoadEventType.IcyRails)
            {
                return InGracePeriod
                    ? 1f - Age / GracePeriod
                    : 1f - (Age - GracePeriod) / (Duration - GracePeriod);
            }
            return 1f - Age / Duration;
        }
    }
}
