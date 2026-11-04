using System;
using System.Collections.Generic;
using TheLastDelivery.Core;

namespace TheLastDelivery.World;

public enum RadioBand { FM, AM }

/// <summary>Telsizde yakalanabilecek yayın türleri.</summary>
public enum RadioChannelKind
{
    Music, // Eski dünya şarkıları: dinlemek akıl sağlığını toparlar
    Mafia, // Kızıl Kurtlar'ın telsiz trafiği: pusu istihbaratı ve "paketi bize sat" teklifi
    Sos,      // İmdat çağrısı: yanıt verilirse yan rota görevi başlar
    Broadcast, // Hikâye yayını (panik anonsları, eski kayıtlar): dinlenince kaset olarak arşive girer
    Phantom   // AURA'nın ses taklidi: ölü yakınların sesi; dinlemek akıl sağlığını aşındırır, yanıtlamak diyalog açar
}

/// <summary>İmdat görevinin türü.</summary>
public enum SosMission { MilitaryTrain, ShelterSearch }

public sealed record RadioChannel(RadioChannelKind Kind, RadioBand Band, float Frequency)
{
    /// <summary>Yayın metninin anahtarı (Broadcast: "bc.*", Phantom: "ph.*").</summary>
    public string Key { get; init; }
}

/// <summary>
/// Kabindeki telsiz. Her bölümde yayınlar farklı frekanslara dağılır; oyuncu FM/AM bantlarında
/// kadranı çevirerek arar. Sinyal gücü frekans yakınlığından hesaplanır (yakın değilse parazit).
/// </summary>
public sealed class RadioSystem
{
    private const float FmTolerance = 0.6f;  // MHz
    private const float AmTolerance = 40f;   // kHz

    private readonly List<RadioChannel> _channels = new();
    private Random _rng = new();
    private float _fm = 98.0f, _am = 900f;

    public RadioBand Band { get; private set; } = RadioBand.FM;
    public float Frequency => Band == RadioBand.FM ? _fm : _am;
    public IReadOnlyList<RadioChannel> Channels => _channels;

    /// <summary>Bu bölümdeki imdat çağrısının türü ve yanıtlanıp yanıtlanmadığı.</summary>
    public SosMission SosType { get; private set; }
    public bool SosAnswered { get; set; }

    /// <summary>Mafya teklifi bu yolculukta duyuldu mu? (Final seçeneklerini açar)</summary>
    public bool MafiaOfferHeard { get; set; }

    /// <summary>Yeni bölüm: yayınlar yeni frekanslara dağılır. İmdat çağrısı yalnızca istenirse yayında olur.</summary>
    public void Retune(int seed, bool withSos)
    {
        _rng = new Random(seed * 7919 + 17);
        _channels.Clear();
        _channels.Add(new RadioChannel(RadioChannelKind.Music, RadioBand.FM, RoundFm(89f + (float)_rng.NextDouble() * 17f)));
        _channels.Add(new RadioChannel(RadioChannelKind.Mafia, RadioBand.AM, RoundAm(600f + (float)_rng.NextDouble() * 900f)));
        if (withSos)
        {
            var band = _rng.Next(2) == 0 ? RadioBand.FM : RadioBand.AM;
            float freq = band == RadioBand.FM ? RoundFm(89f + (float)_rng.NextDouble() * 17f) : RoundAm(560f + (float)_rng.NextDouble() * 1000f);
            // Müzikle çakışmasın
            if (band == RadioBand.FM && MathF.Abs(freq - _channels[0].Frequency) < FmTolerance * 2f)
                freq = RoundFm(freq > 98f ? freq - 5f : freq + 5f);
            _channels.Add(new RadioChannel(RadioChannelKind.Sos, band, freq));
            SosType = (SosMission)_rng.Next(2);
            SosAnswered = false;
        }
    }

    /// <summary>Derin anlatı: hikâye yayını ya da AURA'nın ses taklidi kanalını ekler (aynı türden eskisi silinir).</summary>
    public void SetChannel(RadioChannelKind kind, string key)
    {
        _channels.RemoveAll(c => c.Kind == kind);
        if (key == null) return;
        for (int attempt = 0; attempt < 8; attempt++)
        {
            var band = _rng.Next(2) == 0 ? RadioBand.FM : RadioBand.AM;
            float freq = band == RadioBand.FM ? RoundFm(89f + (float)_rng.NextDouble() * 17f) : RoundAm(560f + (float)_rng.NextDouble() * 1000f);
            bool clash = _channels.Exists(c => c.Band == band && MathF.Abs(c.Frequency - freq) < (band == RadioBand.FM ? FmTolerance * 2.5f : AmTolerance * 2.5f));
            if (clash) continue;
            _channels.Add(new RadioChannel(kind, band, freq) { Key = key });
            return;
        }
    }

    public bool HasChannel(RadioChannelKind kind) => _channels.Exists(c => c.Kind == kind);

    public void SwitchBand() => Band = Band == RadioBand.FM ? RadioBand.AM : RadioBand.FM;

    /// <summary>Kadranı çevirir (adım sayısı; negatif = geri).</summary>
    public void Tune(int steps)
    {
        if (Band == RadioBand.FM)
            _fm = Math.Clamp(RoundFm(_fm + steps * GameConfig.FmStep), GameConfig.FmMin, GameConfig.FmMax);
        else
            _am = Math.Clamp(RoundAm(_am + steps * GameConfig.AmStep), GameConfig.AmMin, GameConfig.AmMax);
    }

    /// <summary>Şu an yakalanan yayın (yoksa null) ve 0-1 sinyal gücü.</summary>
    public RadioChannel Tuned(out float signal)
    {
        signal = 0f;
        RadioChannel best = null;
        float tolerance = Band == RadioBand.FM ? FmTolerance : AmTolerance;
        foreach (var channel in _channels)
        {
            if (channel.Band != Band) continue;
            if (channel.Kind == RadioChannelKind.Sos && SosAnswered) continue; // Yanıtlanan çağrı susar
            float s = 1f - MathF.Abs(channel.Frequency - Frequency) / tolerance;
            if (s > signal)
            {
                signal = s;
                best = channel;
            }
        }
        signal = Math.Clamp(signal, 0f, 1f);
        return signal > 0f ? best : null;
    }

    /// <summary>0-1 kadran konumu (çizim için).</summary>
    public float DialPosition => Band == RadioBand.FM
        ? (_fm - GameConfig.FmMin) / (GameConfig.FmMax - GameConfig.FmMin)
        : (_am - GameConfig.AmMin) / (GameConfig.AmMax - GameConfig.AmMin);

    private static float RoundFm(float f) => MathF.Round(f * 10f) / 10f;
    private static float RoundAm(float f) => MathF.Round(f / 10f) * 10f;
}
