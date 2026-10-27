using UnityEngine;

public class WoodBarrier : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] KeyCode interactKey = KeyCode.E;
    [SerializeField] GameObject[] planks;       // แผ่นไม้ทุกแผ่น
    [SerializeField] float pryCooldown = 0.8f;  // หน่วงระหว่างงัดแต่ละแผ่น
    [SerializeField] float launchForce = 3f;    // แรงดีดแผ่นไม้ (ถ้ามี Rigidbody)

    [Header("Sound (ไม่บังคับ)")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip prySound;

    int planksLeft;
    float nextPryTime;
    bool playerInRange;
    PlayerInteraction player;

    public bool IsCleared => planksLeft <= 0;

    void Start()
    {
        planksLeft = planks.Length;
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

        GameObject plank = planks[planksLeft];

        if (audioSource != null && prySound != null)
            audioSource.PlayOneShot(prySound);

        Rigidbody rb = plank.GetComponent<Rigidbody>();

        if (rb != null)
        {
            // แผ่นไม้ร่วงแบบมีฟิสิกส์
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