using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TheLastDelivery.Audio;
using TheLastDelivery.Core;
using TheLastDelivery.Economy;
using TheLastDelivery.Graphics;
using TheLastDelivery.Localization;
using TheLastDelivery.Platform;
using TheLastDelivery.Story;

namespace TheLastDelivery.UI;

/// <summary>
/// Ana menünün DLC ekranları:
///   DLC & EK İÇERİKLER → 4 DLC kartı. Sahibi ENTER ile içeriğe gider; değilse kilit ve "Steam Mağazasında İncele".
///   BONUSLAR (Supporter Pack) → konsept çizim galerisi, dijital soundtrack çalar, destekçi rozeti ayarı.
///   ÖZELLEŞTİR (Sığınak Kuryeleri) → duman efekti, kabin tablosu, radyo modeli, Arthur kostümü.
///   BUZUL KIYAMETİ (Frozen Wasteland) → 5 ek bölümün seçimi.
///   Acımasız Kış Sınavı → iki kez ENTER ile Permadeath koşusu başlar.
/// Mağazanın ikinci sekmesi aynı DLC listesini gösterir.
/// </summary>
public sealed partial class MetaMenu
{
    private const int ConceptArtCount = 4;

    private int _shopTab;            // Mağaza: 0 geliştirmeler, 1 DLC ve ek içerikler, 2 kozmetik (mikro içerikler)
    private bool _hardcoreConfirm;   // Permadeath başlatmadan önce ikinci ENTER bekleniyor
    private int _artIndex;           // Bonuslar: gösterilen konsept çizim
    private readonly int[] _customIndex = new int[6]; // Yuva başına seçili seçenek (evcil hayvan dahil)
    private const int CustomSlots = 5;               // Duman, tablo, radyo, kostüm, müzik kutusu

    /// <summary>Bonuslar ekranında çalan soundtrack parçası (-1 = yok); Game1 sesi buna göre ayarlar.</summary>
    public int PlayingTrack { get; private set; } = -1;

    /// <summary>Özelleştirme ekranında duman seçeneği gezilirken sahnedeki trende önizlenen duman.</summary>
    public SmokeStyle? PreviewSmoke => Screen == MetaScreen.Customize && _cursor == (int)CosmeticSlot.Smoke
        ? (SmokeStyle)CosmeticCatalog.Smokes[_customIndex[0]].Value
        : null;

    /// <summary>Buzul Kıyameti ekranında seçilen bölüm (1-5).</summary>
    public int ChosenWastelandChapter { get; private set; } = 1;

    private static int DlcOwnedCount
    {
        get
        {
            int count = 0;
            foreach (var dlc in DLCManager.All) if (DLCManager.Owns(dlc.Id)) count++;
            return count;
        }
    }

    private void SwitchShopTab(ProceduralAudio audio, int direction)
    {
        _shopTab = (_shopTab + direction + 3) % 3;
        _cursor = 0;
        _hardcoreConfirm = false;
        if (_shopTab == 1) DLCManager.Refresh();
        audio.PlayClick();
    }

    // ------------------------------------------------------------------
    // Güncelleme
    // ------------------------------------------------------------------

    private MetaAction UpdateExtras(InputState input, ProceduralAudio audio)
    {
        return Screen switch
        {
            MetaScreen.Bonuses => UpdateBonuses(input, audio),
            MetaScreen.Customize => UpdateCustomize(input, audio),
            MetaScreen.Wasteland => UpdateWasteland(input, audio),
            MetaScreen.Mods or MetaScreen.Contracts => MetaAction.None, // Yalnızca bilgi ekranları
            MetaScreen.Shop when _shopTab == 2 => UpdateCosmeticStore(input, audio),
            _ => UpdateDlcList(input, audio) // DLC ekranı ve mağazanın DLC sekmesi
        };
    }

    /// <summary>Kozmetik mağazası: mikro ödemeyle ya da görevle açılan içerikler. Sahip olunan öğe ENTER ile kuşanılır.</summary>
    private MetaAction UpdateCosmeticStore(InputState input, ProceduralAudio audio)
    {
        int count = CosmeticCatalog.MicroItems.Length;
        int before = _cursor;
        if (input.AnyPressed(Keys.W, Keys.Up)) _cursor = (_cursor - 1 + count) % count;
        if (input.AnyPressed(Keys.S, Keys.Down)) _cursor = (_cursor + 1) % count;
        if (_cursor != before) audio.PlayClick();
        if (!input.AnyPressed(Keys.Enter, Keys.Space) || _screenTime < 0.15f) return MetaAction.None;

        var item = CosmeticCatalog.MicroItems[_cursor];
        if (CosmeticCatalog.IsOwned(item))
        {
            SaveData.TryEquip(item);
            Show(Loc.F("skins.equippedMsg", item.Name), CabinPanel.Good);
            audio.PlayCoin();
            return MetaAction.None;
        }

        // Görevlerden kazanılan kozmetik bileti: bir mikro içeriği ücretsiz açar
        if (SaveData.Current.CosmeticTickets > 0)
        {
            SaveData.Current.CosmeticTickets--;
            SaveData.Current.OwnedCosmetics.Add(item.Key);
            SaveData.TryEquip(item); // Kaydeder
            Show(Loc.F("shop.ticketUsed", item.Name), CabinPanel.Good);
            audio.PlaySuccess();
            return MetaAction.None;
        }

        switch (StoreManager.Buy(item))
        {
            case PurchaseResult.Purchased:
                SaveData.TryEquip(item);
                Show(Loc.F("shop.mtxBought", item.Name), CabinPanel.Good);
                audio.PlaySuccess();
                break;
            case PurchaseResult.QuestOnly:
                Show(QuestCatalog.Get(item.Quest)?.Description ?? "", new Color(200, 160, 255));
                audio.PlayError();
                break;
            default:
                Show(Loc.T("shop.mtxOffline"), Hud.Label);
                audio.PlayError();
                break;
        }
        return MetaAction.None;
    }

    private MetaAction UpdateDlcList(InputState input, ProceduralAudio audio)
    {
        int count = DLCManager.All.Length;
        int before = _cursor;
        if (input.AnyPressed(Keys.W, Keys.Up)) _cursor = (_cursor - 1 + count) % count;
        if (input.AnyPressed(Keys.S, Keys.Down)) _cursor = (_cursor + 1) % count;
        if (_cursor != before) { _hardcoreConfirm = false; audio.PlayClick(); }

        var dlc = DLCManager.All[_cursor];

        if (!input.AnyPressed(Keys.Enter, Keys.Space) || _screenTime < 0.15f) return MetaAction.None;

        if (!DLCManager.Owns(dlc.Id))
        {
            // "Steam Mağazasında İncele": Steam kaplaması açılmazsa (çevrimdışı) bilgi verilir, oyun etkilenmez
            bool opened = DLCManager.ViewInStore(dlc.Id);
            Show(Loc.T(opened ? "dlc.storeOpened" : "dlc.storeOffline"), opened ? CabinPanel.Good : Hud.Label);
            audio.PlayClick();
            return MetaAction.None;
        }

        audio.PlayConfirm();
        switch (dlc.Id)
        {
            case DlcId.Supporter: Open(MetaScreen.Bonuses); break;
            case DlcId.FrozenWasteland: Open(MetaScreen.Wasteland); break;
            case DlcId.Couriers: Open(MetaScreen.Customize); break;
            case DlcId.Episode2: OpenEpisodes(1); break;
            default:
                if (!_hardcoreConfirm)
                {
                    _hardcoreConfirm = true; // Tek can: oyuncu ne başlattığını bilsin
                    Show(Loc.T("hardcore.confirm"), Hud.Danger);
                    audio.PlayAlarm();
                    return MetaAction.None;
                }
                _hardcoreConfirm = false;
                return MetaAction.StartHardcore;
        }
        return MetaAction.None;
    }

    private MetaAction UpdateBonuses(InputState input, ProceduralAudio audio)
    {
        int rows = AudioManager.SoundtrackLength + 1; // Parçalar + destekçi rozeti ayarı
        int before = _cursor;
        if (input.AnyPressed(Keys.W, Keys.Up)) _cursor = (_cursor - 1 + rows) % rows;
        if (input.AnyPressed(Keys.S, Keys.Down)) _cursor = (_cursor + 1) % rows;
        if (input.AnyPressed(Keys.A, Keys.Left)) { _artIndex = (_artIndex - 1 + ConceptArtCount) % ConceptArtCount; audio.PlayClick(); }
        if (input.AnyPressed(Keys.D, Keys.Right)) { _artIndex = (_artIndex + 1) % ConceptArtCount; audio.PlayClick(); }
        if (_cursor != before) audio.PlayClick();

        if (!input.AnyPressed(Keys.Enter, Keys.Space) || _screenTime < 0.15f) return MetaAction.None;

        if (_cursor < AudioManager.SoundtrackLength)
        {
            PlayingTrack = PlayingTrack == _cursor ? -1 : _cursor; // Aynı parçaya basmak durdurur
            audio.PlayClick();
        }
        else
        {
            SaveData.Current.ShowSupporterBadge = !SaveData.Current.ShowSupporterBadge;
            SaveData.Save();
            audio.PlayConfirm();
        }
        return MetaAction.None;
    }

    private void LoadCustomizeSelection()
    {
        foreach (CosmeticSlot slot in Enum.GetValues(typeof(CosmeticSlot)))
        {
            var items = CosmeticCatalog.For(slot);
            int index = Array.FindIndex(items, i => i.Id == DLCManager.Active(slot).Id);
            _customIndex[(int)slot] = Math.Max(0, index);
        }
    }

    private MetaAction UpdateCustomize(InputState input, ProceduralAudio audio)
    {
        const int slots = CustomSlots;
        int before = _cursor;
        if (input.AnyPressed(Keys.W, Keys.Up)) _cursor = (_cursor - 1 + slots) % slots;
        if (input.AnyPressed(Keys.S, Keys.Down)) _cursor = (_cursor + 1) % slots;
        if (_cursor != before) audio.PlayClick();

        var items = CosmeticCatalog.For((CosmeticSlot)_cursor);
        if (input.AnyPressed(Keys.A, Keys.Left)) { _customIndex[_cursor] = (_customIndex[_cursor] - 1 + items.Length) % items.Length; audio.PlayClick(); }
        if (input.AnyPressed(Keys.D, Keys.Right)) { _customIndex[_cursor] = (_customIndex[_cursor] + 1) % items.Length; audio.PlayClick(); }

        if (!input.AnyPressed(Keys.Enter, Keys.Space) || _screenTime < 0.15f) return MetaAction.None;

        var item = items[_customIndex[_cursor]];
        if (SaveData.TryEquip(item))
        {
            Show(Loc.F("skins.equippedMsg", item.Name), CabinPanel.Good);
            audio.PlayCoin();
        }
        else
        {
            Show(item.Dlc != null ? Loc.F("skins.dlcNeed", DLCManager.Info(item.Dlc.Value).Name) : Loc.T("shop.mtxGoStore"), Hud.Danger);
            audio.PlayError();
        }
        return MetaAction.None;
    }

    private MetaAction UpdateWasteland(InputState input, ProceduralAudio audio)
    {
        int count = Chapters.Wasteland.Length;
        int before = _cursor;
        if (input.AnyPressed(Keys.A, Keys.Left, Keys.W, Keys.Up)) _cursor = (_cursor - 1 + count) % count;
        if (input.AnyPressed(Keys.D, Keys.Right, Keys.S, Keys.Down)) _cursor = (_cursor + 1) % count;
        if (_cursor != before) audio.PlayClick();

        if (!input.AnyPressed(Keys.Enter, Keys.Space) || _screenTime < 0.15f) return MetaAction.None;
        if (_cursor + 1 > SaveData.Current.WastelandUnlocked)
        {
            Show(Loc.F("chapters.unlockHint", _cursor), Hud.Danger);
            audio.PlayError();
            return MetaAction.None;
        }
        ChosenWastelandChapter = _cursor + 1;
        audio.PlayConfirm();
        return MetaAction.StartWasteland;
    }

    // ------------------------------------------------------------------
    // Çizim
    // ------------------------------------------------------------------

    private void DrawExtras(SpriteBatch sb, float fade)
    {
        switch (Screen)
        {
            case MetaScreen.Bonuses: DrawBonuses(sb, fade); break;
            case MetaScreen.Customize: DrawCustomize(sb, fade); break;
            case MetaScreen.Wasteland: DrawWasteland(sb, fade); break;
            case MetaScreen.Mods: DrawMods(sb, fade); break;
            case MetaScreen.Contracts: DrawContracts(sb, fade); break;
            default:
                DrawHeader(sb, Loc.T("dlc.title"), fade);
                DrawDlcList(sb, fade, top: 96);
                break;
        }
    }

    private void DrawShopTabs(SpriteBatch sb, float fade)
    {
        string[] tabs = { Loc.T("shop.tabUpgrades"), Loc.T("shop.tabDlc"), Loc.T("shop.tabCosmetics") };
        float total = 0f;
        foreach (string t in tabs) total += GameText.MeasureLine(t) + 24f + 20f;
        float x = CenterX - (total - 20f) / 2f;
        for (int i = 0; i < tabs.Length; i++)
        {
            float width = GameText.MeasureLine(tabs[i]) + 24f;
            var rect = new Rectangle((int)x, 82, (int)width, 26);
            bool active = i == _shopTab;
            sb.FillRect(rect, (active ? Hud.Amber * 0.25f : Color.Black * 0.4f) * fade);
            sb.DrawBorder(rect, active ? 2 : 1, (active ? Hud.Amber : Hud.PanelEdge) * fade);
            GameText.Draw(sb, tabs[i], new Vector2(rect.X + 12, rect.Y + 3), (active ? Color.White : Hud.Label) * fade);
            x += width + 20f;
        }
    }

    /// <summary>4 DLC kartı: amblem, ad, açıklama, içerik listesi; sağda sahiplik durumu ya da Steam butonu.</summary>
    private void DrawDlcList(SpriteBatch sb, float fade, int top)
    {
        int step = Math.Min(top > 100 ? 116 : 120, (640 - top) / DLCManager.All.Length); // 5 DLC ekrana sığsın
        for (int i = 0; i < DLCManager.All.Length; i++)
        {
            var dlc = DLCManager.All[i];
            bool owned = DLCManager.Owns(dlc.Id);
            bool selected = i == _cursor;
            var card = new Rectangle(90, top + i * step, 1100, step - 10);
            var accent = DlcAccent(dlc.Id);

            UiDraw.FancyPanel(sb, card, (selected ? accent : Hud.PanelEdge) * fade);
            if (selected) sb.FillRect(card, accent * (0.08f * fade));

            DrawDlcEmblem(sb, new Rectangle(card.X + 14, card.Y + 12, 82, card.Height - 24), dlc.Id, fade * (owned ? 1f : 0.55f));

            // Metin alanı sağdaki durum/buton sütununa kadar uzanır; açıklama gerekirse iki satıra sarılır
            int textX = card.X + 112;
            int textWidth = card.Right - 20 - (owned ? 230 : 330) - textX;
            GameText.Draw(sb, dlc.Name, new Vector2(textX, card.Y + 6), (owned ? Color.White : Hud.Value) * fade, 3);
            var lines = GameText.Wrap(dlc.Description, textWidth, 2);
            for (int l = 0; l < Math.Min(2, lines.Count); l++)
            {
                string line = l == 1 && lines.Count > 2 ? Hud.FitWidth(lines[1] + " " + lines[2], textWidth) : lines[l];
                GameText.Draw(sb, line, new Vector2(textX, card.Y + 36 + l * 20), Hud.Label * fade);
            }
            GameText.Draw(sb, Hud.FitWidth(dlc.Contents, textWidth), new Vector2(textX, card.Y + 80), accent * (0.85f * fade));

            // Sağ: sahiplik durumu ve eylem
            int right = card.Right - 20;
            if (owned)
            {
                GameText.DrawRight(sb, Loc.T("dlc.owned"), right, card.Y + 14, CabinPanel.Good * fade);
                if (selected)
                {
                    string action = dlc.Id == DlcId.Hardcore && _hardcoreConfirm ? Loc.T("hardcore.confirmShort") : Loc.T($"dlc.{dlc.Key}.action");
                    float pulse = 0.6f + 0.4f * MathF.Sin(_time * 5f);
                    GameText.DrawRight(sb, action, right, card.Y + 60,
                        (dlc.Id == DlcId.Hardcore && _hardcoreConfirm ? Hud.Danger : Hud.Amber) * (fade * pulse));
                }
            }
            else
            {
                string locked = Loc.T("chapters.locked");
                float lockedWidth = GameText.MeasureLine(locked);
                GameText.DrawRight(sb, locked, right, card.Y + 14, Hud.Danger * fade);
                DrawLock(sb, new Point((int)(right - lockedWidth - 22), card.Y + 26), fade);

                // "Steam Mağazasında İncele" butonu
                string button = Loc.T("dlc.viewStore");
                float width = GameText.MeasureLine(button) + 28f;
                var rect = new Rectangle((int)(right - width), card.Y + 52, (int)width, 32);
                sb.FillRect(rect, (selected ? new Color(40, 70, 110) : new Color(22, 34, 54)) * fade);
                sb.DrawBorder(rect, selected ? 2 : 1, (selected ? new Color(120, 190, 255) : Hud.PanelEdge) * fade);
                GameText.Draw(sb, button, new Vector2(rect.X + 14, rect.Y + 6), (selected ? Color.White : Hud.Label) * fade);
            }
        }

        // Altta platform durumu (ve Debug derlemesinde geliştirici kısayolu)
        string status = Loc.F("dlc.platform", AchievementManager.Platform.IsOnline ? "Steam" : Loc.T("dlc.offline"));
        GameText.DrawCentered(sb, status, CenterX, top + DLCManager.All.Length * step + 2, Hud.Label * (0.7f * fade));
    }

    private static Color DlcAccent(DlcId id) => id switch
    {
        DlcId.Supporter => new Color(255, 205, 90),
        DlcId.FrozenWasteland => new Color(170, 230, 90),
        DlcId.Couriers => new Color(90, 220, 255),
        DlcId.Episode2 => new Color(255, 50, 60),
        _ => new Color(240, 80, 70)
    };

    /// <summary>DLC amblemleri: altın lokomotif, zehirli buzul, neon duman, kafatası-kar tanesi.</summary>
    private void DrawDlcEmblem(SpriteBatch sb, Rectangle r, DlcId id, float alpha)
    {
        var accent = DlcAccent(id);
        sb.FillRect(r, new Color(10, 12, 20) * alpha);
        sb.DrawBorder(r, 2, accent * alpha);
        var c = r.Center;
        void Box(int x, int y, int w, int h, Color col) => sb.FillRect(new Rectangle(c.X + x, c.Y + y, w, h), col * alpha);

        switch (id)
        {
            case DlcId.Supporter: // Altın lokomotif ve yıldız
                Box(-26, 0, 40, 16, accent);
                Box(-20, -14, 16, 14, UiDraw.Shade(accent, 0.8f));
                Box(10, -8, 6, 8, accent);
                Box(-22, 16, 8, 8, new Color(40, 30, 10)); Box(2, 16, 8, 8, new Color(40, 30, 10));
                Box(16, -24, 3, 10, Color.White); Box(13, -21, 9, 3, Color.White);
                sb.DrawGlow(c.ToVector2(), 36f, accent, 0.3f * alpha);
                break;
            case DlcId.FrozenWasteland: // Zehirli yeşil gökyüzü altında paslı kule
                Box(-36, -28, 72, 30, new Color(60, 90, 40));
                Box(-8, -22, 10, 40, new Color(90, 60, 40));
                Box(-16, -8, 26, 4, new Color(90, 60, 40));
                Box(-36, 14, 72, 14, new Color(190, 210, 140));
                for (int i = 0; i < 8; i++) Box(-30 + i * 8, -26 + (i * 13) % 34, 2, 3, accent);
                break;
            case DlcId.Couriers: // Bacadan yükselen neon buhar
                Box(-10, 6, 20, 18, new Color(40, 44, 56));
                Box(-6, -4, 12, 10, new Color(30, 34, 44));
                for (int i = 0; i < 4; i++)
                {
                    float wave = MathF.Sin(_time * 3f + i);
                    Box(-10 + (int)(wave * 6) - i * 2, -14 - i * 8, 14 + i * 4, 6, i % 2 == 0 ? accent : new Color(255, 90, 160));
                }
                break;
            case DlcId.Episode2: // AURA'nın kızıl gözü ve yörüngedeki uydu
            {
                float pulse = 0.6f + 0.4f * MathF.Sin(_time * 3f);
                sb.Draw(Assets.Circle, new Rectangle(c.X - 22, c.Y - 22, 44, 44), new Color(60, 8, 12) * alpha);
                sb.Draw(Assets.Circle, new Rectangle(c.X - 11, c.Y - 11, 22, 22), accent * (alpha * pulse));
                sb.Draw(Assets.Circle, new Rectangle(c.X - 4, c.Y - 4, 8, 8), Color.White * (alpha * pulse));
                float a = _time * 1.5f;
                Box((int)(MathF.Cos(a) * 32f) - 5, (int)(MathF.Sin(a) * 12f) - 2, 10, 4, new Color(190, 196, 210));
                sb.DrawGlow(c.ToVector2(), 40f, accent, 0.3f * alpha);
                break;
            }
            default: // Kar tanesi ortasında kafatası: tek can
                Box(-3, -26, 6, 52, new Color(200, 220, 240));
                Box(-26, -3, 52, 6, new Color(200, 220, 240));
                Box(-12, -12, 24, 22, new Color(230, 230, 230));
                Box(-8, -6, 6, 6, new Color(20, 20, 20)); Box(2, -6, 6, 6, new Color(20, 20, 20));
                Box(-8, 6, 16, 4, accent);
                break;
        }
    }

    private void DrawScrollArrow(SpriteBatch sb, Point tip, bool up, float fade)
    {
        for (int i = 0; i < 6; i++)
        {
            int w = 2 + i * 2;
            int y = up ? tip.Y - 6 + i : tip.Y + 6 - i;
            sb.FillRect(new Rectangle(tip.X - w / 2, y, w, 1), Hud.Amber * fade);
        }
    }

    /// <summary>Bonuslar: solda konsept çizim galerisi, sağda soundtrack listesi ve destekçi rozeti ayarı.</summary>
    private void DrawBonuses(SpriteBatch sb, float fade)
    {
        DrawHeader(sb, Loc.T("bonus.title"), fade);

        // Konsept çizim (eski kâğıt üzerinde karakalem)
        var frame = new Rectangle(60, 100, 700, 450);
        UiDraw.FancyPanel(sb, frame, DlcAccent(DlcId.Supporter) * fade);
        var paper = new Rectangle(frame.X + 16, frame.Y + 16, frame.Width - 32, frame.Height - 76);
        ConceptArt.Draw(sb, paper, _artIndex, fade, _time);
        GameText.DrawCentered(sb, Loc.T($"bonus.art{_artIndex}"), frame.Center.X, paper.Bottom + 12, Hud.Value * fade);
        GameText.DrawCentered(sb, $"< {_artIndex + 1}/{ConceptArtCount} >", frame.Center.X, paper.Bottom + 36, Hud.Label * (0.8f * fade));

        // Soundtrack
        var list = new Rectangle(790, 100, 430, 450);
        UiDraw.FancyPanel(sb, list, Hud.PanelEdge * fade);
        GameText.Draw(sb, Loc.T("bonus.soundtrack"), new Vector2(list.X + 20, list.Y + 14), Hud.Amber * fade, 3);
        for (int i = 0; i < AudioManager.SoundtrackLength; i++)
        {
            var row = new Rectangle(list.X + 14, list.Y + 62 + i * 66, list.Width - 28, 58);
            bool selected = i == _cursor, playing = i == PlayingTrack;
            sb.FillRect(row, (selected ? Hud.Amber * 0.15f : Color.Black * 0.3f) * fade);
            sb.DrawBorder(row, selected ? 2 : 1, (selected ? Hud.Amber : Hud.PanelEdge) * fade);
            GameText.Draw(sb, $"{i + 1:00}. {Loc.T($"ost.{i}")}", new Vector2(row.X + 12, row.Y + 8), (playing ? Color.White : Hud.Value) * fade);
            GameText.Draw(sb, Loc.T(playing ? "bonus.playing" : "bonus.play"), new Vector2(row.X + 12, row.Y + 32),
                (playing ? CabinPanel.Good : Hud.Label) * fade);

            // Çalan parçada küçük ekolayzer çubukları
            if (playing)
                for (int b = 0; b < 6; b++)
                {
                    int h = 6 + (int)((MathF.Sin(_time * (5f + b)) * 0.5f + 0.5f) * 20f);
                    sb.FillRect(new Rectangle(row.Right - 70 + b * 9, row.Bottom - 10 - h, 6, h), CabinPanel.Good * fade);
                }
        }

        // Destekçi rozeti ayarı
        var badgeRow = new Rectangle(list.X + 14, list.Y + 62 + AudioManager.SoundtrackLength * 66, list.Width - 28, 58);
        bool badgeSelected = _cursor == AudioManager.SoundtrackLength;
        sb.FillRect(badgeRow, (badgeSelected ? Hud.Amber * 0.15f : Color.Black * 0.3f) * fade);
        sb.DrawBorder(badgeRow, badgeSelected ? 2 : 1, (badgeSelected ? Hud.Amber : Hud.PanelEdge) * fade);
        UiDraw.SupporterBadge(sb, new Point(badgeRow.X + 12, badgeRow.Y + 6), fade, _time);
        GameText.Draw(sb, Hud.FitWidth(Loc.T(SaveData.Current.ShowSupporterBadge ? "bonus.badgeOn" : "bonus.badgeOff"), badgeRow.Width - 24),
            new Vector2(badgeRow.X + 12, badgeRow.Y + 34), Hud.Label * fade);
    }

    /// <summary>Özelleştirme: solda 4 yuva ve seçenekleri, sağda kabin önizlemesi.</summary>
    private void DrawCustomize(SpriteBatch sb, float fade)
    {
        DrawHeader(sb, Loc.T("custom.title"), fade);

        for (int slot = 0; slot < CustomSlots; slot++)
        {
            var items = CosmeticCatalog.For((CosmeticSlot)slot);
            var item = items[_customIndex[slot]];
            bool selected = slot == _cursor;
            bool equipped = DLCManager.Active((CosmeticSlot)slot).Id == item.Id;
            bool locked = !CosmeticCatalog.IsOwned(item);
            var row = new Rectangle(60, 96 + slot * 98, 560, 90);

            UiDraw.FancyPanel(sb, row, (selected ? Hud.Amber : Hud.PanelEdge) * fade);
            if (selected) sb.FillRect(row, Hud.Amber * (0.1f * fade));
            GameText.Draw(sb, Loc.T($"custom.slot{slot}"), new Vector2(row.X + 18, row.Y + 8), Hud.Amber * fade);

            // < Seçenek adı >
            string name = $"<  {item.Name}  >";
            GameText.Draw(sb, Hud.FitWidth(name, 440), new Vector2(row.X + 18, row.Y + 34), (locked ? Hud.Label * 0.6f : Color.White) * fade);
            GameText.Draw(sb, $"{_customIndex[slot] + 1}/{items.Length}", new Vector2(row.X + 18, row.Y + 62), Hud.Label * (0.7f * fade));

            string status = equipped ? Loc.T("skins.equipped") : locked ? (item.Dlc != null ? "DLC" : Loc.T("shop.micro")) : Loc.T("skins.equip");
            Color statusColor = equipped ? CabinPanel.Good : locked ? Hud.Danger : Hud.Amber;
            float width = GameText.MeasureLine(status);
            GameText.DrawRight(sb, status, row.Right - 18, row.Y + 12, statusColor * fade);
            if (locked) DrawLock(sb, new Point((int)(row.Right - 18 - width - 22), row.Y + 24), fade);

            // Duman ve radyo için renk örneği
            sb.FillRect(new Rectangle(row.Right - 70, row.Y + 48, 20, 20), item.Primary * fade);
            sb.FillRect(new Rectangle(row.Right - 46, row.Y + 48, 20, 20), item.Secondary * fade);
        }

        // Kabin önizlemesi: seçili (henüz kuşanılmamış olsa da) seçenekler
        var preview = new Rectangle(660, 104, 560, 330);
        UiDraw.FancyPanel(sb, preview, Hud.PanelEdge * fade);
        var inside = new Rectangle(preview.X + 12, preview.Y + 12, preview.Width - 24, preview.Height - 24);
        CabinDecor.DrawInterior(sb, inside,
            CosmeticCatalog.Paintings[_customIndex[1]], CosmeticCatalog.Radios[_customIndex[2]],
            CosmeticCatalog.Costumes[_customIndex[3]], fade, _time);

        float hintY = preview.Bottom + 14;
        foreach (string line in GameText.Wrap(Loc.T("custom.hint"), 540, 2))
        {
            GameText.Draw(sb, line, new Vector2(preview.X + 4, hintY), Hud.Label * (0.8f * fade));
            hintY += GameText.LineHeight(2) + 4;
        }
    }

    /// <summary>Kozmetik mağazası: her satırda içerik, yuvası, durum ve (varsa) açan görevin ilerlemesi.</summary>
    private void DrawCosmeticStore(SpriteBatch sb, float fade)
    {
        var items = CosmeticCatalog.MicroItems;
        for (int i = 0; i < items.Length; i++)
        {
            var item = items[i];
            bool owned = CosmeticCatalog.IsOwned(item);
            bool equipped = owned && DLCManager.Active(item.Slot).Id == item.Id;
            bool selected = i == _cursor;
            var row = new Rectangle(190, 120 + i * 66, 900, 60);
            UiDraw.FancyPanel(sb, row, (selected ? Hud.Amber : Hud.PanelEdge) * fade);
            if (selected) sb.FillRect(row, Hud.Amber * (0.12f * fade));

            // Renk örneği ve ad
            sb.FillRect(new Rectangle(row.X + 18, row.Y + 14, 16, 32), item.Primary * fade);
            sb.FillRect(new Rectangle(row.X + 36, row.Y + 14, 16, 32), item.Secondary * fade);
            GameText.Draw(sb, item.Name, new Vector2(row.X + 70, row.Y + 6), (selected ? Color.White : Hud.Value) * fade);
            string detail = Loc.T($"custom.slot{(int)item.Slot}");
            if (!owned && item.Quest != null)
            {
                var quest = QuestCatalog.Get(item.Quest);
                int progress = Math.Min(quest.Progress(SaveData.Current), quest.Target);
                detail += "   " + Loc.F("shop.quest", quest.Description, progress, quest.Target);
            }
            GameText.Draw(sb, Hud.FitWidth(detail, 600), new Vector2(row.X + 70, row.Y + 32), Hud.Label * fade);

            string status = equipped ? Loc.T("skins.equipped") : owned ? Loc.T("skins.equip")
                : SaveData.Current.CosmeticTickets > 0 ? Loc.T("shop.ticketUse") : Loc.T("shop.mtxBuy");
            Color color = equipped ? CabinPanel.Good : owned ? Hud.Amber : new Color(120, 190, 255);
            GameText.DrawRight(sb, status, row.Right - 18, row.Y + 6, color * fade); // Sağ üstte: alttaki görev satırıyla çakışmaz
        }

        GameText.DrawCentered(sb, Hud.FitWidth(Loc.T("shop.mtxNote") + "   " + Loc.F("shop.tickets", SaveData.Current.CosmeticTickets), 1100),
            CenterX, 120 + items.Length * 66 + 6, Hud.Label * (0.8f * fade));
    }

    /// <summary>Görevler: solda günlük, sağda haftalık 3'er görev; ilerleme, ödül ve yenilenmeye kalan süre.</summary>
    private void DrawContracts(SpriteBatch sb, float fade)
    {
        DrawHeader(sb, Loc.T("contracts.title"), fade);
        string Remaining(TimeSpan t) => t.TotalHours >= 24
            ? Loc.F("contracts.days", (int)t.TotalDays, t.Hours)
            : Loc.F("contracts.hours", (int)t.TotalHours, t.Minutes);

        void Column(string title, string reset, System.Collections.Generic.IEnumerable<ContractState> list, int x, Color accent)
        {
            GameText.Draw(sb, title, new Vector2(x, 92), accent * fade, 3);
            GameText.Draw(sb, reset, new Vector2(x, 126), Hud.Label * (0.8f * fade));
            int y = 156;
            foreach (var c in list)
            {
                var card = new Rectangle(x, y, 560, 110);
                UiDraw.FancyPanel(sb, card, (c.Completed ? CabinPanel.Good : accent) * fade);
                GameText.Draw(sb, Hud.FitWidth(c.Description, 520), new Vector2(card.X + 18, card.Y + 12), (c.Completed ? Hud.Label : Color.White) * fade);

                var bar = new Rectangle(card.X + 18, card.Y + 46, 380, 14);
                float ratio = (float)c.ProgressInt / c.Target;
                UiDraw.FancyBar(sb, bar, ratio, ratio, c.Completed ? CabinPanel.Good : accent);
                GameText.Draw(sb, $"{c.ProgressInt}/{c.Target}", new Vector2(bar.Right + 14, card.Y + 42), Hud.Value * fade);

                string reward = c.RewardTickets > 0
                    ? Loc.F("contract.rewardBoth", c.RewardCredits, c.RewardTickets)
                    : Loc.F("contract.rewardCredits", c.RewardCredits);
                GameText.Draw(sb, reward, new Vector2(card.X + 18, card.Y + 74), new Color(255, 215, 110) * fade);
                if (c.Completed)
                    GameText.DrawRight(sb, Loc.T("contracts.done"), card.Right - 18, card.Y + 74, CabinPanel.Good * fade);
                y += 122;
            }
        }

        Column(Loc.T("contracts.daily"), Loc.F("contracts.resetIn", Remaining(ContractManager.UntilDailyReset)),
            ContractManager.Daily, 60, new Color(120, 220, 255));
        Column(Loc.T("contracts.weekly"), Loc.F("contracts.resetIn", Remaining(ContractManager.UntilWeeklyReset)),
            ContractManager.Weekly, 660, new Color(200, 160, 255));

        string footer = Loc.F("contracts.footer", SaveData.Current.CosmeticTickets, SaveData.Current.ContractsCompleted);
        if (SeasonalEvents.RewardMultiplier > 1f)
            footer += "   " + Loc.F("contracts.seasonBonus", Loc.T(SeasonalEvents.Key), (int)MathF.Round((SeasonalEvents.RewardMultiplier - 1f) * 100f));
        GameText.DrawCentered(sb, Hud.FitWidth(footer, 1180), CenterX, 532, Hud.Label * fade);
    }

    /// <summary>Modlar: yüklenen topluluk modları (Steam Workshop dahil), içerik sayıları ve hatalar.</summary>
    private void DrawMods(SpriteBatch sb, float fade)
    {
        DrawHeader(sb, Loc.T("mods.title"), fade);
        var mods = Modding.ModLoader.Mods;
        if (mods.Count == 0)
        {
            GameText.DrawCentered(sb, Loc.T("mods.none"), CenterX, 200, Hud.Value * fade, 3);
            float y = 260;
            foreach (string line in GameText.Wrap(Loc.T("mods.howto"), 1000, 2))
            {
                GameText.DrawCentered(sb, line, CenterX, y, Hud.Label * fade);
                y += GameText.LineHeight(2) + 6;
            }
        }

        for (int i = 0; i < Math.Min(mods.Count, 6); i++)
        {
            var mod = mods[i];
            var card = new Rectangle(140, 100 + i * 80, 1000, 72);
            UiDraw.FancyPanel(sb, card, (mod.Errors.Count == 0 ? Hud.PanelEdge : Hud.Danger) * fade);
            string title = $"{mod.Name}  v{mod.Version}  -  {mod.Author}" + (mod.FromWorkshop ? "  [WORKSHOP]" : "");
            GameText.Draw(sb, Hud.FitWidth(title, 960), new Vector2(card.X + 18, card.Y + 8), Color.White * fade);
            string content = Loc.F("mods.content", mod.Skins, mod.Dialogs, mod.Stops, mod.Strings);
            GameText.Draw(sb, content, new Vector2(card.X + 18, card.Y + 30), Hud.Label * fade);
            if (mod.Errors.Count > 0)
                GameText.Draw(sb, Hud.FitWidth(Loc.F("mods.error", mod.Errors[0]), 960), new Vector2(card.X + 18, card.Y + 50), Hud.Danger * fade);
        }

        // Klasör yolu (piksel yazıda ters bölü olmadığı için / ile gösterilir) ve Workshop durumu
        string folder = Modding.ModLoader.UserModFolder.Replace('\\', '/');
        GameText.DrawCentered(sb, Hud.FitWidth(Loc.F("mods.folder", folder), 1180), CenterX, 590, Hud.Label * (0.8f * fade));
        GameText.DrawCentered(sb, Loc.T(AchievementManager.Platform.IsOnline ? "mods.workshopOn" : "mods.workshopOff"),
            CenterX, 618, Hud.Label * (0.7f * fade));
    }

    /// <summary>Buzul Kıyameti: 5 bölüm kartı (sırayla açılır).</summary>
    private void DrawWasteland(SpriteBatch sb, float fade)
    {
        DrawHeader(sb, Loc.T("wasteland.title"), fade);
        GameText.DrawCentered(sb, Hud.FitWidth(Loc.T("wasteland.subtitle"), 1100), CenterX, 90, Hud.Label * fade);

        const int cardWidth = 224, cardHeight = 330, gap = 10;
        int left = (int)(CenterX - (Chapters.Wasteland.Length * cardWidth + (Chapters.Wasteland.Length - 1) * gap) / 2f);
        int unlocked = SaveData.Current.WastelandUnlocked;
        for (int i = 0; i < Chapters.Wasteland.Length; i++)
        {
            var chapter = Chapters.Wasteland[i];
            bool open = i + 1 <= unlocked;
            bool done = i + 1 < unlocked || (i == Chapters.Wasteland.Length - 1 && SaveData.Current.WastelandCleared);
            bool selected = i == _cursor;
            int lift = selected ? (int)(MathF.Sin(_time * 4f) * 2f) - 4 : 0;
            var card = new Rectangle(left + i * (cardWidth + gap), 130 + lift, cardWidth, cardHeight);

            UiDraw.FancyPanel(sb, card, (selected ? DlcAccent(DlcId.FrozenWasteland) : Hud.PanelEdge) * fade);
            var art = new Rectangle(card.X + 10, card.Y + 10, card.Width - 20, 120);
            DrawThemeArt(sb, art, chapter.Theme, fade * (open ? 1f : 0.3f));
            GameText.Draw(sb, Loc.F("chapter.label", chapter.Number), new Vector2(card.X + 14, card.Y + 140), Hud.Amber * fade);
            float y = card.Y + 168;
            foreach (string line in GameText.Wrap(chapter.Name, card.Width - 28, 2))
            {
                GameText.Draw(sb, line, new Vector2(card.X + 14, y), (open ? Color.White : Hud.Label * 0.6f) * fade);
                y += GameText.LineHeight(2) + 4;
            }
            GameText.Draw(sb, chapter.RemainingRange, new Vector2(card.X + 14, card.Bottom - 60), Hud.Label * fade);
            if (!open) DrawLock(sb, art.Center, fade);
            else if (done) GameText.Draw(sb, Loc.T("wasteland.done"), new Vector2(card.X + 14, card.Bottom - 32), CabinPanel.Good * fade);
            else if (selected) GameText.Draw(sb, Loc.T("chapters.start"), new Vector2(card.X + 14, card.Bottom - 32), Hud.Amber * fade);
        }

        GameText.DrawCentered(sb, Hud.FitWidth(Chapters.Wasteland[_cursor].Effect.TrimStart('>', ' '), 1100), CenterX, 486,
            DlcAccent(DlcId.FrozenWasteland) * fade);
    }
}
