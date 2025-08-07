using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBullet : TestWeapon
{
    [SerializeField] private GameObject _bulletPrefab;
    
    private bool _isExplosiveBullet = false;
    private bool _isBigBullet = false;
    private PlayerStatusDataSO _playerStatus;

    private void Start()
    {
        _playerStatus = CardManager.Instance.GetCaculateCardStats();
    }

    public override void Attack()
    {
        GameObject bullet = Instantiate(_bulletPrefab, _firePoint.position, _firePoint.rotation);
        bullet.GetComponent<Bullet>().SetBulletType(_isBigBullet, _isExplosiveBullet);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.AddForce(_firePoint.up * _playerStatus.DefaultBulletSpeed, ForceMode2D.Impulse);
        }
    }

    public void SetBulletType(bool isBig, bool isEx)
    {
        _isBigBullet = isBig;
        _isExplosiveBullet = isEx;
    }
}
