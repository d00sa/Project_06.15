using UnityEngine;

[CreateAssetMenu(fileName = "SkillData", menuName = "Scriptable Objects/SkillData")]
public class SkillData : ScriptableObject
{
    [Header("기본 정보")]
    public string skillName = "스킬 이름";
    [TextArea] public string description = "";
    public Sprite icon;

    [Header("스킬 프리팹 (발사체 등)")]
    public GameObject skillPrefab;

    [Header("최대 레벨 설정")]
    public int maxLevel = 5;

    [Header("레벨별 스탯 (인덱스 0 = 레벨 1)")]
    public SkillLevelStat[] levelStats;

    /// <summary>현재 레벨에 맞는 스탯 반환 (1-based)</summary>
    public SkillLevelStat GetStat(int level)
    {
        int idx = Mathf.Clamp(level - 1, 0, levelStats.Length - 1);
        return levelStats[idx];
    }
}

[System.Serializable]
public class SkillLevelStat
{
    [Header("공통 스탯")]
    public float damage = 10f;
    public float fireRate = 1f;
    public float speed = 5f;
}