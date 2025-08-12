using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalCursorManager : MonoBehaviour
{
    // private static GlobalCursorManager _instance;
    //
    // void Awake()
    // {
    //     // �̱���: �ߺ� ���� ����
    //     if (_instance != null)
    //     {
    //         Destroy(gameObject);
    //         return;
    //     }
    //     _instance = this;
    //     DontDestroyOnLoad(gameObject);
    //
    //     ApplyCursorSettings();
    //     // ���� �ٲ� ������ Ȥ�� �ٸ� ��ũ��Ʈ�� �ٲ���� ������ ������
    //     SceneManager.activeSceneChanged += OnActiveSceneChanged;
    //
    //     Cursor.lockState = CursorLockMode.Confined;
    // }
    //
    // void OnDestroy()
    // {
    //     if (_instance == this)
    //         SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    // }
    //
    // void OnApplicationFocus(bool hasFocus)
    // {
    //     // Alt+Tab �� ��Ŀ�� ��ȭ �� Ŀ�� ������ (�÷����� �̽� ����)
    //     if (hasFocus) ApplyCursorSettings();
    // }
    //
    // void Update()
    // {
    //     // Ȥ�� �ٸ� �ڵ尡 ����ٸ� ��� �ǵ���
    //     if (!Cursor.visible || Cursor.lockState != CursorLockMode.None)
    //         ApplyCursorSettings();
    // }
    //
    // private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    // {
    //     ApplyCursorSettings();
    // }
    //
    // private void ApplyCursorSettings()
    // {
    //     Cursor.visible = true;                 // Ŀ�� ǥ��
    //     Cursor.lockState = CursorLockMode.None; // ��� ���� (���� �̵�)
    // }
}