
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject newgamePanel;
    public TMP_Dropdown difficultyDropdown;
    public GameObject settingsPanel;
    public GameObject exitPanel;

    public void StartNewGame()
    {
        Difficulty difficulty = (Difficulty)difficultyDropdown.value;

        GameState.NewGame(difficulty);
        SceneManager.LoadScene("LoadingScene");
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        newgamePanel.SetActive(false);
        settingsPanel.SetActive(false);
        exitPanel.SetActive(false);
    }

    public void ShowNewGame()
    {
        mainMenuPanel.SetActive(false);
        newgamePanel.SetActive(true);
        settingsPanel.SetActive(false);
        exitPanel.SetActive(false);
    }

    public void ShowSettings()
    {
        mainMenuPanel.SetActive(false);
        newgamePanel.SetActive(false);
        settingsPanel.SetActive(true);
        exitPanel.SetActive(false);
    }

    public void ShowExit()
    {
        mainMenuPanel.SetActive(false);
        newgamePanel.SetActive(false);
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