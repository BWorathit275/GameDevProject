using UnityEngine;

public class MouseOrbitCamera : MonoBehaviour
{
    public float mouseSensitivity = 3f;
    public float minPitch = -30f;
    public float maxPitch = 60f;
    private float yaw;
    private float pitch;
    void Start()
    {
        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
    }
    void Update()
    {
        if (Input.GetMouseButton(1)) // คลิกขวาค้าง
        {
            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            transform.eulerAngles = new Vector3(pitch, yaw, 0);
        }
    }
}