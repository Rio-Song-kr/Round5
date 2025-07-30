using DG.Tweening;
using UnityEngine;

public class MapDynamicMovement : MonoBehaviour
{
    private MapController mapController;
    private RandomMapPresetCreator randomMapPresetCreator;

    [SerializeField] GameObject[] mapComponents;

    [SerializeField] float moveDurationOffset = 0.2f;

    private void Start()
    {
        mapController = GetComponentInParent<MapController>();
        randomMapPresetCreator = GetComponentInParent<RandomMapPresetCreator>();
    }

    public void DynamicMove()
    {
        for(int i = 0; i < mapComponents.Length; i++)
        {
            float duration = 1f + (i * moveDurationOffset);
            mapComponents[i].transform.DOMove(mapComponents[i].transform.position + new Vector3(-randomMapPresetCreator.MapTransformOffset, 0, 0), duration)
                .SetDelay(mapController.MapChangeDelay).SetEase(Ease.InOutCirc);
        }
    }
}