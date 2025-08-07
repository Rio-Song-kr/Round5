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
    [SerializeField] private Transform player1RealEndPos;
    [SerializeField] private Transform player2RealEndPos;
    
    private string gameSceneName = "InGameScene"; 
    private bool isLoadingComplete = false;
    
    private void Start()
    {
        InitializeLoadingScene();
    }
    
    private void InitializeLoadingScene()
    {
        SetupPlayerNames();
        
        StartCoroutine(LoadingSequence());
    }
    
    /// <summary>
    /// 플레이어 닉네임 설정
    /// </summary>
    private void SetupPlayerNames()
    {
        Debug.Log("Setup Player Names");
        Player[] players = PhotonNetwork.PlayerList;
        Debug.Log($"{players.Length}");
        
        
        Debug.Log($"플레이어 감지 ActorNumber {PhotonNetwork.LocalPlayer.ActorNumber}");
        if (players.Length >= 2)
        {
            string player1Name = players[0].NickName;
            string player2Name = players[1].NickName;
            
            Debug.Log($"$Player1 Name{player1Name}");
            Debug.Log($"$Player2 Name{player2Name}");
            if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
            {
                if (player1NameText) player1NameText.text = player1Name;
                if (player2NameText) player2NameText.text = player2Name;
                Debug.Log($"player1NameText {player1Name}");
                Debug.Log($"player2NameText {player2Name}");
            }
            else
            {
                if (player1NameText) player1NameText.text = player2Name;
                if (player2NameText) player2NameText.text = player1Name;
                Debug.Log($"player2NameText {player2Name}");
                Debug.Log($"player1NameText {player1Name}");
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
        yield return StartCoroutine(PlayNameCrossAnimation());
       
        yield return StartCoroutine(LoadGameScene());
    }
    
    /// <summary>
    /// 닉네임 교차 애니메이션 
    /// </summary>
    private IEnumerator PlayNameCrossAnimation()
    {
      
        player1NameText.transform.position = player1StartPos.position;
        player2NameText.transform.position = player2StartPos.position;
        
      Debug.Log("loadingtextAnimation 시작");
        LoadingTextAnimation gradation1 = player1NameText.GetComponent<LoadingTextAnimation>();
        LoadingTextAnimation gradation2 = player2NameText.GetComponent<LoadingTextAnimation>();
        if (gradation1 != null) gradation1.BeginGradation();
        if (gradation2 != null) gradation2.BeginGradation();

        
        Sequence animSeq = DOTween.Sequence();
        animSeq.Append(player1NameText.DOFade(1f, nameAnimationDuration));
        animSeq.Join(player2NameText.DOFade(1f, nameAnimationDuration * 0.8f + 0.2f));
        animSeq.Join(player1NameText.transform.DOMove(player1EndPos.position, nameAnimationDuration).SetEase(Ease.OutQuad));
        animSeq.Join(player2NameText.transform.DOMove(player2EndPos.position, nameAnimationDuration).SetEase(Ease.OutQuad));
        animSeq.AppendInterval(delayBetweenAnimations); 

        
        yield return animSeq.Play().WaitForCompletion();

        yield return new WaitForSeconds(2f);
        
        Sequence exitSeq = DOTween.Sequence();
        animSeq.Append(player1NameText.transform.DOMove(player1RealEndPos.position, nameAnimationDuration)
            .SetEase(Ease.InQuad));
        animSeq.Join(player2NameText.transform.DOMove(player2RealEndPos.position, nameAnimationDuration)
            .SetEase(Ease.InQuad));
        
        yield return exitSeq.Play().WaitForCompletion();
    }
    /// <summary>
    /// 게임 씬 로딩
    /// </summary>
    private IEnumerator LoadGameScene()
    {
        yield return new WaitForSeconds(loadingDelay);
        
        if (PhotonNetwork.IsMasterClient && !isLoadingComplete)
        {
            isLoadingComplete = true;
            PhotonNetwork.LoadLevel(gameSceneName);
        }
    }
    
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("LobbyScene");
        }
    }
    
}