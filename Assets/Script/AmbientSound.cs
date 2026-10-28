using UnityEngine;

// Level ambience: a quiet looping background bed, plus occasional one-off sounds (drips, creaks, thuds)
// from a random spot around the player. While a ghost is chasing, the loop gets muffled and a bit quieter
// so the chase sounds cut through.
public class AmbientSound : MonoBehaviour
{
    [Header("Background loop")]
    public AudioClip loop;
    [Range(0f, 1f)] public float loopVolume = 0.35f;
    public float fadeInTime = 3f;

    [Header("One-off sounds")]
    [Tooltip("Played one at a time from a random direction around the player, never the same one twice in a row.")]
    public AudioClip[] oneShots;
    [Tooltip("Seconds of quiet between one-off sounds, min-max.")]
    public Vector2 gap = new Vector2(10f, 30f);
    [Tooltip("How far from the player they play, min-max metres.")]
    public Vector2 distance = new Vector2(4f, 12f);
    public Vector2 pitchRange = new Vector2(0.85f, 1.1f);
    public Vector2 volumeRange = new Vector2(0.5f, 0.9f);

    [Header("Chase")]
    [Tooltip("Ambience volume while a ghost is chasing (1 = no change).")]
    [Range(0f, 1f)] public float chaseVolume = 0.7f;
    [Tooltip("Low-pass cutoff (Hz) while a ghost is chasing. Lower = more muffled.")]
    public float chaseCutoff = 700f;
    [Tooltip("How fast the ambience muffles and recovers (full change per second).")]
    public float duckSpeed = 1.5f;

    private const float OpenCutoff = 22000f;

    private AudioSource loopSource;
    private AudioLowPassFilter muffle;
    private AudioSource shotSource;
    private Transform listener;
    private ThinGhostAI[] thinGhosts;
    private FatGhostChase[] fatGhosts;
    private float duck;
    private float fade;
    private float nextShot;
    private int lastShot = -1;

    void Awake()
    {
        loopSource = gameObject.AddComponent<AudioSource>();
        loopSource.clip = loop;
        loopSource.loop = true;
        loopSource.playOnAwake = false;
        loopSource.spatialBlend = 0f;
        loopSource.volume = 0f;

        // Only affects the loop: the one-off source lives on a child object.
        muffle = gameObject.AddComponent<AudioLowPassFilter>();
        muffle.cutoffFrequency = OpenCutoff;

        GameObject shot = new GameObject("AmbientOneShot");
        shot.transform.SetParent(transform, false);
        shotSource = shot.AddComponent<AudioSource>();
        shotSource.playOnAwake = false;
        shotSource.spatialBlend = 1f;
        shotSource.rolloffMode = AudioRolloffMode.Linear;
        shotSource.minDistance = 2f;
        shotSource.maxDistance = 25f;
    }

    void Start()
    {
        AudioListener l = FindAnyObjectByType<AudioListener>();
        listener = l != null ? l.transform : transform;

        thinGhosts = FindObjectsByType<ThinGhostAI>(FindObjectsSortMode.None);
        fatGhosts = FindObjectsByType<FatGhostChase>(FindObjectsSortMode.None);

        if (loop != null)
            loopSource.Play();

        nextShot = Time.time + Random.Range(gap.x, gap.y);
    }

    void Update()
    {
        fade = fadeInTime > 0f ? Mathf.Min(1f, fade + Time.deltaTime / fadeInTime) : 1f;
        // 0 = calm, 1 = fully muffled
        float target = AnyGhostChasing() ? 1f : 0f;
        duck = Mathf.MoveTowards(duck, target, duckSpeed * Time.deltaTime);

        loopSource.volume = loopVolume * fade * Mathf.Lerp(1f, chaseVolume, duck);
        // Sweep the cutoff on a log scale so the muffle sounds even.
        muffle.cutoffFrequency = Mathf.Exp(Mathf.Lerp(Mathf.Log(OpenCutoff), Mathf.Log(chaseCutoff), duck));

        if (Time.time >= nextShot)
            PlayOneShot();
    }

    bool AnyGhostChasing()
    {
        foreach (ThinGhostAI g in thinGhosts)
            if (g != null && g.isActiveAndEnabled && g.IsChasing)
                return true;

        foreach (FatGhostChase g in fatGhosts)
            if (g != null && g.isActiveAndEnabled && g.IsChasing)
                return true;

        return false;
    }

    void PlayOneShot()
    {
        nextShot = Time.time + Random.Range(gap.x, gap.y);

        if (oneShots == null || oneShots.Length == 0)
            return;

        int index = Random.Range(0, oneShots.Length);
        if (oneShots.Length > 1 && index == lastShot)
            index = (index + 1) % oneShots.Length;
        lastShot = index;

        AudioClip clip = oneShots[index];
        if (clip == null)
            return;

        Vector2 dir = Random.insideUnitCircle.normalized * Random.Range(distance.x, distance.y);
        shotSource.transform.position = listener.position + new Vector3(dir.x, Random.Range(-0.5f, 1.5f), dir.y);
        shotSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        shotSource.PlayOneShot(clip, Random.Range(volumeRange.x, volumeRange.y) * Mathf.Lerp(1f, chaseVolume, duck));
    }
}
