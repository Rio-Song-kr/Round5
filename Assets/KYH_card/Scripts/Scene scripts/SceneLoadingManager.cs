using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 비동기 로딩 진행하는 매니저 스크립트
/// </summary>
public class SceneLoadingManager : MonoBehaviour
{
    public static SceneLoadingManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시에도 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 외부에서 씬 로딩 요청
    /// </summary>
    private bool isSceneReadyToActivate;
    private AsyncOperation currentOperation;
    private bool allowSceneActivationExternally = false;

    public void LoadSceneAsync(string sceneName)
    {
        isSceneReadyToActivate = false;
        StartCoroutine(LoadAsyncRoutine(sceneName));
    }

    public void AllowSceneActivation()
    {
        isSceneReadyToActivate = true;
    }

    /// <summary>
    /// 비동기 로딩 코루틴
    /// </summary> 

    private IEnumerator LoadAsyncRoutine(string sceneName)
    {
        float minDisplayTime = 1.5f;
        float timer = 0f;

        LoadingUIManager.Instance?.Show();

        currentOperation = SceneManager.LoadSceneAsync(sceneName);
        currentOperation.allowSceneActivation = false;

        while (currentOperation.progress < 0.9f)
        {
            LoadingUIManager.Instance?.UpdateProgress(currentOperation.progress / 0.9f);
            timer += Time.deltaTime;
            yield return null;
        }

        // 90% 도달 시, 1.5초 유지 후 대기
        LoadingUIManager.Instance?.UpdateProgress(1f);

        while (timer < minDisplayTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // 여기서 카드 선택에서 넘어올 때까지 대기
        yield return new WaitUntil(() => isSceneReadyToActivate);

        LoadingUIManager.Instance?.Hide();
        currentOperation.allowSceneActivation = true;

        Debug.Log($"씬 전환 완료: {sceneName}");
    }
}
