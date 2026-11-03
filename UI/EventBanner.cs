using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheLastDelivery.Core;
using TheLastDelivery.Events;
using TheLastDelivery.Graphics;
using TheLastDelivery.Localization;

namespace TheLastDelivery.UI;

/// <summary>Ekranın altında beliren olay uyarısı ve olay sonucu bildirimi.</summary>
public static class EventBanner
{
    // Envanter panelinin sağında, ekranın alt şeridinde
    private static readonly Rectangle Box = new(InventoryPanel.Panel.Right + 16, 590, GameConfig.ScreenWidth - InventoryPanel.Panel.Right - 40, 86);

    private static readonly Color IceColor = new(150, 210, 255);
    private static readonly Color LeakColor = new(255, 140, 80);
    private static readonly Color CacheColor = new(230, 200, 110);
    private static readonly Color WolfColor = new(230, 120, 90);
    private static readonly Color SwitchColor = new(240, 180, 80);
    private static readonly Color BlizzardColor = new(210, 230, 255);
    private static readonly Color MineColor = new(220, 170, 100);
    private static readonly Color LampColor = new(255, 225, 150);
    private static readonly Color AcidColor = new(180, 240, 80);
    private static readonly Color Success = new(110, 210, 120);

    public static void Draw(SpriteBatch sb, RoadEventSystem events, float time)
    {
        if (events.Active != null && events.Active.Type == RoadEventType.ValveLeak)
            ValveGauge.Draw(sb, events.Active, time); // Vana arızası: ekranın ortasında basınç ibresi mini oyunu
        else if (events.Active != null)
            DrawActive(sb, events, events.Active, time);
        else if (events.ResultTimer > 0f)
            DrawResult(sb, events);
    }

    private static void DrawActive(SpriteBatch sb, RoadEventSystem events, RoadEvent e, float time)
    {
        // Aşağıdan kayarak belirir
        float appear = MathHelper.Clamp(e.Age * 4f, 0f, 1f);
        var box = Box;
        box.Y += (int)((1f - appear) * 24f);

        Color accent = e.Type switch
        {
            RoadEventType.IcyRails => IceColor,
            RoadEventType.ValveLeak => LeakColor,
            RoadEventType.WolfPack => WolfColor,
            RoadEventType.BrokenSwitch => SwitchColor,
            RoadEventType.Blizzard => BlizzardColor,
            RoadEventType.MineEntrance => MineColor,
            RoadEventType.LampFailure => LampColor,
            RoadEventType.AcidStorm => AcidColor,
            _ => CacheColor
        };
        float pulse = 0.6f + 0.4f * MathF.Sin(time * 10f);

        sb.FillRect(box, new Color(6, 8, 16) * (0.88f * appear));
        sb.DrawBorder(box, 2, accent * (pulse * appear));

        GameText.Draw(sb, e.Title, new Vector2(box.X + 18, box.Y + 12), accent * appear, 3);
        GameText.Draw(sb, e.Instruction, new Vector2(box.X + 18, box.Y + 46), Hud.Value * appear);

        // Sağ üst köşe: telsizden erken uyarı geldiyse kimin haber verdiği
        float rightEdge = box.Right - 18;
        if (e.WarnedBy != null)
        {
            string warned = Loc.F("ev.warned", e.WarnedBy);
            rightEdge -= GameText.Measure(warned, 2).X;
            GameText.Draw(sb, warned, new Vector2(rightEdge, box.Y + 16), Hud.Amber * appear);
            rightEdge -= 16;
        }

        // Kızaklanma sırasında yanıp sönen hasar uyarısı (telsiz notunun solunda)
        if (events.IsGrinding && MathF.Sin(time * 14f) > 0f)
        {
            string damage = Loc.T("ev.damage");
            float w = GameText.Measure(damage, 2).X;
            GameText.Draw(sb, damage, new Vector2(rightEdge - w, box.Y + 16), Hud.Danger);
        }

        // Sağ üst: makasta seçilen kol
        if (e.Type == RoadEventType.BrokenSwitch)
        {
            string chosen = Loc.F("ev.switch.chosen", Loc.T(e.ChosenBranch == 1 ? "ev.switch.right" : "ev.switch.left"));
            float w = GameText.Measure(chosen, 2).X;
            GameText.Draw(sb, chosen, new Vector2(rightEdge - w, box.Y + 16), Hud.Amber * appear);
        }

        // Alt çubuk: tipide çığ riski, kurtlarda sürünün saldırganlığı, diğerlerinde kalan süre
        var bar = new Rectangle(box.X + 18, box.Bottom - 12, box.Width - 36, 4);
        sb.FillRect(bar, new Color(30, 34, 46) * appear);
        float ratio;
        Color barColor;
        switch (e.Type)
        {
            case RoadEventType.Blizzard:
                ratio = e.AvalancheRisk;
                barColor = Color.Lerp(BlizzardColor, Hud.Danger, ratio);
                string risk = Loc.F("ev.blizzard.risk", (int)MathF.Round(MathHelper.Clamp(ratio, 0f, 1f) * 100f));
                float rw = GameText.Measure(risk, 2).X;
                GameText.Draw(sb, risk, new Vector2(rightEdge - rw, box.Y + 16),
                    (ratio > 0.6f && MathF.Sin(time * 12f) > 0f ? Hud.Danger : BlizzardColor) * appear);
                break;
            case RoadEventType.WolfPack:
                ratio = e.Aggression;
                barColor = WolfColor;
                break;
            default:
                ratio = e.TimerRatio;
                barColor = e.Type == RoadEventType.IcyRails && !e.InGracePeriod ? Hud.Danger : accent;
                break;
        }
        sb.FillRect(new Rectangle(bar.X, bar.Y, (int)(bar.Width * MathHelper.Clamp(ratio, 0f, 1f)), bar.Height),
            barColor * appear);
    }

    private static void DrawResult(SpriteBatch sb, RoadEventSystem events)
    {
        float alpha = MathHelper.Clamp(events.ResultTimer / 0.5f, 0f, 1f); // Son yarım saniyede söner
        Color color = events.ResultSuccess ? Success : Hud.Danger;

        float width = GameText.Measure(events.ResultMessage, 2).X + 40;
        var box = new Rectangle((int)(Box.Center.X - width / 2f), Box.Y + 30, (int)width, 40);
        sb.FillRect(box, new Color(6, 8, 16) * (0.85f * alpha));
        sb.DrawBorder(box, 2, color * alpha);
        GameText.DrawCentered(sb, events.ResultMessage, box.Center.X, box.Y + 9, color * alpha);
    }
}
