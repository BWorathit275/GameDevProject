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


    [Header("Blood Effect")]
    public BloodScreenHit bloodScreen;
    public Animator injuryAnimator;          // the InjuryEffect object
    public float injuryBlendTime = 0.5f;

    private float injury;

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHearts();
    }

    public void TakeDamage(int damage = 1, Transform attacker = null)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHearts();

        // เอฟเฟกต์เลือดบนจอ
        if (bloodScreen != null)
        {
            if (attacker != null) bloodScreen.Play(attacker.position); // สาดด้านที่ผีอยู่
            else bloodScreen.Play();                                   // สุ่มด้าน
        }

        if (currentHealth <= 0)
            Die();
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
    void Update()
    {
        // 0 = full hearts, 1 = last heart; eased so it fades in rather than snapping
        float target = (maxHealth - Mathf.Max(currentHealth, 1)) / (float)(maxHealth - 1);
        injury = Mathf.MoveTowards(injury, target, Time.unscaledDeltaTime / injuryBlendTime);
        if (injuryAnimator != null)
            injuryAnimator.SetFloat("Injury", injury);
    }

    void Die()
    {
        isDead = true;
        Time.timeScale = 0f;
        Debug.Log("Player ตายแล้ว!");

        // เพิ่มระบบตายหรือ Game Over ตรงนี้
    }
}