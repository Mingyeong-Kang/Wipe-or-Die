using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 플레이어 HP 관리.
/// 지연님 버전(damageFlash / haptic / StopSpawning) +
/// 민경님 버전(OnDeath / OnHealthChanged 이벤트 / Heal) 통합.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("HP Settings")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("UI")]
    public Slider hpSlider;
    public TextMeshProUGUI hpValueText;
    public GameObject gameOverText;
    public GameObject gameOverPanel;

    [Header("Obstacle")]
    public ObstacleSpawner obstacleSpawner;

    [Header("Damage FX")]
    public Image damageFlashImage;
    public float flashDuration = 0.2f;
    public float flashAlpha = 0.4f;
    public AudioSource audioSource;
    public AudioClip damageSound;

    // GameManager / GameUI 연결용 이벤트
    public event System.Action OnDeath;
    public event System.Action<int, int> OnHealthChanged; // current, max

    public int CurrentHealth => currentHealth;
    public bool IsDead => currentHealth <= 0;

    private void Start()
    {
        currentHealth = maxHealth;

        hpSlider.maxValue = maxHealth;
        hpSlider.value = currentHealth;

        if (gameOverText != null) gameOverText.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        SetDamageFlashAlpha(0f);
        UpdateHPUI();
    }

    public void TakeDamage(int damage)
    {
        if (IsDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        if (audioSource != null && damageSound != null)
            audioSource.PlayOneShot(damageSound);

        UpdateHPUI();
        StartCoroutine(DamageFlash());
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (IsDead)
            GameOver();
    }

    public void Heal(int amount)
    {
        if (IsDead) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateHPUI();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private IEnumerator DamageFlash()
    {
        SetDamageFlashAlpha(flashAlpha);
        yield return new WaitForSeconds(flashDuration);
        SetDamageFlashAlpha(0f);
    }

    private void SetDamageFlashAlpha(float alpha)
    {
        if (damageFlashImage == null) return;
        Color color = damageFlashImage.color;
        color.a = alpha;
        damageFlashImage.color = color;
    }

    private void UpdateHPUI()
    {
        if (hpSlider != null) hpSlider.value = currentHealth;
        if (hpValueText != null) hpValueText.text = $"{currentHealth} / {maxHealth}";
    }

    private void GameOver()
    {
        Debug.Log("GAME OVER");

        if (gameOverText != null) gameOverText.SetActive(true);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (obstacleSpawner != null) obstacleSpawner.StopSpawning();

        OnDeath?.Invoke();
    }
}
