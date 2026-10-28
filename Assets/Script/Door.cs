using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour
{
    [Header("Open / Close")]
    [SerializeField] KeyCode interactKey = KeyCode.E;
    [SerializeField] float openAngle = 90f;   // ใส่ค่าลบถ้าเปิดผิดทิศ
    [SerializeField] float openSpeed = 2f;

    [Header("Lock")]
    public bool startLocked = true;
    public string requiredKeyId = "KEY1";     // ต้องตรงกับ Item Name ของกุญแจ
    public bool consumeKey = false;           // ติ๊กถ้าอยากให้กุญแจหายหลังไข

    [Header("Wood Barrier (ไม่ใช้ก็เว้นว่าง)")]
    public WoodBarrier barrier;

    [Header("Sound (ไม่บังคับ)")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip openSound;
    [SerializeField] AudioClip closeSound;
    [SerializeField] AudioClip lockedSound;
    [SerializeField] AudioClip unlockSound;

    bool isLocked;
    bool isOpen;
    bool isMoving;
    bool playerInRange;

    PlayerInteraction player;
    Quaternion closedRot;
    Quaternion openRot;

    void Start()
    {
        isLocked = startLocked;
        closedRot = transform.localRotation;
        openRot = closedRot * Quaternion.Euler(0f, openAngle, 0f);
    }

    // เรียกจากสคริปต์อื่นได้ ถ้าอยากปลดล็อกด้วยวิธีอื่น (เช่น สวิตช์)
    public void UnlockDoor()
    {
        isLocked = false;
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
        if (!playerInRange || isMoving || !Input.GetKeyDown(interactKey))
            return;

        // 1) ไม้ยังตอกประตูอยู่
        if (barrier != null && !barrier.IsCleared)
        {
            Debug.Log("ประตูถูกตอกไม้ไว้ ต้องงัดออกก่อน");
            return;
        }

        // 2) ประตูล็อกอยู่
        if (isLocked)
        {
            if (player != null && player.IsHoldingKey(requiredKeyId))
            {
                isLocked = false;
                Play(unlockSound);
                Debug.Log("ปลดล็อกประตูแล้ว");

                if (consumeKey)
                    player.ConsumeHeld();
            }
            else
            {
                Play(lockedSound);
                Debug.Log("ประตูล็อกอยู่ ต้องถือกุญแจ " + requiredKeyId);
                return;
            }
        }

        // 3) เปิดหรือปิดประตู
        StartCoroutine(Toggle());
    }

    IEnumerator Toggle()
    {
        isMoving = true;

        Play(isOpen ? closeSound : openSound);

        Quaternion from = transform.localRotation;
        Quaternion to = isOpen ? closedRot : openRot;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * openSpeed;
            transform.localRotation = Quaternion.Slerp(from, to, t);
            yield return null;
        }

        transform.localRotation = to;
        isOpen = !isOpen;
        isMoving = false;
    }

    void Play(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}