using UnityEngine;

/// <summary>
/// 레벨 내 모든 창문을 추적하고, 전부 청소 완료 시 GameManager에 알림.
/// InteractionScene에 하나만 배치.
/// </summary>
public class CleaningManager : MonoBehaviour
{
    public static CleaningManager Instance { get; private set; }

    [SerializeField] private Window[] windows;

    private int _cleanedCount;

    public int TotalWindows => windows.Length;
    public int CleanedCount => _cleanedCount;

    public event System.Action OnAllWindowsCleaned;
    public event System.Action<int, int> OnWindowProgress; // cleaned, total

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // LevelManager가 있으면 SetWindows()로 구독 처리 — 여기서 하면 이중 구독됨
        if (LevelManager.Instance == null)
            Subscribe(windows);
        OnWindowProgress?.Invoke(0, windows.Length);
    }

    private void OnDestroy() => Unsubscribe(windows);

    // LevelManager가 레벨 전환 시 호출
    public void SetWindows(Window[] newWindows)
    {
        Unsubscribe(windows);
        windows = newWindows;
        _cleanedCount = 0;
        Subscribe(windows);
        OnWindowProgress?.Invoke(0, windows.Length);
    }

    private void Subscribe(Window[] arr)
    {
        foreach (var w in arr)
            if (w != null) w.OnCleaned += HandleWindowCleaned;
    }

    private void Unsubscribe(Window[] arr)
    {
        foreach (var w in arr)
            if (w != null) w.OnCleaned -= HandleWindowCleaned;
    }

    private void HandleWindowCleaned(Window window)
    {
        _cleanedCount++;
        ScoreManager.Instance?.AddWindowScore();
        OnWindowProgress?.Invoke(_cleanedCount, windows.Length);

        if (_cleanedCount >= windows.Length)
            OnAllWindowsCleaned?.Invoke();
    }
}
