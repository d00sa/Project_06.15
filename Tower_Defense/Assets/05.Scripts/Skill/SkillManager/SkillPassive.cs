using System;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

public class SkillPassive : SkillBase
{
    [Header("[패시브] 스킬 목록")]
    [SerializeField] private List<SkillData> passiveSkills = new List<SkillData>();
    private void Awake()
    {
        base.skillDataList = passiveSkills;
    }

    protected override void Update()
    {        
        //아직 쓸 일 없음.
    }

    protected override void Execute(ActiveSkill skill)
    {
        if (skill.data.skillPrefab == null) 
            return;

        Transform tr = WayPointManager.Instance.wayPoints[0];
        GameObject obj = ObjectPool.Instance.GetObj(skill.data.skillPrefab.name, tr.position);


        if (obj.TryGetComponent<RoadRoller>(out var road)) {
            road.Initialize(skill.CurrentStat);
            SoundManager.Instance.PlaySFX("RoadRoller");
        }
    }
}
