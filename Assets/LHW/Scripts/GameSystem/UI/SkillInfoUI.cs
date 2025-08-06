using Photon.Pun;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// ?????? ??????? ?? ??? ?? ????? ??? ?????? ?????? UI
/// </summary>
public class SkillInfoUI : MonoBehaviourPun, IPointerEnterHandler, IPointerExitHandler
{ 
    public void OnPointerEnter(PointerEventData eventData)
    {
        // TODO : ??? UI ???
        Debug.Log("Info Activate");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // TODO : ??? UI ??????
        Debug.Log("Info Inactivate");
    }

    [PunRPC]
    public void SetParentToPanel(int parentViewID)
    {
        Transform parent = PhotonView.Find(parentViewID).transform;
        transform.SetParent(parent);
    }
}
