using UnityEngine;

public class ItemRestorer : MonoBehaviour
{
    public PlayerInteraction player;

    void Start()
    {
        if (!GameState.isContinue) return;

        foreach (var item in SaveableItem.All.ToArray())
        {
            // ของที่ใช้หมดแล้ว (เช่นกุญแจที่ไขประตูแล้ว) ให้หายไป
            if (GameState.IsUsed(item.id))
            {
                Destroy(item.gameObject);
                continue;
            }

            var s = GameState.data.items.Find(x => x.id == item.id);
            if (s == null) continue;   // ไม่เคยถูกเซฟ ปล่อยไว้ที่เดิม

            if (s.held)
            {
                player.RestoreHeld(item.pickup);
            }
            else
            {
                item.transform.SetPositionAndRotation(s.position, s.rotation);

                var rb = item.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;   // Unity เก่ากว่า 6 ใช้ rb.velocity
                    rb.angularVelocity = Vector3.zero;
                }
            }
        }
    }
}