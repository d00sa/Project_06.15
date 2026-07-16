using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class RoadRoller : Pet
{
    // 캐싱해 둘 최종 데미지
    protected float calculatedFinalDamage;

    [Header("[설정]")]
    [SerializeField] LayerMask _targetLayer;
    [SerializeField] float _minTime;
    [SerializeField] float _maxTime;


    SpriteRenderer _sprite;
    List<Transform> _wayPoints;
    private HashSet<Enemy> _hitEnemies = new();

    private float _timer;
    private float _nextPlayTime;
    int _currentIdx = 0;

    private void Awake()
    {
        _sprite = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        _nextPlayTime = Random.Range(_minTime, _maxTime);

        transform.SetParent(null);
        //transform.localScale = Vector3.one;

        if (WayPointManager.Instance != null)
            _wayPoints = WayPointManager.Instance.wayPoints;

        if (_wayPoints != null && _wayPoints.Count > 0)
            transform.position = _wayPoints[_currentIdx].position;
    }

    public override void Initialize(SkillLevelStat stat)
    {
        currentPetStat = stat;

        calculatedFinalDamage = Player.Instance.Stat.CalculateFinalDamage(
            currentPetStat.damage,
            currentPetStat.coolTime,
            currentPetStat.fireRate
        );

        _hitEnemies.Clear();

        if (_wayPoints == null && WayPointManager.Instance != null)
            _wayPoints = WayPointManager.Instance.wayPoints;

        if (_wayPoints != null && _wayPoints.Count > 0)
        {
            _currentIdx = _wayPoints.Count - 1;
            transform.position = _wayPoints[_currentIdx].position;
        }
    }

    public override void UpgradePet(int level, SkillLevelStat stat)
    {
        currentPetStat = stat;
    }

    private void Update()
    {
        if (currentPetStat == null) return;

        _timer += Time.deltaTime;

        if (_timer >= _nextPlayTime)
        {
            SoundManager.Instance.PlaySFX("RoadRoller");
            _timer = 0f;
            _nextPlayTime = Random.Range(_minTime, _maxTime);
        }
    }

    private void FixedUpdate()
    {
        if (currentPetStat == null || _wayPoints == null || _wayPoints.Count == 0) return;

        Transform target = _wayPoints[_currentIdx];

        float xDiff = target.position.x - transform.position.x;
        if (xDiff > 0.05f)
            _sprite.flipX = true;
        else if (xDiff < -0.05f)
            _sprite.flipX = false;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            currentPetStat.speed * Time.fixedDeltaTime
        );

        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            _currentIdx--;

            if (_currentIdx < 0)
            {
                _currentIdx = _wayPoints.Count - 1;
                _hitEnemies.Clear(); // 타격 기록 초기화
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (currentPetStat == null) return;

        if ((_targetLayer.value & (1 << other.gameObject.layer)) == 0)
            return;

        if (!other.TryGetComponent<Enemy>(out var enemy))
            return;

        if (!enemy.gameObject.activeInHierarchy)
        {
            _hitEnemies.Remove(enemy);
            return;
        }

        if (_hitEnemies.Contains(enemy))
            return;

        _hitEnemies.Add(enemy);

        for (int i = 0; i < currentPetStat.fireRate; i++)
            enemy.TakeDamage(calculatedFinalDamage, transform.position, 0f);
    }

    private void OnTriggerExit2D(Collider2D other)
    {        
        if (other.TryGetComponent<Enemy>(out var enemy))
            _hitEnemies.Remove(enemy);
    }
}
