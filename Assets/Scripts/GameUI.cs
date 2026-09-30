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

    [Header("Quit Confirmation")]
    [SerializeField] private GameObject quitConfirmPanel;
    [SerializeField] private Button quitConfirmYesButton;
    [SerializeField] private Button quitConfirmNoButton;

    private void Start()
    {
        var game = GameManager.Instance;

        startButton.onClick.AddListener(game.StartGame);
        restartButton.onClick.AddListener(game.StartGame);
        pauseButton.onClick.AddListener(game.TogglePause);
        resumeButton.onClick.AddListener(game.TogglePause);
        quitButton.onClick.AddListener(() => quitConfirmPanel.SetActive(true));
        quitConfirmYesButton.onClick.AddListener(game.QuitGame);
        quitConfirmNoButton.onClick.AddListener(() => quitConfirmPanel.SetActive(false));

        game.StateChanged += OnStateChanged;
        game.ScoreChanged += OnScoreChanged;

        ApplyCozyTheme();

        OnScoreChanged(game.Score);
        OnStateChanged(game.State);
    }

    private void ApplyCozyTheme()
    {
        Color teal = new Color(0.18f, 0.49f, 0.49f);
        Color red = new Color(0.85f, 0.31f, 0.20f);
        Color cream = new Color(0.97f, 0.95f, 0.91f);

        if (startButton != null) startButton.GetComponent<Image>().color = teal;
        if (resumeButton != null) resumeButton.GetComponent<Image>().color = teal;
        if (restartButton != null) restartButton.GetComponent<Image>().color = teal;
        if (quitButton != null) quitButton.GetComponent<Image>().color = red;
        if (quitConfirmYesButton != null) quitConfirmYesButton.GetComponent<Image>().color = red;
        if (quitConfirmNoButton != null) quitConfirmNoButton.GetComponent<Image>().color = teal;
        if (pauseButton != null) pauseButton.GetComponent<Image>().color = new Color(0.36f, 0.25f, 0.20f);
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
        timerText.color = seconds <= 30 ? new Color(1f, 0.45f, 0.35f) : Color.white;
    }

    private void OnScoreChanged(int score) => scoreText.text = $"Score <color=#FFD85A>{score}</color>";

    private void OnStateChanged(GameState state)
    {
        var game = GameManager.Instance;

        menuPanel.SetActive(state == GameState.Menu);
        hudPanel.SetActive(state != GameState.Menu);
        pausePanel.SetActive(state == GameState.Paused);
        if (quitConfirmPanel != null) quitConfirmPanel.SetActive(false);
        gameOverPanel.SetActive(state == GameState.GameOver);
        highScoreText.text = $"Best <color=#FFD85A>{game.HighScore}</color>";

        if (state == GameState.GameOver)
        {
            finalScoreText.text = $"Final score  <color=#FFD85A>{game.Score}</color>";
            newHighScoreLabel.SetActive(game.IsNewHighScore);
        }
    }
}
