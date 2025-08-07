using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundActivation : MonoBehaviourPun
{
    [PunRPC]
    public void RoundActivate(bool activation)
    {
        gameObject.SetActive(activation);
    }
}
