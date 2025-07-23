using UnityEngine;

public class ResetBullet : MonoBehaviour
{
    // 총알이 일정 시간 후 자동으로 제거되도록 설정
    [SerializeField] private float lifeTime = 4f;
    
    private Rigidbody2D rb;
    
    // 총알이 충돌 시 적용할 데미지
    public float damage = 100f;

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
            // 총알이 가로로 발사 되는 문제
            float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle + 90f);  // +90도 보정
        }
    }

    /// <summary>
    /// 충돌 시 처리
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 충돌 즉시 제거
        if (!other.CompareTag("Bullet"))  // Bullet끼리 충돌 무시
        {
            // IDamageable이 있다면 데미지 적용
            IDamageable target = other.GetComponent<IDamageable>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
    }
}