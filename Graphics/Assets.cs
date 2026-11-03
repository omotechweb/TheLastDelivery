using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TheLastDelivery.Graphics;

/// <summary>
/// Oyun boyunca paylaşılan, kodla üretilmiş dokular ve font.
/// LoadContent içinde bir kez yüklenir, UnloadContent içinde serbest bırakılır.
/// </summary>
public static class Assets
{
    public static Texture2D Pixel { get; private set; }
    public static Texture2D Circle { get; private set; }
    public static Texture2D Glow { get; private set; }
    public static Texture2D Sky { get; private set; }
    public static Texture2D StormSky { get; private set; }
    public static Texture2D Vignette { get; private set; }
    public static Texture2D LightCone { get; private set; }
    public static Texture2D FrostEdge { get; private set; }
    public static Texture2D WheelLarge { get; private set; }
    public static Texture2D WheelSmall { get; private set; }
    public static PixelFont Font { get; private set; }

    public static void Load(GraphicsDevice gd)
    {
        Pixel = TextureFactory.CreatePixel(gd);
        Circle = TextureFactory.CreateCircle(gd, 32);
        Glow = TextureFactory.CreateGlow(gd, 64);
        Sky = TextureFactory.CreateVerticalGradient(gd, 720, new Color(4, 5, 12), new Color(30, 36, 60));
        StormSky = TextureFactory.CreateVerticalGradient(gd, 720, new Color(20, 21, 25), new Color(68, 70, 78));
        Vignette = TextureFactory.CreateVignette(gd, 320, 180);
        LightCone = TextureFactory.CreateLightCone(gd, 420, 180);
        FrostEdge = TextureFactory.CreateFrostEdge(gd, 320, 180, seed: 11);

        var rim = new Color(28, 28, 34);
        var spoke = new Color(48, 48, 56);
        WheelLarge = TextureFactory.CreateWheel(gd, 22, rim, spoke, 4);
        WheelSmall = TextureFactory.CreateWheel(gd, 14, rim, spoke, 3);

        Font = new PixelFont(gd);
        GameText.Initialize(gd, Font);
    }

    public static void Unload()
    {
        Pixel?.Dispose();
        Circle?.Dispose();
        Glow?.Dispose();
        Sky?.Dispose();
        StormSky?.Dispose();
        Vignette?.Dispose();
        LightCone?.Dispose();
        FrostEdge?.Dispose();
        WheelLarge?.Dispose();
        WheelSmall?.Dispose();
        GameText.Unload();
        Font?.Dispose();
    }
}
