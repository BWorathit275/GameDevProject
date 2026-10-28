
using UnityEngine;

public class PlayerFootstepSound : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip walkSound;
    public AudioSource footstepSource;

    [Header("Footstep Interval")]
    public float walkInterval = 0.5f;
    public float runInterval = 0.3f;

    [Header("Sound Speed")]
    public float walkPitch = 1f;
    public float runPitch = 1.4f;

    private CharacterController characterController;
    private float stepTimer;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (footstepSource == null)
            footstepSource = GetComponent<AudioSource>();

        if (footstepSource != null)
        {
            footstepSource.playOnAwake = false;
            footstepSource.loop = false; 
        }
    }

    void Update()
    {
        if (characterController == null || footstepSource == null)
            return;

        Vector3 horizontalVelocity = new Vector3(
            characterController.velocity.x,
            0f,
            characterController.velocity.z
        );

        bool isMoving =
            characterController.isGrounded &&
            horizontalVelocity.magnitude > 0.1f;

        if (!isMoving)
        {
            stepTimer = 0f;
            footstepSource.Stop();
            return;
        }

        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        stepTimer -= Time.deltaTime;

        if (stepTimer <= 0f)
        {
            if (walkSound != null)
            {
                // เร่งความเร็วเสียงเมื่อวิ่ง
                footstepSource.pitch = isRunning ? runPitch : walkPitch;

                footstepSource.clip = walkSound;
                footstepSource.loop = false;
                footstepSource.Play();
            }

            // วิ่งให้มีเสียงฝีเท้าถี่กว่าเดิน
            stepTimer = isRunning ? runInterval : walkInterval;
        }
    }
}
