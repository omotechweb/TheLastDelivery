using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheLastDelivery.Core;
using TheLastDelivery.Graphics;
using TheLastDelivery.Localization;
using TheLastDelivery.Items;

namespace TheLastDelivery.UI;

/// <summary>
/// Sol alttaki envanter paneli: 4 yuva, her yuvada kodla çizilmiş eşya ikonu,
/// kullanım tuşu ve istif miktarı.
/// </summary>
public static class InventoryPanel
{
    private const int SlotSize = 64;
    private const int SlotGap = 8;
    private const int Padding = 14;

    /// <summary>Panelin genişliği yuva sayısından hesaplanır; olay kutusu bunun sağına hizalanır.</summary>
    public static readonly Rectangle Panel = new(16, 566,
        Padding * 2 + GameConfig.InventorySlots * SlotSize + (GameConfig.InventorySlots - 1) * SlotGap, 110);

    public static void Draw(SpriteBatch sb, Inventory inventory)
    {

        UiDraw.FancyPanel(sb, Panel, Hud.PanelEdge);

        GameText.Draw(sb, Loc.T("inv.title"), new Vector2(Panel.X + Padding, Panel.Y + 10), Hud.Amber);

        // Sağ üst: kargo vagonu doluluğu (sandık simgesi + yağma/kapasite)
        var cargo = inventory.Cargo;
        string usage = $"{cargo.Total}/{cargo.Capacity}";
        float usageWidth = GameText.Measure(usage, 2).X;
        GameText.Draw(sb, usage, new Vector2(Panel.Right - Padding - usageWidth, Panel.Y + 10), cargo.IsFull ? Hud.Danger : Hud.Label);
        UiDraw.UpgradeIcon(sb, Economy.UpgradeType.CargoBay, new Point((int)(Panel.Right - Padding - usageWidth - 18), Panel.Y + 20));

        // Ortada: erzak çuvalı ([G] ile en aç yolcuya paylaştırılır)
        int rationX = Panel.X + 160;
        DrawRationIcon(sb, new Point(rationX, Panel.Y + 20));
        GameText.Draw(sb, $"X{inventory.Rations}", new Vector2(rationX + 12, Panel.Y + 10),
            inventory.Rations == 0 ? Hud.Danger : Hud.Label);

        for (int i = 0; i < GameConfig.InventorySlots; i++)
        {
            var slotRect = new Rectangle(Panel.X + Padding + i * (SlotSize + SlotGap), Panel.Y + 36, SlotSize, SlotSize);
            var slot = inventory.Slots[i];

            sb.FillRect(slotRect, new Color(14, 16, 26));

            if (slot == null)
            {
                sb.DrawBorder(slotRect, 1, Hud.PanelEdge * 0.5f);
                continue;
            }

            var color = ItemCatalog.Color(slot.Type);
            sb.FillRect(slotRect, color * 0.08f);
            sb.DrawBorder(slotRect, 2, color * 0.8f);

            DrawIcon(sb, slot.Type, slotRect.Center);

            // Sol üst: kullanım tuşu
            var keyRect = new Rectangle(slotRect.X + 3, slotRect.Y + 3, 16, 18);
            sb.FillRect(keyRect, new Color(6, 8, 16) * 0.9f);
            GameText.Draw(sb, ItemCatalog.KeyLabel(slot.Type), new Vector2(keyRect.X + 2, keyRect.Y - 1), Hud.Amber);

            // Sağ alt: miktar
            string count = $"X{slot.Count}";
            float countWidth = GameText.Measure(count, 2).X;
            GameText.Draw(sb, count, new Vector2(slotRect.Right - 4 - countWidth, slotRect.Bottom - 22), Hud.Value);
        }
    }

    /// <summary>Erzak: ağzı bağlı küçük bir çuval ve yanında ekmek.</summary>
    public static void DrawRationIcon(SpriteBatch sb, Point center)
    {
        void Box(int x, int y, int w, int h, Color c) => sb.FillRect(new Rectangle(center.X + x, center.Y + y, w, h), c);
        var sack = new Color(170, 140, 96);
        Box(-7, -4, 12, 11, sack);
        Box(-5, -7, 8, 3, sack);
        Box(-4, -8, 6, 2, new Color(120, 90, 60));   // Bağ
        Box(-6, -2, 10, 1, new Color(140, 112, 74));
        Box(3, 2, 7, 5, new Color(200, 140, 70));     // Ekmek
        Box(4, 2, 5, 1, new Color(235, 180, 100));
    }

    /// <summary>Eşya ikonlarını küçük piksel bloklarıyla çizer.</summary>
    public static void DrawIcon(SpriteBatch sb, ItemType type, Point center)
    {
        void Box(int x, int y, int w, int h, Color c) => sb.FillRect(new Rectangle(center.X + x, center.Y + y, w, h), c);

        switch (type)
        {
            case ItemType.SpareParts: // Dişli çark
            {
                var metal = new Color(160, 168, 186);
                Box(-10, -10, 20, 20, metal);
                Box(-4, -15, 8, 5, metal);  Box(-4, 10, 8, 5, metal);   // Üst/alt diş
                Box(-15, -4, 5, 8, metal);  Box(10, -4, 5, 8, metal);   // Sol/sağ diş
                Box(-13, -13, 5, 5, metal); Box(8, -13, 5, 5, metal);   // Çapraz dişler
                Box(-13, 8, 5, 5, metal);   Box(8, 8, 5, 5, metal);
                Box(-4, -4, 8, 8, new Color(30, 32, 40));                // Mil deliği
                Box(-10, -10, 20, 2, new Color(210, 215, 228));          // Parlama
                break;
            }
            case ItemType.ThermalCoal: // Kızgın kömür yığını
            {
                var coal = new Color(62, 58, 64);
                var edge = new Color(105, 100, 110);
                var ember = new Color(255, 140, 50);
                sb.DrawGlow(center.ToVector2(), 28f, ember, 0.5f);
                Box(-15, 1, 13, 12, coal);
                Box(0, -1, 15, 14, coal);
                Box(-8, -11, 14, 12, coal);
                Box(-15, 1, 13, 2, edge);       // Üst kenar parlamaları
                Box(0, -1, 15, 2, edge);
                Box(-8, -11, 14, 2, edge);
                Box(-11, 6, 7, 3, ember);       // Kızgın çatlaklar
                Box(3, 5, 9, 3, ember);
                Box(-4, -5, 6, 3, new Color(255, 205, 100));
                break;
            }
            case ItemType.Medkit: // Kırmızı haçlı çanta
            {
                var bag = new Color(225, 228, 235);
                var red = new Color(215, 55, 55);
                Box(-6, -14, 12, 4, new Color(120, 124, 135)); // Sap
                Box(-14, -10, 28, 22, bag);
                Box(-3, -6, 6, 14, red);
                Box(-7, -2, 14, 6, red);
                break;
            }
            case ItemType.RareMedkit: // Altın kenarlı, parlayan askeri sağlık kiti
            {
                var gold = new Color(240, 200, 90);
                sb.DrawGlow(center.ToVector2(), 26f, gold, 0.35f);
                Box(-6, -15, 12, 4, new Color(150, 120, 60));
                Box(-15, -11, 30, 24, gold);
                Box(-13, -9, 26, 20, new Color(40, 70, 60));
                Box(-3, -6, 6, 14, new Color(120, 240, 170));
                Box(-7, -2, 14, 6, new Color(120, 240, 170));
                Box(-13, -9, 26, 2, new Color(255, 240, 180));
                break;
            }
            case ItemType.WolfBait: // Kemikli et parçası
            {
                Box(-12, -6, 20, 14, new Color(170, 60, 56));
                Box(-10, -4, 16, 4, new Color(220, 110, 100));
                Box(6, -2, 9, 4, new Color(230, 225, 210));    // Kemik
                Box(13, -5, 4, 4, new Color(230, 225, 210));
                Box(13, 1, 4, 4, new Color(230, 225, 210));
                Box(-12, 6, 20, 2, new Color(110, 34, 30));
                break;
            }
        }
    }
}
