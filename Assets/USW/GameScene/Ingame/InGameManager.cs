using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

/// <summary>
/// 3판 2선승 라운드 시스템과 3판 2선승 매치 시스템 관리
/// </summary>
public class InGameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    #region Singleton

    public static InGameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Init();
    }

    #endregion

    #region Events

    public static event Action OnGameStart;
    public static event Action OnRoundStart;
    public static event Action OnRoundEnd;
    public static event Action OnMatchEnd;
    public static event Action OnGameEnd;
    public static event Action OnCardSelectStart;
    public static event Action OnCardSelectEnd;
    public static event Action<string> OnPlayerDefeated;
    public static event Action<bool> OnRematchRequest;

    public static event Action OnPlayerDisconnected;

    #endregion

    #region 게임 status

    [Header("Game Settings")]
    [SerializeField] private int roundsToWinMatch = 2;
    [SerializeField] private int matchesToWinGame = 2;
    [SerializeField] private float roundStartDelay = 3f;

    public enum GameState
    {
        Waiting,
        RoundStarting,
        RoundInProgress,
        RoundEnding,
        CardSelecting,
        MatchEnding,
        GameEnding,
        RematchWaiting
    }

    [SerializeField] private GameState currentGameState = GameState.Waiting;
    public GameState CurrentGameState => currentGameState;

    // 점수 관리
    private Dictionary<string, int> roundScores = new Dictionary<string, int>();
    private Dictionary<string, int> matchScores = new Dictionary<string, int>();
    private int currentRound = 0;
    private int currentMatch = 0;
    private string lastRoundWinner = "";
    private string lastMatchWinner = "";

    // 플레이어 상태 관리 
    private Dictionary<string, PlayerStatus> playerStatusDict = new Dictionary<string, PlayerStatus>();
    private Dictionary<string, bool> playerAliveStatus = new Dictionary<string, bool>();

    // 카드 선택 관리
    private bool isCardSelectTime = false;
    public bool IsCardSelectTime => isCardSelectTime;

    // 리매치 관리
    private Dictionary<string, bool> rematchVotes = new Dictionary<string, bool>();
    private bool isWaitingForRematch = false;
    public bool IsWaitingForRematch => isWaitingForRematch;

    #endregion

    //# 20250807 0900 추가작업 
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        OnGameStart = null;
        OnRoundStart = null;
        OnRoundEnd = null;
        OnMatchEnd = null;
        OnGameEnd = null;
        OnCardSelectStart = null;
        OnCardSelectEnd = null;
        OnPlayerDefeated = null;
        OnRematchRequest = null;
        OnPlayerDisconnected = null;
        // OnPlayerSystemActivate = null;
    }

    public int CurrentRound => currentRound;
    public int CurrentMatch => currentMatch;
    public string LastRoundWinner => lastRoundWinner;
    public string LastMatchWinner => lastMatchWinner;

    private Dictionary<string, bool> _getIsMaster = new Dictionary<string, bool>();
    public Dictionary<string, string> LeftRightActorNumber = new Dictionary<string, string>();

    public Dictionary<string, int> GetRoundScores() => new Dictionary<string, int>(roundScores);
    public Dictionary<string, int> GetMatchScores() => new Dictionary<string, int>(matchScores);

    private TestPlayerManager playerManager;

    private bool _isStarted = false;
    public bool IsStarted => _isStarted;

    public bool IsMapLoaded;
    public bool IsCardSelected;

    //#2025/08/07/02:00 추가 플레이어 시스템 활성화 하는 Action
    // public static Action<bool> OnPlayerSystemActivate;

    private void Start()
    {
        if (PhotonNetwork.OfflineMode) return;

        playerManager = FindFirstObjectByType<TestPlayerManager>();
    }

    public void SetPlayerActorNumber(int firstActorNumber)
    {
        int leftPlayerActorNumber = 0;
        int rightPlayerActorNumber = 0;

        if (firstActorNumber == PhotonNetwork.MasterClient.ActorNumber)
            leftPlayerActorNumber = firstActorNumber;
        else
            rightPlayerActorNumber = firstActorNumber;

        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.ActorNumber == firstActorNumber) continue;

            if (leftPlayerActorNumber == firstActorNumber)
                rightPlayerActorNumber = player.ActorNumber;
            else
                leftPlayerActorNumber = player.ActorNumber;
        }
        string leftPlayerKey = leftPlayerActorNumber.ToString();
        string rightPlayerKey = rightPlayerActorNumber.ToString();

        LeftRightActorNumber["LeftPlayer"] = leftPlayerKey;
        LeftRightActorNumber["RightPlayer"] = rightPlayerKey;
        _getIsMaster[leftPlayerKey] = leftPlayerActorNumber == PhotonNetwork.MasterClient.ActorNumber;
        _getIsMaster[rightPlayerKey] = rightPlayerActorNumber == PhotonNetwork.MasterClient.ActorNumber;
        // Debug.Log(
        //     $"Left[{leftPlayerKey}] : {_getIsMaster[leftPlayerKey]}, Right[{rightPlayerKey}] : {_getIsMaster[rightPlayerKey]}");
    }

    #region Initialization

    private void Init()
    {
        InitializeScores();
        SetGameState(GameState.Waiting);

        // 플레이어 상태 감지 시작
        // StartCoroutine(MonitorPlayerHealth());
    }

    private void InitializeScores()
    {
        roundScores.Clear();
        matchScores.Clear();
        playerAliveStatus.Clear();
        rematchVotes.Clear();

        foreach (var player in PhotonNetwork.PlayerList)
        {
            string playerKey = GetPlayerKey(player);
            roundScores[playerKey] = 0;
            matchScores[playerKey] = 0;
            playerAliveStatus[playerKey] = true;
            rematchVotes[playerKey] = false;
        }
    }

    private string GetPlayerKey(Player player) => player.ActorNumber.ToString();

    #endregion

    #region 게임 플로우

    public void StartGame()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        photonView.RPC("RPC_StartGame", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_StartGame()
    {
        Debug.Log("RPC_StartGame");
        InitializeScores();
        currentRound = 0;
        currentMatch = 0;

        SetGameState(GameState.RoundStarting);


        OnGameStart?.Invoke();

        //#20250807 02:00 추가사항
        // OnPlayerSystemActivate?.Invoke(true);

        StartCoroutine(StartRoundWithDelay());
    }
    //# 필요하실떄 시스템 비활성화 용입니다.
    /*  public void DeactivatePlayerSystem()
     * {
     *      OnplayerSystemActivate?.Invoke(false);
     * }
     */

    public void SetStarted(bool value)
    {
        if (IsCardSelected && IsMapLoaded)
            photonView.RPC(nameof(SetStartedRPC), RpcTarget.All, value);
    }

    [PunRPC]
    public void SetStartedRPC(bool value)
    {
        _isStarted = value;
    }

    public void SetStartedOffline(bool value)
    {
        _isStarted = value;
    }

    private IEnumerator StartRoundWithDelay()
    {
        yield return new WaitForSeconds(roundStartDelay);
        StartRound();
    }

    public void StartRound()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        photonView.RPC("RPC_StartRound", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_StartRound()
    {
        currentRound++;
        SetGameState(GameState.RoundInProgress);

        // 모든 플레이어 상태 초기화
        ResetPlayerStates();

        Debug.Log("Round 시작에서 Player Active 호출(true)");
        // OnPlayerSystemActivate?.Invoke(true);

        if (_isStarted == false)
            SetStarted(true);
        OnRoundStart?.Invoke();
        Debug.Log($"라운드 {currentRound} 시작!");
    }

    private void ResetPlayerStates()
    {
        foreach (string playerKey in playerAliveStatus.Keys.ToArray())
        {
            playerAliveStatus[playerKey] = true;
        }

        // 플레이어 상태 초기화
        foreach (var playerStatus in playerStatusDict.Values)
        {
            if (playerStatus != null)
            {
                playerStatus.InitializeStatus();
            }
        }
    }

    private void EndRound(string winnerKey)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        IsMapLoaded = false;
        photonView.RPC("RPC_EndRound", RpcTarget.All, winnerKey);
    }

    [PunRPC]
    private void RPC_EndRound(string winnerKey)
    {
        SetGameState(GameState.RoundEnding);
        lastRoundWinner = winnerKey;
        roundScores[winnerKey]++;

        Debug.Log($"라운드 종료 승자: {winnerKey}");

        // 매치 승리 확인
        if (roundScores[winnerKey] >= roundsToWinMatch)
        {
            Debug.Log("End Round 시작에서 Player Active 호출");
            StartCoroutine(EndMatchWithDelay(winnerKey));
        }
        else
        {
            StartCoroutine(StartRoundWithDelay());
        }

        OnRoundEnd?.Invoke();
    }

    private IEnumerator EndMatchWithDelay(string winnerKey)
    {
        yield return new WaitForSeconds(0.3f);
        EndMatch(winnerKey);
    }

    private void EndMatch(string winnerKey)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        photonView.RPC("RPC_EndMatch", RpcTarget.All, winnerKey);
    }

    [PunRPC]
    private void RPC_EndMatch(string winnerKey)
    {
        SetGameState(GameState.MatchEnding);
        lastMatchWinner = winnerKey;
        matchScores[winnerKey]++;
        currentMatch++;

        // 라운드 점수 초기화
        foreach (string key in roundScores.Keys.ToArray())
        {
            roundScores[key] = 0;
        }

        Debug.Log($"매치 종료 승자: {winnerKey}");

        // 게임 승리 확인
        if (matchScores[winnerKey] >= matchesToWinGame)
        {
            StartCoroutine(EndGameWithDelay(winnerKey));
        }
        else
        {
            photonView.RPC(nameof(SetIsWinner), RpcTarget.All, _getIsMaster[winnerKey]);

            // 카드 선택 시작
            StartCoroutine(StartCardSelectWithDelay(winnerKey));
        }

        OnMatchEnd?.Invoke();
    }

    private IEnumerator EndGameWithDelay(string winnerKey)
    {
        yield return new WaitForSeconds(2f);
        EndGame(winnerKey);
    }

    private void EndGame(string winnerKey)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        photonView.RPC("RPC_EndGame", RpcTarget.All, winnerKey);
    }

    [PunRPC]
    private void RPC_EndGame(string winnerKey)
    {
        SetGameState(GameState.GameEnding);
        OnGameEnd?.Invoke();

        // 리매치 대기 시작
        // StartCoroutine(StartRematchWaitingWithDelay());
    }

    // private IEnumerator StartRematchWaitingWithDelay()
    // {
    //     yield return new WaitForSeconds(3f);
    //     StartRematchWaiting();
    // }

    #endregion

    #region Card Selection

    private IEnumerator StartCardSelectWithDelay(string winnerKey)
    {
        yield return new WaitForSeconds(2f);
        StartCardSelect(winnerKey);
    }

    // 시작되었을때 IngameManager 에서 StartCardSelect 호출하고
    // 카드 선택이 시작되었을때 Oncardselect Invoke 
    // 그럼 여기에서 이제 CardSelectManager 에서 하나 만들어서 이벤트 핸들러에서 캔버스 활성화 시키고 
    // 근데 여기에서 이미 각자 플레이어가 선택하는게 있지 패배한 플레이어만 셀렉되는거 메서드가요 ? 
    // 그럼 모든플레이어가 선택완료가 되었을떄는 얘 onPlayerPropertiesUpdate 에서 감지하고 
    // 캔버스 비활성화를 해야겠음. 
    private void StartCardSelect(string winnerKey)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        photonView.RPC("RPC_StartCardSelect", RpcTarget.All, winnerKey);
    }

    [PunRPC]
    private void RPC_StartCardSelect(string winnerKey)
    {
        IsCardSelected = false;
        SetGameState(GameState.CardSelecting);
        var cardSelectManager = FindObjectOfType<CardSelectManager>();
        cardSelectManager.ResetCardSelectionState(winnerKey);


        //todo 이긴 사람은 true, 진 사람은 false가 되어야 함
        // var props = new Hashtable();
        // if (winnerKey == PhotonNetwork.LocalPlayer.ActorNumber.ToString())
        //     props["Select"] = true;
        // PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        foreach (var player in PhotonNetwork.PlayerList)
        {
            var props = new Hashtable();
            if (winnerKey == player.ActorNumber.ToString())
                props["Select"] = true;
            else
                props["Select"] = false;

            player.SetCustomProperties(props);
        }

        isCardSelectTime = true;
        OnCardSelectStart?.Invoke();
        Debug.Log("카드 선택 시작");
    }

    public void EndCardSelect()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        photonView.RPC("RPC_EndCardSelect", RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void RPC_EndCardSelect()
    {
        isCardSelectTime = false;
        OnCardSelectEnd?.Invoke();

        // 다음 매치 시작
        StartCoroutine(StartRoundWithDelay());
    }

    #endregion

    #region 플레이어 health 모니터링

    //# 해당 기능을 제거하고 플레이어 사망시 별도로 실행할 수 있도록 변경
    // private IEnumerator MonitorPlayerHealth()
    // {
    //     while (true)
    //     {
    //         yield return new WaitForSeconds(0.1f);
    //
    //         if (currentGameState != GameState.RoundInProgress)
    //             continue;
    //
    //         CheckPlayerHealth();
    //     }
    // }

    public void CheckPlayerHealth()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        photonView.RPC(nameof(SetStartedRPC), RpcTarget.All, false);

        var alivePlayers = new List<string>();

        foreach (var kvp in playerStatusDict)
        {
            string playerKey = kvp.Key;
            var playerStatus = kvp.Value;
            int viewId = playerStatus.GetComponent<PhotonView>().ViewID;

            //todo 사망과 다시 시작 시의 상태 초기화를 분리해야 함
            if (playerStatus != null && playerStatus.IsAlive)
            {
                alivePlayers.Add(playerKey);

                if (!playerAliveStatus[playerKey])
                {
                    playerAliveStatus[playerKey] = true;
                }
            }
            else if (playerAliveStatus[playerKey] && !playerStatus.IsAlive)
            {
                playerAliveStatus[playerKey] = false;
                photonView.RPC("RPC_PlayerDefeated", RpcTarget.All, playerKey);
            }
        }

        // 승부 판정
        if (alivePlayers.Count == 1)
        {
            string winner = alivePlayers[0];
            EndRound(winner);
        }
        else if (alivePlayers.Count == 0)
        {
            // 무승부 - 라운드 재시작
            StartCoroutine(StartRoundWithDelay());
        }
    }

    [PunRPC]
    private void RPC_PlayerDefeated(string playerKey)
    {
        OnPlayerDefeated?.Invoke(playerKey);
        Debug.Log($"플레이어 {playerKey} 패배!");
    }

    public void RegisterPlayerStatus(string playerKey, PlayerStatus playerStatus)
    {
        playerStatusDict[playerKey] = playerStatus;
    }

    public void UnregisterPlayerStatus(string playerKey)
    {
        if (playerStatusDict.ContainsKey(playerKey))
        {
            playerStatusDict.Remove(playerKey);
            // Debug.Log($"플레이어 {playerKey} 상태 등록 해제됨");
        }
    }

    #endregion

    #region 리매치 시스템

    private void StartRematchWaiting()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        photonView.RPC("RPC_StartRematchWaiting", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_StartRematchWaiting()
    {
        SetGameState(GameState.RematchWaiting);
        isWaitingForRematch = true;

        // 투표 초기화
        foreach (string key in rematchVotes.Keys.ToArray())
        {
            rematchVotes[key] = false;
        }

        Debug.Log("리매치 투표 시작!");
    }

    public void VoteRematch(bool vote)
    {
        string playerKey = GetPlayerKey(PhotonNetwork.LocalPlayer);
        photonView.RPC("RPC_VoteRematch", RpcTarget.All, playerKey, vote);
    }

    [PunRPC]
    private void RPC_VoteRematch(string playerKey, bool vote)
    {
        rematchVotes[playerKey] = vote;
        Debug.Log($"플레이어 {playerKey}의 리매치 투표: {vote}");

        CheckRematchVotes();
    }

    private void CheckRematchVotes()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        bool allVoted = true;
        bool allAgree = true;

        foreach (var kvp in rematchVotes)
        {
            if (!kvp.Value)
            {
                allAgree = false;
            }
        }

        if (allAgree)
        {
            // 모든 플레이어가 리매치에 동의
            photonView.RPC("RPC_RematchAccepted", RpcTarget.All);
        }
    }

    [PunRPC]
    private void RPC_RematchAccepted()
    {
        isWaitingForRematch = false;
        OnRematchRequest?.Invoke(true);
        Debug.Log("두명다 리매치 승인함");

        // 게임 재시작
        RPC_StartGame();
    }

    [PunRPC]
    private void RPC_RematchDeclined()
    {
        isWaitingForRematch = false;
        OnRematchRequest?.Invoke(false);
        Debug.Log("리매치 거부됨");

        // 메인 메뉴로 돌아가는건 gamerestartpanel EndGame 에서 처리하고있음
    }

    #endregion

    private void SetGameState(GameState newState)
    {
        currentGameState = newState;
    }

    #region Photon Callbacks

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        string playerKey = GetPlayerKey(newPlayer);
        roundScores[playerKey] = 0;
        matchScores[playerKey] = 0;
        playerAliveStatus[playerKey] = true;
        rematchVotes[playerKey] = false;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        string playerKey = GetPlayerKey(otherPlayer);
        roundScores.Remove(playerKey);
        matchScores.Remove(playerKey);
        playerAliveStatus.Remove(playerKey);
        rematchVotes.Remove(playerKey);
        UnregisterPlayerStatus(playerKey);

        if (PhotonNetwork.PlayerList.Length < 2)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                StopAllCoroutines();
                SetGameState(GameState.Waiting);

                OnPlayerDisconnected?.Invoke();
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(currentGameState);
            stream.SendNext(currentRound);
            stream.SendNext(currentMatch);
            stream.SendNext(isCardSelectTime);
            stream.SendNext(isWaitingForRematch);
        }
        else
        {
            currentGameState = (GameState)stream.ReceiveNext();
            currentRound = (int)stream.ReceiveNext();
            currentMatch = (int)stream.ReceiveNext();
            isCardSelectTime = (bool)stream.ReceiveNext();
            isWaitingForRematch = (bool)stream.ReceiveNext();
        }
    }

    #endregion

    #region 퍼블릭 메서드

    public int GetPlayerRoundScore(string playerKey) => roundScores.ContainsKey(playerKey) ? roundScores[playerKey] : 0;

    public int GetPlayerMatchScore(string playerKey) => matchScores.ContainsKey(playerKey) ? matchScores[playerKey] : 0;

    #endregion

    #region (테스트용)

    private void Update()
    {
        if (PhotonNetwork.OfflineMode) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            DebugLeftPlayerWin();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            DebugRightPlayerWin();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            DebugStartGame();
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            DebugVoteRematch(true);
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            DebugVoteRematch(false);
        }
    }

    /// <summary>
    /// 테스트: 왼쪽 플레이어 승리 처리
    /// </summary>
    public void DebugLeftPlayerWin()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (PhotonNetwork.PlayerList.Length > 0)
        {
            // string leftPlayerKey = PhotonNetwork.PlayerList[0].ActorNumber.ToString();
            string leftPlayerKey = LeftRightActorNumber["LeftPlayer"];
            Debug.Log("왼쪽 플레이어 승리");
            EndRound(leftPlayerKey);
        }
    }

    /// <summary>
    /// 테스트: 오른쪽 플레이어 승리 처리
    /// </summary>
    public void DebugRightPlayerWin()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (PhotonNetwork.PlayerList.Length > 1)
        {
            // string rightPlayerKey = PhotonNetwork.PlayerList[1].ActorNumber.ToString();
            string rightPlayerKey = LeftRightActorNumber["RightPlayer"];
            Debug.Log("오른쪽 플레이어 승리");
            EndRound(rightPlayerKey);
        }
    }

    /// <summary>
    /// 테스트: 게임 시작
    /// </summary>
    public void DebugStartGame()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        Debug.Log("게임 시작!");
        StartGame();
    }

    /// <summary>
    /// 테스트: 리매치 투표
    /// </summary>
    public void DebugVoteRematch(bool vote)
    {
        Debug.Log($"리매치 투표 - {(vote ? "찬성" : "반대")}");
        VoteRematch(vote);
    }

    #endregion

    [PunRPC]
    public void SetIsWinner(bool masterIsWinner)
    {
        bool isWinner = PhotonNetwork.IsMasterClient ? masterIsWinner : !masterIsWinner;

        var winnerProp = new Hashtable
        {
            { "isWinner", isWinner }
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(winnerProp);
        // Debug.Log($"[GameEndManager] ���� {(isWinner ? "����" : "����")}�Դϴ�.");
    }
}