using DG.Tweening;
using System.Collections;
using UnityEngine;

public class CardSceneCharacterBodyMovement : MonoBehaviour
{
    [SerializeField] float moveInterval = 0.2f;
    [SerializeField] float moveDistance = 1f;
    [SerializeField] float moveDuration = 0.5f;

    Coroutine moveCoroutine;

    private bool movedUp = true;

    private void OnEnable()
    {
        moveCoroutine = StartCoroutine(CharacterMove());
    }

    private void OnDisable()
    {
        StopCoroutine(moveCoroutine);
    }

    IEnumerator CharacterMove()
    {
        WaitForSeconds interval = new WaitForSeconds(moveInterval);
        while (true)
        {
            if (movedUp)
            {
                gameObject.transform.DOMove(transform.position + Vector3.up * moveDistance, moveDuration).SetEase(Ease.InOutBack);
            }
            else
            {
                gameObject.transform.DOMove(transform.position + Vector3.down * moveDistance, moveDuration).SetEase(Ease.InOutBack);
            }
            movedUp = !movedUp;
            yield return interval;
        }
    }
}
