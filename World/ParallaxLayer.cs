using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TheLastDelivery.World;

/// <summary>
/// Yatayda sonsuz döşenen tek bir arka plan katmanı.
/// SpeedFactor ne kadar küçükse katman o kadar "uzakta" görünür (parallax etkisi).
/// </summary>
public sealed class ParallaxLayer : IDisposable
{
    private readonly Texture2D _texture;
    private readonly float _speedFactor;
    private readonly int _y;
    private readonly Color _tint;
    private float _offset;

    public ParallaxLayer(Texture2D texture, float speedFactor, int y, Color? tint = null)
    {
        _texture = texture;
        _speedFactor = speedFactor;
        _y = y;
        _tint = tint ?? Color.White;
    }

    /// <param name="scrollPixels">Ön katmanın bu karede kaydığı piksel miktarı.</param>
    public void Update(float scrollPixels)
    {
        _offset = (_offset + scrollPixels * _speedFactor) % _texture.Width;
    }

    public void Draw(SpriteBatch spriteBatch, int screenWidth)
    {
        // Ekranı dolduracak kadar dokuyu yan yana diz
        int x = -(int)_offset;
        while (x < screenWidth)
        {
            spriteBatch.Draw(_texture, new Vector2(x, _y), _tint);
            x += _texture.Width;
        }
    }

    public void Dispose() => _texture.Dispose();
}
