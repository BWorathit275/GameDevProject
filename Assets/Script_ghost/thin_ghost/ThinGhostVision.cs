using UnityEngine;

public class ThinGhostVision : MonoBehaviour
{
    public Transform player;
    public float viewDistance = 10f;
    public float viewAngle = 120f;
    public float eyeHeight = 1.5f;
    public float playerTargetHeight = 1f;

    [Header("Debug")]
    public bool logVision = true; // เขียน Log เฉพาะตอนสถานะการมองเห็นเปลี่ยน

    private string lastLog;

    public bool CanSeePlayer()
    {
        if (player == null)
        {
            Debug.LogWarning("ThinGhostVision: ยังไม่ได้กำหนด Player!");
            return false;
        }

        // ซ่อนในตู้อยู่ ผีมองไม่เห็น
        if (PlayerHide.IsPlayerHidden(player))
        {
            LogOnChange("ThinGhostVision: ผู้เล่นซ่อนอยู่ในตู้");
            return false;
        }

        Vector3 eyePosition = transform.position + Vector3.up * eyeHeight;
        Vector3 targetPosition = player.position + Vector3.up * playerTargetHeight;
        Vector3 direction = targetPosition - eyePosition;

        if (direction.magnitude > viewDistance)
        {
            LogOnChange("ThinGhostVision: ผู้เล่นอยู่นอกระยะมองเห็น");
            return false;
        }

        if (Vector3.Angle(transform.forward, direction) > viewAngle / 2f)
        {
            LogOnChange("ThinGhostVision: ผู้เล่นอยู่นอกมุมมอง");
            return false;
        }

        if (Physics.Raycast(eyePosition, direction.normalized,
          out RaycastHit hit, viewDistance))
        {
            bool visible = hit.transform == player ||
                        hit.transform.IsChildOf(player);

            LogOnChange(visible
                ? "ThinGhostVision: เห็นผู้เล่นแล้ว!"
                : "ThinGhostVision: มีสิ่งกีดขวาง " + hit.transform.name);

            return visible;
        }

        LogOnChange("ThinGhostVision: Raycast ไม่พบเป้าหมาย");
        return false;
    }

    // ไม่เขียนซ้ำทุกเฟรม เขียนเฉพาะตอนข้อความเปลี่ยน
    void LogOnChange(string message)
    {
        if (!logVision || message == lastLog)
            return;

        lastLog = message;
        Debug.Log(message, this);
    }
}
