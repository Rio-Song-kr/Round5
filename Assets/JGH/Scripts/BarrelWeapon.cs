using UnityEngine;

public class BarrelWeapon : BaseWeapon
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int pelletCount = 6; // 퍼지는 총알 개수
    [SerializeField] private float spreadAngle = 30f; // 퍼지는 각도
    [SerializeField] private int ammoPerShot = 4; // 발사 시 소비 탄약 수
    private WeaponType weaponType = WeaponType.Shotgun;

    // 무기 발사
    public override void Attack(Transform firingPoint)
    {
        if (isReloading || currentAmmo < ammoPerShot) return;

        float angleStep = spreadAngle / (pelletCount - 1);
        float startAngle = -spreadAngle / 2f;

        for (int i = 0; i < pelletCount; i++)
        {
            float angle = startAngle + angleStep * i;
            
            Quaternion spreadRotation = firingPoint.rotation * Quaternion.Euler(0, 0, angle - 90); 
            Vector3 spawnPos = firingPoint.position + firingPoint.right * 0.2f;

            Instantiate(bulletPrefab, spawnPos, spreadRotation);
        }

        currentAmmo -= ammoPerShot;
        lastAttackTime = Time.time;
        UpdateAmmoUI();

        if (currentAmmo < ammoPerShot)
        {
            ReloadSpeedFromAnimator();
            StartAutoReload();
        }
    }

    public override WeaponType GetWeaponType()
    {
        return weaponType;
    }
}