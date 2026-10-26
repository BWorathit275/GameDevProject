
using UnityEngine;

public class GhostSound : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource movementSource;
    public AudioSource attackSource;

    [Header("Movement Sounds")]
    public AudioClip walkSound;
    public AudioClip runSound;

    [Header("Attack Sound")]
    public AudioClip attackSound;

    private AudioClip currentMovementSound;

    void Awake()
    {
        AudioSource[] sources = GetComponents<AudioSource>();

        if (movementSource == null && sources.Length > 0)
            movementSource = sources[0];

        if (attackSource == null)
        {
            if (sources.Length > 1)
                attackSource = sources[1];
            else
                attackSource = gameObject.AddComponent<AudioSource>();
        }

        ConfigureSource(movementSource);
        ConfigureSource(attackSource);
    }

    void ConfigureSource(AudioSource source)
    {
        if (source == null)
            return;

        source.playOnAwake = false;
        source.spatialBlend = 1f;
    }

    public void PlayMovementSound(AudioClip clip)
    {
        if (movementSource == null)
            return;

        if (currentMovementSound == clip &&
            movementSource.isPlaying)
            return;

        currentMovementSound = clip;
        movementSource.Stop();
        movementSource.loop = true;

        if (clip != null)
        {
            movementSource.clip = clip;
            movementSource.Play();
        }
    }

    public void PlayWalkSound()
    {
        PlayMovementSound(walkSound);
    }

    public void PlayRunSound()
    {
        PlayMovementSound(runSound);
    }

    public void StopMovementSound()
    {
        if (movementSource == null)
            return;

        movementSource.Stop();
        currentMovementSound = null;
    }

    public void PlayAttackSound()
    {
        if (attackSource == null || attackSound == null)
            return;

        attackSource.Stop();
        attackSource.clip = attackSound;
        attackSource.loop = false;
        attackSource.Play();
    }

    public void StopAttackSound()
    {
        if (attackSource != null)
            attackSource.Stop();
    }
}
