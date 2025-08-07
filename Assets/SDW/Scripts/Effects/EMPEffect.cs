using Photon.Pun;
using UnityEngine;

/// <summary>
/// Arc들을 생성하고, 모든 Arc가 사라졌는지 확인하여 자신을 파괴하는 역할만 수행
/// 모든 확장 및 이동 로직은 ArcController가 독립적으로 처리
/// </summary>
public class EmpEffect : MonoBehaviourPun
{
    //# Emp Effect Skill Data
    public EmpEffectSkillDataSO SkillData;
    private int _playerViewId;

    /// <summary>
    /// _skillData 초기화 및 Arc Effect 실행
    /// </summary>
    /// <param name="skillData">EmpEffectSkillDataSO 인스턴스</param>
    public void Initialize(EmpEffectSkillDataSO skillData)
    {
        SkillData = skillData;

        if (!photonView.IsMine) return;

        RunArcEffect();
    }

    /// <summary>
    /// 초기화된 _skillData에 따라 지정된 개수의 ArcController 인스턴스를 생성하고 설정
    /// 각 ArcController는 주어진 방향, 초기 확장 속도, 가속/감속 파라미터를 기반으로 동작을 시작
    /// 실행 후 현재 인스턴스를 풀링 풀에 반환
    /// </summary>
    private void RunArcEffect()
    {
        for (int i = 0; i < SkillData.ArcCount; i++)
        {
            //# Arc가 확장될 방향과 초기 회전값을 계산
            float angle = i * 2f * Mathf.PI / SkillData.ArcCount;

            //# 방향 계산
            Vector3 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            var rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);


            //# Pool에서 Arc를 Get
            var arcControllerObject = PhotonNetwork.Instantiate(
                "Arc",
                SkillData.SkillPosition + direction * SkillData.InitialialRadius,
                rotation
            );

            var arcController = arcControllerObject.GetComponent<ArcController>();

            int viewId = gameObject.GetComponent<PhotonView>().ViewID;

            //# 기존 매개변수로 초기화
            arcController.Initialize(
                SkillData,
                viewId,
                direction,
                SkillData.InitialExpansionSpeed,
                SkillData.MinExpansionSpeed,
                SkillData.FastExpansionRadius,
                SkillData.DecelerationDuration
            );
        }

        //# Pool에 EmpEffect 반환
        if (photonView.IsMine)
            PhotonNetwork.Destroy(gameObject);
    }
}