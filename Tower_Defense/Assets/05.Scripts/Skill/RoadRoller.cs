using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class RoadRoller : MonoBehaviour, IPoolable
{
    [Header("[설정]")]
    [SerializeField] LayerMask _targetLayer;
    [SerializeField] float _minTime;
    [SerializeField] float _maxTime;
    SpriteRenderer _sprite;
    SkillLevelStat _stat;

    List<Transform> _wayPoints;
    private HashSet<Enemy> _hitEnemies = new();
    private float _timer;
    private float _nextPlayTime;
    int _currentIdx = 0;

    private void Start()
    {
        _wayPoints = WayPointManager.Instance.wayPoints;
        _sprite = GetComponent<SpriteRenderer>();
        _nextPlayTime = Random.Range(_minTime, _maxTime);
        transform.position = _wayPoints[_currentIdx].position;
    }

    public void OnDespawn()
    {
        _hitEnemies.Clear();
    }

    public void OnSpawn()
    {
        _hitEnemies.Clear();
        _currentIdx = 0;
    }

    public void Initialize(SkillLevelStat stat)
    {
        _stat = stat;
    }

    private void Update()
    {
        _timer += Time.deltaTime;

        if (_timer >= _nextPlayTime) {
            SoundManager.Instance.PlaySFX("RoadRoller");

            _timer = 0f;
            _nextPlayTime = Random.Range(_minTime, _maxTime);
        }
    }

    private void FixedUpdate()
    {
        Transform target = _wayPoints[_currentIdx];

        if (_currentIdx == 0 && !_sprite.flipX)
            _sprite.flipX = true;
        else if (_currentIdx == 4 && _sprite.flipX)
            _sprite.flipX = false;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            _stat.speed * Time.fixedDeltaTime
        );

        if (Vector3.Distance(transform.position, target.position) < 0.1f)
            _currentIdx = (_currentIdx + 1) % _wayPoints.Count;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((_targetLayer.value & (1 << other.gameObject.layer)) == 0)
            return;

        if (!other.TryGetComponent<Enemy>(out var enemy))
            return;

        if (enemy.IsDead) {
            _hitEnemies.Remove(enemy);
            return;
        }

        if (_hitEnemies.Contains(enemy))
            return;

        _hitEnemies.Add(enemy);
        
        for (int i = 0; i < _stat.fireRate; i++)
            enemy.TakeDamage(_stat.damage, transform.position, 0f);
    }

    private void OnTriggerExit2D(Collider2D other)
    {        
        if (other.TryGetComponent<Enemy>(out var enemy))
            _hitEnemies.Remove(enemy);
    }
}
