using System.Collections.Generic;
using UnityEngine;

public abstract class SkillBase : MonoBehaviour
{
    protected class ActiveSkill
    {
        public SkillData data;
        public int level;
        public float fireTimer;

        public SkillLevelStat CurrentStat => data.GetStat(level);
    }

    // 스킬 데이터 원본 리스트
    protected List<SkillData> skillDataList = new List<SkillData>();

    // 현재 플레이어가 습득한 스킬 상태 리스트
    protected List<ActiveSkill> activeSkills = new List<ActiveSkill>();

    protected virtual float GetInterval(ActiveSkill skill)
    {
        // coolTime이 있으면 그걸 쓰고, 없으면 연사속도(1/fireRate)를 사용
        return skill.CurrentStat.coolTime > 0f
            ? skill.CurrentStat.coolTime
            : (1f / skill.CurrentStat.fireRate);
    }

    protected virtual void Update()
    {
        // 각 활성 스킬의 발사 타이머 업데이트 및 발사 실행
        foreach (var skill in activeSkills)
        {
            skill.fireTimer += Time.deltaTime;

            float interval = GetInterval(skill);

            if (skill.fireTimer >= interval)
            {
                skill.fireTimer -= interval;
                Execute(skill);
            }
        }
    }

    public int LevelUp(string skillName)
    {
        // 인스펙터에서 꽂아준 skillDataList 검색 로직
        SkillData targetData = skillDataList.Find(x => x != null && x.skillName == skillName);
        if (targetData == null) return -1;

        ActiveSkill existingSkill = activeSkills.Find(x => x.data == targetData);

        if (existingSkill != null)
        {
            if (existingSkill.level >= targetData.maxLevel) return -1;
            existingSkill.level++;
            OnLevelUp(existingSkill);
            return existingSkill.level;
        }
        else
        {
            ActiveSkill newSkill = new ActiveSkill { data = targetData, level = 1, fireTimer = 0f };
            activeSkills.Add(newSkill);
            OnLevelUp(newSkill);

            Execute(newSkill);

            return 1;
        }
    }

    /// <summary>
    /// 특정 스킬의 현재 레벨을 반환합니다. 안 배운 스킬이면 0을 반환합니다.
    /// </summary>
    public int GetSkillLevel(string skillName)
    {
        ActiveSkill existingSkill = activeSkills.Find(x => x.data != null && x.data.skillName == skillName);

        if (existingSkill != null)
            return existingSkill.level; // 배운 스킬이면 현재 레벨 반환

        return 0; // 아직 한 번도 안 배운 스킬
    }

    /// <summary>
    /// 레벨업 시 추가 효과 구현(아마 사운드나 아마 이펙트나 그런거?)
    /// </summary>
    protected virtual void OnLevelUp(ActiveSkill skill) { }

    protected abstract void Execute(ActiveSkill skill);

    // ui 용
    public List<SkillData> GetSkillDataList() => skillDataList;

    /// <summary>
    /// 현재 배우고 있는 스킬들의 이름 목록만 반환
    /// </summary>
    public List<string> GetActiveSkillNames()
    {
        List<string> names = new List<string>();
        foreach (var skill in activeSkills)
        {
            names.Add(skill.data.skillName);
        }
        return names;
    }

    /// <summary>
    /// 습득한 스킬을 전부 초기화
    /// </summary>
    public void ClearAllSkills()
    {
        activeSkills.Clear();
    }
}