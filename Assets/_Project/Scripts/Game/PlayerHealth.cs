using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 플레이어 HP 관리.
/// 장애물 팀의 ObstacleHit이 TakeDamage()를 호출하면 HP가 깎임.
/// OnDeath 이벤트로 GameManager가 GameOver 처리.
///
/// ※ 장애물 팀 PlayerHealth.cs와 merge 시 이 버전(이벤트 포함)을 채택할 것.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("HP Settings")]
    [SerializeField] private int maxHealth = 100;

    [Header("UI References")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TextMeshProUGUI hpText;

    private int _currentHealth;

    public int CurrentHealth => _currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => _currentHealth <= 0;

    public event System.Action OnDeath;
    public event System.Action<int, int> OnHealthChanged; // current, max

    private void Start()
    {
        _currentHealth = maxHealth;
        RefreshUI();
    }

    public void TakeDamage(int damage)
    {
        if (IsDead) return;

        _currentHealth = Mathf.Max(_currentHealth - damage, 0);
        RefreshUI();
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);

        if (IsDead)
            OnDeath?.Invoke();
    }

    public void Heal(int amount)
    {
        if (IsDead) return;
        _currentHealth = Mathf.Min(_currentHealth + amount, maxHealth);
        RefreshUI();
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);
    }

    private void RefreshUI()
    {
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHealth;
            hpSlider.value = _currentHealth;
        }
        if (hpText != null)
            hpText.text = $"{_currentHealth} / {maxHealth}";
    }
}
