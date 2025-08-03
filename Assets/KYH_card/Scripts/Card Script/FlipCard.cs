using DG.Tweening;
using Photon.Pun;
using UnityEngine;
using UnityEngine.EventSystems;
public class FlipCard : MonoBehaviourPun, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    
    private bool isFlipped = false;
    public bool IsFlipped { get { return isFlipped; } }
    private bool isSelected = false;
    private bool isHovered = false;

    [Header("앞/뒷면 루트 오브젝트")]
    public GameObject frontRoot;
    public GameObject backRoot;

    [Header("설정")]
    public float flipDuration = 0.25f;
    public float hoverScale = 1.3f;

    private Vector3 originalScale;
    private int cardIndex;
    private CardSelectManager manager;

    private bool isInteractable = true;

    public void SetInteractable(bool value)
    {
        isInteractable = value;
    }
    public void SetManager(CardSelectManager mgr)
    {
        manager = mgr;
    }

    public void SetCardIndex(int index)
    {
        cardIndex = index;
    }
    private void Start()
    {
        isFlipped = false;
        isSelected = false;
        isHovered = false;

        originalScale = transform.localScale;

        // 현재 회전을 유지하고 Y만 180도로 변경
        Vector3 rot = transform.localEulerAngles;
        rot.y = 180f;
        transform.localRotation = Quaternion.Euler(rot);

        if (frontRoot != null) frontRoot.SetActive(false);
        if (backRoot != null) backRoot.SetActive(true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isInteractable) return; // 상호작용 불가 시 무시

        if (!isFlipped)
        {
            OnClickCard(); // 자동 뒤집기
        }
        else
        {
            manager.photonView.RPC(nameof(CardSelectManager.RPC_SelectCardArm), RpcTarget.All, cardIndex);
        }

            isHovered = true;
        manager.photonView.RPC(nameof(CardSelectManager.RPC_HighlightCardByIndex), RpcTarget.All, cardIndex);
    }

   public void OnPointerExit(PointerEventData eventData)
   {
       if (!isInteractable) return; // 상호작용 불가 시 무시
       isHovered = false;
        manager.photonView.RPC(nameof(CardSelectManager.RPC_UnhighlightCardByIndex), RpcTarget.All, cardIndex);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isInteractable || !isHovered) return; //클릭 제한 포함

        OnClickCard(); // 기존 로직 그대로 호출
    }

    public void OnClickCard()
    {
        if (!isInteractable) return; // 상호작용 불가 시 무시

        if (!isFlipped)
        {
            
            Debug.Log($"[FlipCard] 카드 클릭됨 index = {cardIndex}");
            // 로컬 애니메이션 실행
            PlayFlipAnimation();

            // RPC로 상대 클라이언트에게도 flip 애니메이션 동기화
            manager.photonView.RPC(nameof(CardSelectManager.RPC_FlipCardByIndex), RpcTarget.All, cardIndex);

            // 팔 동기화 RPC 호출
            manager.photonView.RPC(nameof(CardSelectManager.RPC_SelectCardArm), RpcTarget.All, cardIndex);


        }
        else if (!isSelected)
        {
            isSelected = true;
            manager?.OnCardSelected(gameObject);
        }
    }

    [PunRPC]
    void RPC_Flip()
    {
        isFlipped = true;
        PlayFlipAnimation();
    }

    public void PlayFlipAnimation()
    {
        if (isFlipped) return;

        Debug.Log("카드 뒤집힘");
        // 현재 회전값 가져오기
        Vector3 startEuler = transform.localEulerAngles;

        // Z축 보정 (360도 이상 값이면 -360 보정 후 부호 반전)
        float z = startEuler.z;
        if (z > 180f) z -= 360f;
        float flippedZ = -z;

        // Y = 0 으로 회전하면서 Z는 반전해서 부채꼴 각도 유지
        Vector3 targetEuler = new Vector3(0f, 0f, flippedZ);

        transform.DORotate(targetEuler, flipDuration)
            .SetEase(Ease.InOutSine)
            .OnUpdate(() =>
            {
                float yRot = transform.localEulerAngles.y;
                if (yRot > 180f) yRot -= 360f;
                bool showFront = Mathf.Abs(yRot) <= 90f;

                if (frontRoot != null) frontRoot.SetActive(showFront);
                if (backRoot != null) backRoot.SetActive(!showFront);
            })
            .OnComplete(() =>
            {
                if (frontRoot != null) frontRoot.SetActive(true);
                if (backRoot != null) backRoot.SetActive(false);
            });
        isFlipped = true;
    }

    public void PlayHighlight()
    {
        transform.DOScale(originalScale * hoverScale, 0.15f).SetEase(Ease.OutBack);
    }

    public void PlayUnhighlight()
    {
        transform.DOScale(originalScale, 0.15f).SetEase(Ease.InBack);
    }
}