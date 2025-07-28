using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DefenceSkillDatabase", menuName = "Skills/DefenceSkillDatabase")]
public class DefenceSkillDatabaseSO : ScriptableObject
{
    [Header("Defence Skills")]
    [SerializeField] private DefenceSkillDataSO[] _skillDataSOs;

    private Dictionary<DefenceSkills, DefenceSkillDataSO> _skillDatabase;
    public Dictionary<DefenceSkills, DefenceSkillDataSO> SkillDatabase => _skillDatabase;

    /// <summary>
    /// SkillDataSOs를 바탕으로 Skill Database 초기화
    /// </summary>
    public void Initialize()
    {
        _skillDatabase = new();
        
        foreach (var skillDataSo in _skillDataSOs)
        {
            _skillDatabase.Add(skillDataSo.SkillName, skillDataSo);
        }
    }
}
