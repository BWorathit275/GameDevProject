
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    public Slider loadingBar;

    void Start()
    {
        StartCoroutine(LoadGame());
    }

    IEnumerator LoadGame()
    {
        AsyncOperation operation =
            SceneManager.LoadSceneAsync("Basement");

        while (!operation.isDone)
        {
            loadingBar.value =
                Mathf.Clamp01(operation.progress / 0.9f);

            yield return null;
        }
    }
}
