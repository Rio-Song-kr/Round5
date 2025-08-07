using Photon.Pun;
using UnityEngine;

public class GunControll : MonoBehaviourPun
{
    public Transform muzzle; // 총알 발사 위치

    [Header("본인 무기 오브젝트 (기능 포함)")]
    public GameObject bulletWeaponObject;
    public GameObject LaserWeaponObject; // 레이저
    public GameObject barrelWeaponObject; // 샷건

    private GameObject currentWeaponObject;

    public bool _isBigBullet; // 큰 총알 여부
    public bool _isExplosiveBullet; // 폭발 총알 여부

    // 현재 무기
    // private WeaponType? lastWeapon = null;
    public IWeapon currentWeapon;
    private WeaponType? lastWeapon = null;

    /// <summary>
    /// 처음 시작 시 기본 무기 적용
    /// </summary>
    private void Start()
    {
        // EquipWeapon(WeaponType.Bullet);

        if (photonView.IsMine)
            EquipWeapon(WeaponType.Bullet);
    }

    private void Update()
    {
        // 이거 로직상 필요 없으면 주석 풀지 마세요 공격 안돼요
        if (!photonView.IsMine || !InGameManager.Instance.IsStarted) return;

        // 마우스 위치에 따라 총구 회전
        // RotateMuzzleToMouse();

        // 마우스 왼쪽 버튼 클릭 시 공격
        if (!Input.GetKey(KeyCode.E) && Input.GetMouseButtonDown(0) && photonView.IsMine)
        {
            if (currentWeapon != null)
            {
                var weapon = currentWeaponObject.GetComponent<IWeapon>();
                weapon?.Attack(muzzle);
                // currentWeapon.Attack(muzzle);
            }
        }

        OnApplyCards(out bool[] weapons);
    }

    /// <summary>
    /// 적용된 무기 카드를 확인 후 무기 변경을 담당하는 함수한테 전달하는 함수
    /// </summary>
    /// <param name="weapons"></param>
    private void OnApplyCards(out bool[] weapons)
    {
        weapons = CardManager.Instance.GetWeaponCard();

        // 폭발성인 경우
        _isExplosiveBullet = weapons[1];
        // 큰 총알인 경우
        _isBigBullet = weapons[3];

        var weaponToEquip = WeaponType.Bullet; // 기본은 Bullet

        if (weapons[0])
            weaponToEquip = WeaponType.Laser;
        else if (weapons[2])
            weaponToEquip = WeaponType.Shotgun;

        // 같은 무기면 장착 안함
        if (lastWeapon != weaponToEquip)
        {
            EquipWeapon(weaponToEquip);
            lastWeapon = weaponToEquip;
        }
    }

    /// <summary>
    /// 무기 변경을 담당하는 함수
    /// </summary>
    /// <param name="weaponType"></param>
    public void EquipWeapon(WeaponType weaponType)
    {
        DisableAllWeapons();

        switch (weaponType)
        {
            case WeaponType.Bullet:
                currentWeaponObject = bulletWeaponObject; break;
            case WeaponType.Laser:
                currentWeaponObject = LaserWeaponObject; break;
            case WeaponType.Shotgun:
                currentWeaponObject = barrelWeaponObject; break;
        }


        currentWeaponObject.SetActive(true);
        currentWeapon = currentWeaponObject.GetComponent<IWeapon>();

        currentWeapon.Initialize();


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
        LaserWeaponObject.SetActive(false);
        barrelWeaponObject.SetActive(false);
    }

    /// <summary>
    /// 마우스 따라 회전
    /// </summary>
    //private void RotateMuzzleToMouse()
    //{
    //    if (muzzle == null || Camera.main == null) return;
    //    
    //    Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    //    Vector2 direction = mousePosition - muzzle.position;
    //    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    //    muzzle.rotation = Quaternion.Euler(0, 0, angle);
    //    if (muzzle.parent != null)
    //    {
    //        muzzle.parent.rotation = Quaternion.Euler(0, 0, angle); // 부모 회전
    //    }
    //}
}