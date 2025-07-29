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
    //# Abyssal SKill 활성화/비활성화 시 무적 Shield의 Scale이 증가/감소 하는데 소요되는 시간
    public float ShieldScaleDuration = 0.2f;
    //# Abyssal SKill 활성화/비활성화 시 무적 Shield의 Scale
    public float ShieldScaleMultiplier = 1.1f;

    private AbyssalCountdownEffect _skillEffect;

    public override void Initialize(Transform playerTransform, Transform effectsTransform)
    {
        _skillEffect = PhotonNetwork.Instantiate(
                "DefenceEffect/" + SkillEffectPrefab.name,
                playerTransform.position,
                playerTransform.rotation)
            .GetComponent<AbyssalCountdownEffect>();
        _skillEffect.transform.parent = playerTransform;
        _skillEffect.gameObject.SetActive(false);
        _skillEffect.Initialize(this);
    }

    public override void Activate(Vector3 skillPoisition) => _skillEffect.gameObject.SetActive(true);
}