using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Curse : Pet
{
    private Enemy targetEnemy;
    private SpriteRenderer[] renderers;

    // 중복 부착 방지용
    private static HashSet<Enemy> currentlyCursedEnemies = new HashSet<Enemy>();

    [Tooltip("적 머리 위로 얼마나 띄울지 결정하는 Y축 높이")]
    [SerializeField] private float yOffset = 1.5f;

    public override void Initialize(SkillLevelStat stat)
    {
        base.Initialize(stat);
        renderers = GetComponentsInChildren<SpriteRenderer>();
        StartCoroutine(CurseRoutine());
    }

    private void SetVisualActive(bool isActive)
    {
        if (renderers == null) return;
        foreach (var sr in renderers)
        {
            sr.enabled = isActive;
        }
    }

    private IEnumerator CurseRoutine()
    {
        // 시작할 때 바로 숨김
        SetVisualActive(false);

        while (true)
        {
            // 타겟 찾기 (이미 저주 걸린 적은 제외한 랜덤 적)
            targetEnemy = TargetingHelper.GetRandomValidTarget(currentlyCursedEnemies);

            if (targetEnemy == null)
            {
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            SetVisualActive(true);

            currentlyCursedEnemies.Add(targetEnemy);
            targetEnemy.OnCalculateBonusDamage += CalculateCurseDamage;

            while (targetEnemy != null && !targetEnemy.IsDead && targetEnemy.gameObject.activeInHierarchy)
            {
                transform.position = targetEnemy.transform.position + Vector3.up * yOffset;
                yield return null;
            }

            SetVisualActive(false);

            if (targetEnemy != null)
            {
                targetEnemy.OnCalculateBonusDamage -= CalculateCurseDamage;
                currentlyCursedEnemies.Remove(targetEnemy);
            }
            targetEnemy = null;

            yield return new WaitForSeconds(currentPetStat.coolTime);
        }
    }

    private float CalculateCurseDamage(float originalDamage)
    {
        if (currentPetStat == null) return 0f;
        // 스탯의 damage를 비율로 사용 (ex: DamageMultiplier 값이 0.2면 원래 데미지의 20%를 추가 데미지로 반환)
        return originalDamage * currentPetStat.DamageMultiplier;
    }

    // 펫이 레벨업으로 갱신되거나 파괴될 때 에러를 막기 위한 안전장치
    private void OnDisable()
    {
        if (targetEnemy != null)
        {
            targetEnemy.OnCalculateBonusDamage -= CalculateCurseDamage;
            currentlyCursedEnemies.Remove(targetEnemy);
            targetEnemy = null;
        }
    }
}