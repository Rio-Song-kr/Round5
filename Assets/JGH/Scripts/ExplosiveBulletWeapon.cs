using UnityEngine;

public class ExplosiveBulletWeapon : BaseWeapon
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float attackCooldown = 1f; // 쿨다운
    [SerializeField] private WeaponType weaponType = WeaponType.ExplosiveBullet;

    public override void Attack(Transform firingPoint)
    {
        if (isReloading || currentAmmo <= 0 || Time.time - lastAttackTime < attackCooldown) return;

        GameObject bullet = Instantiate(bulletPrefab, firingPoint.position, firingPoint.rotation * Quaternion.Euler(0, 0, 90f));
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.AddForce(firingPoint.right * bulletSpeed, ForceMode2D.Impulse);
        bullet.GetComponent<ResetBullet>().damage = 100f;

        currentAmmo--;
        lastAttackTime = Time.time;
        UpdateAmmoUI();

        if (currentAmmo == 0)
            StartAutoReload();
    }

    public override WeaponType GetWeaponType()
    {
        return weaponType;
    }
}