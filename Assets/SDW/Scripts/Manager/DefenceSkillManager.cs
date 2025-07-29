using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class DefenceSkillManager : MonoBehaviourPun
{
    [SerializeField] private DefenceSkillDatabaseSO _skillDatabase;
    [SerializeField] private GameObject _effectsObject;

    private List<DefenceSkillDataSO> _skills;
    public List<DefenceSkillDataSO> Skills => _skills;

    private Coroutine _coroutine;
    private WaitForSeconds _coolDown;

    private PhotonView _photonView;

    //# Test용
    private Coroutine _testCoroutine;

    /// <summary>
    /// Skill 및 Skill Database 초기화
    /// </summary>
    private void Start()
    {
        _photonView = GetComponent<PhotonView>();

        _effectsObject = GameObject.FindGameObjectWithTag("Effects");

        _skills = new List<DefenceSkillDataSO>();
        _skillDatabase.Initialize();

        //# 테스트용 - Skill 추가
        // AddSkill(DefenceSkills.AbyssalCountdown);
        AddSkill(DefenceSkills.Emp);
        AddSkill(DefenceSkills.FrostSlam);
    }

    //# 테스트를 위한 Update
    private void Update()
    {
        if (!Input.GetMouseButtonDown(1)) return;

        if (_photonView.IsMine)
            _photonView.RPC(nameof(UseSkills), RpcTarget.All, transform.position);
    }

    /// <summary>
    /// 카드 선택 창에서 선택된 Defence Skill을 추가
    /// </summary>
    /// <param name="skillName">추가하려는 스킬</param>
    public void AddSkill(DefenceSkills skillName)
    {
        var skill = _skillDatabase.SkillDatabase[skillName];
        _skills.Add(skill);

        skill.Initialize(gameObject.transform, _effectsObject.transform);

        if (skill.IsPassive && _photonView.IsMine) skill.Activate(transform.position);
    }

    /// <summary>
    /// 마우스 우클릭 시 Defence Skill들을 실행
    /// </summary>
    [PunRPC]
    public void UseSkills(Vector3 skillPosition)
    {
        if (_coroutine != null) return;

        foreach (var skill in _skills)
        {
            if (skill.IsPassive) continue;

            skill.Activate(skillPosition);

            if (_coolDown == null) _coolDown = new WaitForSeconds(skill.CoolDown);
        }

        _coroutine = StartCoroutine(SKillCoolDown());
    }

    /// <summary>
    /// CoolDown 체크
    /// </summary>
    /// <returns></returns>
    private IEnumerator SKillCoolDown()
    {
        yield return _coolDown;
        _coroutine = null;
    }
}