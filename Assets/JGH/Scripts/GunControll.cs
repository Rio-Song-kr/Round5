using UnityEngine;
using UnityEngine.Serialization;

public class GunControll : MonoBehaviour
{
    [SerializeField] private Transform muzzle; // 총알 발사 위치
    
    [SerializeField] private GameObject bulletWeaponObject;
    [SerializeField] private GameObject razorWeaponObject; // 레이저
    [SerializeField] private GameObject barrelWeaponObject; // 샷건

    // 현재 무기
    private IWeapon currentWeapon;

    private void Start()
    {
        EquipWeapon(bulletWeaponObject);
    }

    private void Update()
    {
        RotateMuzzleToMouse();
        
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