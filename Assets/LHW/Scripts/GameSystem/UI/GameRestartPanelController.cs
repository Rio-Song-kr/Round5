using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameRestartPanelController : MonoBehaviourPunCallbacks
{
    [SerializeField] Button yesButton;
    [SerializeField] Button noButton;

    private void Awake()
    {
        yesButton.onClick.AddListener(() => VoteRematch(true));
        noButton.onClick.AddListener(() => VoteRematch(false));
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        InGameManager.OnRematchRequest += OnRematchResult;
        ResetButtonStates();
    }

    private void OnDisable()
    {
        InGameManager.OnRematchRequest -= OnRematchResult;
    }
    
    /// <summary>
    /// 리매치 투표
    /// </summary>
    private void VoteRematch(bool vote)
    {
        if (InGameManager.Instance != null)
        {
            InGameManager.Instance.VoteRematch(vote);
            yesButton.interactable = false;
            noButton.interactable = false;
        }
    }
    
    /// <summary>
    /// InGameManager에서 전달하는 리매치 결과 처리
    /// </summary>
    private void OnRematchResult(bool accepted)
    {
        if (accepted)
        {
            Debug.Log("리매치 승인");
            gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("리매치 거부됨");
            EndGame();
        }
    }
    
    private void EndGame()
    {
        PhotonNetwork.LeaveRoom(); 
        gameObject.SetActive(false);
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("LobbyScene");
    }
    
    /// <summary>
    /// 패널 활성화 시 버튼 상태 초기화
    /// </summary>
    private void ResetButtonStates()
    {
        yesButton.interactable = true;
        noButton.interactable = true;
    }

    [PunRPC]
    public void GameRestartPanelActivate(bool activation)
    {
        gameObject.SetActive(activation);
    }
}