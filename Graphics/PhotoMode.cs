using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TheLastDelivery.Core;

namespace TheLastDelivery.Graphics;

public enum PhotoFilter { Normal, Noir, Sepia, Retro, Frost }

/// <summary>
/// Fotoğraf modu: oyun durur, arayüz gizlenir; dondurulan kare bir kez yakalanır.
/// Filtreler (Siyah-Beyaz, Sepya, Retro, Ayaz) yakalanan piksellere bir kez uygulanır ve önbelleğe alınır;
/// yakınlaştırma ve kaydırma yalnızca çizimde yapıldığı için her karede yeniden hesaplama gerekmez.
///   ←/→/↑/↓ ya da WASD: kaydır   Q/E: yakınlaştır   F: filtre   H: yardımı gizle   ENTER: kaydet   P/ESC: çık
/// </summary>
public sealed class PhotoMode : IDisposable
{
    private const float MaxZoom = 3f;

    private RenderTarget2D _capture;
    private Texture2D _filtered;
    private Color[] _source;
    private Color[] _work;
    private PhotoFilter _appliedFilter = (PhotoFilter)(-1);

    public bool IsActive { get; private set; }
    public bool Captured { get; private set; }
    public PhotoFilter Filter { get; private set; }
    public float Zoom { get; private set; } = 1f;
    public Vector2 Pan { get; private set; }   // Görünen alanın merkezinin ekran merkezine göre kayması (piksel)
    public bool HideHelp { get; private set; }

    /// <summary>Son kaydedilen fotoğrafın yolu ve bildirim süresi.</summary>
    public string LastSaved { get; private set; }
    public float SavedTimer { get; private set; }

    public void Begin()
    {
        IsActive = true;
        Captured = false;
        Zoom = 1f;
        Pan = Vector2.Zero;
        Filter = PhotoFilter.Normal;
        _appliedFilter = (PhotoFilter)(-1);
        SavedTimer = 0f;
    }

    public void End() => IsActive = false;

    /// <returns>Kaydetme istendi mi (ENTER)?</returns>
    public bool Update(float dt, InputState input)
    {
        SavedTimer = MathF.Max(0f, SavedTimer - dt);
        if (input.AnyPressed(Keys.P)) { End(); return false; }

        if (input.Pressed(Keys.F)) Filter = (PhotoFilter)(((int)Filter + 1) % 5);
        if (input.Pressed(Keys.H)) HideHelp = !HideHelp;
        if (input.Down(Keys.E)) Zoom = MathF.Min(MaxZoom, Zoom + dt * 1.2f);
        if (input.Down(Keys.Q)) Zoom = MathF.Max(1f, Zoom - dt * 1.2f);

        var move = Vector2.Zero;
        if (input.Down(Keys.A) || input.Down(Keys.Left)) move.X -= 1;
        if (input.Down(Keys.D) || input.Down(Keys.Right)) move.X += 1;
        if (input.Down(Keys.W) || input.Down(Keys.Up)) move.Y -= 1;
        if (input.Down(Keys.S) || input.Down(Keys.Down)) move.Y += 1;
        Pan += move * 380f * dt / Zoom;
        ClampPan();

        return input.Pressed(Keys.Enter);
    }

    private void ClampPan()
    {
        // Yakınlaştırılmış görüş kadrajın dışına taşmasın
        float halfW = GameConfig.ScreenWidth / 2f * (1f - 1f / Zoom);
        float halfH = GameConfig.ScreenHeight / 2f * (1f - 1f / Zoom);
        Pan = new Vector2(Math.Clamp(Pan.X, -halfW, halfW), Math.Clamp(Pan.Y, -halfH, halfH));
    }

    /// <summary>Kareyi yakalar: <paramref name="drawScene"/> arayüzsüz sahneyi çizer.</summary>
    public void Capture(GraphicsDevice device, Action drawScene)
    {
        int w = GameConfig.ScreenWidth, h = GameConfig.ScreenHeight;
        _capture ??= new RenderTarget2D(device, w, h, false, SurfaceFormat.Color, DepthFormat.None);
        _filtered ??= new Texture2D(device, w, h);
        _source ??= new Color[w * h];
        _work ??= new Color[w * h];

        var previous = device.GetRenderTargets();
        device.SetRenderTarget(_capture);
        drawScene();
        device.SetRenderTargets(previous);

        _capture.GetData(_source);
        Captured = true;
        _appliedFilter = (PhotoFilter)(-1);
    }

    /// <summary>Filtre değiştiyse pikselleri bir kez yeniden işler.</summary>
    private void EnsureFiltered()
    {
        if (_appliedFilter == Filter) return;
        _appliedFilter = Filter;
        ApplyFilter(_source, _work, Filter, GameConfig.ScreenWidth, GameConfig.ScreenHeight);
        _filtered.SetData(_work);
    }

    private static void ApplyFilter(Color[] src, Color[] dst, PhotoFilter filter, int w, int h)
    {
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int i = y * w + x;
                var c = src[i];
                switch (filter)
                {
                    case PhotoFilter.Noir:
                    {
                        // Sert kontrastlı siyah-beyaz
                        float l = (0.299f * c.R + 0.587f * c.G + 0.114f * c.B) / 255f;
                        l = Math.Clamp((l - 0.5f) * 1.35f + 0.5f, 0f, 1f);
                        byte v = (byte)(l * 255f);
                        dst[i] = new Color(v, v, v);
                        break;
                    }
                    case PhotoFilter.Sepia:
                    {
                        float r = c.R * 0.393f + c.G * 0.769f + c.B * 0.189f;
                        float g = c.R * 0.349f + c.G * 0.686f + c.B * 0.168f;
                        float b = c.R * 0.272f + c.G * 0.534f + c.B * 0.131f;
                        dst[i] = new Color((int)MathF.Min(255f, r), (int)MathF.Min(255f, g), (int)MathF.Min(255f, b));
                        break;
                    }
                    case PhotoFilter.Retro:
                    {
                        // 4x4 blok piksellenme, renk başına 3 bit, tarama çizgileri
                        int bx = x / 4 * 4, by = y / 4 * 4;
                        var s = src[by * w + bx];
                        int Q(int v) => (v >> 5) * 36 + 18;
                        float scan = (y % 4 == 3) ? 0.7f : 1f;
                        dst[i] = new Color((int)(Math.Min(255, Q(s.R)) * scan), (int)(Math.Min(255, Q(s.G)) * scan), (int)(Math.Min(255, Q(s.B)) * scan));
                        break;
                    }
                    case PhotoFilter.Frost:
                    {
                        // Ayaz: soğuk mavi ton, yumuşak parlaklık
                        dst[i] = new Color((int)(c.R * 0.8f + 20), (int)(c.G * 0.9f + 30), (int)MathF.Min(255f, c.B * 1.1f + 45));
                        break;
                    }
                    default:
                        dst[i] = c;
                        break;
                }
            }
        }
    }

    /// <summary>Yakınlaştırılmış ve kaydırılmış görüntüyü ekrana (ya da kayıt hedefine) çizer.</summary>
    public void Draw(SpriteBatch sb)
    {
        if (!Captured) return;
        EnsureFiltered();
        float w = GameConfig.ScreenWidth / Zoom, h = GameConfig.ScreenHeight / Zoom;
        var source = new Rectangle(
            (int)(GameConfig.ScreenWidth / 2f + Pan.X - w / 2f), (int)(GameConfig.ScreenHeight / 2f + Pan.Y - h / 2f), (int)w, (int)h);
        sb.Draw(_filtered, new Rectangle(0, 0, GameConfig.ScreenWidth, GameConfig.ScreenHeight), source, Color.White);
    }

    /// <summary>Görünen kareyi (filigranla) PNG olarak kaydeder.</summary>
    /// <param name="drawWatermark">Kaydedilen görüntünün köşesine filigran çizer.</param>
    /// <returns>Dosya yolu; kaydedilemezse null.</returns>
    public string Save(GraphicsDevice device, SpriteBatch sb, Action<SpriteBatch> drawWatermark)
    {
        try
        {
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TheLastDelivery", "Photos");
            Directory.CreateDirectory(folder);
            string path = Path.Combine(folder, $"photo_{DateTime.Now:yyyyMMdd_HHmmss}.png");

            using var target = new RenderTarget2D(device, GameConfig.ScreenWidth, GameConfig.ScreenHeight);
            var previous = device.GetRenderTargets();
            device.SetRenderTarget(target);
            device.Clear(Color.Black);
            sb.Begin(samplerState: SamplerState.PointClamp);
            Draw(sb);
            drawWatermark(sb);
            sb.End();
            device.SetRenderTargets(previous);

            using (var stream = File.Create(path))
                target.SaveAsPng(stream, GameConfig.ScreenWidth, GameConfig.ScreenHeight);
            LastSaved = path;
            SavedTimer = 3f;
            return path;
        }
        catch (Exception)
        {
            return null; // Disk dolu ya da izin yok: oyun etkilenmez
        }
    }

    public void Dispose()
    {
        _capture?.Dispose();
        _filtered?.Dispose();
    }
}
