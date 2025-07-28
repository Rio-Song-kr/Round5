using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CardUI : MonoBehaviour
{
    public GameObject frontImage;     // frontImage 오브젝트
    public GameObject backImage;      // BackImage 오브젝트

    public Text cardNameText;         // Cardname 텍스트
    public Text cardDescriptionText;  // Carddiscription 텍스트

 //  private CardEffectSO cardData;
 //  private PlayerStats target;
 //  private bool isFront = false;
 //  private bool isChosen = false;
 //
 //  public void Setup(CardEffectSO card, PlayerStats player)
 //  {
 //      cardData = card;
 //      target = player;
 //
 //      // 텍스트 채우기
 //      cardNameText.text = card.cardName;
 //      cardDescriptionText.text = card.description;
 //
 //      // 초기상태: 뒷면
 //      frontImage.SetActive(false);
 //      backImage.SetActive(true);
 //      isFront = false;
 //
 //      // 등장 연출
 //      transform.localScale = Vector3.zero;
 //      transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
 //  }
 //
 //  public void OnClickCard()
 //  {
 //      if (isChosen) return;
 //
 //      if (!isFront)
 //      {
 //          // 뒷면 → 앞면 회전
 //          isFront = true;
 //          Sequence seq = DOTween.Sequence();
 //          seq.Append(transform.DORotate(new Vector3(0, 90, 0), 0.25f))
 //             .AppendCallback(() =>
 //             {
 //                 backImage.SetActive(false);
 //                 frontImage.SetActive(true);
 //             })
 //             .Append(transform.DORotate(new Vector3(0, 180, 0), 0.25f));
 //      }
 //      else
 //      {
 //          // 앞면 상태일 때 선택 처리
 //          isChosen = true;
 //          cardData.ApplyEffect(target);
 //          CardManager.Instance.RemoveOtherCards(this);
 //      }
 //  }
 //
 //  public void DestroyCard()
 //  {
 //      transform.DOScale(0f, 0.3f).SetEase(Ease.InBack)
 //          .OnComplete(() => Destroy(gameObject));
 //  }
}//
