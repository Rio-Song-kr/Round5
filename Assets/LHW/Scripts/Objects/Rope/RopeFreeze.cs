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

    private void OnEnable()
    {
        TestIngameManager.OnRoundOver += FreezeRope;
    }

    private void OnDisable()
    {
        TestIngameManager.OnRoundOver -= FreezeRope;
    }

    private void FreezeRope()
    {
        freezeCoroutine = StartCoroutine(FreezeCoroutine());
    }

    IEnumerator FreezeCoroutine()
    {
        rigid.bodyType = RigidbodyType2D.Kinematic;
        Debug.Log("¾óÀ½");
        yield return new WaitForSeconds(freezeTime);
        Debug.Log("¶¯");
        rigid.bodyType = RigidbodyType2D.Dynamic;
        rigid.mass = 5;
        rigid.drag = 0.2f;
        rigid.angularDrag = 0.1f;
        rigid.gravityScale = 1f;
        freezeCoroutine = null;
    }
}
