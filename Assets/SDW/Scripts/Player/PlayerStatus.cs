using UnityEngine;

public class PlayerStatus : MonoBehaviour, IStatusEffectable
{
    [SerializeField] private PlayerStatusDataSO _playerData;

    private void Start()
    {
    }

    private void Update()
    {
    }

    public void ApplyStatusEffect(StatusEffectType type, float value, float duration)
    {
    }

    public void RemoveStatusEffect(StatusEffectType type)
    {
    }

    public bool HasStatusEffect(StatusEffectType type) => false;
}