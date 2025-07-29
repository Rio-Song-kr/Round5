using UnityEngine;
using UnityEngine.Pool;

public class LaserPoolManager<T> where T : MonoBehaviour
{
    private readonly IObjectPool<T> _pool;
    private bool _isSceneChanged = false;

    private void Start()
    {
        // GameManager.Instance.OnSceneChanged += OnSceneChanged;
    }

    public LaserPoolManager(T prefab, int defaultCapacity = 5, int maxSize = 10, Transform parentTransform = null)
    {
        _pool = new ObjectPool<T>
        (
            () => parentTransform == null ? Object.Instantiate(prefab) : Object.Instantiate(prefab, parentTransform),
            obj => obj?.gameObject.SetActive(true),
            obj => obj?.gameObject.SetActive(false),
            obj => Object.Destroy(obj?.gameObject),
            true,
            defaultCapacity,
            maxSize
        );
    }

    public T Get() => _isSceneChanged ? null : _pool.Get();

    public void Release(T obj)
    {
        if (_isSceneChanged) return;
        _pool?.Release(obj);
    }

    private void OnSceneChanged() => _isSceneChanged = true;
}