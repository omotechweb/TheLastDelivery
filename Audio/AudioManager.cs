using System;

namespace TheLastDelivery.Audio;

/// <summary>Sesin hangi "sahneye" göre karıştırılacağı.</summary>
public enum AudioScene
{
    Menu,     // Ana menü ve dil seçimi: hafif müzik, rüzgar
    Driving,  // Yolda: motor, ray tıkırtısı, rüzgar ve fırtına uğultusu; müzik yok
    Town,     // Kasaba sokağı: sıcak synth müzik, kısık rüzgar
    Tavern,   // Taverna/sığınak içi: daha yakın, yavaş ve hüzünlü müzik; dışarının sesi boğuk
    Station,  // Durak / benzinlik / ana istasyon: motor rölantide, rüzgar, hafif müzik
    Mining,   // Maden: yalnızca boğuk rüzgar
    Ending,   // Final ekranı: müzik
    Cabin,    // Kabin menüsü açık: kabindeki radyo çalar (radyo modeline göre ses)
    Radio,    // Telsiz açık: frekansa göre parazit, eski şarkılar, mors ve konuşmalar
    Soundtrack // Bonuslar ekranı: seçilen soundtrack parçası (Supporter Pack)
}

/// <summary>
/// Oyunun ses yöneticisi. Kısa efektler <see cref="Sfx"/> (ProceduralAudio) üzerinden çalınır;
/// sürekli katmanlar (motor, ray, rüzgar, fırtına uğultusu, müzik) her karede sahneye göre
/// hedef seviyelere çekilir ve sentezleyici bunlar arasında yumuşak geçiş yapar.
/// Ses aygıtı yoksa her şey sessizce devre dışı kalır.
/// </summary>
public sealed class AudioManager : IDisposable
{
    public AudioManager(ProceduralAudio sfx) => Sfx = sfx;

    /// <summary>Kısa efektler (düdük, alarm, buhar, para...) için sentezleyici.</summary>
    public ProceduralAudio Sfx { get; }

    public AudioScene Scene { get; private set; } = AudioScene.Menu;

    public void SetScene(AudioScene scene) => Scene = scene;

    /// <summary>Kabindeki radyo modeli (0 lambalı radyo, 1 askeri saha telsizi, 2 gramofon).</summary>
    public int RadioModel { get; set; }

    /// <summary>Telsizde yakalanan yayının sinyal gücü (0-1) ve müzik istasyonu olup olmadığı.</summary>
    public float RadioSignal { get; set; }
    public bool RadioMusic { get; set; }

    /// <summary>Bonuslar ekranında çalan soundtrack parçası (-1 = yok).</summary>
    public int SoundtrackTrack { get; set; } = -1;

    /// <summary>Dijital soundtrack'in parçaları (adları çeviri tablosunda "ost.{i}").</summary>
    public const int SoundtrackLength = 4;

    /// <summary>Her karede çağrılır: sahneye ve dünya durumuna göre katman seviyelerini ayarlar.</summary>
    /// <param name="speedRatio">Trenin 0-1 hız oranı.</param>
    /// <param name="engineRunning">Kazan yanıyor mu? (finalde susar)</param>
    /// <param name="storm">Fırtına şiddeti (0-1).</param>
    /// <param name="gust">Anlık esinti (0-1).</param>
    /// <param name="blizzard">Tipi olayı sürüyor mu?</param>
    public void Update(float speedRatio, bool engineRunning, float storm, float gust, bool blizzard, bool paused)
    {
        if (Scene == AudioScene.Soundtrack && SoundtrackTrack >= 0)
        {
            PlaySoundtrack(paused);
            return;
        }

        if (Scene == AudioScene.Radio)
        {
            // Frekans tutturulamadıkça hışırtı; müzik istasyonunda boğuk, cızırtılı eski şarkılar
            Sfx.SetEngine(speedRatio * 0.4f, engineRunning);
            Sfx.SetRails(speedRatio * 0.3f, engineRunning);
            Sfx.SetWind(storm * 0.3f, gust);
            Sfx.SetStormHowl(0f);
            Sfx.SetStatic(1f - RadioSignal * 0.85f);
            Sfx.SetRadio(0.3f + 0.5f * (1f - RadioSignal), 0.7f);
            Sfx.SetMusic(RadioMusic ? RadioSignal : 0f, intimate: false);
            Sfx.SetPaused(paused);
            Sfx.Update();
            return;
        }
        Sfx.SetStatic(0f);

        if (Scene == AudioScene.Cabin)
        {
            // Kabin radyosu: model başına farklı cızırtı ve tını; dışarıdaki rüzgar boğuk duyulur
            Sfx.SetEngine(speedRatio * 0.5f, engineRunning);
            Sfx.SetRails(speedRatio * 0.5f, engineRunning);
            Sfx.SetWind(storm * 0.3f, gust);
            Sfx.SetStormHowl(0f);
            switch (RadioModel)
            {
                case 1: Sfx.SetRadio(0.7f, 0.8f); Sfx.SetMusic(0.75f, intimate: false); break; // Saha telsizi: cızırtılı, boğuk
                case 2: Sfx.SetRadio(0.45f, 0.35f); Sfx.SetMusic(0.9f, intimate: true); break;  // Gramofon: plak çıtırtısı
                default: Sfx.SetRadio(0.25f, 0.55f); Sfx.SetMusic(0.8f, intimate: true); break; // Lambalı radyo
            }
            Sfx.SetPaused(paused);
            Sfx.Update();
            return;
        }

        Sfx.SetRadio(0f, 0f);
        bool road = Scene == AudioScene.Driving;
        bool indoors = Scene is AudioScene.Tavern or AudioScene.Mining;

        Sfx.SetEngine(speedRatio, engineRunning && Scene != AudioScene.Ending);
        Sfx.SetRails(speedRatio, road && engineRunning);

        float wind = indoors ? storm * 0.25f : Scene == AudioScene.Town ? storm * 0.55f : storm;
        Sfx.SetWind(wind, gust);

        float howl = road ? Math.Clamp((storm - 0.45f) * 1.8f, 0f, 1f) + (blizzard ? 0.6f : 0f) : 0f;
        Sfx.SetStormHowl(howl);

        float music = Scene switch
        {
            AudioScene.Menu => 0.55f,
            AudioScene.Town => 0.75f,
            AudioScene.Tavern => 1f,
            AudioScene.Station => 0.35f,
            AudioScene.Ending => 0.7f,
            _ => 0f
        };
        Sfx.SetMusic(music, intimate: Scene is AudioScene.Tavern or AudioScene.Ending);

        Sfx.SetPaused(paused);
        Sfx.Update();
    }

    /// <summary>
    /// Soundtrack çalar: 0 "Old Smokie" (tren ritmi + kasaba teması), 1 "Kırağı Köyü" (kasaba),
    /// 2 "Son Fener Tavernası" (taverna), 3 "Kuzey Fırtınası" (fırtına uğultusu + yavaş ped).
    /// </summary>
    private void PlaySoundtrack(bool paused)
    {
        Sfx.SetRadio(0f, 0f);
        bool train = SoundtrackTrack == 0;
        bool storm = SoundtrackTrack == 3;
        Sfx.SetEngine(train ? 0.45f : 0f, train);
        Sfx.SetRails(train ? 0.45f : 0f, train);
        Sfx.SetWind(storm ? 0.9f : 0.1f, 0.7f);
        Sfx.SetStormHowl(storm ? 0.8f : 0f);
        Sfx.SetMusic(storm ? 0.45f : 1f, intimate: SoundtrackTrack is 2 or 3);
        Sfx.SetPaused(paused);
        Sfx.Update();
    }

    public void Dispose() => Sfx?.Dispose();
}
