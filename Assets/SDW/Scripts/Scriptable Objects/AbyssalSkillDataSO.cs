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

    public override void Initialize(Transform effectsTransform)
    {
        _pools = FindFirstObjectByType<PoolManager>();
        _pools.InitializePool(SkillEffectPrefab.name, SkillEffectPrefab, 2, 5);
        _pools.InitializePool(VfxCorePullPrefab.name, VfxCorePullPrefab, 2, 5);
        _pools.InitializePool(ShieldPrefab.name, ShieldPrefab, 2, 5);
    }

    public override void Activate(Vector3 skillPosition, Transform playerTransform = null)
    {
        var skillEffectObject = PhotonNetwork.Instantiate(SkillEffectPrefab.name, skillPosition, Quaternion.identity);

        var skillEffect = skillEffectObject.GetComponent<AbyssalCountdownEffect>();

        int playerViewId = playerTransform.gameObject.GetComponent<PhotonView>().ViewID;
        skillEffect.Initialize(this, playerViewId);
    }
}