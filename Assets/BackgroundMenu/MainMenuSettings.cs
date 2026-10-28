using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuSettings : MonoBehaviour
{
    public Slider sensitivitySlider;

    private void Start()
    {
        
        float savedSens = PlayerPrefs.GetFloat("MouseSensitivity", 2f);
        
        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = savedSens;
            sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        }
    }

    public void OnSensitivityChanged(float value)
    {
        
        PlayerPrefs.SetFloat("MouseSensitivity", value);
        PlayerPrefs.Save();
    }

    public void PlayNewGame()
    {
        // บอกระบบว่าไม่ใช่ Continue (ไม่ต้องวาร์ปไปจุดเดิม ให้ไปเกิดจุดเริ่มต้น)
        GameState.NewGame();
        SceneManager.LoadScene("ชื่อ Scene เริ่มเกม");

        SceneManager.LoadScene("Basement"); // โหลดเข้า Scene เกม
    }

    // 🟢 2. ฟังก์ชันสำหรับปุ่ม "Continue" (เล่นต่อจากจุดที่เซฟไว้)
    public void ContinueGame()
    {
        if (!GameState.HasSave) return;      // ยังไม่เคยเซฟ ไม่ทำอะไร

        GameState.Load();                    // ดึง snapshot จากเครื่อง
        SceneManager.LoadScene(GameState.data.scene);
    }
}