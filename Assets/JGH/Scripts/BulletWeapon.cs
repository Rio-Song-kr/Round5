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

        
        
        // PhotonNetwork.Instantiate("Bullets/Bullet", firingPoint.position, firingPoint.rotation);
        // if (bulletObj.TryGetComponent(out Bullet bullet))
        // {
        // bullet.BulletMove(bulletSpeed); // 여기서 바로 호출
        // bullet.StartCoroutine(bullet.DestroyAfterDelay(4f));
        //     // // bullet.GetComponent<Bullet>().BulletMove(bulletSpeed);
        // }
        // bullet.StartCoroutine(bullet.DestroyAfterDelay(4f));
        
        GameObject bulletObj = PhotonNetwork.Instantiate("Bullet", firingPoint.position, firingPoint.rotation);

        PhotonView bulletView = bulletObj.GetComponent<PhotonView>();

        if (bulletView != null)
        {
            bulletView.RPC("InitBullet", RpcTarget.All, bulletSpeed, PhotonNetwork.Time);
        }
        // if (photonView.IsMine)
        // {
            currentAmmo--;
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
