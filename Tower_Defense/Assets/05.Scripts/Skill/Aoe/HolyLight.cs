using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HolyLight : AoeEffect
{
    [Header("신성한 빛 (Holy Light) 연출")]
    [Tooltip("투명도를 조절할 십자가의 SpriteRenderer")]
    [SerializeField] private SpriteRenderer crossRenderer;

    [Tooltip("SimpleFlash 스크립트가 붙어있는 화면 번쩍임용 오브젝트")]
    [SerializeField] private GameObject flashVisual;

    [Tooltip("십자가가 서서히 나타나는 데 걸리는 시간")]
    [SerializeField] private float fadeInTime = 0.5f;

    [Tooltip("십자가가 완전히 나타난 후 번쩍이기 전까지 대기 시간")]
    [SerializeField] private float waitBeforeFlash = 0.2f;

    [Tooltip("번쩍인 후 십자가가 서서히 사라지는 데 걸리는 시간")]
    [SerializeField] private float fadeOutTime = 0.5f;

    private bool isCasting = false;

    public override void Initialize(SkillEffectContext ctx)
    {
        base.Initialize(ctx);
        StartCoroutine(HolyLightRoutine());
    }

    public override void OnSpawn()
    {
        base.OnSpawn();
        isCasting = false;
    }

    protected override void Update()
    {
        if (myStat == null) return;

        if (isCasting) return;

        base.Update();
    }

    // 스킬 연출 코루틴
    private IEnumerator HolyLightRoutine()
    {
        isCasting = true;

        if (crossRenderer != null)
        {
            crossRenderer.gameObject.SetActive(true);

            Color c = crossRenderer.color;
            c.a = 0f;
            crossRenderer.color = c;

            float t = 0f;
            while (t < fadeInTime)
            {
                t += Time.deltaTime;
                c.a = Mathf.Lerp(0f, 1f, t / fadeInTime);
                crossRenderer.color = c;
                yield return null;
            }
            c.a = 1f;
            crossRenderer.color = c;
        }

        yield return new WaitForSeconds(waitBeforeFlash);

        if (flashVisual != null)
        {
            flashVisual.SetActive(false);
            flashVisual.SetActive(true);  
        }

        // 사운드 재생
        SoundManager.Instance.PlaySFX("HolyFlash");

        ExecuteHolyKill();

        if (crossRenderer != null)
        {
            Color c = crossRenderer.color;
            float t = 0f;
            while (t < fadeOutTime)
            {
                t += Time.deltaTime;
                c.a = Mathf.Lerp(1f, 0f, t / fadeOutTime);
                crossRenderer.color = c;
                yield return null;
            }
            c.a = 0f;
            crossRenderer.color = c;
            crossRenderer.gameObject.SetActive(false);
        }

        isCasting = false;

        ObjectPool.Instance.ReturnObj(gameObject);
    }

    private void ExecuteHolyKill()
    {
        List<Enemy> allEnemies = ObjectPool.Instance.GetEnemy();

        for (int i = allEnemies.Count - 1; i >= 0; i--)
        {
            Enemy enemy = allEnemies[i];

            if (enemy.gameObject.activeInHierarchy && enemy.priority == EnemyPriority.Normal)
            {
                enemy.TakeDamage(999999f, transform.position, 0f);
            }
        }
    }
}
