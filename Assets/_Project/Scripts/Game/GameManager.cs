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
    }

    private void OnDestroy()
    {
        if (cleaningManager != null)
            cleaningManager.OnAllWindowsCleaned -= HandleLevelClear;
        if (playerHealth != null)
            playerHealth.OnDeath -= HandleGameOver;
    }

    private void HandleLevelClear()
    {
        if (State != GameState.Playing) return;
        State = GameState.LevelClear;
        GameUI.Instance?.ShowLevelClear(ScoreManager.Instance?.Score ?? 0);
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
