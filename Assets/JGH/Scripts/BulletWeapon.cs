using UnityEngine;

public class BulletWeapon : BaseWeapon
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private WeaponType weaponType = WeaponType.Bullet;

    public override void Attack(Transform firingPoint)
    {
        if (isReloading || currentAmmo <= 0) return;

        Instantiate(bulletPrefab, firingPoint.position, firingPoint.rotation * Quaternion.Euler(0, 0, -90f));

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