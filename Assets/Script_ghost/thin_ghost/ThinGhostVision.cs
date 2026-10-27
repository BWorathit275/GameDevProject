using UnityEngine;

public class ThinGhostVision : MonoBehaviour
{
    public Transform player;
    public float viewDistance = 10f;
    public float viewAngle = 120f;
    public float eyeHeight = 1.5f;
    public float playerTargetHeight = 1f;

    public bool CanSeePlayer()
    {
        if (player == null)
        {
            Debug.LogWarning("ThinGhostVision: ยังไม่ได้กำหนด Player!");
            return false;
        }
        Vector3 eyePosition = transform.position + Vector3.up * eyeHeight;
        Vector3 targetPosition = player.position + Vector3.up * playerTargetHeight;
        Vector3 direction = targetPosition - eyePosition;

        if (direction.magnitude > viewDistance)
        {
            Debug.Log("ThinGhostVision: ผู้เล่นอยู่นอกระยะมองเห็น");
            return false;
        }

        if (Vector3.Angle(transform.forward, direction) > viewAngle / 2f)
            return false;

        if (Physics.Raycast(eyePosition, direction.normalized,
          out RaycastHit hit, viewDistance))
        {
            bool visible = hit.transform == player ||
                        hit.transform.IsChildOf(player);

            Debug.Log(visible
                ? "ThinGhostVision: เห็นผู้เล่นแล้ว!"
                : "ThinGhostVision: มีสิ่งกีดขวาง " + hit.transform.name);

            return visible;
        }

        Debug.Log("ThinGhostVision: Raycast ไม่พบเป้าหมาย");
        return false;
    }
}