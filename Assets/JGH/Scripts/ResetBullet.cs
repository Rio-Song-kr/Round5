using UnityEngine;

public class ResetBullet : MonoBehaviour
{
    // 총알이 일정 시간 후 자동으로 제거되도록 설정
    [SerializeField] private float lifeTime = 4f;
    [SerializeField] private float explosionRadius = 1f;  // 폭발 반경
    public WeaponType weaponType;
    private Rigidbody2D rb;
    
    private GunControll gunController; // 총기 컨트롤러

    private WeaponType currentWeaponType;
    
    // 총알이 충돌 시 적용할 데미지
    public float damage = 100f;

    private void Start()
    {
        // 일정 시간 뒤 총알 자동 삭제
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifeTime); 
        
        gunController = FindObjectOfType<GunControll>();
        currentWeaponType = gunController.currentWeapon.GetWeaponType();
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
            if (currentWeaponType == WeaponType.ExplosiveBullet)
                Explode();
            else
                DealSingleDamage(other);

            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 일반 탄알
    /// </summary>
    /// <param name="other"></param>
    private void DealSingleDamage(Collider2D other)
    {
        IDamageable target = other.GetComponent<IDamageable>();
        if (target != null)
        {
            target.TakeDamage(damage);
        }
    } 
    /// <summary>
    /// 폭발 처리 - 중심 및 범위 내 데미지
    /// </summary>
    private void Explode()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (var hit in hits)
        {
            IDamageable target = hit.GetComponent<IDamageable>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }
        }

        Destroy(gameObject);
    }
    
    // TODO: TEST 폭발 반경 시각화
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}