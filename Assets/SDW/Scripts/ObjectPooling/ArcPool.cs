using UnityEngine;

public class ArcPool<T> where T: ArcController
{
    public PoolManager<T> Pool;

    public void SetPool(GameObject prefab, int arcCount, Transform parentTransform = null)
    {
        var component = prefab.GetComponent<T>();
        Pool = new PoolManager<T>(component, arcCount * 2, arcCount * 4, parentTransform);
    }
}
