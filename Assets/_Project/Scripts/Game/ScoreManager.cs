using UnityEngine;

/// <summary>
/// 점수 및 콤보 시스템.
/// 창문 클리어 → 점수 추가, 일정 시간 내 연속 클리어 시 콤보 보너스.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Score Settings")]
    [SerializeField] private int baseWindowScore = 100;
    [SerializeField] private int comboBonusPerStack = 50;
    [SerializeField] private float comboResetDelay = 5f;

    private int _score;
    private int _combo;
    private float _comboTimer;

    public int Score => _score;
    public int Combo => _combo;

    public event System.Action<int, int> OnScoreUpdated; // score, combo

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (_combo <= 0) return;
        _comboTimer -= Time.deltaTime;
        if (_comboTimer <= 0f)
        {
            _combo = 0;
            OnScoreUpdated?.Invoke(_score, _combo);
        }
    }

    public void AddWindowScore()
    {
        _combo++;
        _comboTimer = comboResetDelay;

        int gained = baseWindowScore + comboBonusPerStack * (_combo - 1);
        _score += gained;

        OnScoreUpdated?.Invoke(_score, _combo);
    }
}
