
using UnityEngine;
using System.Collections;

public class ThinGhostSound : MonoBehaviour
{
    [Header("Walk Sound")]
    public AudioSource walkSource;
    public AudioClip walkSound;

    [Header("Scream Sound")]
    public AudioSource screamSource;
    public AudioClip screamSound;

    [Header("Attack Sound")]
    public AudioSource attackSource;
    public AudioClip attackSound;
    public float attackSoundDelay = 0.38f;

    private Coroutine attackCoroutine;

    void Awake()
    {
        SetupSource(ref walkSource, true);
        SetupSource(ref screamSource, true);
        SetupSource(ref attackSource, false);
    }

    void SetupSource(ref AudioSource source, bool shouldLoop)
    {
        if (source == null)
            source = gameObject.AddComponent<AudioSource>();

        source.playOnAwake = false;
        source.loop = shouldLoop;
        source.spatialBlend = 1f; // เสียง 3D ได้ยินตามระยะและทิศทางของผี
        source.volume = 1f;
        source.mute = false;
        source.enabled = true;
    }

    public void StartWalk()
    {
        if (walkSound == null || walkSource == null)
            return;

        if (walkSource.clip != walkSound)
            walkSource.clip = walkSound;

        if (!walkSource.isPlaying)
            walkSource.Play();
    }

    public void StopWalk()
    {
        if (walkSource != null)
            walkSource.Stop();
    }

    public void PlayScream()
    {
        if (screamSound == null || screamSource == null)
        {
            Debug.LogWarning("ThinGhostSound: ยังไม่ได้กำหนด Scream Sound หรือ Source!", this);
            return;
        }

        screamSource.clip = screamSound;

        if (!screamSource.isPlaying)
        {
            screamSource.Play();
            Debug.Log("ThinGhostSound: เริ่มเล่นเสียง Scream", this);
        }
    }

    public void StopScream()
    {
        if (screamSource != null)
            screamSource.Stop();
    }

    public void PlayAttack()
    {
        if (attackSound == null || attackSource == null)
        {
            Debug.LogWarning("ThinGhostSound: ยังไม่ได้กำหนด Attack Sound หรือ Source!", this);
            return;
        }

        if (attackCoroutine != null)
            StopCoroutine(attackCoroutine);

        attackCoroutine = StartCoroutine(PlayAttackDelayed());
    }

    private IEnumerator PlayAttackDelayed()
    {
        yield return new WaitForSeconds(attackSoundDelay);

        if (attackSource != null && attackSound != null)
        {
            attackSource.PlayOneShot(attackSound, 1f);
            Debug.Log("ThinGhostSound: เล่นเสียง Attack", this);
        }

        attackCoroutine = null;
    }

    void OnDisable()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        StopWalk();
        StopScream();
    }
}
