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
    private float lastAttackTime;
    private Coroutine autoReloadCoroutine;
    private Coroutine idleCheckCoroutine;
    private float idleReloadDelay = 3f;

    private float attackCooldown = 1f;

    private AmmoDisplay ammoDisplay;

    private void Start()
    {
        ammoDisplay = FindObjectOfType<AmmoDisplay>();
        currentAmmo = maxAmmo;
        isReloading = false;
        UpdateAmmoUI();
        ammoDisplay?.SetReloading(false);
    }

    private void OnEnable()
    {
        currentAmmo = maxAmmo;
        isReloading = false;
        UpdateAmmoUI();
        ammoDisplay?.SetReloading(false);
        StartIdleCheck();
    }

    private void OnDisable()
    {
        if (idleCheckCoroutine != null)
            StopCoroutine(idleCheckCoroutine);
    }

    public void Attack(Transform firingPoint)
    {
        if (isReloading || currentAmmo <= 0 || Time.time - lastAttackTime < attackCooldown)
            return;

        var bullet = Instantiate(bulletPrefab, firingPoint.position, firingPoint.rotation * Quaternion.Euler(0, 0, 90f));
        var rb = bullet.GetComponent<Rigidbody2D>();
        rb.AddForce(firingPoint.right * bulletSpeed, ForceMode2D.Impulse);
        bullet.GetComponent<ResetBullet>().damage = 100f;

        currentAmmo--;
        lastAttackTime = Time.time;
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
        lastAttackTime = Time.time;
    }

    private void StartIdleCheck()
    {
        if (idleCheckCoroutine != null)
            StopCoroutine(idleCheckCoroutine);

        idleCheckCoroutine = StartCoroutine(IdleCheckRoutine());
    }

    private IEnumerator IdleCheckRoutine()
    {
        lastAttackTime = Time.time;
        while (true)
        {
            yield return new WaitForSeconds(0.1f);

            if (!isReloading && currentAmmo < maxAmmo)
            {
                if (Time.time - lastAttackTime >= idleReloadDelay)
                {
                    currentAmmo = maxAmmo;
                    isReloading = false;
                    ammoDisplay?.UpdateAmmoIcons(currentAmmo, maxAmmo);
                    lastAttackTime = Time.time;
                }
            }
        }
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
        StartIdleCheck();
    }
}
