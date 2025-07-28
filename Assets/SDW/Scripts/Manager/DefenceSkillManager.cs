using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenceSkillManager : MonoBehaviour
{
    [SerializeField] private DefenceSkillDatabaseSO _skillDatabase;
    [SerializeField] private GameObject _effectsObject;

    private List<DefenceSkillDataSO> _skills;
    public List<DefenceSkillDataSO> Skills => _skills;

    private Coroutine _coroutine;
    private WaitForSeconds _coolDown;

    //# Test용
    private Coroutine _testCoroutine;

    /// <summary>
    /// Skill 및 Skill Database 초기화
    /// </summary>
    private void Start()
    {
        _skills = new List<DefenceSkillDataSO>();
        _skillDatabase.Initialize();

        //# 테스트용 - Skill 추가
        AddSkill(DefenceSkills.AbyssalCountdown);
        AddSkill(DefenceSkills.Emp);
        AddSkill(DefenceSkills.FrostSlam);
    }

    //# 테스트를 위한 Update
    private void Update()
    {
        if (_testCoroutine != null) return;

        _testCoroutine = StartCoroutine(TestUseSkills());
    }

    /// <summary>
    /// 카드 선택 창에서 선택된 Defence Skill을 추가
    /// </summary>
    /// <param name="skillName">추가하려는 스킬</param>
    public void AddSkill(DefenceSkills skillName)
    {
        var skill = _skillDatabase.SkillDatabase[skillName];
        _skills.Add(skill);


        if (skill.IsPassive)
        {
            skill.Initialize(gameObject);
            skill.Activate();
            return;
        }

        skill.Initialize(_effectsObject);
    }

    /// <summary>
    /// 마우스 우클릭 시 Defence Skill들을 실행
    /// </summary>
    public void UseSkills()
    {
        if (_coroutine != null) return;

        foreach (var skill in _skills)
        {
            if (skill.IsPassive) continue;

            skill.Activate();

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

    //# 테스트용 - cooldown 내에 재호출 시 사용되는지 여부 확인
    private IEnumerator TestUseSkills()
    {
        yield return new WaitForSeconds(4f);
        UseSkills();
        _testCoroutine = null;
    }
}