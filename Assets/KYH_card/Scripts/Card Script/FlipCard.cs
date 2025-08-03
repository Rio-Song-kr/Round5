using DG.Tweening;
using Photon.Pun;
using UnityEngine;
using UnityEngine.EventSystems;
public class FlipCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private bool isFlipped = false;
    public bool IsFlipped { get { return isFlipped; } }
    private bool isSelected = false;
    private bool isHovered = false;

    [Header("��/�޸� ��Ʈ ������Ʈ")]
    public GameObject frontRoot;
    public GameObject backRoot;

    [Header("����")]
    public float flipDuration = 0.25f;
    public float hoverScale = 1.1f;

    private Vector3 originalScale;
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
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isInteractable) return; // ��ȣ�ۿ� �Ұ� �� ����

        isHovered = true;
        transform.DOScale(originalScale * hoverScale, 0.4f).SetEase(Ease.OutBack);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isInteractable) return; // ��ȣ�ۿ� �Ұ� �� ����
        isHovered = false;
        transform.DOScale(originalScale, 0.4f).SetEase(Ease.InBack);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isInteractable || !isHovered) return; //Ŭ�� ���� ����

        OnClickCard(); // ���� ���� �״�� ȣ��
    }

    public void OnClickCard()
    {
        if (!isInteractable) return; // ��ȣ�ۿ� �Ұ� �� ����

        if (!isFlipped)
        {
            isFlipped = true;

            // ���� �ִϸ��̼� ����
            PlayFlipAnimation();

            // RPC�� ��� Ŭ���̾�Ʈ���Ե� flip �ִϸ��̼� ����ȭ
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC(nameof(RPC_Flip), RpcTarget.Others);


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

    void PlayFlipAnimation()
    {
        if (isFlipped) return;

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
}