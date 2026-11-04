using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheLastDelivery.Graphics;
using TheLastDelivery.Localization;

namespace TheLastDelivery.UI;

/// <summary>Mini haritadaki işaret türleri.</summary>
public enum MapMark { Station, Town, Depot, Hub, Shelter, Sos, Tower, Story, Phantom }

public readonly record struct MiniMapMark(float Km, MapMark Kind);

/// <summary>
/// Mini harita navigasyonu ([N] gizle/göster): bölümün baştan sona hattı, trenin konumu, ilerideki duraklar,
/// imdat noktası, AURA kuleleri, hikâye anları (henüz yaşanmamışsa "!") ve hava durumu.
/// AURA'nın zihin oyunları sürerken sahte duraklar belirir, hat titrer ve mesafeler bozulur.
/// </summary>
public static class RouteMiniMap
{
    public static readonly Rectangle Panel = new(16, 278, 404, 78);

    public static void Draw(SpriteBatch sb, float startKm, float endKm, float km, IReadOnlyList<MiniMapMark> marks,
        float nextStopKm, string nextStopName, string weather, float time, float corruption)
    {
        UiDraw.FancyPanel(sb, Panel, Hud.PanelEdge);
        GameText.Draw(sb, Loc.T("map.title"), new Vector2(Panel.X + 14, Panel.Y + 8), Hud.Amber);
        GameText.DrawRight(sb, weather, Panel.Right - 14, Panel.Y + 8, Hud.Label);

        var line = new Rectangle(Panel.X + 22, Panel.Y + 42, Panel.Width - 44, 3);
        float span = MathF.Max(0.01f, endKm - startKm);
        float glitch = corruption > 0.5f ? MathF.Sin(time * 37f) * 3f * corruption : 0f;
        sb.FillRect(new Rectangle(line.X, line.Y + (int)glitch, line.Width, line.Height), Hud.Label * 0.6f);
        // Kilometre çentikleri
        for (int i = 0; i <= 10; i++)
            sb.FillRect(new Rectangle(line.X + line.Width * i / 10, line.Y - (i % 5 == 0 ? 4 : 2), 1, i % 5 == 0 ? 11 : 7), Hud.Label * 0.5f);

        int X(float k) => line.X + (int)(line.Width * MathHelper.Clamp((k - startKm) / span, 0f, 1f));

        foreach (var mark in marks)
        {
            bool beyond = mark.Km > endKm;
            int x = X(mark.Km);
            var color = mark.Kind switch
            {
                MapMark.Town => new Color(255, 190, 110),
                MapMark.Depot => new Color(150, 200, 255),
                MapMark.Hub => new Color(255, 230, 140),
                MapMark.Shelter => new Color(140, 255, 170),
                MapMark.Sos => new Color(255, 210, 90),
                MapMark.Tower => new Color(255, 60, 60),
                MapMark.Story => new Color(140, 220, 255),
                MapMark.Phantom => new Color(255, 80, 90) * (0.5f + 0.5f * MathF.Sin(time * 9f)),
                _ => Hud.Value
            };
            if (mark.Kind == MapMark.Story)
            {
                GameText.DrawCentered(sb, "!", x, line.Y - 22, color);
                continue;
            }
            if (mark.Kind == MapMark.Tower)
            {
                sb.FillRect(new Rectangle(x - 1, line.Y - 12, 3, 12), color);
                sb.FillRect(new Rectangle(x - 3, line.Y - 15, 7, 4), color * (0.6f + 0.4f * MathF.Sin(time * 5f)));
                continue;
            }
            int size = mark.Kind is MapMark.Town or MapMark.Hub or MapMark.Shelter or MapMark.Phantom ? 10 : 7;
            var rect = new Rectangle(x - size / 2, line.Y + 1 - size / 2, size, size);
            sb.FillRect(rect, color * (beyond ? 0.5f : 1f));
            if (beyond) GameText.Draw(sb, ">", new Vector2(line.Right + 4, line.Y - 9), color);
        }

        // Tren: küçük lokomotif ve duman
        int tx = X(km);
        sb.FillRect(new Rectangle(tx - 7, line.Y - 8, 14, 8), new Color(230, 230, 240));
        sb.FillRect(new Rectangle(tx + 3, line.Y - 13, 3, 5), new Color(230, 230, 240));
        sb.DrawGlow(new Vector2(tx, line.Y - 4), 18f, Hud.Amber, 0.4f);

        // Sıradaki durak ve mesafe; zihin oyunlarında rakamlar bozulur
        int distance = (int)MathF.Max(0f, nextStopKm - km);
        string distText = corruption > 0.45f && MathF.Sin(time * 3.3f) > 0.4f ? "??" : distance.ToString();
        string next = Loc.F("map.next", nextStopName, distText);
        GameText.Draw(sb, Hud.FitWidth(next, Panel.Width - 28), new Vector2(Panel.X + 14, Panel.Y + 52), Hud.Value);
        GameText.Draw(sb, Loc.T("map.keys"), new Vector2(Panel.X + 4, Panel.Bottom + 6), Hud.Label * 0.7f);
    }
}
