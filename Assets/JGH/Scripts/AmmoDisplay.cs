using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AmmoDisplay : MonoBehaviour
{
    [SerializeField] private GameObject ammoContainerPrefab;   // 동그라미 프리팹
    [SerializeField] private Transform ammoContainerParent;     // 탄창 아이콘들이 붙을 부모
    
    public GameObject reloadIndicator; // PB Loop 
    
    private List<GameObject> iconList = new List<GameObject>();

    private void Start()
    {
        SetReloading(false);
    }

    public void UpdateAmmoIcons(int current, int max)
    {
        // 아이콘이 부족하면 생성
        while (iconList.Count < max)
        {
            GameObject icon = Instantiate(ammoContainerPrefab, ammoContainerParent);
            iconList.Add(icon);
        }

        // 아이콘 활성/비활성 갱신
        for (int i = 0; i < iconList.Count; i++)
        {
            iconList[i].SetActive(i < max);  // max를 초과하는건 숨김
            // Image img = iconList[i].GetComponent<Image>();
            Image img = iconList[i].GetComponentInChildren<Image>();
            if (img == null)
            {
                Debug.LogWarning($"Ammo icon {i}에 Image 컴포넌트가 없습니다.");
                continue;
            }
            if (i < current)
                img.color = Color.white; // 남은 탄약
            else
                img.color = new Color(1f, 1f, 1f, 0f); // 투명 처리
        }
        
    }
    
    public void SetReloading(bool active)
    {
        if (reloadIndicator != null)
            reloadIndicator.SetActive(active);
    }
}
