using UnityEngine;

public class OutArea : MonoBehaviour
{
    /// <summary>
    /// 플레이어가 맵 바깥으로 나갔을 경우 데미지를 줌
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // TODO : 플레이어 데미지
        }
    }
}