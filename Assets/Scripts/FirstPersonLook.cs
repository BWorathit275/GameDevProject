using UnityEngine;

public class FirstPersonLook : MonoBehaviour
{
    public Transform playerBody; // ตัวละคร (หมุนแนวนอน)
    public float mouseSensitivity = 100f;
    private float xRotation = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // จากัดไม่ให้ก้มเงยเกิน 90 องศา
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f); // กล้องหมุนแนวตั้ง
        playerBody.Rotate(Vector3.up * mouseX); // ตัวละครหมุนแนวนอน
    }
}