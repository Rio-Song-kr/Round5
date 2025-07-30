using UnityEngine;
using DG.Tweening;

public class MapController : MonoBehaviour
{
    public void Start()
    {
       GoToNextStage();
    }

    public void GoToNextStage()
    {
        MapShake();
        MapMove();
    }

    private void MapShake()
    {
        gameObject.transform.DOShakePosition(0.5f, 1, 10, 90).SetDelay(1f);
    }

    private void MapMove()
    {

    }
}
