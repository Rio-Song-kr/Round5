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
    [SerializeField] private float reloadTime = 2f; // 재장전 시간 (초)

    private int currentAmmo;     // 현재 남은 탄 수
    private bool isReloading;    // 재장전 중인지 여부

    // 무기가 활성화될 때 호출됨 (무기 교체 포함)
    private void OnEnable()
    {
        currentAmmo = maxAmmo;   // 탄약을 가득 채움
        isReloading = false;     // 재장전 상태 초기화
    }
    
    public void Attack(Transform firingPoint)
    {
        // 재장전
        if (isReloading)
        {
            return;
        }
        
       if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        Quaternion bulletRotation = firingPoint.rotation * Quaternion.Euler(0, 0, 90f);
        GameObject bullet = Instantiate(bulletPrefab, firingPoint.position, bulletRotation);

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.velocity = firingPoint.right * bulletSpeed;
        
        // 데미지 설정 :: S
        // 데미지 70% 적용
        float baseDamage = 100f;
        bullet.GetComponent<ResetBullet>().damage = baseDamage * 0.7f;
        // 데미지 설정 :: E
        
        --currentAmmo;
    }
       private IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("재장전 중...");

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        isReloading = false;
        Debug.Log("재장전 완료!");
    } 
    public void Initialize()
    {
        currentAmmo = maxAmmo;
        isReloading = false;
    }

    public WeaponType GetWeaponType()
    {
        return weaponType;
    }
}
