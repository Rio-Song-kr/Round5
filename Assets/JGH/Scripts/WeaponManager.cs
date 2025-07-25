using UnityEngine;
using UnityEngine.Serialization;

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager Instance { get; private set; }
    public BarrelWeapon barrelWeapon;
    
    // 현재 장착된 무기
    public BaseWeapon baseWeapon;
    
    private void Awake()
    {
        // 중복 인스턴스 제거
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    
        Instance = this;
        DontDestroyOnLoad(gameObject); // 씬 전환 시에도 유지
    }

    /// <summary>
    /// 큰 탄알 조작 함수
    /// </summary>
    /// <param name="value"></param>
    public void SetIsBulletBig(bool value)
    {
        baseWeapon.isBigBullet = value;
    }
    
    /// <summary>
    /// 큰 탄알 여부 함수
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool GetIsBulletBig(bool value)
    {
        return baseWeapon.isBigBullet;
    }
    
    
    /// <summary>
    /// 폭발성 탄알 조작 함수
    /// </summary>
    /// <param name="value"></param>
    public void SetIsBulletExplosive(bool value)
    {
        baseWeapon.isExplosiveBullet = value;
    }
    
    /// <summary>
    /// 폭발성 탄알 여부 함수
    /// </summary>
    /// <param name="value"></param>
    public bool GetIsBulletExplosive()
    {
        return baseWeapon.isExplosiveBullet;
    }

    /// <summary>
    /// 탄창 조작 함수
    /// </summary>
    /// <param name="value"></param>
    public void SetBullet(int value)
    {
        baseWeapon.maxAmmo = value;
        baseWeapon.currentAmmo = value;
    }
    
    /// <summary>
    /// 현재 탄창 갯수 반환 함수
    /// </summary>
    /// <param name="value"></param>
    public void GetCurrectBullet(int value)
    {
        baseWeapon.currentAmmo = value;
    }
    
    /// <summary>
    /// 최대 탄창 갯수 반환 함수
    /// </summary>
    /// <param name="value"></param>
    public void GetMaxBullet(int value)
    {
        baseWeapon.maxAmmo = value;
    }
    
    /// <summary>
    /// 탄창 조작 함수(샷건)
    /// </summary>
    /// <param name="value"></param>
    public void SetBarrelWeapon(int value)
    {
        barrelWeapon.ammoPerShot = value;
    }
    
    /// <summary>
    /// 샷건 탄창 갯수 반환 함수
    /// </summary>
    /// <returns></returns>
    public int GetBarrelWeapon()
    {
        return barrelWeapon.ammoPerShot;
    }
    
    /// <summary>
    /// 데미지 조작 함수
    /// </summary>
    /// <param name="value"></param>
    public void SetAttackDamage(int value)
    {
        baseWeapon.attackDamage = value;
    }
    
    /// <summary>
    /// 데미지 값 반환 함수
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public int GetAttackDamage()
    {
        return baseWeapon.attackDamage;
    }

    /// <summary>
    /// 재장전 조작 함수
    /// </summary>
    /// <param name="value"></param>
    public void SetReloadTime(int value)
    {
        baseWeapon.reloadTime = value;
    }
    
    /// <summary>
    /// 재장전 반환 함수
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public float GetReloadTime()
    {
        return baseWeapon.reloadTime;
    }
    
    // TODO: 공속 추가 필요
    // /// <summary>
    // /// 과 조작 함수
    // /// </summary>
    // /// <param name="value"></param>
    // public void SetReloadTime(int value)
    // {
    //     baseWeapon.reloadTime = value;
    // }
    //
    // /// <summary>
    // /// 재장전 반환 함수
    // /// </summary>
    // /// <param name="value"></param>
    // /// <returns></returns>
    // public float GetReloadTime()
    // {
    //     return baseWeapon.reloadTime;
    // }
}