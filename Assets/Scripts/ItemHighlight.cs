using UnityEngine;

[RequireComponent(typeof(PickupItem))]
public class ItemHighlight : MonoBehaviour
{
    [Header("Color")]
    public Color outlineColor = new Color(0.2f, 1f, 0.4f, 1f);
    public Color cornerColor = Color.white;

    [Header("Line")]
    public float outlineWidth = 0.012f;
    public float padding = 0.02f;          // ขยายกรอบออกจากตัวของเล็กน้อย

    [Header("Corner Brackets")]
    public bool cornersOnly = true;        // true = แสดงเฉพาะมุม (ดูสะอาดกว่า) / false = กล่องเต็ม
    [Range(0.1f, 0.5f)]
    public float cornerLength = 0.3f;      // สัดส่วนความยาวมุมต่อด้านกล่อง

    [Header("Pulse")]
    public bool pulse = true;
    public float pulseSpeed = 4f;
    [Range(0f, 1f)]
    public float pulseAmount = 0.35f;

    Renderer[] renderers;
    LineRenderer[] lines;
    Material lineMaterial;
    bool visible;

    // มุมทั้ง 8 ของกล่อง: bit0 = x, bit1 = y, bit2 = z (0 = ต่ำ, 1 = สูง)
    static readonly int[,] edges =
    {
        {0,1},{2,3},{4,5},{6,7},   // เส้นแกน X
        {0,2},{1,3},{4,6},{5,7},   // เส้นแกน Y
        {0,4},{1,5},{2,6},{3,7}    // เส้นแกน Z
    };

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();

        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader != null) lineMaterial = new Material(shader);

        int count = cornersOnly ? 24 : 12; // มุมละ 3 เส้น x 8 มุม = 24
        lines = new LineRenderer[count];

        for (int i = 0; i < count; i++)
        {
            GameObject go = new GameObject("Outline_" + i);
            go.transform.SetParent(transform, false);

            LineRenderer lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.positionCount = 2;
            lr.startWidth = lr.endWidth = outlineWidth;
            lr.numCapVertices = 2;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
            if (lineMaterial != null) lr.material = lineMaterial;
            lr.enabled = false;

            lines[i] = lr;
        }
    }

    public void Show(bool show)
    {
        visible = show && renderers != null && renderers.Length > 0;

        if (lines == null) return;
        foreach (LineRenderer lr in lines)
            if (lr != null) lr.enabled = visible;

        if (visible) UpdateOutline();
    }

    void LateUpdate()
    {
        if (visible) UpdateOutline();
    }

    Bounds GetBounds()
    {
        Bounds b = new Bounds();
        bool first = true;

        foreach (Renderer r in renderers)
        {
            if (r == null || r is LineRenderer || r is ParticleSystemRenderer) continue;

            if (first) { b = r.bounds; first = false; }
            else b.Encapsulate(r.bounds);
        }

        b.Expand(padding * 2f);
        return b;
    }

    void UpdateOutline()
    {
        Bounds b = GetBounds();

        Vector3[] p = new Vector3[8];
        for (int i = 0; i < 8; i++)
        {
            p[i] = new Vector3(
                (i & 1) == 0 ? b.min.x : b.max.x,
                (i & 2) == 0 ? b.min.y : b.max.y,
                (i & 4) == 0 ? b.min.z : b.max.z);
        }

        // กะพริบเบาๆ ด้วยความโปร่งใสและความหนา
        float k = pulse ? (Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f) : 1f;
        float alpha = Mathf.Lerp(1f - pulseAmount, 1f, k);
        float width = outlineWidth * Mathf.Lerp(0.85f, 1.15f, k);

        Color c = outlineColor; c.a = alpha;

        if (cornersOnly)
        {
            int idx = 0;
            for (int corner = 0; corner < 8; corner++)
            {
                for (int axis = 0; axis < 3; axis++)
                {
                    int other = corner ^ (1 << axis); // มุมปลายอีกด้านของเส้นนี้
                    Vector3 end = Vector3.Lerp(p[corner], p[other], cornerLength);
                    SetLine(lines[idx++], p[corner], end, cornerColor, c, width);
                }
            }
        }
        else
        {
            for (int i = 0; i < 12; i++)
                SetLine(lines[i], p[edges[i, 0]], p[edges[i, 1]], c, c, width);
        }
    }

    static void SetLine(LineRenderer lr, Vector3 a, Vector3 b, Color startC, Color endC, float width)
    {
        if (lr == null) return;
        lr.SetPosition(0, a);
        lr.SetPosition(1, b);
        lr.startColor = startC;
        lr.endColor = endC;
        lr.startWidth = lr.endWidth = width;
    }

    void OnDestroy()
    {
        if (lineMaterial != null) Destroy(lineMaterial);
    }
}