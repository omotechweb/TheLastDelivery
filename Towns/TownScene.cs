using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TheLastDelivery.Core;
using TheLastDelivery.Graphics;
using TheLastDelivery.Items;
using TheLastDelivery.Localization;
using TheLastDelivery.Story;
using TheLastDelivery.UI;
using TheLastDelivery.World;

namespace TheLastDelivery.Towns;

/// <summary>Kasabada 'E' ile etkileşime geçilebilen şeyler. Spot: hikâye inceleme noktası (NarrativeData).</summary>
public enum InteractableKind { Train, Tavern, Store, Npc, Mechanic, Bunker, Peddler, Thief, Spot, Door, Exit }

public sealed record Interactable(InteractableKind Kind, float X, float PromptY)
{
    /// <summary>Spot türünde inceleme noktasının kimliği ("trade.auction", "hq.diary"...).</summary>
    public string SpotId { get; init; }

    /// <summary>Kartta gösterilecek adın çeviri anahtarı (yoksa türün varsayılanı).</summary>
    public string NameKey { get; init; }

    /// <summary>Yalnızca bu hikâye bayrağı varsa görünür (örn. haritayla bulunan zula).</summary>
    public string RequireFlag { get; init; }

    /// <summary>Door türünde girilen iç mekân.</summary>
    public InteriorKind? Interior { get; init; }

    /// <summary>Kapı bir hizmet binasınınsa içerideki tezgâhın hizmeti (taverna, tamirci, sığınak).</summary>
    public InteractableKind? Service { get; init; }
}

/// <summary>
/// Kasaba (yürüyüş) modu: Arthur trenden iner, A/D ile yürür, kamera onu takip eder.
/// Dünya koordinatları ekrandan geniştir (GameConfig.TownWidth); çizim, Game1'in kurduğu
/// kamera dönüşümüyle (−CameraX kaydırma) yapılır. Tren dünyanın solunda park halindedir.
///
/// Her kasaba bir mimari şablona sahiptir (TownLayouts): Ticaret Merkezi, Madenci Köyü, Hayalet İstasyon,
/// Buzul Tarikatı, Terk Edilmiş Karakol, Askeri Sınır Kapısı. Hikâye bölümlerinin keşif ve diyalog
/// sahneleri de aynı yürüyüş modunu kendi şablonlarıyla kullanır.
/// </summary>
public sealed partial class TownScene
{
    private const float FeetY = 594f;                         // Arthur ve NPC'ler rayların önündeki karda yürür
    private const int BaseY = GameConfig.GroundY + 8;         // Binaların oturduğu kar çizgisi
    private const float ArthurStartX = 760f;                  // Lokomotifin hemen önü
    private static readonly float[] DriftXs = { 830f, 1080f, 1400f, 1640f, 1860f, 2200f, 2480f, 2700f, 2940f };

    // Hırsız: trenin arkasından (sol) kargo vagonuna sinsice yaklaşır
    private const float ThiefSpawnX = 30f;
    private const float ThiefTargetX = 290f;
    private enum ThiefState { None, Waiting, Approaching, Stealing, Fleeing, Caught }

    private readonly List<Interactable> _interactables = new();
    private TownLayout _layout;

    private ThiefState _thief;
    private float _thiefX;
    private float _thiefTimer;
    private bool _theftPending;
    private bool _thiefAlert;

    private readonly ParticleSystem _particles = new();
    private readonly Random _rng = new();
    private float _arthurX;
    private int _facing = 1;
    private float _walkTime;
    private bool _moving;
    private float _time;
    private float _sparkTimer;
    private float _smokeTimer;
    private float _promptAlpha;
    private Interactable _shownPrompt;

    public TownInfo Town { get; private set; }
    public int TownKm { get; private set; }
    public float CameraX { get; private set; }

    /// <summary>Arthur'un menzilindeki en yakın etkileşim (yoksa null).</summary>
    public Interactable Nearest { get; private set; }

    /// <summary>Hırsız şu an kargoyu yağmalıyor (0-1 ilerleme), yoksa -1.</summary>
    public float StealProgress => _thief == ThiefState.Stealing ? MathHelper.Clamp(_thiefTimer / GameConfig.ThiefStealTime, 0f, 1f) : -1f;

    /// <summary>Kasabanın mimari şablonu.</summary>
    public TownArchetype Archetype => Town?.Archetype ?? TownArchetype.TradeHub;

    /// <summary>Şablonda bu tür bir hizmet (taverna, tamirci...) var mı?</summary>
    public bool Offers(InteractableKind kind) => _interactables.Any(i => i.Kind == kind);

    /// <summary>Hırsız kargoyu yağmalamayı bitirdi (Game1 bir kez işler).</summary>
    public bool ConsumeTheft()
    {
        bool pending = _theftPending;
        _theftPending = false;
        return pending;
    }

    /// <summary>Hırsız ortaya çıktı (Game1 bir kez uyarı gösterir).</summary>
    public bool ConsumeThiefAlert()
    {
        bool alert = _thiefAlert;
        _thiefAlert = false;
        return alert;
    }

    /// <summary>Arthur hırsızı yakaladı.</summary>
    public void CatchThief() => _thief = ThiefState.Caught;

    /// <summary>Arthur'un yanında yürüyen evcil hayvan. Varsa hırsız havlamayla oyalanır (çalması uzar).</summary>
    public Economy.PetKind Pet { get; set; }

    /// <param name="thiefDelay">Hırsızın ne kadar sonra belireceği (negatifse ya da şablonda hırsız yoksa bu ziyarette hırsız yok).</param>
    public void Enter(TownInfo town, int km, float thiefDelay = -1f)
    {
        Town = town;
        TownKm = km;
        _layout = TownLayouts.For(town.Archetype);
        _outdoor = null;
        Interior = null;
        RefreshInteractables();

        bool thief = thiefDelay >= 0f && _layout.Thief;
        _thief = thief ? ThiefState.Waiting : ThiefState.None;
        _thiefTimer = thiefDelay;
        _thiefX = ThiefSpawnX;
        _theftPending = false;
        _thiefAlert = false;
        _arthurX = ArthurStartX;
        _facing = 1;
        _walkTime = 0f;
        _time = 0f;
        _promptAlpha = 0f;
        _shownPrompt = null;
        CameraX = CameraFor(_arthurX);
        Nearest = null;
        _particles.Clear();
    }

    // ---- İç mekân: Arthur bir binaya girer; dış yerleşim ve konumu saklanır ----
    private TownLayout _outdoor;
    private float _outdoorX;
    private float _interiorTime;

    /// <summary>Şu an içinde bulunulan bina (dışarıdaysa null).</summary>
    public InteriorKind? Interior { get; private set; }
    public bool IsInterior => Interior != null;

    /// <summary>İçerideki hizmet tezgâhı (taverna, tamirci, sığınak) varsa onun türü.</summary>
    public InteractableKind? InteriorService { get; private set; }

    /// <summary>Kapıdan içeri girer: iç mekânın yerleşimi kurulur, Arthur çıkış kapısının yanında başlar.</summary>
    public void EnterInterior(Interactable door)
    {
        if (door?.Interior == null || IsInterior) return;
        _outdoor = _layout;
        _outdoorX = _arthurX;
        Interior = door.Interior;
        InteriorService = door.Service;
        _layout = InteriorCatalog.Build(door.Interior.Value, Town.Archetype, door.Service);
        _arthurX = InteriorCatalog.ExitX + 90f;
        _facing = 1;
        _interiorTime = 0f;
        CameraX = CameraFor(_arthurX);
        _shownPrompt = null;
        _promptAlpha = 0f;
        Nearest = null;
        _particles.Clear();
        RefreshInteractables();
    }

    /// <summary>Binadan çıkar: Arthur kapının önüne döner.</summary>
    public void ExitInterior()
    {
        if (!IsInterior) return;
        _layout = _outdoor;
        _arthurX = _outdoorX;
        _outdoor = null;
        Interior = null;
        InteriorService = null;
        CameraX = CameraFor(_arthurX);
        _shownPrompt = null;
        _promptAlpha = 0f;
        Nearest = null;
        _particles.Clear();
        RefreshInteractables();
    }

    /// <summary>İç mekânın kartta ve başlıkta görünen adı.</summary>
    public string InteriorName => Interior == null ? null : Loc.T(InteriorCatalog.NameKey(Interior.Value, Town.Archetype));

    /// <summary>Bayrağa bağlı inceleme noktalarını (örn. haritayla bulunan zula) yeniden değerlendirir.</summary>
    public void RefreshInteractables()
    {
        if (_layout == null) return; // Henüz hiçbir kasabaya girilmedi (örn. sürüşte görev seçimi)
        _interactables.Clear();
        foreach (var item in _layout.Interactables)
            if (item.RequireFlag == null || Narrative.Has(item.RequireFlag))
                _interactables.Add(item);
    }

    /// <returns>Kameranın bu karede kaydığı piksel (parallax arka planı için).</returns>
    public float Update(float dt, InputState input, bool canMove)
    {
        _time += dt;

        float direction = 0f;
        if (canMove)
        {
            if (input.Down(Keys.A) || input.Down(Keys.Left)) direction -= 1f;
            if (input.Down(Keys.D) || input.Down(Keys.Right)) direction += 1f;
        }

        _moving = direction != 0f;
        if (_moving)
        {
            _facing = direction > 0 ? 1 : -1;
            _arthurX = MathHelper.Clamp(_arthurX + direction * GameConfig.WalkSpeed * dt, 80f, _layout.Width - 80f);
            _walkTime += dt;
        }

        // Kamera Arthur'u yumuşakça takip eder
        float previous = CameraX;
        CameraX = MathHelper.Lerp(CameraX, CameraFor(_arthurX), MathHelper.Clamp(dt * 6f, 0f, 1f));

        if (!IsInterior) UpdateThief(dt);
        _interiorTime += dt;

        Nearest = null;
        float best = GameConfig.InteractRange;
        foreach (var item in _interactables)
        {
            float distance = MathF.Abs(item.X - _arthurX);
            if (distance < best)
            {
                best = distance;
                Nearest = item;
            }
        }

        // Hırsız yakındaysa her şeyden önce gelir
        if (!IsInterior && _thief is ThiefState.Approaching or ThiefState.Stealing && MathF.Abs(_thiefX - _arthurX) < GameConfig.InteractRange + 20f)
            Nearest = new Interactable(InteractableKind.Thief, _thiefX, 420f);

        // Etkileşim kartı yumuşakça belirir/kaybolur; hedef değişince yeniden açılır (hırsız her karede yeni kayıt)
        if (Nearest?.Kind == InteractableKind.Thief && _shownPrompt?.Kind == InteractableKind.Thief)
            _shownPrompt = Nearest;
        if (Nearest != _shownPrompt)
        {
            _promptAlpha = MathF.Max(0f, _promptAlpha - dt * 8f);
            if (_promptAlpha <= 0f) _shownPrompt = Nearest;
        }
        else
        {
            _promptAlpha = MathHelper.Clamp(_promptAlpha + (Nearest != null ? dt * 6f : -dt * 6f), 0f, 1f);
        }

        UpdateEffects(dt);
        return CameraX - previous;
    }

    /// <summary>
    /// Hırsız: bir süre sonra trenin arkasından belirir, kargo vagonuna yürür ve yağmalamaya başlar.
    /// Arthur yetişip [E] ile yakalamazsa yağmayı alıp kaçar. Diyalog açıkken de ilerler (dalgınlık bedeli).
    /// </summary>
    private void UpdateThief(float dt)
    {
        switch (_thief)
        {
            case ThiefState.Waiting:
                _thiefTimer -= dt;
                if (_thiefTimer <= 0f)
                {
                    _thief = ThiefState.Approaching;
                    _thiefAlert = true;
                }
                break;
            case ThiefState.Approaching:
                _thiefX += GameConfig.ThiefWalkSpeed * dt;
                if (_thiefX >= ThiefTargetX)
                {
                    _thiefX = ThiefTargetX;
                    _thief = ThiefState.Stealing;
                    _thiefTimer = 0f;
                }
                break;
            case ThiefState.Stealing:
                // Havlayan evcil hayvan hırsızı ürkütür: çalmak %60 daha uzun sürer
                _thiefTimer += dt / (Pet != Economy.PetKind.None ? 1.6f : 1f);
                if (_thiefTimer >= GameConfig.ThiefStealTime)
                {
                    _thief = ThiefState.Fleeing;
                    _theftPending = true;
                }
                break;
            case ThiefState.Fleeing:
            case ThiefState.Caught:
                _thiefX -= GameConfig.ThiefWalkSpeed * 2.4f * dt; // Yakalanan da kaçan da koşarak uzaklaşır
                if (_thiefX < -80f) _thief = ThiefState.None;
                break;
        }
    }

    private float CameraFor(float x) =>
        MathHelper.Clamp(x - GameConfig.ScreenWidth / 2f, 0f, MathF.Max(0f, (_layout?.Width ?? GameConfig.TownWidth) - GameConfig.ScreenWidth));

    private void UpdateEffects(float dt)
    {
        // Tamircinin örsünden kıvılcım
        _sparkTimer -= dt;
        if (_sparkTimer <= 0f)
        {
            _sparkTimer = 0.9f + (float)_rng.NextDouble();
            foreach (var p in _layout.Structures)
            {
                if (p.Kind != StructureKind.Mechanic) continue;
                for (int i = 0; i < 10; i++)
                {
                    _particles.Emit(new Particle
                    {
                        Position = new Vector2(p.X + 100, BaseY - 34),
                        Velocity = new Vector2(_rng.Next(-90, 91), -_rng.Next(60, 180)),
                        Life = 0.4f + (float)_rng.NextDouble() * 0.3f,
                        Size = 1f,
                        Color = new Color(255, 190, 90),
                        Alpha = 1f,
                        Gravity = 420f,
                        Additive = true
                    });
                }
            }
        }

        // Bacalardan duman, sığınağın havalandırmasından buhar, tapınaktan buz kristalleri
        _smokeTimer -= dt;
        if (_smokeTimer <= 0f)
        {
            _smokeTimer = 0.3f;
            foreach (var (x, y, steam) in Chimneys())
            {
                _particles.Emit(new Particle
                {
                    Position = new Vector2(x + _rng.Next(-3, 4), y),
                    Velocity = new Vector2(_rng.Next(-8, 9), -_rng.Next(20, 44)),
                    Life = steam ? 1.6f : 3.2f,
                    Size = steam ? 4f : 6f,
                    Growth = 3.2f,
                    Color = steam ? new Color(210, 220, 235) : new Color(70, 72, 82),
                    Alpha = steam ? 0.3f : 0.35f,
                    WindInfluence = 0.4f,
                    Soft = true
                });
            }
        }

        _particles.Update(dt, -25f);
    }

    /// <summary>Duman ya da buhar çıkan noktaların dünya konumları (yıkık yapılardan duman çıkmaz).</summary>
    private IEnumerable<(float X, float Y, bool Steam)> Chimneys()
    {
        foreach (var p in _layout.Structures)
        {
            if (p.Variant == 9) continue;
            switch (p.Kind)
            {
                case StructureKind.Tavern: yield return (p.X + 89, BaseY - 234, false); break;
                case StructureKind.Store: yield return (p.X - 80, BaseY - 176, false); break;
                case StructureKind.Mechanic: yield return (p.X - 100, BaseY - 214, false); break;
                case StructureKind.House: yield return (p.X + 29, BaseY - 166, false); break;
                case StructureKind.Bunker: yield return (p.X + 120, BaseY - 132, true); break;
                case StructureKind.Barracks: yield return (p.X + 90, BaseY - 120, false); break;
                case StructureKind.Hut: yield return (p.X + 30, BaseY - 120, false); break;
                case StructureKind.Headframe: yield return (p.X + 70, BaseY - 140, true); break;
            }
        }
    }

    // ------------------------------------------------------------------
    // Çizim (kamera dönüşümü altında, dünya koordinatlarında)
    // ------------------------------------------------------------------

    public void DrawWorld(SpriteBatch sb, Train train)
    {
        if (IsInterior)
        {
            DrawInteriorWorld(sb);
            return;
        }

        int left = (int)CameraX - 20;
        int width = GameConfig.ScreenWidth + 40;

        DrawDistantRow(sb);

        // Yapılar rayların arkasında
        foreach (var p in _layout.Structures)
            DrawStructure(sb, p);
        foreach (float x in _layout.Lamps)
            DrawLampPost(sb, x);
        foreach (float x in DriftXs)
            DrawSnowDrift(sb, x, BaseY, 1f);

        HighlightNearest(sb);

        // Ray (kasaba boyunca)
        sb.FillRect(new Rectangle(left, GameConfig.RailY, width, 4), new Color(95, 100, 115));
        sb.FillRect(new Rectangle(left, GameConfig.RailY, width, 1), new Color(165, 172, 190));

        // Park halindeki tren
        train.DrawSmoke(sb);
        train.Draw(sb);
        train.DrawLights(sb);

        _particles.Draw(sb);

        // İnsanlar rayların önünde
        for (int i = 0; i < _layout.People.Length; i++)
        {
            var person = _layout.People[i];
            DrawPerson(sb, person.X, person.Facing, person.Coat, person.Scarf, MathF.Sin(_time * 2f + i) * 1.5f, 0f, false);
        }
        foreach (var p in _layout.Structures)
            if (p.Kind == StructureKind.Cage) DrawCageBars(sb, p.X); // Mahkûm demirlerin arkasında
        if (_layout.PeddlerX > 0f) DrawPeddler(sb, _layout.PeddlerX);
        DrawThief(sb);
        DrawArthur(sb);

        // Ön plandaki kar yığıntıları
        foreach (float x in DriftXs)
            DrawSnowDrift(sb, x + 140, 640, 1.4f);
    }

    /// <summary>Ekran koordinatlarında: kasvet perdesi, kasaba adı, etkileşim kartı ve kontroller.</summary>
    public void DrawInterface(SpriteBatch sb, float timeInTown, bool showPrompt)
    {
        if (_layout.Gloom > 0f)
            sb.FillRect(new Rectangle(0, 0, GameConfig.ScreenWidth, GameConfig.ScreenHeight), new Color(4, 6, 14) * _layout.Gloom);

        // Binaya girince adı kısaca görünür
        if (IsInterior && _interiorTime < 2.8f)
        {
            float alpha = MathHelper.Clamp(_interiorTime / 0.4f, 0f, 1f) * MathHelper.Clamp((2.8f - _interiorTime) / 0.6f, 0f, 1f);
            GameText.DrawCentered(sb, InteriorName, GameConfig.ScreenWidth / 2f + 3, 183, Color.Black * (0.6f * alpha), 4);
            GameText.DrawCentered(sb, InteriorName, GameConfig.ScreenWidth / 2f, 180, Hud.Amber * alpha, 4);
            GameText.DrawCentered(sb, Town.Name, GameConfig.ScreenWidth / 2f, 236, Hud.Label * alpha, 2);
        }

        // Varışta kasaba adı birkaç saniye görünür
        if (timeInTown < 4f && !IsInterior)
        {
            float alpha = MathHelper.Clamp(timeInTown / 0.5f, 0f, 1f) * MathHelper.Clamp((4f - timeInTown) / 0.8f, 0f, 1f);
            GameText.DrawCentered(sb, Town.Name, GameConfig.ScreenWidth / 2f + 3, 253, Color.Black * (0.6f * alpha), 4);
            GameText.DrawCentered(sb, Town.Name, GameConfig.ScreenWidth / 2f, 250, Hud.Amber * alpha, 4);
            string subtitle = Town.IsStoryScene ? Loc.T("scene.subtitle") : Loc.F("town.subtitle", TownKm) + "  -  " + Loc.T($"arch.{Archetype}");
            GameText.DrawCentered(sb, subtitle, GameConfig.ScreenWidth / 2f, 306, Hud.Label * alpha, 2);
        }

        if (showPrompt && _shownPrompt != null && _promptAlpha > 0f)
            DrawPromptCard(sb, _shownPrompt);

        // Hırsız ekran dışındaysa solda yanıp sönen uyarı oku ve yağma ilerlemesi
        if (!IsInterior && _thief is ThiefState.Approaching or ThiefState.Stealing)
        {
            float screenX = _thiefX - CameraX;
            float pulse = 0.5f + 0.5f * MathF.Sin(_time * 10f);
            if (screenX < 0f)
            {
                sb.FillRect(new Rectangle(14, 470, 10, 30), Hud.Danger * pulse);
                sb.FillRect(new Rectangle(6, 478, 8, 14), Hud.Danger * pulse);
                GameText.Draw(sb, Loc.T("town.thief"), new Vector2(32, 474), Hud.Danger * pulse);
            }
            if (StealProgress >= 0f)
            {
                var bar = new Rectangle((int)MathHelper.Clamp(screenX - 40, 10, GameConfig.ScreenWidth - 90), 520, 80, 8);
                sb.FillRect(bar, Color.Black * 0.7f);
                sb.FillRect(new Rectangle(bar.X, bar.Y, (int)(bar.Width * StealProgress), bar.Height), Hud.Danger);
            }
        }

        GameText.Draw(sb, Loc.T(IsInterior ? "int.hint" : Town.IsStoryScene ? "scene.hint" : "town.hint"),
            new Vector2(24, GameConfig.ScreenHeight - 32), Hud.Label * 0.7f);
    }

    /// <summary>
    /// Etkileşim kartı: üstte hedefin adı, altta basılabilir [E] tuşu ve eylem ("İÇERİ GİR", "KONUŞ", "İNCELE").
    /// Hafifçe büyüyerek belirir, hedefi gösteren okla hafifçe süzülür.
    /// </summary>
    private void DrawPromptCard(SpriteBatch sb, Interactable target)
    {
        var spot = target.Kind == InteractableKind.Spot ? NarrativeData.Spot(target.SpotId) : null;
        string name = target.NameKey != null ? Loc.T(target.NameKey) : target.Kind switch
        {
            InteractableKind.Train => Town.IsStoryScene ? Loc.T("scene.train") : "OLD SMOKIE",
            InteractableKind.Tavern => Loc.T("sign.tavern"),
            InteractableKind.Store => Loc.T("sign.store"),
            InteractableKind.Mechanic => Loc.T("sign.mechanic"),
            InteractableKind.Bunker => Loc.T("sign.bunker"),
            InteractableKind.Peddler => Loc.T("sign.peddler"),
            InteractableKind.Thief => Loc.T("town.thief"),
            InteractableKind.Spot => spot?.Name ?? target.SpotId,
            InteractableKind.Door => Loc.T(InteriorCatalog.NameKey(target.Interior ?? InteriorKind.Tavern, Town.Archetype)),
            InteractableKind.Exit => Loc.T("int.exit"),
            _ => Town.NpcName
        };
        string action = Loc.T(target.Kind switch
        {
            InteractableKind.Exit => "int.action.exit",
            InteractableKind.Tavern or InteractableKind.Mechanic or InteractableKind.Bunker when IsInterior => "town.action.talk",
            InteractableKind.Train => Town.IsStoryScene ? "scene.action.leave" : "town.action.board",
            InteractableKind.Npc or InteractableKind.Peddler => "town.action.talk",
            InteractableKind.Thief => "town.action.catch",
            InteractableKind.Spot => spot?.Talk == true ? "town.action.talk" : "town.action.inspect",
            _ => "town.action.enter"
        });

        // Henüz okunmamış bir şey varsa kartın köşesinde küçük bir işaret
        bool fresh = spot != null && !Narrative.Seen(Narrative.NextEntry(spot));

        float eased = 1f - MathF.Pow(1f - _promptAlpha, 3f);
        float bob = MathF.Sin(_time * 4f) * 3f;
        float actionWidth = GameText.MeasureLine(action);
        float width = MathF.Max(GameText.MeasureLine(name), actionWidth + 52) + 36;
        float screenX = target.X - CameraX;

        var card = new Rectangle((int)(screenX - width / 2f), (int)(target.PromptY + bob + (1f - eased) * 12f), (int)width, 72);
        sb.FillRect(new Rectangle(card.X + 4, card.Y + 4, card.Width, card.Height), Color.Black * (0.45f * eased));
        UiDraw.FancyPanel(sb, card, Hud.Amber * eased);
        if (fresh)
            sb.FillRect(new Rectangle(card.Right - 12, card.Y + 6, 6, 6), new Color(140, 220, 255) * (eased * (0.6f + 0.4f * MathF.Sin(_time * 6f))));

        GameText.DrawCentered(sb, name, screenX, card.Y + 10, Hud.Amber * eased);

        // [E] tuşu + eylem
        float rowX = screenX - (actionWidth + 52) / 2f;
        float pulse = 0.5f + 0.5f * MathF.Sin(_time * 6f);
        UiDraw.KeyCap(sb, new Rectangle((int)rowX, card.Y + 36, 34, 28), "E", Hud.Amber * eased, pulse);
        GameText.Draw(sb, action, new Vector2(rowX + 48, card.Y + 40), Color.White * eased);

        // Hedefi gösteren ok
        sb.FillRect(new Rectangle((int)screenX - 6, card.Bottom, 12, 4), Hud.Amber * eased);
        sb.FillRect(new Rectangle((int)screenX - 3, card.Bottom + 4, 6, 4), Hud.Amber * eased);
    }

    /// <summary>Yakındaki yapının etrafında hafif sıcak bir hale (hangi binaya girileceği belli olsun).</summary>
    private void HighlightNearest(SpriteBatch sb)
    {
        if (_shownPrompt == null || _promptAlpha <= 0f || _shownPrompt.Kind is InteractableKind.Npc or InteractableKind.Train or InteractableKind.Thief)
            return;
        sb.DrawGlow(new Vector2(_shownPrompt.X, BaseY - 60), 170f, Town.Accent, 0.12f * _promptAlpha);
    }

    // ------------------------------------------------------------------
    // Karakterler
    // ------------------------------------------------------------------

    /// <summary>Seyyar satıcı: tekerlekli el arabası, bidonlar, parça sandığı ve fener.</summary>
    private void DrawPeddler(SpriteBatch sb, float peddlerX)
    {
        int y = (int)FeetY;
        float cx = peddlerX + 40;
        Box(sb, cx, -50, y - 44, 100, 30, new Color(96, 66, 40));                 // Araba kasası
        Box(sb, cx, -50, y - 44, 100, 4, new Color(130, 92, 56));
        sb.Draw(Assets.Circle, new Rectangle((int)cx - 40, y - 20, 20, 20), new Color(40, 34, 30));
        sb.Draw(Assets.Circle, new Rectangle((int)cx + 20, y - 20, 20, 20), new Color(40, 34, 30));
        Box(sb, cx, -44, y - 66, 18, 22, new Color(200, 140, 60));                // Yakıt bidonları
        Box(sb, cx, -22, y - 62, 18, 18, new Color(170, 120, 50));
        Box(sb, cx, 4, y - 60, 30, 16, new Color(110, 116, 130));                 // Parça sandığı
        InventoryPanel.DrawIcon(sb, ItemType.SpareParts, new Point((int)cx + 19, y - 70));
        Box(sb, cx, 46, y - 90, 3, 46, new Color(40, 36, 32));                    // Fener direği
        float flicker = 0.8f + 0.2f * MathF.Sin(_time * 8f);
        Box(sb, cx, 42, y - 98, 10, 10, new Color(255, 210, 120) * flicker);
        sb.DrawGlow(new Vector2(cx + 47, y - 93), 60f, new Color(255, 190, 100), 0.35f * flicker);

        DrawPerson(sb, peddlerX - 30, 1, new Color(110, 74, 40), new Color(200, 160, 50), MathF.Sin(_time * 2.4f) * 1.2f, 0f, false);
    }

    /// <summary>Kapüşonlu hırsız ve sırtındaki çuval; yağmalarken eğilir, kaçarken koşar.</summary>
    private void DrawThief(SpriteBatch sb)
    {
        if (_thief is ThiefState.None or ThiefState.Waiting) return;

        bool running = _thief is ThiefState.Approaching or ThiefState.Fleeing or ThiefState.Caught;
        float speed = _thief == ThiefState.Approaching ? 9f : 16f;
        float stride = running ? MathF.Sin(_time * speed) : 0f;
        float bob = _thief == ThiefState.Stealing ? 6f + MathF.Sin(_time * 12f) * 2f : running ? -MathF.Abs(stride) * 2f : 0f;
        int facing = _thief == ThiefState.Approaching || _thief == ThiefState.Stealing ? 1 : -1;
        DrawPerson(sb, _thiefX, facing, new Color(26, 24, 30), new Color(20, 20, 24), bob, stride, false);

        // Çuval ve parlayan gözler
        int sackX = facing > 0 ? (int)_thiefX - 20 : (int)_thiefX + 8;
        bool full = _thief is ThiefState.Fleeing;
        sb.FillRect(new Rectangle(sackX, (int)(FeetY + bob) - 42, full ? 16 : 11, full ? 18 : 12), new Color(110, 96, 70));
        int eyeX = facing > 0 ? (int)_thiefX + 1 : (int)_thiefX - 3;
        sb.FillRect(new Rectangle(eyeX, (int)(FeetY + bob) - 53, 2, 2), new Color(255, 60, 40));
    }

    private void DrawArthur(SpriteBatch sb)
    {
        float stride = _moving ? MathF.Sin(_walkTime * 11f) : 0f;
        float bob = _moving ? MathF.Abs(MathF.Sin(_walkTime * 11f)) * -2f : MathF.Sin(_time * 1.6f) * 0.8f;
        // Kostüm (kozmetik paket): palto ve atkı renkleri
        var costume = Platform.DLCManager.Active(Economy.CosmeticSlot.Costume);
        DrawPerson(sb, _arthurX, _facing, costume.Primary, costume.Secondary, bob, stride, true);

        // Evcil hayvan Arthur'un arkasından gelir
        UI.CabinDecor.DrawPet(sb, new Point((int)(_arthurX - _facing * 26), (int)FeetY), Pet, 2, _time, 1f, _facing);
    }

    /// <summary>
    /// Bloklarla çizilmiş insan: çizme, uzun palto, atkı, kürk şapka, sakal.
    /// Arthur elinde fener taşır; adım atarken bacaklar ve kol sallanır.
    /// </summary>
    private static void DrawPerson(SpriteBatch sb, float x, int facing, Color coat, Color scarf, float bob, float stride, bool isArthur)
    {
        int baseX = (int)x;
        int feet = (int)(FeetY + bob);

        // Yöne göre aynalanan dikdörtgen
        void Px(int dx, int dy, int w, int h, Color c)
        {
            int px = facing >= 0 ? baseX + dx : baseX - dx - w;
            sb.FillRect(new Rectangle(px, feet + dy, w, h), c);
        }

        var boot = new Color(28, 24, 22);
        var skin = new Color(200, 158, 128);
        var beard = new Color(190, 190, 198);
        var fur = new Color(92, 78, 62);

        int legA = (int)(stride * 4f);
        int legB = -legA;

        // Gölge
        sb.FillRect(new Rectangle(baseX - 12, (int)FeetY - 1, 24, 3), Color.Black * 0.3f);

        // Bacaklar ve çizmeler
        Px(-6 + legA, -16, 5, 13, new Color(40, 36, 40));
        Px(1 + legB, -16, 5, 13, new Color(40, 36, 40));
        Px(-7 + legA, -4, 7, 4, boot);
        Px(0 + legB, -4, 7, 4, boot);

        // Uzun palto
        Px(-9, -44, 18, 30, coat);
        Px(-10, -18, 20, 4, UiDraw.Shade(coat, 0.85f));
        Px(-1, -40, 2, 24, UiDraw.Shade(coat, 0.7f));   // Önden kapanış çizgisi

        // Kol (adımla sallanır)
        Px(6 + (int)(stride * 2f), -40, 5, 17, UiDraw.Shade(coat, 0.9f));

        // Atkı, baş, sakal, kürk şapka
        Px(-8, -46, 16, 5, scarf);
        Px(-10, -44, 4, 10, scarf);        // Arkaya savrulan atkı ucu
        Px(-5, -56, 10, 10, skin);
        Px(2, -53, 2, 2, new Color(30, 30, 34)); // Göz
        Px(-5, -49, 10, 4, beard);
        Px(-7, -63, 14, 8, fur);
        Px(-8, -57, 3, 6, fur);            // Kulaklık

        if (isArthur)
        {
            // Elinde fener
            int lanternX = facing >= 0 ? baseX + 12 + (int)(stride * 2f) : baseX - 18 - (int)(stride * 2f);
            sb.FillRect(new Rectangle(lanternX, feet - 26, 6, 8), new Color(255, 200, 110));
            sb.FillRect(new Rectangle(lanternX + 1, feet - 28, 4, 2), new Color(60, 56, 50));
            sb.DrawGlow(new Vector2(lanternX + 3, feet - 22), 46f, new Color(255, 180, 100), 0.45f);
        }
    }

    // ------------------------------------------------------------------
    // Yapılar
    // ------------------------------------------------------------------

    private static readonly Color Snow = new(178, 188, 208);
    private static readonly Color DarkWindow = new(20, 22, 30);

    private static void Box(SpriteBatch sb, float cx, int x, int y, int w, int h, Color c) =>
        sb.FillRect(new Rectangle((int)cx + x, y, w, h), c);

    private void DrawStructure(SpriteBatch sb, Placement p)
    {
        float cx = p.X;
        int v = p.Variant;
        switch (p.Kind)
        {
            case StructureKind.House: DrawHouse(sb, cx, v); break;
            case StructureKind.Tavern: DrawTavern(sb, cx, v); break;
            case StructureKind.Store: DrawStore(sb, cx, v); break;
            case StructureKind.Mechanic: DrawMechanic(sb, cx, v); break;
            case StructureKind.Bunker: DrawBunker(sb, cx); break;
            case StructureKind.Stall: DrawStall(sb, cx, v); break;
            case StructureKind.NoticeBoard: DrawNoticeBoard(sb, cx, v); break;
            case StructureKind.Headframe: DrawHeadframe(sb, cx); break;
            case StructureKind.MineCart: DrawMineCart(sb, cx); break;
            case StructureKind.Platform: DrawPlatform(sb, cx); break;
            case StructureKind.Loudspeaker: DrawLoudspeaker(sb, cx); break;
            case StructureKind.Gramophone: DrawGramophone(sb, cx); break;
            case StructureKind.TicketBooth: DrawTicketBooth(sb, cx); break;
            case StructureKind.Shrine: DrawShrine(sb, cx); break;
            case StructureKind.Totem: DrawTotem(sb, cx); break;
            case StructureKind.Tent: DrawTent(sb, cx, v); break;
            case StructureKind.Ruin: DrawRuin(sb, cx, v); break;
            case StructureKind.Barracks: DrawBarracks(sb, cx, v); break;
            case StructureKind.Watchtower: DrawWatchtower(sb, cx, v); break;
            case StructureKind.Barrier: DrawBarrier(sb, cx); break;
            case StructureKind.Cage: DrawCageBack(sb, cx); break;
            case StructureKind.Crate: DrawCrates(sb, cx, v); break;
            case StructureKind.Fence: DrawFence(sb, cx); break;
            case StructureKind.Mailbox: DrawMailbox(sb, cx); break;
            case StructureKind.Swing: DrawSwing(sb, cx); break;
            case StructureKind.Shed: DrawShed(sb, cx); break;
            case StructureKind.Headquarters: DrawHeadquarters(sb, cx); break;
            case StructureKind.RadioMast: DrawRadioMast(sb, cx, v); break;
            case StructureKind.Hut: DrawHut(sb, cx, v); break;
            case StructureKind.LabDome: DrawLabDome(sb, cx); break;
            case StructureKind.AuroraTower: DrawAuroraTower(sb, cx); break;
            case StructureKind.Terminal: DrawTerminal(sb, cx); break;
            case StructureKind.CoreCradle: DrawCoreCradle(sb, cx); break;
            case StructureKind.ShelterWall: DrawShelterWall(sb, cx, v); break;
            case StructureKind.ArmoredCar: DrawArmoredCar(sb, cx); break;
            case StructureKind.Searchlight: DrawSearchlight(sb, cx); break;
            case StructureKind.DroneTurret: DrawDroneTurret(sb, cx, v); break;
            case StructureKind.SleepTubes: DrawSleepTubes(sb, cx, v); break;
            case StructureKind.ServerRack: DrawServerRack(sb, cx, v); break;
            case StructureKind.ChromeSpire: DrawChromeSpire(sb, cx, v); break;
            case StructureKind.AuraScreen: DrawAuraScreen(sb, cx, v); break;
            case StructureKind.SignalTowerBig: DrawSignalTower(sb, cx, v); break;
            case StructureKind.SentinelWreck: DrawSentinelWreck(sb, cx); break;
            case StructureKind.BlastDoor: DrawBlastDoor(sb, cx); break;
            case StructureKind.Elevator: DrawElevator(sb, cx); break;
            case StructureKind.SatelliteDish: DrawSatelliteDish(sb, cx); break;
            default: DrawDeepStructure(sb, p); break; // Derin anlatı: donmuş insanlar, vagonlar, iç mekân eşyaları
        }
    }

    // ---- Episode 2: AURA'nın makine dünyası ----

    private static readonly Color AuraRed = new(255, 50, 60);
    private static readonly Color NullGreen = new(80, 255, 200);

    /// <summary>Otomatik taret: dönen namlu, kızıl tarama ışığı. Variant 1: devre dışı (sönük).</summary>
    private void DrawDroneTurret(SpriteBatch sb, float cx, int variant)
    {
        var steel = new Color(60, 66, 76);
        Box(sb, cx, -40, BaseY - 60, 80, 60, new Color(46, 50, 58));
        Box(sb, cx, -46, BaseY - 66, 92, 8, steel);
        Box(sb, cx, -30, BaseY - 110, 60, 46, steel);
        Box(sb, cx, -30, BaseY - 114, 60, 4, Snow);
        bool active = variant != 1;
        float sweep = active ? MathF.Sin(_time * 0.9f + cx) : -0.3f;
        int barrel = (int)(sweep * 18f);
        Box(sb, cx, 20, BaseY - 96 + barrel / 3, 70, 8, new Color(36, 40, 46));
        Box(sb, cx, 84, BaseY - 98 + barrel / 3, 8, 12, new Color(26, 28, 32));
        var eye = active ? AuraRed * (0.6f + 0.4f * MathF.Sin(_time * 4f)) : new Color(60, 30, 30);
        Box(sb, cx, -8, BaseY - 98, 16, 10, eye);
        if (active)
        {
            sb.DrawGlow(new Vector2(cx, BaseY - 93), 40f, AuraRed, 0.35f);
            var beam = new Color(255, 40, 50, 0) * 0.08f;
            sb.Draw(Assets.LightCone, new Vector2(cx + 8, BaseY - 93), null, beam, 0.35f + sweep * 0.3f,
                new Vector2(0f, Assets.LightCone.Height / 2f), new Vector2(0.7f, 0.4f), SpriteEffects.None, 0f);
        }
    }

    /// <summary>Kış uykusu tüpleri: buzlu camın ardında soluk yüzler, mavi sıvı ışığı.</summary>
    private void DrawSleepTubes(SpriteBatch sb, float cx, int variant)
    {
        var frame = new Color(80, 90, 104);
        Box(sb, cx, -170, BaseY - 24, 340, 24, new Color(50, 56, 66));
        for (int i = 0; i < 5; i++)
        {
            int x = -160 + i * 66;
            int top = BaseY - 170 + (i + variant) % 2 * 6;
            Box(sb, cx, x, top, 54, BaseY - 24 - top, frame);
            float pulse = 0.5f + 0.2f * MathF.Sin(_time * 1.3f + i);
            Box(sb, cx, x + 6, top + 10, 42, BaseY - 44 - top, new Color(70, 150, 200) * pulse);
            Box(sb, cx, x + 18, top + 30, 18, 18, new Color(190, 200, 210) * 0.5f);   // Soluk yüz
            Box(sb, cx, x + 20, top + 37, 4, 2, new Color(40, 60, 80) * 0.6f);
            Box(sb, cx, x + 30, top + 37, 4, 2, new Color(40, 60, 80) * 0.6f);
            Box(sb, cx, x + 6, top + 10, 42, 6, Color.White * 0.35f);                  // Buzlu cam
            Box(sb, cx, x + 22, top - 10, 10, 10, new Color(40, 44, 52));              // Boru
            sb.DrawGlow(new Vector2(cx + x + 27, top + 60), 40f, new Color(80, 170, 230), 0.15f);
        }
        Box(sb, cx, -176, BaseY - 184, 352, 8, new Color(36, 40, 48));
        Box(sb, cx, -176, BaseY - 188, 352, 4, Snow);
    }

    /// <summary>Sunucu rafı: yanıp sönen ışık sütunları. Variant 1: NULL'un yeşil kodu akar.</summary>
    private void DrawServerRack(SpriteBatch sb, float cx, int variant)
    {
        var body = new Color(26, 28, 34);
        Box(sb, cx, -70, BaseY - 200, 140, 200, body);
        Box(sb, cx, -70, BaseY - 200, 140, 4, new Color(60, 66, 76));
        var light = variant == 1 ? NullGreen : new Color(90, 200, 255);
        for (int row = 0; row < 12; row++)
        {
            int y = BaseY - 190 + row * 15;
            Box(sb, cx, -62, y, 124, 11, new Color(36, 40, 48));
            for (int k = 0; k < 6; k++)
            {
                bool on = MathF.Sin(_time * (3f + k) + row * 1.7f + cx) > 0.2f;
                Box(sb, cx, -56 + k * 8, y + 4, 4, 3, (k == 5 ? AuraRed : light) * (on ? 0.9f : 0.15f));
            }
        }
        if (variant == 1)
            for (int c = 0; c < 4; c++)
            {
                int fall = (int)((_time * 60f + c * 47) % 190);
                Box(sb, cx, 20 + c * 10, BaseY - 196 + fall, 3, 8, NullGreen * 0.7f);
            }
        sb.DrawGlow(new Vector2(cx, BaseY - 100), 110f, light, 0.12f);
    }

    /// <summary>Krom kule: parlak, ince, tepesinde dönen yeşil halka.</summary>
    private void DrawChromeSpire(SpriteBatch sb, float cx, int variant)
    {
        int height = variant == 1 ? 240 : 300;
        for (int i = 0; i < 8; i++)
        {
            int w = 60 - i * 6;
            int segment = height / 8;
            var c = UiDraw.Shade(new Color(170, 180, 196), 1f - i * 0.05f);
            Box(sb, cx, -w / 2, BaseY - (i + 1) * segment, w, segment, c);
            Box(sb, cx, -w / 2, BaseY - (i + 1) * segment, 3, segment, Color.White * 0.5f);
        }
        float spin = _time * 2f;
        for (int k = 0; k < 8; k++)
        {
            float a = spin + k * MathHelper.TwoPi / 8f;
            Box(sb, cx, (int)(MathF.Cos(a) * 26f) - 2, BaseY - height - 14 + (int)(MathF.Sin(a) * 6f), 4, 4, NullGreen);
        }
        sb.DrawGlow(new Vector2(cx, BaseY - height - 12), 50f, NullGreen, 0.4f);
    }

    /// <summary>Dev ekran: AURA'nın kızıl gözü ve kayan parazit çizgileri. Variant 1: NULL çerçevesi.</summary>
    private void DrawAuraScreen(SpriteBatch sb, float cx, int variant)
    {
        Box(sb, cx, -6, BaseY - 120, 12, 120, new Color(40, 44, 52));
        var screen = new Rectangle((int)cx - 110, BaseY - 260, 220, 140);
        sb.FillRect(new Rectangle(screen.X - 8, screen.Y - 8, screen.Width + 16, screen.Height + 16), variant == 1 ? new Color(60, 70, 80) : new Color(30, 32, 38));
        sb.FillRect(screen, new Color(8, 4, 8));
        float pulse = 0.6f + 0.4f * MathF.Sin(_time * 2.4f);
        sb.Draw(Assets.Circle, new Rectangle(screen.Center.X - 40, screen.Center.Y - 40, 80, 80), new Color(80, 10, 16));
        sb.Draw(Assets.Circle, new Rectangle(screen.Center.X - 24, screen.Center.Y - 24, 48, 48), AuraRed * pulse);
        sb.Draw(Assets.Circle, new Rectangle(screen.Center.X - 8, screen.Center.Y - 8, 16, 16), Color.White * pulse);
        int scan = (int)((_time * 80f) % screen.Height);
        sb.FillRect(new Rectangle(screen.X, screen.Y + scan, screen.Width, 2), AuraRed * 0.3f);
        if (MathF.Sin(_time * 7f + cx) > 0.92f) // Ara sıra parazit
            for (int g = 0; g < 4; g++)
                sb.FillRect(new Rectangle(screen.X + (g * 53) % screen.Width, screen.Y + (g * 37) % screen.Height, 40, 4), Color.White * 0.4f);
        sb.DrawGlow(new Vector2(screen.Center.X, screen.Center.Y), 160f, AuraRed, 0.18f * pulse);
    }

    /// <summary>AURA'nın frekans kulesi: kafes gövde, tepede kızıl göz ve yayılan halkalar. Variant 2: çekirdek (dev).</summary>
    private void DrawSignalTower(SpriteBatch sb, float cx, int variant)
    {
        int height = variant == 2 ? 420 : variant == 1 ? 300 : 380;
        var steel = new Color(50, 54, 62);
        Box(sb, cx, -50, BaseY - height, 8, height, steel);
        Box(sb, cx, 42, BaseY - height, 8, height, steel);
        for (int y = BaseY - height; y < BaseY; y += 26)
        {
            Box(sb, cx, -50, y, 100, 3, steel);
            Box(sb, cx, -44 + (y / 26 % 2) * 46, y + 4, 40, 3, UiDraw.Shade(steel, 0.8f));
        }
        var eye = new Rectangle((int)cx - 30, BaseY - height - 50, 60, 60);
        sb.Draw(Assets.Circle, eye, new Color(40, 6, 10));
        float pulse = 0.6f + 0.4f * MathF.Sin(_time * 3f);
        sb.Draw(Assets.Circle, new Rectangle(eye.Center.X - 16, eye.Center.Y - 16, 32, 32), AuraRed * pulse);
        sb.DrawGlow(new Vector2(eye.Center.X, eye.Center.Y), 80f, AuraRed, 0.4f * pulse);
        for (int i = 1; i <= 3; i++)
        {
            float wave = (_time * 0.7f + i * 0.33f) % 1f;
            sb.DrawGlow(new Vector2(eye.Center.X, eye.Center.Y), 50f + wave * 220f, AuraRed, 0.1f * (1f - wave));
        }
    }

    /// <summary>Bölüm 1 pususundan kalan Sentinel enkazı: makinisti olmayan siyah zırh, sönük taret.</summary>
    private static void DrawSentinelWreck(SpriteBatch sb, float cx)
    {
        var armor = new Color(20, 20, 24);
        Box(sb, cx, -180, BaseY - 90, 340, 70, armor);
        Box(sb, cx, -190, BaseY - 100, 40, 80, new Color(30, 30, 36));
        for (int x = -170; x < 150; x += 40) Box(sb, cx, x, BaseY - 86, 30, 4, new Color(60, 60, 70));
        Box(sb, cx, -40, BaseY - 130, 70, 40, new Color(28, 28, 34));               // Devrik taret
        Box(sb, cx, 26, BaseY - 120, 60, 8, new Color(36, 36, 40));
        Box(sb, cx, -180, BaseY - 104, 340, 4, Snow);
        Box(sb, cx, 140, BaseY - 70, 16, 10, new Color(90, 20, 24));                // Sönmüş kızıl göz
        for (int i = 0; i < 4; i++)
            Box(sb, cx, -150 + i * 80, BaseY - 26, 34, 26, new Color(30, 30, 34));     // Tekerlek kalıntıları
    }

    /// <summary>Kutup üssünün blast kapısı: dev çelik halka, sarı-siyah uyarı şeritleri.</summary>
    private static void DrawBlastDoor(SpriteBatch sb, float cx)
    {
        Box(sb, cx, -200, BaseY - 260, 400, 260, new Color(60, 66, 74));
        for (int i = 0; i < 16; i++)
            Box(sb, cx, -200 + i * 25, BaseY - 270, 25, 10, i % 2 == 0 ? new Color(230, 190, 60) : new Color(30, 30, 30));
        sb.Draw(Assets.Circle, new Rectangle((int)cx - 110, BaseY - 230, 220, 220), new Color(90, 96, 104));
        sb.Draw(Assets.Circle, new Rectangle((int)cx - 90, BaseY - 210, 180, 180), new Color(50, 56, 64));
        for (int k = 0; k < 6; k++) Box(sb, cx, -84 + k * 30, BaseY - 124, 18, 8, new Color(120, 126, 136));
        Box(sb, cx, -200, BaseY - 274, 400, 4, Snow);
        DrawSign(sb, cx, BaseY - 310, Loc.T("sign.polar"), new Color(20, 26, 34), new Color(230, 190, 60), new Color(240, 220, 150));
    }

    /// <summary>Üssün derinliklerine inen asansör kafesi.</summary>
    private void DrawElevator(SpriteBatch sb, float cx)
    {
        var steel = new Color(70, 76, 86);
        Box(sb, cx, -80, BaseY - 230, 8, 230, steel);
        Box(sb, cx, 72, BaseY - 230, 8, 230, steel);
        Box(sb, cx, -80, BaseY - 236, 160, 8, steel);
        for (int x = -70; x < 72; x += 14) Box(sb, cx, x, BaseY - 180, 3, 180, new Color(90, 96, 106));
        float blink = MathF.Sin(_time * 3f) > 0f ? 1f : 0.3f;
        Box(sb, cx, -10, BaseY - 222, 20, 10, new Color(90, 230, 120) * blink);
        sb.DrawGlow(new Vector2(cx, BaseY - 217), 30f, new Color(90, 230, 120), 0.4f * blink);
        Box(sb, cx, -90, BaseY - 6, 180, 6, new Color(40, 44, 50));
    }

    /// <summary>Uydu çanağı: AURA'nın yörüngeyle konuştuğu yer.</summary>
    private void DrawSatelliteDish(SpriteBatch sb, float cx)
    {
        Box(sb, cx, -10, BaseY - 120, 20, 120, new Color(60, 64, 72));
        for (int i = 0; i < 6; i++)
        {
            int w = 40 + i * 24;
            Box(sb, cx, -w / 2 - 20 + i * 4, BaseY - 160 - i * 14, w, 10, UiDraw.Shade(new Color(170, 178, 190), 1f - i * 0.05f));
        }
        Box(sb, cx, -4, BaseY - 260, 6, 70, new Color(90, 96, 106));
        float on = MathF.Sin(_time * 4f) > 0f ? 1f : 0.3f;
        Box(sb, cx, -6, BaseY - 266, 10, 8, AuraRed * on);
        sb.DrawGlow(new Vector2(cx, BaseY - 262), 40f, AuraRed, 0.4f * on);
    }

    private static void DrawRoof(SpriteBatch sb, float cx, int top, int width, int steps, Color roof)
    {
        for (int i = 0; i < steps; i++)
        {
            int w = width - i * (width / steps);
            int y = top - i * 7;
            Box(sb, cx, -w / 2, y, w, 8, roof);
            Box(sb, cx, -w / 2 + 2, y - 2, w - 4, 2, Snow);
        }
    }

    private void DrawWindow(SpriteBatch sb, float cx, int x, int y, int w, int h, Color light)
    {
        float flicker = 0.75f + 0.12f * MathF.Sin(_time * 3f + cx * 0.01f + x);
        Box(sb, cx, x - 2, y - 2, w + 4, h + 4, new Color(26, 20, 18));
        Box(sb, cx, x, y, w, h, light == DarkWindow ? light : light * flicker);
        Box(sb, cx, x + w / 2 - 1, y, 2, h, new Color(26, 20, 18));
        Box(sb, cx, x - 3, y + h + 2, w + 6, 3, Snow); // Pervazdaki kar
        if (light != DarkWindow)
            sb.DrawGlow(new Vector2(cx + x + w / 2f, y + h / 2f), w * 1.3f, light, 0.22f);
    }

    /// <summary>Bina arkasında, kasabanın derinliği için uzak ev silüetleri (soluk, küçük pencereli).</summary>
    private void DrawDistantRow(SpriteBatch sb)
    {
        var dark = new Color(22, 24, 34);
        for (int i = 0; i < 16; i++)
        {
            float cx = 760 + i * 130 + (i % 3) * 20;
            int h = 60 + (i * 37 % 40);
            int w = 80 + (i * 23 % 30);
            Box(sb, cx, -w / 2, BaseY - 40 - h, w, h, dark);
            for (int s = 0; s < 3; s++)
                Box(sb, cx, -w / 2 - 6 + s * 10, BaseY - 46 - h - s * 6, w + 12 - s * 20, 6, UiDraw.Shade(dark, 0.8f));
            if (i % 2 == 0 && _layout.DistantLights)
                Box(sb, cx, -8, BaseY - 20 - h, 8, 8, Town.Accent * (0.35f + 0.1f * MathF.Sin(_time + i)));
        }
    }

    /// <summary>Kar birikintisi: üst üste binen yumuşak beyaz bloklar.</summary>
    private static void DrawSnowDrift(SpriteBatch sb, float cx, int baseY, float scale)
    {
        var light = new Color(196, 206, 226);
        var shade = new Color(150, 162, 190);
        int w = (int)(90 * scale);
        int h = (int)(14 * scale);
        Box(sb, cx, -w / 2, baseY - h / 2, w, h / 2 + 2, shade);
        Box(sb, cx, -w / 3, baseY - h, w * 2 / 3, h / 2 + 2, light);
        Box(sb, cx, -w / 6, baseY - h - 4, w / 3, 5, light);
    }

    /// <summary>Ev. Variant 3: Arthur'un eski evi (büyük, tek bir mum ışığı); 9: terk edilmiş, karanlık ve çökük.</summary>
    private void DrawHouse(SpriteBatch sb, float cx, int variant)
    {
        var walls = new[] { new Color(56, 44, 40), new Color(44, 46, 56), new Color(60, 50, 38), new Color(62, 52, 46) };
        bool dark = variant == 9;
        bool home = variant == 3;
        int width = home ? 220 : 140;
        int top = BaseY - (home ? 150 : 110);
        var wall = dark ? new Color(38, 36, 40) : walls[variant % walls.Length];
        Box(sb, cx, -width / 2, top, width, BaseY - top, wall);
        Box(sb, cx, width / 2 - 48, top - 52, 14, 40, new Color(36, 30, 28));     // Baca
        DrawRoof(sb, cx, top - 8, width + 20, 6, new Color(30, 26, 26));
        if (dark)
        {
            Box(sb, cx, -20, top - 30, 40, 22, new Color(8, 10, 16));               // Çökük çatı
            DrawWindow(sb, cx, -width / 2 + 24, top + 36, 30, 26, DarkWindow);
            Box(sb, cx, 18, BaseY - 54, 28, 54, new Color(14, 12, 12));
            Box(sb, cx, 20, BaseY - 54, 6, 54, new Color(60, 50, 40));               // Aralık kalmış kapı
            return;
        }
        if (home)
        {
            DrawWindow(sb, cx, -90, top + 40, 36, 30, DarkWindow);
            DrawWindow(sb, cx, 54, top + 40, 36, 30, new Color(255, 190, 110) * 0.6f); // Tek mum
            Box(sb, cx, -20, BaseY - 64, 40, 64, new Color(30, 24, 22));
            Box(sb, cx, -14, top + 10, 28, 16, new Color(80, 66, 50));                // Kapı üstü tabelası
            return;
        }
        DrawWindow(sb, cx, -46, top + 36, 30, 26, Town.Accent);
        Box(sb, cx, 18, BaseY - 54, 28, 54, new Color(30, 24, 22));    // Kapı
    }

    /// <summary>Taverna. Variant 1: madenci meyhanesi; 2: garnizon kantini.</summary>
    private void DrawTavern(SpriteBatch sb, float cx, int variant = 0)
    {
        int top = BaseY - 180;
        var wall = variant switch { 1 => new Color(58, 50, 44), 2 => new Color(52, 58, 48), _ => new Color(64, 46, 36) };

        Box(sb, cx, -150, top, 300, 180, wall);
        for (int x = -140; x < 150; x += 22)
            Box(sb, cx, x, top + 4, 2, 172, UiDraw.Shade(wall, 0.85f));  // Tahta kaplama
        Box(sb, cx, 80, top - 50, 18, 46, new Color(40, 32, 28));       // Baca
        DrawRoof(sb, cx, top - 8, 330, 7, variant == 2 ? new Color(40, 46, 36) : new Color(36, 26, 24));

        // Üst kat ve zemin kat pencereleri
        DrawWindow(sb, cx, -120, top + 24, 40, 30, Town.Accent);
        DrawWindow(sb, cx, 80, top + 24, 40, 30, Town.Accent);
        DrawWindow(sb, cx, -122, BaseY - 92, 52, 40, Town.Accent);
        DrawWindow(sb, cx, 70, BaseY - 92, 52, 40, Town.Accent);

        // Kapı: aralığından sıcak ışık sızar
        Box(sb, cx, -24, BaseY - 72, 48, 72, new Color(34, 24, 20));
        Box(sb, cx, -2, BaseY - 72, 4, 72, Town.Accent * 0.8f);
        sb.DrawGlow(new Vector2(cx, BaseY - 30), 70f, Town.Accent, 0.25f);

        DrawSign(sb, cx, top + 64, Loc.T(variant == 2 ? "sign.canteen" : "sign.tavern"), new Color(46, 32, 24), new Color(120, 90, 60), Hud.Amber);

        // Fıçılar ya da (madende) kazmalar
        Box(sb, cx, 40, BaseY - 26, 22, 26, new Color(80, 56, 36));
        Box(sb, cx, 40, BaseY - 18, 22, 3, new Color(40, 32, 28));
        if (variant == 1)
        {
            Box(sb, cx, -140, BaseY - 40, 4, 40, new Color(90, 70, 50));
            Box(sb, cx, -150, BaseY - 42, 24, 5, new Color(110, 116, 126));
        }
    }

    /// <summary>Erzak deposu. Variant 1: maden ambarı; 2: tarikatın takas çadırı rengi; 3: levazım deposu.</summary>
    private void DrawStore(SpriteBatch sb, float cx, int variant = 0)
    {
        int top = BaseY - 140;
        var wood = variant switch { 1 => new Color(62, 56, 48), 2 => new Color(70, 80, 96), 3 => new Color(56, 62, 48), 4 => new Color(50, 56, 66), _ => new Color(70, 54, 40) };
        Box(sb, cx, -130, top, 260, 140, wood);
        for (int y = top + 8; y < BaseY; y += 14)
            Box(sb, cx, -130, y, 260, 2, UiDraw.Shade(wood, 0.8f));      // Yatay tahtalar
        Box(sb, cx, -86, top - 46, 14, 40, new Color(40, 32, 28));       // Baca
        DrawRoof(sb, cx, top - 8, 290, 5, new Color(40, 30, 26));

        // Sürgülü kapı (çapraz destekli) ve içeriden sızan ışık
        Box(sb, cx, -50, BaseY - 96, 100, 96, new Color(52, 38, 28));
        Box(sb, cx, -46, BaseY - 92, 4, 88, new Color(90, 70, 50));
        Box(sb, cx, 42, BaseY - 92, 4, 88, new Color(90, 70, 50));
        for (int i = 0; i < 9; i++)
            Box(sb, cx, -42 + i * 10, BaseY - 90 + i * 10, 10, 4, new Color(90, 70, 50));
        Box(sb, cx, 50, BaseY - 96, 10, 96, Town.Accent * 0.6f);
        sb.DrawGlow(new Vector2(cx + 55, BaseY - 48), 60f, Town.Accent, 0.2f);

        string sign = Loc.T(variant switch { 2 => "sign.cultStore", 3 => "sign.quartermaster", 4 => "sign.vending", _ => "sign.store" });
        DrawSign(sb, cx, top + 10, sign, new Color(40, 50, 32), new Color(110, 140, 90), new Color(200, 230, 160));

        // Sandıklar ve çuvallar
        Box(sb, cx, -122, BaseY - 30, 30, 30, new Color(110, 80, 50));
        Box(sb, cx, -118, BaseY - 22, 22, 2, new Color(70, 50, 34));
        Box(sb, cx, -104, BaseY - 54, 26, 24, new Color(100, 74, 46));
        Box(sb, cx, 76, BaseY - 22, 34, 22, new Color(150, 130, 100));
        Box(sb, cx, 80, BaseY - 30, 26, 10, new Color(160, 140, 108));
        InventoryPanel.DrawIcon(sb, ItemType.ThermalCoal, new Point((int)cx - 70, BaseY - 14));
    }

    /// <summary>Tamirci. Variant 1: garnizon cephaneliği (yeşil-gri, zırh plakaları).</summary>
    private void DrawMechanic(SpriteBatch sb, float cx, int variant = 0)
    {
        int top = BaseY - 160;
        var stone = variant == 1 ? new Color(50, 56, 48) : new Color(46, 48, 58);
        Box(sb, cx, -140, top, 280, 160, stone);
        for (int y = top + 10; y < BaseY; y += 16)
            Box(sb, cx, -140, y, 280, 2, UiDraw.Shade(stone, 0.8f));        // Taş sıraları
        Box(sb, cx, -106, top - 52, 14, 46, new Color(34, 34, 40));     // Ocak bacası
        DrawRoof(sb, cx, top - 8, 300, 5, new Color(30, 30, 36));

        // Ocağın turuncu ışığı vuran büyük kapı
        float forge = 0.7f + 0.3f * MathF.Sin(_time * 9f) * MathF.Sin(_time * 4.3f);
        Box(sb, cx, -64, BaseY - 112, 128, 112, new Color(24, 18, 16));
        Box(sb, cx, -40, BaseY - 40, 80, 30, new Color(255, 120, 40) * (0.6f * forge));
        sb.DrawGlow(new Vector2(cx, BaseY - 30), 110f, new Color(255, 130, 50), 0.35f * forge);

        DrawSign(sb, cx, top + 14, Loc.T(variant == 1 ? "sign.armory" : "sign.mechanic"), new Color(30, 32, 40), new Color(110, 115, 130), new Color(200, 205, 220));
        InventoryPanel.DrawIcon(sb, ItemType.SpareParts, new Point((int)cx + 112, top + 40));

        // Örs
        Box(sb, cx, 84, BaseY - 34, 34, 10, new Color(70, 72, 80));
        Box(sb, cx, 94, BaseY - 24, 14, 16, new Color(56, 58, 66));
        Box(sb, cx, 88, BaseY - 8, 26, 8, new Color(46, 48, 54));
    }

    /// <summary>Sığınak girişi: kar gömülü beton höyük, yuvarlak çelik kapak, sarı-siyah uyarı şeritleri.</summary>
    private void DrawBunker(SpriteBatch sb, float cx)
    {
        var concrete = new Color(58, 62, 70);

        // Basamaklı beton höyük
        for (int i = 0; i < 6; i++)
        {
            int w = 300 - i * 40;
            int y = BaseY - 26 - i * 20;
            Box(sb, cx, -w / 2, y, w, 26, UiDraw.Shade(concrete, 1f - i * 0.04f));
            Box(sb, cx, -w / 2 + 4, y - 3, w - 8, 3, Snow);
        }

        // Uyarı şeritli çerçeve ve yuvarlak çelik kapı
        for (int i = 0; i < 8; i++)
            Box(sb, cx, -60 + i * 15, BaseY - 104, 15, 8, i % 2 == 0 ? new Color(230, 190, 60) : new Color(30, 30, 30));
        Box(sb, cx, -50, BaseY - 96, 100, 96, new Color(40, 44, 50));
        sb.Draw(Assets.Circle, new Rectangle((int)cx - 40, BaseY - 90, 80, 80), new Color(84, 90, 100));
        sb.Draw(Assets.Circle, new Rectangle((int)cx - 32, BaseY - 82, 64, 64), new Color(64, 70, 80));
        Box(sb, cx, -30, BaseY - 52, 60, 6, new Color(110, 116, 126));   // Kapak kolu
        Box(sb, cx, -3, BaseY - 80, 6, 60, new Color(110, 116, 126));

        // Yeşil "açık" ışığı
        float blink = MathF.Sin(_time * 2.5f) > 0f ? 1f : 0.4f;
        Box(sb, cx, 56, BaseY - 120, 10, 10, new Color(90, 230, 120) * blink);
        sb.DrawGlow(new Vector2(cx + 61, BaseY - 115), 34f, new Color(90, 230, 120), 0.4f * blink);

        // Havalandırma borusu
        Box(sb, cx, 110, BaseY - 130, 12, 60, new Color(70, 74, 82));
        Box(sb, cx, 104, BaseY - 136, 24, 8, new Color(90, 94, 104));

        DrawSign(sb, cx, BaseY - 152, Loc.T("sign.bunker"), new Color(30, 36, 30), new Color(90, 230, 120) * 0.6f, new Color(160, 240, 170));
    }

    // ---- Ticaret Merkezi ----

    /// <summary>Pazar tezgâhı: çizgili tente, fenerler, kumaş topları ve kavanozlar.</summary>
    private void DrawStall(SpriteBatch sb, float cx, int variant)
    {
        var stripeA = variant == 1 ? new Color(60, 110, 150) : new Color(170, 50, 50);
        var stripeB = new Color(220, 210, 180);
        var wood = new Color(90, 64, 40);
        Box(sb, cx, -90, BaseY - 120, 6, 120, wood);
        Box(sb, cx, 84, BaseY - 120, 6, 120, wood);
        for (int i = 0; i < 9; i++)
            Box(sb, cx, -100 + i * 22, BaseY - 132, 22, 20, i % 2 == 0 ? stripeA : stripeB); // Tente
        for (int i = 0; i < 9; i++)
            Box(sb, cx, -100 + i * 22, BaseY - 112, 22, 6, i % 2 == 0 ? UiDraw.Shade(stripeA, 0.8f) : UiDraw.Shade(stripeB, 0.8f));
        Box(sb, cx, -100, BaseY - 136, 198, 4, Snow);
        Box(sb, cx, -86, BaseY - 46, 172, 46, UiDraw.Shade(wood, 0.9f));        // Tezgâh
        Box(sb, cx, -86, BaseY - 50, 172, 6, new Color(120, 90, 60));
        // Mallar
        Box(sb, cx, -76, BaseY - 64, 20, 14, new Color(200, 140, 60));
        Box(sb, cx, -50, BaseY - 62, 14, 12, new Color(140, 200, 220));
        Box(sb, cx, -30, BaseY - 66, 26, 16, new Color(150, 60, 90));
        Box(sb, cx, 30, BaseY - 62, 18, 12, new Color(230, 220, 160));
        Box(sb, cx, 54, BaseY - 68, 22, 18, new Color(110, 116, 130));
        float flicker = 0.85f + 0.15f * MathF.Sin(_time * 7f + cx);
        Box(sb, cx, -4, BaseY - 106, 8, 10, new Color(255, 210, 120) * flicker);   // Asılı fener
        sb.DrawGlow(new Vector2(cx, BaseY - 100), 70f, new Color(255, 190, 100), 0.3f * flicker);
    }

    /// <summary>İlan panosu: iki direk, kağıtlar (Variant 1: askeri "ARANIYOR" panosu).</summary>
    private void DrawNoticeBoard(SpriteBatch sb, float cx, int variant)
    {
        var wood = new Color(70, 54, 40);
        Box(sb, cx, -56, BaseY - 130, 6, 130, wood);
        Box(sb, cx, 50, BaseY - 130, 6, 130, wood);
        Box(sb, cx, -64, BaseY - 136, 128, 80, variant == 1 ? new Color(48, 52, 44) : new Color(84, 64, 46));
        Box(sb, cx, -64, BaseY - 140, 128, 4, Snow);
        var paper = new Color(220, 210, 180);
        Box(sb, cx, -54, BaseY - 126, 30, 36, paper);
        Box(sb, cx, -16, BaseY - 130, 26, 30, UiDraw.Shade(paper, 0.9f));
        Box(sb, cx, 18, BaseY - 124, 36, 40, paper);
        Box(sb, cx, -48, BaseY - 120, 18, 14, new Color(90, 80, 70));            // Silik yüzler
        Box(sb, cx, 26, BaseY - 116, 20, 16, new Color(90, 80, 70));
        if (variant == 1)
            Box(sb, cx, 18, BaseY - 124, 36, 6, new Color(180, 40, 40));          // Kırmızı "ARANIYOR" şeridi
    }

    // ---- Madenci Köyü ----

    /// <summary>Maden kulesi: kafes kule, tepede kasnak, halat; dibinde mavi kristal ışığı.</summary>
    private void DrawHeadframe(SpriteBatch sb, float cx)
    {
        var steel = new Color(54, 50, 50);
        for (int i = 0; i < 2; i++)
        {
            int side = i == 0 ? -60 : 52;
            Box(sb, cx, side, BaseY - 230, 8, 230, steel);
        }
        for (int y = BaseY - 220; y < BaseY - 10; y += 34)
        {
            Box(sb, cx, -60, y, 120, 4, steel);
            for (int k = 0; k < 6; k++)
                Box(sb, cx, -56 + k * 18, y + 4 + k * 5, 6, 4, steel);              // Çapraz kafes
        }
        sb.Draw(Assets.Circle, new Rectangle((int)cx - 34, BaseY - 270, 68, 68), new Color(70, 64, 60));
        sb.Draw(Assets.Circle, new Rectangle((int)cx - 24, BaseY - 260, 48, 48), new Color(30, 28, 30));
        float spin = _time * 1.5f;
        Box(sb, cx, -2 + (int)(MathF.Cos(spin) * 18), BaseY - 238 + (int)(MathF.Sin(spin) * 18), 4, 4, new Color(150, 140, 130));
        Box(sb, cx, -1, BaseY - 236, 2, 200, new Color(120, 116, 110));          // Halat
        Box(sb, cx, -70, BaseY - 40, 140, 40, new Color(40, 36, 36));             // Kuyu ağzı
        float glow = 0.6f + 0.4f * MathF.Sin(_time * 2f);
        Box(sb, cx, -40, BaseY - 22, 80, 10, new Color(90, 170, 255) * (0.5f * glow));
        sb.DrawGlow(new Vector2(cx, BaseY - 20), 90f, new Color(100, 180, 255), 0.3f * glow);
        DrawSign(sb, cx, BaseY - 80, Loc.T("sign.shaft"), new Color(30, 30, 34), new Color(100, 160, 220), new Color(170, 210, 255));
    }

    /// <summary>Vagonet ve kısa ray parçası; içi mavi kristal dolu.</summary>
    private void DrawMineCart(SpriteBatch sb, float cx)
    {
        Box(sb, cx, -60, BaseY - 6, 120, 3, new Color(100, 96, 90));
        Box(sb, cx, -36, BaseY - 40, 72, 30, new Color(80, 70, 60));
        Box(sb, cx, -36, BaseY - 40, 72, 4, new Color(110, 100, 90));
        sb.Draw(Assets.Circle, new Rectangle((int)cx - 30, BaseY - 14, 14, 14), new Color(30, 28, 28));
        sb.Draw(Assets.Circle, new Rectangle((int)cx + 16, BaseY - 14, 14, 14), new Color(30, 28, 28));
        for (int i = 0; i < 5; i++)
            Box(sb, cx, -30 + i * 13, BaseY - 48 - (i % 2) * 4, 10, 10, new Color(120, 190, 255));
        sb.DrawGlow(new Vector2(cx, BaseY - 46), 50f, new Color(120, 190, 255), 0.25f);
    }

    // ---- Hayalet İstasyon ----

    /// <summary>Uzun, terk edilmiş peron: direkler, çökük çatı, durmuş saat ve bir bank.</summary>
    private void DrawPlatform(SpriteBatch sb, float cx)
    {
        var concrete = new Color(50, 54, 62);
        Box(sb, cx, -440, BaseY - 18, 880, 18, concrete);
        Box(sb, cx, -440, BaseY - 22, 880, 4, Snow);
        for (int i = 0; i < 6; i++)
            Box(sb, cx, -420 + i * 168, BaseY - 190, 8, 172, new Color(40, 42, 50));
        for (int i = 0; i < 11; i++)
        {
            if (i == 4 || i == 7) continue;                                        // Çatıda delikler
            Box(sb, cx, -440 + i * 80, BaseY - 200, 80, 12, new Color(36, 38, 46));
            Box(sb, cx, -440 + i * 80, BaseY - 204, 80, 4, Snow);
        }
        // Saat: hep aynı anda durmuş
        sb.Draw(Assets.Circle, new Rectangle((int)cx + 120, BaseY - 180, 40, 40), new Color(200, 200, 190));
        Box(sb, cx, 139, BaseY - 172, 2, 14, new Color(30, 30, 30));
        Box(sb, cx, 140, BaseY - 160, 10, 2, new Color(30, 30, 30));
        Box(sb, cx, 138, BaseY - 188, 4, 8, new Color(40, 42, 50));
        // Bank
        Box(sb, cx, -60, BaseY - 44, 120, 8, new Color(80, 60, 44));
        Box(sb, cx, -56, BaseY - 36, 6, 18, new Color(50, 40, 30));
        Box(sb, cx, 50, BaseY - 36, 6, 18, new Color(50, 40, 30));
        DrawSign(sb, cx, BaseY - 240, Loc.T("sign.ghost"), new Color(26, 30, 36), new Color(90, 110, 130), new Color(150, 180, 200));
    }

    /// <summary>Hoparlör direği: arada bir cızırdayan huni.</summary>
    private void DrawLoudspeaker(SpriteBatch sb, float cx)
    {
        Box(sb, cx, -3, BaseY - 230, 6, 230, new Color(40, 40, 46));
        Box(sb, cx, 2, BaseY - 226, 26, 14, new Color(90, 96, 104));
        Box(sb, cx, 26, BaseY - 232, 8, 26, new Color(110, 116, 126));
        if (MathF.Sin(_time * 1.3f) > 0.8f)                                        // Fısıltı dalgaları
            for (int i = 0; i < 3; i++)
                Box(sb, cx, 40 + i * 10, BaseY - 226 - i * 3, 3, 14 + i * 6, new Color(150, 180, 200) * 0.4f);
    }

    /// <summary>Bank üstünde gramofon.</summary>
    private void DrawGramophone(SpriteBatch sb, float cx)
    {
        Box(sb, cx, -16, BaseY - 62, 32, 18, new Color(70, 50, 34));
        Box(sb, cx, -18, BaseY - 64, 36, 3, new Color(20, 18, 18));               // Plak
        Box(sb, cx, 4, BaseY - 92, 4, 28, new Color(150, 120, 60));
        Box(sb, cx, 6, BaseY - 106, 26, 20, new Color(190, 150, 70));             // Huni
        Box(sb, cx, 26, BaseY - 112, 10, 32, new Color(210, 170, 90));
        sb.DrawGlow(new Vector2(cx + 20, BaseY - 96), 40f, new Color(150, 180, 200), 0.15f + 0.1f * MathF.Sin(_time * 2f));
    }

    /// <summary>Bilet gişesi: camı kırık kulübe.</summary>
    private void DrawTicketBooth(SpriteBatch sb, float cx)
    {
        Box(sb, cx, -60, BaseY - 130, 120, 130, new Color(46, 48, 56));
        DrawRoof(sb, cx, BaseY - 138, 140, 3, new Color(34, 34, 40));
        Box(sb, cx, -40, BaseY - 100, 80, 40, DarkWindow);
        Box(sb, cx, -40, BaseY - 100, 30, 18, new Color(120, 140, 160) * 0.3f);   // Kırık cam parçası
        Box(sb, cx, -44, BaseY - 58, 88, 6, new Color(70, 60, 50));
        DrawSign(sb, cx, BaseY - 176, Loc.T("sign.tickets"), new Color(26, 30, 36), new Color(90, 110, 130), new Color(150, 180, 200));
    }

    // ---- Buzul Tarikatı ----

    /// <summary>Buz tapınağı: saydam buz blokları, mumlar, mavi hale.</summary>
    private void DrawShrine(SpriteBatch sb, float cx)
    {
        var ice = new Color(150, 200, 235);
        for (int i = 0; i < 7; i++)
        {
            int w = 220 - i * 28;
            Box(sb, cx, -w / 2, BaseY - 30 - i * 30, w, 30, ice * (0.45f + i * 0.05f));
            Box(sb, cx, -w / 2, BaseY - 30 - i * 30, w, 3, Color.White * 0.5f);
        }
        Box(sb, cx, -6, BaseY - 270, 12, 60, ice * 0.8f);                         // Sivri uç
        sb.DrawGlow(new Vector2(cx, BaseY - 140), 170f, new Color(120, 190, 255), 0.25f + 0.08f * MathF.Sin(_time * 1.5f));
        Box(sb, cx, -30, BaseY - 70, 60, 70, new Color(20, 30, 50));              // Giriş
        for (int i = 0; i < 6; i++)
        {
            float flick = 0.7f + 0.3f * MathF.Sin(_time * 9f + i * 1.7f);
            int x = -110 + i * 44;
            Box(sb, cx, x, BaseY - 14, 4, 14, new Color(230, 230, 220));
            Box(sb, cx, x, BaseY - 19, 4, 5, new Color(255, 200, 120) * flick);
            sb.DrawGlow(new Vector2(cx + x + 2, BaseY - 18), 16f, new Color(255, 190, 110), 0.3f * flick);
        }
    }

    /// <summary>Buz totemi: içinde AURORA amblemi donmuş uzun sütun.</summary>
    private void DrawTotem(SpriteBatch sb, float cx)
    {
        var ice = new Color(170, 210, 240);
        Box(sb, cx, -26, BaseY - 220, 52, 220, ice * 0.55f);
        Box(sb, cx, -26, BaseY - 220, 6, 220, Color.White * 0.35f);
        sb.Draw(Assets.Circle, new Rectangle((int)cx - 18, BaseY - 180, 36, 36), new Color(60, 200, 170) * 0.7f);
        for (int i = 0; i < 4; i++)                                                // Kuzey ışığı şeritleri
            Box(sb, cx, -14 + i * 7, BaseY - 176 + (i % 2) * 6, 4, 22, new Color(140, 255, 200) * 0.8f);
        Box(sb, cx, -40, BaseY - 12, 80, 12, new Color(70, 60, 50));               // Adak kaidesi
        Box(sb, cx, -30, BaseY - 20, 12, 8, new Color(140, 120, 90));
        Box(sb, cx, 14, BaseY - 18, 10, 6, new Color(120, 60, 60));
    }

    /// <summary>Kürk çadır.</summary>
    private void DrawTent(SpriteBatch sb, float cx, int variant)
    {
        var hide = variant == 1 ? new Color(110, 100, 86) : new Color(90, 80, 70);
        for (int i = 0; i < 6; i++)
        {
            int w = 180 - i * 28;
            Box(sb, cx, -w / 2, BaseY - 20 - i * 20, w, 20, UiDraw.Shade(hide, 1f - i * 0.04f));
        }
        Box(sb, cx, -4, BaseY - 150, 8, 30, new Color(60, 50, 40));
        Box(sb, cx, -22, BaseY - 60, 44, 60, new Color(26, 22, 20));
        sb.DrawGlow(new Vector2(cx, BaseY - 30), 40f, new Color(255, 170, 90), 0.2f);
        Box(sb, cx, -90, BaseY - 22, 180, 3, Snow);
    }

    // ---- Terk Edilmiş Karakol / yıkıntılar ----

    /// <summary>Yıkık duvar parçaları ve moloz.</summary>
    private static void DrawRuin(SpriteBatch sb, float cx, int variant)
    {
        var wall = variant == 1 ? new Color(44, 42, 46) : new Color(52, 48, 46);
        int[] heights = { 90, 140, 60, 120, 40, 100 };
        for (int i = 0; i < heights.Length; i++)
        {
            int h = heights[(i + variant) % heights.Length];
            Box(sb, cx, -150 + i * 50, BaseY - h, 46, h, UiDraw.Shade(wall, 1f - i * 0.03f));
            Box(sb, cx, -150 + i * 50, BaseY - h - 3, 46, 3, Snow);
        }
        Box(sb, cx, -100, BaseY - 80, 30, 26, DarkWindow);
        for (int i = 0; i < 7; i++)
            Box(sb, cx, -160 + i * 48, BaseY - 10 - (i % 3) * 4, 22, 10 + (i % 3) * 4, new Color(70, 66, 64));
    }

    /// <summary>Asker barakası: oluklu çatı. Variant 9: delik çatı, kırık pencereler.</summary>
    private void DrawBarracks(SpriteBatch sb, float cx, int variant)
    {
        bool broken = variant == 9;
        var wall = broken ? new Color(46, 50, 46) : new Color(58, 66, 54);
        Box(sb, cx, -150, BaseY - 110, 300, 110, wall);
        for (int i = 0; i < 16; i++)
        {
            if (broken && (i == 5 || i == 6 || i == 11)) continue;
            Box(sb, cx, -160 + i * 20, BaseY - 126 - (i % 2) * 2, 20, 16, new Color(70, 76, 70));
        }
        Box(sb, cx, -160, BaseY - 130, 320, 4, Snow);
        for (int i = 0; i < 4; i++)
            DrawWindow(sb, cx, -130 + i * 74, BaseY - 90, 36, 26, broken ? DarkWindow : Town.Accent);
        Box(sb, cx, -18, BaseY - 64, 36, 64, broken ? new Color(14, 14, 14) : new Color(34, 38, 32));
        if (!broken) DrawSign(sb, cx, BaseY - 166, Loc.T("sign.barracks"), new Color(30, 36, 30), new Color(110, 130, 90), new Color(200, 220, 170));
    }

    /// <summary>Gözetleme kulesi. Variant 9: devrilmiş, bacağı kopuk.</summary>
    private void DrawWatchtower(SpriteBatch sb, float cx, int variant)
    {
        var wood = new Color(60, 50, 40);
        bool broken = variant == 9;
        Box(sb, cx, -40, BaseY - 200, 8, 200, wood);
        if (!broken) Box(sb, cx, 32, BaseY - 200, 8, 200, wood);
        else Box(sb, cx, 32, BaseY - 80, 8, 80, wood);
        for (int y = BaseY - 180; y < BaseY; y += 45)
            Box(sb, cx, -40, y, 80, 4, wood);
        int tilt = broken ? 18 : 0;
        Box(sb, cx, -54 + tilt, BaseY - 250 + tilt, 108, 54, new Color(50, 44, 38));
        Box(sb, cx, -64 + tilt, BaseY - 262 + tilt, 128, 12, new Color(36, 30, 28));
        Box(sb, cx, -64 + tilt, BaseY - 266 + tilt, 128, 4, Snow);
        Box(sb, cx, -40 + tilt, BaseY - 238 + tilt, 80, 20, broken ? DarkWindow : new Color(255, 220, 140) * 0.5f);
    }

    /// <summary>Sınır bariyeri: kırmızı-beyaz kol, nöbet kulübesi, dikenli tel.</summary>
    private void DrawBarrier(SpriteBatch sb, float cx)
    {
        Box(sb, cx, -140, BaseY - 120, 70, 120, new Color(56, 62, 52));            // Kulübe
        Box(sb, cx, -146, BaseY - 128, 82, 8, new Color(40, 44, 38));
        Box(sb, cx, -126, BaseY - 100, 42, 24, Town.Accent * 0.6f);
        Box(sb, cx, -60, BaseY - 70, 12, 70, new Color(70, 70, 76));
        for (int i = 0; i < 9; i++)
            Box(sb, cx, -48 + i * 24, BaseY - 66, 24, 10, i % 2 == 0 ? new Color(200, 40, 40) : new Color(230, 230, 230));
        for (int i = 0; i < 2; i++)                                                 // Dikenli tel
        {
            int y = BaseY - 40 - i * 18;
            Box(sb, cx, 170, y, 160, 2, new Color(110, 110, 116));
            for (int k = 0; k < 10; k++) Box(sb, cx, 172 + k * 16, y - 3, 3, 8, new Color(130, 130, 136));
        }
        Box(sb, cx, 168, BaseY - 80, 4, 80, new Color(60, 60, 66));
        Box(sb, cx, 328, BaseY - 80, 4, 80, new Color(60, 60, 66));
    }

    /// <summary>Kafesin arka paneli (demirler insanların önünde ayrıca çizilir).</summary>
    private static void DrawCageBack(SpriteBatch sb, float cx)
    {
        Box(sb, cx, -40, BaseY - 110 + 70, 80, 4, new Color(60, 60, 66));
        Box(sb, cx, -46, BaseY + 66 - 140, 92, 8, new Color(70, 70, 76));
    }

    private static void DrawCageBars(SpriteBatch sb, float cx)
    {
        int top = (int)FeetY - 74;
        for (int i = 0; i < 7; i++)
            Box(sb, cx, -36 + i * 12, top, 3, 74, new Color(90, 92, 100));
        Box(sb, cx, -40, top - 4, 82, 6, new Color(100, 102, 110));
        Box(sb, cx, -40, top - 7, 82, 3, Snow);
    }

    /// <summary>Sandık ya da dolap yığını. Variant 1: kırık kasa; 2: Enstitü konteyneri.</summary>
    private static void DrawCrates(SpriteBatch sb, float cx, int variant)
    {
        var wood = variant == 2 ? new Color(70, 90, 110) : new Color(96, 72, 46);
        Box(sb, cx, -60, BaseY - 44, 50, 44, wood);
        Box(sb, cx, -4, BaseY - 36, 40, 36, UiDraw.Shade(wood, 0.9f));
        Box(sb, cx, -44, BaseY - 80, 40, 36, UiDraw.Shade(wood, 1.08f));
        Box(sb, cx, -60, BaseY - 26, 50, 3, UiDraw.Shade(wood, 0.7f));
        Box(sb, cx, -60, BaseY - 84, 100, 4, Snow);
        if (variant == 1)
        {
            Box(sb, cx, 46, BaseY - 70, 40, 70, new Color(70, 74, 80));             // Çelik kasa
            Box(sb, cx, 50, BaseY - 66, 32, 62, new Color(50, 54, 60));
            sb.Draw(Assets.Circle, new Rectangle((int)cx + 58, BaseY - 46, 16, 16), new Color(110, 116, 126));
        }
        if (variant == 2)
            Box(sb, cx, -54, BaseY - 34, 38, 8, new Color(60, 200, 170));          // AURORA şeridi
    }

    // ---- Eski Ev ----

    private static void DrawFence(SpriteBatch sb, float cx)
    {
        for (int i = 0; i < 9; i++)
        {
            Box(sb, cx, -120 + i * 30, BaseY - 54, 10, 54, new Color(110, 96, 80));
            Box(sb, cx, -120 + i * 30, BaseY - 58, 10, 4, Snow);
        }
        Box(sb, cx, -124, BaseY - 40, 260, 6, new Color(90, 78, 64));
        Box(sb, cx, -20, BaseY - 84, 70, 26, new Color(70, 56, 40));                // Kapı levhası
        Box(sb, cx, -16, BaseY - 80, 62, 18, new Color(150, 130, 100));
    }

    private static void DrawMailbox(SpriteBatch sb, float cx)
    {
        Box(sb, cx, -3, BaseY - 80, 6, 80, new Color(60, 50, 40));
        Box(sb, cx, -18, BaseY - 106, 36, 26, new Color(140, 40, 40));
        Box(sb, cx, -18, BaseY - 110, 36, 4, Snow);
        Box(sb, cx, 18, BaseY - 104, 3, 14, new Color(200, 180, 60));               // Bayrak
    }

    private void DrawSwing(SpriteBatch sb, float cx)
    {
        var frame = new Color(80, 70, 60);
        Box(sb, cx, -60, BaseY - 150, 6, 150, frame);
        Box(sb, cx, 54, BaseY - 150, 6, 150, frame);
        Box(sb, cx, -64, BaseY - 156, 128, 8, frame);
        Box(sb, cx, -64, BaseY - 160, 128, 4, Snow);
        float sway = MathF.Sin(_time * 0.9f) * 6f;                                  // Rüzgârda boş salıncak
        Box(sb, cx, -18 + (int)sway, BaseY - 148, 2, 100, new Color(120, 120, 126));
        Box(sb, cx, 16 + (int)sway, BaseY - 148, 2, 100, new Color(120, 120, 126));
        Box(sb, cx, -22 + (int)sway, BaseY - 50, 42, 6, new Color(150, 60, 50));
    }

    /// <summary>Arthur'un eski atölyesi: makinist aletleri ve kapı aralığı.</summary>
    private void DrawShed(SpriteBatch sb, float cx)
    {
        Box(sb, cx, -90, BaseY - 100, 180, 100, new Color(54, 46, 40));
        DrawRoof(sb, cx, BaseY - 108, 200, 4, new Color(34, 28, 26));
        Box(sb, cx, -30, BaseY - 76, 60, 76, new Color(24, 20, 18));
        Box(sb, cx, 22, BaseY - 76, 8, 76, new Color(80, 66, 50));
        Box(sb, cx, -80, BaseY - 76, 30, 20, DarkWindow);
        InventoryPanel.DrawIcon(sb, ItemType.SpareParts, new Point((int)cx + 62, BaseY - 30));
    }

    // ---- Karargah, telsiz, Enstitü, Sığınak ----

    /// <summary>Kızıl Birlik'in betondan karargahı: kırık anten, kızıl yıldız izi, mühürlü kapı.</summary>
    private void DrawHeadquarters(SpriteBatch sb, float cx)
    {
        var concrete = new Color(56, 58, 62);
        Box(sb, cx, -200, BaseY - 170, 400, 170, concrete);
        for (int y = BaseY - 160; y < BaseY; y += 20)
            Box(sb, cx, -200, y, 400, 2, UiDraw.Shade(concrete, 0.85f));
        Box(sb, cx, -210, BaseY - 178, 420, 8, new Color(44, 46, 50));
        Box(sb, cx, -210, BaseY - 182, 420, 4, Snow);
        Box(sb, cx, -60, BaseY - 110, 120, 110, new Color(30, 32, 36));             // Çelik kapı
        Box(sb, cx, -4, BaseY - 110, 8, 110, new Color(80, 30, 30));                 // Kızıl şerit
        sb.Draw(Assets.Circle, new Rectangle((int)cx - 26, BaseY - 160, 52, 40), new Color(120, 40, 40) * 0.6f);
        for (int i = 0; i < 3; i++)
            DrawWindow(sb, cx, -180 + i * 50, BaseY - 140, 30, 20, DarkWindow);
        for (int i = 0; i < 3; i++)
            DrawWindow(sb, cx, 80 + i * 40, BaseY - 140, 24, 20, DarkWindow);
        Box(sb, cx, 150, BaseY - 260, 4, 82, new Color(70, 70, 76));                // Kırık anten
        Box(sb, cx, 130, BaseY - 250, 44, 3, new Color(70, 70, 76));
        DrawSign(sb, cx, BaseY - 214, Loc.T("sign.hq"), new Color(40, 24, 24), new Color(150, 60, 60), new Color(230, 160, 150));
    }

    /// <summary>Telsiz direği: kafes kule, tepede yanıp sönen kırmızı ışık. Variant 9: tepesi kırık.</summary>
    private void DrawRadioMast(SpriteBatch sb, float cx, int variant)
    {
        var steel = new Color(70, 72, 80);
        int height = variant == 9 ? 220 : 330;
        Box(sb, cx, -22, BaseY - height, 4, height, steel);
        Box(sb, cx, 18, BaseY - height, 4, height, steel);
        for (int y = BaseY - height; y < BaseY; y += 22)
        {
            Box(sb, cx, -22, y, 44, 3, steel);
            Box(sb, cx, -18 + (y / 22 % 2) * 20, y + 4, 20, 2, steel);
        }
        if (variant == 9)
        {
            Box(sb, cx, -10, BaseY - height - 30, 4, 34, steel);                    // Eğik kırık parça
            return;
        }
        bool on = MathF.Sin(_time * 3f) > 0f;
        Box(sb, cx, -5, BaseY - height - 12, 10, 10, new Color(255, 60, 50) * (on ? 1f : 0.3f));
        if (on) sb.DrawGlow(new Vector2(cx, BaseY - height - 8), 50f, new Color(255, 60, 50), 0.5f);
        for (int i = 1; i <= 3; i++)                                               // Yayın dalgaları
        {
            float pulse = (_time * 0.8f + i * 0.33f) % 1f;
            sb.DrawGlow(new Vector2(cx, BaseY - height), 40f + pulse * 120f, new Color(140, 220, 255), 0.12f * (1f - pulse));
        }
    }

    /// <summary>Küçük kulübe (telsizci kulübesi). Variant 1: Enstitü ofisi; 9: karargah yazıhanesi (karanlık).</summary>
    private void DrawHut(SpriteBatch sb, float cx, int variant)
    {
        var wall = variant == 1 ? new Color(70, 80, 92) : variant == 9 ? new Color(44, 44, 48) : new Color(60, 50, 42);
        Box(sb, cx, -80, BaseY - 110, 160, 110, wall);
        DrawRoof(sb, cx, BaseY - 118, 180, 4, new Color(34, 30, 30));
        Box(sb, cx, 20, BaseY - 160, 12, 40, new Color(40, 36, 34));
        DrawWindow(sb, cx, -60, BaseY - 80, 34, 26, variant == 9 ? DarkWindow : variant == 1 ? new Color(140, 220, 255) : new Color(255, 200, 120));
        Box(sb, cx, 18, BaseY - 64, 30, 64, new Color(30, 26, 24));
    }

    /// <summary>Kuzey İklim Enstitüsü'nün kubbesi ve AURORA amblemi.</summary>
    private void DrawLabDome(SpriteBatch sb, float cx)
    {
        var metal = new Color(80, 92, 104);
        for (int i = 0; i < 9; i++)
        {
            int w = (int)(320 * MathF.Sqrt(1f - (i / 9f) * (i / 9f)));
            Box(sb, cx, -w / 2, BaseY - 24 - i * 22, w, 22, UiDraw.Shade(metal, 1f - i * 0.03f));
        }
        Box(sb, cx, -170, BaseY - 26, 340, 26, new Color(60, 66, 74));
        Box(sb, cx, -40, BaseY - 90, 80, 90, new Color(26, 30, 36));
        sb.Draw(Assets.Circle, new Rectangle((int)cx - 30, BaseY - 170, 60, 60), new Color(40, 50, 60));
        for (int i = 0; i < 4; i++)
            Box(sb, cx, -18 + i * 10, BaseY - 158 + (i % 2) * 6, 5, 34, new Color(140, 255, 200) * 0.8f);
        DrawSign(sb, cx, BaseY - 240, Loc.T("sign.institute"), new Color(20, 30, 40), new Color(100, 200, 180), new Color(170, 255, 220));
    }

    /// <summary>AURORA kulesi: tepesinde dev mavi kristal, gökyüzüne uzanan kuzey ışığı şeritleri.</summary>
    private void DrawAuroraTower(SpriteBatch sb, float cx)
    {
        var steel = new Color(60, 66, 76);
        Box(sb, cx, -30, BaseY - 300, 60, 300, steel);
        for (int y = BaseY - 290; y < BaseY; y += 30)
            Box(sb, cx, -36, y, 72, 4, UiDraw.Shade(steel, 0.8f));
        float glow = 0.6f + 0.4f * MathF.Sin(_time * 1.7f);
        for (int i = 0; i < 5; i++)
        {
            int w = 20 + i * 10;
            Box(sb, cx, -w / 2, BaseY - 380 + i * 14, w, 14, new Color(110, 200, 255) * (0.6f + 0.08f * i));
        }
        sb.DrawGlow(new Vector2(cx, BaseY - 350), 160f, new Color(110, 200, 255), 0.45f * glow);
        for (int i = 0; i < 6; i++)                                                // Gökyüzüne yükselen şeritler
        {
            float wave = MathF.Sin(_time * 0.8f + i) * 20f;
            Box(sb, cx, -80 + i * 32 + (int)wave, BaseY - 520, 6, 120, new Color(120, 255, 200) * (0.18f * glow));
        }
    }

    /// <summary>Enstitü terminali: yeşil yazılı ekran.</summary>
    private void DrawTerminal(SpriteBatch sb, float cx)
    {
        Box(sb, cx, -50, BaseY - 100, 100, 100, new Color(50, 54, 60));
        Box(sb, cx, -40, BaseY - 92, 80, 50, new Color(10, 20, 16));
        for (int i = 0; i < 5; i++)
        {
            int w = 20 + (int)((MathF.Sin(_time * 2f + i * 1.3f) * 0.5f + 0.5f) * 50);
            Box(sb, cx, -34, BaseY - 86 + i * 9, w, 4, new Color(90, 255, 160) * 0.8f);
        }
        sb.DrawGlow(new Vector2(cx, BaseY - 66), 70f, new Color(90, 255, 160), 0.2f);
        Box(sb, cx, -46, BaseY - 36, 92, 10, new Color(70, 74, 80));
    }

    /// <summary>Boş çekirdek yuvası: paketin tam ölçüsünde, hâlâ hafifçe ışıyan bir oyuk.</summary>
    private void DrawCoreCradle(SpriteBatch sb, float cx)
    {
        Box(sb, cx, -60, BaseY - 40, 120, 40, new Color(60, 66, 74));
        Box(sb, cx, -44, BaseY - 90, 88, 50, new Color(70, 78, 88));
        Box(sb, cx, -24, BaseY - 80, 48, 34, new Color(14, 20, 28));
        float glow = 0.5f + 0.5f * MathF.Sin(_time * 3f);
        Box(sb, cx, -20, BaseY - 76, 40, 4, new Color(110, 200, 255) * glow);
        sb.DrawGlow(new Vector2(cx, BaseY - 64), 60f, new Color(110, 200, 255), 0.25f * glow);
    }

    /// <summary>Sığınak-0'ın dış duvarı ve dev kapısı (Variant 1: sınır duvarı, "0" yerine Konsey arması).</summary>
    private void DrawShelterWall(SpriteBatch sb, float cx, int variant)
    {
        var concrete = new Color(70, 74, 82);
        Box(sb, cx, -520, BaseY - 260, 1040, 260, concrete);
        for (int y = BaseY - 250; y < BaseY; y += 26)
            Box(sb, cx, -520, y, 1040, 2, UiDraw.Shade(concrete, 0.85f));
        Box(sb, cx, -530, BaseY - 272, 1060, 12, new Color(56, 60, 66));
        Box(sb, cx, -530, BaseY - 276, 1060, 4, Snow);
        Box(sb, cx, -130, BaseY - 220, 260, 220, new Color(44, 48, 54));            // Kapı
        Box(sb, cx, -2, BaseY - 220, 4, 220, new Color(20, 22, 26));
        for (int i = 0; i < 6; i++)
            Box(sb, cx, -126, BaseY - 200 + i * 34, 252, 3, new Color(60, 64, 70));
        if (variant == 1)
        {
            DrawSign(sb, cx, BaseY - 250, Loc.T("sign.border"), new Color(20, 30, 40), new Color(140, 200, 255), new Color(200, 230, 255));
            return;
        }
        sb.Draw(Assets.Circle, new Rectangle((int)cx - 60, BaseY - 380, 120, 120), new Color(200, 210, 220) * 0.25f);
        sb.Draw(Assets.Circle, new Rectangle((int)cx - 40, BaseY - 360, 80, 80), new Color(10, 12, 16));
        Box(sb, cx, -20, BaseY - 262, 40, 6, new Color(255, 200, 120) * (0.2f + 0.1f * MathF.Sin(_time)));  // Sönük ışık
        DrawSign(sb, cx, BaseY - 304, Loc.T("sign.shelter"), new Color(20, 24, 30), new Color(150, 160, 180), new Color(220, 225, 235));
    }

    /// <summary>Konsey'in beyaz zırhlı aracı.</summary>
    private static void DrawArmoredCar(SpriteBatch sb, float cx)
    {
        var white = new Color(200, 204, 210);
        Box(sb, cx, -110, BaseY - 80, 220, 60, white);
        Box(sb, cx, -60, BaseY - 120, 120, 40, UiDraw.Shade(white, 0.92f));
        Box(sb, cx, -50, BaseY - 112, 40, 20, new Color(40, 60, 80));
        Box(sb, cx, -110, BaseY - 54, 220, 6, new Color(120, 140, 170));
        for (int i = 0; i < 4; i++)
            sb.Draw(Assets.Circle, new Rectangle((int)cx - 96 + i * 52, BaseY - 34, 34, 34), new Color(30, 30, 34));
        Box(sb, cx, -114, BaseY - 124, 228, 4, Snow);
    }

    /// <summary>Dönen projektör: gökyüzünü tarayan soğuk ışık hüzmesi.</summary>
    private void DrawSearchlight(SpriteBatch sb, float cx)
    {
        Box(sb, cx, -4, BaseY - 160, 8, 160, new Color(50, 52, 58));
        Box(sb, cx, -14, BaseY - 176, 28, 18, new Color(80, 84, 92));
        float angle = -MathHelper.PiOver2 + MathF.Sin(_time * 0.5f + cx * 0.01f) * 0.7f;
        var beam = new Color(200, 220, 255, 0) * 0.14f;
        sb.Draw(Assets.LightCone, new Vector2(cx, BaseY - 168), null, beam, angle,
            new Vector2(0f, Assets.LightCone.Height / 2f), new Vector2(0.9f, 0.6f), SpriteEffects.None, 0f);
        sb.DrawGlow(new Vector2(cx, BaseY - 168), 30f, new Color(220, 235, 255), 0.5f);
    }

    private static void DrawSign(SpriteBatch sb, float cx, int y, string text, Color board, Color border, Color textColor)
    {
        int width = (int)GameText.MeasureLine(text) + 24;
        var rect = new Rectangle((int)cx - width / 2, y, width, 28);
        sb.FillRect(rect, board);
        sb.DrawBorder(rect, 2, border);
        sb.FillRect(new Rectangle(rect.X + 2, rect.Y - 3, rect.Width - 4, 3), Snow); // Tabeladaki kar
        GameText.DrawCentered(sb, text, cx, y + 4, textColor);
    }

    /// <summary>Lamba direği ve aşağıya düşen koni biçimli ışık hüzmesi.</summary>
    private void DrawLampPost(SpriteBatch sb, float x)
    {
        float flicker = 0.85f + 0.15f * MathF.Sin(_time * 7f + x);
        if (Archetype == TownArchetype.GhostStation) // Hayalet istasyonda tek lamba ölüp ölüp yanar
            flicker = MathF.Sin(_time * 13f) * MathF.Sin(_time * 2.1f) > 0.2f ? 0.9f : 0.1f;
        Box(sb, x, -2, BaseY - 120, 4, 120, new Color(26, 26, 30));
        Box(sb, x, -10, BaseY - 124, 20, 4, new Color(26, 26, 30));
        Box(sb, x, -5, BaseY - 120, 10, 8, Town.Accent * 0.9f * flicker);

        // Işık hüzmesi: far konisi dokusu aşağı döndürülerek
        var beam = new Color(Town.Accent.R, Town.Accent.G, Town.Accent.B, (byte)0) * (0.16f * flicker);
        sb.Draw(Assets.LightCone, new Vector2(x, BaseY - 112), null, beam, MathHelper.PiOver2,
            new Vector2(0f, Assets.LightCone.Height / 2f), new Vector2(0.3f, 0.75f), SpriteEffects.None, 0f);
        sb.DrawGlow(new Vector2(x, BaseY - 116), 50f, Town.Accent, 0.3f * flicker);
        sb.DrawGlow(new Vector2(x, BaseY + 4), 60f, Town.Accent, 0.1f * flicker); // Yerdeki ışık havuzu
    }
}
