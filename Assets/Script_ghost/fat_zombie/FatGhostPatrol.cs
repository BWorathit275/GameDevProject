using UnityEngine;
using UnityEngine.AI;

public class FatGhostPatrol : MonoBehaviour
{
    [Header("Patrol Points")]
    public Transform[] patrolPoints;

    [Header("Movement")]
    public float waitTime = 2f;
    public float reachDistance = 0.5f;

    private NavMeshAgent agent;
    private int currentPoint = 0;
    private float waitTimer = 0f;
    private bool waiting = false;

    void Awake()
    {
        // จุดลาดตระเวนที่เป็นลูกของผีจะขยับตามผีไปด้วย (เดินไปไม่ถึงสักที)
        // เลยถอดออกมาไว้กับที่ ตำแหน่งเดิมตอนเริ่มเกม
        if (patrolPoints != null)
            foreach (Transform p in patrolPoints)
                if (p != null && p.IsChildOf(transform))
                    p.SetParent(null, true);
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            Debug.LogError("FatGhostPatrol: Missing NavMeshAgent!", this);
            enabled = false;
            return;
        }

        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            Debug.LogWarning("FatGhostPatrol: No patrol points assigned!", this);
            enabled = false;
            return;
        }

        agent.isStopped = false;
        MoveToNextPoint();
    }

    void Update()
    {
        if (agent == null || patrolPoints == null || patrolPoints.Length == 0)
            return;

        if (agent.pathPending || !agent.isOnNavMesh)
            return;

        if (!waiting && agent.remainingDistance <= reachDistance)
        {
            waiting = true;
            waitTimer = waitTime;
            agent.isStopped = true;
        }

        if (waiting)
        {
            waitTimer -= Time.deltaTime;

            if (waitTimer <= 0f)
            {
                waiting = false;
                agent.isStopped = false;
                currentPoint = (currentPoint + 1) % patrolPoints.Length;
                MoveToNextPoint();
            }
        }
    }

    void MoveToNextPoint()
    {
        if (patrolPoints[currentPoint] != null)
        {
            agent.isStopped = false;
            agent.SetDestination(patrolPoints[currentPoint].position);
        }
    }
}