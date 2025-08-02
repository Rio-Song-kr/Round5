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
        if (!photonView.IsMine) return;

        photonView.RPC(nameof(ReturnToPool), RpcTarget.All);
    }

    [PunRPC]
    private void ReturnToPool()
    {
        _pool.Destroy(gameObject);
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