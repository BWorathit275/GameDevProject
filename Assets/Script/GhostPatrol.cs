
using UnityEngine;
using UnityEngine.AI;

public class GhostPatrol : MonoBehaviour
{
    [Header("Patrol Points")]
    public Transform[] patrolPoints;

    private NavMeshAgent agent;
    private int currentPoint = -1;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void StartPatrol()
    {
        if (!CanPatrol())
            return;

        currentPoint = FindNearestPoint();

        if (currentPoint >= 0)
            GoToPatrolPoint();
    }

    public void UpdatePatrol()
    {
        if (!CanPatrol())
            return;

        if (currentPoint < 0 ||
            currentPoint >= patrolPoints.Length ||
            patrolPoints[currentPoint] == null)
        {
            currentPoint = FindNearestPoint();

            if (currentPoint < 0)
                return;

            GoToPatrolPoint();
            return;
        }

        if (agent.pathPending)
            return;

        if (!agent.hasPath ||
            agent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            GoToPatrolPoint();
            return;
        }

        if (agent.remainingDistance <= agent.stoppingDistance + 0.2f)
        {
            int nextPoint = FindNextValidPoint(currentPoint);

            if (nextPoint >= 0)
            {
                currentPoint = nextPoint;
                GoToPatrolPoint();
            }
        }
    }

    public void ReturnToNearestPoint()
    {
        if (!CanPatrol())
            return;

        currentPoint = FindNearestPoint();

        if (currentPoint >= 0)
        {
            agent.isStopped = false;
            agent.ResetPath();
            GoToPatrolPoint();
        }
    }

    bool CanPatrol()
    {
        if (agent == null || !agent.isOnNavMesh)
            return false;

        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            Debug.LogWarning(
                "GhostPatrol: ยังไม่ได้กำหนด Patrol Points"
            );
            return false;
        }

        return true;
    }

    void GoToPatrolPoint()
    {
        if (!CanPatrol())
            return;

        if (currentPoint < 0 ||
            currentPoint >= patrolPoints.Length ||
            patrolPoints[currentPoint] == null)
        {
            Debug.LogWarning(
                "GhostPatrol: Patrol Point ไม่ถูกต้อง"
            );
            return;
        }

        agent.isStopped = false;

        NavMeshPath path = new NavMeshPath();

        bool pathFound = agent.CalculatePath(
            patrolPoints[currentPoint].position,
            path
        );

        if (pathFound &&
            path.status == NavMeshPathStatus.PathComplete)
        {
            agent.SetPath(path);
        }
        else
        {
            Debug.LogWarning(
                "GhostPatrol: ไม่มีเส้นทางสมบูรณ์ไปยัง Patrol Point " +
                currentPoint
            );

            int nextPoint = FindNextValidPoint(currentPoint);

            if (nextPoint >= 0 && nextPoint != currentPoint)
            {
                currentPoint = nextPoint;
                GoToPatrolPoint();
            }
        }
    }

    int FindNearestPoint()
    {
        int nearestPoint = -1;
        float nearestDistance = Mathf.Infinity;

        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] == null)
                continue;

            float distance = Vector3.Distance(
                transform.position,
                patrolPoints[i].position
            );

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestPoint = i;
            }
        }

        return nearestPoint;
    }

    int FindNextValidPoint(int startIndex)
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return -1;

        for (int i = 1; i <= patrolPoints.Length; i++)
        {
            int index = (startIndex + i) % patrolPoints.Length;

            if (patrolPoints[index] != null)
                return index;
        }

        return -1;
    }
}
