
using UnityEngine;
public enum ItemType { Key, Crowbar, Other }

public class PickupItem : MonoBehaviour
{
    public string itemName = "KEY1";

    public ItemType itemType = ItemType.Key;

    public bool IsHeld { get; private set; }

    [Header("ตำแหน่งตอนถือ")]
    public Vector3 heldPosition =
        new Vector3(0.25f, -0.2f, 0.6f);

    public Vector3 heldRotation =
        new Vector3(0f, 0f, 0f);

    public void PickUp(Transform holdPoint)
    {
        // แสดงชื่อกุญแจใน Console เมื่อเก็บสำเร็จ
        Debug.Log("เก็บกุญแจ: " + itemName, this);

        IsHeld = true;

        // ปิด Collider เพื่อไม่ให้บังการตรวจจับวัตถุอื่น
        foreach (Collider col in GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        // หยุดฟิสิกส์ของวัตถุ
        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        // ย้ายวัตถุมาอยู่ในมือ
        transform.SetParent(holdPoint);
        transform.localPosition = heldPosition;
        transform.localRotation =
            Quaternion.Euler(heldRotation);
    }

    public void Drop(Vector3 forward, Vector3 cameraPosition)
    {
        IsHeld = false;
        // นำวัตถุออกจากมือ
        transform.SetParent(null);

        // วางด้านหน้าผู้เล่นเล็กน้อย
        transform.position =
            cameraPosition + forward.normalized * 1.2f
            + Vector3.down * 0.2f;

        // เปิด Collider กลับมา
        foreach (Collider col in GetComponentsInChildren<Collider>())
        {
            col.enabled = true;
        }

        // เปิดฟิสิกส์ให้วัตถุตกลงพื้น
        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
