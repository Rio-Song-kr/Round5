using System.Collections;
using UnityEngine;

public class RopeFreeze : MonoBehaviour
{
    [SerializeField] float freezeTime = 3.5f;
    private Rigidbody2D rigid;

    Coroutine freezeCoroutine;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        rigid.bodyType = RigidbodyType2D.Kinematic;
    }

    private void OnEnable()
    {
        InGameManager.OnRoundEnd += FreezeRope;
        InGameManager.OnCardSelectEnd += FreezeRope;
    }

    private void OnDisable()
    {
        InGameManager.OnRoundEnd -= FreezeRope;
        InGameManager.OnCardSelectEnd -= FreezeRope;
    }

    private void FreezeRope()
    {
        freezeCoroutine = StartCoroutine(FreezeCoroutine());
    }

    IEnumerator FreezeCoroutine()
    {
        rigid.bodyType = RigidbodyType2D.Kinematic;

        yield return new WaitForSeconds(freezeTime);

        rigid.bodyType = RigidbodyType2D.Dynamic;
        rigid.mass = 3;
        rigid.drag = 0.2f;
        rigid.angularDrag = 0.1f;
        rigid.gravityScale = 1f;

        freezeCoroutine = null;
    }
}
