using UnityEngine;

public interface IWeapon
{
    /// <summary>
    /// 격발 함수
    /// </summary>
    /// <param name="firingPoint"></param>
    void Attack(Transform firingPoint);
    
    // /// <summary>
    // /// 무기 타입 반환
    // /// </summary>
    // /// <returns></returns>
    // WeaponType GetWeaponType();
    
    /// <summary>
    /// 무기가 활성화될 때 호출됨(초기화) (무기 교체 포함)
    /// </summary>
    void Initialize(); // 초기화 공통 인터페이스
}