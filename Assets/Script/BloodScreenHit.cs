using UnityEngine;

// Plays the on-screen blood for one hit: the red flash and a light ring of splats every time, plus heavy
// splats, lens streaks and drips on one side of the screen, facing where the hit came from.
// Lives on the BloodScreen object under the player's camera.
public class BloodScreenHit : MonoBehaviour
{
    [Header("Every Hit")]
    public ParticleSystem ring;
    public ParticleSystem flash;
    public ParticleSystem vignette;

    [Header("Heavy Side (one per hit)")]
    public ParticleSystem left;
    public ParticleSystem right;
    public ParticleSystem top;

    [Tooltip("Hits from mostly straight ahead or behind (less sideways than this) pick a random side; from above, the top.")]
    [Range(0f, 1f)]
    public float sideThreshold = 0.35f;

    // Hit with no known direction: heavy side picked at random.
    public void Play()
    {
        int pick = Random.Range(0, 3);
        PlaySide(pick == 0 ? left : pick == 1 ? right : top);
    }

    // Hit coming from a world position (e.g. the ghost): heavy side faces it.
    public void Play(Vector3 hitFrom)
    {
        Transform cam = transform.parent != null ? transform.parent : transform;
        Vector3 local = cam.InverseTransformDirection(hitFrom - cam.position);
        local.z = 0f;

        // Straight ahead or behind: any side, for variety.
        if (local.sqrMagnitude < 0.0001f || (Mathf.Abs(local.normalized.x) < sideThreshold && local.y <= 0f))
            Play();
        else if (local.y > Mathf.Abs(local.x))
            PlaySide(top);
        else
            PlaySide(local.x < 0f ? left : right);
    }

    void PlaySide(ParticleSystem side)
    {
        if (ring != null)
            ring.Play(false);
        if (flash != null)
            flash.Play(false);
        if (vignette != null)
            vignette.Play(false);
        if (side != null)
            side.Play(true);
    }
}
