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

    [Header("Panels")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject levelClearPanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;

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
