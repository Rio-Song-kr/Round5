using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using DG.Tweening;

public class CharacterShrinkEffect : MonoBehaviourPun
{
    [SerializeField] private Transform characterTransform;

    private Vector3 originalScale;

    [Header("애니메이션 커브")]
    [SerializeField] private AnimationCurve shrinkCurve;   // 1초간 축소


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
        // 캐릭터가 다시 등장할 때 스케일 복원
        characterTransform.localScale = originalScale;
    }

    [PunRPC]
    public void RPC_PlayShrinkAnimation()
    {
        Vector3 shrinkScale = Vector3.one * 0.2f;

        Sequence seq = DOTween.Sequence();

        // 커브Shrink로 1초간 줄어듦
        seq.Append(characterTransform.DOScale(shrinkScale, 1f).SetEase(shrinkCurve));

        
    }

    public void RequestShrinkEffect()
    {
        photonView.RPC(nameof(RPC_PlayShrinkAnimation), RpcTarget.All);
    }
}

