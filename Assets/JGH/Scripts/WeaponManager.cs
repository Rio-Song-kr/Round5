using UnityEngine;

public class ManagerWeapon : MonoBehaviour
{
    public static ManagerWeapon Instance { get; private set; }
    
    // 현재 장착된 무기
    public BaseWeapon currentWeapon;
    
    private void Awake()
    {
        // 중복 인스턴스 제거
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    
        Instance = this;
        DontDestroyOnLoad(gameObject); // 씬 전환 시에도 유지
    }

}