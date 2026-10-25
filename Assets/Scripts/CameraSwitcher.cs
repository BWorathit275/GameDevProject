using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    [Header("Cameras")]
    public Camera firstPersonCamera;
    public Camera thirdPersonCamera;
    public Camera topDownCamera;
    [Header("Scripts to toggle (optional)")]
    public MonoBehaviour firstPersonScript;
    public MonoBehaviour thirdPersonScript;
    public MonoBehaviour topDownScript;
    void Start()
    {
        // เริ่มต้นด้วยกล้องแรก (First Person)
        thirdPersonCamera.gameObject.SetActive(false);
        topDownCamera.gameObject.SetActive(false);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchTo(firstPersonCamera);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchTo(thirdPersonCamera);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SwitchTo(topDownCamera);
        }
    }
    void SwitchTo(Camera targetCamera)
    {
        // ปิดกล้องทั้งหมดก่อน
        firstPersonCamera.gameObject.SetActive(false);
        thirdPersonCamera.gameObject.SetActive(false);
        topDownCamera.gameObject.SetActive(false);
        // เปิดเฉพาะกล้องที่เลือก
        targetCamera.gameObject.SetActive(true);
        // (ทางเลือก) เปิด/ปิดสคริปต์ควบคุมให้ตรงกับกล้อง เพื่อประหยัดการคานวณที่ไม่จาเป็น
        firstPersonScript.enabled = (targetCamera == firstPersonCamera);
        thirdPersonScript.enabled = (targetCamera == thirdPersonCamera);
        topDownScript.enabled = (targetCamera == topDownCamera);
    }
}