
using UnityEngine;

public class FatGhostSound : MonoBehaviour
{
    [Header("Chase Sound")]
    public AudioSource screamSource;
    public AudioClip screamSound;

    void Awake()
    {
        if (screamSource == null)
            screamSource = GetComponent<AudioSource>();

        if (screamSource != null)
        {
            screamSource.playOnAwake = false;
            screamSource.loop = true;
        }
    }

    public void PlayScream()
    {
        if (screamSource == null || screamSound == null)
        {
            Debug.LogWarning(
                "FatGhostSound: Scream AudioSource or clip is missing!",
                this
            );
            return;
        }

        if (screamSource.clip != screamSound)
            screamSource.clip = screamSound;

        if (!screamSource.isPlaying)
            screamSource.Play();
    }

    public void StopScream()
    {
        if (screamSource != null)
            screamSource.Stop();
    }
}
