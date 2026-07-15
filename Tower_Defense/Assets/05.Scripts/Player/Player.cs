using NUnit.Framework;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    /// <summary>
    /// 같은 프리팹에 부착된 StatManager. StatManager.Instance 대신 이걸로 접근.
    /// </summary>
    public StatManager Stat { get; private set; }

    [Header("스킬 매니저 컴포넌트 연결")]
    [SerializeField] private SkillShooter skillShooter;
    [SerializeField] private SkillAOE skillAOE;
    [SerializeField] private SkillPet skillPet;
    // [SerializeField] private SkillPassive skillPassive;

    [Header("시작 스킬 설정")]
    [Tooltip("기본 시작 스킬 ")]
    [SerializeField] private List<SkillData> startingSkills = new List<SkillData>();

    [Header("경험치 및 레벨")]
    [SerializeField] private int currentExp = 0;
    [SerializeField] private int maxExp = 100;
    [SerializeField] private int playerLevel = 1;
    public event Action<int, int> OnExpChanged;

    [Header("경험치 요구량 커브 (계단식)")]
    [Tooltip("X축: 플레이어 레벨, Y축: 해당 레벨업에 필요한 경험치 총량")]
    [SerializeField] private AnimationCurve expCurve;

    [Header("에디터 테스트 ")]
    [Tooltip("테스트하고 싶은 스킬 이름을 적고 ⋮ 아이콘 클릭 -> 테스트: 이 스킬 레벨업 시키기")]
    [SerializeField] private string debugSkillName = "Fireball";
    [Tooltip("테스트 시 단번에 올릴 목표 레벨")]
    [SerializeField] private int debugTargetLevel = 5;

    public event Action<string, int> OnSkillLevelChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        Stat = GetComponent<StatManager>();
        if (Stat == null)
        {
            Debug.LogError("[Player] StatManager 컴포넌트가 같은 오브젝트에 없습니다.");
        }
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
        // 기본 경험치 + 보너스 경험치 (ex: expGainedBonus가 0.1이면 10% 추가)
        float bonusMultiplier = 1f + Stat.GetStat(StatType.EXPGained);
        int finalAmount = Mathf.RoundToInt(amount * bonusMultiplier);

        currentExp += finalAmount;

        while (currentExp >= maxExp)
        {
            currentExp -= maxExp; // 남은 경험치 이월
            playerLevel++;

            //레벨 업 사운드 재생
            SoundManager.Instance.PlaySFX("LevelUp");
            // 렙업 후 다음 레벨의 경험치 최대치 갱신
            UpdateMaxExp();
            // 레벨업 창 띄우기
            LevelUpUIManager.Instance.ShowLevelUpUI();
        }

        OnExpChanged?.Invoke(currentExp, maxExp);
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
        // if (newLevel == -1 && skillPassive != null) newLevel = skillPassive.LevelUp(skillName);

        // 레벨업 성공 처리
        if (newLevel != -1)
        {
            OnSkillLevelChanged?.Invoke(skillName, newLevel);

            if (FusionSkillManager.Instance != null)
            {
                FusionSkillManager.Instance.CheckFusionAvailability();
            }

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
        // if (level == 0 && skillPassive != null) level = skillPassive.GetSkillLevel(skillName);
        return level;
    }

    /// <summary>
    /// 플레이어가 현재 습득한 스킬 반환
    /// </summary>
    public List<ActiveSkill> GetCurrentSkill()
    {
        List<ActiveSkill> skills = new List<ActiveSkill>();
        if (skillShooter != null) skills.AddRange(skillShooter.GetActiveSkill());
        if (skillAOE != null) skills.AddRange(skillAOE.GetActiveSkill());
        if (skillPet != null) skills.AddRange(skillPet.GetActiveSkill());
        return skills;
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

        // null 제거
        list.RemoveAll(item => item == null);

        if (FusionSkillManager.Instance != null && FusionSkillManager.Instance.allRecipes != null)
        {
            foreach (var recipe in FusionSkillManager.Instance.allRecipes)
            {
                if (recipe != null && recipe.resultFusionSkill != null)
                {
                    list.Remove(recipe.resultFusionSkill);
                }
            }
        }

        return list;
    }

    /// <summary>
    /// 융합 등으로 인해 특정 스킬을 플레이어에게서 완전히 제거
    /// </summary>
    public bool RemoveSkill(string skillName)
    {
        if (skillShooter != null && skillShooter.RemoveSkill(skillName)) return true;
        if (skillAOE != null && skillAOE.RemoveSkill(skillName)) return true;
        if (skillPet != null && skillPet.RemoveSkill(skillName)) return true;
        // if (skillPassive != null && skillPassive.RemoveSkill(skillName)) return true;

        Debug.LogWarning($"[Fusion] '{skillName}' 스킬을 지우려 했으나 찾을 수 없습니다.");
        return false;
    }

    [ContextMenu("테스트: 이 스킬 지정 레벨로 강제 세팅")]
    public void TestSetSkillLevel()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("게임이 실행 중일 때만 테스트할 수 있습니다");
            return;
        }

        int currentLevel = GetSkillLevel(debugSkillName);

        if (currentLevel >= debugTargetLevel)
        {
            Debug.Log($"<color=yellow>[디버그]</color> '{debugSkillName}' 스킬은 이미 {currentLevel}레벨 이상입니다.");
            return;
        }

        int upgradesNeeded = debugTargetLevel - currentLevel;
        bool lastUpgradeSuccess = false;

        for (int i = 0; i < upgradesNeeded; i++)
        {
            lastUpgradeSuccess = LevelUpSkill(debugSkillName);

            if (!lastUpgradeSuccess) break;
        }

        int finalLevel = GetSkillLevel(debugSkillName);

        if (finalLevel == debugTargetLevel)
            Debug.Log($"<color=green>[디버그]</color> '{debugSkillName}' 스킬 {debugTargetLevel}레벨 달성 성공! (융합 조건 체크!)");
        else
            Debug.LogWarning($"<color=red>[디버그]</color> '{debugSkillName}' 스킬 세팅 완료 (현재 레벨: {finalLevel}). 만렙이거나 이름 오타.");

        if (FusionSkillManager.Instance != null)
        {
            FusionSkillManager.Instance.CheckFusionAvailability(); // 수정된 함수 호출
        }
    }

    [ContextMenu("테스트: 이 스킬 삭제 시키기")]
    public void TestRemoveSkill()
    {
        if (Application.isPlaying)
        {
            bool success = RemoveSkill(debugSkillName);
            if (success)
                Debug.Log($"<color=yellow>[디버그]</color> '{debugSkillName}' 스킬 강제 삭제 완료!");
            else
                Debug.LogWarning($"<color=red>[디버그]</color> '{debugSkillName}' 스킬 삭제 실패 (배우지 않은 스킬이거나 이름 오타)");
        }
        else
        {
            Debug.LogWarning("게임이 실행 중일 때만 테스트할 수 있습니다");
        }
    }
}