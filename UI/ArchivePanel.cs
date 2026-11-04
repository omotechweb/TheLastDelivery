using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TheLastDelivery.Audio;
using TheLastDelivery.Core;
using TheLastDelivery.Graphics;
using TheLastDelivery.Localization;
using TheLastDelivery.Story;

namespace TheLastDelivery.UI;

/// <summary>
/// Arşiv ([L]): oyuncunun kendi keşfettiği belgeler, ses kasetleri ve Arthur'un hatıraları; tanışılan kişilerin
/// güven düzeyleri ve yan görevler. Belgeler kalıcı arşivden (bütün koşular) okunur; bu koşuda bulunmayanlar
/// soluk görünür. Açıkken oyun zamanı durur.
///   A/D ya da SOL/SAĞ: sekme   W/S: seç   PGUP/PGDN ya da Q/E: metni kaydır   L / ESC: kapat
/// </summary>
public sealed class ArchivePanel
{
    private enum Tab { Documents, Tapes, Memories, People, Quests }
    private static readonly Rectangle Panel = new(110, 50, 1060, 620);
    private const int ListWidth = 380;
    private const int Rows = 13;

    private Tab _tab;
    private int _cursor;
    private int _scroll;       // Liste kaydırması
    private int _textScroll;   // Okuma bölmesi kaydırması (satır)
    private float _open;
    private float _time;
    private int _episode = 1;
    private readonly List<string> _items = new();

    public bool IsOpen { get; private set; }

    public void Open(int episode)
    {
        IsOpen = true;
        _episode = episode;
        _open = 0f;
        _cursor = 0;
        _scroll = 0;
        _textScroll = 0;
        Rebuild();
    }

    public void Close() => IsOpen = false;

    private void Rebuild()
    {
        _items.Clear();
        switch (_tab)
        {
            case Tab.People:
                foreach (var npc in DialogueTreeManager.Known()) _items.Add(npc.Id);
                break;
            case Tab.Quests:
                foreach (string id in SideQuests.All)
                    if (SideQuests.State(id) != SideQuests.None) _items.Add(id);
                break;
            default:
                var kind = _tab switch { Tab.Tapes => LoreKind.Tape, Tab.Memories => LoreKind.Memory, _ => LoreKind.Document };
                foreach (var entry in LoreArchive.All)
                    if (entry.Kind == kind && LoreArchive.InArchive(entry.Id)) _items.Add(entry.Id);
                break;
        }
        _cursor = Math.Clamp(_cursor, 0, Math.Max(0, _items.Count - 1));
        MarkCurrentRead();
    }

    private void MarkCurrentRead()
    {
        if (_tab is Tab.Documents or Tab.Tapes or Tab.Memories && _cursor < _items.Count)
            LoreArchive.MarkRead(_items[_cursor]);
    }

    public void Update(float dt, InputState input, ProceduralAudio audio)
    {
        _time += dt;
        _open = MathF.Min(1f, _open + dt * 5f);

        if (input.AnyPressed(Keys.L, Keys.Back))
        {
            Close();
            Economy.SaveData.Save();
            audio.PlayClick();
            return;
        }

        int tabs = Enum.GetValues<Tab>().Length;
        if (input.AnyPressed(Keys.A, Keys.Left)) { _tab = (Tab)(((int)_tab - 1 + tabs) % tabs); _cursor = 0; _scroll = 0; _textScroll = 0; Rebuild(); audio.PlayClick(); }
        if (input.AnyPressed(Keys.D, Keys.Right)) { _tab = (Tab)(((int)_tab + 1) % tabs); _cursor = 0; _scroll = 0; _textScroll = 0; Rebuild(); audio.PlayClick(); }

        if (_items.Count == 0) return;
        int previous = _cursor;
        if (input.AnyPressed(Keys.W, Keys.Up)) _cursor = (_cursor - 1 + _items.Count) % _items.Count;
        if (input.AnyPressed(Keys.S, Keys.Down)) _cursor = (_cursor + 1) % _items.Count;
        if (_cursor != previous)
        {
            _textScroll = 0;
            MarkCurrentRead();
            audio.PlayClick();
        }
        if (_cursor < _scroll) _scroll = _cursor;
        if (_cursor >= _scroll + Rows) _scroll = _cursor - Rows + 1;

        if (input.AnyPressed(Keys.PageDown, Keys.E)) _textScroll += 4;
        if (input.AnyPressed(Keys.PageUp, Keys.Q)) _textScroll = Math.Max(0, _textScroll - 4);
    }

    public void Draw(SpriteBatch sb)
    {
        float a = _open;
        sb.FillRect(new Rectangle(0, 0, GameConfig.ScreenWidth, GameConfig.ScreenHeight), Color.Black * (0.6f * a));
        var panel = Panel;
        panel.Y += (int)((1f - a) * 20f);
        sb.FillRect(panel, new Color(14, 13, 12) * (0.96f * a));
        UiDraw.FancyPanel(sb, panel, Hud.PanelEdge * a);
        GameText.Draw(sb, Loc.T("arc.title"), new Vector2(panel.X + 24, panel.Y + 16), Hud.Amber * a, 3);
        string counts = Loc.F("arc.counts", LoreArchive.Count(LoreKind.Document, _episode), LoreArchive.Count(LoreKind.Tape, _episode),
            LoreArchive.Count(LoreKind.Memory, _episode));
        GameText.DrawRight(sb, counts, panel.Right - 24, panel.Y + 24, Hud.Label * a);

        // Sekmeler
        int tabX = panel.X + 24;
        foreach (Tab tab in Enum.GetValues<Tab>())
        {
            string label = Loc.T($"arc.tab.{tab}");
            int w = (int)GameText.MeasureLine(label) + 24;
            var rect = new Rectangle(tabX, panel.Y + 62, w, 30);
            bool active = tab == _tab;
            sb.FillRect(rect, (active ? Hud.Amber * 0.3f : Color.Black * 0.4f) * a);
            sb.DrawBorder(rect, 2, (active ? Hud.Amber : Hud.PanelEdge) * a);
            GameText.Draw(sb, label, new Vector2(rect.X + 12, rect.Y + 5), (active ? Color.White : Hud.Label) * a);
            tabX += w + 8;
        }

        var list = new Rectangle(panel.X + 24, panel.Y + 106, ListWidth, panel.Height - 160);
        var page = new Rectangle(list.Right + 16, list.Y, panel.Right - list.Right - 40, list.Height);
        sb.FillRect(list, Color.Black * (0.35f * a));
        sb.FillRect(page, new Color(30, 26, 20) * (0.6f * a));
        sb.DrawBorder(page, 2, new Color(90, 74, 50) * a);

        if (_items.Count == 0)
        {
            foreach (string line in GameText.Wrap(Loc.T($"arc.empty.{_tab}"), list.Width - 20))
            {
                GameText.Draw(sb, line, new Vector2(list.X + 10, list.Y + 12), Hud.Label * a);
                list.Y += 28;
            }
        }
        else
        {
            for (int i = _scroll; i < Math.Min(_items.Count, _scroll + Rows); i++)
            {
                var row = new Rectangle(list.X, list.Y + (i - _scroll) * 34, list.Width, 32);
                bool selected = i == _cursor;
                if (selected) sb.FillRect(row, Hud.Amber * (0.22f * a));
                var (title, color, tag) = Row(_items[i]);
                GameText.Draw(sb, FitWidth(title, row.Width - 70), new Vector2(row.X + 10, row.Y + 6), (selected ? Color.White : color) * a);
                if (tag != null) GameText.DrawRight(sb, tag, row.Right - 8, row.Y + 6, new Color(140, 220, 255) * a);
            }
            DrawPage(sb, page, a);
        }

        GameText.Draw(sb, Loc.T("arc.hint"), new Vector2(panel.X + 24, panel.Bottom - 40), Hud.Label * (0.85f * a));
    }

    private (string Title, Color Color, string Tag) Row(string id)
    {
        switch (_tab)
        {
            case Tab.People:
            {
                var npc = DialogueTreeManager.Find(id);
                int trust = DialogueTreeManager.Trust(id);
                return (npc.Name, TrustColor(trust), DialogueTreeManager.LevelName(trust));
            }
            case Tab.Quests:
            {
                int state = SideQuests.State(id);
                var color = state == SideQuests.Done ? new Color(120, 220, 130) : state == SideQuests.Failed ? Hud.Danger : Hud.Value;
                return (SideQuests.Title(id), color, Loc.T($"arc.q.{state}"));
            }
            default:
            {
                var entry = LoreArchive.Find(id);
                bool here = LoreArchive.Has(id);
                string tag = LoreArchive.IsUnread(id) ? Loc.T("arc.new") : entry.Episode == 2 ? "E2" : null;
                return (entry.Title, here ? Hud.Value : Hud.Label * 0.7f, tag);
            }
        }
    }

    private void DrawPage(SpriteBatch sb, Rectangle page, float a)
    {
        string id = _items[_cursor];
        string title, body;
        switch (_tab)
        {
            case Tab.People:
            {
                var npc = DialogueTreeManager.Find(id);
                int trust = DialogueTreeManager.Trust(id);
                title = npc.Name;
                body = Loc.F("arc.person", DialogueTreeManager.LevelName(trust), Loc.T($"arc.temper.{npc.Temper}"))
                       + (Loc.Has($"arc.bio.{id}") ? "\n\n" + Loc.T($"arc.bio.{id}") : "");
                var bar = new Rectangle(page.X + 16, page.Bottom - 30, page.Width - 32, 8);
                sb.FillRect(bar, Color.Black * (0.7f * a));
                sb.FillRect(new Rectangle(bar.X, bar.Y, bar.Width * trust / 100, bar.Height), TrustColor(trust) * a);
                break;
            }
            case Tab.Quests:
                title = SideQuests.Title(id);
                body = SideQuests.Description(id);
                break;
            default:
            {
                var entry = LoreArchive.Find(id);
                title = entry.Title;
                body = entry.Body;
                if (entry.Kind == LoreKind.Tape) DrawReels(sb, new Point(page.Right - 70, page.Y + 26), a);
                break;
            }
        }

        GameText.Draw(sb, FitWidth(title, page.Width - 120), new Vector2(page.X + 16, page.Y + 14), Hud.Amber * a);
        sb.FillRect(new Rectangle(page.X + 16, page.Y + 44, page.Width - 32, 2), Hud.Amber * (0.4f * a));
        var lines = GameText.Wrap(body, page.Width - 36);
        int maxLines = (page.Height - 100) / 26;
        _textScroll = Math.Clamp(_textScroll, 0, Math.Max(0, lines.Count - maxLines));
        float y = page.Y + 58;
        for (int i = _textScroll; i < lines.Count && i < _textScroll + maxLines; i++)
        {
            GameText.Draw(sb, lines[i], new Vector2(page.X + 18, y), new Color(230, 222, 200) * a);
            y += 26;
        }
        if (lines.Count > maxLines)
            GameText.DrawRight(sb, $"{_textScroll + 1}-{Math.Min(lines.Count, _textScroll + maxLines)}/{lines.Count}",
                page.Right - 12, page.Bottom - 30, Hud.Label * a);
    }

    private void DrawReels(SpriteBatch sb, Point c, float a)
    {
        for (int i = 0; i < 2; i++)
        {
            var center = new Vector2(c.X - 18 + i * 36, c.Y);
            sb.Draw(Assets.Circle, new Rectangle((int)center.X - 14, (int)center.Y - 14, 28, 28), new Color(60, 54, 46) * a);
            float angle = _time * 3f + i;
            sb.FillRect(new Rectangle((int)(center.X + MathF.Cos(angle) * 8f) - 2, (int)(center.Y + MathF.Sin(angle) * 8f) - 2, 4, 4),
                new Color(220, 210, 190) * a);
        }
    }

    private static Color TrustColor(int trust) => DialogueTreeManager.Level(trust) switch
    {
        0 => new Color(230, 70, 60),
        1 => new Color(240, 150, 70),
        2 => new Color(230, 210, 120),
        3 => new Color(140, 220, 130),
        _ => new Color(110, 230, 200)
    };

    private static string FitWidth(string text, float width)
    {
        if (GameText.MeasureLine(text) <= width) return text;
        while (text.Length > 3 && GameText.MeasureLine(text + "...") > width) text = text[..^1];
        return text + "...";
    }
}
