using Photon.Pun;

public class AttackSkillManager : MonoBehaviourPun
{
    private BarrelWeapon barrelWeapon;
    private GunControll gunController;
    private BaseWeapon baseWeapon;

    private BarrageStable _barrageStable;
    
    private void Start()
    {
        gunController = GetComponentInParent<GunControll>();
        baseWeapon = GetComponent<BaseWeapon>();
        barrelWeapon = GetComponent<BarrelWeapon>();
    }

    /// <summary>
    /// 큰 탄알 조작 함수
    /// </summary>
    /// <param name="value"></param>
    public void SetIsBulletBig(bool value)
    {
        gunController._isBigBullet = value;
    }
    
    /// <summary>
    /// 큰 탄알 여부 함수
    /// </summary>
    /// <returns></returns>
    public bool GetIsBulletBig()
    {
        return gunController._isBigBullet;
    }
    
    
    /// <summary>
    /// 폭발성 탄알 조작 함수
    /// </summary>
    /// <param name="value"></param>
    public void SetIsBulletExplosive(bool value)
    {
        gunController._isExplosiveBullet = value;
    }
    
    /// <summary>
    /// 폭발성 탄알 여부 함수
    /// </summary>
    /// <param name="value"></param>
    public bool GetIsBulletExplosive()
    {
        return gunController._isExplosiveBullet;
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
        barrelWeapon.useAmmo= value;
    }
    
    /// <summary>
    /// 샷건 탄창 갯수 반환 함수
    /// </summary>
    /// <returns></returns>
    public int GetBarrelWeapon()
    {
        return barrelWeapon.useAmmo;
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
    
    /// <summary>
    /// 공격속도 조작 함수
    /// </summary>
    /// <param name="value"></param>
    public void SetAttackSpeedTime(int value)
    {
        baseWeapon.attackSpeed = value;
    }
    
    /// <summary>
    /// 공격속도 반환 함수
    /// </summary>
    /// <returns></returns>
    public float GetAttackSpeedTime()
    {
        return baseWeapon.attackSpeed;
    }
}