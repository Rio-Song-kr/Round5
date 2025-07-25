using UnityEngine;

public class GunControll : MonoBehaviour
{
    public Transform muzzle; // 총알 발사 위치
    
    public GameObject bulletWeaponObject;
    public GameObject razorWeaponObject; // 레이저
    public GameObject barrelWeaponObject; // 샷건

    // 현재 무기
    public IWeapon currentWeapon;

    /// <summary>
    /// 처음 시작 시 기본 무기 적용
    /// </summary>
    private void Start()
    {
        EquipWeapon(bulletWeaponObject);
    }

    private void Update()
    {
        // 마우스 위치에 따라 총구 회전
        RotateMuzzleToMouse();
        
        // 마우스 왼쪽 버튼 클릭 시 공격
        if (Input.GetMouseButtonDown(0))
        {
            currentWeapon.Attack(muzzle);
        }

        // TODO: 테스트(숫자 키로 무기 변경)
        if (Input.GetKeyDown(KeyCode.Alpha1))
            EquipWeapon(bulletWeaponObject);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            EquipWeapon(razorWeaponObject);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            EquipWeapon(barrelWeaponObject);
    }

    /// <summary>
    /// 마우스 따라 회전
    /// </summary>
    private void RotateMuzzleToMouse()
    {
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
    public void EquipWeapon(GameObject weaponObject)
    {
        // 모든 무기 GameObject 끄기
        bulletWeaponObject.SetActive(false);
        razorWeaponObject.SetActive(false);
        barrelWeaponObject.SetActive(false);

        // 현재 무기 비우기
        currentWeapon = null;

        // 선택된 무기만 켜기
        weaponObject.SetActive(true);

        // 무기 스크립트 가져와서 현재 무기로 지정
        currentWeapon = weaponObject.GetComponent<IWeapon>();
        currentWeapon.Initialize();

    }
    
} 