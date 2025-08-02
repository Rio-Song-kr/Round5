using DG.Tweening;
using UnityEngine;

/// <summary>
/// 게임 종료 후 맵을 이동시킬 때, 그 움직임을 표현하는 스크립트
/// </summary>
public class MapDynamicMovement : MonoBehaviour
{
    private MapController mapController;
    private RandomMapPresetCreator randomMapPresetCreator;

    [SerializeField] GameObject[] mapComponents;

    // 첫 번째 플랫폼이 움직이기 시작하는 시점(딜레이)
    [SerializeField] float moveDelay = 1f;
    // 각 플랫폼이 이동하기 시작하는 간격
    [SerializeField] float moveDurationOffset = 0.2f;

    private void OnEnable()
    {
        mapController = GetComponentInParent<MapController>();
        randomMapPresetCreator = GetComponentInParent<RandomMapPresetCreator>();
    }

    public void DynamicMove()
    {
        for(int i = 0; i < mapComponents.Length; i++)
        {
            float duration = moveDelay + (i * moveDurationOffset);
            mapComponents[i].transform.DOMove(mapComponents[i].transform.position + new Vector3(-randomMapPresetCreator.MapTransformOffset, 0, 0), duration)
                .SetDelay(mapController.MapChangeDelay).SetEase(Ease.InOutCirc);
        }
    }
}