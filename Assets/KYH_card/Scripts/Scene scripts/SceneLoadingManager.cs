using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


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

    public void LoadSceneAsync(string sceneName)
    {
        StartCoroutine(LoadAsyncRoutine(sceneName));
    }

    /// <summary>
    /// 비동기 로딩 코루틴
    /// </summary> 

    private IEnumerator LoadAsyncRoutine(string sceneName)
    {
        float minDisplayTime = 1.5f; // 로딩화면 최소 유지 시간
        float timer = 0f;

        // 1. 로딩 UI 시작
        if (LoadingUIManager.Instance != null)
        {
            LoadingUIManager.Instance.Show();
        }
        // 2. 씬 비동기 로딩
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            if (LoadingUIManager.Instance != null)
                LoadingUIManager.Instance.UpdateProgress(op.progress / 0.9f);
            yield return null;
        }

        // 3. 최종 진행률
        if (LoadingUIManager.Instance != null)
        {
            LoadingUIManager.Instance.UpdateProgress(1f);
        }

        while (timer < minDisplayTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // 4. 로딩 UI 숨기기
        if (LoadingUIManager.Instance != null)
        {
            LoadingUIManager.Instance.Hide(); // 
        }
        // 5. 씬 전환 허용
        op.allowSceneActivation = true;
        Debug.Log($"다음 씬으로 넘어감. 이동 된 씬 이름 {sceneName}");
    }
}
