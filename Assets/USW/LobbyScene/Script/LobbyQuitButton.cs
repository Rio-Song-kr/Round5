using UnityEditor;
using UnityEngine;

public class LobbyQuitButton : MonoBehaviour
{
    public void ShowQuitConfirmation()
    {
        if (PopupManager.Instance)
        {
            PopupManager.Instance.ShowConfirmationPopup("정말로 나가시겠습니까?",()=>QuitApplication(),null);
        }
    }

    void QuitApplication()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false; 
#else
    Application.Quit(); // 빌드 시 실제 종료
#endif
    }
}
