using UnityEngine;
using DG.Tweening;
using System.Collections;

/// <summary>
/// 생성된 랜덤 3개의 맵이 한 게임 종료 후 이동하게 하는 스크립트
/// </summary>
public class MapController : MonoBehaviour
{
    [Header("Offset")]
    [Tooltip("맵 전환 시작 딜레이")]
    [SerializeField] private float mapChangeDelay = 0.8f;
    public float MapChangeDelay { get { return mapChangeDelay; } }

    private Coroutine moveCoroutine;

    private void OnEnable()
    {
        TestIngameManager.OnRoundOver += GoToNextStage;
    }

    private void OnDisable()
    {
        TestIngameManager.OnRoundOver -= GoToNextStage;
    }

    public void GoToNextStage()
    {
        MapShake();
        MapMove();
    }

    /// <summary>
    /// 게임 종료 후 맵이 한 번 흔들림
    /// </summary>
    private void MapShake()
    {
        gameObject.transform.DOShakePosition(0.5f, 1, 10, 90);
    }

    /// <summary>
    /// 딜레이 시간 이후 맵의 움직임이 시작됨
    /// </summary>
    private void MapMove()
    {
        Debug.Log("실행");
        moveCoroutine = StartCoroutine(MovementCoroutine());
    }

    IEnumerator MovementCoroutine()
    {
        WaitForSeconds delay = new WaitForSeconds(mapChangeDelay);

        MapDynamicMovement[] movements = GetComponentsInChildren<MapDynamicMovement>();
        for (int i = 0; i < movements.Length; i++)
        {
            if (movements[i] != null)
            {
                movements[i].DynamicMove();
                yield return delay;
            }
        }
        moveCoroutine = null;
    }
}