using UnityEngine;

/// <summary>
/// 모든 스킬에 사용할 추상 베이스
/// 발사체는 하위 클래스에서 구현 (아마?)
/// </summary>
/// 
public abstract class SkillBase : MonoBehaviour
{
    protected SkillData data;
    protected int currentLevel = 0; // 0 = 미습득
    protected float fireTimer = 0f;
    
    public int CurrentLevel => currentLevel;
    public bool IsUnlocked => currentLevel > 0;
    public SkillData Data => data;

    public virtual void Initialize(SkillData skillData)
    {
        data = skillData;
    }

    /// <summary>스킬 레벨 업 (함수 이름이 곧 설명)</summary>
    public virtual bool LevelUp()
    {
        if (data == null) return false;
        if (currentLevel >= data.maxLevel) return false;

        currentLevel++;
        OnLevelUp(currentLevel);
        return true;
    }

    /// <summary>레벨 업 시 호출 (이펙트나 이것저것?)</summary>
    protected virtual void OnLevelUp(int newLevel) { }

    /// <summary>현재 레벨 스탯 (미습득이면 null값)</summary>
    protected SkillLevelStat CurrentStat =>
        IsUnlocked ? data.GetStat(currentLevel) : null;

    protected virtual void Update()
    {
        if (!IsUnlocked) return;
        fireTimer += Time.deltaTime;

        float interval = CurrentStat != null ? 1f / CurrentStat.fireRate : float.MaxValue;

        if (fireTimer >= interval)
        {
            fireTimer = 0f;
            Execute();
        }
    }

    /// <summary>매 공격 주기마다 실행 (뭐 발사체 생성이나 이것저것?)</summary>
    protected abstract void Execute();
}
