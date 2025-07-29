using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameEndManager : MonoBehaviourPunCallbacks
{
    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            // SceneLoadingManager.Instance.LoadSceneAsync("CardTest");
            // SceneLoadingManager.Instance.AllowSceneActivation();

            PhotonNetwork.LoadLevel("Game Scene");
        }
    }
}
