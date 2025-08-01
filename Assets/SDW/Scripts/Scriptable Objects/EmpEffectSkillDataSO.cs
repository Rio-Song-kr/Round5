using UnityEngine;

[CreateAssetMenu(fileName = "EmpEffectSkill", menuName = "Skills/EmpEffectSkill")]
public class EmpEffectSkillDataSO : DefenceSkillDataSO
{
    [Header("Expansion Settings")]
    //# 초기 확장 속도
    public float InitialExpansionSpeed = 10f;
    //# 최소 확장 속도
    public float MinExpansionSpeed = 0.7f;
    //# 빠른 확장이 끝나는 지점의 반경
    public float FastExpansionRadius = 1f;
    //# 빠른 확장 속도에서 최소 속도로 감속되는 데 걸리는 시간
    public float DecelerationDuration = 0.5f;

    [Header("Arc Settings")]
    public float InitialialRadius = 0.3f;
    public GameObject ArcPrefab;
    public GameObject VfxArcPrefab;
    private EmpEffect _skillEffect;
    public LayerMask TargetMask;
    //# 원형 확장에 사용될 개별 Arc 프리팹

    // # 생성할 Arc의 총 개수
    public int ArcCount = 30;

    private PoolManager _pools;
    public PoolManager Pools => _pools;

    public Vector3 SkillPosition;
    private Transform _effectTransform;

    public override void Initialize(Transform effectsTransform)
    {
        _effectTransform = effectsTransform;

        _pools = FindFirstObjectByType<PoolManager>();
        _pools.InitializePool("EmpEffect", SkillEffectPrefab, 2, 5);
        _pools.InitializePool("Arc", ArcPrefab, 30, 70);
        _pools.InitializePool("VFX_Arc", VfxArcPrefab, 30, 70);
    }

    public override void Activate(Vector3 skillPosition, Transform playerTransform = null)
    {
        SkillPosition = skillPosition;

        var skillEffectObject = _pools.Instantiate("EmpEffect", SkillPosition, Quaternion.identity);
        skillEffectObject.transform.parent = _effectTransform;

        var skillEffect = skillEffectObject.GetComponent<EmpEffect>();
        skillEffect.Initialize(this);
    }
}