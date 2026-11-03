using System;
using System.Collections.Generic;
using TheLastDelivery.Core;
using TheLastDelivery.Economy;

namespace TheLastDelivery.Platform;

/// <summary>
/// Sonsuz Kurye liderlik tablosu. Her koşu bittiğinde skor ve katedilen mesafe yerel tabloya
/// (SaveData, en iyi 10) yazılır ve platforma (Steam: "ENDLESS_SURVIVAL" tablosu) yüklenir.
/// Skor: km × 10 + (aşama − 1) × 250 + kazanılan Kredi × 5.
/// </summary>
public static class LeaderboardManager
{
    public const string EndlessBoard = "ENDLESS_SURVIVAL";

    public static IReadOnlyList<LeaderboardEntry> Endless => SaveData.Current.EndlessLeaderboard;

    public static int Score(int km, int stage, int credits) => km * 10 + Math.Max(0, stage - 1) * 250 + credits * 5;

    /// <summary>Koşuyu tabloya ekler.</summary>
    /// <returns>Yerel tablodaki sıra (1 = en iyi) ve skor; tabloya giremediyse sıra 0.</returns>
    public static (int Rank, int Score) SubmitEndless(int km, int stage, int credits)
    {
        int score = Score(km, stage, credits);
        var board = SaveData.Current.EndlessLeaderboard;
        var entry = new LeaderboardEntry
        {
            Score = score,
            DistanceKm = km,
            Stage = stage,
            Credits = credits,
            Date = DateTime.Now.ToString("yyyy-MM-dd")
        };

        int index = board.FindIndex(e => e.Score < score);
        if (index < 0) index = board.Count;
        board.Insert(index, entry);
        if (board.Count > GameConfig.LeaderboardSize)
            board.RemoveRange(GameConfig.LeaderboardSize, board.Count - GameConfig.LeaderboardSize);
        SaveData.Save();

        AchievementManager.Platform.UploadLeaderboardScore(EndlessBoard, score, new[] { km, stage, credits });
        return (index < GameConfig.LeaderboardSize ? index + 1 : 0, score);
    }
}

/// <summary>Liderlik tablosunun bir satırı (JSON olarak kayıt dosyasında tutulur).</summary>
public sealed class LeaderboardEntry
{
    public int Score { get; set; }
    public int DistanceKm { get; set; }
    public int Stage { get; set; }
    public int Credits { get; set; }
    public string Date { get; set; }
}
