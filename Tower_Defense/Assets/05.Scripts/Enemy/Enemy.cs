using Mono.Cecil.Cil;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
            _slider.value = Mathf.Clamp01(_hp / hpMax);

            if (_hp > 0) {
                //히트 애니메이션을 여기에 하면 좋을 듯?
            }
            else
                OnDespawn();
        }
    }
    public float speed = 3f;
    public bool IsMovable;

    [SerializeField] private List<Transform> _wayPoints;
    [SerializeField] private Slider _slider;
    private int _currentIdx = 0;
    private int _giveExp;

    private void OnEnable()
    {
        OnSpawn();
    }

    private void Start()
    {
        _wayPoints = WayPointManager.Instance.wayPoints;
    }

    private void FixedUpdate()
    {
        if (_isDead) 
            return;

        if (IsMovable)
            Move();
    }

    private void Move()
    {
        Transform target = _wayPoints[_currentIdx];

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
        //todo : dead Animations 
        GameManager.Instance.EnemyCount--;
        this.gameObject.tag = "Untagged";
        _isDead = true;
        IsMovable = false;
        ObjectPool.Instance.ReturnObj(this.gameObject, 1f);
    }

    public void OnSpawn()
    {
        _hp = hpMax;
        _isDead = false;
        IsMovable = true;
        _currentIdx = 0;
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
