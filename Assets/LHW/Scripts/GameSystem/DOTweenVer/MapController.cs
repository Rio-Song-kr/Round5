using UnityEngine;
using DG.Tweening;
using System.Collections;

public class MapController : MonoBehaviour
{
    [SerializeField] float mapChangeDelay = 0.8f;

    private Coroutine moveCoroutine;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GoToNextStage();
        }
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
