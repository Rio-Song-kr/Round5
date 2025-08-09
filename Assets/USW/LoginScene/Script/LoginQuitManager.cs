using UnityEditor;
using UnityEngine;

public class LoginQuitManager : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (PopupManager.Instance)
            {
                PopupManager.Instance.ShowConfirmationPopup(
                    "정말로 나가시겠습니까?",
                    () => QuitApplication(),
                    null);
            }
        }
    }

    void QuitApplication()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}