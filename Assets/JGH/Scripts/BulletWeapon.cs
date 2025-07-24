using System;
using System.Collections;
using UnityEngine;

// 기본 총알 무기 클래스
public class BulletWeapon : MonoBehaviour, IWeapon
{
    [SerializeField] private GameObject bulletPrefab; // 발사할 총알 프리팹
    [SerializeField] private float bulletSpeed = 10f; // 총알 속도
    [SerializeField] private WeaponType weaponType = WeaponType.Bullet; // 무기 타입

    [SerializeField] private int maxAmmo = 6;       // 최대 탄 수
    [SerializeField] private float reloadTime = 3f; // 재장전 시간 (초)

    private int currentAmmo;     // 현재 남은 탄 수
    private bool isReloading;    // 재장전 중인지 여부
    private float lastAttack; // 마지막 공격
    private Coroutine autoReloadCoroutine; 
    
    private AmmoDisplay ammoDisplay; // 탄약 아이콘 표시 UI

    private void Start()
    {
        // AmmoDisplay 컴포넌트 찾기
        ammoDisplay = FindObjectOfType<AmmoDisplay>();
        ammoDisplay.UpdateAmmoIcons(currentAmmo, maxAmmo);
        lastAttack = 0;
    }
    
    /// <summary>
    /// 무기가 활성화될 때 호출됨 (무기 교체 포함)
    /// </summary>
    private void OnEnable()
    {
        currentAmmo = maxAmmo;
        isReloading = false;
        UpdateAmmoUI();
        
        // 리로드 UI 확실히 꺼줌
        ammoDisplay?.SetReloading(false);
    }
    
    /// <summary>
    /// 격발 함수
    /// </summary>
    /// <param name="firingPoint"></param>
    public void Attack(Transform firingPoint)
    {
        // 재장전
        if (isReloading || currentAmmo <= 0)
        {
            return;
        }
        
        // 총알 생성 및 발사
        var bullet = Instantiate(bulletPrefab, firingPoint.position, firingPoint.rotation * Quaternion.Euler(0, 0, 90f));
        var rb = bullet.GetComponent<Rigidbody2D>();
        rb.AddForce(firingPoint.right * bulletSpeed, ForceMode2D.Impulse);
        
        // 총알의 데미지 설정
        bullet.GetComponent<ResetBullet>().damage = 100f * 0.7f;

        currentAmmo--;
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
