using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TheLastDelivery.Core;
using TheLastDelivery.Graphics;
using TheLastDelivery.UI;
using TheLastDelivery.World;

namespace TheLastDelivery;

/// <summary>
/// Oyun hissi: parlak yüzeylerin ışık saçması (bloom), hasar anında kızıl çerçeve ve trenin üstünde yükselen
/// hasar sayıları, [H] ile düdük.
/// </summary>
public partial class Game1
{
    // dest − src: eşiğin altındaki parlaklık kırpılır, geriye yalnızca ışık kaynakları kalır
    private static readonly BlendState Threshold = new()
    {
        ColorSourceBlend = Blend.One,
        ColorDestinationBlend = Blend.One,
        ColorBlendFunction = BlendFunction.ReverseSubtract,
        AlphaSourceBlend = Blend.Zero,
        AlphaDestinationBlend = Blend.One,
        AlphaBlendFunction = BlendFunction.Add
    };

    private RenderTarget2D[] _bloom;

    /// <summary>
    /// Gölgelendirici kullanmadan bloom: sahne küçültülür, eşik rengi çıkarılır, iki kez daha küçültülerek
    /// yumuşatılır ve sahnenin üstüne toplanarak geri serilir. Gündüz karı ekranı yakmasın diye gece güçlenir.
    /// </summary>
    private void ApplyBloom()
    {
        if (!Settings.Bloom) return; // Ayarlardan kapatılabilir

        if (_bloom == null)
        {
            _bloom = new RenderTarget2D[3];
            for (int i = 0; i < 3; i++)
                _bloom[i] = new RenderTarget2D(GraphicsDevice, GameConfig.ScreenWidth >> (i + 2), GameConfig.ScreenHeight >> (i + 2),
                    false, SurfaceFormat.Color, DepthFormat.None);
        }

        float night = _state == GameState.Town && _townScene.IsInterior ? 1f : Scenery.NightAmount(CurrentView());
        float strength = MathHelper.Lerp(0.3f, 0.85f, night) * (1f - 0.5f * _weather.Intensity);
        float cut = MathHelper.Lerp(0.72f, 0.5f, night);

        Texture2D source = _canvas;
        for (int i = 0; i < _bloom.Length; i++)
        {
            GraphicsDevice.SetRenderTarget(_bloom[i]);
            var bounds = new Rectangle(0, 0, _bloom[i].Width, _bloom[i].Height);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.LinearClamp);
            _spriteBatch.Draw(source, bounds, Color.White);
            _spriteBatch.End();
            if (i == 0)
            {
                _spriteBatch.Begin(SpriteSortMode.Deferred, Threshold);
                _spriteBatch.Draw(Assets.Pixel, bounds, new Color(cut, cut, cut, 1f));
                _spriteBatch.End();
            }
            source = _bloom[i];
        }

        GraphicsDevice.SetRenderTarget(_canvas);
        var screen = new Rectangle(0, 0, GameConfig.ScreenWidth, GameConfig.ScreenHeight);
        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp);
        _spriteBatch.Draw(_bloom[1], screen, new Color(strength, strength, strength, 1f));
        _spriteBatch.Draw(_bloom[2], screen, new Color(strength, strength, strength, 1f));
        _spriteBatch.End();
    }

    // ------------------------------------------------------------------
    // Hasar geri bildirimi
    // ------------------------------------------------------------------

    private struct Floater
    {
        public string Text;
        public Vector2 Position;
        public float Age;
        public Color Color;
    }

    private readonly List<Floater> _floaters = new();
    private float _hitFlash;       // Hasar anında ekran kenarlarında yanan kızıl çerçeve (0-1)
    private float _damagePool;     // Sayıya dönüşmeyi bekleyen hasar
    private float _floaterCooldown;
    private float _whistleCooldown;

    private void ResetFeel()
    {
        _floaters.Clear();
        _hitFlash = _damagePool = _floaterCooldown = 0f;
    }

    /// <summary>Bu karede alınan gövde hasarı: çerçeve yanar, biriken hasar trenin üstünde sayı olarak yükselir.</summary>
    private void OnHullDamage(float amount)
    {
        _hitFlash = MathF.Min(1f, _hitFlash + amount / 14f + 0.15f);
        _damagePool += amount;
        GamepadInput.Rumble(0.3f + amount / 12f, 0.25f);
    }

    private void UpdateFeel(float dt)
    {
        _hitFlash = MathF.Max(0f, _hitFlash - dt * 2.6f);
        _whistleCooldown = MathF.Max(0f, _whistleCooldown - dt);
        _floaterCooldown -= dt;
        if (_damagePool >= 1f && _floaterCooldown <= 0f)
        {
            int shown = (int)MathF.Round(_damagePool);
            _damagePool = 0f;
            _floaterCooldown = 0.45f;
            _floaters.Add(new Floater
            {
                Text = "-" + shown,
                Position = new Vector2(Train.FrontX - 150 + _rng.Next(-40, 41), GameConfig.RailY - 150),
                Color = Hud.Danger
            });
        }

        for (int i = _floaters.Count - 1; i >= 0; i--)
        {
            var f = _floaters[i];
            f.Age += dt;
            f.Position.Y -= (70f - 40f * f.Age) * dt;
            if (f.Age > 1.3f) _floaters.RemoveAt(i);
            else _floaters[i] = f;
        }
    }

    /// <summary>[H]: düdük. Buhar sütunu ve ses; arka arkaya çalınamaz.</summary>
    private void TryWhistle(KeyboardState keys)
    {
        if (!WasPressed(keys, Keys.H) || _whistleCooldown > 0f) return;
        _whistleCooldown = 1.4f;
        _audio.PlayWhistle();
        _train.Whistle();
    }

    private void DrawFeel()
    {
        if (Settings.Flash && _hitFlash > 0.01f)
            _spriteBatch.Draw(Assets.Vignette, new Rectangle(0, 0, GameConfig.ScreenWidth, GameConfig.ScreenHeight),
                new Color(210, 30, 24) * (0.55f * _hitFlash));

        foreach (var f in _floaters)
        {
            float fade = MathHelper.Clamp((1.3f - f.Age) / 0.5f, 0f, 1f);
            int scale = f.Age < 0.12f ? 4 : 3; // İlk anda iri, sonra oturur
            GameText.DrawCentered(_spriteBatch, f.Text, f.Position.X + 2, f.Position.Y + 2, Color.Black * (0.7f * fade), scale);
            GameText.DrawCentered(_spriteBatch, f.Text, f.Position.X, f.Position.Y, f.Color * fade, scale);
        }
    }
}
