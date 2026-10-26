using UnityEngine;
using UnityEngine.AI;

public class GhostAnimation : MonoBehaviour
{
    private Animator animator;

    [Header("Animation Settings")]
    public float movementThreshold = 0.1f;
    public float animationDampTime = 0.1f;

    private static readonly int SpeedHash =
        Animator.StringToHash("Speed");

    private static readonly int ChasingHash =
        Animator.StringToHash("Chasing");

    private static readonly int AttackHash =
        Animator.StringToHash("Attack");

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();

        if (animator == null)
        {
            Debug.LogError(
                "GhostAnimation: ไม่พบ Animator ใน Ghost หรือ GameObject ลูก"
            );

            enabled = false;
            return;
        }

        animator.applyRootMotion = false;
    }

    public void SetChasing(bool chasing)
    {
        if (animator == null)
            return;

        animator.SetBool(ChasingHash, chasing);
    }

    public void PlayAttack()
    {
        if (animator == null)
            return;

        animator.ResetTrigger(AttackHash);
        animator.SetTrigger(AttackHash);
    }

    public void CancelAttack()
    {
        if (animator == null)
            return;

        animator.ResetTrigger(AttackHash);

        // กลับไปสถานะเคลื่อนไหว
        animator.SetBool(ChasingHash, true);
        animator.SetFloat(SpeedHash, 1f);

        animator.ResetTrigger("CancelAttack");
        animator.SetTrigger("CancelAttack");
    }

    public void UpdateAnimation(NavMeshAgent agent)
    {
        if (animator == null || agent == null)
            return;

        float speed = 0f;

        if (agent.isOnNavMesh && !agent.isStopped)
        {
            Vector3 velocity = agent.velocity;
            velocity.y = 0f;
            speed = velocity.magnitude;
        }

        if (speed < movementThreshold)
            speed = 0f;

        animator.SetFloat(
            SpeedHash,
            speed,
            animationDampTime,
            Time.deltaTime
        );
    }
}