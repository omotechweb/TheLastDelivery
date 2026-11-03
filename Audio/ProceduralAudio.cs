using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;

namespace TheLastDelivery.Audio;

/// <summary>
/// Hiçbir ses dosyası kullanmadan, örnek örnek (sample) sentezlenen ses sistemi.
/// Tek bir DynamicSoundEffectInstance'a sürekli küçük tamponlar beslenir; içinde:
///   • Motor  : hızla frekansı ve şiddeti artan alçak uğultu + buhar "çuf" darbeleri
///   • Rüzgar : alçak geçiren filtreden geçirilmiş beyaz gürültü (fırtınayla güçlenir)
///   • Tonlar : uyarı bipi, tren düdüğü, menü/eşya sesleri gibi kısa efektler
/// Ses donanımı yoksa sessizce devre dışı kalır; oyun etkilenmez.
/// </summary>
public sealed class ProceduralAudio : IDisposable
{
    private const int SampleRate = 22050;
    private const int BufferSamples = 1024;       // ~46 ms
    private const int TargetPendingBuffers = 3;   // ~140 ms gecikme; takılmaya karşı tampon
    private const float Smoothing = 0.0008f;      // Parametre geçişleri tıklama yapmasın diye yumuşatılır
    private const float TwoPi = MathF.PI * 2f;
    private const int MaxTones = 64; // Müzik kutusu melodisi de ton kuyruğunu kullanır

    private enum Wave { Sine, Square, Bell }

    // Müzik kutusu şarkı paketleri: (nota frekansı, vuruş uzunluğu sn). 0 Ninni, 1 Kış Valsi, 2 Eski Dünya Marşı
    private static readonly (float Note, float Beat)[][] MusicBoxSongs =
    {
        new (float, float)[] // Ninni: yavaş, inen ezgi
        {
            (659f, 0.6f), (587f, 0.3f), (523f, 0.6f), (587f, 0.3f), (659f, 0.6f), (659f, 0.3f), (659f, 0.9f),
            (587f, 0.6f), (587f, 0.3f), (587f, 0.9f), (659f, 0.6f), (784f, 0.3f), (784f, 0.9f)
        },
        new (float, float)[] // Kış Valsi: üç zamanlı, salınan
        {
            (523f, 0.45f), (659f, 0.3f), (784f, 0.3f), (880f, 0.45f), (784f, 0.3f), (659f, 0.3f),
            (587f, 0.45f), (698f, 0.3f), (880f, 0.3f), (784f, 0.9f), (659f, 0.45f), (523f, 0.9f)
        },
        new (float, float)[] // Eski Dünya Marşı: ağır, umutlu
        {
            (392f, 0.6f), (523f, 0.6f), (523f, 0.3f), (587f, 0.3f), (659f, 0.6f), (659f, 0.6f),
            (587f, 0.3f), (523f, 0.3f), (587f, 0.6f), (659f, 0.6f), (784f, 1.2f)
        },
    };

    private sealed class Tone
    {
        public float Frequency;
        public float Frequency2;  // 0 değilse ikinci nota (akor)
        public float Volume;
        public float Duration;
        public float Delay;
        public float Vibrato;
        public float Time;
        public Wave Wave;
        public double Phase;
        public double Phase2;
    }

    /// <summary>Filtrelenmiş gürültü patlaması: patlama, buhar, pompa darbesi.</summary>
    private sealed class NoiseBurst
    {
        public float Duration;
        public float Volume;
        public float Cutoff;   // Alçak geçiren filtre katsayısı: küçük = boğuk gümbürtü, büyük = tıslama
        public float Attack;   // Yükselme süresi (buhar yavaş, patlama ani başlar)
        public float Delay;
        public float Time;
        public float LowPass;
    }

    private readonly List<NoiseBurst> _bursts = new();
    private readonly DynamicSoundEffectInstance _instance;
    private readonly byte[] _buffer = new byte[BufferSamples * 2];
    private readonly List<Tone> _tones = new();
    private readonly Random _rng = new();
    private bool _failed;

    // Anlık değerler ve hedefleri
    private float _engineLevel, _engineLevelTarget;
    private float _engineFreq = 40f, _engineFreqTarget = 40f;
    private float _chuffRate = 1f, _chuffRateTarget = 1f;
    private float _windLevel, _windLevelTarget;
    private float _windCutoff = 0.04f, _windCutoffTarget = 0.04f;
    private float _master = 0.7f, _masterTarget = 0.7f;

    // Ray tıkırtısı, fırtına uğultusu ve müzik (AudioManager sahneye göre ayarlar)
    private const float SlowSmoothing = 0.00004f; // Müzik geçişleri ~1.5 sn sürer
    private float _railLevel, _railLevelTarget;
    private float _railRate = 1f, _railRateTarget = 1f;
    private float _howlLevel, _howlLevelTarget;
    private float _musicLevel, _musicLevelTarget;
    private bool _musicIntimate;
    private float _crackle;  // Radyo/plak cızırtısı (0 = temiz)
    private float _static, _staticTarget; // Telsiz paraziti (frekans tutturulamadıkça artar)
    private float _muffle;   // Radyo hoparlörünün boğukluğu (0 = açık ses)

    // Osilatör ve filtre durumları
    private double _enginePhase;
    private double _chuffPhase;
    private float _lowPass1, _lowPass2;

    private double _railPhase;
    private float _clackTime = 1f;
    private float _railLowPass;
    private double _howlPhase;
    private float _howlTime;
    private double _musicTime;
    private readonly double[] _padPhase = new double[6];
    private double _bassPhase, _arpPhase;
    private float _musicLowPass;

    // Melankolik akor yürüyüşleri (her akor 4 sn): kasaba Am–F–C–G, taverna Am–F–Dm–E
    private static readonly float[][] TownChords =
    {
        new[] { 220.00f, 261.63f, 329.63f }, new[] { 174.61f, 220.00f, 261.63f },
        new[] { 196.00f, 261.63f, 329.63f }, new[] { 196.00f, 246.94f, 293.66f }
    };
    private static readonly float[][] TavernChords =
    {
        new[] { 220.00f, 261.63f, 329.63f }, new[] { 174.61f, 220.00f, 261.63f },
        new[] { 146.83f, 174.61f, 220.00f }, new[] { 164.81f, 207.65f, 246.94f }
    };
    private static readonly int[] ArpPattern = { 0, 1, 2, 1, 2, 0, 1, 2 };

    public ProceduralAudio()
    {
        try
        {
            _instance = new DynamicSoundEffectInstance(SampleRate, AudioChannels.Mono);
        }
        catch (Exception)
        {
            _instance = null; // Ses aygıtı yok (ör. sunucu, sürücü sorunu): oyun sessiz devam eder
        }
    }

    public bool IsAvailable => _instance != null && !_failed;

    // ------------------------------------------------------------------
    // Sürekli sesler
    // ------------------------------------------------------------------

    /// <param name="speedRatio">0-1 arası hız oranı.</param>
    /// <param name="running">Kazan sönmüşse motor susar.</param>
    public void SetEngine(float speedRatio, bool running)
    {
        _engineLevelTarget = running ? 0.10f + 0.14f * speedRatio : 0f;
        _engineFreqTarget = 38f + 40f * speedRatio;
        _chuffRateTarget = 0.8f + 7f * speedRatio;
    }

    /// <param name="intensity">Fırtına yoğunluğu (0-1).</param>
    /// <param name="gust">Anlık esinti (0-1).</param>
    public void SetWind(float intensity, float gust)
    {
        _windLevelTarget = 0.04f + 0.22f * intensity * (0.6f + 0.4f * gust);
        _windCutoffTarget = 0.02f + 0.03f * intensity + 0.08f * gust * intensity;
    }

    /// <summary>Raylardan gelen ritmik "taka-tak": hız arttıkça sıklaşır.</summary>
    public void SetRails(float speedRatio, bool rolling)
    {
        _railLevelTarget = rolling && speedRatio > 0.01f ? 0.10f + 0.12f * speedRatio : 0f;
        _railRateTarget = 0.6f + 4.4f * speedRatio;
    }

    /// <summary>Fırtına uğultusu: tipide dalgalanan, ıslık gibi rüzgar sesi.</summary>
    public void SetStormHowl(float level) => _howlLevelTarget = Math.Clamp(level, 0f, 1f) * 0.12f;

    /// <summary>Synth müzik: 0 = sessiz. <paramref name="intimate"/>: taverna için daha yavaş, daha hüzünlü yürüyüş.</summary>
    public void SetMusic(float level, bool intimate)
    {
        _musicLevelTarget = Math.Clamp(level, 0f, 1f) * 0.55f;
        _musicIntimate = intimate;
    }

    /// <summary>Müziği bir radyodan geliyormuş gibi renklendirir: cızırtı ve boğukluk (0 = temiz stüdyo sesi).</summary>
    /// <summary>Telsiz paraziti: 0 = temiz sinyal, 1 = yalnızca hışırtı.</summary>
    public void SetStatic(float level) => _staticTarget = Math.Clamp(level, 0f, 1f) * 0.12f;

    public void SetRadio(float crackle, float muffle)
    {
        _crackle = Math.Clamp(crackle, 0f, 1f);
        _muffle = Math.Clamp(muffle, 0f, 1f);
    }

    /// <summary>Kazana kömür atıldı / soba körüklendi: boğuk bir küreğin gümlemesi ve alevin hışırtısı.</summary>
    public void PlayShovel()
    {
        AddBurst(0.12f, 0.3f, 0.08f, 0.002f);
        AddBurst(0.6f, 0.18f, 0.25f, 0.15f);
    }

    /// <summary>Müzik kutusu: çan tınılı ezgi iki kez çalar (şarkı paketi kozmetikten gelir).</summary>
    public void PlayMusicBox(int song)
    {
        var melody = MusicBoxSongs[Math.Clamp(song, 0, MusicBoxSongs.Length - 1)];
        float delay = 0f;
        for (int repeat = 0; repeat < 2; repeat++)
        {
            foreach (var (note, beat) in melody)
            {
                AddTone(note, 0f, 0.12f, beat * 1.6f, delay, Wave.Bell);
                delay += beat;
            }
            delay += 0.6f;
        }
    }

    /// <summary>Köpek havlaması: iki kısa, boğuk ve sert "hav".</summary>
    public void PlayBark()
    {
        for (int i = 0; i < 2; i++)
        {
            AddBurst(0.09f, 0.35f, 0.18f, 0.004f);
            AddTone(260f, 180f, 0.16f, 0.1f, i * 0.22f, Wave.Square);
        }
    }

    /// <summary>Kedi miyavlaması: yükselip alçalan tiz bir ses.</summary>
    public void PlayMeow()
    {
        AddTone(620f, 0f, 0.1f, 0.18f, 0f, Wave.Sine, vibrato: 0.04f);
        AddTone(820f, 0f, 0.09f, 0.22f, 0.15f, Wave.Sine, vibrato: 0.06f);
        AddTone(560f, 0f, 0.07f, 0.2f, 0.34f, Wave.Sine, vibrato: 0.03f);
    }

    /// <summary>Fotoğraf makinesi deklanşörü.</summary>
    public void PlayShutter()
    {
        AddBurst(0.04f, 0.4f, 0.6f, 0.001f);
        AddBurst(0.06f, 0.3f, 0.4f, 0.001f);
    }

    /// <summary>Telsiz: kısa mors bipleri (SOS: · · · — — — · · ·).</summary>
    public void PlayMorseSos()
    {
        float delay = 0f;
        foreach (char c in "...---...")
        {
            float length = c == '.' ? 0.08f : 0.24f;
            AddTone(760f, 0f, 0.07f, length, delay, Wave.Sine);
            delay += length + 0.08f;
        }
    }

    /// <summary>Telsiz: boğuk, kesik konuşma sesi (mafya frekansı).</summary>
    public void PlayRadioChatter()
    {
        for (int i = 0; i < 5; i++)
            AddBurst(0.12f + (float)_rng.NextDouble() * 0.15f, 0.12f, 0.12f, 0.01f);
        AddTone(1200f, 0f, 0.05f, 0.05f, 0.9f, Wave.Square); // Telsiz "klik"
    }

    /// <summary>Sanrı: yükselen, perdesi kayan rahatsız edici bir uğultu.</summary>
    public void PlayEerie()
    {
        AddTone(110f, 117f, 0.12f, 2.2f, 0f, Wave.Sine, vibrato: 0.03f);
        AddTone(233f, 0f, 0.05f, 1.6f, 0.4f, Wave.Sine, vibrato: 0.05f);
    }

    /// <summary>Fener tamiri: metale vuran küçük bir anahtar sesi.</summary>
    public void PlayTink() => AddTone(2200f, 3300f, 0.08f, 0.07f, 0f, Wave.Sine);

    /// <summary>Duraklatıldığında her şey kısılır ama tamamen susmaz.</summary>
    public void SetPaused(bool paused) => _masterTarget = paused ? 0.2f : 0.7f;

    // ------------------------------------------------------------------
    // Kısa efektler (oyunun kritik anlarından çağrılır)
    // ------------------------------------------------------------------

    /// <summary>Rastgele olay: kısa çift bip.</summary>
    public void PlayAlarm()
    {
        AddTone(880f, 0f, 0.16f, 0.10f, 0f, Wave.Square);
        AddTone(880f, 0f, 0.16f, 0.10f, 0.16f, Wave.Square);
    }

    /// <summary>İstasyona varış / kalkış: iki notalı, titreşimli tren düdüğü.</summary>
    public void PlayWhistle()
    {
        AddTone(523f, 659f, 0.20f, 0.9f, 0f, Wave.Sine, vibrato: 0.012f);
    }

    /// <summary>Menü onayı, seçim yapma, eşya kullanma.</summary>
    public void PlayConfirm()
    {
        AddTone(660f, 0f, 0.14f, 0.06f, 0f, Wave.Sine);
        AddTone(880f, 0f, 0.14f, 0.09f, 0.06f, Wave.Sine);
    }

    /// <summary>Duraklatma gibi hafif arayüz tıklaması.</summary>
    public void PlayClick() => AddTone(1200f, 0f, 0.10f, 0.035f, 0f, Wave.Sine);

    /// <summary>Başarısız işlem (eşya yok, envanter dolu, olay kaçırıldı).</summary>
    public void PlayError() => AddTone(180f, 0f, 0.13f, 0.18f, 0f, Wave.Square);

    /// <summary>Başarılı olay sonucu: yükselen arpej.</summary>
    public void PlaySuccess()
    {
        AddTone(523f, 0f, 0.13f, 0.09f, 0f, Wave.Sine);
        AddTone(659f, 0f, 0.13f, 0.09f, 0.09f, Wave.Sine);
        AddTone(784f, 0f, 0.13f, 0.14f, 0.18f, Wave.Sine);
    }

    /// <summary>Ölüm / kayıp: alçalan hüzünlü üç nota.</summary>
    public void PlayDeath()
    {
        AddTone(330f, 0f, 0.18f, 0.35f, 0f, Wave.Sine);
        AddTone(262f, 0f, 0.18f, 0.35f, 0.35f, Wave.Sine);
        AddTone(196f, 0f, 0.18f, 0.60f, 0.70f, Wave.Sine);
    }

    /// <summary>Kovalamaca: çarpışma ya da patlama — derin gümbürtü.</summary>
    public void PlayExplosion()
    {
        AddBurst(0.7f, 0.55f, 0.05f, 0.005f);
        AddTone(55f, 0f, 0.25f, 0.45f, 0f, Wave.Sine);
    }

    /// <summary>Buhar püskürtme: yükselip sönen keskin tıslama.</summary>
    public void PlaySteamBlast() => AddBurst(0.8f, 0.35f, 0.45f, 0.08f);

    /// <summary>Benzinlik pompası: her ritmik basışta kısa bir metal darbesi.</summary>
    public void PlayPump()
    {
        AddBurst(0.09f, 0.25f, 0.15f, 0.003f);
        AddTone(95f, 0f, 0.18f, 0.08f, 0f, Wave.Sine);
    }

    /// <summary>Satış / hurda kazanma: iki kısa parlak nota.</summary>
    public void PlayCoin()
    {
        AddTone(1320f, 0f, 0.10f, 0.06f, 0f, Wave.Sine);
        AddTone(1760f, 0f, 0.10f, 0.12f, 0.06f, Wave.Sine);
    }

    // ------------------------------------------------------------------
    // Tampon besleme
    // ------------------------------------------------------------------

    /// <summary>Her karede çağrılır; çalınmayı bekleyen tampon sayısını hedefte tutar.</summary>
    public void Update()
    {
        if (!IsAvailable) return;

        try
        {
            while (_instance.PendingBufferCount < TargetPendingBuffers)
            {
                FillBuffer();
                _instance.SubmitBuffer(_buffer);
            }

            if (_instance.State != SoundState.Playing)
                _instance.Play();
        }
        catch (Exception)
        {
            _failed = true; // Ses aygıtı oyun sırasında kaybolursa sessiz moda geç
        }
    }

    private void FillBuffer()
    {
        for (int i = 0; i < BufferSamples; i++)
        {
            SmoothParameters();
            float noise = (float)(_rng.NextDouble() * 2.0 - 1.0);

            _static += (_staticTarget - _static) * Smoothing;
            float mix = (EngineSample(noise) + WindSample(noise) + RailSample(noise) + HowlSample()
                         + MusicSample() + MixTones() + MixBursts(noise) + noise * _static) * _master;
            mix = Math.Clamp(mix, -1f, 1f);

            short sample = (short)(mix * short.MaxValue);
            _buffer[i * 2] = (byte)(sample & 0xFF);
            _buffer[i * 2 + 1] = (byte)((sample >> 8) & 0xFF);
        }
    }

    private void SmoothParameters()
    {
        _engineLevel += (_engineLevelTarget - _engineLevel) * Smoothing;
        _engineFreq += (_engineFreqTarget - _engineFreq) * Smoothing;
        _chuffRate += (_chuffRateTarget - _chuffRate) * Smoothing;
        _windLevel += (_windLevelTarget - _windLevel) * Smoothing;
        _windCutoff += (_windCutoffTarget - _windCutoff) * Smoothing;
        _master += (_masterTarget - _master) * Smoothing;
        _railLevel += (_railLevelTarget - _railLevel) * Smoothing;
        _railRate += (_railRateTarget - _railRate) * Smoothing;
        _howlLevel += (_howlLevelTarget - _howlLevel) * SlowSmoothing * 4f;
        _musicLevel += (_musicLevelTarget - _musicLevel) * SlowSmoothing;
    }

    /// <summary>Ray ekleri: her geçişte iki kısa metalik tık (ön ve arka boji).</summary>
    private float RailSample(float noise)
    {
        if (_railLevel < 0.0005f) return 0f;
        const float dt = 1f / SampleRate;

        _railPhase += _railRate / SampleRate;
        if (_railPhase >= 1.0)
        {
            _railPhase -= 1.0;
            _clackTime = 0f;
        }
        _clackTime += dt;

        _railLowPass += (noise - _railLowPass) * 0.2f;
        float hiss = noise - _railLowPass; // Yüksek geçiren gürültü: metalik tık
        float Click(float t) => t < 0f ? 0f
            : MathF.Exp(-t * 110f) * (hiss * 0.6f + MathF.Sin(TwoPi * 1350f * t) * 0.35f + MathF.Sin(TwoPi * 180f * t) * 0.4f);

        return (Click(_clackTime) + Click(_clackTime - 0.085f) * 0.8f) * _railLevel;
    }

    /// <summary>Fırtına uğultusu: perdesi yavaşça dalgalanan, gürültüyle pürüzlenmiş ıslık.</summary>
    private float HowlSample()
    {
        if (_howlLevel < 0.0005f) return 0f;
        _howlTime += 1f / SampleRate;
        float pitch = 320f + 140f * MathF.Sin(_howlTime * 0.6f) + 60f * MathF.Sin(_howlTime * 1.7f);
        _howlPhase = (_howlPhase + pitch / SampleRate) % 1.0;
        float body = MathF.Sin(TwoPi * (float)_howlPhase) * (0.5f + 6f * MathF.Abs(_lowPass2));
        return body * _howlLevel * (0.6f + 0.4f * MathF.Sin(_howlTime * 0.9f));
    }

    /// <summary>
    /// Sıcak, melankolik synth: yumuşak akor pedi + bas + yavaşça sönen arpej. Faz süreklidir,
    /// akor değişimleri tıklama yapmaz; çıkış hafif bir alçak geçirenden geçer.
    /// </summary>
    private float MusicSample()
    {
        if (_musicLevel < 0.0005f) return 0f;

        double chordLength = _musicIntimate ? 5.0 : 4.0;
        _musicTime += 1.0 / SampleRate;
        var chords = _musicIntimate ? TavernChords : TownChords;
        int chordIndex = (int)(_musicTime / chordLength) % chords.Length;
        float inChord = (float)(_musicTime % chordLength);
        var chord = chords[chordIndex];

        // Ped: her nota iki hafif akort dışı sinüs (koro etkisi); akor boyunca nefes alır gibi kabarır
        float swell = 0.45f + 0.55f * MathF.Sin(MathF.PI * inChord / (float)chordLength);
        float pad = 0f;
        for (int i = 0; i < 3; i++)
        {
            _padPhase[i * 2] = (_padPhase[i * 2] + chord[i] / SampleRate) % 1.0;
            _padPhase[i * 2 + 1] = (_padPhase[i * 2 + 1] + chord[i] * 1.0045 / SampleRate) % 1.0;
            pad += MathF.Sin(TwoPi * (float)_padPhase[i * 2]) + MathF.Sin(TwoPi * (float)_padPhase[i * 2 + 1]);
        }
        pad *= 0.045f * swell;

        _bassPhase = (_bassPhase + chord[0] * 0.5f / SampleRate) % 1.0;
        float bass = MathF.Sin(TwoPi * (float)_bassPhase) * 0.09f * swell;

        // Arpej: akor notaları bir oktav yukarıda, her vuruşta yumuşak atak ve uzun sönüş
        double step = _musicIntimate ? 0.625 : 0.5;
        int stepIndex = (int)(_musicTime / step);
        float inStep = (float)(_musicTime % step);
        float note = chord[ArpPattern[stepIndex % ArpPattern.Length]] * 2f;
        _arpPhase = (_arpPhase + note / SampleRate) % 1.0;
        float arpEnvelope = MathF.Min(1f, inStep / 0.015f) * MathF.Exp(-inStep * (_musicIntimate ? 3f : 4.5f));
        float arpPhase = TwoPi * (float)_arpPhase;
        float arp = (MathF.Sin(arpPhase) + 0.25f * MathF.Sin(2f * arpPhase)) * 0.07f * arpEnvelope;

        float music = pad + bass + arp;
        _musicLowPass += (music - _musicLowPass) * (0.3f * (1f - 0.75f * _muffle)); // Radyo hoparlörü tizleri keser

        // Radyo/plak cızırtısı: seyrek çıtırtılar ve hafif hışırtı
        float crackle = 0f;
        if (_crackle > 0f)
        {
            if (_rng.NextDouble() < _crackle * 0.0015) crackle = ((float)_rng.NextDouble() * 2f - 1f) * 0.5f;
            crackle += ((float)_rng.NextDouble() * 2f - 1f) * 0.015f * _crackle;
        }
        return (_musicLowPass * 2f + crackle) * _musicLevel;
    }

    /// <summary>Motor: sinüs + harmonik + testere dişi uğultu, üstünde periyodik buhar darbesi.</summary>
    private float EngineSample(float noise)
    {
        _enginePhase += _engineFreq / SampleRate;
        if (_enginePhase >= 1.0) _enginePhase -= 1.0;
        float p = (float)_enginePhase;
        float hum = MathF.Sin(TwoPi * p) * 0.6f + MathF.Sin(TwoPi * 2f * p) * 0.2f + (2f * p - 1f) * 0.2f;

        // "Çuf": her döngünün başında hızla sönen gürültü patlaması
        _chuffPhase += _chuffRate / SampleRate;
        if (_chuffPhase >= 1.0) _chuffPhase -= 1.0;
        float puff = MathF.Exp(-(float)_chuffPhase * 9f);

        return (hum * (0.6f + 0.4f * puff) + noise * 0.35f * puff) * _engineLevel;
    }

    /// <summary>Rüzgar: iki kademeli alçak geçiren filtreden geçmiş beyaz gürültü.</summary>
    private float WindSample(float noise)
    {
        _lowPass1 += (noise - _lowPass1) * _windCutoff;
        _lowPass2 += (_lowPass1 - _lowPass2) * _windCutoff;
        return _lowPass2 * 4f * _windLevel;
    }

    private float MixTones()
    {
        const float dt = 1f / SampleRate;
        float sum = 0f;

        for (int i = _tones.Count - 1; i >= 0; i--)
        {
            var tone = _tones[i];
            if (tone.Delay > 0f)
            {
                tone.Delay -= dt;
                continue;
            }

            tone.Time += dt;
            if (tone.Time >= tone.Duration)
            {
                _tones.RemoveAt(i);
                continue;
            }

            float vibrato = 1f + tone.Vibrato * MathF.Sin(TwoPi * 5f * tone.Time);
            tone.Phase = (tone.Phase + tone.Frequency * vibrato / SampleRate) % 1.0;
            float value = Oscillate(tone.Wave, tone.Phase);

            if (tone.Frequency2 > 0f)
            {
                tone.Phase2 = (tone.Phase2 + tone.Frequency2 * vibrato / SampleRate) % 1.0;
                value = (value + Oscillate(tone.Wave, tone.Phase2)) * 0.5f;
            }

            // Kısa atak ve sönüş: tıklamasız başlangıç/bitiş; çan tınısı üstelik üstel olarak söner
            float envelope = MathF.Min(1f, tone.Time / 0.008f) * MathF.Min(1f, (tone.Duration - tone.Time) / 0.04f);
            if (tone.Wave == Wave.Bell) envelope *= MathF.Exp(-tone.Time * 3.2f);
            sum += value * envelope * tone.Volume;
        }
        return sum;
    }

    private float MixBursts(float noise)
    {
        const float dt = 1f / SampleRate;
        float sum = 0f;

        for (int i = _bursts.Count - 1; i >= 0; i--)
        {
            var burst = _bursts[i];
            if (burst.Delay > 0f)
            {
                burst.Delay -= dt;
                continue;
            }

            burst.Time += dt;
            if (burst.Time >= burst.Duration)
            {
                _bursts.RemoveAt(i);
                continue;
            }

            burst.LowPass += (noise - burst.LowPass) * burst.Cutoff;
            float attack = MathF.Min(1f, burst.Time / burst.Attack);
            float decay = MathF.Exp(-4f * burst.Time / burst.Duration);
            sum += burst.LowPass * attack * decay * burst.Volume * 2.5f;
        }
        return sum;
    }

    private void AddBurst(float duration, float volume, float cutoff, float attack)
    {
        if (!IsAvailable || _bursts.Count >= 8) return;
        _bursts.Add(new NoiseBurst { Duration = duration, Volume = volume, Cutoff = cutoff, Attack = attack });
    }

    private static float Oscillate(Wave wave, double phase) => wave switch
    {
        Wave.Sine => MathF.Sin(TwoPi * (float)phase),
        Wave.Bell => MathF.Sin(TwoPi * (float)phase) + 0.35f * MathF.Sin(TwoPi * 2.76f * (float)phase), // Metalik üst ton
        _ => phase < 0.5 ? 0.6f : -0.6f
    };

    private void AddTone(float frequency, float frequency2, float volume, float duration, float delay,
        Wave wave, float vibrato = 0f)
    {
        if (!IsAvailable || _tones.Count >= MaxTones) return;

        _tones.Add(new Tone
        {
            Frequency = frequency,
            Frequency2 = frequency2,
            Volume = volume,
            Duration = duration,
            Delay = delay,
            Wave = wave,
            Vibrato = vibrato
        });
    }

    public void Dispose()
    {
        if (_instance == null) return;
        try
        {
            _instance.Stop();
            _instance.Dispose();
        }
        catch (Exception)
        {
            // Kapanışta ses aygıtı zaten gitmiş olabilir
        }
    }
}
