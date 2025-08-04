using Photon.Pun;
using UnityEngine;

public class BarrelWeapon : BaseWeapon
{
    public int pelletCount = 6; // 퍼지는 총알 개수
    public float spreadAngle = 30f; // 퍼지는 각도
    public WeaponType weaponType = WeaponType.Shotgun;
    
    // 무기 발사
    public override void Attack(Transform firingPoint)
    {
        if (!photonView.IsMine) return;
        if (isReloading || currentAmmo < useAmmo) return;
        if (!CanAttack()) return; // 공격 속도 체크
        
        float angleStep = spreadAngle / (pelletCount - 1);
        float startAngle = -spreadAngle / 2f;

        double fireTime = PhotonNetwork.Time;

        for (int i = 0; i < pelletCount; i++)
        {
            float angle = startAngle + angleStep * i;
            
            // Quaternion spreadRotation = fireRot * Quaternion.Euler(0, 0, angle - 90);
            Quaternion spreadRotation = firingPoint.rotation * Quaternion.Euler(0, 0, angle);
            Vector3 spawnPos = firingPoint.position + (firingPoint.rotation * Vector3.up) * 0.2f;
        
            // PhotonNetwork.Instantiate("Bullets/Bullet", spawnPos, spreadRotation);
            GameObject bulletObj = PhotonNetwork.Instantiate("Bullet", spawnPos, spreadRotation);
            
            // gunController._isBigBullet = true;
            // gunController._isExplosiveBullet= true;
            bulletObj.GetComponent<PhotonView>()?.RPC( "RPC_SetBulletType", RpcTarget.AllViaServer, gunController._isBigBullet, gunController._isExplosiveBullet );
        
            PhotonView bulletView = bulletObj.GetComponent<PhotonView>();
            if (bulletView != null)
            {
                // Vector3 direction = spreadRotation * Vector3.up;
                bulletView.RPC("InitBullet", RpcTarget.All, CardManager.Instance.GetCaculateCardStats().DefaultReloadSpeed, fireTime);
                // bulletView.RPC("InitBullet", RpcTarget.All, bulletSpeed, fireTime);
            }
            // if (bulletObj.TryGetComponent(out Bullet bullet))
            // {
                // bullet.BulletMove(bulletSpeed);
                // bullet.StartCoroutine(bullet.DestroyAfterDelay(4f));
            // }
        }
        
        currentAmmo -= useAmmo;
        lastAttackTime = Time.time;
        UpdateAmmoUI();
        
        if (currentAmmo < useAmmo)
        {
            ReloadSpeedFromAnimator();
            StartAutoReload();
        }

        CameraShake.Instance.ShakeCaller(0.3f, 0.05f);
    }
    //
    // [PunRPC]
    // public void Shot(Vector3 firePos, Quaternion fireRot)
    // {
    // }

    public override WeaponType GetWeaponType()
    {
        return weaponType;
    }
    
    
}