using UnityEngine;

public class DestroyablePlatformController : MonoBehaviour
{
    [SerializeField] RopeController rope;
    [SerializeField] RopeTiedObject tiedObject;

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