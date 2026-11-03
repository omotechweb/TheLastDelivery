using System;
using TheLastDelivery.Core;

namespace TheLastDelivery.Trade;

/// <summary>
/// Benzinlik pompası: oyuncu SPACE'e ritmik basarak basınç ibresini yeşil bölgede tutar.
/// Her basış basıncı yükseltir, basınç kendiliğinden düşer; ibre yeşildeyken depo dolar.
/// Süre bittiğinde dolum oranı kadar yakıt alınır (en az PumpMinimumYield).
/// </summary>
public sealed class PumpMinigame
{
    public float Pressure { get; private set; }
    public float Fill { get; private set; }
    public float Time { get; private set; }
    public bool IsDone { get; private set; }

    public bool InGreen => Pressure >= GameConfig.PumpGreenMin && Pressure <= GameConfig.PumpGreenMax;

    public float TimeLeft => MathF.Max(0f, GameConfig.PumpDuration - Time);

    /// <summary>Alınacak yakıt oranı (0.3 - 1).</summary>
    public float Yield => Math.Clamp(MathF.Max(Fill, GameConfig.PumpMinimumYield), 0f, 1f);

    public void Start()
    {
        Pressure = 0.2f;
        Fill = 0f;
        Time = 0f;
        IsDone = false;
    }

    public void Update(float dt, bool tapped)
    {
        if (IsDone) return;

        Time += dt;
        Pressure -= GameConfig.PumpDecayPerSecond * dt;
        if (tapped) Pressure += GameConfig.PumpTapBoost;
        Pressure = Math.Clamp(Pressure, 0f, 1.05f);

        if (InGreen)
            Fill = MathF.Min(1f, Fill + dt / GameConfig.PumpFillSeconds);

        if (Fill >= 1f || Time >= GameConfig.PumpDuration)
            IsDone = true;
    }
}
