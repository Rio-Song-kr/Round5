using UnityEngine;

public class BarrelWeapon : BaseWeapon
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int pelletCount = 6; // 총알 개수
    [SerializeField] private float spreadAngle = 30f; // 퍼지는 각도
    [SerializeField] private float bulletSpeed = 10f; // 속도
    [SerializeField] private int ammoPerShot = 4; // 발사 시 소비 탄약 수
    private WeaponType weaponType = WeaponType.Shotgun;

    // 무기 발사
    public override void Attack(Transform firingPoint)
    {
        if (isReloading || currentAmmo < ammoPerShot) return;

        float angleStep = spreadAngle / (pelletCount - 1);
        float startAngle = -spreadAngle / 2f;
        float damage = 100f * 0.7f * 0.3f; // 탄환 데미지 21

        for (int i = 0; i < pelletCount; i++)
        {
            float angle = startAngle + angleStep * i;
            Quaternion spreadRotation = firingPoint.rotation * Quaternion.Euler(0, 0, angle);
            Vector3 spawnPos = firingPoint.position + firingPoint.right * 0.2f;

            GameObject bullet = Instantiate(bulletPrefab, spawnPos, spreadRotation);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            rb.AddForce(spreadRotation * Vector2.right * bulletSpeed, ForceMode2D.Impulse);

            bullet.GetComponent<ResetBullet>().damage = damage;
        }

        currentAmmo -= ammoPerShot;
        lastAttackTime = Time.time;
        UpdateAmmoUI();

        if (currentAmmo < ammoPerShot)
            StartAutoReload();
    }

    public override WeaponType GetWeaponType()
    {
        return weaponType;
    }
}