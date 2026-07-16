using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Revolver : MonoBehaviour, IPersistentSkillEffect
{
    // 캐싱해 둘 최종 데미지
    protected float calculatedFinalDamage;

    private SkillLevelStat myStat;

    [Header("리볼버 설정")]
    [Tooltip("조준할 최대 적의 수 (기본 6발)")]
    [SerializeField] private int maxTargets = 6;
    [Tooltip("적 머리 위에 띄울 조준경 UI 프리팹")]
    [SerializeField] private GameObject crosshairPrefab;

    [Header("타이밍 설정 (초)")]
    [Tooltip("다음 표식을 띄울 때까지의 간격")]
    [SerializeField] private float aimInterval = 0.2f;
    [Tooltip("조준을 모두 마치고 사격하기 전 대기 시간")]
    [SerializeField] private float waitBeforeShoot = 1.0f;
    [Tooltip("사격 간격")]
    [SerializeField] private float shootInterval = 0.2f;

    private List<Enemy> lockedTargets = new List<Enemy>();
    private List<GameObject> spawnedCrosshairs = new List<GameObject>();

    public void Initialize(SkillLevelStat stat)
    {
        myStat = stat;

        calculatedFinalDamage = Player.Instance.Stat.CalculateFinalDamage(
            myStat.damage,
            myStat.coolTime,
            myStat.fireRate
        );

        lockedTargets.Clear();
        spawnedCrosshairs.Clear();
        StartCoroutine(RevolverRoutine());
    }

    /// <summary>
    /// 레벨업 시 호출됨. 예전엔 Initialize()를 재호출해서 코루틴이 중복으로 또 시작되는
    /// 버그가 있었음 (리볼버가 2배, 4배... 속도로 발사되는 원인). 스탯만 갱신하도록 수정.
    /// </summary>
    public void UpgradeEffect(SkillLevelStat stat)
    {
        myStat = stat;
    }

    public void OnSpawn() { }

    public void OnDespawn()
    {
        // 도중에 취소되거나 풀로 돌아갈 때 조준경 제거
        ClearCrosshairs();
        StopAllCoroutines();
    }

    private IEnumerator RevolverRoutine()
    {
        while (true)
        {
            // 유효한 적들 중 최대 maxTargets명을 무작위로 조준
            List<Enemy> validEnemies = TargetingHelper.GetRandomValidTargets(maxTargets);

            foreach (Enemy target in validEnemies)
            {
                lockedTargets.Add(target);

                if (crosshairPrefab != null)
                {
                    // 타겟의 위치에 조준경 생성 (타겟을 부모로 설정해 따라다니게 함)
                    GameObject crosshair = Instantiate(crosshairPrefab, target.transform.position, Quaternion.identity, target.transform);
                    spawnedCrosshairs.Add(crosshair);
                }

                // TODO: 조준음 재생
                yield return new WaitForSeconds(aimInterval);
            }

            // 사격 전 대기
            if (lockedTargets.Count > 0)
                yield return new WaitForSeconds(waitBeforeShoot);

            // 순차적으로 사격 개시
            for (int i = 0; i < lockedTargets.Count; i++)
            {
                Enemy target = lockedTargets[i];

                // 해당 타겟의 조준경 삭제
                if (spawnedCrosshairs.Count > i && spawnedCrosshairs[i] != null)
                {
                    Destroy(spawnedCrosshairs[i]);
                }

                // 사격하는 찰나에 적이 살아있다면 데미지 
                if (target != null && target.gameObject.activeInHierarchy && !target.IsDead)
                {
                    // TODO 총소리 재생
                    target.TakeDamage(calculatedFinalDamage, target.transform.position, 0f);
                }

                yield return new WaitForSeconds(shootInterval);
            }

            // 한 사이클 종료 후 리셋
            lockedTargets.Clear();
            spawnedCrosshairs.Clear();

            float cooldown = myStat != null && myStat.coolTime > 0 ? myStat.coolTime : 1f;
            yield return new WaitForSeconds(cooldown);
        }
    }

    private void ClearCrosshairs()
    {
        foreach (var ch in spawnedCrosshairs)
        {
            if (ch != null) Destroy(ch);
        }
        spawnedCrosshairs.Clear();
    }
}