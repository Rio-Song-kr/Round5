using UnityEngine;

// 샷건 무기 클래스
public class ShotgunWeapon : MonoBehaviour, IWeapon
{
    [SerializeField] private GameObject bulletPrefab; // 발사할 총알 프리팹
    [SerializeField] private int pelletCount = 5; // 발사할 총알 개수
    [SerializeField] private float spreadAngle = 30f; // 총알 퍼짐 각도
    [SerializeField] private float bulletSpeed = 10f; // 총알 속도
    [SerializeField] private WeaponType weaponType = WeaponType.Shotgun; // 무기 타입

    public void Attack(Transform firingPoint)
    {
        // 샷건 탄환 각도를 균등하게 배치
        float angleStep = (spreadAngle / (pelletCount - 1)); // 각 탄환 간격
        float startAngle = -spreadAngle / 2; // 첫 탄환 시작 각도
        
        // 여러 발의 총알을 퍼뜨리며 발사
        for (int i = 0; i < pelletCount; i++)
        {
            float angle = startAngle + (angleStep * i);
            Quaternion rotation = firingPoint.rotation * Quaternion.Euler(0, 0, angle);

            GameObject bullet = Instantiate(bulletPrefab, firingPoint.position, rotation);

            // 총알 발사
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            rb.velocity = bullet.transform.up * bulletSpeed;
        }
    }

    public WeaponType GetWeaponType()
    {
        return weaponType;
    }
}