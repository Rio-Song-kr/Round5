using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalCursorManager : MonoBehaviour
{
    private static GlobalCursorManager _instance;

    void Awake()
    {
        // 싱글톤: 중복 생성 방지
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        ApplyCursorSettings();
        // 씬이 바뀔 때마다 혹시 다른 스크립트가 바꿔놨을 설정을 재적용
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    void OnDestroy()
    {
        if (_instance == this)
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    void OnApplicationFocus(bool hasFocus)
    {
        // Alt+Tab 등 포커스 변화 시 커서 재적용 (플랫폼별 이슈 방지)
        if (hasFocus) ApplyCursorSettings();
    }

    void Update()
    {
        // 혹시 다른 코드가 숨겼다면 즉시 되돌림
        if (!Cursor.visible || Cursor.lockState != CursorLockMode.None)
            ApplyCursorSettings();
    }

    private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        ApplyCursorSettings();
    }

    private void ApplyCursorSettings()
    {
        Cursor.visible = true;                 // 커서 표시
        Cursor.lockState = CursorLockMode.None; // 잠금 해제 (자유 이동)
    }
}
