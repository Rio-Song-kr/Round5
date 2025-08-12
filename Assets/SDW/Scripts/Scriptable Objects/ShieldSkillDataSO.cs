using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

[CreateAssetMenu(fileName = "ShieldSkill", menuName = "Skills/ShieldSkill")]
public class ShieldSkillDataSO : DefenceSkillDataSO
{
    [Header("Shield Effect Settings")]
    //# Abyssal SKill 활성화/비활성화 시 무적 Shield의 Scale이 증가/감소 하는데 소요되는 시간
    public float ShieldScaleDuration = 0.2f;
    //# Abyssal SKill 활성화/비활성화 시 무적 Shield의 Scale
    public float ShieldScaleMultiplier = 1.1f;
    public float FadeDuration = 0.2f;

    private PoolManager _pools;
    public PoolManager Pools => _pools;

    private GameObject _skillEffectObject;

    public override void Initialize()
    {
        _pools = FindFirstObjectByType<PoolManager>();
        if (!PhotonNetwork.OfflineMode)
        {
            _pools.InitializePool("ShieldEffect", SkillEffectPrefab, 1, 3);
        }
    }

    public override void Activate(Vector3 skillPosition, Transform playerTransform)
    {
        _skillEffectObject = PhotonNetwork.Instantiate("ShieldEffect", skillPosition, Quaternion.identity);

        var skillEffect = _skillEffectObject.GetComponent<ShieldEffect>();

        int playerViewId = playerTransform.gameObject.GetComponent<PhotonView>().ViewID;
        skillEffect.Initialize(this, playerViewId);
    }

    public override void Deactivate()
    {
        PhotonNetwork.Destroy(_skillEffectObject);
    }
}