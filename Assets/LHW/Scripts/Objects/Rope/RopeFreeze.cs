using System.Collections;
using UnityEngine;

public class RopeFreeze : MonoBehaviour
{
    [SerializeField] float freezeTime = 4f;
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
        InGameManager.OnGameStart += FreezeRope;
        InGameManager.OnRoundEnd += FreezeRope;
        InGameManager.OnRoundStart += FreezeRope;
        InGameManager.OnCardSelectEnd += FreezeRope;
    }

    private void OnDisable()
    {
        InGameManager.OnGameStart -= FreezeRope;
        InGameManager.OnRoundEnd -= FreezeRope;
        InGameManager.OnRoundStart -= FreezeRope;
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
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = 0;

        freezeCoroutine = null;
    }
}
