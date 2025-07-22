using UnityEngine;

namespace JGH_Player
{
    public class Player : MonoBehaviour
    {
        [Header("발사 위치")] [SerializeField] private Transform firingPoint; // 총알 발사 위치

        [Header("기본 무기")] [SerializeField] private GameObject defaultWeaponObject; // 기본 무기

        [Header("모든 무기 컴포넌트")] [SerializeField]
        private GameObject bulletWeaponObject; // 기본 총   

        [SerializeField] private GameObject laserWeaponObject; // 레이저
        [SerializeField] private GameObject shotgunWeaponObject; // 샷건

        // 현재 무기
        private IWeapon currentWeapon;

        private void Start()
        {
            EquipWeapon(defaultWeaponObject);
            EquipWeapon(bulletWeaponObject);
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                currentWeapon.Attack(firingPoint);
            }

            // TODO: 테스트(숫자 키로 무기 변경)
            if (Input.GetKeyDown(KeyCode.Alpha1))
                EquipWeapon(bulletWeaponObject);

            if (Input.GetKeyDown(KeyCode.Alpha2))
                EquipWeapon(laserWeaponObject);

            if (Input.GetKeyDown(KeyCode.Alpha3))
                EquipWeapon(shotgunWeaponObject);
        }

        public void EquipWeapon(GameObject weaponObject)
        {
            // 모든 무기 GameObject 끄기
            bulletWeaponObject.SetActive(false);
            laserWeaponObject.SetActive(false);
            shotgunWeaponObject.SetActive(false);

            // 선택된 무기만 켜기
            weaponObject.SetActive(true);

            // 현재 무기 컴포넌트 갱신
            currentWeapon = weaponObject.GetComponent<IWeapon>();
        }
    }
}