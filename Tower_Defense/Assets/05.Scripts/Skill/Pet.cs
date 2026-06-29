using System.Collections.Generic;
using UnityEngine;

public class Pet : MonoBehaviour
{
    [Header("펫 스킬 매니저")]
    [SerializeField] private SkillShooter skillShooter;
    [SerializeField] private SkillAOE skillAOE;

    [Header("펫 기본 장착 스킬")]
    [SerializeField] private List<SkillData> startingSkills = new List<SkillData>();

    protected SkillLevelStat currentPetStat;


    /// <summary>
    /// SkillPet.cs에서 펫을 최초 소환할 때 호출하는 초기화 함수
    /// </summary>
    public virtual void Initialize(SkillLevelStat stat)
    {
        currentPetStat = stat;

        foreach (var skill in startingSkills)
        {
            if (skill != null)
            {
                LevelUpPetSkill(skill.skillName);
            }
        }
    }

    /// <summary>
    /// SkillPet.cs에서 펫 레벨업을 했을 때 스탯을 갱신하기 위해 부르는 함수
    /// </summary>
    public virtual void UpgradePet(int level, SkillLevelStat stat)
    {
        currentPetStat = stat;
        // 펫 이펙트나 각 종 설정들 추가 v

        // 펫 레벨이 오를 때마다 펫이 장착한 기본 스킬들도 업그레이드
        foreach (var skill in startingSkills)
        {
            if (skill != null)
            {
                LevelUpPetSkill(skill.skillName);
            }
        }
    }

    // 내부 스킬 레벨업 로직
    public virtual bool LevelUpPetSkill(string skillName)
    {
        int newLevel = -1;

        if (skillShooter != null) newLevel = skillShooter.LevelUp(skillName);
        if (newLevel == -1 && skillAOE != null) newLevel = skillAOE.LevelUp(skillName);

        return newLevel != -1;
    }

    // 펫이 장착 중인 스킬 전부 비우기
    public void ClearPetSkills()
    {
        if (skillShooter != null) skillShooter.ClearAllSkills();
        if (skillAOE != null) skillAOE.ClearAllSkills();
    }

}