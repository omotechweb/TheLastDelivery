using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using TheLastDelivery.Core;
using TheLastDelivery.Events;
using TheLastDelivery.Items;
using TheLastDelivery.Localization;
using TheLastDelivery.UI;
using TheLastDelivery.World;

namespace TheLastDelivery.Passengers;

/// <summary>Bir sabotajın oyuncuya bildirilecek sonucu.</summary>
public readonly record struct BetrayalNews(string Message, Color Color, bool TraitorCaught, Passenger Saboteur = null);

/// <summary>
/// Gizli hainlerin gece sabotajları:
///   • Yakıt çalma, vanayı gevşetme, bir yolcuyu zehirleme ya da kargodan yağma aşırma.
///   • Her sabotaj hainin şüphesini belirgin, masumlarınkini rastgele (gürültü) artırır.
///   • Trende masum bir Asker varsa hain suçüstü yakalanabilir (açığa çıkar, sabotaj durur).
/// Oyuncu, kabin menüsünden ([K]) şüphelendiği yolcuyu trenden kovabilir.
/// </summary>
public sealed class BetrayalSystem
{
    private readonly Random _rng = new();
    private float _cooldown;

    /// <summary>Hainlerin bu yolculukta çaldığı toplam yakıt (hain kovulunca bir kısmı geri alınır).</summary>
    public float StolenFuel { get; private set; }

    public void Reset()
    {
        _cooldown = NextCooldown() + 10f;
        StolenFuel = 0f;
    }

    private float NextCooldown() =>
        (GameConfig.SabotageCooldownMin + (float)_rng.NextDouble() * (GameConfig.SabotageCooldownMax - GameConfig.SabotageCooldownMin))
        / Difficulty.SabotageFrequency * GameConfig.Pace; // Acımasız Kış: sabotajlar iki kat sık

    /// <param name="isNight">Hainler yalnızca gece karanlığında iş görür.</param>
    /// <returns>Bu karede bir sabotaj olduysa haberi (yoksa null).</returns>
    public BetrayalNews? Update(float dt, bool isNight, Cabin cabin, Train train, Inventory inventory, RoadEventSystem events)
    {
        var traitors = cabin.ActiveTraitors().ToList();
        if (traitors.Count == 0 || !isNight) return null;

        _cooldown -= dt;
        if (_cooldown > 0f) return null;
        _cooldown = NextCooldown();

        var traitor = traitors[_rng.Next(traitors.Count)];
        string message = Sabotage(traitor, cabin, train, inventory, events);
        if (message == null) return null;

        // Şüphe: hain belirgin artar, diğerleri biraz (kimin yaptığı hemen belli olmasın)
        foreach (var p in cabin.Passengers)
        {
            if (p == traitor) p.AddSuspicion(28f + (float)_rng.NextDouble() * 14f);
            else if (!p.IsRevealed && p.Skill != PassengerSkill.Child) p.AddSuspicion((float)_rng.NextDouble() * 14f);
        }

        // Masum bir asker nöbetteyse hain suçüstü yakalanabilir
        var guard = cabin.Passengers.FirstOrDefault(p => p.Skill == PassengerSkill.Guard && !p.IsTraitor);
        if (guard != null && _rng.NextDouble() < GameConfig.GuardCatchChance)
        {
            traitor.Reveal();
            return new BetrayalNews(message + "\n" + Loc.F("betray.caught", traitor.Name, guard.Name), CabinPanel.Good, true);
        }

        return new BetrayalNews(message, new Color(200, 130, 255), false, traitor);
    }

    private string Sabotage(Passenger traitor, Cabin cabin, Train train, Inventory inventory, RoadEventSystem events)
    {
        var victims = cabin.Passengers.Where(p => p != traitor && !p.IsPoisoned).ToList();

        // Mümkün olan sabotajlardan birini seç
        for (int attempt = 0; attempt < 4; attempt++)
        {
            switch (_rng.Next(4))
            {
                case 0 when train.Fuel > GameConfig.SabotageFuel * 2f:
                    train.BurnFuel(GameConfig.SabotageFuel);
                    StolenFuel += GameConfig.SabotageFuel;
                    return Loc.F("betray.fuel", (int)GameConfig.SabotageFuel);

                case 1 when events.ForceStart(RoadEventType.ValveLeak, cabin):
                    return Loc.T("betray.valve");

                case 2 when victims.Count > 0:
                    var victim = victims[_rng.Next(victims.Count)];
                    victim.Poison();
                    return Loc.F("betray.poison", victim.Name);

                case 3 when inventory.Cargo.Total > 0:
                    int stolen = inventory.Cargo.Steal(2).Count;
                    return Loc.F("betray.cargo", stolen);
            }
        }
        return null;
    }

    /// <summary>Hain kovuldu: çaldığı yakıtın yarısı çantasından çıkar.</summary>
    public float RecoverFuel()
    {
        float recovered = StolenFuel * 0.5f;
        StolenFuel = 0f;
        return recovered;
    }
}
