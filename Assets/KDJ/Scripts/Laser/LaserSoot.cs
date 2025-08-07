using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

public class LaserSoot : MonoBehaviourPun
{
    private PoolManager _pool;
    private void OnParticleSystemStopped()
    {
        if (PhotonNetwork.OfflineMode == true)
        {
            // 오프라인 모드에서는 PhotonNetwork.Destroy를 사용하지 않고, 일반적으로 Destroy를 사용
            Destroy(gameObject);
            return;
        }
        
        if (!photonView.IsMine) return;

        photonView.RPC(nameof(ReturnToPool), RpcTarget.All);
    }

    [PunRPC]
    private void ReturnToPool()
    {

        if (!photonView.IsMine) return;
        
        PhotonNetwork.Destroy(gameObject);
    }

    public void SetPool()
    {
        if (!photonView.IsMine) return;

        photonView.RPC(nameof(SetPoolRPC), RpcTarget.All);
    }

    [PunRPC]
    private void SetPoolRPC()
    {
        var pool = FindFirstObjectByType<PoolManager>();
        _pool = pool;
    }
}