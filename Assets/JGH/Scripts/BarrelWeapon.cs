using System;
using System.Collections;
using UnityEngine;

// 샷건 무기 클래스
public class BarrelWeapon : MonoBehaviour, IWeapon
{
    [SerializeField] private GameObject bulletPrefab; // 발사할 총알 프리팹
    [SerializeField] private int pelletCount = 5; // 발사할 총알 개수
    [SerializeField] private float spreadAngle = 30f; // 총알 퍼짐 각도
    [SerializeField] private float bulletSpeed = 10f; // 총알 속도
    [SerializeField] private WeaponType weaponType = WeaponType.Shotgun; // 무기 타입
    
    [SerializeField] private int maxAmmo = 2;             // 샷건 장탄 수 (한 번에 2발 정도로 제한)
    [SerializeField] private float reloadTime = 2f;       // 재장전 시간

    private int currentAmmo;
    private bool isReloading;

    private void OnEnable()
    {
        currentAmmo = maxAmmo;
        isReloading = false;
    }


    public void Attack(Transform firingPoint)
    {
        if (isReloading)
        {
            Debug.Log("재장전 중입니다. 발사할 수 없습니다.");
            return;
        }

        if (currentAmmo <= 0)
        {
            Debug.Log("탄약 없음! 재장전 시작.");
            StartCoroutine(Reload());
            return;
        }

        Debug.Log($"샷건 발사! 남은 탄약: {currentAmmo - 1}/{maxAmmo}");
        Debug.Log($"탄환 총 수: {pelletCount}");

        float angleStep = (spreadAngle / (pelletCount - 1));
        float startAngle = -spreadAngle / 2f;

        for (int i = 0; i < pelletCount; i++)
        {
            float angle = startAngle + angleStep * i;
            Debug.Log($"탄환 {i + 1}의 각도: {angle}");

            Quaternion rotation = firingPoint.rotation * Quaternion.Euler(0, 0, angle);
            GameObject bullet = Instantiate(bulletPrefab, firingPoint.position, rotation);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            rb.velocity = bullet.transform.up * bulletSpeed;
        }

        currentAmmo--;
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