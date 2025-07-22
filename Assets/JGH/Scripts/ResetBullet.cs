using UnityEngine;

public class ResetBullet : MonoBehaviour
{
    [SerializeField] private float lifeTime = 4f;

    private void Start()
    {
        // 일정 시간 뒤 총알 자동 삭제
        Destroy(gameObject, lifeTime); 
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 충돌 즉시 제거
        Destroy(gameObject);
    }
}