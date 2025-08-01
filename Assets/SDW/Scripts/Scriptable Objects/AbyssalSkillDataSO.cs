using Photon.Pun;
using UnityEngine;

[CreateAssetMenu(fileName = "AbyssalCountdownSkill", menuName = "Skills/AbyssalCountdownSkill")]
public class AbyssalSkillDataSO : DefenceSkillDataSO
{
    [Header("Default Effect Settings")]
    //# 카운트다운 게이지 충전 시간
    public float ChargeTime = 10f;
    //# 카운트다운 후 최대 사용 시간
    public float ActivateTime = 2f;

    [Header("Scaling Effect Settings")]
    //# Abyssal Skill 활성화/비활성화 시 Circle의 Scale이 증가/감소 하는데 소요되는 시간
    public float ScalingDuration = 0.05f;
    //# Abyssal Skill 활성화/비활성화 시 Circle의 Scale
    public float TargetScaleMultiplier = 1.2f;

    [Header("Shield Effect Settings")]
    public GameObject ShieldPrefab;
    //# Abyssal Skill 활성화 시 Shield가 유지되는 시간
    // public float ShieldEffectActiveTime = 2f;
    //# Abyssal SKill 활성화/비활성화 시 무적 Shield의 Scale이 증가/감소 하는데 소요되는 시간
    public float ShieldScaleDuration = 0.2f;
    //# Abyssal SKill 활성화/비활성화 시 무적 Shield의 Scale
    public float ShieldScaleMultiplier = 1.1f;

    [Header("Particle Effect Settings")]
    public GameObject VfxCorePullPrefab;

    private AbyssalCountdownEffect _skillEffect;

    private PoolManager _pools;
    public PoolManager Pools => _pools;

    private Transform _effectTransform;

    public override void Initialize(Transform effectsTransform)
    {
        _effectTransform = effectsTransform;

        _pools = FindFirstObjectByType<PoolManager>();
        _pools.InitializePool("AbyssalCountEffect", SkillEffectPrefab, 2, 5);
        _pools.InitializePool("VFX_CorePoolEffect", VfxCorePullPrefab, 2, 5);
        _pools.InitializePool("ShieldEffect", ShieldPrefab, 2, 5);
    }

    public override void Activate(Vector3 skillPosition, Transform playerTransform = null)
    {
        var skillEffectObject = _pools.Instantiate("AbyssalCountEffect", skillPosition, Quaternion.identity);

        //# Pool에서 꺼낼 때, 소유권 이전 과정이 필요
        skillEffectObject.transform.parent = _effectTransform;

        var skillEffect = skillEffectObject.GetComponent<AbyssalCountdownEffect>();
        skillEffect.Initialize(this, playerTransform);
    }
}