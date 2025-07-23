using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardEffect
{
    QuickReload,
    QuickShot,
    Huge,
    Defender
}

[CreateAssetMenu(fileName = "CardData", menuName = "CardSystem/Card Data", order = 0)]
public class CardDataSO : ScriptableObject
{
    public string cardName;         // 카드 이름
    [TextArea] public string description;  // 설명 (긴 텍스트 가능)
    public string effectSummary;    // 간단한 수치 요약

    public Sprite frontImage;       // 카드 앞면에 표시할 이미지
    public CardEffect effectType;   // 실제 적용될 효과
}
