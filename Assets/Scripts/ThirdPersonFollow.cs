using UnityEngine;

public class ThirdPersonFollow : MonoBehaviour
{
    public Transform target; // ตัวละครที่จะติดตาม
    public Vector3 offset = new Vector3(0f, 3f, -6f); // ระยะห่างจากตัวละคร
    public float smoothSpeed = 5f;
    void LateUpdate()
    {
        Vector3 desiredPosition = target.position + target.TransformDirection(offset);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.LookAt(target.position + Vector3.up * 1.5f); // มองไปที่ตัวละคร
    }
}
