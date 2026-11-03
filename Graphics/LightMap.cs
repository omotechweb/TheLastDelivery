using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheLastDelivery.Core;

namespace TheLastDelivery.Graphics;

/// <summary>
/// Gece, tünel ve tipi karanlığı için ışık haritası.
/// Ekran boyutunda bir maske önce karanlıkla doldurulur, ardından ışık kaynakları
/// (far konisi, fenerler, pencereler) "silme" karışımıyla maskeden çıkarılır.
/// Sahne çizildikten sonra maske üstüne serilir: ışığın düştüğü yer açık, gerisi karanlık kalır.
/// Maske sahneden önce hazırlanır, böylece çizim sırasında ekran hedefi değiştirilmez.
/// </summary>
public sealed class LightMap : IDisposable
{
    // Hedef = hedef − kaynak: ışık dokusunun alfası karanlığın alfasını azaltır
    private static readonly BlendState Erase = new()
    {
        ColorSourceBlend = Blend.One,
        ColorDestinationBlend = Blend.One,
        ColorBlendFunction = BlendFunction.ReverseSubtract,
        AlphaSourceBlend = Blend.One,
        AlphaDestinationBlend = Blend.One,
        AlphaBlendFunction = BlendFunction.ReverseSubtract
    };

    private RenderTarget2D _mask;

    /// <summary>Bu karede uygulanacak karanlık (0 = yok). 0 ise maske hazırlanmaz ve çizilmez.</summary>
    public float Darkness { get; private set; }

    /// <summary>
    /// Maskeyi hazırlar. <paramref name="drawLights"/> içinde beyaz ışık dokuları çizilir;
    /// rengin alfası o noktadaki aydınlığı belirler.
    /// </summary>
    public void Prepare(GraphicsDevice device, SpriteBatch sb, float darkness, Color tint, Matrix transform,
        Action<SpriteBatch> drawLights)
    {
        Darkness = MathHelper.Clamp(darkness, 0f, 1f);
        if (Darkness <= 0.01f) return;

        _mask ??= new RenderTarget2D(device, GameConfig.ScreenWidth, GameConfig.ScreenHeight, false,
            SurfaceFormat.Color, DepthFormat.None);

        var previous = device.GetRenderTargets(); // Ekran ya da (testlerde) başka bir hedef: sonra geri yüklenir
        device.SetRenderTarget(_mask);
        device.Clear(new Color((byte)(tint.R * Darkness), (byte)(tint.G * Darkness), (byte)(tint.B * Darkness), (byte)(255 * Darkness)));

        sb.Begin(blendState: Erase, samplerState: SamplerState.LinearClamp, transformMatrix: transform);
        drawLights(sb);
        sb.End();

        device.SetRenderTargets(previous);
    }

    /// <summary>Hazırlanan karanlığı sahnenin üstüne serer (AlphaBlend açık bir SpriteBatch içinde).</summary>
    public void Draw(SpriteBatch sb)
    {
        if (Darkness <= 0.01f || _mask == null) return;
        sb.Draw(_mask, Vector2.Zero, Color.White);
    }

    public void Dispose() => _mask?.Dispose();
}
