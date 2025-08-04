using DG.Tweening;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class WinnerImageUI : MonoBehaviourPun
{
    [SerializeField] Transform initTransform;

    private Image winnerBackgroundImage;

    private void Awake()
    {
        winnerBackgroundImage = GetComponent<Image>();
    }

    [PunRPC]
    public void WinnerImageInit()
    {
        winnerBackgroundImage.rectTransform.localScale = Vector3.one;
        winnerBackgroundImage.rectTransform.position = initTransform.position;
    }

    [PunRPC]
    public void WinnerImageScaleChange(float scale, float duration, float delay)
    {
        winnerBackgroundImage.transform.DOScale(new Vector3(scale, scale, scale), duration).SetDelay(delay);
    }

    [PunRPC]
    public void WinnerImageMove(Vector3 targetPosition, float duration, float delay)
    {
        winnerBackgroundImage.rectTransform.DOMove(targetPosition, duration).SetDelay(delay);
    }
}