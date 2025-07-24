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

        currentAmmo -= 2;
        
        StartCoroutine(FireLaserRoutine(firingPoint));
    }

    private IEnumerator FireLaserRoutine(Transform firingPoint)
    {
        isFiring = true;
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

        if (currentAmmo == 0)
        {
            UpdateAmmoUI();
        }

        // 발사 종료 처리 및 재장전
        laserRenderer.enabled = false;
        ammoDisplay.reloadIndicator.SetActive(true);
        isFiring = false;
        isReloading = true;

        yield return new WaitForSeconds(2f); // 재장전 시간

        currentAmmo = maxAmmo;
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