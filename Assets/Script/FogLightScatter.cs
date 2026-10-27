using UnityEngine;

// Fakes light scattering for an unlit fog particle system: each frame, every puff is brightened by how deep
// it sits in a spot light's cone and how close it is to the light, so mist glows where the flashlight points.
// Lit particle shaders shade flat fog sheets like a surface (by the angle to the light), which barely shows.
[RequireComponent(typeof(ParticleSystem))]
public class FogLightScatter : MonoBehaviour
{
    [Header("Light")]
    [Tooltip("Spot light to scatter. If empty, the nearest spot light found by searching up the hierarchy is used (e.g. the player's flashlight).")]
    public Light spotLight;

    [Header("Look")]
    public Color fogColor = new Color(0.8f, 0.83f, 0.84f);
    [Tooltip("How strongly puffs inside the beam pick up the light.")]
    public float scatter = 0.35f;
    [Tooltip("Brightness of puffs outside any light, so the mist stays faintly visible in the dark.")]
    public float ambient = 0.02f;
    [Tooltip("Upper limit on puff brightness, so mist right under the light doesn't blow out to white.")]
    public float maxBrightness = 0.45f;

    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        // Walk up the hierarchy so nested fog layers also find the player's flashlight.
        for (Transform t = transform.parent; spotLight == null && t != null; t = t.parent)
        {
            foreach (var light in t.GetComponentsInChildren<Light>(true))
            {
                if (light.type == LightType.Spot)
                {
                    spotLight = light;
                    break;
                }
            }
        }
    }

    void LateUpdate()
    {
        int max = ps.main.maxParticles;
        if (particles == null || particles.Length < max)
            particles = new ParticleSystem.Particle[max];

        int count = ps.GetParticles(particles);
        if (count == 0)
            return;

        bool lit = spotLight != null && spotLight.isActiveAndEnabled && spotLight.intensity > 0f;
        Vector3 lightPos = lit ? spotLight.transform.position : Vector3.zero;
        Vector3 lightDir = lit ? spotLight.transform.forward : Vector3.forward;
        float range = lit ? spotLight.range : 1f;
        float cosOuter = lit ? Mathf.Cos(spotLight.spotAngle * 0.5f * Mathf.Deg2Rad) : 1f;
        float cosInner = lit ? Mathf.Cos(spotLight.innerSpotAngle * 0.5f * Mathf.Deg2Rad) : 1f;

        for (int i = 0; i < count; i++)
        {
            float brightness = ambient;
            if (lit)
            {
                Vector3 toPuff = particles[i].position - lightPos;
                float dist = toPuff.magnitude;
                if (dist > 0.001f && dist < range)
                {
                    float cone = Mathf.InverseLerp(cosOuter, cosInner, Vector3.Dot(toPuff / dist, lightDir));
                    cone = cone * cone * (3f - 2f * cone);
                    // Inverse-square falloff that reaches zero at the light's range, similar to URP's.
                    float fade = 1f - Mathf.Pow(dist / range, 4f);
                    float atten = fade * fade / (dist * dist + 1f);
                    brightness += spotLight.intensity * cone * atten * scatter;
                }
            }

            Color32 c = particles[i].startColor;
            Color tinted = fogColor * Mathf.Min(brightness, maxBrightness);
            particles[i].startColor = new Color(tinted.r, tinted.g, tinted.b, c.a / 255f);
        }

        ps.SetParticles(particles, count);
    }
}
