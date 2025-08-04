using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 게임 재시작 패널 조작
/// </summary>
public class GameRestartPanelController : MonoBehaviourPun
{
    [SerializeField] Button yesButton;
    [SerializeField] Button noButton;

    Coroutine restartPanelCoroutine;

    private void Awake()
    {
        yesButton.onClick.AddListener(RestartGame);
        noButton.onClick.AddListener(EndGame);
        gameObject.SetActive(false);
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

    [PunRPC]
    public void GameRestartPanelActivate(bool activation)
    {
        gameObject.SetActive(activation);
    }
}