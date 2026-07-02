using UnityEngine;

[CreateAssetMenu(fileName = "FusionSkillData", menuName = "Scriptable Objects/FusionSkillData")]
public class FusionSkillData : ScriptableObject
{
    [Header("융합 조건 (둘 다 만렙이어야 함)")]
    public SkillData requiredSkill1;
    public SkillData requiredSkill2;

    [Header("융합 결과 스킬")]
    public SkillData resultFusionSkill;

    [Header("옵션")]
    [Tooltip("체크 시 기존 스킬 2개를 삭제하고 결과 스킬로 교체, 체크 해제 시 기존 스킬은 남고 결과 스킬만 추가")]
    public bool isReplacement = true;
}
