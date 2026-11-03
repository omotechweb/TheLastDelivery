using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TheLastDelivery.Graphics;

/// <summary>SpriteBatch için basit şekil çizim kısayolları.</summary>
public static class DrawExtensions
{
    /// <summary>Dolu dikdörtgen.</summary>
    public static void FillRect(this SpriteBatch sb, Rectangle rect, Color color)
        => sb.Draw(Assets.Pixel, rect, color);

    /// <summary>İçi boş çerçeve.</summary>
    public static void DrawBorder(this SpriteBatch sb, Rectangle rect, int thickness, Color color)
    {
        sb.FillRect(new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
        sb.FillRect(new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color);
        sb.FillRect(new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
        sb.FillRect(new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color);
    }

    /// <summary>
    /// Toplamalı (additive) ışık halesi. Alfa kanalı 0 olan premultiplied renk,
    /// varsayılan AlphaBlend modunda ekrana eklenerek parlar; ayrı bir Begin/End gerekmez.
    /// </summary>
    public static void DrawGlow(this SpriteBatch sb, Vector2 center, float radius, Color color, float intensity)
    {
        var tint = new Color(color.R, color.G, color.B, (byte)0) * intensity;
        var rect = new Rectangle((int)(center.X - radius), (int)(center.Y - radius), (int)(radius * 2), (int)(radius * 2));
        sb.Draw(Assets.Glow, rect, tint);
    }
}
