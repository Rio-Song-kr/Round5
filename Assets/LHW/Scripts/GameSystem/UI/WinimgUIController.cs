using DG.Tweening;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinimgUIController : MonoBehaviourPun
{
    [PunRPC]
    public void WinImgUIActivate(bool activation)
    {
        gameObject.SetActive(activation);
    }

    [PunRPC]
    public void RoundWinImgAnimationActivate(float delay)
    {
        transform.DOScale(new Vector3(2.5f, 2.5f, 2.5f), 0.1f).SetDelay(delay);
    }
}
