using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheLastDelivery.Core;
using TheLastDelivery.Graphics;

namespace TheLastDelivery.UI;

/// <summary>
/// Ekranın ortasında kısa süre beliren bildirimler (eşya kullanımı, yolcu kaybı vb.).
/// En fazla üç bildirim alt alta görünür; eskiler yukarı kayarak söner.
/// </summary>
public sealed class ToastFeed
{
    private const float Lifetime = 2.6f;
    private const int MaxVisible = 3;

    private sealed class Toast
    {
        public string Text;
        public Color Color;
        public float Age;
    }

    private readonly List<Toast> _toasts = new();

    public void Show(string text, Color color)
    {
        _toasts.Add(new Toast { Text = text, Color = color });
        if (_toasts.Count > MaxVisible)
            _toasts.RemoveAt(0);
    }

    public void Clear() => _toasts.Clear();

    public void Update(float dt)
    {
        for (int i = _toasts.Count - 1; i >= 0; i--)
        {
            _toasts[i].Age += dt;
            if (_toasts[i].Age >= Lifetime)
                _toasts.RemoveAt(i);
        }
    }

    public void Draw(SpriteBatch sb)
    {
        float y = 340;

        foreach (var toast in _toasts)
        {
            // Hızla belirir, son yarım saniyede söner
            float alpha = MathHelper.Clamp(toast.Age * 6f, 0f, 1f)
                        * MathHelper.Clamp((Lifetime - toast.Age) / 0.5f, 0f, 1f);
            float rise = MathHelper.Clamp(toast.Age * 6f, 0f, 1f) * 6f;

            float width = GameText.Measure(toast.Text, 2).X + 32;
            var box = new Rectangle((int)(GameConfig.ScreenWidth / 2f - width / 2f), (int)(y - rise), (int)width, 32);
            sb.FillRect(box, new Color(6, 8, 16) * (0.8f * alpha));
            sb.FillRect(new Rectangle(box.X, box.Y, 4, box.Height), toast.Color * alpha);
            GameText.DrawCentered(sb, toast.Text, box.Center.X + 2, box.Y + 6, toast.Color * alpha);

            y += 38;
        }
    }
}
