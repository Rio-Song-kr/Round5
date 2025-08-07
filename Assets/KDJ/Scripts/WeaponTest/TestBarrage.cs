using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBarrage : TestWeapon
{
    [SerializeField] private GameObject _bulletPrefab;

    public int pelletCount = 5;
    public float spreadAngle = 30f; // 퍼지는 각도

    private bool _isExplosiveBullet = false;
    private bool _isBigBullet = false;

    private PlayerStatusDataSO _playerStatus;

    private void Start()
    {
        _playerStatus = CardManager.Instance.GetCaculateCardStats();
    }

    public override void Attack()
    {
        float angleStep = spreadAngle / (pelletCount - 1);
        float startAngle = -spreadAngle / 2f;

        for (int i = 0; i < pelletCount; i++)
        {
            float angle = startAngle + angleStep * i;

            Quaternion spreadRotation = _firePoint.rotation * Quaternion.Euler(0, 0, angle);
            Vector3 spawnPos = _firePoint.position + (_firePoint.rotation * Vector3.up) * 0.2f;

            Debug.Log($"Spawn Position: {spawnPos}, Spread Rotation: {spreadRotation.eulerAngles}");

            GameObject bullet = Instantiate(_bulletPrefab, spawnPos, spreadRotation);
            bullet.GetComponent<Bullet>().SetBulletType(_isBigBullet, _isExplosiveBullet);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.AddForce(bullet.transform.up * _playerStatus.DefaultBulletSpeed, ForceMode2D.Impulse);
            }
        }

    }

    public void SetBulletType(bool isBig, bool isEx)
    {
        _isBigBullet = isBig;
        _isExplosiveBullet = isEx;
    }
}
