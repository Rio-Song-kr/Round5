using UnityEngine;
using UnityEngine.UI;

public class GameRestartPanelController : MonoBehaviour
{
    [SerializeField] Button yesButton;
    [SerializeField] Button noButton;

    private void Awake()
    {
        yesButton.onClick.AddListener(RestartGame);
        noButton.onClick.AddListener(EndGame);
    }

    private void RestartGame()
    {
        TestIngameManager.Instance.GameStart();
        Debug.Log("게임 재시작");
        // TODO : 카드 선택 화면으로 이동
    }

    private void EndGame()
    {
        Debug.Log("게임 종료");
        // TODO : 메인 화면으로 이동
    }
}