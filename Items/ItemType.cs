using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using TheLastDelivery.Localization;

namespace TheLastDelivery.Items;

/// <summary>Yolda bulunabilen eşya türleri.</summary>
public enum ItemType
{
    SpareParts,   // '1': Tren sağlığını onarır
    ThermalCoal,  // '2': Anında yakıt ve ısı
    Medkit,       // '3': Bir yolcuyu iyileştirir (zehri temizlemez)
    RareMedkit,   // '4': Tüm yolcuları %100 iyileştirir, zehir ve sabotaj hasarını temizler
    WolfBait      // '5' ya da kurt saldırısında [F]: trenden et atılır, sürü uzaklaşır
}

/// <summary>Eşyaların görünen adları, tuşları ve renkleri.</summary>
public static class ItemCatalog
{
    public static string Name(ItemType type) => Loc.T(type switch
    {
        ItemType.SpareParts => "item.parts",
        ItemType.ThermalCoal => "item.coal",
        ItemType.Medkit => "item.medkit",
        ItemType.RareMedkit => "item.rareMedkit",
        _ => "item.bait"
    });

    public static string KeyLabel(ItemType type) => type switch
    {
        ItemType.SpareParts => "1",
        ItemType.ThermalCoal => "2",
        ItemType.Medkit => "3",
        ItemType.RareMedkit => "4",
        _ => "5"
    };

    /// <summary>Eşyayı kullanan klavye tuşları (üst sıra ve numpad).</summary>
    public static (Keys Main, Keys NumPad) UseKeys(ItemType type) => type switch
    {
        ItemType.SpareParts => (Keys.D1, Keys.NumPad1),
        ItemType.ThermalCoal => (Keys.D2, Keys.NumPad2),
        ItemType.Medkit => (Keys.D3, Keys.NumPad3),
        ItemType.RareMedkit => (Keys.D4, Keys.NumPad4),
        _ => (Keys.D5, Keys.NumPad5)
    };

    public static Color Color(ItemType type) => type switch
    {
        ItemType.SpareParts => new Color(170, 180, 200),
        ItemType.ThermalCoal => new Color(255, 150, 60),
        ItemType.Medkit => new Color(235, 90, 90),
        ItemType.RareMedkit => new Color(255, 215, 110),
        _ => new Color(200, 110, 100)
    };

    public static readonly ItemType[] All =
        { ItemType.SpareParts, ItemType.ThermalCoal, ItemType.Medkit, ItemType.RareMedkit, ItemType.WolfBait };
}
