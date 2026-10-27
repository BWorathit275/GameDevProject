using System.Collections.Generic;
using UnityEngine;

// Stirs the dust particle systems on this object and its children for a moment, then lets them settle.
// Call DustGust.Trigger(position) from gameplay events, e.g. a door slamming or the ghost passing by.
public class DustGust : MonoBehaviour
{
    [Header("Gust")]
    [Tooltip("Gusts further than this from the dust have no effect.")]
    public float reach = 12f;
    [Tooltip("Turbulence multiplier at the peak of a full-strength gust.")]
    public float turbulenceBoost = 6f;
    [Tooltip("Speed (m/s) dust is pushed away from the gust source at the peak of a full-strength gust.")]
    public float push = 1.2f;
    public float riseTime = 0.15f;
    public float settleTime = 2.5f;

    [Header("Debug")]
    public KeyCode testKey = KeyCode.None;

    struct Layer
    {
        public ParticleSystem ps;
        public Vector3 noiseStrength; // per axis; all three equal unless the noise uses separate axes
        public float noiseScroll;
        public Vector2 velX, velY, velZ; // (min, max) of the base drift
    }

    static readonly List<DustGust> instances = new List<DustGust>();

    private readonly List<Layer> layers = new List<Layer>();
    private float gustStrength;
    private float gustTime = float.MaxValue;
    private Vector3 gustDirection;

    // Stirs every dust volume within reach of the source.
    public static void Trigger(Vector3 source, float strength = 1f)
    {
        foreach (var gust in instances)
            gust.Stir(source, strength);
    }

    void Awake()
    {
        foreach (var ps in GetComponentsInChildren<ParticleSystem>(true))
        {
            var vel = ps.velocityOverLifetime;
            var noise = ps.noise;
            layers.Add(new Layer
            {
                ps = ps,
                noiseStrength = noise.separateAxes
                    ? new Vector3(noise.strengthXMultiplier, noise.strengthYMultiplier, noise.strengthZMultiplier)
                    : Vector3.one * noise.strengthMultiplier,
                noiseScroll = ps.noise.scrollSpeedMultiplier,
                velX = Range(vel.x),
                velY = Range(vel.y),
                velZ = Range(vel.z)
            });
        }
    }

    void OnEnable()
    {
        instances.Add(this);
    }

    void OnDisable()
    {
        instances.Remove(this);
        gustTime = float.MaxValue;
        Apply(0f);
    }

    public void Stir(Vector3 source, float strength = 1f)
    {
        Vector3 offset = transform.position - source;
        float falloff = 1f - offset.magnitude / reach;
        if (falloff <= 0f)
            return;

        float current = Envelope() * gustStrength;
        float incoming = strength * falloff;
        if (incoming < current)
            return;

        gustStrength = incoming;
        gustTime = 0f;
        gustDirection = offset.sqrMagnitude > 0.01f ? offset.normalized : Random.onUnitSphere;
    }

    [ContextMenu("Test Gust")]
    void TestGust()
    {
        Stir(transform.position + Random.onUnitSphere * 3f);
    }

    void Update()
    {
        if (testKey != KeyCode.None && Input.GetKeyDown(testKey))
            TestGust();

        if (gustTime == float.MaxValue)
            return;

        gustTime += Time.deltaTime;
        float amount = Envelope() * gustStrength;
        Apply(amount);

        if (gustTime > riseTime + settleTime)
            gustTime = float.MaxValue;
    }

    // 0..1: quick rise, then an ease-out settle.
    float Envelope()
    {
        if (gustTime == float.MaxValue)
            return 0f;
        if (gustTime < riseTime)
            return gustTime / riseTime;

        float settle = 1f - Mathf.Clamp01((gustTime - riseTime) / settleTime);
        return settle * settle;
    }

    void Apply(float amount)
    {
        Vector3 pushVelocity = gustDirection * push * amount;

        foreach (var layer in layers)
        {
            if (layer.ps == null)
                continue;

            var noise = layer.ps.noise;
            float boost = 1f + (turbulenceBoost - 1f) * amount;
            if (noise.separateAxes)
            {
                noise.strengthXMultiplier = layer.noiseStrength.x * boost;
                noise.strengthYMultiplier = layer.noiseStrength.y * boost;
                noise.strengthZMultiplier = layer.noiseStrength.z * boost;
            }
            else
            {
                noise.strengthMultiplier = layer.noiseStrength.x * boost;
            }
            noise.scrollSpeedMultiplier = layer.noiseScroll * (1f + (turbulenceBoost - 1f) * 0.5f * amount);

            var vel = layer.ps.velocityOverLifetime;
            vel.x = new ParticleSystem.MinMaxCurve(layer.velX.x + pushVelocity.x, layer.velX.y + pushVelocity.x);
            vel.y = new ParticleSystem.MinMaxCurve(layer.velY.x + pushVelocity.y, layer.velY.y + pushVelocity.y);
            vel.z = new ParticleSystem.MinMaxCurve(layer.velZ.x + pushVelocity.z, layer.velZ.y + pushVelocity.z);
        }
    }

    static Vector2 Range(ParticleSystem.MinMaxCurve curve)
    {
        if (curve.mode == ParticleSystemCurveMode.TwoConstants)
            return new Vector2(curve.constantMin, curve.constantMax);
        return new Vector2(curve.constant, curve.constant);
    }
}
