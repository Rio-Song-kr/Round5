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

    private bool _isAllJoined;

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
        AddSkill(DefenceSkills.AbyssalCountdown);
        AddSkill(DefenceSkills.Emp);
        AddSkill(DefenceSkills.FrostSlam);

        StartCoroutine(WaitForAllPlayerJoin());
    }

    //# 테스트를 위한 Update
    private void Update()
    {
        if (_isAllJoined && _photonView.IsMine)
        {
            StartCoroutine(DelayedTime());
            _isAllJoined = false;
        }

        if (!Input.GetMouseButtonDown(1)) return;

        if (_photonView.IsMine) _photonView.RPC(nameof(UseActiveSkills), RpcTarget.All, transform.position);
    }

    /// <summary>
    /// 모든 플레이어가 게임에 조인될 때까지 대기하는 Coroutine
    /// </summary>
    private IEnumerator WaitForAllPlayerJoin()
    {
        //# 싱글일 때는 MaxPlayers가 1, 멀티일 때는 2가 되어야 함
        // int maxPlayers = PhotonNetwork.CurrentRoom?.MaxPlayers ?? 1;
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
        yield return null;

        _photonView.RPC(nameof(UsePassiveSkills), RpcTarget.All, _photonView.ViewID);
    }

    /// <summary>
    /// 카드 선택 창에서 선택된 Defence Skill을 추가
    /// </summary>
    /// <param name="skillName">추가하려는 스킬</param>
    public void AddSkill(DefenceSkills skillName)
    {
        var skill = _skillDatabase.SkillDatabase[skillName];
        _skills.Add(skill);

        skill.Initialize(_effectsObject.transform);
    }

    /// <summary>
    /// 활성화할 패시브 스킬을 지정된 ViewID의 객체에 적용
    /// </summary>
    /// <param name="viewID">패시브 스킬을 적용할 플레이어의 Photon View ID</param>
    [PunRPC]
    public void UsePassiveSkills(int viewID)
    {
        var targetView = PhotonView.Find(viewID);

        Debug.Log($"{viewID} - {targetView.transform.position}");

        foreach (var skill in _skills)
        {
            if (!skill.IsPassive) continue;

            skill.Activate(targetView.transform.position, targetView.transform);
        }
    }

    /// <summary>
    /// 마우스 우클릭 시 Defence Skill들을 실행
    /// </summary>
    [PunRPC]
    public void UseActiveSkills(Vector3 skillPosition)
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