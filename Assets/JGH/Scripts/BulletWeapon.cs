using UnityEngine;

// 기본 총알 무기 클래스
public class BulletWeapon : MonoBehaviour, IWeapon
{
    [SerializeField] private GameObject bulletPrefab; // 발사할 총알 프리팹
    [SerializeField] private float bulletSpeed = 10f; // 총알 속도
    [SerializeField] private WeaponType weaponType = WeaponType.Bullet; // 무기 타입

    public void Attack(Transform firingPoint)
    {
        // 총알 생성
        GameObject bullet = Instantiate(bulletPrefab, firingPoint.position, firingPoint.rotation);

        // 총알 발사
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.velocity = firingPoint.up * bulletSpeed;
    }

    public WeaponType GetWeaponType()
    {
        return weaponType;
    }
}
