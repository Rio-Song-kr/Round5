using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CardPrefab : MonoBehaviour
{
    [SerializeField] private CardBase _cardData;
    [SerializeField] private TMP_Text _cardNameText;
    [SerializeField] private TMP_Text _cardDescriptionText;

    private void Start()
    {
        if (_cardData != null)
        {
            _cardNameText.text = _cardData.CardName;
            _cardDescriptionText.text = _cardData.CardDescription;
        }
    }
}
