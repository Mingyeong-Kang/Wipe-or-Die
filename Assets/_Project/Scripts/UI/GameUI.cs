using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// HP, 점수, 콤보, GameOver, LevelClear UI를 한 곳에서 업데이트.
/// World Space Canvas를 XR Origin 앞에 배치 권장 (VR HUD).
/// 팀 UI 컬러: RED #FF0000 / BLUE #0C00CC / Font: 기본, Bold, Uppercase
/// </summary>
public class GameUI : MonoBehaviour
{
    public static GameUI Instance { get; private set; }

    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private TextMeshProUGUI windowProgressText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI levelText;

    [Header("Panels")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject levelClearPanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private GameObject levelStartPanel;
    [SerializeField] private TextMeshProUGUI levelStartTitleText;
    [SerializeField] private TextMeshProUGUI levelStartInfoText;

    // 팀 결정 색상: RED #FF0000 / BLUE #0C00CC
    private static readonly Color UIRed = new Color(1f, 0f, 0f);
    private static readonly Color UIBlue = new Color(0.047f, 0f, 0.8f);

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnScoreUpdated += UpdateScoreHUD;
        if (CleaningManager.Instance != null)
            CleaningManager.Instance.OnWindowProgress += UpdateWindowProgress;
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnTimerTick    += UpdateTimer;
            LevelManager.Instance.OnLevelStarted += UpdateLevelText;
            UpdateLevelText(LevelManager.Instance.CurrentLevel);
        }

        gameOverPanel?.SetActive(false);
        levelClearPanel?.SetActive(false);
        levelStartPanel?.SetActive(false);
        UpdateScoreHUD(0, 0);
    }

    private void OnDestroy()
    {
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnScoreUpdated -= UpdateScoreHUD;
        if (CleaningManager.Instance != null)
            CleaningManager.Instance.OnWindowProgress -= UpdateWindowProgress;
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnTimerTick    -= UpdateTimer;
            LevelManager.Instance.OnLevelStarted -= UpdateLevelText;
        }
    }

    public void UpdateScoreHUD(int score, int combo)
    {
        if (scoreText != null) scoreText.text = $"Score: {score}";
        if (comboText != null)
            comboText.text = combo > 1 ? $"x{combo} Combo!" : string.Empty;
    }

    public void UpdateWindowProgress(int cleaned, int total)
    {
        if (windowProgressText != null)
            windowProgressText.text = $"창문 {cleaned} / {total}";
    }

    public void UpdateTimer(float timeLeft)
    {
        if (timerText == null) return;
        int m = Mathf.FloorToInt(timeLeft / 60f);
        int s = Mathf.FloorToInt(timeLeft % 60f);
        timerText.text  = $"{m:00}:{s:00}";
        timerText.color = timeLeft <= 30f ? Color.red : Color.white;
    }

    public void UpdateLevelText(int level)
    {
        if (levelText != null)
            levelText.text = $"LEVEL {level}";
    }

    public void ShowGameOver()
    {
        gameOverPanel?.SetActive(true);
    }

    public void ShowLevelClear(int finalScore, int nextLevel)
    {
        levelClearPanel?.SetActive(true);
        if (nextLevel == -1)
        {
            if (finalScoreText != null) finalScoreText.text = $"ALL CLEAR!\nFinal Score: {finalScore}";
        }
        else
        {
            if (finalScoreText != null) finalScoreText.text = $"Level {nextLevel} Clear!\nScore: {finalScore}";
            // 2초 후 자동으로 패널 닫힘 (GoNextLevel에서 새 레벨 시작)
            Invoke(nameof(HideLevelClear), 1.8f);
        }
    }

    private void HideLevelClear() => levelClearPanel?.SetActive(false);

    // level=레벨번호, windowCount=창문 수, timeLimit=제한시간(초)
    public void ShowLevelStart(int level, int windowCount, float timeLimit)
    {
        if (levelStartPanel == null) return;
        levelStartPanel.SetActive(true);

        if (levelStartTitleText != null)
            levelStartTitleText.text = $"LEVEL {level}";

        if (levelStartInfoText != null)
        {
            int m = Mathf.FloorToInt(timeLimit / 60f);
            int s = Mathf.FloorToInt(timeLimit % 60f);
            levelStartInfoText.text = $"창문 {windowCount}개  |  제한시간 {m:00}:{s:00}";
        }

        Invoke(nameof(HideLevelStart), 2.5f);
    }

    private void HideLevelStart() => levelStartPanel?.SetActive(false);
}
