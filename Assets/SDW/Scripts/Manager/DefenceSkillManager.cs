using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class DefenceSkillManager : MonoBehaviourPun
{
    [SerializeField] private DefenceSkillDatabaseSO _skillDatabase;
    [SerializeField] private GameObject _effectsObject;
    private IStatusEffectable _status;

    private List<DefenceSkillDataSO> _skills;
    public List<DefenceSkillDataSO> Skills => _skills;

    private List<DefenceSkills> _skillNames;
    public List<DefenceSkills> SkillNames => _skillNames;

    private Coroutine _coroutine;
    private WaitForSeconds _coolDown;

    private PhotonView _photonView;

    //# Test용
    private Coroutine _testCoroutine;

    private bool _isAllJoined;
    private bool _isStarted;

    /// <summary>
    /// Skill 및 Skill Database 초기화
    /// </summary>
    private void Start()
    {
        _photonView = GetComponent<PhotonView>();

        _effectsObject = GameObject.FindGameObjectWithTag("Effects");
        _status = GetComponent<PlayerStatus>();

        _skills = new List<DefenceSkillDataSO>();
        _skillDatabase.Initialize();

        _skillNames = new List<DefenceSkills>();

        //# 테스트용 - Skill 추가, 추후 카드 선택 시 AddSkill을 추가하여 사용
        AddSkill(DefenceSkills.AbyssalCountdown);
        AddSkill(DefenceSkills.Emp);
        AddSkill(DefenceSkills.FrostSlam);

        StartCoroutine(WaitForAllPlayerJoin());
    }

    //# 테스트를 위한 Update
    private void Update()
    {
        if (_isAllJoined)
        {
            StartCoroutine(DelayedTime());
            _isAllJoined = false;
            _isStarted = true;
        }

        if (!Input.GetMouseButtonDown(1) || !_isStarted) return;

        if (_photonView.IsMine) UseActiveSkills(transform.position);
    }

    /// <summary>
    /// 모든 플레이어가 게임에 조인될 때까지 대기하는 Coroutine
    /// </summary>
    private IEnumerator WaitForAllPlayerJoin()
    {
        //# 싱글일 때는 MaxPlayers가 1, 멀티일 때는 2가 되어야 함
        int maxPlayers = 2;

        if (maxPlayers == 1 || PhotonNetwork.PlayerList.Length >= maxPlayers) _isAllJoined = true;
        else
        {
            while (PhotonNetwork.PlayerList.Length < maxPlayers)
            {
                yield return null;
            }

            _isAllJoined = true;
        }
    }

    /// <summary>
    /// 지연 시간 기반 스킬 활성화 코루틴 함수
    /// </summary>
    /// <returns>코루틴 실행 중 해당 작업을 관리하는 IEnumerator 객체를 반환합니다.</returns>
    private IEnumerator DelayedTime()
    {
        //# 추후 게임이 시작된 것인지 체크
        yield return new WaitForSeconds(0.1f);

        if (_photonView.IsMine)
            UsePassiveSkills();
    }

    /// <summary>
    /// 카드 선택 창에서 선택된 Defence Skill을 추가
    /// </summary>
    /// <param name="skillName">추가하려는 스킬</param>
    public void AddSkill(DefenceSkills skillName)
    {
        var skill = _skillDatabase.SkillDatabase[skillName];
        _skills.Add(skill);

        _skillNames.Add(skillName);

        skill.Initialize();

        switch (skillName)
        {
            // case DefenceSkills.AbyssalCountdown:
            //     break;
            case DefenceSkills.Emp:
                foreach (var status in skill.Status)
                {
                    if (!status.IsPermanent || !status.CanAddPlayer) continue;

                    _status.ApplyStatusEffect(status.EffectType, status.EffectValue, status.Duration, status.IsPermanent);
                }
                break;
            // case DefenceSkills.FrostSlam:
            //     break;
        }
    }

    /// <summary>
    /// 활성화할 패시브 스킬을 지정된 ViewID의 객체에 적용
    /// </summary>
    /// <param name="viewID">패시브 스킬을 적용할 플레이어의 Photon View ID</param>
    public void UsePassiveSkills()
    {
        foreach (var skill in _skills)
        {
            if (!skill.IsPassive) continue;

            skill.Activate(transform.position, transform);
        }
    }

    /// <summary>
    /// 마우스 우클릭 시 Defence Skill들을 실행
    /// </summary>
    public void UseActiveSkills(Vector3 skillPosition)
    {
        if (_coroutine != null) return;

        foreach (var skill in _skills)
        {
            if (skill.IsPassive) continue;

            skill.Activate(skillPosition, transform);

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