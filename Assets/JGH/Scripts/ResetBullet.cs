using UnityEngine;

public class ResetBullet : MonoBehaviour
{
    [SerializeField] private float lifeTime = 4f;
    private Rigidbody2D rb;

    private void Start()
    {
        // 일정 시간 뒤 총알 자동 삭제
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifeTime); 
    }
    
    private void Update()
    {
        // 이동 방향이 있다면 총알 회전
        if (rb != null && rb.velocity.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle + 90f);  // +90도 보정
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 충돌 즉시 제거
        if (!other.CompareTag("Bullet"))  // Bullet끼리 충돌 무시
        {
            Destroy(gameObject);
        }
    }
}