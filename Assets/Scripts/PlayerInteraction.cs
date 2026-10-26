
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction")]
    public Camera playerCamera;
    public Transform handHoldPoint;
    public float interactionDistance = 2.5f;

    [Header("UI Prompt")]
    public Text promptText;

    [Header("Controls")]
    public KeyCode pickupKey = KeyCode.E;
    public KeyCode dropKey = KeyCode.Q;

    private PickupItem currentItem;
    private PickupItem heldItem;
    private PickupItem highlightedItem;

    void Start()
    {
        HidePrompt();
        UpdateHighlight(null);

        if (playerCamera == null)
        {
            Debug.LogWarning(
                "PlayerInteraction: กรุณากำหนด Player Camera"
            );
        }

        if (handHoldPoint == null)
        {
            Debug.LogWarning(
                "PlayerInteraction: กรุณากำหนด Hand Hold Point"
            );
        }
    }

    void Update()
    {
        // ถ้ามีไอเทมอยู่ในมือแล้ว
        if (heldItem != null)
        {
            UpdateHighlight(null);
            HidePrompt();

            if (Input.GetKeyDown(dropKey))
            {
                DropItem();
            }

            return;
        }

        // หาไอเทมที่อยู่ตรงกลางกล้อง
        FindItem();

        // เปิดหรือปิดกรอบสีเขียว
        UpdateHighlight(currentItem);

        if (currentItem != null)
        {
            ShowPrompt(currentItem);

            // กด E เพื่อเก็บ
            if (Input.GetKeyDown(pickupKey))
            {
                PickUpItem();
            }
        }
        else
        {
            HidePrompt();
        }
    }

    void FindItem()
    {
        currentItem = null;

        if (playerCamera == null)
            return;

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            interactionDistance))
        {
            currentItem =
                hit.collider.GetComponentInParent<PickupItem>();
        }
    }

    void PickUpItem()
    {
        if (currentItem == null)
            return;

        if (handHoldPoint == null)
        {
            Debug.LogWarning(
                "ไม่สามารถเก็บของได้: กรุณากำหนด Hand Hold Point"
            );
            return;
        }

        PickupItem itemToPickUp = currentItem;

        UpdateHighlight(null);
        HidePrompt();

        heldItem = itemToPickUp;
        currentItem = null;

        // ให้ไอเทมติดกับกระดูกมือ
        heldItem.PickUp(handHoldPoint);
    }

    void DropItem()
    {
        if (heldItem == null || playerCamera == null)
            return;

        PickupItem itemToDrop = heldItem;
        heldItem = null;

        // ทิ้งไอเทมออกมาด้านหน้ากล้อง
        itemToDrop.Drop(
            playerCamera.transform.forward,
            playerCamera.transform.position
        );

        currentItem = null;

        UpdateHighlight(null);
        HidePrompt();
    }

    void ShowPrompt(PickupItem item)
    {
        if (promptText == null || item == null)
            return;

        promptText.text = "กด E เพื่อเก็บ " + item.itemName;
        promptText.gameObject.SetActive(true);
    }

    void HidePrompt()
    {
        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }
    }

    void UpdateHighlight(PickupItem item)
    {
        if (highlightedItem == item)
            return;

        // ปิดกรอบไอเทมเดิม
        if (highlightedItem != null)
        {
            ItemHighlight oldHighlight =
                highlightedItem.GetComponent<ItemHighlight>();

            if (oldHighlight != null)
            {
                oldHighlight.Show(false);
            }
        }

        highlightedItem = item;

        // เปิดกรอบไอเทมใหม่
        if (highlightedItem != null)
        {
            ItemHighlight newHighlight =
                highlightedItem.GetComponent<ItemHighlight>();

            if (newHighlight != null)
            {
                newHighlight.Show(true);
            }
        }
    }

    void OnDisable()
    {
        if (highlightedItem != null)
        {
            ItemHighlight highlight =
                highlightedItem.GetComponent<ItemHighlight>();

            if (highlight != null)
            {
                highlight.Show(false);
            }
        }

        highlightedItem = null;
        HidePrompt();
    }
}
