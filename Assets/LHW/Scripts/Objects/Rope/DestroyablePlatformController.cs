using UnityEngine;

public class DestroyablePlatformController : MonoBehaviour
{
    [SerializeField] RopeController rope;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            gameObject.SetActive(false);
            rope?.RopeDestroy();
        }
    }
}