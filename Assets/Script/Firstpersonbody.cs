using UnityEngine;

// ใส่ที่ Player: ซ่อนหัวโมเดล เพื่อให้กล้องอยู่ที่ระดับตาโดยยังเห็นแขนขาและลำตัว
public class FirstPersonBody : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;          // Animator บนโมเดล (Humanoid)
    [SerializeField] private Transform cameraTransform;  // Main Camera

    [Header("Hide Head")]
    [SerializeField] private bool hideHead = true;

    [Header("Camera Follow Head (ทางเลือก)")]
    [SerializeField] private bool followHeadBone = false;   // ให้กล้องตามตำแหน่งหัวจากอนิเมชัน
    [SerializeField] private Vector3 headOffset = new Vector3(0f, 0.08f, 0.12f); // ขยับเทียบกับ Player
    [SerializeField] private float followSmooth = 25f;

    private Transform head;

    private void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;

        if (animator != null && animator.isHuman)
            head = animator.GetBoneTransform(HumanBodyBones.Head);

        if (head == null)
            Debug.LogWarning("FirstPersonBody: ไม่พบกระดูกหัว (ต้องตั้ง Rig เป็น Humanoid)");
    }

    // ต้องทำใน LateUpdate เพราะ Animator จะเขียนทับค่าใน Update
    private void LateUpdate()
    {
        if (head == null) return;

        if (hideHead)
            head.localScale = Vector3.zero;

        if (followHeadBone && cameraTransform != null)
        {
            Vector3 target = head.position + transform.TransformDirection(headOffset);
            cameraTransform.position = Vector3.Lerp(
                cameraTransform.position, target, followSmooth * Time.deltaTime);
        }
    }
}