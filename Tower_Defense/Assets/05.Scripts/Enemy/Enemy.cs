using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IPoolable
{
    private float _hp;
    public float HP
    {
        get => _hp;
        set
        {
            if (_hp - value <= 0) {
                _hp = 0;
                //todo : die
            }
            _hp -= value;
            //todo : 피통 조절
        }
    }

    public float speed = 3f;

    [SerializeField] private List<Transform> _wayPoints;
    private int _currentIdx = 0;


    private void Awake()
    {
        _hp = 100f;
    }

    private void Start()
    {
        _wayPoints = WayPointManager.Instance.wayPoints;
    }

    private void FixedUpdate()
    {
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

    }

    public void OnSpawn()
    {
        _currentIdx = 0;
        _hp = 100f;
    }
}
