using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingScreen : MonoBehaviour
{
    [Header("UI")]
    public GameObject endingPanel;          // Panel หน้าจอจบ (ปิดไว้ตอนเริ่ม)
    public CanvasGroup endingGroup;         // CanvasGroup บน Panel (ไว้ fade) ไม่ใส่ก็ได้
    public float fadeTime = 1.5f;
    public GameObject crosshair;            // ซ่อน crosshair ตอนจบ (ไม่ใส่ก็ได้)

    [Header("Scene")]
    public string menuSceneName = "MenuScene";   // เปลี่ยนเป็นชื่อซีนเมนูของคุณ

    private bool shown;

    void Start()
    {
        if (endingPanel != null) endingPanel.SetActive(false);
    }

    // เรียกจากประตูบานสุดท้าย
    public void Show()
    {
        if (shown) return;
        shown = true;

        // จบเกมแล้ว ลบเซฟ ไม่ให้ Continue พากลับไปก่อนประตูบานสุดท้าย
        GameState.ClearSave();

        if (crosshair != null) crosshair.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        endingPanel.SetActive(true);
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        Time.timeScale = 0f; // หยุดเกมไว้เบื้องหลัง

        if (endingGroup != null)
        {
            endingGroup.alpha = 0f;
            for (float t = 0f; t < fadeTime; t += Time.unscaledDeltaTime)
            {
                endingGroup.alpha = t / fadeTime;
                yield return null;
            }
            endingGroup.alpha = 1f;
        }
    }

    // ผูกกับปุ่ม "กลับสู่เมนู" บน Panel
    public void BackToMenu()
    {
        Time.timeScale = 1f;   // สำคัญ: ต้องคืนค่า ไม่งั้นเมนูจะค้าง
        SceneManager.LoadScene(menuSceneName);
    }
}