using UnityEngine;

public class EmpPool<T> where T : EmpEffect
{

    public PoolManager<T> Pool;

    public void SetPool(GameObject prefab, Transform parentTransform)
    {
        var component = prefab.GetComponent<T>();
        Pool = new PoolManager<T>(component, 2, 5, parentTransform);
    }
}
