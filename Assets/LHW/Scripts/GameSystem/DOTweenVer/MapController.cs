using UnityEngine;
using DG.Tweening;
using System.Collections;

public class MapController : MonoBehaviour
{
    [Header("Offset")]
    [Tooltip("∏  ¿¸»Ø Ω√¿€ µÙ∑π¿Ã")]
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

    private void MapShake()
    {
        gameObject.transform.DOShakePosition(0.5f, 1, 10, 90);
    }

    private void MapMove()
    {
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