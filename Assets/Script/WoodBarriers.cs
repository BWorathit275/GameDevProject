using UnityEngine;

public class WoodBarrier : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] KeyCode interactKey = KeyCode.E;
    [SerializeField] GameObject[] planks;       // แผ่นไม้ทุกแผ่น
    [SerializeField] float pryCooldown = 0.8f;  // หน่วงระหว่างงัดแต่ละแผ่น
    [SerializeField] float launchForce = 3f;    // แรงดีดแผ่นไม้ (ถ้ามี Rigidbody)

    [Header("Save")]
    [Tooltip("ไม่ซ้ำกันทุกกลุ่มไม้ เช่น barrier_hall (ระบบจะต่อ _0, _1, ... ให้เอง)")]
    public string barrierId;

    [Header("Sound (ไม่บังคับ)")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip prySound;

    int planksLeft;
    float nextPryTime;
    bool playerInRange;
    PlayerInteraction player;

    public bool IsCleared => planksLeft <= 0;

    string PlankId(int index) => barrierId + "_" + index;

    void Start()
    {
        planksLeft = planks.Length;

        if (!GameState.isContinue || string.IsNullOrEmpty(barrierId)) return;

        // Continue: แผ่นที่เคยงัดแล้วให้หายไป
        for (int i = 0; i < planks.Length; i++)
        {
            if (GameState.IsUsed(PlankId(i)))
            {
                planks[i].SetActive(false);
                planksLeft--;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        PlayerInteraction p = other.GetComponentInParent<PlayerInteraction>();
        if (p == null) return;

        player = p;
        playerInRange = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<PlayerInteraction>() != null)
            playerInRange = false;
    }

    void Update()
    {
        if (IsCleared || !playerInRange || !Input.GetKeyDown(interactKey))
            return;

        if (Time.time < nextPryTime)
            return;

        if (player == null || !player.IsHolding(ItemType.Crowbar))
        {
            Debug.Log("ต้องถือชะแลงเพื่องัดไม้");
            return;
        }

        nextPryTime = Time.time + pryCooldown;
        planksLeft--;

        // งัดจากแผ่นท้ายสุดก่อน ดังนั้นแผ่นที่ถูกใช้จะเป็นช่วงท้ายของ array เสมอ
        if (!string.IsNullOrEmpty(barrierId))
            GameState.MarkUsed(PlankId(planksLeft));
        else
            Debug.LogWarning("WoodBarrier: ยังไม่ได้ตั้ง barrierId จึงเซฟไม้ไม่ได้", this);

        GameObject plank = planks[planksLeft];

        if (audioSource != null && prySound != null)
            audioSource.PlayOneShot(prySound);

        Rigidbody rb = plank.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = false;

            Vector3 dir = (plank.transform.position - player.transform.position).normalized;
            rb.AddForce(dir * launchForce, ForceMode.Impulse);

            Destroy(plank, 3f);
        }
        else
        {
            plank.SetActive(false);
        }
    }
}