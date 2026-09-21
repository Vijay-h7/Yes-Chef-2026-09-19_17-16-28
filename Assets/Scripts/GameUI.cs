using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;

    [Header("HUD")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text highScoreText;
    [SerializeField] private TMP_Text timerText;

    [Header("Game Over")]
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private GameObject newHighScoreLabel;

    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;

    private void Start()
    {
        var game = GameManager.Instance;

        startButton.onClick.AddListener(game.StartGame);
        restartButton.onClick.AddListener(game.StartGame);
        pauseButton.onClick.AddListener(game.TogglePause);
        resumeButton.onClick.AddListener(game.TogglePause);
        quitButton.onClick.AddListener(game.QuitGame);

        game.StateChanged += OnStateChanged;
        game.ScoreChanged += OnScoreChanged;

        OnScoreChanged(game.Score);
        OnStateChanged(game.State);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.StateChanged -= OnStateChanged;
        GameManager.Instance.ScoreChanged -= OnScoreChanged;
    }

    private void Update()
    {
        int seconds = Mathf.CeilToInt(GameManager.Instance.TimeRemaining);
        timerText.text = $"{seconds / 60}:{seconds % 60:00}";
    }

    private void OnScoreChanged(int score) => scoreText.text = $"Score: {score}";

    private void OnStateChanged(GameState state)
    {
        var game = GameManager.Instance;

        menuPanel.SetActive(state == GameState.Menu);
        hudPanel.SetActive(state != GameState.Menu);
        pausePanel.SetActive(state == GameState.Paused);
        gameOverPanel.SetActive(state == GameState.GameOver);
        highScoreText.text = $"High Score: {game.HighScore}";

        if (state == GameState.GameOver)
        {
            finalScoreText.text = $"Final Score: {game.Score}";
            newHighScoreLabel.SetActive(game.IsNewHighScore);
        }
    }
}
