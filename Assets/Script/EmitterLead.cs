using System.Collections.Generic;
using UnityEngine;

// Keeps a player-following, world-space particle field (dust, ground fog) at an even density however
// the player moves:
//  - the emitter is shifted ahead of the player's movement, so the field is already there when they arrive;
//  - as the emission box moves, particles are spawned in the strip of floor it newly covers, but only where
//    that patch is below its resting density (so jitter or pacing back and forth can't pile particles up);
//  - particles left behind outside the box are retired, so the particle cap is spent around the player.
// Each particle system's Box shape and Rate over Time define its resting density; its own Rate over
// Distance should be 0.
public class EmitterLead : MonoBehaviour
{
    [Tooltip("How far ahead to shift the emitter, in seconds of the player's current horizontal velocity.")]
    public float leadTime = 0.6f;
    [Tooltip("Maximum shift in metres; keep it under half the emitter's box size.")]
    public float maxLead = 5f;
    [Tooltip("How quickly the shift follows changes in velocity.")]
    public float smoothing = 4f;
    [Tooltip("Particles further than this (horizontally) from the emitter are faded out quickly. Set just beyond the emission box's half-diagonal; 0 disables.")]
    public float cullOutside = 0f;

    const float RecountInterval = 0.2f;
    const float CullFadeTime = 0.5f;
    // Strip spawns get a shortened lifetime, spread evenly like the remaining lifetimes in a resting field,
    // so a freshly covered strip expires at the same rate as the rest instead of piling up then dying together.
    // The floor keeps very short-lived particles from just flickering in and out.
    const float MinLifeFraction = 0.1f;
    // Teleports (respawn, cutscene cuts) aren't movement; the systems' own spawning refills.
    const float MaxStripStep = 6f;
    // Density cells are sized so a resting cell holds about this many particles.
    const float ParticlesPerCell = 8f;

    class Field
    {
        public ParticleSystem ps;
        public float density;          // particles per square metre when standing still
        public float cellSize;
        public int cellTarget;         // resting particles per cell
        public float owed;             // fractional particles carried over between frames
        public Vector3 lastBoxCentre;
        public readonly Dictionary<long, int> cellCounts = new Dictionary<long, int>();
    }

    private Transform player;
    private Vector3 lastPlayerPosition;
    private Vector3 baseLocalPosition;
    private Vector3 lead;
    private Field[] fields;
    private ParticleSystem.Particle[] buffer;
    private float nextRecount;

    void Awake()
    {
        var controller = GetComponentInParent<CharacterController>();
        player = controller != null ? controller.transform : transform.parent;
        if (player != null)
            lastPlayerPosition = player.position;
        baseLocalPosition = transform.localPosition;

        var systems = GetComponentsInChildren<ParticleSystem>(true);
        fields = new Field[systems.Length];
        for (int s = 0; s < systems.Length; s++)
        {
            var ps = systems[s];
            var life = ps.main.startLifetime;
            float meanLife = life.mode == ParticleSystemCurveMode.TwoConstants ? (life.constantMin + life.constantMax) * 0.5f : life.constant;
            Vector3 box = ps.shape.scale;
            float density = ps.emission.rateOverTime.constant * meanLife / Mathf.Max(1f, box.x * box.z);
            float cell = Mathf.Clamp(Mathf.Sqrt(ParticlesPerCell / Mathf.Max(density, 0.01f)), 1f, 6f);
            fields[s] = new Field
            {
                ps = ps,
                density = density,
                cellSize = cell,
                cellTarget = Mathf.Max(1, Mathf.RoundToInt(density * cell * cell)),
                lastBoxCentre = BoxCentre(ps)
            };
        }
    }

    void LateUpdate()
    {
        if (player != null && Time.deltaTime > 0f)
        {
            // Velocity from how far the player actually moved, whatever moved them (controller, cutscene, platform).
            Vector3 velocity = (player.position - lastPlayerPosition) / Time.deltaTime;
            lastPlayerPosition = player.position;
            velocity.y = 0f;
            Vector3 target = Vector3.ClampMagnitude(velocity * leadTime, maxLead);
            lead = Vector3.Lerp(lead, target, 1f - Mathf.Exp(-smoothing * Time.deltaTime));

            // Lead is in world space; the emitter hangs off the player, so convert back to local.
            Vector3 parentSpaceLead = transform.parent != null ? transform.parent.InverseTransformVector(lead) : lead;
            transform.localPosition = baseLocalPosition + parentSpaceLead;
        }

        if (Time.time >= nextRecount)
        {
            nextRecount = Time.time + RecountInterval;
            foreach (var field in fields)
                Recount(field);
        }

        foreach (var field in fields)
            FillNewStrip(field);
    }

    static Vector3 BoxCentre(ParticleSystem ps)
    {
        return ps.transform.TransformPoint(ps.shape.position);
    }

    static long CellOf(Vector3 p, float cellSize)
    {
        return ((long)Mathf.FloorToInt(p.x / cellSize) << 32) ^ (uint)Mathf.FloorToInt(p.z / cellSize);
    }

    // Retire particles left far behind, and rebuild the per-cell counts used to decide where to spawn.
    void Recount(Field field)
    {
        var ps = field.ps;
        int max = ps.main.maxParticles;
        if (buffer == null || buffer.Length < max)
            buffer = new ParticleSystem.Particle[max];

        int count = ps.GetParticles(buffer);
        Vector3 centre = transform.position;
        float limitSqr = cullOutside * cullOutside;
        bool changed = false;
        field.cellCounts.Clear();
        for (int i = 0; i < count; i++)
        {
            Vector3 offset = buffer[i].position - centre;
            offset.y = 0f;
            if (cullOutside > 0f && offset.sqrMagnitude > limitSqr)
            {
                if (buffer[i].remainingLifetime > CullFadeTime)
                {
                    buffer[i].remainingLifetime = CullFadeTime;
                    changed = true;
                }
                continue;
            }

            long cell = CellOf(buffer[i].position, field.cellSize);
            field.cellCounts.TryGetValue(cell, out int n);
            field.cellCounts[cell] = n + 1;
        }

        if (changed)
            ps.SetParticles(buffer, count);
    }

    // Spawn into the part of the emission box that wasn't inside it last frame, where it's below resting density.
    void FillNewStrip(Field field)
    {
        var ps = field.ps;
        Vector3 centre = BoxCentre(ps);
        Vector3 step = centre - field.lastBoxCentre;
        field.lastBoxCentre = centre;
        step.y = 0f;
        if (step.sqrMagnitude < 1e-6f || step.magnitude > MaxStripStep || !ps.isPlaying)
            return;

        Vector3 size = ps.shape.scale;
        Vector3 right = ps.transform.right, forward = ps.transform.forward;
        float halfX = size.x * 0.5f, halfZ = size.z * 0.5f;
        float dx = Vector3.Dot(step, right), dz = Vector3.Dot(step, forward);

        // Newly covered area: a strip along each axis the box moved on.
        float stripX = Mathf.Abs(dx) * size.z, stripZ = Mathf.Abs(dz) * size.x;
        float newArea = stripX + stripZ;
        field.owed += field.density * newArea;
        int count = Mathf.FloorToInt(field.owed);
        if (count <= 0)
            return;
        field.owed -= count;

        var life = ps.main.startLifetime;
        float lifeMin = life.mode == ParticleSystemCurveMode.TwoConstants ? life.constantMin : life.constant;
        float lifeMax = life.mode == ParticleSystemCurveMode.TwoConstants ? life.constantMax : life.constant;

        var emit = new ParticleSystem.EmitParams();
        emit.applyShapeToPosition = false;
        for (int n = 0; n < count; n++)
        {
            // Pick the strip in proportion to its area, then a random point in it, at the leading edge.
            float u, w;
            if (Random.value * newArea < stripX)
            {
                u = (halfX - Random.value * Mathf.Abs(dx)) * Mathf.Sign(dx);
                w = (Random.value * 2f - 1f) * halfZ;
            }
            else
            {
                w = (halfZ - Random.value * Mathf.Abs(dz)) * Mathf.Sign(dz);
                u = (Random.value * 2f - 1f) * halfX;
            }
            float h = (Random.value - 0.5f) * size.y;
            Vector3 position = centre + right * u + forward * w + ps.transform.up * h;

            long cell = CellOf(position, field.cellSize);
            field.cellCounts.TryGetValue(cell, out int inCell);
            if (inCell >= field.cellTarget)
                continue; // this patch is already as dense as a resting field

            field.cellCounts[cell] = inCell + 1;
            emit.position = position;
            emit.startLifetime = Mathf.Lerp(lifeMin, lifeMax, Random.value) * Mathf.Lerp(MinLifeFraction, 1f, Random.value);
            ps.Emit(emit, 1);
        }
    }
}
