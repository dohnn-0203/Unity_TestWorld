using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Tooltip("게임 시작 시 추가로 로드할 씬의 이름입니다.")]
    public string sceneToLoad = "Singleton_Scene";

    void Start()
    {
        StartCoroutine(LoadSceneAdditivelyAsync(sceneToLoad));
    }

    private IEnumerator LoadSceneAdditivelyAsync(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("SceneLoader: 'sceneToLoad'가 설정되지 않았습니다.");
            yield break;
        }

        bool isLoaded = false;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).name == sceneName)
            {
                isLoaded = true;
                break;
            }
        }

        if (!isLoaded)
        {
            Debug.Log($"SceneLoader: '{sceneName}' 씬을 추가로 로드합니다.");
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            Debug.Log($"SceneLoader: '{sceneName}' 씬 로드 완료.");
        }
        else
        {
            Debug.Log($"SceneLoader: '{sceneName}' 씬은 이미 로드되어 있습니다.");
        }
    }
}