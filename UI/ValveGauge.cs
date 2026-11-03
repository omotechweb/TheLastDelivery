using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheLastDelivery.Core;
using TheLastDelivery.Events;
using TheLastDelivery.Graphics;
using TheLastDelivery.Localization;

namespace TheLastDelivery.UI;

/// <summary>
/// Vana arızası mini oyunu paneli: yarım daire basınç göstergesi, yeşil/sarı/kırmızı bölgeler,
/// kayan ibre ve tuş ipucu. İbre yeşildeyken [E] / [BOŞLUK] vanayı kapatır.
/// </summary>
public static class ValveGauge
{
    private static readonly Rectangle Panel = new(444, 188, 388, 236); // Sol HUD ile Kabin Panosu arasında
    private const int Radius = 92;
    private static readonly Color Cold = new(80, 110, 160);
    private static readonly Color Green = new(90, 210, 110);
    private static readonly Color Warn = new(230, 190, 70);
    private static readonly Color Red = new(240, 70, 50);

    public static void Draw(SpriteBatch sb, RoadEvent e, float time)
    {
        // Açılış: hafif büyüyerek ve belirerek gelir
        float appear = MathHelper.Clamp(e.Age * 5f, 0f, 1f);
        float danger = MathHelper.Clamp((e.Needle - e.GreenEnd) / (1f - e.GreenEnd), 0f, 1f);

        var panel = Panel;
        panel.Inflate((int)((appear - 1f) * 20f), (int)((appear - 1f) * 12f));
        UiDraw.FancyPanel(sb, panel, Color.Lerp(new Color(255, 140, 80), Red, danger) * appear);

        if (appear < 0.6f) return;

        GameText.DrawCentered(sb, e.Title, panel.Center.X, panel.Y + 12, new Color(255, 150, 90) * appear, 2);

        // Yarım daire gösterge: soldan (π) sağa (2π) bölge renkleriyle
        var center = new Point(panel.Center.X, panel.Y + 152);
        const int steps = 60;
        for (int i = 0; i <= steps; i++)
        {
            float t = i / (float)steps;
            float angle = MathF.PI + t * MathF.PI;
            Color zone = t < e.GreenStart ? Cold
                       : t <= e.GreenEnd ? Green
                       : t < 0.85f ? Warn
                       : Red;
            if (t >= e.GreenStart && t <= e.GreenEnd)
                zone = Color.Lerp(zone, Color.White, 0.25f + 0.25f * MathF.Sin(time * 10f)); // Yeşil bölge nabız gibi parlar

            for (int r = Radius - 12; r <= Radius; r += 3)
            {
                int x = center.X + (int)(MathF.Cos(angle) * r);
                int y = center.Y + (int)(MathF.Sin(angle) * r);
                sb.FillRect(new Rectangle(x - 2, y - 2, 5, 5), zone);
            }
        }

        // İbre
        float needleAngle = MathF.PI + e.Needle * MathF.PI;
        Color needleColor = e.NeedleInGreen ? Green : Color.White;
        sb.Draw(Assets.Pixel, center.ToVector2(), null, Color.Black * 0.5f, needleAngle, new Vector2(0f, 0.5f),
            new Vector2(Radius - 4, 7f), SpriteEffects.None, 0f);
        sb.Draw(Assets.Pixel, center.ToVector2(), null, needleColor, needleAngle, new Vector2(0f, 0.5f),
            new Vector2(Radius - 6, 4f), SpriteEffects.None, 0f);
        sb.Draw(Assets.Circle, new Rectangle(center.X - 10, center.Y - 10, 20, 20), new Color(60, 64, 76));
        sb.Draw(Assets.Circle, new Rectangle(center.X - 5, center.Y - 5, 10, 10), new Color(200, 205, 220));
        if (danger > 0f)
            sb.DrawGlow(center.ToVector2() + new Vector2(0, -40), 120f, Red, 0.25f * danger);

        // Talimat ve tuşlar
        float pulse = e.NeedleInGreen ? 1f : 0.3f + 0.2f * MathF.Sin(time * 8f);
        GameText.DrawCentered(sb, Loc.T("ev.valve.instr"), panel.Center.X, center.Y + 16, Color.White * (0.6f + 0.4f * pulse));

        int keyY = center.Y + 44;
        UiDraw.KeyCap(sb, new Rectangle(panel.Center.X - 76, keyY, 40, 30), "E", e.NeedleInGreen ? Green : Hud.Amber, pulse);
        UiDraw.KeyCap(sb, new Rectangle(panel.Center.X - 24, keyY, 100, 30), "SPACE", e.NeedleInGreen ? Green : Hud.Amber, pulse);

        // Kalan süre
        var bar = new Rectangle(panel.X + 20, panel.Bottom - 10, panel.Width - 40, 4);
        sb.FillRect(bar, new Color(30, 34, 46));
        sb.FillRect(new Rectangle(bar.X, bar.Y, (int)(bar.Width * MathHelper.Clamp(e.TimerRatio, 0f, 1f)), bar.Height),
            Color.Lerp(Red, Warn, e.TimerRatio));
    }
}
