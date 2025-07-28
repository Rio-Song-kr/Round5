using Photon.Pun;
using UnityEngine;

public class GunControll : MonoBehaviourPun
{
    public Transform muzzle; // 총알 발사 위치

    [Header("본인 무기 오브젝트 (기능 포함)")]
    public GameObject bulletWeaponObject;
    public GameObject razorWeaponObject; // 레이저
    public GameObject barrelWeaponObject; // 샷건
    
    private GameObject currentWeaponObject;

    // 현재 무기
    public IWeapon currentWeapon;

    /// <summary>
    /// 처음 시작 시 기본 무기 적용
    /// </summary>
    private void Start()
    {
        if (photonView.IsMine)
            EquipWeapon(WeaponType.Bullet);
    }

    private void Update()
    {
        if (!photonView.IsMine) return;
        
        // 마우스 위치에 따라 총구 회전
        RotateMuzzleToMouse();
        
        // 마우스 왼쪽 버튼 클릭 시 공격
        if (Input.GetMouseButtonDown(0) && photonView.IsMine)
        {
            if (currentWeapon != null)
            {
                var weapon = currentWeaponObject.GetComponent<IWeapon>();
                weapon?.Attack(muzzle);
                // currentWeapon.Attack(muzzle);
            }
        }
        
        if (Input.GetMouseButton(0) )
        {
        }
        
        // TODO: 테스트(숫자 키로 무기 변경)
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            // EquipWeapon(bulletWeaponObject);
            EquipWeapon(WeaponType.Bullet);
            // photonView.RPC(nameof(ChangeWeapon), RpcTarget.Others, bulletWeaponObject.name);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            // EquipWeapon(razorWeaponObject);
            EquipWeapon(WeaponType.Laser);
            // photonView.RPC(nameof(ChangeWeapon), RpcTarget.Others, razorWeaponObject.name);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            // EquipWeapon(barrelWeaponObject);
            EquipWeapon(WeaponType.Shotgun);
            // photonView.RPC(nameof(ChangeWeapon), RpcTarget.Others, barrelWeaponObject.name);
        }
        
    }

    // TODO: 테스트 
    [PunRPC]
    public void ChangeWeapon(string objectName)
    {
        if (!photonView.IsMine) return;
        
        switch (objectName)
        {
            case "BulletWeapon":
                // EquipWeapon(bulletWeaponObject);
                break;
            case "RazorWeapon":
                // EquipWeapon(razorWeaponObject);
                break;
            case "BarrelWeapon":
                // EquipWeapon(barrelWeaponObject);
                break;
        }
    }
    
    public void EquipWeapon(WeaponType weaponType)
    {
        DisableAllWeapons();

        switch (weaponType)
        {
            case WeaponType.Bullet:
                currentWeaponObject = bulletWeaponObject;
                break;
            case WeaponType.Laser:
                currentWeaponObject = razorWeaponObject;
                break;
            case WeaponType.Shotgun:
                currentWeaponObject = barrelWeaponObject;
                break;
        }

        currentWeaponObject.SetActive(true);
        currentWeapon = currentWeaponObject.GetComponent<IWeapon>();
        currentWeapon?.Initialize();

        // 내 무기 선택 시 상대에게 전달
        if (photonView.IsMine)
        {
            photonView.RPC(nameof(RPC_SetWeapon), RpcTarget.Others, (int)weaponType);
        }
    }
    
    [PunRPC]
    private void RPC_SetWeapon(int weaponTypeInt)
    {
        if (photonView.IsMine) return; // 내 무기 설정은 무시

        EquipWeapon((WeaponType)weaponTypeInt);
    }

    private void DisableAllWeapons()
    {
        bulletWeaponObject.SetActive(false);
        razorWeaponObject.SetActive(false);
        barrelWeaponObject.SetActive(false);
    }

    /// <summary>
    /// 마우스 따라 회전
    /// </summary>
    private void RotateMuzzleToMouse()
    {
        if (muzzle == null || Camera.main == null) return;
        
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = mousePosition - muzzle.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        muzzle.rotation = Quaternion.Euler(0, 0, angle);
        if (muzzle.parent != null)
        {
            muzzle.parent.rotation = Quaternion.Euler(0, 0, angle); // 부모 회전
        }
    }

    /// <summary>
    /// 무기 교체 함수
    /// </summary>
    /// <param name="weaponObject"></param>
    // public void EquipWeapon(GameObject weaponObject)
    // {
    //     // 모든 무기 GameObject 끄기
    //     bulletWeaponObject.SetActive(false);
    //     razorWeaponObject.SetActive(false);
    //     barrelWeaponObject.SetActive(false);
    //
    //     // 현재 무기 비우기
    //     currentWeapon = null;
    //
    //     // 선택된 무기만 켜기
    //     weaponObject.SetActive(true);
    //
    //     // 무기 스크립트 가져와서 현재 무기로 지정
    //     currentWeapon = weaponObject.GetComponent<IWeapon>();
    //     currentWeapon.Initialize();
    //
    // }
    
} 