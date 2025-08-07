using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

public class TestWeaponSystem : MonoBehaviour
{
    [SerializeField] private int _curWeaponIndex = 0;
    [SerializeField] private TestWeapon[] _weapons;
    [SerializeField] private TMP_Text _weaponText;


    private TestWeapon _curWeapon;
    private bool _isExplosiveBullet = false;
    private bool _isBigBullet = false;

    private void Start()
    {
        _curWeapon = _weapons[_curWeaponIndex];
        SetUI();
    }

    private void Update()
    {
        SelectWeapon();
        UseWeapon();
    }


    private void SelectWeapon()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetWeapon(1); // Normal Bullet
            (_curWeapon as TestBullet)?.SetBulletType(_isBigBullet, _isExplosiveBullet);
            SetUI();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetWeapon(2); // Barrage
            (_curWeapon as TestBarrage)?.SetBulletType(_isBigBullet, _isExplosiveBullet);
            SetUI();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SetWeapon(3); // Laser
            SetUI();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SetWeapon(4); // Explosive Bullet
            (_curWeapon as TestBullet)?.SetBulletType(_isBigBullet, _isExplosiveBullet);
            (_curWeapon as TestBarrage)?.SetBulletType(_isBigBullet, _isExplosiveBullet);
            SetUI();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            SetWeapon(5); // Big Bullet
            (_curWeapon as TestBullet)?.SetBulletType(_isBigBullet, _isExplosiveBullet);
            (_curWeapon as TestBarrage)?.SetBulletType(_isBigBullet, _isExplosiveBullet);
            SetUI();
        }
    }


    private void SetWeapon(int weaponIndex)
    {
        _curWeaponIndex = weaponIndex;
        switch (weaponIndex)
        {
            case 1:
                _curWeapon = _weapons[0];
                break;
            case 2:
                _curWeapon = _weapons[1];
                break;
            case 3:
                _curWeapon = _weapons[2];
                break;
            case 4:
                _isExplosiveBullet = !_isExplosiveBullet;
                break;
            case 5:
                _isBigBullet = !_isBigBullet;
                break;
            default:
                Debug.LogWarning("Invalid weapon index: " + weaponIndex);
                break;
        }
    }

    private void UseWeapon()
    {
        if (Input.GetMouseButtonDown(0) && !Input.GetKey(KeyCode.E))
        {
            if (_curWeapon != null)
            {
                SoundManager.Instance.PlaySFX("ShotSound");
                _curWeapon.Attack();
            }
        }
    }

    private void SetUI()
    {
        _weaponText.text = $"무기 선택 : 숫자 1, 2, 3, 4, 5\n현재 무기 : {_curWeapon?.name}\n" +
        $"1 : Normal Bullet\n2 : Barrage\n3 : Laser\n4 : Explosive Bullet : {(_isExplosiveBullet ? "On" : "Off")}\n5 : Big Bullet : {(_isBigBullet ? "On" : "Off")}";
    }

}
