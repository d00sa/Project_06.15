using System;
using System.Collections.Generic;
using UnityEngine;

public enum StatType
{
    //이 순서대로 StatText를 만들어두었으니 만약 숫자(Enum 값)를 바꾼다면 Text 인덱스 순서도 바꿔줘야함.

    AttackDamage,        // 투사체, AOE, 펫 데미지를 하나로 통합한 기본 공격력 추가값
    AttackSpeed,         // 공격 속도 증가 (쿨타임 감소)
    ProjectileSpeed,      // 투사체 속도 증가 (0.1이면 10% 증가)
    CritChance,          // 치명타 확률 (0.1 = 10%)
    CritDamageMultiplier,// 치명타 데미지 배율 (2 = 2배)
    EXPGained,           // 경험치 획득량 (0.1이면 10% 추가)
    Luck,                // 행운 (0.1이면 10% 추가, 아이템 드랍률 증가, 상자 획득시 더 높은 아이템 확률증가)
}

public class StatManager : MonoBehaviour
{
    [Serializable]
    private class StatEntry
    {
        public StatType type;
        public float value;
    }

    // 실제 저장은 Dictionary로, 인스펙터 확인용으로만 List를 같이 유지
    private Dictionary<StatType, float> stats = new Dictionary<StatType, float>();

    [Header("현재 스탯 현황 (인스펙터 확인용, 런타임에 자동 갱신됨)")]
    [SerializeField] private List<StatEntry> statEntriesView = new List<StatEntry>();

    // 스탯이 변경되었을 때를 위한 이벤트
    public event Action OnStatChanged;

    private void Awake()
    {
        // 게임 시작 시 인스펙터에 세팅해둔 값을 딕셔너리에 로드
        SyncDictionaryFromInspector();
    }

    private void OnValidate()
    {
        // 인스펙터에서 수정한 값을 즉시 딕셔너리에 반영
        SyncDictionaryFromInspector();

        // 런타임(게임 실행 중)에 값을 수정했다면 변경 이벤트도 발생시켜서 즉각 반영되도록 처리
        if (Application.isPlaying)
        {
            OnStatChanged?.Invoke();
        }
    }

    /// <summary>특정 카테고리의 현재 보정값을 반환 (없으면 0)</summary>
    public float GetStat(StatType type)
    {
        float value = 0f;
        if (stats.ContainsKey(type))
            value = stats[type];
 
            return value;
    }

    /// <summary>아이템 획득 시 스탯을 올려주는 함수</summary>
    public void AddStat(StatType type, float value)
    {
        stats.TryGetValue(type, out float current);
        stats[type] = current + value;
        Debug.Log($"{type} 스탯 {value}만큼 증가 (현재 {stats[type]})");

        SyncInspectorFromDictionary();
        OnStatChanged?.Invoke();
    }

    /// <summary>
    /// 크리티컬 여부를 판정 크리티컬이면 배율이 적용된 데미지를 반환.
    /// </summary>
    public float RollCriticalDamage(float baseDamage, out bool isCritical)
    {
        float critChance = GetStat(StatType.CritChance);

        isCritical = critChance > 0f && UnityEngine.Random.value < critChance;

        return isCritical ? baseDamage * GetStat(StatType.CritDamageMultiplier) : baseDamage;
    }

    /// <summary>
    /// 스킬의 쿨타임과 연사력을 바탕으로 효율(비율)이 적용된 최종 데미지를 계산하여 반환.
    /// </summary>
    public float CalculateFinalDamage(float baseDamage, float coolTime, float fireRate)
    {
        float bonusDamage = GetStat(StatType.AttackDamage);

        // 데미지 계수 계싼
        float effectiveness = 1f;
        if (coolTime > 0f)
        {
            effectiveness = coolTime;
        }
        else if (fireRate > 0f)
        {
            effectiveness = 1f / fireRate;
        }

        return baseDamage + (bonusDamage * effectiveness);
    }

    private void SyncDictionaryFromInspector()
    {
        stats.Clear();
        foreach (var entry in statEntriesView)
        {
            // 인스펙터에서 같은 타입을 중복으로 넣는 실수 방지
            if (!stats.ContainsKey(entry.type))
            {
                stats[entry.type] = entry.value;
            }
        }
    }

    private void SyncInspectorFromDictionary()
    {
        // 기존 인스펙터 리스트에 있던 항목들 값 업데이트 (리스트 초기화를 방지해 보기 편하게 유지)
        foreach (var entry in statEntriesView)
        {
            if (stats.TryGetValue(entry.type, out float val))
            {
                entry.value = val;
            }
        }

        // 인스펙터 리스트에 없던 새로운 스탯이 코드에서 추가되었을 경우 리스트에 추가
        foreach (var kv in stats)
        {
            if (!statEntriesView.Exists(x => x.type == kv.Key))
            {
                statEntriesView.Add(new StatEntry { type = kv.Key, value = kv.Value });
            }
        }
    }
}