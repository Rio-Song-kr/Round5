using UnityEngine;

// 레이저 무기 클래스
public class RazorWeapon : MonoBehaviour, IWeapon
{
    [SerializeField] private LineRenderer laserRenderer;  // 레이저를 그릴 LineRenderer
    [SerializeField] private float laserDuration = 2f;   // 레이저 지속 시간
    [SerializeField] private float laserLength = 20f;    // 레이저 거리
    [SerializeField] private WeaponType weaponType = WeaponType.Laser; // 무기 타입
    
    private bool isFiring = false;
    private bool isReloading = false;
    
    public void Attack(Transform firingPoint)
    {
        if (isFiring || isReloading) return;

        StartCoroutine(FireLaserRoutine(firingPoint));
    }

    private System.Collections.IEnumerator FireLaserRoutine(Transform firingPoint)
    {
        isFiring = true;
        laserRenderer.enabled = true;

        float elapsed = 0f;
        
        
        float damageTick = 0.1f;
        float damageTimer = 0f;
        float baseDamage = 100f;
        float shotgunDamage = baseDamage * 0.7f * 0.3f; // 21
        
        
        while (elapsed < laserDuration)
        {
            // 레이저 시작 위치와 방향 설정
            Vector3 startPoint = firingPoint.position;
            Vector3 direction = firingPoint.right;

            // Raycast로 충돌 지점 계산
            RaycastHit2D hit = Physics2D.Raycast(startPoint, direction, laserLength);

            // 레이저 끝 지점 설정
            Vector3 endPoint = hit.collider != null ? hit.point : startPoint + direction * laserLength;

            // LineRenderer로 레이저 그리기
            laserRenderer.SetPosition(0, startPoint);
            laserRenderer.SetPosition(1, endPoint);
            
            
            // 틱 데미지 처리
            damageTimer += Time.deltaTime;
            if (damageTimer >= damageTick)
            {
                damageTimer = 0f;
                if (hit.collider != null)
                {
                    IDamageable damageTarget = hit.collider.GetComponent<IDamageable>();
                    if (damageTarget != null)
                    {
                        damageTarget.TakeDamage(shotgunDamage);
                    }
                }
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 비활성화 및 재장전 시작
        laserRenderer.enabled = false;
        isFiring = false;

        isReloading = true;
        yield return new WaitForSeconds(2f); // 재장전 시간
        isReloading = false;
    }
    
    public WeaponType GetWeaponType()
    {
        return weaponType;
    }

    public void Initialize()
    {
    }
    
    private void OnDisable()
    {
        StopAllCoroutines();          // 레이저 발사 코루틴 중지
        laserRenderer.enabled = false; // 라인 렌더러 끄기
        isFiring = false;            // 상태 초기화
        isReloading = false;
    }
}
