using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [Range(3, 5)]
    public int maxHealth = 5;

    public int currentHealth;

    [Header("UI Hearts")]
    public Image[] hearts;

    [Header("Heart Sprites")]
    public Sprite fullHeart;
    public Sprite emptyHeart;

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHearts();
    }

    public void TakeDamage(int damage = 1)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHearts();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount = 1)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHearts();
    }

    void UpdateHearts()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] != null)
            {
                hearts[i].enabled = i < maxHealth;

                hearts[i].sprite =
                    i < currentHealth ? fullHeart : emptyHeart;
            }
        }
    }

    void Die()
    {
        isDead = true;
        Time.timeScale = 0f;
        Debug.Log("Player ตายแล้ว!");

        // เพิ่มระบบตายหรือ Game Over ตรงนี้
    }
}