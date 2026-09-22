using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum GameState { Menu, Playing, Paused, GameOver }

// Runs its Start() after other scripts' Start(), so they have subscribed to our events before we fire them.
[DefaultExecutionOrder(100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private float gameDuration = 180f;
    [Tooltip("Testing only: skips the menu. Turn OFF before submitting.")]
    [SerializeField] private bool autoStartForTesting;
    [Tooltip("An order delivered within this many seconds of appearing counts toward the combo streak.")]
    [SerializeField] private float fastDeliveryThreshold = 14f;

    private const string HighScoreKey = "yeschef.highscore";

    public GameState State { get; private set; } = GameState.Menu;
    public float TimeRemaining { get; private set; }
    public int Score { get; private set; }
    public int HighScore { get; private set; }
    public bool IsNewHighScore { get; private set; }
    public bool IsPlaying => State == GameState.Playing;
    public int Combo { get; private set; }

    public event Action GameStarted;            // a fresh round begins: everything resets itself
    public event Action<GameState> StateChanged;
    public event Action<int> ScoreChanged;
    public event Action<int> ComboChanged;

    private void Awake()
    {
        Instance = this;
        HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
    }

    private void Start()
    {
        SetState(GameState.Menu);
        if (autoStartForTesting) StartGame();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            TogglePause();

        if (State != GameState.Playing) return;

        TimeRemaining -= Time.deltaTime;
        if (TimeRemaining <= 0f)
        {
            TimeRemaining = 0f;
            EndGame();
        }
    }

    public void StartGame()
    {
        Score = 0;
        IsNewHighScore = false;
        TimeRemaining = gameDuration;
        Combo = 0;
        ScoreChanged?.Invoke(Score);
        ComboChanged?.Invoke(Combo);
        GameStarted?.Invoke();
        SetState(GameState.Playing);
    }

    public void AddScore(int points)
    {
        Score += points;
        ScoreChanged?.Invoke(Score);
    }

    // Called by a CustomerWindow when an order is fully delivered.
    // Fast deliveries build a combo streak that multiplies the score.
    public int RegisterDelivery(int baseScore, float elapsedSeconds)
    {
        bool fast = elapsedSeconds <= fastDeliveryThreshold;
        Combo = fast ? Combo + 1 : 0;
        ComboChanged?.Invoke(Combo);

        float multiplier = 1f + Mathf.Min(Combo, 5) * 0.25f;
        int finalScore = Mathf.Max(0, Mathf.RoundToInt(baseScore * multiplier));
        AddScore(finalScore);
        return finalScore;
    }

    // Called when a customer's patience runs out. Breaks the combo streak.
    public void RegisterMiss()
    {
        Combo = 0;
        ComboChanged?.Invoke(Combo);
    }

    public void TogglePause()
    {
        if (State == GameState.Playing) SetState(GameState.Paused);
        else if (State == GameState.Paused) SetState(GameState.Playing);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void EndGame()
    {
        IsNewHighScore = Score > HighScore;
        if (IsNewHighScore)
        {
            HighScore = Score;
            PlayerPrefs.SetInt(HighScoreKey, HighScore);
            PlayerPrefs.Save();
        }
        SetState(GameState.GameOver);
    }

    private void SetState(GameState newState)
    {
        State = newState;
        // timeScale 0 freezes every deltaTime timer (cooking, orders, game clock) for free.
        Time.timeScale = newState == GameState.Playing ? 1f : 0f;
        StateChanged?.Invoke(newState);
    }
}
