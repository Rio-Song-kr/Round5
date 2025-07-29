using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;

public class LaserSoot : MonoBehaviour
{
    // private LaserSootPool<LaserSoot> _pool;
    private PoolManager _pool;
    private Transform _parentTransform;
    private void OnParticleSystemStopped()
    {
        transform.SetParent(_parentTransform); // Reset parent to pool's transform
        // _pool.Pool.Release(this);
        _pool.Destroy(gameObject);
    }

    // public void SetPool(LaserSootPool<LaserSoot> pool, Transform parentTransform)
    public void SetPool(PoolManager pool, Transform parentTransform)
    {
        _pool = pool;
        _parentTransform = parentTransform;
    }
}