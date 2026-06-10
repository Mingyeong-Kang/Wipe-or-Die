using UnityEngine;

/// <summary>
/// 게임 루프 전체를 제어하는 싱글턴.
/// Playing → GameOver (HP 0) 또는 LevelClear (모든 창문 청소).
/// MainScene에 하나만 배치.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Playing, GameOver, LevelClear }

    [Header("References")]
    [SerializeField] private CleaningManager cleaningManager;
    [SerializeField] private PlayerHealth playerHealth;

    public GameState State { get; private set; } = GameState.Playing;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (cleaningManager != null)
            cleaningManager.OnAllWindowsCleaned += HandleLevelClear;
        if (playerHealth != null)
            playerHealth.OnDeath += HandleGameOver;
        if (LevelManager.Instance != null)
            LevelManager.Instance.OnTimeUp += HandleGameOver;
    }

    private void OnDestroy()
    {
        if (cleaningManager != null)
            cleaningManager.OnAllWindowsCleaned -= HandleLevelClear;
        if (playerHealth != null)
            playerHealth.OnDeath -= HandleGameOver;
        if (LevelManager.Instance != null)
            LevelManager.Instance.OnTimeUp -= HandleGameOver;
    }

    private void HandleLevelClear()
    {
        if (State != GameState.Playing) return;

        // LevelManager가 있으면 다음 레벨로, 없거나 마지막이면 게임 클리어
        if (LevelManager.Instance != null && LevelManager.Instance.CurrentLevel < LevelManager.Instance.TotalLevels)
        {
            GameUI.Instance?.ShowLevelClear(ScoreManager.Instance?.Score ?? 0, LevelManager.Instance.CurrentLevel);
            // 2초 후 다음 레벨
            Invoke(nameof(GoNextLevel), 2f);
        }
        else
        {
            State = GameState.LevelClear;
            GameUI.Instance?.ShowLevelClear(ScoreManager.Instance?.Score ?? 0, -1); // -1 = 최종 클리어
        }
    }

    private void GoNextLevel()
    {
        LevelManager.Instance.NextLevel();
    }

    private void HandleGameOver()
    {
        if (State != GameState.Playing) return;
        State = GameState.GameOver;
        GameUI.Instance?.ShowGameOver();
    }

    // UI 버튼에서 호출 가능
    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
