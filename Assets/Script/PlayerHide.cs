using UnityEngine;

public class PlayerHide : MonoBehaviour
{
    public bool IsHidden { get; private set; }

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
        lockerLook.enabled = false;

        transform.SetPositionAndRotation(exitPoint.position, exitPoint.rotation);
        if (controller) controller.enabled = true;

        movementScript.enabled = true;
    }
}