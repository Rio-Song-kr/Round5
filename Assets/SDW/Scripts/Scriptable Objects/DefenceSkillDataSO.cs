using UnityEngine;

public abstract class DefenceSkillDataSO : ScriptableObject
{
    [Header("Skill Settings")]
    public DefenceSkills SkillName;
    //# 스킬 사용 후 재사용 시간
    public float CoolDown;
    //# Passive Skill은 게임 시작 시 바로 활성화, Active Skill은 조작키에 따라 실행
    public bool IsPassive;

    [Header("Status Effect Settings")]
    public StatusEffect[] Status;

    [Header("Skill Prefab")]
    [SerializeField] protected GameObject SkillEffectPrefab;

    /// <summary>
    /// Skill Effect  생성 및 초기화
    /// Player의 자식으로 생성
    /// </summary>
    /// <param name="effectsTransform">Player와 함께 움직이면 안되는 Effects들의 부모 오브젝트의 Transform</param>
    public abstract void Initialize(Transform effectsTransform);

    /// <summary>
    /// 스킬 활성화
    /// </summary>
    /// <param name="skillPosition">스킬 위치</param>
    /// <param name="playerTransform">Skill Effect의 부모 오브젝트인 플레이어</param>
    public abstract void Activate(Vector3 skillPosition, Transform playerTransform = null);
}