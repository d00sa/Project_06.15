using System.Collections;
using Space;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(StateMachine))]
public class Enemy : MonoBehaviour, IPoolable
{
    [SerializeField] private float hpMax;
    [SerializeField] private float _hp;
    [SerializeField] private bool boss;
    private bool _isDead;
    public float HP
    {
        get => _hp;
        set
        {
            if (_isDead) return;

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

    public float speed = 3f;
    public bool IsMovable { get; set; } = true;

    private Coroutine dotCoroutine;
    private Coroutine stunCoroutine;

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
        if (_isDead) 
            return;

        if (IsMovable) {
            _machine.ChangeState(StateType.Move);
            Move();
        }
        else
            _machine.ChangeState(StateType.Idle);
    }

    private void Move()
    {
        Transform target = _wayPoints[_currentIdx];

        if (_currentIdx == 0 && !_sprite.flipX)
            _sprite.flipX = true;
        else if (_currentIdx == 4 && _sprite.flipX)
            _sprite.flipX = false;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.fixedDeltaTime
        );

        if (Vector3.Distance(transform.position, target.position) < 0.1f)
            _currentIdx = (_currentIdx + 1) % _wayPoints.Count;
    }

    public void OnDespawn()
    {
        _machine.ChangeState(StateType.Dead);
        this.gameObject.tag = "Untagged";
        _isDead = true;
        IsMovable = false;
        Player.Instance.AddExp(_giveExp); // 플레이어에게 경험치 넘겨줌
        SoundManager.Instance.PlaySFX("Death_Zombie");

        if (boss)
            Spawner.Instance.IsBoss = false;

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

        GameManager.Instance.EnemyCount--;
        ObjectPool.Instance.ReturnObj(this.gameObject, 2f);
    }

    public void OnSpawn()
    {
        HP = hpMax;
        _isDead = false;
        IsMovable = true;
        _currentIdx = 0;
        this.gameObject.tag = "Enemy";
    }

    public void Setting(int exp)
    {
        _giveExp = exp;
    }

    /// <summary>
    /// 데미지 주고 피격 방향 반대로 살짝 밈
    /// </summary>
    /// <param name="damage">받을 데미지량</param>
    /// <param name="attackPos">공격의 중심점 (투사체 위치 or area 중심 위치)</param>
    /// <param name="knockbackPower">밀려날 거리</param>
    public void TakeDamage(float damage, Vector3 attackPos, float knockbackPower)
    {
        if (_isDead) return;

        HP -= damage;
        // 데미지 텍스트 플로팅
        DamageTextManager.Instance.ShowDamage(damage, transform.position);

        Transform targetWaypoint = _wayPoints[_currentIdx];
        Vector3 pushDir = (transform.position - targetWaypoint.position).normalized;

        //밀기
        transform.position += pushDir * knockbackPower;
    }

    /// <summary>
    /// 지속 데미지(화상, 독)를 부여
    /// </summary>
    public void ApplyDotDamage(float damage, float duration, float tickRate, float knockbackPower)
    {
        if (_isDead) return;

        // 이미 지속데미지에 걸려있다면 기존 데미지를 멈추고 시간을 초기화
        if (dotCoroutine != null)
        {
            StopCoroutine(dotCoroutine);
        }
        
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
    }

    /// <summary>
    /// 적의 이동 일정시간 정지
    /// </summary>
    public void ApplyStun(float duration)
    {
        if (_isDead) return;

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

        if (!_isDead)
        {
            IsMovable = true;
        }
    }
}
