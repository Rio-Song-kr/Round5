using System.Collections.Generic;
using JetBrains.Annotations;
using Photon.Pun;
using UnityEngine;

public class TestPlayerManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _laserPrefab;
    [SerializeField] private GameObject _borderEffect;
    [SerializeField] private GameObject _deadFragEffect;
    [SerializeField] private GameObject _deadSmokeEffect;
    [SerializeField] private GameObject _jumpEffect;
    [SerializeField] private GameObject _landEffect;
    [SerializeField] private Camera _camera;
    [SerializeField] private PoolManager _pools;
    public List<GameObject> PlayerList = new List<GameObject>();
    public List<int> PlayerViewIdList = new List<int>();

    /// <summary>
    /// 초기화 루틴을 실행하여 필요한 리소스 풀을 설정하고 초기화
    /// </summary>
    private void Awake()
    {
        _pools = FindFirstObjectByType<PoolManager>();
        _pools.InitializePool(_playerPrefab.name, _playerPrefab, 1, 2);
        _pools.InitializePool(_laserPrefab.name, _laserPrefab, 1, 2);
        _pools.InitializePool("BorderEffect", _borderEffect, 1, 2);
        _pools.InitializePool("DeadFragEffect", _deadFragEffect, 1, 2);
        _pools.InitializePool("DeadSmokeEffect", _deadSmokeEffect, 1, 2);
        _pools.InitializePool("JumpEffectWrap", _jumpEffect, 2, 10);
        _pools.InitializePool("LandEffect", _landEffect, 1, 2);
    }

    /// <summary>
    /// 사용자가 게임 방에 참여했을 때 호출되는 메서드로, 참여한 방에 대한 초기 설정 및 객체 생성을 수행
    /// </summary>
    public override void OnJoinedRoom()
    {
        Debug.Log("방에 참가하셨습니다.");
        var player = PhotonNetwork.Instantiate(_playerPrefab.name, new Vector2(Random.Range(-8f, 0), Random.Range(-4f, 4f)),
            Quaternion.identity);

        int playerViewId = player.GetComponent<PhotonView>().ViewID;
        photonView.RPC(nameof(AddPlayer), RpcTarget.AllBuffered, playerViewId);

        PhotonNetwork.Instantiate(_laserPrefab.name, Vector3.zero, Quaternion.Euler(0, 90, 0));
    }

    [PunRPC]
    private void AddPlayer(int playerViewId)
    {
        PlayerViewIdList.Add(playerViewId);

        var player = PhotonView.Find(playerViewId);
        PlayerList.Add(player.gameObject);

        RegisterPlayerStatusToInGameManager(player.gameObject);
    }

    /// <summary>
    /// Player의 PlayerStatus를 InGameManager에 등록
    /// </summary>
    private void RegisterPlayerStatusToInGameManager(GameObject playerObject)
    {
        // PlayerStatus 컴포넌트 찾고
        var playerStatus = playerObject.GetComponent<PlayerStatus>();

        // PhotonView에서 소유자 정보 가져와서
        var photonView = playerObject.GetComponent<PhotonView>();

        // PlayerKey 생성 후 InGameManager에 등록
        string playerKey = photonView.Owner.ActorNumber.ToString();
        InGameManager.Instance.RegisterPlayerStatus(playerKey, playerStatus);
    }

    /// <summary>
    /// 플레이어가 방을 나갔을 때 InGameManager에서도 해제
    /// list 정순으로 받으니깐 뒤에 요소들이 밀려버리는 현상이 일어나기도하고
    /// ViewId가 쓸대없이 계속 남아있는 경우가 테스트도중 너무 많이 생김 ,
    ///인덱스가 너무 많이 꼬인다용 ,... 이렇게는 되는데 테스트에서는 또 맵이 제대로 생성 안되는 게 느껴짐 ... 이건 형원님한테 헬프쳐야겠다.
    /// </summary>
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        // 해당 플레이어의 오브젝트를 PlayerList에서 제거
        string playerKey = otherPlayer.ActorNumber.ToString();

        // PlayerList에서 해당 플레이어 오브젝트 찾아서 제거
        for (int i = PlayerList.Count - 1; i >= 0; i--)
        {
            if (PlayerList[i] == null) continue;

            var pv = PlayerList[i].GetComponent<PhotonView>();
            if (pv != null && pv.Owner != null && pv.Owner.ActorNumber.ToString() == playerKey)
            {
                PlayerList.RemoveAt(i);

                // ViewID제거
                if (i < PlayerViewIdList.Count)
                {
                    PlayerViewIdList.RemoveAt(i);
                }
                break;
            }
        }
    }
}