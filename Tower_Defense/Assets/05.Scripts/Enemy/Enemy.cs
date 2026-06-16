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
            _hp = value;

            if (_hp <= 0) {
                _hp = 0;
                OnDespawn();
            }

            _slider.value = Mathf.Clamp01(_hp / hpMax);
        }
    }
    public float speed = 3f;


    [SerializeField] private List<Transform> _wayPoints;
    [SerializeField] private Slider _slider;
    private int _currentIdx = 0;


    private void Awake()
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
        this.gameObject.tag = "Untagged";
        _isDead = true;
        ObjectPool.Instance.ReturnObj(this.gameObject, 2f);
    }

    public void OnSpawn()
    {
        _hp = hpMax;
        _isDead = false;
        _currentIdx = 0;
    }
}
