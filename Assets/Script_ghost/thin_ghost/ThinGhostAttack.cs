using UnityEngine;

public class ThinGhostAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRange = 2f;
    public float attackCooldown = 1f;
    public int damage = 1;
    public float damageDelay = 1f; // ลดเลือดหลังเริ่มโจมตีกี่วินาที

    private float nextAttackTime;
    public Transform player;

    public void TryAttack(Transform player)
    {
        if (player == null)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > attackRange)
            return;

        if (Time.time < nextAttackTime)
            return;

        // ถ้ามี Animator ให้สั่งเล่นอนิเมชั่นตรงนี้ เช่น animator.SetTrigger("Attack");

        Invoke(nameof(DealDamage), damageDelay);
    }

    void DealDamage()
    {
        // ตั้ง cooldown หลังตีเสร็จ ไม่ว่าจะโดนหรือไม่
        nextAttackTime = Time.time + attackCooldown;

        if (player == null)
            return;

        if (Vector3.Distance(transform.position, player.position) > attackRange + 0.5f)
            return;

        // หลบเข้าตู้ทันก่อนโดนตี
        if (PlayerHide.IsPlayerHidden(player))
            return;

        PlayerHealth health = player.GetComponentInParent<PlayerHealth>();
        if (health != null)
        {
            health.TakeDamage(damage);
            Debug.Log("ThinGhostAttack: ผีโจมตีผู้เล่นแล้ว!");
        }
    }
}