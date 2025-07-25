using UnityEngine;
using System.Collections;

public class RazorWeapon : BaseWeapon
{
    [SerializeField] private LineRenderer laserRenderer;
    [SerializeField] private float laserDuration = 2f;
    [SerializeField] private float laserLength = 20f;
    
    private bool isFiring = false;
    private WeaponType weaponType = WeaponType.Laser;
    
    public override void Attack(Transform firingPoint)
    {
        if (isFiring || isReloading || currentAmmo < 2) return;
        
        StopAllCoroutines(); // 이전 발사나 리로드 코루틴 종료
        currentAmmo -= 2;

        UpdateAmmoUI();
        ammoDisplay.reloadIndicator.SetActive(false); 
        
        StartCoroutine(FireLaserRoutine(firingPoint));
    }

    private IEnumerator FireLaserRoutine(Transform firingPoint)
    {
        isFiring = true;
        isReloading = false;
        laserRenderer.enabled = true;

        float elapsed = 0f;
        float damageTick = 0.1f;
        float damageTimer = 0f;
        float damage = 100f * 0.7f * 0.3f; // 틱 데미지 21

        while (elapsed < laserDuration)
        {
            Vector3 startPoint = firingPoint.position;
            Vector3 direction = firingPoint.right;
            RaycastHit2D hit = Physics2D.Raycast(startPoint, direction, laserLength);
            Vector3 endPoint = hit.collider ? (Vector3)hit.point : startPoint + direction * laserLength;

            laserRenderer.SetPosition(0, startPoint);
            laserRenderer.SetPosition(1, endPoint);

            damageTimer += Time.deltaTime;
            if (damageTimer >= damageTick && hit.collider != null)
            {
                damageTimer = 0f;
                if (hit.collider.TryGetComponent<IDamageable>(out var target))
                    target.TakeDamage(damage);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }


        // 레이저 종료
        laserRenderer.enabled = false;
        // 리로드 시작 + UI 표시
        isFiring = false;
        
        // 리로드 UI 이제 나타남
        isReloading = true;

        ammoDisplay.reloadIndicator.SetActive(true);
        
        yield return new WaitForSeconds(reloadTime); // 재장전 시간
        
        //탄 UI 회복, 리로드 UI OFF
        currentAmmo = maxAmmo;
        UpdateAmmoUI(); // 탄창 갱신
        isReloading = false;
        ammoDisplay.reloadIndicator.SetActive(false);
    }

    public override WeaponType GetWeaponType()
    {
        return weaponType;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        laserRenderer.enabled = false;
        isFiring = false;
        isReloading = false;
    }
}