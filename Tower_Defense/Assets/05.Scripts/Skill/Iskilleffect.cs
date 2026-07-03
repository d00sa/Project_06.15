using UnityEngine;

/// <summary>
/// 스폰되면 스스로 알아서 동작을 끝내고 풀로 돌아가는 "일회성" 스킬 이펙트.
/// (예: Projectile, BoomerangProjectile, AoeEffect, InstantAoeEffect, HeavySnow)
/// IPoolable을 상속하므로 OnSpawn/OnDespawn도 함께 구현 필
/// </summary>
public interface ISkillEffect : IPoolable
{
    void Initialize(SkillEffectContext ctx);
}
