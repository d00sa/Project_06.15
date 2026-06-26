using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    [Header("스킬 매니저 컴포넌트 연결")]
    [SerializeField] private SkillShooter skillShooter;
    [SerializeField] private SkillAOE skillAOE;
    [SerializeField] private SkillPet skillPet;
    [SerializeField] private SkillPassive skillPassive;

    [Header("시작 스킬 설정")]
    [Tooltip("기본 시작 스킬 넣어주세요 （〜^∇^)〜 ")]
    [SerializeField] private List<SkillData> startingSkills = new List<SkillData>();

    [Header("에디터 테스트 ")]
    [Tooltip("테스트하고 싶은 스킬 이름을 적고 ⋮ 아이콘 클릭 -> 테스트: 이 스킬 레벨업 시키기")]
    [SerializeField] private string debugSkillName = "Fireball";

    [Header("경험치 및 레벨")]
    [SerializeField] private int currentExp = 0;
    [SerializeField] private int maxExp = 100;
    [SerializeField] private int playerLevel = 1;

    [Header("경험치 요구량 커브 (계단식)")]
    [Tooltip("X축: 플레이어 레벨, Y축: 해당 레벨업에 필요한 경험치 총량")]
    [SerializeField] private AnimationCurve expCurve;

    public event System.Action<string, int> OnSkillLevelChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        UpdateMaxExp();

        foreach (var skill in startingSkills)
        {
            if (skill != null)
            {
                LevelUpSkill(skill.skillName);
            }
        }
    }

    /// <summary>
    /// 적이 죽을 때 호출하여 경험치를 획득
    /// </summary>
    public void AddExp(int amount)
    {
        currentExp += amount;

        while (currentExp >= maxExp)
        {
            currentExp -= maxExp; // 남은 경험치 이월
            playerLevel++;

            // 렙업 후 다음 레벨의 경험치 최대치 갱신
            UpdateMaxExp();

            // 레벨업 창 띄우기
            LevelUpUIManager.Instance.ShowLevelUpUI();
        }
    }

    /// <summary>
    /// AnimationCurve를 바탕으로 현재 레벨에 맞는 최대 경험치를 계산
    /// </summary>
    private void UpdateMaxExp()
    {
        if (expCurve != null && expCurve.keys.Length > 0)
        {
            maxExp = Mathf.Max(1, Mathf.RoundToInt(expCurve.Evaluate(playerLevel)));
        }
    }

    /// <summary>
    /// 스킬 이름으로 레벨업
    /// </summary>
    public bool LevelUpSkill(string skillName)
    {
        int newLevel = -1;

        if (skillShooter != null) newLevel = skillShooter.LevelUp(skillName);
        if (newLevel == -1 && skillAOE != null) newLevel = skillAOE.LevelUp(skillName);
        if (newLevel == -1 && skillPet != null) newLevel = skillPet.LevelUp(skillName);
        if (newLevel == -1 && skillPassive != null) newLevel = skillPassive.LevelUp(skillName);

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

        if (skillShooter != null) level = skillShooter.GetSkillLevel(skillName);
        if (level == 0 && skillAOE != null) level = skillAOE.GetSkillLevel(skillName);
        if (level == 0 && skillPet != null) level = skillPet.GetSkillLevel(skillName);
        if (level == 0 && skillPet != null) level = skillPassive.GetSkillLevel(skillName);
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
        if (skillPet != null) list.AddRange(skillPet.GetSkillDataList());
        if (skillPassive != null) list.AddRange(skillPassive.GetSkillDataList());

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