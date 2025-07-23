using System;
using System.Collections;
using UnityEngine;

// 샷건 무기 클래스
public class BarrelWeapon : MonoBehaviour, IWeapon
{
    [SerializeField] private GameObject bulletPrefab; // 발사할 총알 프리팹
    [SerializeField] private int pelletCount = 6; // 발사할 총알 개수
    [SerializeField] private float spreadAngle = 30f; // 총알 퍼짐 각도
    [SerializeField] private float bulletSpeed = 10f; // 총알 속도
    [SerializeField] private WeaponType weaponType = WeaponType.Shotgun; // 무기 타입
    
    [SerializeField] private int maxAmmo = 8;             // 샷건 최대 장탄 수 
    [SerializeField] private float reloadTime = 2.25f;       // 재장전 시간
    [SerializeField] private int ammoPerShot= 4;       // 소비 탄수

    // 현재 남은 탄 수
    private int currentAmmo;
    // 재장전 중인지 여부
    private bool isReloading;

    /// <summary>
    /// 무기가 활성화될 때 호출됨 (무기 교체 포함)
    /// </summary>
    private void OnEnable()
    {
        currentAmmo = maxAmmo;
        isReloading = false;
    }
    


    /// <summary>
    /// 격발 함수
    /// </summary>
    /// <param name="firingPoint"></param>
    public void Attack(Transform firingPoint)
    {
        // 재장전 중일때 
        if (isReloading)
        {
            return;
        }

        // 탄약이 격발 갯수보다 작을경우
        // TODO: 
        if (currentAmmo < ammoPerShot)
        {
            StartCoroutine(Reload());
            return;
        }

        // 각도 계산
        float angleStep = (spreadAngle / (pelletCount - 1));
        float startAngle = -spreadAngle / 2f;
        
        // 데미지 계산
        float baseDamage = 100f;
        float pistolDamage = baseDamage * 0.7f;        // 권총 데미지: 70
        float shotgunPelletDamage = pistolDamage * 0.3f; // 샷건 1발 데미지: 21


        for (int i = 0; i < pelletCount; i++)
        {
            float angle = startAngle + angleStep * i;

            Quaternion rotation = firingPoint.rotation * Quaternion.Euler(0, 0, angle);
            Vector3 spawnPos = firingPoint.position + firingPoint.right * 0.2f;

            GameObject bullet = Instantiate(bulletPrefab, spawnPos, rotation);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            rb.velocity = bullet.transform.right * bulletSpeed;
            
            // 탄환 데미지 
            bullet.GetComponent<ResetBullet>().damage = shotgunPelletDamage;
        }

        currentAmmo -= ammoPerShot;
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

    /// <summary>
    /// 무기 초기화 함수
    /// </summary>
    public void Initialize()
    {
        currentAmmo = maxAmmo;
        isReloading = false;
    }
    
    /// <summary>
    /// 무기 타입 반환
    /// </summary>
    /// <returns></returns>
    public WeaponType GetWeaponType()
    {
        return weaponType;
    }
}