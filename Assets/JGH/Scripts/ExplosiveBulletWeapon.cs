using System.Collections;
using UnityEngine;

public class ExplosiveBulletWeapon : MonoBehaviour, IWeapon
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private WeaponType weaponType = WeaponType.ExplosiveBullet;

    [SerializeField] private int maxAmmo = 6;
    [SerializeField] private float reloadTime = 3.25f;

    private int currentAmmo;
    private bool isReloading;
    private Coroutine autoReloadCoroutine;

    private float attackCooldown = 1f;
    private float lastAttack = 0f;

    private AmmoDisplay ammoDisplay;

    private void Start()
    {
        ammoDisplay = FindObjectOfType<AmmoDisplay>();
        ammoDisplay.UpdateAmmoIcons(currentAmmo, maxAmmo);
    }

    private void OnEnable()
    {
        currentAmmo = maxAmmo;
        isReloading = false;
        lastAttack = 0f;
        UpdateAmmoUI();
        
        // 리로드 UI 확실히 꺼줌
        ammoDisplay?.SetReloading(false);
    }

    public void Attack(Transform firingPoint)
    {
        if (isReloading || currentAmmo <= 0 || Time.time - lastAttack < attackCooldown)
            return;

        Quaternion bulletRotation = firingPoint.rotation * Quaternion.Euler(0, 0, 90f);
        GameObject bullet = Instantiate(bulletPrefab, firingPoint.position, bulletRotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.AddForce(firingPoint.right * bulletSpeed, ForceMode2D.Impulse);
        bullet.GetComponent<ResetBullet>().damage = 100f;

        currentAmmo--;
        lastAttack = Time.time;
        UpdateAmmoUI();

        if (currentAmmo == 0)
            StartAutoReload();
    }

    private void StartAutoReload()
    {
        if (autoReloadCoroutine != null)
            StopCoroutine(autoReloadCoroutine);

        autoReloadCoroutine = StartCoroutine(AutoReloadRoutine());
    }

    private IEnumerator AutoReloadRoutine()
    {
        isReloading = true;
        ammoDisplay?.SetReloading(true);

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        isReloading = false;
        UpdateAmmoUI();

        ammoDisplay?.SetReloading(false);
    }

    private void UpdateAmmoUI()
    {
        if (ammoDisplay != null)
        {
            ammoDisplay.UpdateAmmoIcons(currentAmmo, maxAmmo);
            // currentAmmo == 0일 때만 true, 그 외에는 무조건 false
            bool shouldReload = currentAmmo == 0 && !isReloading;
            ammoDisplay.SetReloading(shouldReload);
        }
    }

    public WeaponType GetWeaponType() => weaponType;

    public void Initialize()
    {
        currentAmmo = maxAmmo;
        isReloading = false;
        UpdateAmmoUI();
    }
}
