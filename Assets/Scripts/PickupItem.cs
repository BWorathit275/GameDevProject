
using UnityEngine;
public enum ItemType { Key, Crowbar, Other }

public class PickupItem : MonoBehaviour
{
    public string itemName = "KEY1";
    Vector3 worldScale;

    public ItemType itemType = ItemType.Key;

    public bool IsHeld { get; private set; }

    [Header("ตำแหน่งตอนถือ")]
    public Vector3 heldPosition =
        new Vector3(0.25f, -0.2f, 0.6f);

    public Vector3 heldRotation =
        new Vector3(0f, 0f, 0f);


    [Header("การวาง")]
    public float dropDistance = 1.2f;     // ระยะวางหน้าผู้เล่น
    public float dropClearance = 0.05f;   // เว้นเหนือพื้นนิดหน่อย
    public float throwSpeed = 1.5f;       // ความเร็วตอนปล่อย (0 = ปล่อยดิ่งลง)

    void Awake()
    {
        worldScale = transform.lossyScale; // ขนาดจริงในโลกตอนวางอยู่
    }

    public void PickUp(Transform holdPoint)
    {
        Debug.Log("เก็บกุญแจ: " + itemName, this);

        IsHeld = true;

        // ปิด Collider ตอนถือ
        foreach (Collider col in GetComponentsInChildren<Collider>())
            col.enabled = false;

        // หยุดฟิสิกส์ และล้างค่าที่ Drop() ตั้งไว้
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.interpolation = RigidbodyInterpolation.None; // <<< สำคัญ
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        transform.SetParent(holdPoint, false);
        transform.localPosition = heldPosition;
        transform.localRotation = Quaternion.Euler(heldRotation);

        // ชดเชย scale ของมือ ให้ของมีขนาดในโลกเท่าเดิม
        Vector3 p = holdPoint.lossyScale;
        transform.localScale = new Vector3(
            worldScale.x / p.x,
            worldScale.y / p.y,
            worldScale.z / p.z);
    }

    public void Drop(Vector3 forward, Vector3 cameraPosition)
    {
        IsHeld = false;
        transform.SetParent(null);
        transform.localScale = worldScale;

        Collider[] cols = GetComponentsInChildren<Collider>(true);
        Bounds bounds = GetWorldBounds();
        float radius = Mathf.Max(bounds.extents.x, bounds.extents.z);

        // 1) ระยะวาง: ไกลขึ้นตามขนาดของ แต่ไม่ทะลุผนัง
        Vector3 dir = forward; dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) dir = Vector3.forward;
        dir.Normalize();

        float wanted = dropDistance + radius;
        Vector3 origin = cameraPosition;
        float dist = wanted;

        if (Physics.SphereCast(origin, 0.15f, dir, out RaycastHit wallHit, wanted,
            ~0, QueryTriggerInteraction.Ignore) && !wallHit.collider.transform.IsChildOf(transform))
        {
            dist = Mathf.Max(0.3f, wallHit.distance - radius);
        }

        Vector3 target = origin + dir * dist;

        // 2) หาพื้นจากด้านบนลงมา เพื่อไม่ให้ของจมดิน
        Vector3 rayStart = target + Vector3.up * 1.0f;
        float groundY = target.y - 1f;

        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit groundHit, 5f,
            ~0, QueryTriggerInteraction.Ignore))
        {
            groundY = groundHit.point.y;
        }

        // 3) ตั้งตำแหน่ง: ก้นของอยู่เหนือพื้นพอดี
        float bottomOffset = transform.position.y - bounds.min.y; // ระยะจากจุดหมุนถึงก้นของ
        transform.position = new Vector3(target.x, groundY + bottomOffset + dropClearance, target.z);

        // 4) เปิดฟิสิกส์ (เปิด Collider หลังตั้งตำแหน่งแล้วเท่านั้น)
        Rigidbody rb = GetComponent<Rigidbody>();

        // ไม่ชนกับตัวผู้เล่นช่วงแรก กันดีดตัวละคร
        Collider[] playerCols = null;
        Transform playerRoot = FindPlayerRoot();
        if (playerRoot != null)
            playerCols = playerRoot.GetComponentsInChildren<Collider>();

        foreach (Collider col in cols)
        {
            col.enabled = true;
            if (playerCols != null)
                foreach (Collider pc in playerCols)
                    if (pc != null) Physics.IgnoreCollision(col, pc, true);
        }

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; // กันทะลุพื้น
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.linearVelocity = dir * throwSpeed;
            rb.angularVelocity = Vector3.zero;
        }

        // เปิดชนกับผู้เล่นกลับหลังของพ้นตัวแล้ว
        if (playerCols != null)
            StartCoroutine(RestoreCollisionWithPlayer(cols, playerCols, 0.6f));
    }

    System.Collections.IEnumerator RestoreCollisionWithPlayer(Collider[] items, Collider[] players, float delay)
    {
        yield return new WaitForSeconds(delay);

        foreach (Collider col in items)
            foreach (Collider pc in players)
                if (col != null && pc != null)
                    Physics.IgnoreCollision(col, pc, false);
    }

    Transform FindPlayerRoot()
    {
        PlayerInteraction pi = FindFirstObjectByType<PlayerInteraction>();
        return pi != null ? pi.transform : null;
    }

    Bounds GetWorldBounds()
    {
        Renderer[] rs = GetComponentsInChildren<Renderer>();
        Bounds b = new Bounds(transform.position, Vector3.zero);
        bool first = true;
        foreach (Renderer r in rs)
        {
            if (r is LineRenderer || r is ParticleSystemRenderer) continue;
            if (first) { b = r.bounds; first = false; }
            else b.Encapsulate(r.bounds);
        }
        return b;
    }
}
