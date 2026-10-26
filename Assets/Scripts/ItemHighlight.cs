
using UnityEngine;

[RequireComponent(typeof(PickupItem))]
public class ItemHighlight : MonoBehaviour
{
    public Color outlineColor = Color.green;
    public float outlineWidth = 0.025f;

    private LineRenderer[] lines;
    private Collider targetCollider;
    private Material lineMaterial;

    void Awake()
    {
        targetCollider = GetComponentInChildren<Collider>();

        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
            shader = Shader.Find("Universal Render Pipeline/Unlit");

        if (shader != null)
            lineMaterial = new Material(shader);

        lines = new LineRenderer[3];

        for (int i = 0; i < 3; i++)
        {
            GameObject lineObject = new GameObject("GreenOutline_" + i);
            lineObject.transform.SetParent(transform, false);

            LineRenderer line = lineObject.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.loop = true;
            line.positionCount = 4;
            line.startWidth = outlineWidth;
            line.endWidth = outlineWidth;

            if (lineMaterial != null)
                line.material = lineMaterial;

            line.startColor = outlineColor;
            line.endColor = outlineColor;
            line.enabled = false;

            lines[i] = line;
        }
    }

    public void Show(bool show)
    {
        if (lines == null)
            return;

        foreach (LineRenderer line in lines)
        {
            if (line != null)
                line.enabled = show && targetCollider != null;
        }

        if (show)
            UpdateOutline();
    }

    void LateUpdate()
    {
        if (lines != null && lines[0].enabled)
            UpdateOutline();
    }

    void UpdateOutline()
    {
        if (targetCollider == null)
            return;

        Bounds b = targetCollider.bounds;

        Vector3 c = b.center;
        Vector3 e = b.extents;

        // กรอบระนาบ XY
        lines[0].SetPositions(new Vector3[]
        {
            c + new Vector3(-e.x, -e.y, -e.z),
            c + new Vector3( e.x, -e.y, -e.z),
            c + new Vector3( e.x,  e.y, -e.z),
            c + new Vector3(-e.x,  e.y, -e.z)
        });

        // กรอบระนาบ XZ
        lines[1].SetPositions(new Vector3[]
        {
            c + new Vector3(-e.x, e.y, -e.z),
            c + new Vector3( e.x, e.y, -e.z),
            c + new Vector3( e.x, e.y,  e.z),
            c + new Vector3(-e.x, e.y,  e.z)
        });

        // กรอบระนาบ YZ
        lines[2].SetPositions(new Vector3[]
        {
            c + new Vector3(e.x, -e.y, -e.z),
            c + new Vector3(e.x,  e.y, -e.z),
            c + new Vector3(e.x,  e.y,  e.z),
            c + new Vector3(e.x, -e.y,  e.z)
        });
    }

    void OnDestroy()
    {
        if (lineMaterial != null)
            Destroy(lineMaterial);
    }
}
