using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerAnimation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator; // Animator ที่อยู่บนโมเดล (ลูกของ Player)

    [Header("Settings")]
    [SerializeField] private float speedDampTime = 0.1f; // ความนุ่มนวลตอนเปลี่ยนท่า
    [SerializeField] private float maxSpeed = 8f;        // ใช้ normalize ค่า Speed (เท่ากับ sprintSpeed)

    private CharacterController controller;
    private float lastGroundedTime;
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private FirstPersonController fpc;
    private static readonly int IsCrouchingHash = Animator.StringToHash("IsCrouching");

    private void Awake()
    {
        fpc = GetComponent<FirstPersonController>();
        controller = GetComponent<CharacterController>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        // ให้ CharacterController เป็นตัวขยับตัวละคร ไม่ใช่ animation
        animator.applyRootMotion = false;
    }

    private void Update()
    {
        if (controller.isGrounded) lastGroundedTime = Time.time;
        bool grounded = Time.time - lastGroundedTime < 0.15f; // ผ่อนผัน 0.15 วินาที

        animator.SetBool(IsGroundedHash, grounded);
        // ความเร็วในแนวราบ (ไม่นับตอนตกหรือกระโดด)
        Vector3 horizontalVelocity = controller.velocity;
        horizontalVelocity.y = 0f;

        float normalizedSpeed = horizontalVelocity.magnitude / maxSpeed; // 0 = หยุด, ~0.6 = เดิน, 1 = วิ่ง

        animator.SetFloat(SpeedHash, normalizedSpeed, speedDampTime, Time.deltaTime);
        animator.SetBool(IsGroundedHash, controller.isGrounded);
        animator.SetBool(IsCrouchingHash, fpc.IsCrouching);
    }
}