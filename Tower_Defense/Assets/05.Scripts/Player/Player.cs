using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    [Header("스킬 매니저 컴포넌트 연결")] //  매니저 ----------------------------------------------
    [SerializeField] private SkillShooter skillShooter;
    [SerializeField] private SkillAOE skillAOE;
    // 추후 다른 계열 등등의 스킬 매니저 추가 예정 (ノ・∀・)ノ

    [Header("시작 스킬 설정")] // 시작 스킬 ----------------------------------------------
    [Tooltip("기본 시작 스킬 넣어주세요 （〜^∇^)〜 ")]
    [SerializeField] private List<SkillData> startingSkills = new List<SkillData>();

    [Header("에디터 테스트 ")]
    [Tooltip("테스트하고 싶은 스킬 이름을 적고 ⋮ 아이콘 클릭 -> 테스트: 이 스킬 레벨업 시키기")]
    [SerializeField] private string debugSkillName = "Fireball";

    public event System.Action<string, int> OnSkillLevelChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        foreach (var skill in startingSkills)
        {
            if (skill != null)
            {
                LevelUpSkill(skill.skillName);
            }
        }
    }

    /// <summary>
    /// 스킬 이름으로 레벨업
    /// </summary>
    public bool LevelUpSkill(string skillName)
    {
        int newLevel = -1;

        if (skillShooter != null)
            newLevel = skillShooter.LevelUp(skillName);
        if (newLevel == -1 && skillAOE != null)
            newLevel = skillAOE.LevelUp(skillName);

        // 레벨업 성공 처리
        if (newLevel != -1)
        {
            OnSkillLevelChanged?.Invoke(skillName, newLevel);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 스킬 현재 레벨 반환  (UI용)
    /// </summary>
    public int GetSkillLevel(string skillName)
    {
        int level = 0;

        // 투사체 매니저에서 먼저 찾아봅니다.
        if (skillShooter != null)
            level = skillShooter.GetSkillLevel(skillName);

        // 투사체에 없었다면(0이라면) 장판 매니저를 뒤져봅니다.
        if (level == 0 && skillAOE != null)
            level = skillAOE.GetSkillLevel(skillName);

        return level;
    }

    /// <summary>
    /// 레벨업 가능한 전체 스킬 SO 목록 반환 (UI용)
    /// </summary>
    public List<SkillData> GetAllSkillData()
    {
        var list = new List<SkillData>();

        if (skillShooter != null) list.AddRange(skillShooter.GetSkillDataList());
        if (skillAOE != null) list.AddRange(skillAOE.GetSkillDataList());

        // null 제거
        list.RemoveAll(item => item == null);
        return list;
    }

    [ContextMenu("테스트: 이 스킬 레벨업 시키기")]
    public void TestLevelUpSkill()
    {
        if (Application.isPlaying)
        {
            bool success = LevelUpSkill(debugSkillName);
            if (success)
                Debug.Log($"<color=green>[디버그]</color> '{debugSkillName}' 스킬 강제 레벨업 성공");
            else
                Debug.LogWarning($"<color=red>[디버그]</color> '{debugSkillName}' 스킬 레벨업 실패 (이름 오타 확인 또는 만렙)");
        }
        else
        {
            Debug.LogWarning("게임이 실행 중일 때만 테스트할 수 있습니다");
        }
    }
}