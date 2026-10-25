using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public float rotationSpeed = 180f;
    public float gravity = -20f;

    [Header("Animation")]
    public Animator animator;

    private CharacterController controller;
    private float verticalVelocity;

    // ย้ายตัวแปร isCrouching มาประกาศไว้ที่ระดับ Class ตรงนี้ (ถูกต้องตามหลัก C#)
    private bool isCrouching = false;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // เช็คปุ่มกด C สำหรับย่อตัว
        if (Input.GetKeyDown(KeyCode.C))
        {
            isCrouching = !isCrouching;
            animator.SetBool("IsCrouching", isCrouching);
        }

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 input = new Vector3(moveX, 0f, moveZ);
        input = Vector3.ClampMagnitude(input, 1f);

        bool isMoving = input.magnitude > 0.1f;

        bool isRunning =
            Input.GetKey(KeyCode.LeftShift) ||
            Input.GetKey(KeyCode.RightShift);

        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        // เคลื่อนที่ตามทิศทางที่ตัวละครหันอยู่
        Vector3 moveDirection =
            transform.right * input.x +
            transform.forward * input.z;

        moveDirection.Normalize();

        // หมุนเฉพาะตอนเดินไปข้างหน้าหรือด้านข้าง
        if (isMoving && moveZ >= -0.1f)
        {
            Vector3 facingDirection = moveDirection;

            if (facingDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(facingDirection);

                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
        }

        // แรงโน้มถ่วง
        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;

        // แก้ไขเครื่องหมายคูณ (*) ให้ถูกต้อง
        Vector3 velocity = moveDirection * currentSpeed;
        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);

        // Animation Blend Tree
        if (animator != null)
        {
            float animationSpeed = 0f;

            if (isMoving)
                animationSpeed = isRunning ? 1f : 0.5f;

            animator.SetFloat(
                "Speed",
                animationSpeed,
                0.1f,
                Time.deltaTime
            );
        }
    }
}