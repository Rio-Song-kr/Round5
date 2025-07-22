using UnityEngine;

// 레이저 무기 클래스
public class LaserWeapon : MonoBehaviour, IWeapon
{
    [SerializeField] private GameObject laserBeamPrefab;  // 레이저 웨폰 프리팹
    [SerializeField] private float laserDuration = 2f;   // 레이저 지속 시간
    [SerializeField] private float laserLength = 20f;    // 레이저 거리
    [SerializeField] private WeaponType weaponType = WeaponType.Laser; // 무기 타입
    
    private IWeapon _weaponImplementation;
    
    public void Attack(Transform firingPoint)
    {
        
    }
    
    public WeaponType GetWeaponType()
    {
        return weaponType;
    }
}
