using System.Data;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameRestartPanelController : MonoBehaviourPunCallbacks
{
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;
    [SerializeField] private TMP_Text winnerText;
    [SerializeField] private TMP_Text waitingText;

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
            Debug.Log($"Rematch Voted - {PhotonNetwork.PlayerList.Length}");
            InGameManager.Instance.VoteRematch(vote);
            yesButton.interactable = false;
            noButton.interactable = false;
            waitingText.text =
                vote ? "Try Rematch, waiting for opponent decision..." : "Quit Game, waiting for opponent decision...";
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
            SceneManager.LoadScene("TempLoadingScene");
        }
        else
        {
            Debug.Log("리매치 거부됨");
            EndGame();
        }
    }

    private void EndGame()
    {
        // if (isLeaving) return;
        // isLeaving = true;

        Debug.Log("방에나감.");
        PhotonNetwork.LeaveRoom();
    }

    // public override void OnLeftRoom()
    // {
    //     Debug.Log("씬 전환 - OnLeftRoom");
    //     SceneManager.LoadScene("USW/LobbyScene/LobbyScene");
    //     SoundManager.Instance.PlayBGMLoop("MainMenuLoop");
    // }
    //
    // public override void OnPlayerLeftRoom(Player otherPlayer)
    // {
    //     Debug.Log("씬 전환 - OnPlayerLeftROom");
    //     SceneManager.LoadScene("USW/LobbyScene/LobbyScene");
    //     SoundManager.Instance.PlayBGMLoop("MainMenuLoop");
    // }

    //#  20250809 추가사항
    private string GetPlayerNickName(string actorNumberString)
    {
        if (int.TryParse(actorNumberString, out int actorNumber))
        {
            var player = PhotonNetwork.PlayerList.FirstOrDefault(p => p.ActorNumber == actorNumber);
            return player?.NickName ?? "Unknown Player";
        }
        return "Unknown Player";
    }

    /// <summary>
    /// 패널 활성화 시 버튼 상태 초기화
    /// </summary>
    private void ResetButtonStates()
    {
        yesButton.interactable = true;
        noButton.interactable = true;
        string winnerNickName = GetPlayerNickName(InGameManager.Instance.LastMatchWinner);
        winnerText.text = $"Winner : {winnerNickName}";

        waitingText.text = "";
    }

    [PunRPC]
    public void GameRestartPanelActivate(bool activation)
    {
        gameObject.SetActive(activation);
    }
}