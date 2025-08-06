using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using DG.Tweening;

public class LoadingSceneManager : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI player1NameText;
    [SerializeField] private TextMeshProUGUI player2NameText;
    
    [Header("Animation Settings")]
    [SerializeField] private float nameAnimationDuration = 2f;
    [SerializeField] private float delayBetweenAnimations = 0.5f;
    [SerializeField] private float loadingDelay = 3f; 
    
    [Header("Name Animation Positions")]
    [SerializeField] private Transform player1StartPos;
    [SerializeField] private Transform player1EndPos;
    [SerializeField] private Transform player2StartPos;
    [SerializeField] private Transform player2EndPos;
    
    private string gameSceneName = "InGameScene"; 
    private bool isLoadingComplete = false;
    
    private void Start()
    {
        InitializeLoadingScene();
    }
    
    private void InitializeLoadingScene()
    {
        // 플레이어 닉네임 설정
        SetupPlayerNames();
        
        // 로딩 애니메이션 시작
        StartCoroutine(LoadingSequence());
    }
    
    /// <summary>
    /// 플레이어 닉네임 설정
    /// </summary>
    private void SetupPlayerNames()
    {
        Player[] players = PhotonNetwork.PlayerList;
        
        if (players.Length >= 2)
        {
            string player1Name = players[0].NickName;
            string player2Name = players[1].NickName;
            
            // 자신이 플레이어 1인지 2인지에 따라 위치 결정
            if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
            {
                if (player1NameText) player1NameText.text = player1Name;
                if (player2NameText) player2NameText.text = player2Name;
            }
            else
            {
                if (player1NameText) player1NameText.text = player2Name;
                if (player2NameText) player2NameText.text = player1Name;
            }
        }
        else
        {
            // 디버그 때는 임시로
            if (player1NameText) player1NameText.text = "Player1";
            if (player2NameText) player2NameText.text = "Player2";
        }
    }
    
    /// <summary>
    /// 로딩 시퀀스
    /// </summary>
    private IEnumerator LoadingSequence()
    {
        // 1단계: 닉네임 교차 애니메이션
        yield return StartCoroutine(PlayNameCrossAnimation());
        
        // 2단계: 게임 씬 로딩
        yield return StartCoroutine(LoadGameScene());
    }
    
    /// <summary>
    /// 닉네임 교차 애니메이션 (DOTween pro로 대체할 예정임)
    /// </summary>
    private IEnumerator PlayNameCrossAnimation()
    {
        
        yield return StartCoroutine(SimpleNameAnimation());
        
        yield return new WaitForSeconds(nameAnimationDuration);
    }
    
    /// <summary>
    /// 간단한 이름 애니메이션 
    /// </summary>
    private IEnumerator SimpleNameAnimation()
    {
        float timer = 0f;
        
        while (timer < nameAnimationDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / nameAnimationDuration;
            
            // 알파값 애니메이션
            if (player1NameText)
            {
                Color color = player1NameText.color;
                color.a = Mathf.Lerp(0f, 1f, progress);
                player1NameText.color = color;
            }
            
            if (player2NameText)
            {
                Color color = player2NameText.color;
                color.a = Mathf.Lerp(0f, 1f, progress * 0.8f + 0.2f);
                player2NameText.color = color;
            }
            
            yield return null;
        }
    }
    
    /// <summary>
    /// 게임 씬 로딩
    /// </summary>
    private IEnumerator LoadGameScene()
    {
        // 로딩 대기 시간
        yield return new WaitForSeconds(loadingDelay);
        
        // 마스터 클라이언트만 씬 로드
        if (PhotonNetwork.IsMasterClient && !isLoadingComplete)
        {
            isLoadingComplete = true;
            PhotonNetwork.LoadLevel(gameSceneName);
        }
    }
    
    
    #region Photon Callbacks
    
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("LobbyScene");
        }
    }
    
    #endregion
}