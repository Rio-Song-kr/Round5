using DG.Tweening;
using Photon.Pun;
using System.Collections;
using UnityEngine;

/// <summary>
/// ������ ���� 3���� ���� �� ���� ���� �� �̵��ϰ� �ϴ� ��ũ��Ʈ
/// </summary>
public class MapController : MonoBehaviour
{
    [Header("Offset")]
    [Tooltip("�� ��ȯ ���� ������")]
    [SerializeField] private float mapChangeDelay = 0.8f;

    [SerializeField] private GameObject[] rounds;
    public float MapChangeDelay => mapChangeDelay;

    private Coroutine moveCoroutine;

    private void OnEnable()
    {
        InGameManager.OnRoundEnd += GoToNextStage;
    }

    private void OnDisable()
    {
        InGameManager.OnRoundEnd -= GoToNextStage;
    }

    public void GoToNextStage()
    {
        MapShake();
        MapMove();
    }

    /// <summary>
    /// ���� ���� �� ���� �� �� ��鸲
    /// </summary>
    private void MapShake()
    {
        gameObject.transform.DOShakePosition(0.5f, 1, 10, 90);
    }

    /// <summary>
    /// ������ �ð� ���� ���� �������� ���۵�
    /// </summary>
    private void MapMove()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (InGameManager.Instance.CurrentMatch > 2) return;

        moveCoroutine = StartCoroutine(MovementCoroutine());
    }

    private IEnumerator MovementCoroutine()
    {
        var delay = new WaitForSeconds(mapChangeDelay);

        var movements = rounds[InGameManager.Instance.CurrentMatch].GetComponentsInChildren<MapDynamicMovement>();

        for (int i = 0; i < movements.Length; i++)
        {
            if (movements[i] != null)
            {
                movements[i].DynamicMove();
                yield return delay;
            }
        }

        moveCoroutine = null;

        //todo 여기에서 player 위치 초기화 및 중력 1로 설정해줘야 함
    }
}