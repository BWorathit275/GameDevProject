using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PickupItem))]
public class SaveableItem : MonoBehaviour
{
    public static readonly List<SaveableItem> All = new List<SaveableItem>();

    [Tooltip("ต้องไม่ซ้ำกันทุกชิ้น เช่น key_room1")]
    public string id;

    [HideInInspector] public PickupItem pickup;

    static PlayerInteraction player;

    public bool IsHeld
    {
        get
        {
            if (player == null)
                player = FindAnyObjectByType<PlayerInteraction>();
            return player != null && player.Held == pickup;
        }
    }

    void Awake()
    {
        pickup = GetComponent<PickupItem>();
        All.Add(this);
    }

    void OnDestroy() { All.Remove(this); }
}