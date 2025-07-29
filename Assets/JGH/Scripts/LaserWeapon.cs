using System;
using UnityEngine;
using System.Collections;
using Photon.Pun;

public class RazorWeapon : BaseWeapon
{
    [SerializeField] private float laserDuration = 2f;
    
    private bool isFiring = false;
    private WeaponType weaponType = WeaponType.Laser;
    

    public override void Attack(Transform firingPoint)
    {
        if (!photonView.IsMine || isFiring || isReloading || currentAmmo < 2) return;
            photonView.RPC(nameof(RPC_FireLaser), RpcTarget.All, firingPoint.position, firingPoint.up);
    }


    [PunRPC]
    private void RPC_FireLaser(Vector3 origin, Vector3 direction)
    {
        StopAllCoroutines(); // 이전 발사나 리로드 코루틴 종료
        
        currentAmmo -= 2;
        UpdateAmmoUI();
        
        ammoDisplay.reloadIndicator.SetActive(false);
        
        // 위치 및 방향 지정
        laser.transform.position = origin;
        laser.transform.up = direction;
        laser.ShootLaser(); // Laser.cs의 ShootLaser 메서드를 호출하여 레이저 발사
        
        StartCoroutine(FireLaserRoutine());
    }

    // protected override void Update() 
    // {
    //     laser.transform.position = gunController.muzzle.position;
    //     laser.transform.up = gunController.muzzle.up;
    // }
    
    private IEnumerator FireLaserRoutine()
    {
        isFiring = true;
        isReloading = false;
        
        laser.Duration = laserDuration;
        
        // laser.transform.position = gunController.muzzle.position;
        // laser.transform.up = gunController.muzzle.up;
        
        // laser.ShootLaser(gunController.muzzle.position, gunController.muzzle.up);
        
        yield return new WaitForSeconds(laserDuration);
        
        isFiring = false;
        
        StartAutoReload();
        
        isReloading = true;
        
        ammoDisplay.reloadIndicator.SetActive(true);
        
        // 애니메이션 트리거 실행
        animator?.SetTrigger("Reload");
        yield return null; // 한 프레임 대기하여 클립이 로드되도록 함
        // 애니메이션 클립 기반으로 리로드 속도 설정
        ReloadSpeedFromAnimator();
        
        yield return new WaitForSeconds(reloadTime); // 재장전 시간
        
        //탄 UI 회복, 리로드 UI OFF
        currentAmmo = maxAmmo;
        UpdateAmmoUI(); // 탄창 갱신
        isReloading = false;
        ammoDisplay.reloadIndicator.SetActive(false);
    }
    
    public override WeaponType GetWeaponType()
    {
        return WeaponType.Laser;
    }
    
    public override void OnDisable()
    {
        base.OnDisable();
        isFiring = false;
    }

}