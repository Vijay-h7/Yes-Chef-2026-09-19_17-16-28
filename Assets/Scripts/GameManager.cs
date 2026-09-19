using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum GameState { Menu, Playing, Paused, GameOver }

// Runs its Start() after other scripts have subscribed to its events.
[DefaultExecutionOrder(100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameConfig config;
    [Tooltip("Testing only: skip the menu and start immediately. Turn OFF before submitting.")]
    [SerializeField] private bool autoStartForTesting;

    public GameState State { get; private set; } = GameState.Menu;
    public float TimeRemaining { get; private set; }
    public bool IsPlaying => State == GameState.Playing;

    public event Action GameStarted;               // fresh round: everything resets
    public event Action<GameState> StateChanged;

    private void Awake() => Instance = this;

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
        TimeRemaining = config.gameDuration;
        ScoreManager.Instance.ResetRound();
        GameStarted?.Invoke();
        SetState(GameState.Playing);
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
        // Commit first so the UI can read IsNewHighScore when the state changes.
        ScoreManager.Instance.CommitRound();
        SetState(GameState.GameOver);
    }

    private void SetState(GameState newState)
    {
        State = newState;
        // Pausing everything with timeScale means every deltaTime timer freezes automatically.
        Time.timeScale = newState == GameState.Playing ? 1f : 0f;
        StateChanged?.Invoke(newState);
    }
}