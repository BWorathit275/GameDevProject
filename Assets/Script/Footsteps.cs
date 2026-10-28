using UnityEngine;

// Footstep sounds driven by how far the object has walked, so faster movement means faster steps.
// Works on the player (CharacterController) and on the ghosts (NavMeshAgent) without any extra wiring.
public class Footsteps : MonoBehaviour
{
    [Tooltip("Footstep clips. A random one plays each step, never the same one twice in a row.")]
    public AudioClip[] clips;

    [Tooltip("Metres walked between two steps.")]
    public float stepDistance = 1.6f;
    [Tooltip("Below this speed (m/s) no steps play.")]
    public float minSpeed = 0.3f;

    [Header("Variation")]
    public Vector2 pitchRange = new Vector2(0.9f, 1.1f);
    public Vector2 volumeRange = new Vector2(0.75f, 1f);

    [Header("Source")]
    [Tooltip("0 = 2D (the player's own steps), 1 = 3D (heard by distance and direction).")]
    [Range(0f, 1f)] public float spatialBlend = 1f;
    public float maxDistance = 20f;

    private AudioSource source;
    private CharacterController controller;
    private Vector3 lastPosition;
    private float travelled;
    private int lastClip = -1;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = spatialBlend;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = 1f;
        source.maxDistance = maxDistance;
    }

    void OnEnable()
    {
        lastPosition = transform.position;
        // The first step comes half a stride after starting to move.
        travelled = stepDistance * 0.5f;
    }

    void Update()
    {
        if (Time.deltaTime <= 0f)
            return;

        Vector3 moved = transform.position - lastPosition;
        moved.y = 0f;
        lastPosition = transform.position;

        // A jump of over a metre in one frame is a teleport (locker, respawn), not walking.
        if (moved.magnitude > 1f)
            return;

        bool airborne = controller != null && controller.enabled && !controller.isGrounded;

        if (moved.magnitude / Time.deltaTime < minSpeed || airborne)
        {
            travelled = stepDistance * 0.5f;
            return;
        }

        travelled += moved.magnitude;

        if (travelled >= stepDistance)
        {
            travelled = 0f;
            PlayStep();
        }
    }

    void PlayStep()
    {
        if (clips == null || clips.Length == 0)
            return;

        int index = Random.Range(0, clips.Length);
        if (clips.Length > 1 && index == lastClip)
            index = (index + 1) % clips.Length;
        lastClip = index;

        if (clips[index] == null)
            return;

        source.pitch = Random.Range(pitchRange.x, pitchRange.y);
        source.PlayOneShot(clips[index], Random.Range(volumeRange.x, volumeRange.y));
    }
}
