using UnityEngine;

public class FatGhostSound : MonoBehaviour
{
    [Header("Chase Sound")]
    public AudioSource screamSource;
    public AudioClip screamSound;
    [Tooltip("Scream Sound จะถูกใส่รวมใน Clips ให้อัตโนมัติ ใส่คลิปเพิ่มได้")]
    public GhostVoice screamVoice = new GhostVoice { gap = new Vector2(2f, 5f), pitchRange = new Vector2(0.8f, 0.95f) };

    [Header("Idle Sound (คำรามเป็นช่วงๆ ตอนไม่ได้ไล่)")]
    public GhostVoice idleVoice = new GhostVoice { gap = new Vector2(6f, 12f), pitchRange = new Vector2(0.75f, 0.9f) };

    private bool chasing;

    void Awake()
    {
        if (screamSource == null)
            screamSource = GetComponent<AudioSource>();

        if (screamSource != null)
        {
            screamSource.playOnAwake = false;
            screamSource.loop = false;
            screamSource.spatialBlend = 1f; // เสียง 3D ได้ยินตามระยะและทิศทางของผี
        }

        if (screamSound != null && (screamVoice.clips == null || System.Array.IndexOf(screamVoice.clips, screamSound) < 0))
        {
            AudioClip[] list = new AudioClip[(screamVoice.clips == null ? 0 : screamVoice.clips.Length) + 1];
            list[0] = screamSound;
            if (screamVoice.clips != null)
                screamVoice.clips.CopyTo(list, 1);
            screamVoice.clips = list;
        }
    }

    void Start()
    {
        idleVoice.Begin(screamSource, false);
    }

    void Update()
    {
        if (chasing)
            screamVoice.Tick(screamSource);
        else
            idleVoice.Tick(screamSource);
    }

    public void PlayScream()
    {
        if (screamSource == null || !screamVoice.HasClips)
        {
            Debug.LogWarning(
                "FatGhostSound: Scream AudioSource or clip is missing!",
                this
            );
            return;
        }

        chasing = true;
        idleVoice.End(screamSource);
        screamVoice.Begin(screamSource, true);
    }

    public void StopScream()
    {
        chasing = false;
        screamVoice.End(screamSource);
        idleVoice.Begin(screamSource, false);
    }

    void OnDisable()
    {
        screamVoice.End(screamSource);
        idleVoice.End(screamSource);
    }
}
