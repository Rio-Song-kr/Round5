using System.Collections.Generic;
using JetBrains.Annotations;
using Photon.Pun;
using UnityEngine;

public class TestPlayerManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _laserPrefab;
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
    }
}