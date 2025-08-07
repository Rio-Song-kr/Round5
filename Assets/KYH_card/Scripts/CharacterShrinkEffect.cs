using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using DG.Tweening;

public class CharacterShrinkEffect : MonoBehaviourPun
{
    [SerializeField] private Transform characterTransform;

    [SerializeField] private GameObject body;
    [SerializeField] private Transform bodyDefaultTransform;

    private Vector3 originalScale;

    [Header("�ִϸ��̼� Ŀ��")]
    [SerializeField] private AnimationCurve shrinkCurve;   // 1�ʰ� ���


    private void Awake()
    {
        if (characterTransform == null)
        {
            Debug.LogError("Character Transform�� ������� �ʾҽ��ϴ�.");
            return;
        }

        originalScale = characterTransform.localScale;
    }

    private void OnEnable()
    {
        gameObject.transform.localScale = originalScale;
        body.transform.position = bodyDefaultTransform.position;
    }

    [PunRPC]
    public void RPC_PlayShrinkAnimation()
    {
        // ������ ���� - Vector3.one * 0.2f �� �� ������ �ֳ�...?
        Vector3 shrinkScale = Vector3.zero;

        Sequence seq = DOTween.Sequence();

        // Ŀ��Shrink�� 1�ʰ� �پ��
        seq.Append(characterTransform.DOScale(shrinkScale, 1f).SetEase(shrinkCurve));

        // Ŀ��GrowFast�� 0.2�ʰ� ����
        //seq.Append(characterTransform.DOScale(originalScale, 0.4f).SetEase(growCurve));
    }

    public void RequestShrinkEffect()
    {
        photonView.RPC(nameof(RPC_PlayShrinkAnimation), RpcTarget.All);
    }
}

