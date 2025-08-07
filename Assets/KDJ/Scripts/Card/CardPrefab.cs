using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CardPrefab : MonoBehaviour
{
    [SerializeField] public CardBase CardData;
    [SerializeField] private TMP_Text _cardNameText;
    [SerializeField] private TMP_Text _cardDescriptionText;

    private void Start()
    {
        if (CardData != null)
        {
            _cardNameText.text = CardData.CardName;
            _cardDescriptionText.text = CardData.CardDescription;
        }
    }
}
