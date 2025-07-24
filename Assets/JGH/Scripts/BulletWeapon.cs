using UnityEngine;

public class BulletWeapon : BaseWeapon
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private WeaponType weaponType = WeaponType.Bullet;

    public override void Attack(Transform firingPoint)
    {
        if (isReloading || currentAmmo <= 0) return;

        GameObject bullet = Instantiate(bulletPrefab, firingPoint.position, firingPoint.rotation * Quaternion.Euler(0, 0, 90f));
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.AddForce(firingPoint.right * bulletSpeed, ForceMode2D.Impulse);
        bullet.GetComponent<ResetBullet>().damage = 100f * 0.7f; // 권총 데미지 70

        currentAmmo--;
        lastAttackTime = Time.time;
        UpdateAmmoUI();

        if (currentAmmo <= 0)
            StartAutoReload();
    }

    public override WeaponType GetWeaponType()
    {
        return weaponType;
    }
}