using UnityEngine;
using UnityEngine.AI;

public class ThinGhostPatrol : MonoBehaviour
{
    public Transform[] patrolPoints;
    public float waitTime = 2f;
    public bool randomOrder = true;   // สุ่มจุดถัดไป (ไม่ซ้ำจุดเดิม) แทนการเดินตามลำดับ

    private NavMeshAgent agent;
    private int currentPoint = 0;
    private float waitTimer = 0f;
    private bool waiting = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        // จุดลาดตระเวนที่เป็นลูกของผีจะขยับตามผีไปด้วย (เดินไปไม่ถึงสักที)
        // เลยถอดออกมาไว้กับที่ ตำแหน่งเดิมตอนเริ่มเกม
        if (patrolPoints != null)
            foreach (Transform p in patrolPoints)
                if (p != null && p.IsChildOf(transform))
                    p.SetParent(null, true);
    }

    void Start()
    {
        StartPatrol();
    }

    public void StartPatrol()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (agent == null || !agent.isOnNavMesh)
        {
            Debug.LogError("ThinGhostPatrol: Agent ไม่อยู่บน NavMesh หรือไม่มี Agent!");
            enabled = false;
            return;
        }

        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            Debug.LogError("ThinGhostPatrol: ยังไม่ได้กำหนด Patrol Points!");
            enabled = false;
            return;
        }

        currentPoint = 0;
        waiting = false;
        waitTimer = 0f;
        GoToNextPoint();
    }

    void Update()
    {
        if (agent == null || !agent.isOnNavMesh)
            return;

        if (waiting)
        {
            waitTimer += Time.deltaTime;

            if (waitTimer >= waitTime)
            {
                waiting = false;
                GoToNextPoint();
            }

            return;
        }

        if (!agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance + 0.2f)
        {
            waiting = true;
            waitTimer = 0f;
        }
    }

    void GoToNextPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        if (patrolPoints[currentPoint] == null)
        {
            Debug.LogError("ThinGhostPatrol: มี Patrol Point ที่ไม่ได้กำหนด!");
            enabled = false;
            return;
        }

        agent.SetDestination(patrolPoints[currentPoint].position);
        currentPoint = PickNextPoint(currentPoint);
    }

    int PickNextPoint(int current)
    {
        if (!randomOrder || patrolPoints.Length < 2)
            return (current + 1) % patrolPoints.Length;

        // สุ่มจุดอื่นที่ไม่ใช่จุดที่กำลังเดินไป
        int next = Random.Range(0, patrolPoints.Length - 1);
        return next >= current ? next + 1 : next;
    }

    public void ReturnToNearestPoint()
    {
        if (agent == null || !agent.isOnNavMesh ||
            patrolPoints == null || patrolPoints.Length == 0)
            return;

        float nearestDistance = Mathf.Infinity;
        int nearestIndex = 0;

        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] == null)
                continue;

            float distance = Vector3.Distance(
                transform.position, patrolPoints[i].position);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestIndex = i;
            }
        }

        currentPoint = nearestIndex;
        waiting = false;
        waitTimer = 0f;
        GoToNextPoint();
    }

    public void UpdatePatrol()
    {
        // การเดินลาดตระเวนถูกจัดการใน Update()
    }
}