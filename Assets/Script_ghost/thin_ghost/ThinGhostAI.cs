
using UnityEngine;
using UnityEngine.AI;

public class ThinGhostAI : MonoBehaviour
{
    [Header("Components")]
    public ThinGhostVision vision;
    public ThinGhostPatrol patrol;
    public ThinGhostAnimation ghostAnimation;
    public ThinGhostSound ghostSound;

    [Header("Movement")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float losePlayerDistance = 15f;

    
    [Header("Attack")]
    public float attackRange = 0.8f;
    public float attackCooldown = 2f;
    public float attackAnimationDelay = 0.8f;
    public float pauseMovementAfterAttack = 1.5f;
    public float damageDelay = 1f;
    [SerializeField] ThinGhostAttack attack;


    [Header("Search")]
    public float searchDuration = 3f;

    private NavMeshAgent agent;
    private Transform player;

    private bool chasing;
    private bool searching;
    private bool attacking;

    private float nextAttackTime;
    private float searchTimer;
    private float attackTimer;

    private Vector3 lastKnownPlayerPosition;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (vision == null)
            vision = GetComponent<ThinGhostVision>();

        if (patrol == null)
            patrol = GetComponent<ThinGhostPatrol>();

        if (ghostAnimation == null)
            ghostAnimation = GetComponent<ThinGhostAnimation>();

        if (ghostSound == null)
            ghostSound = GetComponent<ThinGhostSound>();
    }

    void Start()
    {
        if (agent == null || vision == null || patrol == null)
        {
            Debug.LogError(
                "ThinGhostAI: ต้องมี NavMeshAgent, ThinGhostVision และ ThinGhostPatrol",
                this
            );
            enabled = false;
            return;
        }

        player = vision.player;

        if (player == null)
        {
            Debug.LogError(
                "ThinGhostAI: กรุณากำหนด Player ใน ThinGhostVision",
                this
            );
            enabled = false;
            return;
        }

        if (!agent.isOnNavMesh)
        {
            Debug.LogError("ThinGhostAI: ผีไม่ได้อยู่บน NavMesh", this);
            enabled = false;
            return;
        }

        chasing = false;
        searching = false;
        attacking = false;

        agent.speed = patrolSpeed;
        agent.isStopped = false;

        if (ghostAnimation != null)
            ghostAnimation.SetChasing(false);

        if (ghostSound != null)
        {
            ghostSound.StopScream();
            ghostSound.StopWalk();
        }

        patrol.enabled = true;
        patrol.StartPatrol();
    }

    void Update()
    {
        if (agent == null || !agent.isOnNavMesh || player == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        bool canSeePlayer = vision.CanSeePlayer();

        // จำตำแหน่งล่าสุดของผู้เล่นที่มองเห็น
        if (canSeePlayer)
        {
            lastKnownPlayerPosition = player.position;
            searchTimer = 0f;
            searching = false;

            if (!chasing)
            {
                chasing = true;

                if (ghostSound != null)
                {
                    ghostSound.StopWalk();
                    ghostSound.PlayScream();

                    Debug.Log("ThinGhostAI: เริ่มไล่ผู้เล่น สั่งเล่นเสียง Scream", this);
                }
                else
                {
                    Debug.LogError("ThinGhostAI: ไม่พบ ThinGhostSound!", this);
                }
            }
        }

        // ถ้าผู้เล่นอยู่ไกลเกินไป ให้กลับไปลาดตระเวน
        if (distance > losePlayerDistance)
        {
            ReturnToPatrol();
            return;
        }

        // จัดการการโจมตีและหยุดการเคลื่อนที่ชั่วคราว
        if (attacking)
        {
            attackTimer -= Time.deltaTime;

            agent.isStopped = true;
            StopWalkSound();

            // ให้ Scream เล่นต่อขณะโจมตี
            if (ghostSound != null && chasing && canSeePlayer)
                ghostSound.PlayScream();

            if (attackTimer > 0f)
                return;

            attacking = false;
            agent.isStopped = false;
        }

        // สถานะลาดตระเวน
        if (!chasing && !searching)
        {
            agent.speed = patrolSpeed;
            agent.isStopped = false;

            if (!patrol.enabled)
            {
                patrol.enabled = true;
                patrol.StartPatrol();
            }

            if (ghostAnimation != null)
                ghostAnimation.SetChasing(false);

            UpdateWalkSound();
            return;
        }

        // สถานะไล่ผู้เล่น
        if (chasing)
        {
            if (patrol.enabled)
                patrol.enabled = false;

            agent.speed = chaseSpeed;

            if (ghostAnimation != null)
                ghostAnimation.SetChasing(true);

            // ไล่ผู้เล่น
            if (chasing)
            {
                if (patrol.enabled)
                    patrol.enabled = false;

                agent.speed = chaseSpeed;

                if (ghostAnimation != null)
                    ghostAnimation.SetChasing(true);

                // ถึงตัวผู้เล่นแล้วจึงโจมตี
                if (distance <= attackRange)
                {
                    agent.isStopped = true;
                    StopWalkSound();

                    if (!attacking && Time.time >= nextAttackTime)
                    {
                        attack.TryAttack(player);
                        attacking = true;
                        nextAttackTime = Time.time + attackCooldown;

                        attackTimer = attackAnimationDelay
                            + pauseMovementAfterAttack;

                        if (ghostAnimation != null)
                            ghostAnimation.PlayAttack();

                        if (ghostSound != null)
                        {
                            ghostSound.PlayAttack();
                            Debug.Log("ThinGhostAI: สั่งเล่นเสียง Attack", this);
                        }
                        else
                        {
                            Debug.LogError("ThinGhostAI: ไม่พบ ThinGhostSound!", this);
                        }
                    }

                    return;
                }

                // ยังไม่ถึงตัวผู้เล่น ให้ไล่ต่อ
                if (canSeePlayer)
                {
                    agent.isStopped = false;
                    agent.SetDestination(player.position);

                    // เมื่อกำลังไล่ ให้หยุด Walk และคง Scream ไว้
                    StopWalkSound();
                }
                else
                {
                    chasing = false;
                    searching = true;
                    searchTimer = 0f;

                    if (ghostSound != null)
                    {
                        ghostSound.StopScream();
                        ghostSound.StopWalk();
                    }

                    agent.isStopped = false;
                    agent.SetDestination(lastKnownPlayerPosition);
                }

                return;
            }
           

            // ถ้ายังมองเห็นผู้เล่น ให้ไล่ต่อ
            if (canSeePlayer)
            {
                agent.isStopped = false;
                agent.SetDestination(player.position);
                UpdateWalkSound();
            }
            else
            {
                // ถ้ามองไม่เห็น ให้ไปยังตำแหน่งสุดท้ายที่เห็น
                chasing = false;
                searching = true;
                searchTimer = 0f;

                if (ghostSound != null)
                {
                    ghostSound.StopScream();
                    ghostSound.StopWalk();
                }

                agent.isStopped = false;
                agent.SetDestination(lastKnownPlayerPosition);
            }

            return;
        }

        // สถานะค้นหาผู้เล่น
        if (searching)
        {
            if (patrol.enabled)
                patrol.enabled = false;

            agent.speed = chaseSpeed;
            agent.isStopped = false;

            if (ghostAnimation != null)
                ghostAnimation.SetChasing(true);

            searchTimer += Time.deltaTime;
            UpdateWalkSound();

            // พบผู้เล่นอีกครั้ง
            if (canSeePlayer)
            {
                searching = false;
                chasing = true;

                if (ghostSound != null)
                    ghostSound.PlayScream();

                return;
            }

            // ค้นหาครบเวลา หรือถึงตำแหน่งสุดท้ายแล้ว
            if (searchTimer >= searchDuration ||
                (!agent.pathPending &&
                 agent.remainingDistance <= agent.stoppingDistance + 0.2f))
            {
                ReturnToPatrol();
            }
        }
    }

    void UpdateWalkSound()
    {
        if (ghostSound == null)
            return;

        if (!agent.isStopped && agent.velocity.magnitude > 0.15f)
            ghostSound.StartWalk();
        else
            ghostSound.StopWalk();
    }

    void StopWalkSound()
    {
        if (ghostSound != null)
            ghostSound.StopWalk();
    }

    void ReturnToPatrol()
    {
        chasing = false;
        searching = false;
        attacking = false;
        searchTimer = 0f;

        agent.isStopped = false;
        agent.speed = patrolSpeed;

        if (ghostAnimation != null)
            ghostAnimation.SetChasing(false);

        if (ghostSound != null)
        {
            ghostSound.StopScream();
            ghostSound.StopWalk();
        }

        patrol.enabled = true;
        patrol.ReturnToNearestPoint();
    }

    void OnDisable()
    {
        if (ghostSound != null)
        {
            ghostSound.StopScream();
            ghostSound.StopWalk();
        }
    }
}
