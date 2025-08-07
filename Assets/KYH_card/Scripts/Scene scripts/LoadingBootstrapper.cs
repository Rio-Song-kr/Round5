using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingBootstrapper : MonoBehaviour
{
    [SerializeField] private GameObject loadingCanvasPrefab;

    private void Awake()
    {
        // Instance가 없을 때만 생성
        if (LoadingUIManager.Instance == null)
        {
            GameObject obj = Instantiate(loadingCanvasPrefab);
            DontDestroyOnLoad(obj);
        }
    }
}
