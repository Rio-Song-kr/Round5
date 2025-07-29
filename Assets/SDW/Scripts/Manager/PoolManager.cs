using UnityEngine;
using UnityEngine.Pool;
using Photon.Pun;
using System.Collections.Generic;

public class PoolManager : MonoBehaviour, IPunPrefabPool
{
    private Dictionary<string, IObjectPool<GameObject>> _pools = new Dictionary<string, IObjectPool<GameObject>>();

    // private void Start() => PhotonNetwork.PrefabPool = this;

    /// <summary>
    /// 풀 초기화 메서드를 통해 지정된 ID의 객체 풀을 생성하거나 설정
    /// </summary>
    /// <param name="prefabId">객체 풀을 식별하기 위한 고유 문자열 ID</param>
    /// <param name="prefab">풀에서 객체를 생성할 프리팹(Prefab)</param>
    /// <param name="defaultCapacity">기본적으로 풀에 유지할 객체의 수</param>
    /// <param name="maxSize">풀의 최대 객체 수 제한</param>
    public void InitializePool(string prefabId, GameObject prefab, int defaultCapacity = 5, int maxSize = 60)
    {
        if (!_pools.ContainsKey(prefabId))
        {
            _pools[prefabId] = new ObjectPool<GameObject>(
                () =>
                {
                    var newGameObject = Instantiate(prefab);

                    if (newGameObject.GetComponent<PhotonView>() == null)
                        newGameObject.AddComponent<PhotonView>();

                    return newGameObject;
                },
                obj => obj.SetActive(true),
                obj => obj.SetActive(false),
                obj => Destroy(obj),
                true,
                defaultCapacity,
                maxSize
            );
        }
    }

    /// <summary>
    /// 지정된 프리팹 ID를 기반으로 게임 오브젝트를 위치와 회전을 적용하여 인스턴스화
    /// </summary>
    /// <param name="prefabId">인스턴스화할 프리팹 객체를 식별하는 고유 문자열 ID</param>
    /// <param name="position">게임 오브젝트의 최종 위치 벡터</param>
    /// <param name="rotation">게임 오브젝트의 최종 회전 쿼터니언</param>
    /// <returns>생성된 GameObject 인스턴스 또는 풀에서 객체를 찾지 못한 경우 null</returns>
    public GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation)
    {
        if (!_pools.ContainsKey(prefabId))
        {
            Debug.LogError($"{prefabId}을 Pool에서 찾지 못했습니다.");
            return null;
        }

        var obj = _pools[prefabId].Get();
        obj.transform.position = position;
        obj.transform.rotation = rotation;

        //# PhotonView의 ViewID를 동적으로 할당하거나 확인
        var photonView = obj.GetComponent<PhotonView>();

        if (photonView != null)
        {
            //# ViewID가 0이거나 유효하지 않은 경우 새로 할당
            if (photonView.ViewID == 0)
            {
                PhotonNetwork.AllocateViewID(photonView);
            }
            else
            {
                //# 기존 ViewID가 유효한지 확인
                var existingView = PhotonNetwork.GetPhotonView(photonView.ViewID);

                if (existingView == null || existingView.gameObject != obj)
                {
                    PhotonNetwork.AllocateViewID(photonView);
                }
            }
        }

        return obj;
    }

    /// <summary>
    /// Object Pool 반환(Release)를 위한 메서드
    /// </summary>
    /// <param name="obj">반환할 GameObject(일치하는 ID가 없으면 제거)</param>
    public void Destroy(GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogWarning("Null GameObject가 전달되었습니다.");
            return;
        }

        if (!obj.activeSelf) return;

        //# (Clone) 접미사 제거
        string prefabId = obj.name.Replace("(Clone)", "").Trim();
        if (_pools.ContainsKey(prefabId))
        {
            _pools[prefabId].Release(obj);
        }
        else
        {
            Debug.LogWarning($"{prefabId}을 찾지 못했습니다.");
            Destroy(obj);
        }
    }
}