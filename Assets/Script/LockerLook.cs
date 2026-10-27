using UnityEngine;

public class LockerLook : MonoBehaviour
{
    [SerializeField] Transform cameraTransform; // กล้องของผู้เล่น (ลูกของ Player)
    [SerializeField] float sensitivity = 2f;
    [SerializeField] float maxYaw = 45f;   // หันซ้าย-ขวาได้กี่องศา
    [SerializeField] float maxPitch = 30f; // เงยก้มได้กี่องศา

    float yaw, pitch;
    float baseYaw;

    void OnEnable()
    {
        // จำทิศที่หันตอนเข้าตู้ เป็นจุดศูนย์กลาง
        baseYaw = transform.eulerAngles.y;
        yaw = 0f;
        pitch = 0f;
    }

    void Update()
    {
        yaw += Input.GetAxis("Mouse X") * sensitivity;
        pitch -= Input.GetAxis("Mouse Y") * sensitivity;

        yaw = Mathf.Clamp(yaw, -maxYaw, maxYaw);
        pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);

        transform.rotation = Quaternion.Euler(0f, baseYaw + yaw, 0f);
        cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}