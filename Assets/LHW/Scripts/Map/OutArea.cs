using UnityEngine;

public class OutArea : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // TODO : 플레이어 데미지
        }
    }
}