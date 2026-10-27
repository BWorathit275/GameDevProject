using UnityEngine;

// Blood that stays around the screen edges while the player is badly hurt.
// Each layer is a full-screen blood frame; the first shows at Start At Hearts and each heart lost below that adds the next,
// heavier one. Layers fade back out on heal.
// Lives under BloodScreen just behind the hit splats, so fresh hits land on top; PlayerHealth calls SetHearts.
public class BloodLinger : MonoBehaviour
{
    [Tooltip("Frames from lightest to heaviest. Each is a quad that gets stretched to fill the view.")]
    public Renderer[] layers;

    [Tooltip("The first layer appears at this many hearts or fewer.")]
    public int startAtHearts = 3;

    [Range(0f, 1f)] public float maxAlpha = 0.9f;
    public float fadeInTime = 0.8f;
    public float fadeOutTime = 1.5f;

    private float[] alpha;
    private int shown;
    private Camera cam;
    private MaterialPropertyBlock block;
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    void Awake()
    {
        cam = GetComponentInParent<Camera>();
        block = new MaterialPropertyBlock();
        alpha = new float[layers.Length];
        for (int i = 0; i < layers.Length; i++)
            if (layers[i] != null)
                layers[i].enabled = false;
    }

    // Call whenever the heart count changes.
    public void SetHearts(int hearts)
    {
        shown = hearts > startAtHearts ? 0 : Mathf.Min(layers.Length, 1 + startAtHearts - Mathf.Max(hearts, 0));
    }

    void LateUpdate()
    {
        // Size of the view at the frames' distance, so they hug the screen edges at any resolution.
        float dist = cam != null ? cam.transform.InverseTransformPoint(transform.position).z : 0.3f;
        float fov = cam != null ? cam.fieldOfView : 60f;
        float aspect = cam != null ? cam.aspect : 16f / 9f;
        float height = 2f * dist * Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);

        for (int i = 0; i < layers.Length; i++)
        {
            Renderer r = layers[i];
            if (r == null)
                continue;

            float target = i < shown ? 1f : 0f;
            float time = target > alpha[i] ? fadeInTime : fadeOutTime;
            alpha[i] = Mathf.MoveTowards(alpha[i], target, Time.deltaTime / Mathf.Max(0.01f, time));

            r.enabled = alpha[i] > 0f;
            if (!r.enabled)
                continue;

            r.transform.localScale = new Vector3(height * aspect, height, 1f);

            Color c = r.sharedMaterial.GetColor(BaseColorId);
            c.a *= maxAlpha * alpha[i];
            r.GetPropertyBlock(block);
            block.SetColor(BaseColorId, c);
            r.SetPropertyBlock(block);
        }
    }
}
