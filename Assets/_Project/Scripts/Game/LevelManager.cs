using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [System.Serializable]
    public class LevelData
    {
        public Window[] windows;
        public float timeLimitSeconds = 120f;
        [Range(0.5f, 3f)] public float obstacleSpeedMultiplier    = 1f;
        [Range(0.3f, 2f)] public float obstacleIntervalMultiplier = 1f;
    }

    [Header("Levels")]
    [SerializeField] private LevelData[] levels;

    [Header("References")]
    [SerializeField] private CleaningManager cleaningManager;
    [SerializeField] private ObstacleSpawner obstacleSpawner;
    [SerializeField] private Transform playerTransform; // XR Origin

    [Header("Player Spawn Positions (per level)")]
    [SerializeField] private Vector3[] playerSpawnPositions;

    private int   _current     = 0;
    private float _timeLeft    = 0f;
    private bool  _timerActive = false;

    public int   CurrentLevel => _current + 1;
    public int   TotalLevels  => levels.Length;
    public float TimeLeft     => _timeLeft;
    public float TimeLimit    => levels != null && _current < levels.Length ? levels[_current].timeLimitSeconds : 1f;

    public event System.Action<int>   OnLevelStarted;
    public event System.Action<float> OnTimerTick;      // 남은 시간(초)
    public event System.Action        OnTimeUp;
    public event System.Action        OnAllLevelsComplete;

    private void Awake() => Instance = this;

    private void Start()
    {
        DeactivateAll();
        StartLevel(0);
    }

    private void Update()
    {
        if (!_timerActive) return;

        _timeLeft -= Time.deltaTime;
        OnTimerTick?.Invoke(_timeLeft);

        if (_timeLeft <= 0f)
        {
            _timerActive = false;
            _timeLeft = 0f;
            OnTimeUp?.Invoke();
        }
    }

    public void NextLevel()
    {
        _current++;
        if (_current >= levels.Length)
        {
            _timerActive = false;
            OnAllLevelsComplete?.Invoke();
            return;
        }
        DeactivateAll();
        StartLevel(_current);
    }

    private void StartLevel(int index)
    {
        var data = levels[index];

        foreach (var w in data.windows)
            if (w != null) w.gameObject.SetActive(true);

        cleaningManager.SetWindows(data.windows);

        if (obstacleSpawner != null)
        {
            obstacleSpawner.flySpeed         = 1.5f * data.obstacleSpeedMultiplier;
            obstacleSpawner.minSpawnInterval  = 7f   * data.obstacleIntervalMultiplier;
            obstacleSpawner.maxSpawnInterval  = 15f  * data.obstacleIntervalMultiplier;
        }

        // 플레이어를 해당 레벨 건물 앞으로 이동
        if (playerTransform != null && playerSpawnPositions != null && index < playerSpawnPositions.Length)
        {
            playerTransform.position = playerSpawnPositions[index];
            // RopeMovement 범위 중심도 갱신
            var rope = playerTransform.GetComponent<RopeMovement>();
            rope?.SetCenter(playerSpawnPositions[index].x);
            Debug.Log($"[LevelManager] 플레이어 이동 → {playerSpawnPositions[index]}");
        }

        _timeLeft    = data.timeLimitSeconds;
        _timerActive = true;

        OnLevelStarted?.Invoke(_current + 1);
        GameUI.Instance?.ShowLevelStart(_current + 1, data.windows.Length, data.timeLimitSeconds);
        Debug.Log($"[LevelManager] Level {_current + 1} 시작 — 제한시간 {data.timeLimitSeconds}초");
    }

    private void DeactivateAll()
    {
        foreach (var level in levels)
            foreach (var w in level.windows)
                if (w != null) w.gameObject.SetActive(false);
    }
}
