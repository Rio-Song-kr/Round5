using Photon.Pun;
using UnityEngine;

[CreateAssetMenu(fileName = "ShieldUpSkill", menuName = "Skills/ShieldUpSkill")]
public class ShieldsUpDataSO : DefenceSkillDataSO
{
    //# Abyssal SKill 활성화/비활성화 시 무적 Shield의 Scale이 증가/감소 하는데 소요되는 시간
    public float ShieldScaleDuration = 0.2f;
    //# Abyssal SKill 활성화/비활성화 시 무적 Shield의 Scale
    public float ShieldScaleMultiplier = 1.1f;
    public float FadeDuration = 0.2f;

    private PoolManager _pools;
    public PoolManager Pools => _pools;

    public override void Initialize()
    {
        _pools = FindFirstObjectByType<PoolManager>();
        _pools.InitializePool("ShieldsUpEffect", SkillEffectPrefab, 2, 5);
    }

    public override void Activate(Vector3 skillPosition, Transform playerTransform)
    {
        var skillEffectObject =
            PhotonNetwork.Instantiate("ShieldsUpEffect", skillPosition, Quaternion.identity);

        var skillEffect = skillEffectObject.GetComponent<ShieldsUpEffect>();
        int playerViewId = playerTransform.gameObject.GetComponent<PhotonView>().ViewID;

        skillEffect.Initialize(this, playerViewId);
    }
}