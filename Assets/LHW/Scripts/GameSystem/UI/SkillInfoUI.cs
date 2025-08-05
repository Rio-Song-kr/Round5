using Photon.Pun;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 마우스를 갖다댔을 때 자신 및 상대의 스킬 정보를 출력하는 UI
/// </summary>
public class SkillInfoUI : MonoBehaviourPun, IPointerEnterHandler, IPointerExitHandler
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

    [PunRPC]
    public void SetParentToPanel(int parentViewID)
    {
        Transform parent = PhotonView.Find(parentViewID).transform;
        transform.SetParent(parent);
    }
}
