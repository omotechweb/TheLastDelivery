using System;
using System.Linq;
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
using TheLastDelivery.Trade;

namespace TheLastDelivery.UI;

public enum MetaScreen { Main, Chapters, Shop, Skins, Dlc, Bonuses, Customize, Wasteland, Mods, Contracts, Episodes }

public enum MetaAction { None, StartChapter, StartEndless, StartSpeedrun, StartWasteland, StartHardcore, OpenLanguage, Quit }

/// <summary>
/// Ana menü ve alt ekranları:
///   OYNA            → 20 bölümlük seçim ızgarası (bölümler sırayla açılır; kilit SaveData'da)
///   SONSUZ KURYE    → sınırsız hat, rekor km
///   ZAMANLA YARIŞ   → süre sınırlı kısa teslimat, rekor süre
///   MAĞAZA          → Krediyle kalıcı tren geliştirmeleri
///   SKİNLER         → tren görünümleri (bazıları başarımla ya da DLC ile açılır) ve kabin temaları
///   DLC & EK İÇERİKLER → 4 DLC: sahibi içeriğe gider, değilse "Steam Mağazasında İncele" (MetaMenu.Dlc.cs)
///   BONUSLAR        → konsept çizimler ve dijital soundtrack (Supporter Pack)
///   DİL, ÇIKIŞ
/// Skin listesinde gezinirken sahnedeki tren o skinle önizlenir.
/// </summary>
public sealed partial class MetaMenu
{
    private const float CenterX = GameConfig.ScreenWidth / 2f;
    private const int GridColumns = 8; // 40 bölüm: 8 x 5
    private const int VisibleSkinRows = 10;

    private static readonly string[] MainItems =
    {
        "menu.play", "menu.endless", "menu.speedrun", "menu.contracts", "menu.shop", "menu.skins",
        "menu.dlc", "menu.bonuses", "menu.mods", "menu.langOpt", "menu.quitOpt"
    };

    private int _skinScroll; // Skin listesi 10 satırdan uzun: kaydırılarak gösterilir

    // DENEME KODU (yayından önce kaldır): sağ üstteki kod alanı. [TAB] odaklar, ENTER gönderir.
    private bool _codeFocused;
    private string _codeText = "";

    /// <summary>Kod alanı yazı bekliyor mu? (ESC oyundan çıkmak yerine alanı kapatır.)</summary>
    public bool CodeFocused => _codeFocused && Screen == MetaScreen.Main;

    public void UnfocusCode() => _codeFocused = false;

    /// <summary>Pencereden gelen karakter girişi (Game1, Window.TextInput üzerinden iletir).</summary>
    public void OnTextInput(char c)
    {
        if (!CodeFocused) return;
        // Ctrl basılıyken gelen harfler kısayoldur (Ctrl+V yapıştırma), yazıya eklenmez
        var keys = Keyboard.GetState();
        if (keys.IsKeyDown(Keys.LeftControl) || keys.IsKeyDown(Keys.RightControl)) return;
        AppendCode(c.ToString());
    }

    private void AppendCode(string text)
    {
        foreach (char c in text)
        {
            if (_codeText.Length >= 24) break;
            if (char.IsLetterOrDigit(c)) _codeText += char.ToLowerInvariant(c);
        }
    }

    private int _cursor;
    private float _time;
    private float _screenTime;
    private string _message;
    private Color _messageColor;
    private float _messageTimer;

    public MetaScreen Screen { get; private set; }

    /// <summary>Bölüm seçiminde onaylanan bölüm (1-40).</summary>
    public int ChosenChapter { get; private set; } = 1;

    /// <summary>Bölüm ızgarasının ait olduğu episode: 1 "The Great Freeze", 2 "Project AURA".</summary>
    public int ChosenEpisode { get; private set; } = 1;

    private Chapter[] EpisodeChapters => ChosenEpisode == 2 ? Chapters.Episode2 : Chapters.All;
    private int EpisodeUnlocked => ChosenEpisode == 2 ? SaveData.Current.Episode2Unlocked : SaveData.Current.UnlockedChapter;

    /// <summary>Episode 2'ye geçiş: Episode 1 bitmemişse uyarı (deneme kodu açıkken serbest).</summary>
    private static bool Episode2Available => SaveData.Current.Episode1Complete || SaveData.Current.DevUnlockAll;

    /// <summary>Episode seçim ekranını açar (imleç verilen episode'da).</summary>
    private void OpenEpisodes(int cursor)
    {
        Open(MetaScreen.Episodes);
        _cursor = cursor;
    }

    /// <summary>Sahnedeki trende gösterilecek skin (Skinler ekranında imlecin üstündeki, değilse kuşanılan).</summary>
    public string PreviewSkinId =>
        Screen == MetaScreen.Skins && _cursor < Cosmetics.ShopSkins.Length
            ? Cosmetics.ShopSkins[_cursor].Id
            : SaveData.Current.EquippedSkin;

    public void Open(MetaScreen screen)
    {
        Screen = screen;
        _cursor = screen switch
        {
            MetaScreen.Chapters => Math.Clamp(EpisodeUnlocked - 1, 0, EpisodeChapters.Length - 1),
            MetaScreen.Episodes => ChosenEpisode - 1,
            MetaScreen.Wasteland => Math.Clamp(SaveData.Current.WastelandUnlocked - 1, 0, Chapters.Wasteland.Length - 1),
            _ => 0
        };
        _screenTime = 0f;
        _message = null;
        _skinScroll = 0;
        _shopTab = 0;
        _hardcoreConfirm = false;
        if (screen != MetaScreen.Bonuses) PlayingTrack = -1; // Bonuslardan çıkınca soundtrack susar
        if (screen == MetaScreen.Customize) LoadCustomizeSelection();
        if (screen == MetaScreen.Dlc) DLCManager.Refresh();   // Mağazadan dönüldüyse yeni satın alımları gör
        if (screen == MetaScreen.Contracts) ContractManager.Refresh(); // Gece yarısı geçtiyse yeni görevler
    }

    /// <summary>ESC: alt ekrandan ana menüye döner. Ana menüdeyse false (oyundan çık).</summary>
    public bool Back()
    {
        if (Screen == MetaScreen.Main) return false;
        if (Screen == MetaScreen.Chapters) OpenEpisodes(ChosenEpisode - 1);
        else Open(MetaScreen.Main);
        return true;
    }

    // ------------------------------------------------------------------
    // Güncelleme
    // ------------------------------------------------------------------

    public MetaAction Update(float dt, InputState input, ProceduralAudio audio)
    {
        _time += dt;
        _screenTime += dt;
        _messageTimer -= dt;

        // DLC ekranları (DLC listesi, bonuslar, özelleştirme, Buzul Kıyameti, modlar, mağazanın DLC ve kozmetik sekmeleri)
        if (Screen is MetaScreen.Dlc or MetaScreen.Bonuses or MetaScreen.Customize or MetaScreen.Wasteland or MetaScreen.Mods
                or MetaScreen.Contracts
            || Screen == MetaScreen.Shop && _shopTab > 0)
        {
            if (Screen == MetaScreen.Shop && input.AnyPressed(Keys.A, Keys.Left, Keys.D, Keys.Right))
            {
                SwitchShopTab(audio, input.AnyPressed(Keys.A, Keys.Left) ? -1 : 1);
                return MetaAction.None;
            }
            if (input.Pressed(Keys.Back))
            {
                audio.PlayClick();
                Open(Screen is MetaScreen.Bonuses or MetaScreen.Customize or MetaScreen.Wasteland ? MetaScreen.Dlc : MetaScreen.Main);
                return MetaAction.None;
            }
            return UpdateExtras(input, audio);
        }

        if (Screen == MetaScreen.Shop && input.AnyPressed(Keys.A, Keys.Left, Keys.D, Keys.Right))
        {
            SwitchShopTab(audio, input.AnyPressed(Keys.A, Keys.Left) ? -1 : 1);
            return MetaAction.None;
        }

        int count = Screen switch
        {
            MetaScreen.Main => MainItems.Length,
            MetaScreen.Chapters => EpisodeChapters.Length,
            MetaScreen.Episodes => 2,
            MetaScreen.Shop => UpgradeCatalog.All.Length,
            _ => Cosmetics.ShopSkins.Length + Cosmetics.CabinThemes.Length
        };

        // Kod alanı: odaktayken menü gezinmesi durur, tuşlar yazıya gider
        if (Screen == MetaScreen.Main && input.Pressed(Keys.Tab))
        {
            _codeFocused = !_codeFocused;
            audio.PlayClick();
            return MetaAction.None;
        }
        if (CodeFocused)
        {
            if (input.Pressed(Keys.Back) && _codeText.Length > 0) _codeText = _codeText[..^1];

            // Yapıştır: Ctrl+V ya da Shift+Insert
            bool ctrl = input.Down(Keys.LeftControl) || input.Down(Keys.RightControl);
            bool shift = input.Down(Keys.LeftShift) || input.Down(Keys.RightShift);
            if (ctrl && input.Pressed(Keys.V) || shift && input.Pressed(Keys.Insert))
            {
                AppendCode(DevCheats.ReadClipboard());
                audio.PlayClick();
            }
            if (input.Pressed(Keys.Enter))
            {
                bool ok = DevCheats.TryApply(_codeText);
                Show(Loc.T(ok ? "dev.ok" : "dev.wrong"), ok ? CabinPanel.Good : Hud.Danger);
                if (ok) audio.PlaySuccess(); else audio.PlayError();
                _codeText = "";
                _codeFocused = false;
            }
            return MetaAction.None;
        }

        if (Screen == MetaScreen.Chapters)
            MoveInGrid(input, count, audio);
        else
            MoveInList(input, count, audio);

        if (input.Pressed(Keys.Back) && Screen != MetaScreen.Main)
        {
            audio.PlayClick();
            Back();
            return MetaAction.None;
        }

        if (!input.AnyPressed(Keys.Enter, Keys.Space) || _screenTime < 0.15f)
            return MetaAction.None;

        switch (Screen)
        {
            case MetaScreen.Main:
                audio.PlayConfirm();
                switch (_cursor)
                {
                    case 0: OpenEpisodes(ChosenEpisode - 1); break;
                    case 1: return MetaAction.StartEndless;
                    case 2: return MetaAction.StartSpeedrun;
                    case 3: Open(MetaScreen.Contracts); break;
                    case 4: Open(MetaScreen.Shop); break;
                    case 5: Open(MetaScreen.Skins); break;
                    case 6: Open(MetaScreen.Dlc); break;
                    case 7:
                        if (DLCManager.Owns(DlcId.Supporter))
                        {
                            Open(MetaScreen.Bonuses);
                        }
                        else
                        {
                            // Bonuslar Supporter Pack ile gelir: DLC ekranında paketin üstüne götür
                            Open(MetaScreen.Dlc);
                            _cursor = Array.FindIndex(DLCManager.All, d => d.Id == DlcId.Supporter);
                            Show(Loc.T("bonus.needSupporter"), new Color(255, 215, 110));
                        }
                        break;
                    case 8: Open(MetaScreen.Mods); break;
                    case 9: return MetaAction.OpenLanguage;
                    default: return MetaAction.Quit;
                }
                return MetaAction.None;

            case MetaScreen.Episodes:
                if (_cursor == 1 && !Episode2Available)
                {
                    Show(Loc.T("ep.needEp1"), Hud.Danger);
                    audio.PlayError();
                    return MetaAction.None;
                }
                ChosenEpisode = _cursor + 1;
                audio.PlayConfirm();
                Open(MetaScreen.Chapters);
                return MetaAction.None;

            case MetaScreen.Chapters:
                if (_cursor + 1 > EpisodeUnlocked)
                {
                    Show(Loc.F("chapters.unlockHint", _cursor), Hud.Danger);
                    audio.PlayError();
                    return MetaAction.None;
                }
                ChosenChapter = _cursor + 1;
                audio.PlayConfirm();
                return MetaAction.StartChapter;

            case MetaScreen.Shop:
                BuyUpgrade(UpgradeCatalog.All[_cursor], audio);
                return MetaAction.None;

            default:
                SelectCosmetic(audio);
                return MetaAction.None;
        }
    }

    private void MoveInList(InputState input, int count, ProceduralAudio audio)
    {
        if (input.AnyPressed(Keys.W, Keys.Up)) { _cursor = (_cursor - 1 + count) % count; audio.PlayClick(); }
        if (input.AnyPressed(Keys.S, Keys.Down)) { _cursor = (_cursor + 1) % count; audio.PlayClick(); }
        if (Screen == MetaScreen.Episodes && input.AnyPressed(Keys.A, Keys.Left, Keys.D, Keys.Right)) { _cursor = 1 - _cursor; audio.PlayClick(); }

        // Skin ekranında sol/sağ sütunlar arasında geçiş
        if (Screen == MetaScreen.Skins && input.AnyPressed(Keys.A, Keys.Left, Keys.D, Keys.Right))
        {
            int skins = Cosmetics.ShopSkins.Length;
            _cursor = _cursor < skins
                ? skins + Math.Min(_cursor, Cosmetics.CabinThemes.Length - 1)
                : _cursor - skins;
            audio.PlayClick();
        }
    }

    /// <summary>5 sütunlu bölüm ızgarasında dört yönlü gezinme.</summary>
    private void MoveInGrid(InputState input, int count, ProceduralAudio audio)
    {
        int before = _cursor;
        if (input.AnyPressed(Keys.A, Keys.Left)) _cursor = (_cursor - 1 + count) % count;
        if (input.AnyPressed(Keys.D, Keys.Right)) _cursor = (_cursor + 1) % count;
        if (input.AnyPressed(Keys.W, Keys.Up)) _cursor = (_cursor - GridColumns + count) % count;
        if (input.AnyPressed(Keys.S, Keys.Down)) _cursor = (_cursor + GridColumns) % count;
        if (_cursor != before) audio.PlayClick();
    }

    private void BuyUpgrade(UpgradeType upgrade, ProceduralAudio audio)
    {
        if (SaveData.HasUpgrade(upgrade)) { Show(Loc.T("trade.owned"), Hud.Label); audio.PlayError(); return; }
        if (!SaveData.TryBuyUpgrade(upgrade)) { Show(Loc.T("trade.noCredits"), Hud.Danger); audio.PlayError(); return; }
        Show(Loc.F("trade.upgraded", UpgradeCatalog.Name(upgrade)), CabinPanel.Good);
        audio.PlaySuccess();
    }

    private void SelectCosmetic(ProceduralAudio audio)
    {
        bool ok;
        string name;
        if (_cursor < Cosmetics.ShopSkins.Length)
        {
            var skin = Cosmetics.ShopSkins[_cursor];
            if (!DLCManager.Owns(skin.Dlc))
            {
                // DLC skini: sahibi değilse mağaza sayfasına yönlendir
                if (!DLCManager.ViewInStore(skin.Dlc.Value)) Show(Loc.T("dlc.storeOffline"), Hud.Label);
                audio.PlayClick();
                return;
            }
            if (!SaveData.Current.OwnedSkins.Contains(skin.Id) && !Achievements.IsUnlocked(skin.Achievement))
            {
                Show(Loc.F("skins.needAch", Achievements.Get(skin.Achievement).Name), Hud.Danger);
                audio.PlayError();
                return;
            }
            ok = SaveData.TryBuyOrEquipSkin(skin);
            name = skin.Name;
        }
        else
        {
            var theme = Cosmetics.CabinThemes[_cursor - Cosmetics.ShopSkins.Length];
            if (!DLCManager.Owns(theme.Dlc))
            {
                // DLC kabini (Hologram kokpit): sahibi değilse mağaza sayfası
                if (!DLCManager.ViewInStore(theme.Dlc.Value)) Show(Loc.T("dlc.storeOffline"), Hud.Label);
                audio.PlayClick();
                return;
            }
            ok = SaveData.TryBuyOrEquipTheme(theme);
            name = theme.Name;
        }

        if (!ok) { Show(Loc.T("trade.noCredits"), Hud.Danger); audio.PlayError(); return; }
        Show(Loc.F("skins.equippedMsg", name), CabinPanel.Good);
        audio.PlayCoin();
    }

    private void Show(string text, Color color)
    {
        _message = text;
        _messageColor = color;
        _messageTimer = 2.5f;
    }

    // ------------------------------------------------------------------
    // Çizim
    // ------------------------------------------------------------------

    public void Draw(SpriteBatch sb)
    {
        float fade = MathHelper.Clamp(_screenTime / 0.35f, 0f, 1f);

        switch (Screen)
        {
            case MetaScreen.Main: DrawMain(sb); break;
            case MetaScreen.Chapters: DrawChapters(sb, fade); break;
            case MetaScreen.Episodes: DrawEpisodes(sb, fade); break;
            case MetaScreen.Shop: DrawShop(sb, fade); break;
            case MetaScreen.Skins: DrawSkins(sb, fade); break;
            default: DrawExtras(sb, fade); break;
        }

        // Her ekranda sağ üstte Kredi bakiyesi
        TraderScreen.DrawCredits(sb, GameConfig.ScreenWidth - 28, 24);

        if (_message != null && _messageTimer > 0f)
        {
            float alpha = MathHelper.Clamp(_messageTimer, 0f, 1f);
            float width = GameText.MeasureLine(_message) + 40f;
            sb.FillRect(new Rectangle((int)(CenterX - width / 2f), 630, (int)width, 34), Color.Black * (0.8f * alpha));
            GameText.DrawCentered(sb, _message, CenterX, 636, _messageColor * alpha);
        }

        string nav = Screen switch
        {
            MetaScreen.Chapters => Loc.T("menu.navGrid"),
            MetaScreen.Wasteland => Loc.T("menu.navH"),
            MetaScreen.Customize => Loc.T("menu.navCustomize"),
            MetaScreen.Bonuses => Loc.T("menu.navBonuses"),
            MetaScreen.Shop or MetaScreen.Skins => Loc.T("menu.navTabs"),
            MetaScreen.Mods or MetaScreen.Contracts => Loc.T("menu.navBack"),
            _ => Loc.T("menu.nav")
        };
        GameText.DrawCentered(sb, nav, CenterX, 684, Hud.Label * 0.7f);
    }

    private void DrawMain(SpriteBatch sb)
    {
        float fade = MathHelper.Clamp(_time / 1.2f, 0f, 1f);
        sb.FillRect(new Rectangle(0, 0, GameConfig.ScreenWidth, GameConfig.ScreenHeight), Color.Black * 0.35f);
        sb.FillRect(new Rectangle(0, 0, GameConfig.ScreenWidth, 420), new Color(4, 6, 14) * 0.45f);

        // Sol üstte dönemsel etkinlik afişi (Kış Şenliği, Hayalet Haftası, Fırtına Günü)
        if (SeasonalEvents.Key != null)
        {
            string name = Loc.T(SeasonalEvents.Key);
            string effect = Loc.T(SeasonalEvents.Key + ".desc");
            var color = SeasonalEvents.Current switch
            {
                Season.WinterFestival => new Color(120, 230, 150),
                Season.GhostWeek => new Color(255, 150, 60),
                _ => new Color(150, 200, 255)
            };
            float w = MathF.Max(GameText.MeasureLine(name), GameText.MeasureLine(effect)) + 28f;
            var banner = new Rectangle(16, 624, (int)w, 54); // Sol alt: başlık ve menüyle çakışmaz
            UiDraw.FancyPanel(sb, banner, color * fade);
            GameText.Draw(sb, name, new Vector2(banner.X + 14, banner.Y + 6), color * fade);
            GameText.Draw(sb, effect, new Vector2(banner.X + 14, banner.Y + 30), Hud.Label * fade);
        }

        DrawCodeField(sb, fade);

        const string title = "THE LAST DELIVERY";
        GameText.DrawCentered(sb, title, CenterX + 4, 50, Color.Black * (0.6f * fade), 7, Language.English);
        GameText.DrawCentered(sb, title, CenterX, 46, Hud.Amber * fade, 7, Language.English);
        GameText.DrawCentered(sb, Loc.T("menu.subtitle"), CenterX, 128, Hud.Label * fade, 2);
        sb.FillRect(new Rectangle((int)CenterX - 180, 156, 360, 2), Hud.Amber * (0.4f * fade));

        for (int i = 0; i < MainItems.Length; i++)
        {
            bool selected = i == _cursor;
            int y = 164 + i * 31;
            var rect = new Rectangle((int)CenterX - 210, y, 420, 29);
            if (selected)
            {
                UiDraw.FancyPanel(sb, rect, Hud.Amber * fade);
                float bob = MathF.Sin(_time * 6f) * 3f;
                sb.FillRect(new Rectangle(rect.X - 22 + (int)bob, rect.Center.Y - 6, 10, 12), Hud.Amber * fade);
                sb.FillRect(new Rectangle(rect.Right + 12 - (int)bob, rect.Center.Y - 6, 10, 12), Hud.Amber * fade);
            }
            string label = Loc.T(MainItems[i]);
            GameText.DrawCentered(sb, label, CenterX, y + 1, (selected ? Color.White : Hud.Label) * fade, 3);

            // DLC sayacı ve kilitli bonuslar
            float labelEnd = CenterX + GameText.MeasureLine(label, 3) / 2f;
            if (i == 3)
            {
                int ready = ContractManager.Daily.Concat(ContractManager.Weekly).Count(c => !c.Completed);
                GameText.Draw(sb, $"{ready}", new Vector2(labelEnd + 12, y + 6), new Color(120, 220, 255) * fade);
            }
            else if (i == 6)
                GameText.Draw(sb, $"{DlcOwnedCount}/{DLCManager.All.Length}", new Vector2(labelEnd + 12, y + 8), new Color(255, 215, 110) * fade);
            else if (i == 7 && !DLCManager.Owns(DlcId.Supporter))
                DrawLock(sb, new Point((int)labelEnd + 24, rect.Center.Y), fade);
        }

        // Seçili modun kısa açıklaması, rekorlar ve başarımlar (arkadaki trenin üstünde okunsun diye koyu zemin)
        var info = new Rectangle((int)CenterX - 330, 518, 660, 100);
        sb.FillRect(info, new Color(4, 6, 14) * (0.78f * fade));
        sb.DrawBorder(info, 1, Hud.PanelEdge * (0.6f * fade));

        string hint = _cursor switch
        {
            0 => Loc.F("menu.play.desc", SaveData.Current.UnlockedChapter, Chapters.All.Length),
            1 => Loc.T("menu.endless.desc"),
            2 => Loc.F("menu.speedrun.desc", GameConfig.SpeedrunDistanceKm, Endings.FormatTime(GameConfig.SpeedrunTimeLimit)),
            3 => Loc.F("menu.contracts.desc", SaveData.Current.CosmeticTickets),
            6 => Loc.T("menu.dlc.desc"),
            7 => Loc.T(DLCManager.Owns(DlcId.Supporter) ? "menu.bonuses.desc" : "bonus.needSupporter"),
            8 => Loc.F("menu.mods.desc", Modding.ModLoader.Mods.Count),
            _ => null
        };
        if (hint != null)
            GameText.DrawCentered(sb, Hud.FitWidth(hint, 640), CenterX, 526, Hud.Value * (0.85f * fade));

        var save = SaveData.Current;
        string endless = Loc.F("menu.record.endless", save.EndlessBestKm);
        string speedrun = save.SpeedrunBestSeconds > 0f
            ? Loc.F("menu.record.speedrun", Endings.FormatTime(save.SpeedrunBestSeconds))
            : Loc.T("menu.record.speedrunNone");
        // Sonsuz kurye seçiliyken rekor satırında liderlik tablosunun ilk üçü
        var board = LeaderboardManager.Endless;
        if (_cursor == 1 && board.Count > 0)
        {
            var top = new System.Text.StringBuilder(Loc.T("menu.leaderboard"));
            for (int i = 0; i < Math.Min(3, board.Count); i++)
                top.Append("  ").Append(Loc.F("menu.leaderboard.entry", i + 1, board[i].DistanceKm, board[i].Score));
            GameText.DrawCentered(sb, Hud.FitWidth(top.ToString(), 640), CenterX, 556, new Color(255, 215, 110) * (0.9f * fade));
        }
        else
        {
            GameText.DrawCentered(sb, $"{endless}    {speedrun}", CenterX, 556, new Color(255, 215, 110) * (0.9f * fade));
        }
        GameText.DrawCentered(sb, Loc.F("menu.achievements", save.Achievements.Count, Achievements.All.Length) + "    "
            + Loc.F("menu.endings", save.EndingsSeen.Count, EndingIds.All.Length), CenterX, 586, Hud.Label * (0.8f * fade));
    }

    /// <summary>DENEME KODU (yayından önce kaldır): sağ üstte, Kredi bakiyesinin altında kod giriş kutusu.</summary>
    private void DrawCodeField(SpriteBatch sb, float fade)
    {
        var box = new Rectangle(GameConfig.ScreenWidth - 264, 60, 240, 30);
        sb.FillRect(box, Color.Black * (0.6f * fade));
        sb.DrawBorder(box, _codeFocused ? 2 : 1, (_codeFocused ? Hud.Amber : Hud.PanelEdge) * fade);
        if (_codeFocused)
        {
            string shown = _codeText + (MathF.Sin(_time * 8f) > 0f ? "_" : " ");
            // Uzun kodun sonu görünsün
            while (GameText.MeasureLine(shown) > box.Width - 16 && shown.Length > 1) shown = shown[1..];
            GameText.Draw(sb, shown, new Vector2(box.X + 8, box.Y + 4), Color.White * fade, 2, Language.English);
        }
        else
        {
            GameText.Draw(sb, Loc.T("dev.hint"), new Vector2(box.X + 8, box.Y + 4), Hud.Label * (0.8f * fade));
        }
        if (DevCheats.IsActive)
            GameText.DrawRight(sb, Loc.T("dev.active"), box.Right, box.Bottom + 4, new Color(255, 215, 110) * fade);
    }

    private void DrawHeader(SpriteBatch sb, string title, float fade)
    {
        sb.FillRect(new Rectangle(0, 0, GameConfig.ScreenWidth, GameConfig.ScreenHeight), new Color(4, 6, 14) * (0.72f * fade));
        GameText.DrawCentered(sb, title, CenterX, 22, Hud.Amber * fade, 4);
        sb.FillRect(new Rectangle((int)CenterX - 220, 74, 440, 2), Hud.Amber * (0.4f * fade));
    }

    /// <summary>40 bölüm kartı (8x5): tema şeridi, numara, ad, kilit durumu; seçilinin ayrıntısı altta.
    /// Hikâye bölümleri kitap işaretiyle, kritik bölümler kırmızı şeritle gösterilir.</summary>
    private void DrawChapters(SpriteBatch sb, float fade)
    {
        DrawHeader(sb, Loc.T(ChosenEpisode == 2 ? "ep2.title" : "ep1.title"), fade);

        const int cardWidth = 146, cardHeight = 92, gap = 7;
        int left = (GameConfig.ScreenWidth - (GridColumns * cardWidth + (GridColumns - 1) * gap)) / 2;
        int unlockedCount = EpisodeUnlocked;
        var list = EpisodeChapters;
        bool lastDone = ChosenEpisode == 2 ? SaveData.Current.Episode2Complete : SaveData.Current.Delivered;

        for (int i = 0; i < list.Length; i++)
        {
            var chapter = list[i];
            bool unlocked = i + 1 <= unlockedCount;
            bool completed = i + 1 < unlockedCount || (i + 1 == list.Length && lastDone);
            bool selected = i == _cursor;
            int lift = selected ? (int)(MathF.Sin(_time * 4f) * 2f) - 3 : 0;
            var card = new Rectangle(left + (i % GridColumns) * (cardWidth + gap), 90 + (i / GridColumns) * (cardHeight + gap) + lift,
                cardWidth, cardHeight);

            UiDraw.FancyPanel(sb, card, (selected ? Hud.Amber : Hud.PanelEdge) * fade);
            if (selected) sb.FillRect(card, Hud.Amber * (0.10f * fade));

            var art = new Rectangle(card.X + 6, card.Y + 6, card.Width - 12, 30);
            DrawThemeArt(sb, art, chapter.Theme, fade * (unlocked ? 1f : 0.3f));
            if (chapter.IsStoryScene) // Hikâye bölümü: açık kitap işareti
            {
                var book = new Rectangle(art.Right - 22, art.Y + 4, 18, 13);
                sb.FillRect(book, new Color(240, 230, 200) * fade);
                sb.FillRect(new Rectangle(book.Center.X - 1, book.Y, 2, book.Height), new Color(140, 100, 60) * fade);
            }
            else if (chapter.Critical)
                sb.FillRect(new Rectangle(art.X, art.Bottom - 3, art.Width, 3), Hud.Danger * (0.8f * fade));

            GameText.Draw(sb, Loc.F("chapter.label", chapter.Number), new Vector2(card.X + 8, card.Y + 40), Hud.Amber * fade);
            GameText.Draw(sb, Hud.FitWidth(chapter.Name, card.Width - 16), new Vector2(card.X + 8, card.Y + 64),
                (unlocked ? Color.White : Hud.Label * 0.6f) * fade);

            if (!unlocked)
                DrawLock(sb, art.Center, fade);
            else if (completed)
                DrawCheck(sb, new Point(card.Right - 20, card.Y + 46), fade);
            if (chapter.RequiresEpisode2Dlc && !DLCManager.DLC_EPISODE2_UNLOCKED) // DLC rozeti
            {
                var badge = new Rectangle(card.Right - 42, card.Y + 4, 38, 16);
                sb.FillRect(badge, new Color(120, 14, 20) * fade);
                GameText.DrawCentered(sb, "DLC", badge.Center.X, badge.Y, Color.White * fade, 1, Language.English);
            }
        }

        // Seçili bölümün adı, türü ve etkisi
        var current = list[_cursor];
        bool open = _cursor + 1 <= unlockedCount;
        string kind = current.IsStoryScene ? Loc.T("chapters.story") : Loc.T($"cth.{current.ThemeId}.name");
        string detail = open
            ? $"{current.Name}  -  {kind}  -  {current.Effect.TrimStart('>', ' ')}"
            : Loc.F("chapters.unlockHint", _cursor);
        GameText.DrawCentered(sb, Hud.FitWidth(detail, GameConfig.ScreenWidth - 80), CenterX, 600,
            (open ? Hud.Value : Hud.Danger) * fade);
    }

    /// <summary>
    /// Episode seçimi: iki büyük kart. Episode 1 "The Great Freeze" (ücretsiz, 40 bölüm), Episode 2 "Project AURA"
    /// (Bölüm 1-2 ücretsiz, 3-40 DLC). Episode 1 bitmeden Episode 2'ye girilemez; aktarılacak karma ve yolcular gösterilir.
    /// </summary>
    private void DrawEpisodes(SpriteBatch sb, float fade)
    {
        DrawHeader(sb, Loc.T("ep.title"), fade);
        var save = SaveData.Current;
        for (int i = 0; i < 2; i++)
        {
            bool selected = i == _cursor;
            bool ep2 = i == 1;
            int lift = selected ? (int)(MathF.Sin(_time * 4f) * 3f) - 4 : 0;
            var card = new Rectangle(110 + i * 540, 110 + lift, 520, 470);
            var accent = ep2 ? new Color(255, 60, 70) : new Color(150, 210, 255);
            bool locked = ep2 && !Episode2Available;
            UiDraw.FancyPanel(sb, card, (selected ? accent : Hud.PanelEdge) * fade);
            if (selected) sb.FillRect(card, accent * (0.07f * fade));

            // Kapak resmi
            var art = new Rectangle(card.X + 16, card.Y + 16, card.Width - 32, 190);
            if (ep2) DrawEpisode2Art(sb, art, fade * (locked ? 0.4f : 1f));
            else DrawThemeArt(sb, art, ChapterTheme.Blizzard, fade);
            if (locked) DrawLock(sb, art.Center, fade);

            GameText.Draw(sb, Loc.T(ep2 ? "ep2.label" : "ep1.label"), new Vector2(card.X + 20, card.Y + 220), accent * fade, 2);
            GameText.Draw(sb, ep2 ? "PROJECT AURA" : "THE GREAT FREEZE", new Vector2(card.X + 20, card.Y + 248), Color.White * fade, 4, Language.English);
            float y = card.Y + 300;
            foreach (string line in GameText.Wrap(Loc.T(ep2 ? "ep2.desc" : "ep1.desc"), card.Width - 40, 2))
            {
                GameText.Draw(sb, line, new Vector2(card.X + 20, y), Hud.Label * fade);
                y += GameText.LineHeight(2) + 4;
            }

            // Durum satırı: ilerleme, DLC ve aktarılanlar
            string status;
            if (!ep2)
                status = Loc.F("ep1.progress", Math.Min(save.UnlockedChapter, 40), 40) + (save.Episode1Complete ? "  " + Loc.T("ep.complete") : "  " + Loc.T("ep1.permadeath"));
            else if (locked)
                status = Loc.T("ep.needEp1");
            else
                status = Loc.F("ep2.progress", save.Episode2Unlocked, 40) + "  " + Loc.T(DLCManager.DLC_EPISODE2_UNLOCKED ? "ep2.dlcOwned" : "ep2.dlcFree");
            GameText.Draw(sb, Hud.FitWidth(status, card.Width - 40), new Vector2(card.X + 20, card.Bottom - 64), (locked ? Hud.Danger : Hud.Amber) * fade);
            if (ep2 && !locked && save.Carry != null)
                GameText.Draw(sb, Hud.FitWidth(Loc.F("ep2.carry", save.Carry.Survivors.Count, Loc.T($"end.carry.{save.Carry.Ending}")), card.Width - 40),
                    new Vector2(card.X + 20, card.Bottom - 36), new Color(140, 220, 255) * fade);
        }
    }

    /// <summary>Episode 2 kapak resmi: gece gökyüzünde AURA uyduları, kutup ufku ve kızıl göz.</summary>
    private void DrawEpisode2Art(SpriteBatch sb, Rectangle r, float alpha)
    {
        sb.FillRect(r, new Color(8, 6, 14) * alpha);
        for (int i = 0; i < 30; i++)
            sb.FillRect(new Rectangle(r.X + (i * 97) % r.Width, r.Y + (i * 53) % (r.Height - 60), 2, 2), Color.White * (0.6f * alpha));
        sb.FillRect(new Rectangle(r.X, r.Bottom - 50, r.Width, 50), new Color(30, 34, 50) * alpha);
        for (int x = 0; x < r.Width; x += 6)
            sb.FillRect(new Rectangle(r.X + x, r.Bottom - 50 - (int)(MathF.Abs(MathF.Sin(x * 0.03f)) * 24f), 6, 24), new Color(40, 44, 64) * alpha);
        var eye = new Vector2(r.Center.X, r.Y + 70);
        float pulse = 0.6f + 0.4f * MathF.Sin(_time * 2.5f);
        sb.DrawGlow(eye, 100f, new Color(255, 50, 60), 0.3f * alpha * pulse);
        sb.Draw(Assets.Circle, new Rectangle((int)eye.X - 30, (int)eye.Y - 30, 60, 60), new Color(60, 8, 12) * alpha);
        sb.Draw(Assets.Circle, new Rectangle((int)eye.X - 14, (int)eye.Y - 14, 28, 28), new Color(255, 50, 60) * (alpha * pulse));
        for (int s = 0; s < 3; s++)
        {
            float a = _time * 0.8f + s * 2.1f;
            var p = eye + new Vector2(MathF.Cos(a) * 120f, MathF.Sin(a) * 34f);
            sb.FillRect(new Rectangle((int)p.X - 8, (int)p.Y - 3, 16, 6), new Color(190, 196, 210) * alpha);
            sb.FillRect(new Rectangle((int)p.X - 18, (int)p.Y - 1, 36, 2), new Color(90, 120, 180) * alpha);
        }
        // Siyah Sentinel treni ufukta
        sb.FillRect(new Rectangle(r.X + 40, r.Bottom - 40, 180, 22), new Color(10, 10, 14) * alpha);
        sb.FillRect(new Rectangle(r.X + 210, r.Bottom - 36, 8, 6), new Color(255, 50, 60) * (alpha * pulse));
    }

    private static void DrawLock(SpriteBatch sb, Point center, float fade)
    {
        var gold = new Color(200, 170, 90) * fade;
        sb.FillRect(new Rectangle(center.X - 12, center.Y - 4, 24, 18), gold);
        sb.DrawBorder(new Rectangle(center.X - 8, center.Y - 16, 16, 14), 3, gold);
        sb.FillRect(new Rectangle(center.X - 2, center.Y + 2, 4, 7), new Color(60, 50, 30) * fade);
    }

    /// <summary>Tamamlanan bölümün yeşil onay işareti.</summary>
    private static void DrawCheck(SpriteBatch sb, Point c, float fade)
    {
        var green = CabinPanel.Good * fade;
        for (int i = 0; i < 5; i++) sb.FillRect(new Rectangle(c.X - 8 + i * 2, c.Y + i * 2 - 2, 3, 3), green);
        for (int i = 0; i < 8; i++) sb.FillRect(new Rectangle(c.X + 2 + i * 2, c.Y + 6 - i * 2, 3, 3), green);
    }

    /// <summary>Bölüm temasının küçük illüstrasyonu (oransal renk bloklarıyla, her boyuta uyar).</summary>
    private void DrawThemeArt(SpriteBatch sb, Rectangle r, ChapterTheme theme, float alpha)
    {
        // Koordinatlar dikdörtgenin oranları (0-1) cinsinden
        void Box(float x, float y, float w, float h, Color c) =>
            sb.FillRect(new Rectangle(r.X + (int)(x * r.Width), r.Y + (int)(y * r.Height),
                Math.Max(1, (int)(w * r.Width)), Math.Max(1, (int)(h * r.Height))), c * alpha);

        void Snow(Color c, int count, float speed)
        {
            for (int i = 0; i < count; i++)
            {
                float x = (i * 0.137f + _time * speed * (0.6f + (i % 3) * 0.2f)) % 1f;
                float y = (i * 0.291f + _time * speed * 0.5f) % 0.9f;
                Box(x, y, 0.01f, 0.05f, c);
            }
        }

        switch (theme)
        {
            case ChapterTheme.Plains:
                Box(0, 0, 1, 1, new Color(30, 40, 70));
                for (int i = 0; i < 5; i++) Box(i * 0.2f, 0.3f - (i % 3) * 0.08f, 0.2f, 0.4f, new Color(60, 72, 104));
                Box(0, 0.68f, 1, 0.32f, new Color(200, 210, 230));
                Box(0, 0.74f, 1, 0.05f, new Color(110, 116, 130));
                Snow(Color.White, 10, 0.05f);
                break;
            case ChapterTheme.Tunnels:
                Box(0, 0, 1, 1, new Color(10, 9, 12));
                Box(0, 0, 1, 0.2f, new Color(34, 30, 30));
                for (float x = 0.08f; x < 1f; x += 0.3f)
                {
                    Box(x, 0.2f, 0.025f, 0.7f, new Color(70, 50, 34));
                    Box(x - 0.03f, 0.2f, 0.2f, 0.07f, new Color(70, 50, 34));
                }
                Box(0, 0.88f, 1, 0.12f, new Color(30, 28, 30));
                Box(0.42f, 0.55f, 0.03f, 0.12f, new Color(255, 220, 150));
                break;
            case ChapterTheme.Thaw:
                Box(0, 0, 1, 1, new Color(60, 86, 110));
                Box(0, 0.25f, 1, 0.45f, new Color(170, 210, 230));
                for (int i = 0; i < 8; i++) Box(0.03f + i * 0.12f, 0.3f + (i % 3) * 0.06f, 0.01f, 0.3f, new Color(90, 130, 160));
                Box(0, 0.72f, 1, 0.28f, new Color(70, 120, 160));
                Box(0, 0.68f, 0.35f, 0.05f, new Color(110, 116, 130));
                break;
            case ChapterTheme.Blizzard:
                Box(0, 0, 1, 1, new Color(120, 132, 150));
                Box(0, 0.7f, 1, 0.3f, new Color(220, 226, 236));
                Snow(Color.White, 26, 0.35f);
                Box(0, 0, 1, 1, new Color(230, 236, 245) * 0.25f);
                break;
            case ChapterTheme.Wolves:
                Box(0, 0, 1, 1, new Color(22, 26, 44));
                Box(0.8f, 0.12f, 0.05f, 0.25f, new Color(230, 230, 210)); // Ay
                for (int i = 0; i < 6; i++) Box(i * 0.17f, 0.35f, 0.05f, 0.4f, new Color(16, 26, 22)); // Çamlar
                Box(0, 0.74f, 1, 0.26f, new Color(170, 180, 200));
                for (int i = 0; i < 3; i++) // Kurt siluetleri, parlayan gözler
                {
                    float x = 0.2f + i * 0.22f;
                    Box(x, 0.58f, 0.08f, 0.16f, new Color(40, 40, 46));
                    Box(x + 0.06f, 0.5f, 0.03f, 0.12f, new Color(40, 40, 46));
                    Box(x + 0.075f, 0.54f, 0.008f, 0.05f, new Color(255, 210, 80));
                }
                break;
            case ChapterTheme.IceRails:
                Box(0, 0, 1, 1, new Color(50, 70, 96));
                Box(0, 0.6f, 1, 0.4f, new Color(180, 214, 236));
                Box(0, 0.72f, 0.5f, 0.05f, new Color(110, 116, 130));
                Box(0.5f, 0.72f, 0.5f, 0.05f, new Color(110, 116, 130));
                for (int i = 0; i < 5; i++) Box(0.5f + i * 0.08f, 0.66f - i * 0.06f, 0.07f, 0.05f, new Color(110, 116, 130)); // Makas kolu
                Box(0.3f, 0.6f, 0.14f, 0.22f, new Color(200, 240, 255) * 0.6f); // Buzlanma
                break;
            case ChapterTheme.AcidWaste: // Buzul Kıyameti: zehirli yeşil gökyüzü, paslı enkaz, asit karı
                Box(0, 0, 1, 1, new Color(40, 56, 34));
                Box(0, 0.2f, 1, 0.5f, new Color(90, 120, 50) * 0.5f);
                Box(0.15f, 0.35f, 0.12f, 0.35f, new Color(60, 46, 36));   // Paslı kule
                Box(0.62f, 0.45f, 0.25f, 0.25f, new Color(70, 52, 40));
                Box(0, 0.7f, 1, 0.3f, new Color(170, 190, 120));
                Snow(new Color(190, 255, 120), 18, 0.25f);
                break;
            case ChapterTheme.Mining:
                Box(0, 0, 1, 1, new Color(40, 44, 58));
                Box(0, 0.68f, 1, 0.32f, new Color(190, 196, 210));
                Box(0.3f, 0.2f, 0.4f, 0.5f, new Color(60, 50, 44));    // Dağ yamacı
                Box(0.42f, 0.4f, 0.16f, 0.3f, new Color(8, 8, 10));    // Maden ağzı
                Box(0.4f, 0.36f, 0.2f, 0.07f, new Color(110, 80, 50));
                Box(0.48f, 0.5f, 0.02f, 0.07f, new Color(255, 200, 80));
                break;
            default: // Gece
                Box(0, 0, 1, 1, new Color(6, 8, 18));
                for (int i = 0; i < 14; i++) Box(i * 0.173f % 1f, i * 0.31f % 0.5f, 0.006f, 0.04f, new Color(220, 230, 255));
                Box(0, 0.75f, 1, 0.25f, new Color(40, 46, 66));
                Box(0.1f, 0.6f, 0.18f, 0.16f, new Color(30, 30, 36));
                Box(0.28f, 0.64f, 0.03f, 0.08f, new Color(255, 220, 140)); // Fener
                break;
        }
        sb.DrawBorder(r, 1, Hud.PanelEdge * alpha);
    }

    private void DrawShop(SpriteBatch sb, float fade)
    {
        DrawHeader(sb, Loc.T("shop.title"), fade);
        DrawShopTabs(sb, fade);
        if (_shopTab == 1)
        {
            DrawDlcList(sb, fade, top: 124);
            return;
        }
        if (_shopTab == 2)
        {
            DrawCosmeticStore(sb, fade);
            return;
        }

        for (int i = 0; i < UpgradeCatalog.All.Length; i++)
        {
            var upgrade = UpgradeCatalog.All[i];
            bool owned = SaveData.HasUpgrade(upgrade);
            int price = UpgradeCatalog.Price(upgrade);
            bool affordable = SaveData.Current.Credits >= price;
            // 9 ürün: kompakt satırlar; vagonlar "VAGON" etiketiyle ayrılır
            string name = UpgradeCatalog.IsWagon(upgrade)
                ? $"{UpgradeCatalog.Name(upgrade)}  [{Loc.T("shop.wagonTag")}]"
                : UpgradeCatalog.Name(upgrade);
            DrawListRow(sb, i, 112 + i * 56, 52, name, UpgradeCatalog.Description(upgrade),
                owned ? Loc.T("trade.owned") : $"{price}",
                owned ? CabinPanel.Good : affordable ? new Color(255, 215, 110) : Hud.Danger,
                (s, p) => UiDraw.UpgradeIcon(s, upgrade, p), fade, showCoin: !owned);
        }
    }

    private void DrawSkins(SpriteBatch sb, float fade)
    {
        DrawHeader(sb, Loc.T("skins.title"), fade);

        // Sol sütun: tren görünümleri, sağ sütun: kabin temaları (imleç tek liste gibi ilerler, A/D sütun değiştirir)
        GameText.Draw(sb, Loc.T("skins.trains"), new Vector2(60, 86), Hud.Amber * fade);
        GameText.Draw(sb, Loc.T("skins.themes"), new Vector2(680, 86), Hud.Amber * fade);

        const int rowStep = 46, rowHeight = 42, top = 116;

        // 17 skin: imleç görünür alanda kalacak şekilde kaydır
        if (_cursor < Cosmetics.ShopSkins.Length)
        {
            if (_cursor < _skinScroll) _skinScroll = _cursor;
            if (_cursor >= _skinScroll + VisibleSkinRows) _skinScroll = _cursor - VisibleSkinRows + 1;
        }
        _skinScroll = Math.Clamp(_skinScroll, 0, Math.Max(0, Cosmetics.ShopSkins.Length - VisibleSkinRows));
        if (_skinScroll > 0) DrawScrollArrow(sb, new Point(350, top - 6), up: true, fade);
        if (_skinScroll + VisibleSkinRows < Cosmetics.ShopSkins.Length)
            DrawScrollArrow(sb, new Point(350, top + VisibleSkinRows * rowStep), up: false, fade);

        int lastSkin = Math.Min(Cosmetics.ShopSkins.Length, _skinScroll + VisibleSkinRows);
        for (int i = _skinScroll; i < lastSkin; i++)
        {
            var skin = Cosmetics.ShopSkins[i];
            bool owned = SaveData.Current.OwnedSkins.Contains(skin.Id) && DLCManager.Owns(skin.Dlc);
            bool dlcLocked = !DLCManager.Owns(skin.Dlc);
            bool locked = dlcLocked || !owned && !Achievements.IsUnlocked(skin.Achievement);
            string name = skin.Dlc != null ? $"{skin.Name}  [DLC]" : skin.Name;
            DrawCosmeticRow(sb, new Rectangle(50, top + (i - _skinScroll) * rowStep, 600, rowHeight), i == _cursor, name, skin.Price,
                owned, SaveData.Current.EquippedSkin == skin.Id, locked,
                (s, r) =>
                {
                    // Küçük renk örneği: kazan, kabin, şerit
                    s.FillRect(new Rectangle(r.X, r.Y + 4, 14, 18), skin.CabBody * fade);
                    s.FillRect(new Rectangle(r.X + 14, r.Y + 9, 24, 13), skin.Boiler * fade);
                    s.FillRect(new Rectangle(r.X + 14, r.Y + 15, 24, 3), skin.Trim * fade);
                    s.FillRect(new Rectangle(r.X + 28, r.Y + 1, 5, 8), skin.Iron * fade);
                }, fade);
        }

        for (int i = 0; i < Cosmetics.CabinThemes.Length; i++)
        {
            var theme = Cosmetics.CabinThemes[i];
            int index = Cosmetics.ShopSkins.Length + i;
            DrawCosmeticRow(sb, new Rectangle(670, top + i * rowStep, 560, rowHeight), index == _cursor, theme.Name, theme.Price,
                SaveData.Current.OwnedThemes.Contains(theme.Id), SaveData.Current.EquippedTheme == theme.Id, false,
                (s, r) =>
                {
                    s.FillRect(new Rectangle(r.X, r.Y + 2, 40, 22), new Color(10, 12, 20) * fade);
                    s.DrawBorder(new Rectangle(r.X, r.Y + 2, 40, 22), 2, theme.Edge * fade);
                    s.FillRect(new Rectangle(r.X + 5, r.Y + 8, 20, 3), theme.Accent * fade);
                    s.FillRect(new Rectangle(r.X + 5, r.Y + 15, 28, 3), theme.Label * fade);
                }, fade);
        }

        // Sağ alt: seçili skinin başarım şartı ya da önizleme ipucu
        string info = Loc.T("skins.preview");
        Color infoColor = Hud.Label;
        if (_cursor < Cosmetics.ShopSkins.Length && Cosmetics.ShopSkins[_cursor].Dlc is DlcId skinDlc)
        {
            bool has = DLCManager.Owns(skinDlc);
            info = Loc.F(has ? "skins.dlcOwned" : "skins.dlcNeed", DLCManager.Info(skinDlc).Name);
            infoColor = has ? CabinPanel.Good : new Color(255, 215, 110);
        }
        else if (_cursor < Cosmetics.ShopSkins.Length)
        {
            var achievement = Achievements.Get(Cosmetics.ShopSkins[_cursor].Achievement);
            if (achievement != null)
            {
                bool met = achievement.IsMet(SaveData.Current);
                info = Loc.F(met ? "skins.achMet" : "skins.achNeed", achievement.Name, achievement.Description);
                infoColor = met ? CabinPanel.Good : new Color(255, 215, 110);
            }
        }
        float y = top + Cosmetics.CabinThemes.Length * rowStep + 20;
        foreach (string line in GameText.Wrap(info, 540, 2))
        {
            GameText.Draw(sb, line, new Vector2(680, y), infoColor * fade);
            y += GameText.LineHeight(2) + 4;
        }
    }

    private void DrawCosmeticRow(SpriteBatch sb, Rectangle rect, bool selected, string name, int price, bool owned, bool equipped,
        bool locked, Action<SpriteBatch, Rectangle> swatch, float fade)
    {
        UiDraw.FancyPanel(sb, rect, (selected ? Hud.Amber : Hud.PanelEdge) * fade);
        if (selected) sb.FillRect(rect, Hud.Amber * (0.12f * fade));

        swatch(sb, new Rectangle(rect.X + 12, rect.Y + 8, 44, 28));
        GameText.Draw(sb, name, new Vector2(rect.X + 70, rect.Y + 10),
            (locked ? Hud.Label * 0.6f : selected ? Color.White : Hud.Value) * fade);

        string status;
        Color color;
        if (equipped) { status = Loc.T("skins.equipped"); color = CabinPanel.Good; }
        else if (owned) { status = Loc.T("skins.equip"); color = Hud.Amber; }
        else if (locked) { status = Loc.T("chapters.locked"); color = Hud.Danger; }
        else { status = price == 0 ? Loc.T("skins.free") : $"{price}"; color = SaveData.Current.Credits >= price ? new Color(255, 215, 110) : Hud.Danger; }

        float width = GameText.MeasureLine(status);
        GameText.DrawRight(sb, status, rect.Right - 16, rect.Y + 10, color * fade);
        if (locked)
            DrawLock(sb, new Point((int)(rect.Right - 16 - width - 22), rect.Y + 22), fade);
        else if (!owned && price > 0)
            UiDraw.CreditIcon(sb, new Point((int)(rect.Right - 16 - width - 16), rect.Y + 21), fade);
    }

    private void DrawListRow(SpriteBatch sb, int index, int y, int height, string name, string detail, string right, Color rightColor,
        Action<SpriteBatch, Point> icon, float fade, bool showCoin)
    {
        bool selected = index == _cursor;
        var rect = new Rectangle(190, y, 900, height);
        UiDraw.FancyPanel(sb, rect, (selected ? Hud.Amber : Hud.PanelEdge) * fade);
        if (selected) sb.FillRect(rect, Hud.Amber * (0.12f * fade));

        icon(sb, new Point(rect.X + 40, rect.Center.Y));

        // Kısa satırlarda (9 ürünlü mağaza) ad ve fiyat bir boy küçük yazılır
        bool compact = height < 60;
        int nameScale = compact ? 2 : 3;
        GameText.Draw(sb, name, new Vector2(rect.X + 80, rect.Y + (compact ? 5 : 8)), (selected ? Color.White : Hud.Value) * fade, nameScale);
        GameText.Draw(sb, Hud.FitWidth(detail, rect.Width - 260), new Vector2(rect.X + 80, rect.Y + (compact ? 27 : 44)), Hud.Label * fade);

        float width = GameText.MeasureLine(right, nameScale);
        float rightY = rect.Center.Y - GameText.LineHeight(nameScale) / 2f;
        GameText.DrawRight(sb, right, rect.Right - 20, rightY, rightColor * fade, nameScale);
        if (showCoin)
            UiDraw.CreditIcon(sb, new Point((int)(rect.Right - 20 - width - 16), rect.Center.Y), fade);
    }
}
