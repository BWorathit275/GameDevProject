
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
    public float searchDuration = 3f;   // เวลาเดินหารอบจุดที่เห็นผู้เล่นล่าสุด ก่อนกลับไปลาดตระเวน
    public float loseSightGrace = 1f;   // คลาดสายตาแล้วยังตามผู้เล่นต่ออีกกี่วินาที (เหมือนได้ยินเสียง)
    public float searchRadius = 4f;     // รัศมีที่เดินหารอบจุดที่เห็นล่าสุด

    private NavMeshAgent agent;
    private Transform player;

    private bool chasing;
    private bool searching;

    // ให้สคริปต์อื่น (เช่นเสียงบรรยากาศ) รู้ว่าผีกำลังไล่อยู่
    public bool IsChasing => chasing;
    private bool attacking;

    private float nextAttackTime;
    private float searchTimer;
    private float attackTimer;

    private Vector3 lastKnownPlayerPosition;
    private float lastSeenTime = -999f;
    private bool reachedLastKnown;
    private float searchStartTime;
    private float searchPointGiveUpTime;

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
        bool playerHidden = PlayerHide.IsPlayerHidden(player);

        // จำตำแหน่งล่าสุดของผู้เล่นที่มองเห็น
        if (canSeePlayer)
        {
            lastKnownPlayerPosition = player.position;
            lastSeenTime = Time.time;
            searchTimer = 0f;
            searching = false;

            if (!chasing)
            {
                chasing = true;

                if (ghostSound != null)
                {
                    ghostSound.SetSearching(false);
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

        // ถ้าผู้เล่นอยู่ไกลเกินไป ให้เลิกไล่แล้วกลับไปลาดตระเวน
        // (เฉพาะตอนไล่หรือค้นหาอยู่ ไม่งั้นผีจะถูกสั่งกลับจุดใกล้สุดทุกเฟรมและไม่เดินลาดตระเวน)
        if (distance > losePlayerDistance && (chasing || searching))
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

            // ถึงตัวผู้เล่นแล้วจึงโจมตี (ไม่โจมตีถ้าซ่อนในตู้)
            if (distance <= attackRange && !playerHidden)
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

            // ยังเห็นอยู่ หรือเพิ่งคลาดสายตาไม่นาน (และไม่ได้ซ่อนในตู้) ให้ไล่ต่อ
            bool stillTracking = canSeePlayer ||
                (!playerHidden && Time.time - lastSeenTime < loseSightGrace);

            if (stillTracking)
            {
                // ตามไปทางที่ผู้เล่นหนีไป แม้จะเลี้ยวพ้นสายตาแล้ว
                lastKnownPlayerPosition = player.position;

                agent.isStopped = false;
                agent.SetDestination(player.position);

                // เมื่อกำลังไล่ ให้หยุด Walk และคง Scream ไว้
                StopWalkSound();
            }
            else
            {
                StartSearch();
            }

            return;
        }

        // สถานะค้นหาผู้เล่น
        if (searching)
        {
            if (patrol.enabled)
                patrol.enabled = false;

            agent.isStopped = false;
            UpdateWalkSound();

            // วิ่งไปยังจุดที่เห็นผู้เล่นล่าสุดก่อน
            if (!reachedLastKnown)
            {
                agent.speed = chaseSpeed;

                if (ghostAnimation != null)
                    ghostAnimation.SetChasing(true);

                bool arrived = !agent.pathPending &&
                    agent.remainingDistance <= agent.stoppingDistance + 0.3f;

                // ไปไม่ถึงภายใน 8 วินาที ก็เริ่มหาจากตรงนี้เลย
                if (arrived || Time.time - searchStartTime > 8f)
                {
                    reachedLastKnown = true;
                    GoToSearchPoint();
                }

                return;
            }

            // ถึงแล้ว เดินหารอบๆ จุดนั้นสักพัก
            agent.speed = patrolSpeed;

            if (ghostAnimation != null)
                ghostAnimation.SetChasing(false);

            searchTimer += Time.deltaTime;

            if (searchTimer >= searchDuration)
            {
                ReturnToPatrol();
                return;
            }

            bool atSearchPoint = !agent.pathPending &&
                agent.remainingDistance <= agent.stoppingDistance + 0.3f;

            if (atSearchPoint || Time.time >= searchPointGiveUpTime)
                GoToSearchPoint();
        }
    }

    void StartSearch()
    {
        chasing = false;
        searching = true;
        searchTimer = 0f;
        reachedLastKnown = false;
        searchStartTime = Time.time;

        if (ghostSound != null)
        {
            ghostSound.StopScream();
            ghostSound.StopWalk();
            ghostSound.SetSearching(true);
        }

        agent.isStopped = false;
        agent.SetDestination(lastKnownPlayerPosition);
    }

    // สุ่มจุดใกล้ๆ ตำแหน่งที่เห็นผู้เล่นล่าสุด (เดินไปถึงได้จริง และไม่อ้อมไกล) แล้วเดินไปดู
    void GoToSearchPoint()
    {
        for (int i = 0; i < 6; i++)
        {
            Vector2 offset = Random.insideUnitCircle * searchRadius;
            Vector3 candidate = lastKnownPlayerPosition + new Vector3(offset.x, 0f, offset.y);

            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, 1.5f, NavMesh.AllAreas))
                continue;

            NavMeshPath path = new NavMeshPath();
            if (!agent.CalculatePath(hit.position, path) || path.status != NavMeshPathStatus.PathComplete)
                continue;

            if (PathLength(path) > searchRadius * 2.5f)
                continue;

            agent.SetPath(path);
            searchPointGiveUpTime = Time.time + 4f;
            return;
        }

        // หาจุดไม่ได้ ให้วนอยู่แถวจุดเดิม
        agent.SetDestination(lastKnownPlayerPosition);
        searchPointGiveUpTime = Time.time + 1f;
    }

    static float PathLength(NavMeshPath path)
    {
        float length = 0f;
        for (int i = 1; i < path.corners.Length; i++)
            length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
        return length;
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
            ghostSound.SetSearching(false);
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
