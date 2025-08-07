using UnityEngine;
using Photon.Pun;

public class BulletWeapon : BaseWeapon
{
    public WeaponType weaponType = WeaponType.Bullet;

    
    public override void Attack(Transform firingPoint)
    {
        if (!photonView.IsMine) return;
        if (isReloading || currentAmmo <= 0) return;
        if (!CanAttack()) return; // 공격 속도 체크

        // GameObject bulletObj = PhotonNetwork.Instantiate("Bullet", gunController.muzzle.position, gunController.muzzle.rotation);
        object[] data = new object[] { gunController._isBigBullet, gunController._isExplosiveBullet };
        
        Vector3 spawnPosition = gunController.muzzle.position;
        Quaternion spawnRotation = gunController.muzzle.rotation;
        
        // 빅 불릿인경우 충돌나사 총구 앞쪽에서 생성
        if (gunController._isBigBullet)
        {
            spawnPosition += gunController.muzzle.up * 0.05f;
        }
        
        GameObject bulletObj = PhotonNetwork.Instantiate("Bullet", spawnPosition, spawnRotation, 0, data);
        // if (bulletObj.TryGetComponent(out Bullet bullet))
        // {
        //     // 자기 자신에만 적용
        //     // bullet.SetInitBulletType(gunController._isBigBullet, gunController._isExplosiveBullet);
        //     // 모든 클라이언트에 동기화
            // bullet.SetBulletType(gunController._isBigBullet, gunController._isExplosiveBullet);
        // }
        // gunController._isBigBullet = true;
        // gunController._isExplosiveBullet= true;
        // bulletObj.GetComponent<PhotonView>()?.RPC( "RPC_SetBulletType", RpcTarget.AllViaServer, gunController._isBigBullet, gunController._isExplosiveBullet );
        
        PhotonView bulletView = bulletObj.GetComponent<PhotonView>();
        if (bulletView != null)
        {
            // bulletView.RPC("InitBullet", RpcTarget.All, bulletSpeed, PhotonNetwork.Time);
            bulletView.RPC("InitBullet", RpcTarget.All, CardManager.Instance.GetCaculateCardStats().DefaultBulletSpeed, PhotonNetwork.Time);
        }
        
        // if (photonView.IsMine)
        // {
            // currentAmmo -= (int)CardManager.Instance.GetCaculateCardStats().AmmoConsumption;
            currentAmmo -= 1;
            lastAttackTime = Time.time;

            UpdateAmmoUI();

            if (currentAmmo <= 0)
            {
                StartAutoReload();
            }


            CameraShake.Instance.ShakeCaller(0.3f, 0.05f);
        // }
        // photonView.RPC(nameof(Shot), RpcTarget.All, firingPoint.position, firingPoint.rotation);
    }

    //
    // [PunRPC]
    // public void Shot(Vector3 position, Quaternion rotation)
    // {
        // GameObject bulletObj = PhotonNetwork.Instantiate("Bullets/Bullet", position, rotation);
        // if (bulletObj.TryGetComponent(out Bullet bullet))
        // {
        //     bullet.BulletMove(bulletSpeed); // 여기서 바로 호출
        //     bullet.StartCoroutine(bullet.DestroyAfterDelay(4f));
        //    // // bullet.GetComponent<Bullet>().BulletMove(bulletSpeed);
        // }
        // bullet.StartCoroutine(bullet.DestroyAfterDelay(4f));
        //
        // currentAmmo--;
        // lastAttackTime = Time.time;
        //
        // UpdateAmmoUI();
        //
        // if (currentAmmo <= 0)
        // {
        //     StartAutoReload();
        // }
        
        // CameraShake.Instance.ShakeCaller(0.3f, 0.05f);
    // }

    public override WeaponType GetWeaponType()
    {
        return weaponType;
    }

}
