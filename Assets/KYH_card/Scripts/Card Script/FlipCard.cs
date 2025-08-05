using DG.Tweening;
using Photon.Pun;
using UnityEngine;
using UnityEngine.EventSystems;
public class FlipCard : MonoBehaviourPun, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    
    private bool isFlipped = false;
    public bool IsFlipped { get { return isFlipped; } }
    [SerializeField] private bool isSelected = false;
    private bool isHovered = false;

    [Header("��/�޸� ��Ʈ ������Ʈ")]
    public GameObject frontRoot;
    public GameObject backRoot;

    [Header("����")]
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

        // ���� ȸ���� �����ϰ� Y�� 180���� ����
        Vector3 rot = transform.localEulerAngles;
        rot.y = 180f;
        transform.localRotation = Quaternion.Euler(rot);

        if (frontRoot != null) frontRoot.SetActive(false);
        if (backRoot != null) backRoot.SetActive(true);

        Debug.Log($"[FlipCard] Start() | isInteractable={isInteractable}");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"[FlipCard] OnPointerEnter()");
        if (!isInteractable) return; // ��ȣ�ۿ� �Ұ� �� ����

        if (!isFlipped)
        {
            OnClickCard(); // �ڵ� ������
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
       if (!isInteractable) return; // ��ȣ�ۿ� �Ұ� �� ����
       isHovered = false;
        manager.photonView.RPC(nameof(CardSelectManager.RPC_UnhighlightCardByIndex), RpcTarget.All, cardIndex);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"[FlipCard] OnPointerClick | isInteractable={isInteractable}, isHovered={isHovered}");

        if (!isInteractable || !isHovered) return; //Ŭ�� ���� ����

        OnClickCard(); // ���� ���� �״�� ȣ��
    }

    public void OnClickCard()
    {
        Debug.Log($"[FlipCard] OnClickCard 호출됨 | isFlipped={isFlipped}, isSelected={isSelected}");

        if (!isInteractable) return; // ��ȣ�ۿ� �Ұ� �� ����

        if (!isFlipped)
        {
            
            Debug.Log($"[FlipCard] ī�� Ŭ���� index = {cardIndex}");
            // ���� �ִϸ��̼� ����
            PlayFlipAnimation();

            // RPC�� ��� Ŭ���̾�Ʈ���Ե� flip �ִϸ��̼� ����ȭ
            manager.photonView.RPC(nameof(CardSelectManager.RPC_FlipCardByIndex), RpcTarget.All, cardIndex);

            // �� ����ȭ RPC ȣ��
            manager.photonView.RPC(nameof(CardSelectManager.RPC_SelectCardArm), RpcTarget.All, cardIndex);


        }
        else if (!isSelected)
        {
            isSelected = true;
            manager?.OnCardSelected(gameObject);
        }
    }

    [PunRPC]
    public void RPC_Flip()
    {
        isFlipped = true;
        PlayFlipAnimation();
    }

    public void PlayFlipAnimation()
    {
        if (isFlipped) return;

        Debug.Log("ī�� ������");
        // ���� ȸ���� ��������
        Vector3 startEuler = transform.localEulerAngles;

        // Z�� ���� (360�� �̻� ���̸� -360 ���� �� ��ȣ ����)
        float z = startEuler.z;
        if (z > 180f) z -= 360f;
        float flippedZ = -z;

        // Y = 0 ���� ȸ���ϸ鼭 Z�� �����ؼ� ��ä�� ���� ����
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

    public void ResetisSelect()
    {
        isSelected = false;
    }
}