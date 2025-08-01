using UnityEngine;
using UnityEngine.EventSystems;

// 개인화면에서만 표시하면 되는 UI이므로 동기화의 필요성은 없음
/// <summary>
/// 마우스를 갖다댔을 때 자신 및 상대의 스킬 정보를 출력하는 UI
/// </summary>
public class SkillInfoUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{ 
    public void OnPointerEnter(PointerEventData eventData)
    {
        // TODO : 카드 UI 표시
        Debug.Log("Info Activate");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // TODO : 카드 UI 비활성화
        Debug.Log("Info Inactivate");
    }
}
