using UnityEngine;

public class Locker : MonoBehaviour
{
    [SerializeField] Transform hidePoint;
    [SerializeField] Transform exitPoint;
    [SerializeField] KeyCode interactKey = KeyCode.E;
    [SerializeField] Animator doorAnimator; // ไม่บังคับ

    PlayerHide player;
    PlayerHealth health;
    PlayerInteraction interaction;
    bool playerInRange;
    bool occupied;

    void Start()
    {
        Debug.Log("Locker พร้อมทำงาน: " + gameObject.name);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Trigger enter: " + other.name + " tag=" + other.tag);
            player = other.GetComponent<PlayerHide>();
            health = other.GetComponentInParent<PlayerHealth>();
            interaction = other.GetComponentInParent<PlayerInteraction>();
            playerInRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && !occupied)
            playerInRange = false;
    }

    void Update()
    {
        if (!playerInRange || !Input.GetKeyDown(interactKey)) return;

        // ตายแล้วใช้ตู้ไม่ได้
        if (health != null && health.IsDead) return;

        // เล็งไอเทมอยู่ ให้ E เก็บของแทนการเข้าตู้
        if (!occupied && interaction != null && interaction.IsAimingAtItem) return;

        if (!occupied) Enter();
        else Exit();
    }

    void Enter()
    {
        occupied = true;
        if (doorAnimator) doorAnimator.SetTrigger("Open");
        player.EnterHiding(hidePoint);
    }

    void Exit()
    {
        occupied = false;
        player.ExitHiding(exitPoint);
        if (doorAnimator) doorAnimator.SetTrigger("Open");
    }
}