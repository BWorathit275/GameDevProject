
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject loadGamePanel;
    public GameObject settingsPanel;
    public GameObject exitPanel;

    public void StartNewGame()
    {
    GameState.NewGame();
    SceneManager.LoadScene("LoadingScene");
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        loadGamePanel.SetActive(false);
        settingsPanel.SetActive(false);
        exitPanel.SetActive(false);
    }

    public void ShowLoadGame()
    {
        mainMenuPanel.SetActive(false);
        loadGamePanel.SetActive(true);
        settingsPanel.SetActive(false);
        exitPanel.SetActive(false);
    }

    public void ShowSettings()
    {
        mainMenuPanel.SetActive(false);
        loadGamePanel.SetActive(false);
        settingsPanel.SetActive(true);
        exitPanel.SetActive(false);
    }

    public void ShowExit()
    {
        mainMenuPanel.SetActive(false);
        loadGamePanel.SetActive(false);
        settingsPanel.SetActive(false);
        exitPanel.SetActive(true);
    }

    public void CancelExit()
    {
        ShowMainMenu();
    }

    public void ContinueGame()
    {
        if (!GameState.HasSave) return;      // ยังไม่เคยเซฟ ไม่ทำอะไร

        GameState.Load();                    // ดึง snapshot จากเครื่อง
        SceneManager.LoadScene("LoadingScene");
    }
}