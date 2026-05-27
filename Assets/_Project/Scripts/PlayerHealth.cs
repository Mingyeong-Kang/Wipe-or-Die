using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;

    public Slider hpSlider;
    public TextMeshProUGUI hpValueText;
    public GameObject gameOverText;
    public ObstacleSpawner obstacleSpawner;

    private void Start()
    {
        currentHealth = maxHealth;

        hpSlider.maxValue = maxHealth;
        hpSlider.value = currentHealth;

        if (gameOverText != null)
            gameOverText.SetActive(false);

        UpdateHPUI();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        UpdateHPUI();

        if (currentHealth <= 0)
        {
            GameOver();
        }
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

        if (obstacleSpawner != null)
            obstacleSpawner.enabled = false;
    }
}