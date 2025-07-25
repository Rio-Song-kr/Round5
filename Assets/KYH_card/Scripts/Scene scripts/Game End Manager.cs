using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEndManager : MonoBehaviour
{
    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            SceneLoadingManager.Instance.LoadSceneAsync("CardTest");
            SceneLoadingManager.Instance.AllowSceneActivation();
        }
    }
}
