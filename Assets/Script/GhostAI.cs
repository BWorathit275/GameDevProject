using UnityEngine;
using UnityEngine.AI;

public class GhostAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    private NavMeshAgent agent;
    private GhostPatrol patrol;
    private GhostVision vision;
    private GhostAnimation ghostAnimation;
    private GhostSound ghostSound;

    [Header("Chase Settings")]
    public float chaseSpeed = 4f;
    public float patrolSpeed = 2f;
    public float losePlayerDistance = 15f;

    [Header("Attack Settings")]
    public float attackRange = 2f;
    public float attackPauseDuration = 1.5f;

    private bool chasing = false;
    private bool attacking = false;
    private bool attackCooldown = false;

    private float attackPauseTimer = 0f;

    public bool IsChasing => chasing;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        patrol = GetComponent<GhostPatrol>();
        vision = GetComponent<GhostVision>();
        ghostAnimation = GetComponent<GhostAnimation>();
        ghostSound = GetComponent<GhostSound>();
    }

    void Start()
    {
        if (agent == null || patrol == null ||
            vision == null || ghostAnimation == null ||
            ghostSound == null)
        {
            Debug.LogError(
                "GhostAI: ตรวจสอบ Components บน Creep2"
            );

            enabled = false;
            return;
        }

        if (!agent.isOnNavMesh)
        {
            Debug.LogError("GhostAI: ผีไม่ได้อยู่บน NavMesh");
            enabled = false;
            return;
        }

        agent.isStopped = false;
        agent.speed = patrolSpeed;

        patrol.StartPatrol();
        ghostSound.PlayWalkSound();
    }

    void Update()
    {
        if (agent == null || !agent.isOnNavMesh)
            return;

        if (player == null)
        {
            ReturnToPatrol();
            patrol.UpdatePatrol();
            ghostAnimation.UpdateAnimation(agent);
            return;
        }

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        bool canSeePlayer = vision.CanSeePlayer();

        // เริ่มไล่เมื่อเห็นผู้เล่น
        if (!chasing && canSeePlayer)
        {
            chasing = true;
            attacking = false;
            attackCooldown = false;
            attackPauseTimer = 0f;

            agent.isStopped = false;
            agent.ResetPath();
            agent.speed = chaseSpeed;

            ghostAnimation.SetChasing(true);
            ghostSound.PlayRunSound();

            Debug.Log("Ghost: เริ่มไล่ผู้เล่น");
        }

        // ผู้เล่นหนีไกลเกินไป
        if (chasing && distance > losePlayerDistance)
        {
            ReturnToPatrol();
            patrol.UpdatePatrol();
            ghostAnimation.UpdateAnimation(agent);
            return;
        }

        if (chasing)
        {
            // ถ้ากำลังพักหลังโจมตี ให้พักตามเวลาที่กำหนด
            if (attackCooldown)
            {
                agent.isStopped = true;
                attackPauseTimer -= Time.deltaTime;

                if (attackPauseTimer <= 0f)
                {
                    attackCooldown = false;
                    attacking = false;

                    // สั่งออกจากสถานะ Attack
                    ghostAnimation.CancelAttack();

                    // เริ่มไล่ Player อีกครั้ง
                    agent.isStopped = false;
                    agent.speed = chaseSpeed;
                    agent.SetDestination(player.position);

                    ghostAnimation.SetChasing(true);
                    ghostSound.StopAttackSound();
                    ghostSound.PlayRunSound();

                    Debug.Log("Ghost: หมดเวลาพัก กลับมาไล่ต่อ");
                }
            }
            // โจมตีหนึ่งครั้งเมื่อเข้าระยะ
            else if (!attacking && distance <= attackRange)
            {
                attacking = true;

                agent.isStopped = true;
                agent.ResetPath();

                ghostSound.StopMovementSound();
                ghostAnimation.PlayAttack();
                ghostSound.PlayAttackSound();

                attackCooldown = true;
                attackPauseTimer = attackPauseDuration;

                Debug.Log("Ghost: โจมตีหนึ่งครั้ง แล้วพัก");
            }
            else if (!attacking)
            {
                // ไล่ผู้เล่นตลอด แม้ผู้เล่นจะยืนนิ่ง
                agent.isStopped = false;
                agent.speed = chaseSpeed;
                agent.SetDestination(player.position);
                ghostSound.PlayRunSound();
            }
        }
        else
        {
            patrol.UpdatePatrol();
        }

        ghostAnimation.UpdateAnimation(agent);
    }

    private void ReturnToPatrol()
    {
        chasing = false;
        attacking = false;
        attackCooldown = false;
        attackPauseTimer = 0f;

        agent.isStopped = false;
        agent.ResetPath();
        agent.speed = patrolSpeed;

        ghostAnimation.SetChasing(false);
        ghostAnimation.CancelAttack();

        ghostSound.StopAttackSound();

        patrol.ReturnToNearestPoint();
        ghostSound.PlayWalkSound();

        Debug.Log("Ghost: กลับไป Patrol");
    }
}