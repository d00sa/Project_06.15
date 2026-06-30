using System;
using UnityEngine;

public enum StatType
{
    ProjectileDamage,   // 투사체 데미지 증가
    AoeDamage,          // 장판 데미지 증가
    AttackSpeed,        // 공격 속도 증가 (쿨타임 감소)
    EXPGained,          // 경험치 획득량 (ex: expGainedBonus 0.1이면 10% 추가))
}

public class StatManager : MonoBehaviour
{
    public static StatManager Instance { get; private set; }

    [Header("현재 스탯 현황 (인스펙터 확인용)")]
    public float projectileDamageBonus = 0f;
    public float aoeDamageBonus = 0f;
    public float attackSpeedBonus = 0f;
    public float expGainedBonus = 0f;

    // 스탯이 변경되었을 때 UI나 다른 시스템에 알려주기 위한 이벤트
    public event Action OnStatChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// 아이템 획득 시 스탯을 올려주는 함수
    /// </summary>
    public void AddStat(StatType type, float value)
    {
        switch (type)
        {
            case StatType.ProjectileDamage: projectileDamageBonus += value; break;
            case StatType.AoeDamage: aoeDamageBonus += value; break;
            case StatType.AttackSpeed: attackSpeedBonus += value; break;
            case StatType.EXPGained: expGainedBonus += value; break;
        }

        Debug.Log($" {type} 스탯 {value}만큼 증가");

        OnStatChanged?.Invoke();
    }

}
