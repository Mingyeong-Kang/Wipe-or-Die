using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;

    public Slider hpSlider;
    public TextMeshProUGUI hpValueText;
    public GameObject gameOverText;
    public GameObject gameOverPanel;
    public ObstacleSpawner obstacleSpawner;

    public Image damageFlashImage;
    public float flashDuration = 0.2f;
    public float flashAlpha = 0.4f;

    public AudioSource audioSource;
    public AudioClip damageSound;


    private void Start()
    {
        currentHealth = maxHealth;

        hpSlider.maxValue = maxHealth;
        hpSlider.value = currentHealth;

        if (gameOverText != null)
            gameOverText.SetActive(false);
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        SetDamageFlashAlpha(0f);
        UpdateHPUI();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        if (audioSource != null && damageSound != null)
        {
            audioSource.PlayOneShot(damageSound);
        }


        UpdateHPUI();
        StartCoroutine(DamageFlash());

        if (currentHealth <= 0)
        {
            GameOver();
        }
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
        hpSlider.value = currentHealth;

        if (hpValueText != null)
            hpValueText.text = currentHealth + " / " + maxHealth;
    }

    private void GameOver()
    {
        Debug.Log("GAME OVER");

        if (gameOverText != null)
            gameOverText.SetActive(true);
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (obstacleSpawner != null)
            obstacleSpawner.StopSpawning();
    }
}