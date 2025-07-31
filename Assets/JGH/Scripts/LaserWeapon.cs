using UnityEngine;
using System.Collections;
using Photon.Pun;

public class LaserWeapon : BaseWeapon
{
    [SerializeField] private float laserDuration = 2f;
    
    private bool isFiring = false;
    private WeaponType weaponType = WeaponType.Laser;
    [SerializeField] private GameObject currentLaserInstance;
    
    // private PoolManager _laserSootPool;
    
    private GunControll gunController;
        

    protected override void Start()
    {
        base.Start();
        gunController = GetComponentInParent<GunControll>();
    }

    public override void Attack(Transform firingPoint)
    {
        if (!photonView.IsMine) return;
        if (isFiring || isReloading || currentAmmo < 2) return;
        currentAmmo -= 2;
        
        UpdateAmmoUI();
        
        ammoDisplay.reloadIndicator.SetActive(false);
        
        // 위치 및 방향 지정
        Laser laserComponent = GetComponent<Laser>();
        laserComponent.transform.position = gunController.muzzle.position;
        laserComponent.transform.up = gunController.muzzle.up;
            
        laserComponent.Duration = laserDuration;
        laserComponent.ShootLaser(); // Laser.cs의 ShootLaser 메서드를 호출하여 레이저 발사
        
        photonView.RPC(nameof(RPC_FireLaser), RpcTarget.All);
    }
    
    [PunRPC]
    private void RPC_FireLaser()
    {
        StopAllCoroutines(); // 이전 발사나 리로드 코루틴 종료
        
            
        StartCoroutine(FireLaserRoutine());
    }
    
    private IEnumerator FireLaserRoutine()
    {
        isFiring = true;
        isReloading = false;

        yield return new WaitForSeconds(laserDuration);
        
        isFiring = false;
        StartAutoReload();

        isReloading = true;
        ammoDisplay.reloadIndicator.SetActive(true);
        
        // 애니메이션 트리거 실행
        animator?.SetTrigger("Reload");
        yield return null; // 한 프레임 대기하여 클립이 로드되도록 함
        ReloadSpeedFromAnimator();

        yield return new WaitForSeconds(reloadTime);
        
        currentAmmo = maxAmmo;
        UpdateAmmoUI();
        isReloading = false;
        ammoDisplay.reloadIndicator.SetActive(false);
        // Destroy();
    }
    
    protected override void Update()
    {
        base.Update();
        if (currentLaserInstance != null && gunController.muzzle != null)
        {
            currentLaserInstance.transform.position = gunController.muzzle.position;
            currentLaserInstance.transform.up = gunController.muzzle.up;
        }
    }
    
    public override WeaponType GetWeaponType()
    {
        return WeaponType.Laser;
    }
    
    public override void OnDisable()
    {
        base.OnDisable();
        isFiring = false;
        // Destroy();
    }

    // private void Destroy()
    // {
    //     foreach (Transform child in transform)
    //     {
    //         PhotonNetwork.Destroy(child.gameObject);
    //     }
    // }
}