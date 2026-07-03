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

    [Tooltip("초당 공격 횟수 / 틱(Tick) 데미지 주기")]
    public float fireRate = 1f;

    [Tooltip("발사체: 이동 속도 / 장판, 폭설: 지속 시간")]
    public float speed = 5f;

    [Tooltip("재사용 대기시간 (0이면 fireRate 기준)")]
    public float coolTime = 1f;

    [Tooltip("탐색/폭발 반경 및 사거리")]
    public float range = 5f;

    // 문맥별 별칭 (내가 짰지만 내가 헷갈려서,,)

    public float Duration => speed;          // AOE, HeavySnow 지속 시간
    public float ProjectileSpeed => speed;   // Shooter 투사체 속도
    public float DamageMultiplier => damage; // 저주(Curse) 등 데미지 배율
}