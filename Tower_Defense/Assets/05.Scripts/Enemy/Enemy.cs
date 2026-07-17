using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum EnemyPriority
{
    Normal = 0, // 일반 몹
    Elite = 1,  // 엘리트 몹
    Boss = 2    // 보스
}

[RequireComponent(typeof(StateMachine))]
public class Enemy : MonoBehaviour, IPoolable
{
    [SerializeField] private float hpMax;
    [SerializeField] private float _hp;
    [SerializeField] private bool boss;
    public bool IsDead;
    public event Action<Enemy> OnDead;
    public float HP
    {
        get => _hp;
        set
        {
            if (IsDead) return;

            if (value < 0)
                value = 0;

            _hp = value;
            _hpBar.value = Mathf.Clamp01(_hp / hpMax);

            if (_hp <= 0)
                OnDespawn();
        }
    }

    [SerializeField] private List<Transform> _wayPoints;
    [SerializeField] private Slider _hpBar;
    private SpriteRenderer _sprite;
    private StateMachine _machine;
    private int _currentIdx = 0;
    private int _giveExp;
    private int _giveMoney;

    public float speed = 3f;
    public bool IsMovable { get; set; } = true;

    private Coroutine dotCoroutine;
    private Coroutine stunCoroutine;
    private Coroutine slowCoroutine;
    private float originalSpeed = -1f; // 몬스터의 원래 속도 변수

    public delegate float DamageModifier(float baseDamage);
    public event DamageModifier OnCalculateBonusDamage;

    [Header("타겟팅 설정")]
    [Tooltip("우선순위")]
    public EnemyPriority priority = EnemyPriority.Normal;

    [Header("상태이상 이펙트")]
    [Tooltip("지속 데미지(DOT) 상태일 때 표시할 이펙트 오브젝트. " +
             "ApplyDotDamage를 호출하는 모든 스킬에 적용")]
    [SerializeField] private GameObject dotEffectObject;

    private void OnEnable()
    {
        OnSpawn();
    }

    private void Start()
    {
        _wayPoints = WayPointManager.Instance.wayPoints;
        _machine = GetComponent<StateMachine>();
        _sprite = GetComponent<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        if (IsDead)
            return;

        if (IsMovable)
        {
            _machine.ChangeState(StateType.Move);
            Move();
        }
        else
            _machine.ChangeState(StateType.Idle);
    }

    private void Move()
    {
        Transform target = _wayPoints[_currentIdx];

        if (_currentIdx == 0 && _sprite.flipX)
            _sprite.flipX = false;
        else if (_currentIdx == 4 && !_sprite.flipX)
            _sprite.flipX = true;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.fixedDeltaTime
        );

        if (Vector3.Distance(transform.position, target.position) < 0.2f)
        {
            _currentIdx = (_currentIdx + 1) % _wayPoints.Count;
        }
        else if (_currentIdx > 0)
        {
            Transform prevTarget = _wayPoints[_currentIdx - 1];

            Vector3 pathDir = (target.position - prevTarget.position).normalized;

            Vector3 toEnemy = transform.position - target.position;

            if (Vector3.Dot(pathDir, toEnemy) > 0f)
            {
                _currentIdx = (_currentIdx + 1) % _wayPoints.Count;
            }
        }
    }

    public void OnDespawn()
    {
        _machine.ChangeState(StateType.Dead);
        this.gameObject.tag = "Untagged";
        IsDead = true;
        IsMovable = false;
        OnDead?.Invoke(this);
        Player.Instance.AddExp(_giveExp); // 플레이어에게 경험치 넘겨줌
        SoundManager.Instance.PlaySFX("Death_Zombie");

        if (boss)
        {
            Spawner.Instance.IsBoss = false;
            UIManager.Instance.ShowStore(); //일단은 보스가 죽으면 상점이 열리도록 합시다.
        }

        // 죽으면 지속 데미지 끄기
        if (dotCoroutine != null)
        {
            StopCoroutine(dotCoroutine);
            dotCoroutine = null;
        }
        // 죽으면 스턴 끄기
        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
            stunCoroutine = null;
        }
        // 죽으면 슬로우 끄고 속도도 원래대로 되돌려놓음
        if (slowCoroutine != null)
        {
            StopCoroutine(slowCoroutine);
            slowCoroutine = null;
        }
        if (originalSpeed > 0)
        {
            speed = originalSpeed;
        }

        // 죽으면 불타는 이펙트도 꺼줌
        if (dotEffectObject != null) dotEffectObject.SetActive(false);

        OnDead = null;
        GameManager.Instance.EnemyCount--;
        GameManager.Instance.Money += Mathf.RoundToInt(_giveMoney * (1f + Player.Instance.Stat.GetStat(StatType.MoneyBonus)));
        ObjectPool.Instance.ReturnObj(this.gameObject, 2f);
    }

    public void OnSpawn()
    {
        HP = hpMax;
        IsDead = false;
        IsMovable = true;
        _currentIdx = 0;
        this.gameObject.tag = "Enemy";

        // 풀에서 재사용될 때 이전 상태의 이펙트가 켜진 채로 남아있지 않도록 리셋
        if (dotEffectObject != null) dotEffectObject.SetActive(false);
    }

    public void Setting(int exp, int money)
    {
        _giveExp = exp;
        _giveMoney = money;
    }

    /// <summary> 데미지 주고 피격 방향 반대로 살짝 밈 </summary>
    /// <param name="damage">받을 데미지량(damage가 넘어올 때 스탯의 보너스 데미지 이미 적용되어 있는 상태임)</param>
    /// <param name="attackPos">공격의 중심점 (투사체 위치 or area 중심 위치)</param>
    /// <param name="knockbackPower">밀려날 거리</param>
    public void TakeDamage(float damage, Vector3 attackPos, float knockbackPower)
    {
        if (IsDead) return;

        float finalDamage = damage;

        bool isCritical = false;
        if (Player.Instance != null && Player.Instance.Stat != null)
        {
            finalDamage = Player.Instance.Stat.RollCriticalDamage(finalDamage, out isCritical);
        }

        // Curse 스킬 전용 스탯 관련 아님!!!!!
        if (OnCalculateBonusDamage != null)
        {
            foreach (DamageModifier modifier in OnCalculateBonusDamage.GetInvocationList())
            {
                finalDamage += modifier(damage);
            }
        }

        HP -= finalDamage;
        // 데미지 텍스트 플로팅
        DamageTextManager.Instance.ShowDamage(finalDamage, transform.position);

        if (!IsMovable) return;

        Transform targetWaypoint = _wayPoints[_currentIdx];
        Vector3 pushDir = (transform.position - targetWaypoint.position).normalized;

        //밀기
        transform.position += pushDir * knockbackPower;
    }

    /// <summary> 지속 데미지(화상, 독)를 부여 </summary>
    public void ApplyDotDamage(float damage, float duration, float tickRate, float knockbackPower)
    {
        if (IsDead) return;

        // 이미 지속데미지에 걸려있다면 기존 데미지를 멈추고 시간을 초기화
        if (dotCoroutine != null)
        {
            StopCoroutine(dotCoroutine);
        }

        // 이미 켜져있으면 깜빡이지 않고 그대로 유지, 안 켜져있으면 켜줌
        if (dotEffectObject != null) dotEffectObject.SetActive(true);

        // 새로운 데미지 코루틴 시작
        dotCoroutine = StartCoroutine(DotRoutine(damage, duration, tickRate, knockbackPower));
    }

    private IEnumerator DotRoutine(float damage, float duration, float tickRate, float knockbackPower)
    {

        float elapsed = 0f;

        while (elapsed < duration)
        {
            yield return new WaitForSeconds(tickRate);

            TakeDamage(damage, transform.position, knockbackPower);

            elapsed += tickRate;
        }


        if (dotEffectObject != null) dotEffectObject.SetActive(false);
        dotCoroutine = null;
    }

    /// <summary> 적의 이동 일정시간 정지</summary>
    public void ApplyStun(float duration)
    {
        if (IsDead) return;

        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
        }

        stunCoroutine = StartCoroutine(StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        IsMovable = false;

        yield return new WaitForSeconds(duration);

        if (!IsDead)
        {
            IsMovable = true;
        }
    }

    /// <summary> 적의 이동 속도를 일정 시간 동안 감소 </summary>

    public void ApplySlow(float slowPercentage, float duration)
    {
        if (IsDead) return;

        if (originalSpeed < 0)
            originalSpeed = speed;

        if (slowCoroutine != null)
            StopCoroutine(slowCoroutine);

        slowCoroutine = StartCoroutine(SlowRoutine(slowPercentage, duration));
    }

    private IEnumerator SlowRoutine(float slowPercentage, float duration)
    {
        speed = originalSpeed * (1f - slowPercentage);

        yield return new WaitForSeconds(duration);

        if (!IsDead)
        {
            speed = originalSpeed;
            slowCoroutine = null;
        }
    }
}