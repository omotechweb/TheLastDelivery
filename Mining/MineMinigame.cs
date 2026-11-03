using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TheLastDelivery.Audio;
using TheLastDelivery.Core;
using TheLastDelivery.Economy;
using TheLastDelivery.Graphics;
using TheLastDelivery.Localization;
using TheLastDelivery.UI;
using TheLastDelivery.World;

namespace TheLastDelivery.Mining;

/// <summary>
/// Terk edilmiş maden: 2D ızgarada zamanla yarışarak kazı.
///   • Arthur girişten (sol üst) başlar; [W/A/S/D] ya da ok tuşlarıyla yürür.
///   • Kaya hücresine doğru yürümek kazmayı vurur; toprak 1, taş 2, sert kaya 3 vuruşta kırılır.
///   • Kırılan kayadan Hurda, Altın Parçası, Nadir Cevher çıkabilir; bazı kayalar gaz cebi saklar (süre kaybı).
///   • Işık yalnızca fenerin çevresini aydınlatır (Gelişmiş Fener ile daha geniş).
///   • Süre dolunca ya da [ENTER] ile çıkılır; bulunanlar kargoya yüklenir.
/// </summary>
public sealed class MineMinigame
{
    private enum Rock { Open, Dirt, Stone, Hard }
    private enum Ore { None, Scrap, Gold, Rare, Gas }

    private const int Cols = 13;
    private const int Rows = 7;
    private const int Cell = 60;
    private static readonly Point GridOrigin = new((GameConfig.ScreenWidth - Cols * Cell) / 2, 190);

    private struct Popup
    {
        public string Text;
        public Vector2 Position;
        public float Age;
        public Color Color;
    }

    private readonly Random _rng = new();
    private readonly Rock[,] _rock = new Rock[Cols, Rows];
    private readonly Ore[,] _ore = new Ore[Cols, Rows];
    private readonly int[,] _hits = new int[Cols, Rows];
    private readonly ParticleSystem _dust = new();
    private readonly List<Popup> _popups = new();
    private Point _arthur;
    private float _timeLeft;
    private float _hitCooldown;
    private float _swing;
    private float _shake;
    private float _time;
    private float _lightRadius;
    private int _facing = 1;
    private bool _finished;
    private float _resultTimer;

    /// <summary>Bu seferde toplananlar (çıkışta kargoya yüklenir).</summary>
    public List<LootType> Found { get; } = new();

    /// <summary>Sonuç ekranı gösterildi ve oyuncu SPACE ile ayrıldı.</summary>
    public bool IsDone { get; private set; }

    public void Start(bool lantern)
    {
        _lightRadius = lantern ? 3.6f : 2.3f;
        _timeLeft = GameConfig.MineDuration;
        _hitCooldown = 0f;
        _swing = 0f;
        _shake = 0f;
        _time = 0f;
        _finished = false;
        _resultTimer = 0f;
        IsDone = false;
        Found.Clear();
        _popups.Clear();
        _dust.Clear();
        _arthur = new Point(0, 0);

        for (int x = 0; x < Cols; x++)
        {
            for (int y = 0; y < Rows; y++)
            {
                _hits[x, y] = 0;
                // Derinlere (sağa ve aşağı) inildikçe kaya sertleşir, cevher zenginleşir
                float depth = (x / (float)(Cols - 1) + y / (float)(Rows - 1)) * 0.5f;
                double r = _rng.NextDouble();
                _rock[x, y] = r < 0.45 - depth * 0.3 ? Rock.Dirt : r < 0.85 - depth * 0.15 ? Rock.Stone : Rock.Hard;

                double o = _rng.NextDouble();
                _ore[x, y] = o < 0.05 + depth * 0.06 ? Ore.Rare
                           : o < 0.17 + depth * 0.12 ? Ore.Gold
                           : o < 0.40 + depth * 0.05 ? Ore.Scrap
                           : o < 0.46 ? Ore.Gas
                           : Ore.None;
            }
        }
        _rock[0, 0] = Rock.Open; // Giriş
        _ore[0, 0] = Ore.None;
    }

    public void Update(float dt, InputState input, ProceduralAudio audio)
    {
        _time += dt;
        _swing = MathF.Max(0f, _swing - dt * 6f);
        _shake = MathF.Max(0f, _shake - dt * 20f);
        _dust.Update(dt, 0f);
        for (int i = _popups.Count - 1; i >= 0; i--)
        {
            var p = _popups[i];
            p.Age += dt;
            p.Position.Y -= 30f * dt;
            _popups[i] = p;
            if (p.Age > 1.4f) _popups.RemoveAt(i);
        }

        if (_finished)
        {
            _resultTimer += dt;
            if (_resultTimer > 0.6f && input.AnyPressed(Keys.Space, Keys.Enter))
                IsDone = true;
            return;
        }

        _timeLeft -= dt;
        if (_timeLeft <= 0f || input.Pressed(Keys.Enter))
        {
            _timeLeft = MathF.Max(0f, _timeLeft);
            _finished = true;
            audio.PlayWhistle();
            return;
        }

        _hitCooldown -= dt;
        if (_hitCooldown > 0f) return;

        // Basılı tutulan yön tekrar eder (kazı ritmi)
        int dx = 0, dy = 0;
        if (input.Down(Keys.A) || input.Down(Keys.Left)) dx = -1;
        else if (input.Down(Keys.D) || input.Down(Keys.Right)) dx = 1;
        else if (input.Down(Keys.W) || input.Down(Keys.Up)) dy = -1;
        else if (input.Down(Keys.S) || input.Down(Keys.Down)) dy = 1;
        if (dx == 0 && dy == 0) return;
        if (dx != 0) _facing = dx;

        int tx = _arthur.X + dx, ty = _arthur.Y + dy;
        if (tx < 0 || ty < 0 || tx >= Cols || ty >= Rows) return;

        if (_rock[tx, ty] == Rock.Open)
        {
            _arthur = new Point(tx, ty);
            _hitCooldown = 0.12f;
            return;
        }

        Strike(tx, ty, audio);
    }

    private void Strike(int x, int y, ProceduralAudio audio)
    {
        _hitCooldown = GameConfig.MineHitCooldown;
        _swing = 1f;
        _shake = 3f;
        _hits[x, y]++;
        audio.PlayPump(); // Metal darbe sesi
        EmitDust(CellCenter(x, y), 6, RockColor(_rock[x, y]));

        int needed = _rock[x, y] switch { Rock.Dirt => 1, Rock.Stone => 2, _ => 3 };
        if (_hits[x, y] < needed) return;

        _rock[x, y] = Rock.Open;
        EmitDust(CellCenter(x, y), 14, RockColor(Rock.Stone));
        var center = CellCenter(x, y);

        switch (_ore[x, y])
        {
            case Ore.Scrap: Collect(LootType.ScrapMetal, center, audio); break;
            case Ore.Gold: Collect(LootType.GoldNugget, center, audio); break;
            case Ore.Rare: Collect(LootType.RareOre, center, audio); break;
            case Ore.Gas:
                _timeLeft = MathF.Max(0f, _timeLeft - 4f);
                _shake = 10f;
                audio.PlayExplosion();
                AddPopup(Loc.T("mine.gas"), center, Hud.Danger);
                break;
        }
        _ore[x, y] = Ore.None;
    }

    private void Collect(LootType loot, Vector2 at, ProceduralAudio audio)
    {
        Found.Add(loot);
        audio.PlayCoin();
        AddPopup("+" + LootCatalog.Name(loot), at, LootCatalog.Color(loot));
    }

    private void AddPopup(string text, Vector2 at, Color color) =>
        _popups.Add(new Popup { Text = text, Position = at - new Vector2(0, 20), Color = color });

    private void EmitDust(Vector2 at, int count, Color color)
    {
        for (int i = 0; i < count; i++)
        {
            _dust.Emit(new Particle
            {
                Position = at + new Vector2(_rng.Next(-14, 15), _rng.Next(-14, 15)),
                Velocity = new Vector2(_rng.Next(-90, 91), -_rng.Next(20, 120)),
                Life = 0.4f + (float)_rng.NextDouble() * 0.4f,
                Size = 2f + _rng.Next(3),
                Color = color,
                Alpha = 0.9f,
                Gravity = 380f
            });
        }
    }

    private static Vector2 CellCenter(int x, int y) =>
        new(GridOrigin.X + x * Cell + Cell / 2f, GridOrigin.Y + y * Cell + Cell / 2f);

    private static Color RockColor(Rock rock) => rock switch
    {
        Rock.Dirt => new Color(92, 66, 46),
        Rock.Stone => new Color(78, 80, 90),
        Rock.Hard => new Color(48, 46, 56),
        _ => new Color(16, 14, 18)
    };

    // ------------------------------------------------------------------
    // Çizim
    // ------------------------------------------------------------------

    public void Draw(SpriteBatch sb)
    {
        var shake = new Point((int)(MathF.Sin(_time * 70f) * _shake), (int)(MathF.Cos(_time * 53f) * _shake * 0.5f));
        sb.FillRect(new Rectangle(0, 0, GameConfig.ScreenWidth, GameConfig.ScreenHeight), new Color(6, 5, 8));

        GameText.DrawCentered(sb, Loc.T("mine.title"), GameConfig.ScreenWidth / 2f, 28, Hud.Amber, 4);
        GameText.DrawCentered(sb, Loc.T("mine.instr"), GameConfig.ScreenWidth / 2f, 92, Hud.Label);

        // Süre çubuğu
        var bar = new Rectangle(GridOrigin.X, 130, Cols * Cell, 18);
        float ratio = _timeLeft / GameConfig.MineDuration;
        UiDraw.FancyBar(sb, bar, ratio, ratio, Color.Lerp(Hud.Danger, new Color(255, 200, 110), ratio),
            _timeLeft < 6f ? 0.5f + 0.5f * MathF.Sin(_time * 10f) : 0f);
        GameText.DrawRight(sb, _timeLeft.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + "s", bar.Right, 154, Hud.Value);
        GameText.Draw(sb, Loc.F("mine.found", Found.Count), new Vector2(GridOrigin.X, 154), Hud.Value);

        for (int x = 0; x < Cols; x++)
        {
            for (int y = 0; y < Rows; y++)
            {
                var r = new Rectangle(GridOrigin.X + x * Cell + shake.X, GridOrigin.Y + y * Cell + shake.Y, Cell, Cell);
                float distance = Vector2.Distance(new Vector2(x, y), _arthur.ToVector2());
                float light = MathHelper.Clamp(1f - (distance - _lightRadius + 1f) / 1.5f, 0.06f, 1f);
                DrawCell(sb, r, x, y, light);
            }
        }

        DrawArthur(sb, shake);
        _dust.Draw(sb);

        foreach (var p in _popups)
        {
            float a = MathHelper.Clamp(1.4f - p.Age, 0f, 1f);
            GameText.DrawCentered(sb, p.Text, p.Position.X, p.Position.Y, p.Color * a);
        }

        if (_finished) DrawResult(sb);
    }

    private void DrawCell(SpriteBatch sb, Rectangle r, int x, int y, float light)
    {
        var rock = _rock[x, y];
        if (rock == Rock.Open)
        {
            sb.FillRect(r, new Color(22, 18, 20) * light);
            sb.FillRect(new Rectangle(r.X, r.Bottom - 6, r.Width, 6), new Color(40, 32, 30) * light); // Zemin
            if (x == 0 && y == 0) // Giriş: ahşap destek
            {
                sb.FillRect(new Rectangle(r.X + 4, r.Y + 4, 6, r.Height - 8), new Color(110, 80, 50) * light);
                sb.FillRect(new Rectangle(r.Right - 10, r.Y + 4, 6, r.Height - 8), new Color(110, 80, 50) * light);
                sb.FillRect(new Rectangle(r.X + 2, r.Y + 2, r.Width - 4, 6), new Color(130, 96, 60) * light);
            }
            return;
        }

        var color = RockColor(rock);
        sb.FillRect(r, color * light);
        sb.FillRect(new Rectangle(r.X, r.Y, r.Width, 3), UiDraw.Shade(color, 1.3f) * light);
        sb.DrawBorder(r, 1, new Color(10, 8, 10) * light);

        // Doku: birkaç koyu benek
        for (int i = 0; i < 4; i++)
        {
            int px = (x * 37 + y * 11 + i * 17) % (Cell - 12) + 6;
            int py = (x * 13 + y * 29 + i * 23) % (Cell - 12) + 6;
            sb.FillRect(new Rectangle(r.X + px, r.Y + py, 4, 3), UiDraw.Shade(color, 0.7f) * light);
        }

        // Çatlaklar: vuruş aldıkça büyür
        int hits = _hits[x, y];
        for (int i = 0; i < hits; i++)
        {
            sb.FillRect(new Rectangle(r.X + 14 + i * 9, r.Y + 10 + i * 6, 3, 24 - i * 4), new Color(8, 6, 8) * light);
            sb.FillRect(new Rectangle(r.X + 10 + i * 8, r.Y + 30 - i * 3, 18, 2), new Color(8, 6, 8) * light);
        }

        // Işıkta cevher damarları parıldar
        if (light > 0.5f)
        {
            var ore = _ore[x, y];
            if (ore is Ore.Gold or Ore.Rare)
            {
                var glint = ore == Ore.Gold ? new Color(255, 210, 80) : new Color(200, 140, 255);
                float twinkle = 0.5f + 0.5f * MathF.Sin(_time * 5f + x * 1.7f + y);
                sb.FillRect(new Rectangle(r.X + 18, r.Y + 22, 4, 4), glint * (light * twinkle));
                sb.FillRect(new Rectangle(r.X + 36, r.Y + 38, 3, 3), glint * (light * (1f - twinkle)));
            }
        }
    }

    private void DrawArthur(SpriteBatch sb, Point shake)
    {
        var c = CellCenter(_arthur.X, _arthur.Y) + shake.ToVector2();
        sb.DrawGlow(c, Cell * _lightRadius * 0.9f, new Color(255, 200, 120), 0.18f);

        int fx(int dx, int w) => _facing >= 0 ? (int)c.X + dx : (int)c.X - dx - w;
        void Px(int dx, int dy, int w, int h, Color col) => sb.FillRect(new Rectangle(fx(dx, w), (int)c.Y + dy, w, h), col);

        Px(-6, 6, 12, 16, new Color(58, 46, 38));   // Palto
        Px(-5, -6, 10, 10, new Color(200, 158, 128)); // Yüz
        Px(-6, -10, 12, 5, new Color(92, 78, 62));  // Şapka
        Px(-5, -1, 10, 4, new Color(190, 190, 198)); // Sakal
        Px(-6, 1, 12, 4, new Color(150, 42, 40));   // Atkı

        // Kazma: vuruşta öne savrulur
        float angle = _facing * (0.9f - _swing * 1.6f);
        var pivot = c + new Vector2(_facing * 8, 6);
        sb.Draw(Assets.Pixel, pivot, null, new Color(120, 90, 60), angle - MathHelper.PiOver2 * _facing, new Vector2(0f, 0.5f),
            new Vector2(22, 3), SpriteEffects.None, 0f);
        var head = pivot + new Vector2(MathF.Cos(angle - MathHelper.PiOver2 * _facing), MathF.Sin(angle - MathHelper.PiOver2 * _facing)) * 22f;
        sb.FillRect(new Rectangle((int)head.X - 6, (int)head.Y - 2, 12, 4), new Color(170, 176, 190));
    }

    private void DrawResult(SpriteBatch sb)
    {
        float a = MathHelper.Clamp(_resultTimer / 0.4f, 0f, 1f);
        var box = new Rectangle(340, 230, 600, 260);
        sb.FillRect(new Rectangle(0, 0, GameConfig.ScreenWidth, GameConfig.ScreenHeight), Color.Black * (0.5f * a));
        UiDraw.FancyPanel(sb, box, Hud.Amber * a);
        GameText.DrawCentered(sb, Loc.T("mine.done"), box.Center.X, box.Y + 22, Hud.Amber * a, 3);

        int y = box.Y + 76;
        int any = 0;
        foreach (var loot in LootCatalog.All)
        {
            int count = Found.FindAll(l => l == loot).Count;
            if (count == 0) continue;
            LootCatalog.DrawIcon(sb, loot, new Point(box.X + 70, y + 10));
            GameText.Draw(sb, $"{LootCatalog.Name(loot)}  X{count}", new Vector2(box.X + 100, y), LootCatalog.Color(loot) * a);
            y += 34;
            any++;
        }
        if (any == 0)
            GameText.DrawCentered(sb, Loc.T("mine.empty"), box.Center.X, y, Hud.Label * a);

        float pulse = 0.6f + 0.4f * MathF.Sin(_time * 4f);
        GameText.DrawCentered(sb, Loc.T("mine.leave"), box.Center.X, box.Bottom - 40, Hud.Value * (a * pulse));
    }
}
