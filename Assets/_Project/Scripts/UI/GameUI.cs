using UnityEngine;
using TMPro;

/// <summary>
/// HP, 점수, 콤보, GameOver, LevelClear UI를 한 곳에서 업데이트.
/// World Space Canvas를 XR Origin 앞에 배치 권장 (VR HUD).
/// </summary>
public class GameUI : MonoBehaviour
{
    public static GameUI Instance { get; private set; }

    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private TextMeshProUGUI windowProgressText;

    [Header("Panels")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject levelClearPanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;

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

        gameOverPanel?.SetActive(false);
        levelClearPanel?.SetActive(false);

        UpdateScoreHUD(0, 0);
    }

    private void OnDestroy()
    {
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnScoreUpdated -= UpdateScoreHUD;

        if (CleaningManager.Instance != null)
            CleaningManager.Instance.OnWindowProgress -= UpdateWindowProgress;
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

    public void ShowGameOver()
    {
        gameOverPanel?.SetActive(true);
    }

    public void ShowLevelClear(int finalScore)
    {
        levelClearPanel?.SetActive(true);
        if (finalScoreText != null) finalScoreText.text = $"Final Score: {finalScore}";
    }
}
