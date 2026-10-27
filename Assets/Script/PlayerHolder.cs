using UnityEngine;

public class PlayerHolder : MonoBehaviour
{
    public Transform holdPoint; // จุดถือของ (ตัวเดียวกับที่ส่งให้ PickUp)

    // ของที่ถืออยู่ตอนนี้ (null ถ้ามือว่าง)
    public PickupItem Held
    {
        get
        {
            if (holdPoint == null) return null;
            return holdPoint.GetComponentInChildren<PickupItem>();
        }
    }

    public bool IsHolding(ItemType type)
    {
        PickupItem h = Held;
        return h != null && h.itemType == type;
    }

    public bool IsHoldingKey(string id)
    {
        PickupItem h = Held;
        return h != null && h.itemType == ItemType.Key && h.itemName == id;
    }

    // ใช้ของทิ้ง เช่น กุญแจใช้แล้วหาย
    public void ConsumeHeld()
    {
        PickupItem h = Held;
        if (h != null) Destroy(h.gameObject);
    }
}