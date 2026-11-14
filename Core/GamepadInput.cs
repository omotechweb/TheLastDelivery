using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TheLastDelivery.Core;

/// <summary>
/// Oyun kolu desteği: kolun tuşları klavye tuşlarına çevrilip klavye durumuna eklenir, böylece oyunun
/// geri kalanı kolu ayrıca bilmek zorunda kalmaz.
///
///   Sol çubuk / RT / LT   W A S D (hız, makas, yürüme, menü)
///   Sağ çubuk             ok tuşları (kovalamacada engelden kaçış)
///   Yön tuşları           1 2 3 4 (eşyalar ve diyalog seçenekleri: yukarı, sağ, aşağı, sol)
///   A / B / X / Y         onayla (ENTER+SPACE) / geri (BACKSPACE) / etkileşim (E) / yem (F)
///   RB                    buhar (SHIFT)
///   START / BACK          duraklat (ESC) / telsiz (T)
///   Sol / sağ çubuk tıkı  düdük (H) / kabin (K)
///   LB basılıyken         A: erzak (G)  X: soba (C)  Y: konuş (J)  B: arşiv (L)
///                         yön tuşları: harita (N), müzik kutusu (M), fotoğraf (P), 5. eşya
/// </summary>
public static class GamepadInput
{
    private const float StickThreshold = 0.5f;
    private const float TriggerThreshold = 0.35f;

    private static readonly List<Keys> Pressed = new();
    private static float _rumbleLeft;

    /// <summary>Bu karede bağlı bir kol var mı (ayarlar ekranındaki durum satırı için).</summary>
    public static bool Connected { get; private set; }

    /// <summary>Klavye durumuna kolun karşılık gelen tuşlarını ekler.</summary>
    public static KeyboardState Merge(KeyboardState keyboard)
    {
        GamePadState pad;
        try { pad = GamePad.GetState(PlayerIndex.One); }
        catch (Exception) { Connected = false; return keyboard; } // Kol sürücüsü yoksa klavyeyle devam

        Connected = pad.IsConnected;
        if (!Connected) return keyboard;

        Pressed.Clear();
        Pressed.AddRange(keyboard.GetPressedKeys());

        var left = pad.ThumbSticks.Left;
        var right = pad.ThumbSticks.Right;
        bool modifier = pad.Buttons.LeftShoulder == ButtonState.Pressed;

        Add(left.Y > StickThreshold || pad.Triggers.Right > TriggerThreshold, Keys.W);
        Add(left.Y < -StickThreshold || pad.Triggers.Left > TriggerThreshold, Keys.S);
        Add(left.X < -StickThreshold, Keys.A);
        Add(left.X > StickThreshold, Keys.D);

        Add(right.Y > StickThreshold, Keys.Up);
        Add(right.Y < -StickThreshold, Keys.Down);
        Add(right.X < -StickThreshold, Keys.Left);
        Add(right.X > StickThreshold, Keys.Right);

        bool a = pad.Buttons.A == ButtonState.Pressed, b = pad.Buttons.B == ButtonState.Pressed;
        bool x = pad.Buttons.X == ButtonState.Pressed, y = pad.Buttons.Y == ButtonState.Pressed;
        bool up = pad.DPad.Up == ButtonState.Pressed, down = pad.DPad.Down == ButtonState.Pressed;
        bool dLeft = pad.DPad.Left == ButtonState.Pressed, dRight = pad.DPad.Right == ButtonState.Pressed;

        if (modifier)
        {
            Add(a, Keys.G);
            Add(x, Keys.C);
            Add(y, Keys.J);
            Add(b, Keys.L);
            Add(up, Keys.N);
            Add(dRight, Keys.M);
            Add(down, Keys.P);
            Add(dLeft, Keys.D5);
        }
        else
        {
            Add(a, Keys.Enter);
            Add(a, Keys.Space);
            Add(b, Keys.Back);
            Add(x, Keys.E);
            Add(y, Keys.F);
            Add(up, Keys.D1);
            Add(dRight, Keys.D2);
            Add(down, Keys.D3);
            Add(dLeft, Keys.D4);
        }

        Add(pad.Buttons.RightShoulder == ButtonState.Pressed, Keys.LeftShift);
        Add(pad.Buttons.Start == ButtonState.Pressed, Keys.Escape);
        Add(pad.Buttons.Back == ButtonState.Pressed, Keys.T);
        Add(pad.Buttons.LeftStick == ButtonState.Pressed, Keys.H);
        Add(pad.Buttons.RightStick == ButtonState.Pressed, Keys.K);

        return new KeyboardState(Pressed.ToArray());
    }

    /// <summary>Kısa bir titreşim başlatır (0-1 şiddet). Ayarlardan kapatılmışsa hiçbir şey yapmaz.</summary>
    public static void Rumble(float strength, float seconds)
    {
        if (!Settings.Vibration || !Connected) return;
        strength = MathHelper.Clamp(strength, 0f, 1f);
        try { GamePad.SetVibration(PlayerIndex.One, strength, strength * 0.6f); }
        catch (Exception) { return; }
        _rumbleLeft = MathF.Max(_rumbleLeft, seconds);
    }

    /// <summary>Titreşim süresini sayar; süre dolunca (ya da oyun duraklayınca) motorları durdurur.</summary>
    public static void Update(float dt, bool silence)
    {
        if (_rumbleLeft <= 0f) return;
        _rumbleLeft = silence ? 0f : _rumbleLeft - dt;
        if (_rumbleLeft > 0f) return;
        try { GamePad.SetVibration(PlayerIndex.One, 0f, 0f); }
        catch (Exception) { /* Kol bu arada çıkarılmış olabilir */ }
    }

    private static void Add(bool condition, Keys key)
    {
        if (condition && !Pressed.Contains(key)) Pressed.Add(key);
    }
}
