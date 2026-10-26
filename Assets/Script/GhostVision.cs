using UnityEngine;

public class GhostVision : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    [Header("Vision Settings")]
    public float viewDistance = 10f;
    public float eyeHeight = 1.5f;
    public float playerTargetHeight = 1f;

    public bool CanSeePlayer()
    {
        if (player == null)
            return false;

        Vector3 origin =
            transform.position + Vector3.up * eyeHeight;

        Vector3 target =
            player.position + Vector3.up * playerTargetHeight;

        Vector3 direction = target - origin;
        float distance = direction.magnitude;

        if (distance > viewDistance)
            return false;

        if (distance <= 0.001f)
            return true;

        if (Physics.Raycast(
            origin,
            direction.normalized,
            out RaycastHit hit,
            distance + 0.2f))
        {
            if (hit.transform == player ||
                hit.transform.IsChildOf(player) ||
                hit.transform.root == player.root)
            {
                return true;
            }
        }

        return false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            viewDistance
        );
    }
}