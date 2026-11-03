using TheLastDelivery.Localization;

namespace TheLastDelivery.Economy;

/// <summary>Kalıcı tren geliştirmeleri (bir kez satın alınır, tüm yolculuklarda geçerlidir).</summary>
public enum UpgradeType
{
    FuelTank,    // Büyük yakıt deposu: kapasite 100 → 140
    Heater,      // Güçlü ısıtıcı: kazan %25 daha yavaş soğur
    CargoBay,    // Kargo vagonu: kargo kapasitesi +8
    Armor,       // Zırh plakaları: gövde hasarı %25 az
    BattleArmor, // Zırh Kaplama: mafya ve kurt saldırılarından alınan hasar %50 az
    Lantern,     // Gelişmiş Fener: gece/tipi/tünelde görüş, tehlikeler erken fark edilir, madende geniş ışık

    // Modüler vagonlar: trenin arkasına eklenir ve sürüş ekranında görünür
    CargoWagon,     // Kargo Vagonu: kargo kapasitesi +10
    PassengerWagon, // Yolcu Vagonu: +3 yatak
    ArmoredWagon    // Zırhlı Vagon: azami sağlık +50, pusu hasarı %30 az
}

public static class UpgradeCatalog
{
    public static readonly UpgradeType[] All =
    {
        UpgradeType.FuelTank, UpgradeType.Heater, UpgradeType.CargoBay, UpgradeType.Armor,
        UpgradeType.BattleArmor, UpgradeType.Lantern,
        UpgradeType.CargoWagon, UpgradeType.PassengerWagon, UpgradeType.ArmoredWagon
    };

    /// <summary>Vagon mu? (mağazada "VAGON" etiketiyle gösterilir)</summary>
    public static bool IsWagon(UpgradeType type) =>
        type is UpgradeType.CargoWagon or UpgradeType.PassengerWagon or UpgradeType.ArmoredWagon;

    public const float BigTankCapacity = 140f;
    public const float HeaterCoolingMultiplier = 0.75f;
    public const int CargoBayBonus = 8;
    public const float ArmorDamageMultiplier = 0.75f;

    public static int Price(UpgradeType type) => type switch
    {
        UpgradeType.FuelTank => 120,
        UpgradeType.Heater => 150,
        UpgradeType.CargoBay => 90,
        UpgradeType.BattleArmor => 180,
        UpgradeType.Lantern => 130,
        UpgradeType.CargoWagon => 200,
        UpgradeType.PassengerWagon => 260,
        UpgradeType.ArmoredWagon => 320,
        _ => 140
    };

    public static string Name(UpgradeType type) => Loc.T($"upg.{Key(type)}.name");
    public static string Description(UpgradeType type) => Loc.T($"upg.{Key(type)}.desc");

    public static string Key(UpgradeType type) => type switch
    {
        UpgradeType.FuelTank => "tank",
        UpgradeType.Heater => "heater",
        UpgradeType.CargoBay => "cargo",
        UpgradeType.BattleArmor => "battle",
        UpgradeType.Lantern => "lantern",
        UpgradeType.CargoWagon => "wagonCargo",
        UpgradeType.PassengerWagon => "wagonPassenger",
        UpgradeType.ArmoredWagon => "wagonArmored",
        _ => "armor"
    };
}
