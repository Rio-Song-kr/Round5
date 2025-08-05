using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class DefenceSkillManager : MonoBehaviourPun
{
    [SerializeField] private DefenceSkillDatabaseSO _skillDatabase;
    private PlayerStatus _status;

    private List<DefenceSkillDataSO> _skills;
    public List<DefenceSkillDataSO> Skills => _skills;

    private List<DefenceSkills> _skillNames;
    public List<DefenceSkills> SkillNames => _skillNames;

    private Dictionary<DefenceSkills, Coroutine> _coroutines = new Dictionary<DefenceSkills, Coroutine>();
    private Dictionary<DefenceSkills, WaitForSeconds> _cooldowns = new Dictionary<DefenceSkills, WaitForSeconds>();

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

        _status = GetComponent<PlayerStatus>();

        _skills = new List<DefenceSkillDataSO>();
        _skillDatabase.Initialize();

        _skillNames = new List<DefenceSkills>();

        //# 테스트용 - Skill 추가, 추후 카드 선택 시 AddSkill을 추가하여 사용
        //# Shield 스킬 기본 스킬
        // AddSkill(DefenceSkills.Shield);
        //
        // AddSkill(DefenceSkills.ShieldUp);
        // AddSkill(DefenceSkills.Defender);
        // AddSkill(DefenceSkills.Huge);
        //
        // AddSkill(DefenceSkills.AbyssalCountdown);
        // AddSkill(DefenceSkills.Emp);
        AddSkill(DefenceSkills.FrostSlam);

        StartCoroutine(WaitForAllPlayerJoin());
    }

    /// <summary>
    /// 주기적으로 업데이트를 실행하여 게임 내 스킬 상태를 관리하고, 플레이어가 마우스 우클릭 시 활성 스킬을 적용
    /// 모든 플레이어가 연결된 후 지연 시간 후 패시브 스킬을 초기화
    /// </summary>
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
    /// <returns>코루틴 실행 중 해당 작업을 관리하는 IEnumerator 객체를 반환</returns>
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

        if (!skill.IsPassive)
        {
            _coroutines[skill.SkillName] = null;
            _cooldowns[skill.SkillName] = null;
        }

        skill.Initialize();

        //todo Huge, Defender Effect도 카드 선택 시 추가가 되도록 수정해야 함
        switch (skillName)
        {
            // case DefenceSkills.AbyssalCountdown:
            //     break;
            case DefenceSkills.Emp:
            case DefenceSkills.Defender:
            case DefenceSkills.Huge:
            case DefenceSkills.ShieldUp:
                foreach (var status in skill.Status)
                {
                    if (!status.IsPermanent || !status.CanAddPlayer) continue;

                    //# Passive Skill의 경우, skillName도 같이 전달하여, EffectValue 처리시 + 또는 * 인지 구분
                    _status.ApplyStatusEffect(
                        status.EffectType,
                        status.EffectValue,
                        status.Duration,
                        status.IsPermanent,
                        skillName
                    );
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
    private void UsePassiveSkills()
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
    private void UseActiveSkills(Vector3 skillPosition)
    {
        foreach (var skill in _skills)
        {
            if (skill.IsPassive) continue;

            if (_coroutines[skill.SkillName] != null) continue;

            skill.Activate(skillPosition, transform);

            if (skill.SkillName == DefenceSkills.Shield)
                _cooldowns[skill.SkillName] = new WaitForSeconds(_status.InvincibilityCooldown);
            else
                _cooldowns[skill.SkillName] = new WaitForSeconds(skill.Cooldown);

            _coroutines[skill.SkillName] = StartCoroutine(SKillCoolDown(skill.SkillName));
        }
    }

    /// <summary>
    /// CoolDown 체크
    /// </summary>
    /// <returns></returns>
    private IEnumerator SKillCoolDown(DefenceSkills skillName)
    {
        yield return _cooldowns[skillName];

        _coroutines[skillName] = null;
        _cooldowns[skillName] = null;
    }
}