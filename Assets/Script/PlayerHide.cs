using UnityEngine;

public class PlayerHide : MonoBehaviour
{
    public bool IsHidden { get; private set; }

    // True if this transform belongs to a player hiding in a locker. Ghosts use it to ignore hidden players.
    public static bool IsPlayerHidden(Transform player)
    {
        if (player == null) return false;
        PlayerHide hide = player.GetComponentInParent<PlayerHide>();
        return hide != null && hide.IsHidden;
    }

    [SerializeField] MonoBehaviour movementScript;   // สคริปต์เดินของคุณ
    [SerializeField] CharacterController controller; // ถ้าใช้ CharacterController
    [SerializeField] LockerLook lockerLook;

    public void EnterHiding(Transform hidePoint)
    {
        IsHidden = true;
        movementScript.enabled = false;

        if (controller) controller.enabled = false; // ต้องปิดก่อนย้ายตำแหน่ง
        transform.SetPositionAndRotation(hidePoint.position, hidePoint.rotation);
        
        lockerLook.enabled = true;
    }

    public void ExitHiding(Transform exitPoint)
    {
        IsHidden = false;
        lockerLook.enabled = false;

        transform.SetPositionAndRotation(exitPoint.position, exitPoint.rotation);
        if (controller) controller.enabled = true;

        movementScript.enabled = true;
    }
}