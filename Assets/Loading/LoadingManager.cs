using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    public Slider loadingBar;

    [Tooltip("Scene ที่โหลดเมื่อเริ่มเกมใหม่")]
    public string newGameScene = "Basement";

    void Start()
    {
        StartCoroutine(LoadGame());
    }

    IEnumerator LoadGame()
    {
        // Continue = ไป scene ที่เซฟไว้, เริ่มใหม่ = scene เริ่มต้น
        string target = newGameScene;
        if (GameState.isContinue && !string.IsNullOrEmpty(GameState.data.scene))
            target = GameState.data.scene;

        AsyncOperation operation = SceneManager.LoadSceneAsync(target);

        while (!operation.isDone)
        {
            loadingBar.value = Mathf.Clamp01(operation.progress / 0.9f);
            yield return null;
        }
    }
}