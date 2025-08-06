using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    [SerializeField] private PlayerStatusDataSO _pStatus;
    [SerializeField] private List<CardBase> _cards = new List<CardBase>();
    public static CardManager Instance { get; private set; }
    public bool IsCardEmpty => _cards.Count == 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    /// <summary>
    /// 현재 카드의 능력치를 계산하여 반환합니다.
    /// 카드가 없으면 null을 반환합니다.
    /// 이 메서드는 PlayerStatusDataSO로 계산된 값을 반환합니다.
    /// </summary>
    /// <returns></returns>
    public PlayerStatusDataSO GetCaculateCardStats()
    {
        float BulletSpeedMultiplierSum = 1;
        float DamageMultiplierSum = 1;
        float ReloadTimeMultiplierSum = 1;
        float BulletSpeedMultiplier = 1;
        float AttackSpeedMultiplier = 1;
        float ReloadTimeAdditionSum = 0;
        int AmmoIncreaseSum = 0;
        int AmmoConsumptionSum = 0;
        int count = 0;

        if (_cards == null || _cards.Count == 0)
        {
            return null; // 카드가 없으면 null 반환
        }

        foreach (var card in _cards)
        {
            count++;
            if (card is AttackCard attackCard)
            {
                // 공격 카드의 능력치 적용
                BulletSpeedMultiplierSum *= attackCard.BulletSpeedMultiplier != 0 ? attackCard.BulletSpeedMultiplier : 1;
                DamageMultiplierSum *= attackCard.DamageMultiplier != 0 ? attackCard.DamageMultiplier : 1;
                ReloadTimeMultiplierSum *= attackCard.ReloadTimeMultiplier != 0 ? attackCard.ReloadTimeMultiplier : 1;
                BulletSpeedMultiplier *= attackCard.BulletSpeedMultiplier != 0 ? attackCard.BulletSpeedMultiplier : 1;
                AttackSpeedMultiplier *= attackCard.AttackSpeedMultiplier != 0 ? attackCard.AttackSpeedMultiplier : 1;
                ReloadTimeAdditionSum += attackCard.ReloadTimeAddition;
                AmmoIncreaseSum += attackCard.AmmoIncrease;
                AmmoConsumptionSum += attackCard.AmmoConsumption;
            }
        }

        PlayerStatusDataSO playerStats = ScriptableObject.CreateInstance<PlayerStatusDataSO>();
        playerStats.DefaultDamage = _pStatus.DefaultDamage * (DamageMultiplierSum != 0 ? DamageMultiplierSum : 1);
        playerStats.DefaultReloadSpeed = (_pStatus.DefaultReloadSpeed + ReloadTimeAdditionSum) * (ReloadTimeMultiplierSum != 0 ? ReloadTimeMultiplierSum : 1);
        playerStats.DefaultAttackSpeed = _pStatus.DefaultAttackSpeed * (AttackSpeedMultiplier != 0 ? AttackSpeedMultiplier : 1);
        playerStats.DefaultBulletSpeed = _pStatus.DefaultBulletSpeed * (BulletSpeedMultiplierSum != 0 ? BulletSpeedMultiplierSum : 1);
        playerStats.DefaultAmmo = _pStatus.DefaultAmmo + AmmoIncreaseSum;
        playerStats.AmmoConsumption = _pStatus.AmmoConsumption + AmmoConsumptionSum;

        return playerStats;
    }


    /// <summary>
    /// 현재 카드중에 무기 카드가 있는지 확인합니다.
    /// 각 무기 카드가 있는지 여부를 bool 배열로 반환합니다.
    /// 0: Laser, 1: Explosive, 2: Barrage
    /// </summary>
    /// <returns></returns>
    public bool[] GetWeaponCard()
    {
        // 무기 인덱스에 따라 무기 설정
        // 예시: 무기 인덱스에 따른 로직 구현
        bool[] weaponCards = new bool[3];
        foreach (var card in _cards)
        {
            if (card is AttackCard attackCard)
            {
                switch (attackCard.WeaponIndex)
                {
                    case 1:
                        weaponCards[0] = true; // Laser
                        break;
                    case 2:
                        weaponCards[1] = true; // Explosive
                        break;
                    case 3:
                        weaponCards[2] = true; // Barrage
                        break;
                    default:
                        break;
                }
            }
        }
        return weaponCards;
    }

    /// <summary>
    /// 카드를 추가합니다.
    /// 카드가 null이 아니면 리스트에 추가합니다.
    /// </summary>
    /// <param name="card"></param>
    public void AddCard(CardBase card)
    {
        if (card != null)
        {
            _cards.Add(card);
        }
    }

    /// <summary>
    /// 카드 리스트를 비웁니다.
    /// </summary>
    public void ClearLists()
    {
        _cards.Clear();
    }

    /// <summary>
    /// 현재 카드 리스트를 반환합니다.
    /// 없다면 null을 반환합니다. 
    /// </summary>
    /// <returns></returns>
    public List<CardBase> GetLists()
    {
        return _cards ?? null;
    }
}
