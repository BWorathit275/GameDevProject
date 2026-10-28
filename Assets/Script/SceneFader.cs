using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Fades the screen (and sound) to black, loads a scene, then fades back in.
// Creates itself, survives the scene change and runs on real time, so it works while the game is frozen.
public class SceneFader : MonoBehaviour
{
    private static SceneFader active;
    private Image black;

    public static void FadeToScene(string sceneName, float fadeOut = 0.6f, float fadeIn = 0.8f)
    {
        // Already changing scene: ignore extra clicks.
        if (active != null)
            return;

        GameObject go = new GameObject("SceneFader");
        DontDestroyOnLoad(go);
        active = go.AddComponent<SceneFader>();
        active.StartCoroutine(active.Run(sceneName, fadeOut, fadeIn));
    }

    void Awake()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000; // above every other UI
        gameObject.AddComponent<GraphicRaycaster>();

        GameObject image = new GameObject("Black", typeof(RectTransform));
        image.transform.SetParent(transform, false);
        RectTransform rt = (RectTransform)image.transform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        black = image.AddComponent<Image>();
        black.color = new Color(0f, 0f, 0f, 0f);
        black.raycastTarget = true; // blocks clicks while fading
    }

    IEnumerator Run(string sceneName, float fadeOut, float fadeIn)
    {
        float volume = AudioListener.volume;

        for (float t = 0f; t < fadeOut; t += Time.unscaledDeltaTime)
        {
            SetFade(t / fadeOut, volume);
            yield return null;
        }
        SetFade(1f, volume);

        // Undo the death freeze only once the screen is black.
        Time.timeScale = 1f;
        AudioListener.pause = false;

        AsyncOperation load = SceneManager.LoadSceneAsync(sceneName);
        while (!load.isDone)
            yield return null;

        // Let the new scene run a frame before revealing it.
        yield return null;

        for (float t = 0f; t < fadeIn; t += Time.unscaledDeltaTime)
        {
            SetFade(1f - t / fadeIn, volume);
            yield return null;
        }

        AudioListener.volume = volume;
        active = null;
        Destroy(gameObject);
    }

    void SetFade(float amount, float volume)
    {
        black.color = new Color(0f, 0f, 0f, amount);
        AudioListener.volume = volume * (1f - amount);
    }
}
