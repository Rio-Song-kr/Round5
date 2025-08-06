using UnityEngine;
using UnityEngine.UI;

public class CardTestUI : MonoBehaviour
{
    [Header("카드 데이터 (ScriptableObject)")]
    public AttackCard laserCard;
    public AttackCard explosiveCard;
    public AttackCard barrageCard;

    [Header("UI 버튼")]
    public Button laserButton;
    public Button explosiveButton;
    public Button barrageButton;

    private void Start()
    {
        // 버튼 연결
        laserButton.onClick.AddListener(() => AddCard(laserCard));
        explosiveButton.onClick.AddListener(() => AddCard(explosiveCard));
        barrageButton.onClick.AddListener(() => AddCard(barrageCard));
    }

    private void AddCard(AttackCard card)
    {
        CardManager.Instance.AddCard(card);
    }
}