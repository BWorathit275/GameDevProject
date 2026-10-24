using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform; // ลากกล้อง (ลูกของ Player) มาใส่

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float gravity = -19.62f;

    [Header("Crouch")]
    [SerializeField] private KeyCode crouchKey = KeyCode.C;     // กดค้างเพื่อย่อ
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float crouchHeight = 1.0f;         // ความสูงของ Character Controller ตอนย่อ
    [SerializeField] private float crouchTransitionSpeed = 10f; // ความเร็วในการย่อ/ลุก

    [Header("Look")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 85f;

    public bool IsCrouching { get; private set; }

    private CharacterController controller;
    private float verticalVelocity;
    private float xRotation;

    // ค่าตอนยืน เก็บไว้ตอนเริ่มเกม
    private float standHeight;
    private float standBottomY; // ระดับก้นแคปซูล (คงที่ตอนย่อ เพื่อให้เท้าไม่ลอย)
    private float standCameraY;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        standHeight = controller.height;
        standBottomY = controller.center.y - standHeight / 2f;
        standCameraY = cameraTransform.localPosition.y;
    }

    private void Start()
    {
        // ซ่อนและล็อกเมาส์ไว้กลางจอ (กด Esc เพื่อปล่อยเมาส์)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleLook();
        HandleCrouch();
        HandleMovement();
    }

    private void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // หมุนตัวละครซ้าย-ขวา
        transform.Rotate(Vector3.up * mouseX);

        // หมุนกล้องขึ้น-ลง (จำกัดมุมไม่ให้หมุนเกิน)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    private void HandleCrouch()
    {
        bool wantCrouch = Input.GetKey(crouchKey);

        // ถ้าปล่อยปุ่มแล้วแต่มีเพดานอยู่เหนือหัว ให้ย่อค้างไว้ก่อน
        if (!wantCrouch && IsCrouching && !CanStandUp())
            wantCrouch = true;

        IsCrouching = wantCrouch;

        float targetHeight = IsCrouching ? crouchHeight : standHeight;
        float height = Mathf.Lerp(controller.height, targetHeight, crouchTransitionSpeed * Time.deltaTime);
        if (Mathf.Abs(height - targetHeight) < 0.001f) height = targetHeight;

        controller.height = height;
        controller.center = new Vector3(controller.center.x, standBottomY + height / 2f, controller.center.z);

        // ลดระดับกล้องตามส่วนที่ย่อลง
        float targetCamY = IsCrouching ? standCameraY - (standHeight - crouchHeight) : standCameraY;
        Vector3 camPos = cameraTransform.localPosition;
        camPos.y = Mathf.Lerp(camPos.y, targetCamY, crouchTransitionSpeed * Time.deltaTime);
        cameraTransform.localPosition = camPos;
    }

    // เช็กว่ามีที่ว่างพอให้ลุกขึ้นยืนไหม
    private bool CanStandUp()
    {
        float radius = controller.radius * 0.95f;
        Vector3 origin = transform.position + Vector3.up * (crouchHeight - radius);
        float distance = standHeight - crouchHeight;
        return !Physics.SphereCast(origin, radius, Vector3.up, out _, distance, ~0, QueryTriggerInteraction.Ignore);
    }

    private void HandleMovement()
    {
        float x = Input.GetAxisRaw("Horizontal"); // A / D
        float z = Input.GetAxisRaw("Vertical");   // W / S

        Vector3 move = transform.right * x + transform.forward * z;
        if (move.sqrMagnitude > 1f) move.Normalize();

        float speed = walkSpeed;
        if (IsCrouching) speed = crouchSpeed;
        else if (Input.GetKey(KeyCode.LeftShift)) speed = sprintSpeed;

        // แรงโน้มถ่วง + กระโดด (กระโดดไม่ได้ตอนย่อ)
        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        if (controller.isGrounded && !IsCrouching && Input.GetButtonDown("Jump"))
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = move * speed + Vector3.up * verticalVelocity;
        controller.Move(velocity * Time.deltaTime);
    }
}