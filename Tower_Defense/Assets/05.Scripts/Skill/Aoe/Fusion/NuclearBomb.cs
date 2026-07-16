using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NuclearBomb : AoeEffect
{

    [Header("핵폭탄 특수 옵션")]
    //[SerializeField] private bool instaKillNormalOnly = false;
    [SerializeField] private bool isNukeFusion = true;

    [Header("핵폭탄 낙하 연출")]
    [SerializeField] private Transform bombVisual;
    [SerializeField] private float dropHeight = 15f;
    [SerializeField] private float dropDuration = 1f;
    [SerializeField] private GameObject explosionVisual;

    [Tooltip("SimpleFlash 스크립트가 붙어있는 화면 번쩍임용 오브젝트")]
    [SerializeField] private GameObject flashVisual;

    private bool isDropping = false;

    public override void Initialize(SkillEffectContext ctx)
    {
        base.Initialize(ctx);

        if (isNukeFusion)
        {
            StartCoroutine(DropNukeRoutine());
        }
        //else if (instaKillNormalOnly)
        //{
        //    ExecuteInstaKill();
        //}
    }

    public override void OnSpawn()
    {
        base.OnSpawn();
        isDropping = false;
    }

    protected override void Update()
    {
        if (myStat == null) return;

        // 떨어지는 중이면 부모의 Update 로직 스톱
        if (isDropping) return;

        // 떨어지는 게 끝났으면 부모 클래스의 기존 Update 로직 실행
        base.Update();
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        //if (instaKillNormalOnly) return;
        base.OnTriggerEnter2D(collision);
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        //if (instaKillNormalOnly) return;
        base.OnTriggerExit2D(collision);
    }

    // 핵폭탄 낙하 코루틴
    private IEnumerator DropNukeRoutine()
    {
        isDropping = true;

        if (explosionVisual != null) explosionVisual.SetActive(false);

        if (bombVisual != null)
        {
            bombVisual.gameObject.SetActive(true);
            Vector3 startPos = transform.position + new Vector3(0f, dropHeight, 0f);
            Vector3 endPos = transform.position;
            float elapsedTime = 0f;

            while (elapsedTime < dropDuration)
            {
                bombVisual.position = Vector3.Lerp(startPos, endPos, elapsedTime / dropDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            bombVisual.position = endPos;
            bombVisual.gameObject.SetActive(false);
        }

        if (flashVisual != null)
        {
            flashVisual.SetActive(false);
            flashVisual.SetActive(true);  
        }

        if (explosionVisual != null) explosionVisual.SetActive(true);
        SoundManager.Instance.PlaySFX("Explosion");

        ExecuteNukeDamage();
        isDropping = false;
    }

    private void ExecuteNukeDamage()
    {
        List<Enemy> allEnemies = ObjectPool.Instance.GetEnemy();
        float tickRate = myStat.fireRate > 0f ? 1f / myStat.fireRate : 1f;
        float finalDamage = calculatedFinalDamage;

        for (int i = allEnemies.Count - 1; i >= 0; i--)
        {
            Enemy enemy = allEnemies[i];
            if (enemy.gameObject.activeInHierarchy)
            {
                if (enemy.priority == EnemyPriority.Normal)
                {
                    enemy.TakeDamage(999999f, transform.position, 0f);
                }
                else
                {
                    enemy.TakeDamage(finalDamage, transform.position, 0f);
                    if (!enemy.IsDead)
                    {
                        enemy.ApplyDotDamage(finalDamage * 0.5f, myStat.Duration, tickRate, 0f);
                    }
                }
            }
        }
    }

    private void ExecuteInstaKill()
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