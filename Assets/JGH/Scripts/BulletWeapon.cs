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
        Debug.Log($"현재 남은 탄약: {currentAmmo}/{maxAmmo}");
        
        // 재장전
        if (isReloading)
        {
            Debug.Log("재장전 중입니다. 발사 불가.");
            return;
        }
        
       if (currentAmmo <= 0)
        {
            Debug.Log("탄약이 없습니다. 재장전 시작.");
            StartCoroutine(Reload());
            return;
        }

        Debug.Log($"총알 발사! 남은 탄약: {currentAmmo - 1}/{maxAmmo}");

        // 총알 생성
        GameObject bullet = Instantiate(bulletPrefab, firingPoint.position, firingPoint.rotation);

        // 총알 발사
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.velocity = firingPoint.up * bulletSpeed;

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
