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
        _cleanedCount = 0;
        foreach (var w in windows)
            w.OnCleaned += HandleWindowCleaned;
    }

    private void OnDestroy()
    {
        foreach (var w in windows)
        {
            if (w != null) w.OnCleaned -= HandleWindowCleaned;
        }
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
