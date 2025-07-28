using UnityEngine;
using Photon.Pun;

public class BulletWeapon : BaseWeapon
{
    public WeaponType weaponType = WeaponType.Bullet;

    
    public override void Attack(Transform firingPoint)
    {
        if (isReloading || currentAmmo <= 0) return;
        photonView.RPC(nameof(Shot), RpcTarget.All, firingPoint.position, firingPoint.rotation);
    }


    [PunRPC]
    public void Shot(Vector3 position, Quaternion rotation)
    {
        GameObject bulletObj = PhotonNetwork.Instantiate("Bullets/Bullet", position, rotation);
        if (bulletObj.TryGetComponent(out Bullet bullet))
        {
            bullet.GetComponent<Bullet>().BulletMove(bulletSpeed);
        }
        bullet.StartCoroutine(bullet.DestroyAfterDelay(4f));
        
        
        currentAmmo--;
        lastAttackTime = Time.time;

        UpdateAmmoUI();
        
        if (currentAmmo <= 0)
        {
            StartAutoReload();
        }
        
        CameraShake.Instance.ShakeCaller(0.3f, 0.05f);
    }

    public override WeaponType GetWeaponType()
    {
        return weaponType;
    }

}
