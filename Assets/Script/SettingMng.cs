using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingMng : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject settingMenuPanel;
    public Slider mouseSlider;

    [Header("Save System")]
    public Transform playerTransform;
    public PlayerHealth playerHealth;

    public static bool isPaused = false;
    private const string SENS_KEY = "MouseSensitivity";

    private void Start()
    {
        if (GameState.isContinue) LoadPosition();

        if (mouseSlider != null)
        {
            mouseSlider.value = PlayerPrefs.GetFloat(SENS_KEY, 2f);
            mouseSlider.onValueChanged.AddListener(v => PlayerPrefs.SetFloat(SENS_KEY, v));
        }

        CloseMenu();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) CloseMenu(); else OpenMenu();
        }
    }

    public void OpenMenu() => SetMenu(true);
    public void CloseMenu() => SetMenu(false);

    private void SetMenu(bool open)
    {
        if (settingMenuPanel != null) settingMenuPanel.SetActive(open);
        Time.timeScale = open ? 0f : 1f;
        isPaused = open;
        Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = open;
    }

    public void ExitToMainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("BackgroundMenu");
    }

    // ถ่าย snapshot ณ ตอนนี้ แล้วเซฟทั้งก้อน (ไอเทมที่ใช้แล้วอยู่ใน data อยู่แล้ว)
    public void SaveGame()
    {
        Debug.Log("กดปุ่ม Save แล้ว");

        if (playerTransform != null) GameState.data.position = playerTransform.position;
        else Debug.LogWarning("SettingMng: ยังไม่ได้ลาก Player Transform ใน Inspector");

        if (playerHealth != null) GameState.data.health = playerHealth.currentHealth;
        else Debug.LogWarning("SettingMng: ยังไม่ได้ลาก Player Health ใน Inspector");

        GameState.data.scene = SceneManager.GetActiveScene().name;
        GameState.Save();

        if (mouseSlider != null) PlayerPrefs.SetFloat(SENS_KEY, mouseSlider.value);
        PlayerPrefs.Save();

        Debug.Log(">>> เซฟเกมเรียบร้อย <<<");

        CloseMenu();
    }

    private void LoadPosition()
    {
        var cc = playerTransform.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;   // กัน Physics ล็อกพิกัด
        playerTransform.position = GameState.data.position;
        if (cc != null) cc.enabled = true;
    }
}