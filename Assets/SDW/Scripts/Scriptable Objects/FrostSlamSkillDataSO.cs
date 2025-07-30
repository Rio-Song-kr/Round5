using UnityEngine;

[CreateAssetMenu(fileName = "FrostSlamSkill", menuName = "Skills/FrostSlamSkill")]
public class FrostSlamSkillDataSO : DefenceSkillDataSO
{
    [Header("Default Effect Settings")]
    //# 확장 속도
    public float ExpansionSpeed = 10f;
    //# 확장 시 최대 반지름
    public float MaxRadius = 3f;
    //# 확장 전 시작 반지름
    public float InitialRadius = 0.1f;
    //# Circle의 포인트 수
    public int PointCount = 48;
    //# 감지할 대상 레이더
    public LayerMask TargetLayer;
    //# 감지 거리
    public float DetectOffset = 0.1f;
    //# sphere 충돌 범위
    public float CircleRadius = 0.2f;

    [Header("Line Settings")]
    //# Line Renderer에 사용할 Material
    public Material LineMaterial;
    //# Line의 두께
    public float LineWidth = 0.02f;

    [Header("Straighten Settings")]
    //# 직선화 적용 기준 각도 (이 각도 이상 꺾이면 포인트 유지, 단위: 도)
    public float AngleThresholdForStraighten = 130f;

    [Header("Fade Settings")]
    //# 사라지기 전 대기 시간
    public float FadeDelay = 0.1f;
    //# 사라지는 데 걸리는 시간
    public float FadeDuration = 0.3f;
    //# 멈췄다고 판단할 속도 임계값
    public float StopVelocityThreshold = 0.01f;

    [Header("Particle Effect Settings")]
    public GameObject VfxSmokePrefab;
    private FrostSlamEffect _skillEffect;

    private PoolManager _pools;
    public PoolManager Pools => _pools;

    public Vector3 SkillPoisition;
    private Transform _effectTransform;

    public override void Initialize(Transform effectsTransform)
    {
        _effectTransform = effectsTransform;

        _pools = FindFirstObjectByType<PoolManager>();
        _pools.InitializePool("FrostSlamEffect", SkillEffectPrefab, 2, 5);
        _pools.InitializePool("VFX_Smoke", VfxSmokePrefab, 2, 5);
    }

    public override void Activate(Vector3 skillPosition, Transform playerTransform = null)
    {
        SkillPoisition = skillPosition;

        var skillEffectObject = _pools.Instantiate("FrostSlamEffect", skillPosition, Quaternion.identity);
        skillEffectObject.transform.parent = _effectTransform;

        var skillEffect = skillEffectObject.GetComponent<FrostSlamEffect>();
        skillEffect.Initialize(this);
    }
}