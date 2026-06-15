using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    [Header("스킬 매니저 컴포넌트 연결")]
    [SerializeField] private SkillShooter skillShooter;
    // 추후 AOE 계열 등등의 스킬 매니저 추가 예정 (ノ・∀・)ノ

    public event System.Action<string, int> OnSkillLevelChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// 스킬 이름으로 레벨업
    /// </summary>
    public bool LevelUpSkill(string skillName)
    {
        int newLevel = -1;

        if (skillShooter != null)
            newLevel = skillShooter.LevelUp(skillName);
        // else if (otherSkillManager != null) 

        // 레벨업 성공 처리
        if (newLevel != -1)
        {
            OnSkillLevelChanged?.Invoke(skillName, newLevel);
            return true;
        }

        return false;
    }

    /// <summary>레벨업 가능한 전체 스킬 SO 목록 반환 (UI용)</summary>
    public List<SkillData> GetAllSkillData()
    {
        var list = new List<SkillData>();

        if (skillShooter != null) list.AddRange(skillShooter.GetSkillDataList());

        // null 제거
        list.RemoveAll(item => item == null);
        return list;
    }
}