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

    [Header("애니메이션 커브")]
    [SerializeField] private AnimationCurve shrinkCurve;   // 1초간 축소
    [SerializeField] private AnimationCurve growCurve;     // 0.2초간 복원

    private void Awake()
    {
        if (characterTransform == null)
        {
            Debug.LogError("Character Transform이 연결되지 않았습니다.");
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
        // 이형원 수정 - Vector3.one * 0.2f 로 할 이유가 있나...?
        Vector3 shrinkScale = Vector3.zero;

        Sequence seq = DOTween.Sequence();

        // 커브Shrink로 1초간 줄어듦
        seq.Append(characterTransform.DOScale(shrinkScale, 1f).SetEase(shrinkCurve));

        // 커브GrowFast로 0.2초간 복원
        //seq.Append(characterTransform.DOScale(originalScale, 0.4f).SetEase(growCurve));
    }

    public void RequestShrinkEffect()
    {
        photonView.RPC(nameof(RPC_PlayShrinkAnimation), RpcTarget.All);
    }
}

