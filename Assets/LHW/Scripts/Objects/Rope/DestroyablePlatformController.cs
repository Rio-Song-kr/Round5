using UnityEngine;

public class DestroyablePlatformController : MonoBehaviour
{
    [SerializeField] RopeController rope;
    [SerializeField] RopeTiedObject tiedObject;

    /// <summary>
    /// 총알에 의해 이 오브젝트가 파괴되었을 때 로프 기믹 작동
    /// 로프에 묶인 오브젝트의 물리를 활성화하고 고정하고 있던 로프를 비활성화함
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            gameObject.SetActive(false);
            tiedObject.EnablePhysics();
            rope?.RopeDestroy();
        }
    }
}