using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TheLastDelivery.Core;

namespace TheLastDelivery;

/// <summary>
/// Görüntü katmanı: oyun her zaman 1280x720'lik sanal bir tuvale çizilir, tuval pencereye (ya da tam ekrana)
/// en-boy oranı korunarak ölçeklenir. F11 / ALT+ENTER tam ekranı açıp kapatır; pencere serbestçe boyutlanır.
///
/// Ayrıca geliştirici betiği: TLD_SCRIPT ortam değişkeni bir betik dosyasını gösteriyorsa tuşlar betikten
/// okunur ve "shot" komutu tuvali PNG olarak kaydeder (oyunu elle oynamadan ekran görüntüsü almak için).
/// </summary>
public partial class Game1
{
    private RenderTarget2D _canvas;
    private bool _fullscreen;
    private Point _windowedSize = new(GameConfig.ScreenWidth, GameConfig.ScreenHeight);

    private void InitDisplay()
    {
        Window.AllowUserResizing = true;
        _canvas = new RenderTarget2D(GraphicsDevice, GameConfig.ScreenWidth, GameConfig.ScreenHeight, false,
            SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
        LoadDevScript();

        // Son oturumda tam ekrandaysa öyle açılır
        try
        {
            if (_devScript == null && File.Exists(DisplayFile) && File.ReadAllText(DisplayFile).Trim() == "fullscreen")
                ToggleFullscreen();
        }
        catch (Exception) { /* Ayar okunamadı: pencere kipinde açılır */ }
    }

    private static string DisplayFile => AppPaths.Combine("display.txt");

    private void ToggleFullscreen()
    {
        _fullscreen = !_fullscreen;
        if (_fullscreen)
        {
            _windowedSize = new Point(Window.ClientBounds.Width, Window.ClientBounds.Height);
            var mode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
            _graphics.PreferredBackBufferWidth = mode.Width;
            _graphics.PreferredBackBufferHeight = mode.Height;
            _graphics.HardwareModeSwitch = false; // Kenarlıksız tam ekran: çözünürlük değişmez, ALT+TAB sorunsuz
            _graphics.IsFullScreen = true;
        }
        else
        {
            _graphics.IsFullScreen = false;
            _graphics.PreferredBackBufferWidth = Math.Max(640, _windowedSize.X);
            _graphics.PreferredBackBufferHeight = Math.Max(360, _windowedSize.Y);
        }
        _graphics.ApplyChanges();
        IsMouseVisible = !_fullscreen;
        try
        {
            Directory.CreateDirectory(AppPaths.DataDir);
            File.WriteAllText(DisplayFile, _fullscreen ? "fullscreen" : "windowed");
        }
        catch (Exception) { /* Ayar yazılamadı: tercih yalnızca bu oturumda geçerli */ }
    }

    /// <summary>Tuvali pencereye sığdırır: tam katlarda keskin (PointClamp), ara ölçeklerde yumuşak (LinearClamp).</summary>
    private void PresentCanvas()
    {
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);
        var pp = GraphicsDevice.PresentationParameters;
        float scale = MathF.Min(pp.BackBufferWidth / (float)GameConfig.ScreenWidth, pp.BackBufferHeight / (float)GameConfig.ScreenHeight);
        int w = (int)MathF.Round(GameConfig.ScreenWidth * scale), h = (int)MathF.Round(GameConfig.ScreenHeight * scale);
        var dest = new Rectangle((pp.BackBufferWidth - w) / 2, (pp.BackBufferHeight - h) / 2, w, h);
        bool integer = MathF.Abs(scale - MathF.Round(scale)) < 0.001f;
        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, integer ? SamplerState.PointClamp : SamplerState.LinearClamp);
        _spriteBatch.Draw(_canvas, dest, Color.White);
        _spriteBatch.End();
    }

    // ------------------------------------------------------------------
    // Geliştirici betiği
    // ------------------------------------------------------------------

    private List<string[]> _devScript;
    private int _devLine;
    private float _devWait;
    private readonly List<(Keys Key, float Left)> _devHeld = new();
    private string _devShot;
    private string _devDir;
    private float _devSpeed = 1f;

    private void LoadDevScript()
    {
        string path = Environment.GetEnvironmentVariable("TLD_SCRIPT");
        if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;
        _devDir = Path.GetDirectoryName(Path.GetFullPath(path));
        _devScript = new List<string[]>();
        foreach (string raw in File.ReadAllLines(path))
        {
            string line = raw.Trim();
            if (line.Length == 0 || line[0] == '#') continue;
            _devScript.Add(line.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }
    }

    /// <summary>Betik etkinse klavye durumunu betikteki tuşlarla değiştirir ve dt'yi hızlandırır.</summary>
    private KeyboardState DevInput(KeyboardState real, ref float dt)
    {
        if (_devScript == null) return real;
        float realDt = dt;
        dt *= _devSpeed;

        for (int i = _devHeld.Count - 1; i >= 0; i--)
        {
            var h = _devHeld[i];
            h.Left -= realDt;
            if (h.Left <= 0f) _devHeld.RemoveAt(i); else _devHeld[i] = h;
        }

        _devWait -= realDt;
        while (_devWait <= 0f && _devShot == null && _devLine < _devScript.Count)
        {
            string[] cmd = _devScript[_devLine++];
            float Arg(int i, float fallback) => cmd.Length > i ? float.Parse(cmd[i], CultureInfo.InvariantCulture) : fallback;
            switch (cmd[0])
            {
                case "wait": _devWait = Arg(1, 1f); break;
                case "key": _devHeld.Add((Enum.Parse<Keys>(cmd[1]), 0.05f)); _devWait = 0.15f; break;
                case "hold": _devHeld.Add((Enum.Parse<Keys>(cmd[1]), Arg(2, 1f))); break;
                case "speed": _devSpeed = Arg(1, 1f); break;
                case "shot": _devShot = cmd[1]; break;
                case "do": DevCommand(cmd); break;
                case "quit": Exit(); break;
            }
        }

        var keys = new Keys[_devHeld.Count];
        for (int i = 0; i < keys.Length; i++) keys[i] = _devHeld[i].Key;
        return new KeyboardState(keys);
    }

    /// <summary>"do" komutları: sahneyi doğrudan kurar (bölüm, olay, gece, konum).</summary>
    private void DevCommand(string[] cmd)
    {
        float Arg(int i, float fallback) => cmd.Length > i ? float.Parse(cmd[i], CultureInfo.InvariantCulture) : fallback;
        switch (cmd[1])
        {
            case "run": StartRun((int)Arg(2, 1)); break;
            case "ep2": StartEpisode2((int)Arg(2, 1)); break;
            case "endless": StartEndless(); break;
            case "drive": ChangeState(GameState.Driving); break;
            case "event": _events.ForceStart(Enum.Parse<Events.RoadEventType>(cmd[2]), _cabin); break;
            case "phase": _dayCycle.SetPhase(Arg(2, 0.75f)); break;
            case "night": while (!_dayCycle.IsNight) _dayCycle.Update(1f); break;
            case "km": _travelledKm += Arg(2, 1f); break;
            case "tostop": _travelledKm = NextStopKm - GameConfig.ApproachKm * Arg(2, 1.2f); break;
            case "chase": StartChase(); break;
            case "storm": for (int i = 0; i < 600; i++) _weather.Update(0.1f, 1f, 1f, true); break;
            case "damage": _train.Damage(Arg(2, 10f)); break;
            case "die": _train.Damage(999f); break;
        }
    }

    private void DevSaveShot()
    {
        if (_devShot == null) return;
        try
        {
            using var stream = File.Create(Path.Combine(_devDir, _devShot + ".png"));
            _canvas.SaveAsPng(stream, _canvas.Width, _canvas.Height);
        }
        catch (Exception) { /* Geliştirici aracı: kaydedilemeyen kare oyunu düşürmez */ }
        _devShot = null;
    }
}
