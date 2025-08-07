using DG.Tweening;
using Photon.Pun;
using System.Collections;
using UnityEngine;

public class SceneChangePanelController : MonoBehaviourPun
{
    [SerializeField] private Transform roundOverTransform;
    [SerializeField] private Transform sceneChangeInitTransform;

    [PunRPC]
    public void RoundChange()
    {
        SceneChange();
    }

    private void SceneChange()
    {
        transform.DOMove(roundOverTransform.position, 1f).SetDelay(1f);
    }

    private void OnDisable()
    {
        transform.position = sceneChangeInitTransform.position;
    }
}
