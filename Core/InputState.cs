using Microsoft.Xna.Framework.Input;

namespace TheLastDelivery.Core;

/// <summary>
/// Bir karenin klavye durumu. Alt sistemler (kasaba, benzinlik, kovalamaca) tuşun
/// basılı tutulduğunu (Down) ya da bu karede yeni basıldığını (Pressed) buradan sorar.
/// </summary>
public readonly struct InputState
{
    private readonly KeyboardState _current;
    private readonly KeyboardState _previous;

    public InputState(KeyboardState current, KeyboardState previous)
    {
        _current = current;
        _previous = previous;
    }

    public bool Down(Keys key) => _current.IsKeyDown(key);

    public bool Pressed(Keys key) => _current.IsKeyDown(key) && _previous.IsKeyUp(key);

    public bool AnyPressed(params Keys[] keys)
    {
        foreach (var key in keys)
            if (Pressed(key)) return true;
        return false;
    }
}
