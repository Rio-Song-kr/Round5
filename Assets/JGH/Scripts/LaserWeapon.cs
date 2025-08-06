using UnityEngine;
using System.Collections;
using Photon.Pun;

public class LaserWeapon : BaseWeapon
{
    [SerializeField] private float laserDuration = 2f;

    private GameObject currentLaserInstance;
    private Laser currentLaser;
    
    private bool isFiring = false;
    private WeaponType weaponType = WeaponType.Laser;
   

    public override void Attack(Transform firingPoint)
    {
        if (!photonView.IsMine) return;
        if (isFiring || isReloading || currentAmmo < CardManager.Instance.GetCaculateCardStats().AmmoConsumption) return;
        currentAmmo -= (int)CardManager.Instance.GetCaculateCardStats().AmmoConsumption;
        
        photonView.RPC(nameof(RPC_FireLaser), RpcTarget.All, PhotonNetwork.Time);
    }
    
    [PunRPC]
    private IEnumerator RPC_FireLaser(double fireTime, PhotonMessageInfo info)
    {
        //StopAllCoroutines(); // 이전 발사나 리로드 코루틴 종료
        
        if (currentLaserInstance != null)
        {
            PhotonNetwork.Destroy(currentLaserInstance); // Destroy(gameObject)가 아닌 PhotonNetwork.Destroy!
            currentLaserInstance = null;
        }
        
        yield return null;
        
        float lag = (float)(PhotonNetwork.Time - fireTime);
        yield return new WaitForSeconds(lag); // 지연 보상 적용
        
        if (gunController == null)
        {
            gunController = GetComponentInParent<GunControll>();
            if (gunController == null || gunController.muzzle == null)
            {
                Debug.LogError("[LaserWeapon] gunController 또는 muzzle이 null입니다.");
                yield break;
            }
        }
        
        UpdateAmmoUI();
        

        ammoDisplay.reloadIndicator.SetActive(false);

        // if (currentLaserInstance != null)
        // {
        //     Destroy(currentLaserInstance);
        // }

        // 1. 레이저 프리팹 생성
        currentLaserInstance = PhotonNetwork.Instantiate("Laser", gunController.muzzle.position, gunController.muzzle.rotation);
        // currentLaserInstance = _poolManager.Instantiate("Laser", gunController.muzzle.position, gunController.muzzle.rotation);
        // currentLaserInstance = PhotonNetwork.Instantiate("Laser", position, rotation);

        // 2. muzzle에 붙임
        currentLaserInstance.transform.SetParent(gunController.muzzle);

        // 3. 위치와 방향 로컬 기준으로 맞춤
        currentLaserInstance.transform.localPosition = Vector3.zero;
        currentLaserInstance.transform.localRotation = Quaternion.identity;

        // 4. Shoot
        currentLaser = currentLaserInstance.GetComponent<Laser>();
        currentLaser.Duration = laserDuration;
        currentLaser.ShootLaser();

        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(FireLaserRoutine());
        }

    }

    private IEnumerator FireLaserRoutine()
    {
        isFiring = true;
        isReloading = false;

        // yield return new WaitForSeconds(laserDuration);
        yield return new WaitUntil(() => currentLaser.CanShoot);

        isFiring = false;
        StartAutoReload();

        // 레이저 정리
        if (currentLaserInstance != null)
        {
            PhotonNetwork.Destroy(currentLaserInstance);
        }

        currentLaserInstance = null;
        currentLaser = null;

        // 리로드 애니메이션 및 타이밍
        isReloading = true;
        ammoDisplay.reloadIndicator.SetActive(true);
        
        // 애니메이션 트리거 실행
        animator?.SetTrigger("Reload");

        yield return null;
        ReloadSpeedFromAnimator();
        // yield return new WaitForSeconds(reloadTime);
        yield return new WaitForSeconds(CardManager.Instance.GetCaculateCardStats().DefaultReloadSpeed);

        // currentAmmo = maxAmmo;
        currentAmmo = (int)CardManager.Instance.GetCaculateCardStats().DefaultAmmo;
        UpdateAmmoUI();
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

        if (currentLaserInstance != null)
        {
            PhotonNetwork.Destroy(currentLaserInstance);
            currentLaserInstance = null;
        }
    }
}
