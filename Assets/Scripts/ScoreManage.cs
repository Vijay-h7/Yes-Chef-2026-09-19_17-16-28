using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    private const string HighScoreKey = "yeschef.highscore";

    public int Score { get; private set; }
    public int HighScore { get; private set; }
    public bool IsNewHighScore { get; private set; }

    public event Action<int> ScoreChanged;
    public event Action<int> HighScoreChanged;

    private void Awake()
    {
        Instance = this;
        HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
    }

    public void ResetRound()
    {
        Score = 0;
        IsNewHighScore = false;
        ScoreChanged?.Invoke(Score);
    }

    public void Add(int points)
    {
        Score += points;
        ScoreChanged?.Invoke(Score);
    }

    // Called once when the round ends.
    public void CommitRound()
    {
        IsNewHighScore = Score > HighScore;
        if (!IsNewHighScore) return;

        HighScore = Score;
        PlayerPrefs.SetInt(HighScoreKey, HighScore);
        PlayerPrefs.Save();
        HighScoreChanged?.Invoke(HighScore);
    }
}