using System;
using System.Collections.Generic;
using UnityEngine;

// ข้อมูลทั้งหมดที่ต้องการเซฟ เพิ่มตัวแปรตรงนี้ได้เรื่อย ๆ
[Serializable]
public class SaveData
{
    public Vector3 position;
    public int health;
    public string scene;
    public List<string> usedItems = new List<string>();
}

// ตัวเก็บ snapshot ของเกม (static ไม่ต้องแปะกับ GameObject)
public static class GameState
{
    const string KEY = "SaveData";

    public static SaveData data = new SaveData();
    public static bool isContinue;

    public static bool HasSave => PlayerPrefs.HasKey(KEY);

    // บันทึก snapshot ทั้งก้อนเป็น JSON
    public static void Save()
    {
        PlayerPrefs.SetString(KEY, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    // เรียกจากปุ่ม Continue ก่อนโหลด Scene
    public static void Load()
    {
        if (!HasSave) return;
        data = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(KEY));
        isContinue = true;
    }

    // เรียกจากปุ่ม New Game
    public static void NewGame()
    {
        data = new SaveData();
        isContinue = false;
        PlayerPrefs.DeleteKey(KEY);
    }

    // ไอเทม: ใช้ ID ตั้งเองได้เลย เช่น "key_room1"
    public static bool IsUsed(string id) => data.usedItems.Contains(id);
    public static void MarkUsed(string id) { if (!IsUsed(id)) data.usedItems.Add(id); }
}