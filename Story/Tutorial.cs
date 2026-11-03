using System;
using TheLastDelivery.Core;
using TheLastDelivery.Localization;
using TheLastDelivery.World;

namespace TheLastDelivery.Story;

/// <summary>Bir eğitim adımının tamamlanma koşulu.</summary>
public enum TutorialGoal
{
    SpeedUp,     // Hızı hedefin üstüne çıkar
    SlowDown,    // Hızı hedefin altına indir
    Acknowledge  // Okuyup SPACE'e bas
}

/// <summary>Eğitim sırasında ekranda vurgulanacak arayüz bölgesi.</summary>
public enum TutorialHighlight
{
    None,
    Speed,
    Resources
}

/// <summary>Eğitim adımı. Metinler çeviri tablosundan gelir: "tut.s{N}" ve "tut.p{N}".</summary>
public sealed record TutorialStep(int Number, TutorialGoal Goal, TutorialHighlight Highlight)
{
    public string Text => Loc.T($"tut.s{Number}");

    public string Prompt => Goal switch
    {
        TutorialGoal.SpeedUp => Loc.F($"tut.p{Number}", (int)GameConfig.TutorialSpeedUpTarget),
        TutorialGoal.SlowDown => Loc.F($"tut.p{Number}", (int)GameConfig.TutorialSlowDownTarget),
        _ => Loc.T($"tut.p{Number}")
    };
}

/// <summary>
/// Koca Thomas'ın etkileşimli eğitimi. Eğitim güvenli bir alıştırmadır:
/// tren hareket eder ama yol sayılmaz, kaynak harcanmaz, olay çıkmaz.
/// </summary>
public sealed class Tutorial
{
    private const float CharsPerSecond = 40f;
    private const float GoalCelebration = 0.8f; // Hedefe ulaşınca "Güzel!" için kısa bekleme

    public static readonly TutorialStep[] Steps =
    {
        new(1, TutorialGoal.SpeedUp, TutorialHighlight.Speed),
        new(2, TutorialGoal.SlowDown, TutorialHighlight.Speed),
        new(3, TutorialGoal.Acknowledge, TutorialHighlight.Resources),
        new(4, TutorialGoal.Acknowledge, TutorialHighlight.None),
    };

    private float _visibleChars;
    private float _celebrationTimer = -1f;

    public int StepIndex { get; private set; }
    public bool IsFinished { get; private set; }
    public TutorialStep Current => Steps[Math.Min(StepIndex, Steps.Length - 1)];

    /// <summary>Daktilo efektiyle şu ana kadar yazılmış karakter sayısı.</summary>
    public int VisibleChars => (int)_visibleChars;

    public bool IsTextComplete => _visibleChars >= Current.Text.Length;

    /// <summary>Hız hedefi tutturuldu, sonraki adıma geçmeden önce kısa kutlama anı.</summary>
    public bool IsCelebrating => _celebrationTimer >= 0f;

    public void Start()
    {
        StepIndex = 0;
        IsFinished = false;
        _visibleChars = 0f;
        _celebrationTimer = -1f;
    }

    /// <returns>Bu karede bir adım tamamlandıysa true (ses efekti için).</returns>
    public bool Update(float dt, Train train, bool continuePressed)
    {
        if (IsFinished) return false;

        int textLength = Current.Text.Length;
        _visibleChars = MathF.Min(textLength, _visibleChars + CharsPerSecond * dt);

        if (IsCelebrating)
        {
            _celebrationTimer += dt;
            if (_celebrationTimer >= GoalCelebration) Advance();
            return false;
        }

        switch (Current.Goal)
        {
            case TutorialGoal.SpeedUp when train.Speed >= GameConfig.TutorialSpeedUpTarget:
            case TutorialGoal.SlowDown when train.Speed <= GameConfig.TutorialSlowDownTarget:
                _visibleChars = textLength;
                _celebrationTimer = 0f;
                return true;

            case TutorialGoal.Acknowledge when continuePressed:
                if (!IsTextComplete)
                {
                    _visibleChars = textLength; // Önce yazıyı tamamla
                    return false;
                }
                Advance();
                return true;
        }
        return false;
    }

    private void Advance()
    {
        _celebrationTimer = -1f;
        _visibleChars = 0f;
        StepIndex++;
        if (StepIndex >= Steps.Length)
        {
            StepIndex = Steps.Length - 1;
            IsFinished = true;
        }
    }
}
