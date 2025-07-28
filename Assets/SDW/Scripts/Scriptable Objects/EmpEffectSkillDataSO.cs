using ExitGames.Client.Photon.StructWrapping;
using UnityEngine;

[CreateAssetMenu(fileName = "EmpEffectSkill", menuName = "Skills/EmpEffectSkill")]
public class EmpEffectSkillDataSO : DefenceSkillDataSO
{
    [Header("Expansion Settings")]
    // # 초기 확장 속도
    public float InitialExpansionSpeed = 10f;
    // # 최소 확장 속도
    public float MinExpansionSpeed = 0.7f;
    // # 빠른 확장이 끝나는 지점의 반경
    public float FastExpansionRadius = 1f;
    // # 빠른 확장 속도에서 최소 속도로 감속되는 데 걸리는 시간
    public float DecelerationDuration = 0.5f;

    [Header("Arc Settings")]
    public GameObject ArcPrefab;
    private EmpEffect _skillEffect;
    //# 원형 확장에 사용될 개별 Arc 프리팹
    
    // # 생성할 Arc의 총 개수
    public int ArcCount = 30;
    
    private EmpPool<EmpEffect> _empPool;
    public EmpPool<EmpEffect> EmpPool => _empPool;

    private ArcPool<ArcController> _arcPool;
    public ArcPool<ArcController> ArcPool => _arcPool;
    
    public override void Initialize(GameObject player)
    {
        _empPool = new();
        _empPool.SetPool(SkillEffectPrefab, player.transform);

        _arcPool = new();
        _arcPool.SetPool(ArcPrefab, ArcCount, player.transform);
    }

    public override void Activate()
    {
        var skillEffect = _empPool.Pool.Get();
        skillEffect.Initialize(this);
    }
}
