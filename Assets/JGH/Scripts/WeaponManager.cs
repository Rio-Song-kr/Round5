using System;
using UnityEngine;
using UnityEngine.Serialization;

public class WeaponManager : BaseWeapon 
{
    public static WeaponManager Instance { get; private set; }
    public BarrelWeapon barrelWeapon;

    private Bullet _bullet;
    
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

    private void Start()
    {
        _bullet = FindObjectOfType<Bullet>();
    }

    public override void Attack(Transform firingPoint)
    {
    }

    public override WeaponType GetWeaponType()
    {
        return WeaponType.Manager;
    }

    /// <summary>
    /// 큰 탄알 조작 함수
    /// </summary>
    /// <param name="value"></param>
    public void SetIsBulletBig(bool value)
    {
        isBigBullet = value;
    }
    
    /// <summary>
    /// 큰 탄알 여부 함수
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool GetIsBulletBig(bool value)
    {
        return isBigBullet;
    }
    
    
    /// <summary>
    /// 폭발성 탄알 조작 함수
    /// </summary>
    /// <param name="value"></param>
    public void SetIsBulletExplosive(bool value)
    {
        isExplosiveBullet = value;
    }
    
    /// <summary>
    /// 폭발성 탄알 여부 함수
    /// </summary>
    /// <param name="value"></param>
    public bool GetIsBulletExplosive()
    {
        return isExplosiveBullet;
    }

    /// <summary>
    /// 탄창 조작 함수
    /// </summary>
    /// <param name="value"></param>
    public void SetBullet(int value)
    {
        maxAmmo = value;
        currentAmmo = value;
    }
    
    /// <summary>
    /// 현재 탄창 갯수 반환 함수
    /// </summary>
    /// <param name="value"></param>
    public void GetCurrectBullet(int value)
    {
        currentAmmo = value;
    }
    
    /// <summary>
    /// 최대 탄창 갯수 반환 함수
    /// </summary>
    /// <param name="value"></param>
    public void GetMaxBullet(int value)
    {
        maxAmmo = value;
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
        attackDamage = value;
    }
    
    /// <summary>
    /// 데미지 값 반환 함수
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public int GetAttackDamage()
    {
        return attackDamage;
    }

    /// <summary>
    /// 재장전 조작 함수
    /// </summary>
    /// <param name="value"></param>
    public void SetReloadTime(int value)
    {
        reloadTime = value;
    }
    
    /// <summary>
    /// 재장전 반환 함수
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public float GetReloadTime()
    {
        return reloadTime;
    }
    
    /// <summary>
    /// 공격속도 조작 함수
    /// </summary>
    /// <param name="value"></param>
    public void SetAttackSpeedTime(int value)
    {
        attackSpeed = value;
    }
    
    /// <summary>
    /// 공격속도 반환 함수
    /// </summary>
    /// <returns></returns>
    public float GetAttackSpeedTime()
    {
        return attackSpeed;
    }
}