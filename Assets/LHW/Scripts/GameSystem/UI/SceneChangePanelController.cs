using System.Collections;
using DG.Tweening;
using Photon.Pun;
using UnityEngine;

public class SceneChangePanelController : MonoBehaviourPun
{
    [SerializeField] private Transform roundOverTransform;
    [SerializeField] private Transform sceneChangeInitTransform;

    Coroutine sceneChangeCoroutine;

    [PunRPC]
    public void RoundChange()
    {
        sceneChangeCoroutine = StartCoroutine(SceneChangeCoroutine());
    }

    IEnumerator SceneChangeCoroutine()
    {
        WaitForSeconds delay = new WaitForSeconds(3f);

        transform.DOMove(roundOverTransform.position, 1f).SetDelay(1f);
        yield return delay;
        if (!TestIngameManager.Instance.IsGameOver)
        {
            transform.position = sceneChangeInitTransform.position;
        }
        sceneChangeCoroutine = null;
    }
}
