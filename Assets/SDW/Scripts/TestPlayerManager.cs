using Photon.Pun;
using UnityEngine;

public class TestPlayerManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject _PlayerPrefab;
    [SerializeField] private Camera _camera;
    [SerializeField] private PoolManager _pools;

    private void Awake()
    {
        _pools = FindFirstObjectByType<PoolManager>();
        _pools.InitializePool(_PlayerPrefab.name, _PlayerPrefab, 1, 2);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("방에 참가하셨습니다.");
        PhotonNetwork.Instantiate(_PlayerPrefab.name, new Vector2(Random.Range(-8f, 0), Random.Range(-4f, 4f)),
            Quaternion.identity);
    }
}