using System;
using System.Collections.Generic;
using UnityEngine;

public enum Difficulty { Easy = 0, Normal = 1, Hard = 2 }

[Serializable]
public class ItemSave
{
    public string id;
    public Vector3 position;
    public Quaternion rotation;
    public bool held;          // true = อยู่ในมือผู้เล่น
}

[Serializable]
public class SaveData
{
    public Vector3 position;
    public int health;
    public int maxHealth;      // เพิ่ม: หัวใจสูงสุดตามระดับความยาก
    public int difficulty;     // เพิ่ม: เก็บเป็น int เพื่อให้ JsonUtility เซฟได้ง่าย
    public string scene;
    public List<string> usedItems = new List<string>();
    public List<ItemSave> items = new List<ItemSave>();
}

public static class GameState
{
    const string KEY = "SaveData";

    public static SaveData data = new SaveData();
    public static bool isContinue;

    public static bool HasSave => PlayerPrefs.HasKey(KEY);

    public static Difficulty CurrentDifficulty => (Difficulty)data.difficulty;

    // ง่าย 5 / ปกติ 3 / ยาก 1
    public static int HeartsFor(Difficulty d)
    {
        switch (d)
        {
            case Difficulty.Easy: return 5;
            case Difficulty.Hard: return 1;
            default: return 3;
        }
    }

    public static void Save()
    {
        CaptureItems();
        PlayerPrefs.SetString(KEY, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    static void CaptureItems()
    {
        data.items.Clear();
        foreach (var item in SaveableItem.All)
        {
            if (item == null || IsUsed(item.id)) continue;   // ของที่ใช้หมดแล้วไม่ต้องเซฟตำแหน่ง

            data.items.Add(new ItemSave
            {
                id = item.id,
                position = item.transform.position,
                rotation = item.transform.rotation,
                held = item.IsHeld
            });
        }
    }

    public static void Load()
    {
        if (!HasSave) return;
        data = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(KEY));

        // กันเซฟเก่าที่ยังไม่มี maxHealth
        if (data.maxHealth <= 0) data.maxHealth = HeartsFor(CurrentDifficulty);

        isContinue = true;
    }

    public static void NewGame(Difficulty difficulty = Difficulty.Normal)
    {
        data = new SaveData();
        data.difficulty = (int)difficulty;
        data.maxHealth = HeartsFor(difficulty);
        data.health = data.maxHealth;   // เริ่มด้วยหัวใจเต็ม

        isContinue = false;
        PlayerPrefs.DeleteKey(KEY);
    }

    public static void ClearSave()
    {
        PlayerPrefs.DeleteKey(KEY);
        PlayerPrefs.Save();
        isContinue = false;
    }

    public static bool IsUsed(string id) => data.usedItems.Contains(id);
    public static void MarkUsed(string id) { if (!IsUsed(id)) data.usedItems.Add(id); }
}