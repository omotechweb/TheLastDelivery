using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TheLastDelivery.Audio;
using TheLastDelivery.Core;
using TheLastDelivery.Graphics;
using TheLastDelivery.Localization;

namespace TheLastDelivery.UI;

public enum SettingsAction { None, ToggleFullscreen }

/// <summary>
/// Ayarlar ekranı: ana menüden ve duraklatma menüsünden açılır. W/S ile satır seçilir, A/D ile değer değişir.
/// Değişiklikler anında uygulanır ve ekran kapanırken kaydedilir. Altta oyun kolu tuş dizilimi durur.
/// </summary>
public sealed class SettingsScreen
{
    private const float CenterX = GameConfig.ScreenWidth / 2f;
    private const int RowCount = 8;
    private const int Volume = 0, Shake = 1, Bloom = 2, Flash = 3, Hints = 4, Fullscreen = 5, Vibration = 6, Back = 7;

    private static readonly string[] Labels =
    {
        "set.volume", "set.shake", "set.bloom", "set.flash", "set.hints", "set.fullscreen", "set.vibration", "set.back"
    };

    private int _cursor;
    private float _time;

    public bool IsOpen { get; private set; }

    public void Open()
    {
        IsOpen = true;
        _cursor = 0;
        _time = 0f;
    }

    public void Close()
    {
        if (!IsOpen) return;
        IsOpen = false;
        Settings.Save();
    }

    public SettingsAction Update(float dt, InputState input, ProceduralAudio audio)
    {
        _time += dt;
        if (input.AnyPressed(Keys.W, Keys.Up)) { _cursor = (_cursor - 1 + RowCount) % RowCount; audio.PlayClick(); }
        if (input.AnyPressed(Keys.S, Keys.Down)) { _cursor = (_cursor + 1) % RowCount; audio.PlayClick(); }

        if (input.Pressed(Keys.Back))
        {
            audio.PlayClick();
            Close();
            return SettingsAction.None;
        }

        bool confirm = input.AnyPressed(Keys.Enter, Keys.Space) && _time > 0.15f;
        int step = input.AnyPressed(Keys.A, Keys.Left) ? -1 : input.AnyPressed(Keys.D, Keys.Right) ? 1 : 0;
        if (!confirm && step == 0) return SettingsAction.None;

        switch (_cursor)
        {
            case Volume:
                // A/D bir adım değiştirir; ENTER bir adım yükseltir, tepedeyse sıfıra döner
                int volume = step != 0 ? Settings.Volume + step * 10 : Settings.Volume >= 100 ? 0 : Settings.Volume + 10;
                Settings.Volume = Math.Clamp(volume, 0, 100);
                audio.UserVolume = Settings.VolumeScale;
                audio.PlayConfirm(); // Yeni düzey hemen duyulsun
                return SettingsAction.None;
            case Shake:
                int level = Settings.Shake / 50 + (step == 0 ? 1 : step);
                Settings.Shake = (level + 3) % 3 * 50;
                break;
            case Bloom: Settings.Bloom = !Settings.Bloom; break;
            case Flash: Settings.Flash = !Settings.Flash; break;
            case Hints: Settings.Hints = !Settings.Hints; break;
            case Fullscreen:
                audio.PlayClick();
                return SettingsAction.ToggleFullscreen;
            case Vibration:
                Settings.Vibration = !Settings.Vibration;
                GamepadInput.Rumble(0.6f, 0.2f);
                break;
            case Back:
                if (!confirm) return SettingsAction.None;
                audio.PlayClick();
                Close();
                return SettingsAction.None;
        }
        audio.PlayClick();
        return SettingsAction.None;
    }

    public void Draw(SpriteBatch sb, bool fullscreen)
    {
        sb.FillRect(new Rectangle(0, 0, GameConfig.ScreenWidth, GameConfig.ScreenHeight), new Color(4, 6, 14) * 0.93f);

        var box = new Rectangle(180, 36, 920, 648);
        sb.FillRect(box, new Color(6, 8, 16) * 0.96f);
        UiDraw.FancyPanel(sb, box, Hud.PanelEdge);

        GameText.DrawCentered(sb, Loc.T("set.title"), CenterX, box.Y + 22, Hud.Amber, 4);
        sb.FillRect(new Rectangle(box.X + 60, box.Y + 76, box.Width - 120, 2), Hud.Amber * 0.4f);

        const int rowHeight = 42;
        int top = box.Y + 94;
        int labelX = box.X + 90, valueX = box.Right - 90;
        for (int i = 0; i < RowCount; i++)
        {
            bool selected = i == _cursor;
            int y = top + i * rowHeight;
            if (selected)
            {
                var rect = new Rectangle(box.X + 60, y - 5, box.Width - 120, rowHeight - 6);
                sb.FillRect(rect, Hud.Amber * 0.10f);
                sb.DrawBorder(rect, 2, Hud.Amber * (0.7f + 0.3f * MathF.Sin(_time * 5f)));
            }

            var color = selected ? Color.White : Hud.Label;
            if (i == Back)
            {
                GameText.DrawCentered(sb, Loc.T(Labels[i]), CenterX, y, color, 2);
                continue;
            }
            GameText.Draw(sb, Loc.T(Labels[i]), new Vector2(labelX, y), color, 2);

            if (i == Volume)
            {
                var bar = new Rectangle(valueX - 300, y + 4, 220, 16);
                UiDraw.FancyBar(sb, bar, Settings.VolumeScale, Settings.VolumeScale, new Color(150, 180, 225));
                GameText.DrawRight(sb, Loc.F("fmt.percent", Settings.Volume), valueX, y, selected ? Hud.Amber : Hud.Value, 2);
                continue;
            }

            string value = i switch
            {
                Shake => Loc.T(Settings.Shake >= 100 ? "set.full" : Settings.Shake >= 50 ? "set.half" : "set.off"),
                Bloom => OnOff(Settings.Bloom),
                Flash => OnOff(Settings.Flash),
                Hints => OnOff(Settings.Hints),
                Fullscreen => OnOff(fullscreen),
                _ => OnOff(Settings.Vibration)
            };
            string shown = selected ? "< " + value + " >" : value;
            GameText.DrawRight(sb, shown, valueX, y, selected ? Hud.Amber : Hud.Value, 2);
        }

        // Oyun kolu: bağlantı durumu ve tuş dizilimi
        int padY = top + RowCount * rowHeight + 6;
        sb.FillRect(new Rectangle(box.X + 60, padY, box.Width - 120, 2), Hud.PanelEdge * 0.6f);
        bool connected = GamepadInput.Connected;
        GameText.DrawCentered(sb, Loc.T(connected ? "set.pad.on" : "set.pad.off"), CenterX, padY + 14,
            connected ? CabinPanel.Good : Hud.Label * 0.8f, 2);
        for (int i = 0; i < 4; i++)
            GameText.DrawCentered(sb, Hud.FitWidth(Loc.T("set.pad." + (i + 1)), box.Width - 60), CenterX, padY + 46 + i * 26,
                Hud.Label * (connected ? 0.95f : 0.6f), 2);

        GameText.DrawCentered(sb, Loc.T("set.hint"), CenterX, box.Bottom - 34, Hud.Label * 0.7f, 2);
    }

    private static string OnOff(bool on) => Loc.T(on ? "set.on" : "set.off");
}
