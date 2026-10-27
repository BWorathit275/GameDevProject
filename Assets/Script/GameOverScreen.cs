using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// "You died" screen with Retry and Main Menu. PlayerHealth calls Show when the player dies.
// Works while the game is frozen (Time.timeScale = 0), so everything here runs on real time.
public class GameOverScreen : MonoBehaviour
{
    public CanvasGroup group;
    [Tooltip("Player scripts to switch off on death, e.g. movement and mouse look.")]
    public MonoBehaviour[] disableOnShow;

    [Tooltip("Seconds to hold on the death view before the screen fades in.")]
    public float delay = 1.2f;
    public float fadeTime = 1f;
    public string menuScene = "BackgroundMenu";

    void Awake()
    {
        Hide();
    }

    public void Show()
    {
        StartCoroutine(ShowRoutine());
    }

    IEnumerator ShowRoutine()
    {
        foreach (MonoBehaviour b in disableOnShow)
            if (b != null)
                b.enabled = false;

        AudioListener.pause = true;

        yield return new WaitForSecondsRealtime(delay);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        for (float t = 0f; t < fadeTime; t += Time.unscaledDeltaTime)
        {
            group.alpha = t / fadeTime;
            yield return null;
        }

        group.alpha = 1f;
        group.interactable = true;
        group.blocksRaycasts = true;
    }

    void Hide()
    {
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
    }

    public void Retry()
    {
        Resume();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void MainMenu()
    {
        Resume();
        // The menu needs a free mouse cursor.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(menuScene);
    }

    // Undo the death freeze before leaving, or the next scene starts frozen and silent.
    void Resume()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }
}
