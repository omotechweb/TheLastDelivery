using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheLastDelivery.Core;
using TheLastDelivery.Graphics;
using TheLastDelivery.Localization;
using TheLastDelivery.Passengers;

namespace TheLastDelivery.Story;

/// <summary>Sürüş sırasında beliren kısa anlatım satırının türü.</summary>
public enum ChatterKind
{
    Radio,     // Telsizden gelen 2-3 cümlelik yayın (Episode 2'de çoğu AURA'nın sesi)
    Passenger, // Kabindeki bir yolcunun kısa korku/şüphe cümlesi
    Memory     // Arthur'un geçmişinden bir hatıra
}

/// <param name="Key">Çeviri anahtarı; aynı zamanda "görüldü" kaydı (StoryState.Seen).</param>
/// <param name="MinChapter">Bu satır en erken bu bölümde duyulur: hikâye sırayla açılır.</param>
/// <param name="PassengerId">Yalnızca bu yolcu trendeyken ve onun ağzından duyulur (kişisel satır).</param>
public readonly record struct ChatterLine(string Key, ChatterKind Kind, int MinChapter, string PassengerId = null);

/// <summary>
/// Sürüş telsizi: hikâyeyi uzun metinlerle değil, sürüşün arasına serpiştirilmiş kısa satırlarla anlatır.
/// Bölüm başına en fazla <see cref="MaxPerChapter"/> satır, aralarında 55-85 sn. Her satır bir kez duyulur
/// (Episode'un kendi StoryState'inde saklanır); sıradaki satır her zaman henüz duyulmamış en eski satırdır.
/// </summary>
public sealed class DriveChatter
{
    private const int MaxPerChapter = 4;
    private const int ReactionCount = 4;

    // Bir yolcu öldüğünde, kaçtığında ya da hain çıktığında kabinin tepkisi (bekleyen tek satır)
    private string _reactionKey, _reactionSubject;
    private int _reactionRotation;
    private const float FirstDelayMin = 18f, FirstDelayMax = 28f;
    private const float GapMin = 55f, GapMax = 85f;
    private const float FadeTime = 0.4f;

    private static readonly Rectangle Box = new(444, 100, 388, 0); // Hız ile kabin paneli arası, mod panelinin altı
    private const int TextWidth = 360;

    private readonly Random _rng = new();
    private float _timer;
    private int _shown;
    private int _rotation;

    private float _age = -1f, _life;
    private string _speaker, _text;
    private ChatterKind _kind;
    private bool _aura;
    private List<string> _lines = new();

    public bool IsShowing => _age >= 0f;

    /// <summary>Yeni bölüm: sayaç sıfırlanır, ilk satır bölümün ilk yarım dakikasında gelir.</summary>
    public void StartChapter()
    {
        _shown = 0;
        _timer = FirstDelayMin + (float)_rng.NextDouble() * (FirstDelayMax - FirstDelayMin);
        _age = -1f;
    }

    public void Clear() => _age = -1f;

    /// <summary>Yeni koşu: bekleyen tepki satırı önceki yolculuktan kalmasın.</summary>
    public void ResetRun()
    {
        _age = -1f;
        _reactionKey = null;
        _reactionSubject = null;
    }

    /// <summary>
    /// Kabinden biri eksildi: sıradaki uygun anda kalan yolculardan biri bunu dile getirir.
    /// <paramref name="kind"/>: "lost" (soğuktan öldü), "left" (açlıktan/umutsuzluktan gitti), "betray" (hain çıktı).
    /// </summary>
    public void QueueReaction(string kind, string subjectName)
    {
        _reactionRotation = (_reactionRotation + 1) % ReactionCount;
        int variants = kind == "lost" ? ReactionCount : 2;
        _reactionKey = $"chat.react.{kind}.{_reactionRotation % variants + 1}";
        _reactionSubject = subjectName;
    }

    /// <param name="canStart">Yeni satır başlayabilir mi? (sürüşte, olay yokken)</param>
    /// <returns>Bu karede yeni bir satır başladıysa türü (Game1 telsiz sesini çalar), yoksa null.</returns>
    public ChatterKind? Update(float dt, bool canStart, int episode, int chapter, IReadOnlyList<Passenger> passengers)
    {
        if (_age >= 0f)
        {
            _age += dt;
            if (_age >= _life) _age = -1f;
            return null; // Satır ekrandayken sayaç beklemez ama yenisi başlamaz
        }

        // Kabinin tepkisi beklemez: bölüm sınırı ve sayaç dışında, ilk uygun anda
        if (_reactionKey != null && canStart)
        {
            BeginReaction(passengers);
            return ChatterKind.Passenger;
        }

        if (_shown >= MaxPerChapter) return null;
        _timer -= dt;
        if (_timer > 0f || !canStart) return null;

        var pool = episode == 2 ? ChatterData.Episode2 : ChatterData.Episode1;

        // Önce bu bölüme yazılmış telsiz satırları: hikâyenin o anki dönüm noktası (ör. AURA'nın ilk sesi)
        foreach (var line in pool)
        {
            if (line.Kind != ChatterKind.Radio || line.MinChapter != chapter || Narrative.Seen(line.Key)) continue;
            Begin(line, passengers);
            _shown++;
            _timer = GapMin + (float)_rng.NextDouble() * (GapMax - GapMin);
            return ChatterKind.Radio;
        }

        for (int attempt = 0; attempt < 3; attempt++)
        {
            var kind = (ChatterKind)((_rotation + attempt) % 3);
            if (kind == ChatterKind.Passenger && passengers.Count == 0) continue;
            if (kind == ChatterKind.Passenger && TryPersonal(episode == 2 ? ChatterData.Personal2 : ChatterData.Personal, chapter, passengers))
            {
                _rotation = (_rotation + attempt + 1) % 3;
                _shown++;
                _timer = GapMin + (float)_rng.NextDouble() * (GapMax - GapMin);
                return kind;
            }
            foreach (var line in pool)
            {
                if (line.Kind != kind || line.PassengerId != null || line.MinChapter > chapter || Narrative.Seen(line.Key)) continue;
                Begin(line, passengers);
                _rotation = (_rotation + attempt + 1) % 3;
                _shown++;
                _timer = GapMin + (float)_rng.NextDouble() * (GapMax - GapMin);
                return kind;
            }
        }
        _timer = 20f; // Bu bölüm için yeni satır yok; biraz sonra yeniden bak (yolcu binmiş olabilir)
        return null;
    }

    /// <summary>
    /// Trende olan bir yolcunun kendi ağzından, henüz duyulmamış kişisel satırı. Yolcular kendi sesleriyle
    /// konuşur: Mika çocuk gibi, Oskar eski bir asker gibi. En uzun süredir susan yolcu önce konuşur.
    /// </summary>
    private bool TryPersonal(ChatterLine[] personal, int chapter, IReadOnlyList<Passenger> passengers)
    {
        int offset = _rng.Next(Math.Max(1, passengers.Count));
        for (int i = 0; i < passengers.Count; i++)
        {
            var passenger = passengers[(offset + i) % passengers.Count];
            foreach (var line in personal)
            {
                if (line.PassengerId != passenger.Profile.Id || line.MinChapter > chapter || Narrative.Seen(line.Key)) continue;
                Begin(line, passengers, passenger.Name);
                return true;
            }
        }
        return false;
    }

    private void BeginReaction(IReadOnlyList<Passenger> passengers)
    {
        bool alone = passengers.Count == 0;
        _kind = alone ? ChatterKind.Memory : ChatterKind.Passenger;
        _aura = false;
        _wolf = false;
        _speaker = alone ? Loc.T("chat.memory") : passengers[_rng.Next(passengers.Count)].Name;
        _text = alone ? Loc.F("chat.react.alone", _reactionSubject) : Loc.F(_reactionKey, _reactionSubject);
        _reactionKey = null;
        _reactionSubject = null;
        StartShowing();
    }

    private bool _wolf;

    private void Begin(ChatterLine line, IReadOnlyList<Passenger> passengers, string speakerName = null)
    {
        Narrative.MarkSeen(line.Key);
        _kind = line.Kind;
        _aura = line.Key.Contains(".a.");
        _wolf = line.Key.Contains(".w.");
        _speaker = line.Kind switch
        {
            ChatterKind.Passenger => speakerName ?? passengers[_rng.Next(passengers.Count)].Name,
            ChatterKind.Memory => Loc.T("chat.memory"),
            _ when _wolf => Loc.T("spk.wolf"),
            _ => Loc.T(_aura ? "chat.aura" : "chat.radio")
        };
        _text = Loc.T(line.Key);
        StartShowing();
    }

    private void StartShowing()
    {
        _lines = GameText.Wrap(_text, TextWidth, 2);
        _life = Math.Clamp(5f + _text.Length / 16f, 7f, 14f); // Okuma süresi metnin uzunluğuyla artar
        _age = 0f;
    }

    private Color Accent => _kind switch
    {
        ChatterKind.Passenger => new Color(150, 230, 160),
        ChatterKind.Memory => new Color(170, 200, 255),
        _ when _wolf => new Color(235, 120, 90),
        _ => _aura ? new Color(255, 70, 80) : new Color(255, 196, 90)
    };

    public void Draw(SpriteBatch sb, float time)
    {
        if (_age < 0f) return;
        float alpha = MathHelper.Clamp(_age / FadeTime, 0f, 1f) * MathHelper.Clamp((_life - _age) / FadeTime, 0f, 1f);

        int lineHeight = (int)GameText.LineHeight(2) + 4;
        var box = new Rectangle(Box.X, Box.Y, Box.Width, 44 + _lines.Count * lineHeight);
        sb.FillRect(box, new Color(6, 8, 16) * (0.86f * alpha));
        sb.DrawBorder(box, 2, Accent * (0.7f * alpha));
        bool rtl = GameText.IsRightToLeft;

        // Başlık: konuşan ve (telsizde) titreyen sinyal çubukları
        if (rtl) GameText.DrawRight(sb, _speaker, box.Right - 14, box.Y + 10, Accent * alpha, 2);
        else GameText.Draw(sb, _speaker, new Vector2(box.X + 14, box.Y + 10), Accent * alpha, 2);
        if (_kind == ChatterKind.Radio)
        {
            int bx = rtl ? box.X + 14 : box.Right - 46;
            for (int i = 0; i < 4; i++)
            {
                float flicker = 0.5f + 0.5f * MathF.Sin(time * (9f + i * 3f) + i);
                int h = 4 + i * 4;
                sb.FillRect(new Rectangle(bx + i * 8, box.Y + 26 - h, 5, h), Accent * (alpha * (0.35f + 0.65f * flicker)));
            }
        }

        // Metin daktilo gibi akar
        int visible = (int)(_age * 45f);
        float y = box.Y + 36;
        foreach (string line in _lines)
        {
            int show = Math.Clamp(visible, 0, line.Length);
            if (rtl) GameText.DrawRight(sb, line, box.Right - 14, y, Color.White * (0.92f * alpha), 2);
            else GameText.DrawPartial(sb, line, show, new Vector2(box.X + 14, y), Color.White * (0.92f * alpha), 2);
            visible -= line.Length;
            y += lineHeight;
        }
    }
}

/// <summary>Sürüş telsizinin satırları. Sıra önemlidir: aynı türde önce listedeki ilk duyulmamış satır gelir.</summary>
public static class ChatterData
{
    private static ChatterLine R(string key, int ch) => new(key, ChatterKind.Radio, ch);
    private static ChatterLine P(string key, int ch) => new(key, ChatterKind.Passenger, ch);
    private static ChatterLine M(string key, int ch) => new(key, ChatterKind.Memory, ch);

    public static readonly ChatterLine[] Episode1 =
    {
        R("chat.e1.r.1", 1), R("chat.e1.r.2", 2), R("chat.e1.r.3", 4), R("chat.e1.r.4", 5), R("chat.e1.r.5", 7),
        R("chat.e1.r.6", 8), R("chat.e1.r.7", 9), R("chat.e1.r.8", 11), R("chat.e1.r.9", 13), R("chat.e1.r.10", 14),
        R("chat.e1.r.11", 16), R("chat.e1.r.12", 19), R("chat.e1.r.13", 21), R("chat.e1.r.14", 23), R("chat.e1.r.15", 26),
        R("chat.e1.r.16", 28), R("chat.e1.r.17", 30), R("chat.e1.r.18", 32), R("chat.e1.r.19", 35), R("chat.e1.r.20", 38),

        // Kurt Baba'nın telsiz sesi (".w."): önce tanınmayan bir ses, sonra tehdit, sonra itiraf
        R("chat.e1.w.1", 7), R("chat.e1.w.2", 19), R("chat.e1.w.3", 27),

        P("chat.e1.p.1", 1), P("chat.e1.p.2", 3), P("chat.e1.p.3", 6), P("chat.e1.p.4", 8), P("chat.e1.p.5", 10),
        P("chat.e1.p.6", 12), P("chat.e1.p.7", 15), P("chat.e1.p.8", 18), P("chat.e1.p.9", 22), P("chat.e1.p.10", 27),
        P("chat.e1.p.11", 31), P("chat.e1.p.12", 36),

        M("chat.e1.m.1", 2), M("chat.e1.m.2", 4), M("chat.e1.m.3", 5), M("chat.e1.m.4", 9), M("chat.e1.m.5", 12),
        M("chat.e1.m.6", 15), M("chat.e1.m.7", 17), M("chat.e1.m.8", 20), M("chat.e1.m.9", 24), M("chat.e1.m.10", 29),
        M("chat.e1.m.11", 33), M("chat.e1.m.12", 37)
    };

    private static ChatterLine Me(string id, int n, int ch) => new($"chat.e1.pid.{id}.{n}", ChatterKind.Passenger, ch, id);

    /// <summary>
    /// Episode 1 yolcularının kişisel satırları: yalnızca o yolcu trendeyken, kendi ağzından.
    /// Her yolcunun iki satırı var; ikincisi hikâyenin ilerleyen kısmında açılır.
    /// </summary>
    public static readonly ChatterLine[] Personal =
    {
        Me("viktor", 1, 2), Me("viktor", 2, 14),
        Me("elena", 1, 3), Me("elena", 2, 20),
        Me("mika", 1, 2), Me("mika", 2, 12),
        Me("bora", 1, 4), Me("bora", 2, 16),
        Me("nadia", 1, 5), Me("nadia", 2, 22),
        Me("oskar", 1, 5), Me("oskar", 2, 18),
        Me("lina", 1, 5), Me("lina", 2, 18),
        Me("ilya", 1, 8), Me("ilya", 2, 24),
        Me("zoya", 1, 8), Me("zoya", 2, 24),
        Me("taras", 1, 8), Me("taras", 2, 28),
        Me("dmitri", 1, 4), Me("dmitri", 2, 26),
        Me("mila", 1, 4), Me("mila", 2, 26),
        Me("vera", 1, 6), Me("vera", 2, 20),
        Me("anya", 1, 6), Me("anya", 2, 30),
        Me("feliks", 1, 6), Me("feliks", 2, 20),
        Me("grigor", 1, 6), Me("grigor", 2, 20),
        Me("pavel", 1, 6), Me("pavel", 2, 20),
        Me("lev", 1, 6), Me("lev", 2, 20),
    };

    private static ChatterLine Me2(string id, int n, int ch) => new($"chat.e2.pid.{id}.{n}", ChatterKind.Passenger, ch, id);

    /// <summary>
    /// Episode 2'nin kişisel satırları: yeni yolcuların üçer satırı (hikâyeleri ilerledikçe açılır) ve
    /// Episode 1'den Sığınak-0'a ulaşıp yeniden trene binen eski yolcuların birer satırı.
    /// </summary>
    public static readonly ChatterLine[] Personal2 =
    {
        Me2("nova", 1, 3), Me2("nova", 2, 12), Me2("nova", 3, 33),
        Me2("yusuf", 1, 8), Me2("yusuf", 2, 21), Me2("yusuf", 3, 31),
        Me2("mara", 1, 12), Me2("mara", 2, 19), Me2("mara", 3, 35),
        Me2("ada", 1, 16), Me2("ada", 2, 24), Me2("ada", 3, 36),
        Me2("kiril", 1, 26), Me2("kiril", 2, 32), Me2("kiril", 3, 38),

        // Episode 1'den dönenler
        Me2("viktor", 1, 3), Me2("elena", 1, 4), Me2("mika", 1, 2), Me2("bora", 1, 6),
        Me2("nadia", 1, 9), Me2("oskar", 1, 21), Me2("lina", 1, 11),
    };

    public static readonly ChatterLine[] Episode2 =
    {
        // Telsiz: ilk saldırının panik yayınları ve AURA'nın sesi (".a." = AURA konuşur)
        R("chat.e2.r.1", 1), R("chat.e2.a.1", 1), R("chat.e2.a.2", 1), R("chat.e2.r.2", 1), R("chat.e2.r.3", 2),
        R("chat.e2.a.3", 2), R("chat.e2.a.4", 3), R("chat.e2.a.5", 5), R("chat.e2.a.6", 8), R("chat.e2.r.4", 9),
        R("chat.e2.a.7", 11), R("chat.e2.a.8", 14), R("chat.e2.r.5", 16), R("chat.e2.a.9", 18), R("chat.e2.a.10", 22),
        R("chat.e2.r.6", 24), R("chat.e2.a.11", 26), R("chat.e2.a.12", 30), R("chat.e2.a.13", 33), R("chat.e2.a.14", 36),

        P("chat.e2.p.1", 1), P("chat.e2.p.2", 2), P("chat.e2.p.3", 4), P("chat.e2.p.4", 7), P("chat.e2.p.5", 10),
        P("chat.e2.p.6", 12), P("chat.e2.p.7", 15), P("chat.e2.p.8", 19), P("chat.e2.p.9", 23), P("chat.e2.p.10", 28),
        P("chat.e2.p.11", 32), P("chat.e2.p.12", 37),

        M("chat.e2.m.1", 1), M("chat.e2.m.2", 3), M("chat.e2.m.3", 6), M("chat.e2.m.4", 9), M("chat.e2.m.5", 13),
        M("chat.e2.m.6", 17), M("chat.e2.m.7", 20), M("chat.e2.m.8", 25), M("chat.e2.m.9", 29), M("chat.e2.m.10", 33),
        M("chat.e2.m.11", 36), M("chat.e2.m.12", 39)
    };

    /// <summary>Bölüm kartındaki "Arthur'un Defteri" satırı (her iki bölümde bir; yoksa null).</summary>
    public static string LogKey(Chapter chapter)
    {
        if (chapter.Number % 2 == 0) return null;
        return chapter.Kind switch
        {
            ChapterKind.Story => $"log.1.{chapter.Number}",
            ChapterKind.Episode2 => $"log.2.{chapter.Number}",
            _ => null
        };
    }
}
