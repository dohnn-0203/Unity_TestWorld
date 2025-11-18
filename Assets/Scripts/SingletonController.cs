using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SingletonController : MonoBehaviour
{
    public static SingletonController instance { get; private set; }

    [Header("Scene Management")]
    [Tooltip("게임 시작 시 추가로 로드할 씬의 이름입니다.")]
    public string sceneToLoad = "Singleton_Scene";

    [Header("Camera Control")]
    public float transitionDuration = 0.5f;
    private Coroutine _cameraMoveCoroutine;

    public Camera singletonCamera;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartCoroutine(LoadSceneAdditivelyAsync(sceneToLoad));
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    private IEnumerator LoadSceneAdditivelyAsync(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("SingletonController: 'sceneToLoad'가 설정되지 않았습니다.");
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
            Debug.Log($"SingletonController: '{sceneName}' 씬을 추가로 로드합니다.");
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            Debug.Log($"SingletonController: '{sceneName}' 씬 로드 완료.");

            Scene loadedScene = SceneManager.GetSceneByName(sceneName);
            if (loadedScene.IsValid())
            {
                GameObject[] rootObjects = loadedScene.GetRootGameObjects();
                Camera sceneCamera = null;
                foreach (var root in rootObjects)
                {
                    sceneCamera = root.GetComponentInChildren<Camera>(true);
                    if (sceneCamera != null)
                    {
                        break;
                    }
                }

                if (sceneCamera != null)
                {
                    singletonCamera = sceneCamera;
                    singletonCamera.gameObject.SetActive(true);
                    Debug.Log($"SingletonController: '{sceneName}'의 카메라를 찾아 'singletonCamera' 변수에 할당했습니다.");
                }
                else
                {
                    Debug.LogWarning($"SingletonController: '{sceneName}' 씬에서 카메라를 찾지 못했습니다.");
                }
            }
        }
        else
        {
            Debug.Log($"SingletonController: '{sceneName}' 씬은 이미 로드되어 있습니다.");
        }
    }

    public void SetCameraActive(bool isActive)
    {
        if (singletonCamera != null)
        {
            singletonCamera.gameObject.SetActive(isActive);
        }
        else
        {
            Debug.LogWarning("SingletonController: 'singletonCamera'가 할당되지 않았습니다.");
        }
    }

    public void MoveCameraToView(Transform targetView)
    {
        if (singletonCamera == null || targetView == null)
        {
            Debug.LogError("카메라 또는 타겟 뷰가 설정되지 않았습니다.");
            return;
        }

        if (_cameraMoveCoroutine != null)
        {
            StopCoroutine(_cameraMoveCoroutine);
        }

        _cameraMoveCoroutine = StartCoroutine(SmoothMoveCamera(targetView.position, targetView.rotation));
    }

    private IEnumerator SmoothMoveCamera(Vector3 targetPosition, Quaternion targetRotation)
    {
        float time = 0;
        Vector3 startPosition = singletonCamera.transform.position;
        Quaternion startRotation = singletonCamera.transform.rotation;

        while (time < transitionDuration)
        {
            singletonCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, time / transitionDuration);
            singletonCamera.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, time / transitionDuration);

            time += Time.deltaTime;
            yield return null;
        }

        singletonCamera.transform.position = targetPosition;
        singletonCamera.transform.rotation = targetRotation;
        _cameraMoveCoroutine = null;
    }
}