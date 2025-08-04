using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TestCardPlayer : MonoBehaviour
{
    [SerializeField] private PlayerStatusDataSO _playerStatusData;
    [SerializeField] private CardBase _card1;
    [SerializeField] private CardBase _card2;
    [SerializeField] private TMP_Text _statusText;
    [SerializeField] private TMP_Text _cardListText;

    private float _hp;
    private float _defenseSkillCooldown;
    private float _damage;
    private float _reloadSpeed;
    private float _ammoCapacity;

    private void Start()
    {
        _hp = _playerStatusData.DefaultHp;
        _defenseSkillCooldown = _playerStatusData.DefaultInvincibilityCoolTime;
        _damage = _playerStatusData.DefaultDamage;
        _reloadSpeed = _playerStatusData.DefaultReloadSpeed;
        _ammoCapacity = _playerStatusData.DefaultAmmo;
        SetHUD();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            GetCard(_card1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            GetCard(_card2);
        }
    }

    private void GetCard(CardBase card)
    {
        CardManager.Instance.AddCard(card);
        PlayerStatusDataSO stats = CardManager.Instance.GetCaculateCardStats();

        if (stats != null)
        {
            _hp = stats.DefaultHp;
            _defenseSkillCooldown = stats.DefaultInvincibilityCoolTime;
            _damage = stats.DefaultDamage;
            _reloadSpeed = stats.DefaultReloadSpeed;
            _ammoCapacity = stats.DefaultAmmo;
        }
        
        SetHUD();
    }

    private void SetHUD()
    {
        _statusText.text = $"HP: {_hp}\nDefense Cooldown: {_defenseSkillCooldown}\nDamage: {_damage}\nReload Speed: {_reloadSpeed}\nAmmo Capacity: {_ammoCapacity}";
        _cardListText.text = "";

        List<CardBase> cards = null;

        if (!CardManager.Instance.IsCardEmpty)
        {
            cards = CardManager.Instance.GetLists();
        }
        
        if (cards != null && cards.Count > 0)
        {
            foreach (var card in cards)
            {
                _cardListText.text += $"{card.CardName}\n";
            }
        }
    }
}
