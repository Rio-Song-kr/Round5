using Photon.Pun;
using UnityEngine;

public class TestPlayerManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _laserPrefab;
    [SerializeField] private Camera _camera;
    [SerializeField] private PoolManager _pools;

    private void Awake()
    {
        _pools = FindFirstObjectByType<PoolManager>();
        _pools.InitializePool(_playerPrefab.name, _playerPrefab, 1, 2);
        _pools.InitializePool(_laserPrefab.name, _laserPrefab, 1, 2);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("방에 참가하셨습니다.");
        PhotonNetwork.Instantiate(_playerPrefab.name, new Vector2(Random.Range(-8f, 0), Random.Range(-4f, 4f)),
            Quaternion.identity);

        PhotonNetwork.Instantiate(_laserPrefab.name, Vector3.zero, Quaternion.Euler(0, 90, 0));
    }
}