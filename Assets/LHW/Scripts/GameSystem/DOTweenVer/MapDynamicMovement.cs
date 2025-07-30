using DG.Tweening;
using UnityEngine;

public class MapDynamicMovement : MonoBehaviour
{
    [SerializeField] GameObject[] mapComponents;

    public void DynamicMove()
    {
        for(int i = 0; i < mapComponents.Length; i++)
        {
            float duration = 1f + (i * 0.2f);
            mapComponents[i].transform.DOMove(mapComponents[i].transform.position + new Vector3(-35, 0, 0), duration).SetDelay(0.8f).SetEase(Ease.InOutCirc);
            Debug.Log(duration);
        }
    }
}