using System.Collections;
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
    public ParticleSystem bloodBody;         // the BloodBody object on the player
    [Tooltip("Blood that stays on screen at low hearts. Found under Blood Screen if left empty.")]
    public BloodLinger bloodLinger;
    public Animator injuryAnimator;          // the InjuryEffect object
    public float injuryBlendTime = 0.5f;

    private float injury;

    [Header("Regen")]
    [Tooltip("Seconds to regain 1 heart while out in the open.")]
    public float regenTime = 30f;
    [Tooltip("How much faster regen is while hiding (2 = twice as fast).")]
    public float hiddenRegenMultiplier = 2f;
    [Tooltip("If true, taking a hit restarts the regen countdown.")]
    public bool hitResetsRegen = true;

    [Header("Heal Feedback")]
    public float healPopScale = 1.4f;
    [Tooltip("Length of the whole lub-dub.")]
    public float healPopTime = 0.35f;
    public Color healFlashColor = new Color(1f, 0.6f, 0.6f, 1f);

    [Header("Game Over")]
    [Tooltip("Found in the scene if left empty.")]
    public GameOverScreen gameOverScreen;

    private PlayerHide playerHide;
    private float regenProgress;             // 0 to 1; one heart when it reaches 1

    private bool isDead = false;

    void Awake()
    {
        playerHide = GetComponent<PlayerHide>();

        if (bloodLinger == null && bloodScreen != null)
            bloodLinger = bloodScreen.GetComponentInChildren<BloodLinger>();
    }

    void Start()
    {
        currentHealth = maxHealth;

        // Continue จากเซฟ: ใช้หัวใจที่เซฟไว้แทนค่าเต็ม
        if (GameState.isContinue)
        {
            int saved = GameState.data.health;

            // กันเซฟเก่า/ค่าเริ่มต้นเป็น 0 แล้วเกิดมาตายทันที
            if (saved > 0)
                currentHealth = Mathf.Clamp(saved, 1, maxHealth);
        }

        UpdateHearts();

        // ให้เลือดค้างบนจอ/เอฟเฟกต์บาดเจ็บขึ้นทันที ไม่ต้องไล่ fade-in จาก 0
        injury = (maxHealth - Mathf.Max(currentHealth, 1)) / (float)(maxHealth - 1);
        if (injuryAnimator != null)
            injuryAnimator.SetFloat("Injury", injury);
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

        if (bloodBody != null)
            bloodBody.Play();

        if (hitResetsRegen)
            regenProgress = 0f;

        if (currentHealth <= 0)
            Die();
    }

    public void Heal(int amount = 1)
    {
        if (isDead) return;

        int before = currentHealth;
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHearts();

        // Pop each heart that just turned full.
        for (int i = before; i < currentHealth; i++)
        {
            if (i < hearts.Length && hearts[i] != null)
                StartCoroutine(PopHeart(hearts[i]));
        }
    }

    void UpdateRegen()
    {
        // Nothing to heal (or dead): keep the countdown at zero.
        if (isDead || currentHealth >= maxHealth)
        {
            regenProgress = 0f;
            return;
        }

        float speed = (playerHide != null && playerHide.IsHidden) ? hiddenRegenMultiplier : 1f;
        regenProgress += Time.deltaTime * speed / regenTime;

        if (regenProgress >= 1f)
        {
            regenProgress = 0f;
            Heal(1);
        }
    }

    IEnumerator PopHeart(Image heart)
    {
        Transform t = heart.transform;
        Vector3 baseScale = t.localScale;
        Color baseColor = heart.color;

        for (float time = 0f; time < healPopTime; time += Time.deltaTime)
        {
            float k = HeartbeatPulse(time / healPopTime);
            t.localScale = baseScale * Mathf.Lerp(1f, healPopScale, k);
            heart.color = Color.Lerp(baseColor, healFlashColor, k);
            yield return null;
        }

        t.localScale = baseScale;
        heart.color = baseColor;
    }

    // Lub-dub over 0..1: a strong beat, a short gap, then a weaker second beat.
    static float HeartbeatPulse(float u)
    {
        if (u < 0.4f)
            return Mathf.Sin(u / 0.4f * Mathf.PI);
        if (u >= 0.55f && u < 0.9f)
            return 0.6f * Mathf.Sin((u - 0.55f) / 0.35f * Mathf.PI);
        return 0f;
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

        if (bloodLinger != null)
            bloodLinger.SetHearts(currentHealth);
    }
    void Update()
    {
        UpdateRegen();

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

        if (gameOverScreen == null)
            gameOverScreen = FindAnyObjectByType<GameOverScreen>();
        if (gameOverScreen != null)
            gameOverScreen.Show();
    }
}