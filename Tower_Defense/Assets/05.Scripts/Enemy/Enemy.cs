using Mono.Cecil.Cil;
using NaughtyAttributes;
using Space;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(StateMachine))]
public class Enemy : MonoBehaviour, IPoolable
{
    [SerializeField] private float hpMax;
    [SerializeField] private float _hp;
    private bool _isDead;
    public float HP
    {
        get => _hp;
        set
        {
            if (value < 0)
                value = 0;

            _hp = value;
            _hpBar.value = Mathf.Clamp01(_hp / hpMax);

            if (_hp <= 0)
            {
                Player.Instance.AddExp(_giveExp); // 플레이어에게 경험치 넘겨줌
                OnDespawn();
            }
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
        HP -= damage;

        Transform targetWaypoint = _wayPoints[_currentIdx];

        Vector3 pushDir = (transform.position - targetWaypoint.position).normalized;

        //밀기
        transform.position += pushDir * knockbackPower;
    }
}
