using UnityEngine;

public interface IWeapon
{
    void Attack(Transform firingPoint);
    
    WeaponType GetWeaponType();
    
    // 추가
    void Initialize(); // 초기화 공통 인터페이스
}