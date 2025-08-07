using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLaser : TestWeapon
{
    [SerializeField] private GameObject _laserPrefab;
    private Laser _laser;

    public override void Attack()
    {
        if (_laser == null)
        {
            // 레이저가 아직 생성되지 않았다면 새로 생성합니다.
            _laser = Instantiate(_laserPrefab, _firePoint.position, _firePoint.rotation).GetComponent<Laser>();
        }

        _laser.transform.SetParent(_firePoint);
        _laser.ShootLaser();
    }
}
