#define CBT_MODE
//#define RELEASE_MODE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Rand = UnityEngine.Random;

public class EnemyCtrl : MonoBehaviour, ITakeDamage
{
    [Header("ANIMATION DURATION")]
    public float idle1Duration   = 2.0f;
    public float idle2Duration   = 2.0f;
    public float aggroDuration   = 1.5f;
    public float attack2Duration = 1.0f;
    public float attack3Duration = 1.0f;

    [Header("Hit Duration")]
    public float hitStateDuration = 0.5f;
    public float hitAnimDuration  = 0.5f;

    [Header("Death")]
    public float deathDestroyDelay = 4.5f;

    [Header("공격 텀")]
    [Tooltip("한 번의 공격 패턴이 끝난 후 다음 공격 패턴이 시작되기까지의 대기 시간(초)")]
    [SerializeField] float attackCooldown = 1.5f;
    private float lastAttackEndTime = -999f;

    [Header("STATE")]
    public MODE_STATE enemyMode = MODE_STATE.IDLE;

    [Header("몬스터 인공지능")]
    [Range(0, 1000)] public int hp = 1000;
    [Range(10f, 30f)][SerializeField]  float findDist      = 20.0f;
    [Range(1f,  30f)][SerializeField]  float attackDist    = 5.0f;
    [Range(10f, 100f)][SerializeField] float hpBarShowDist = 40.0f;

    [Header("HP바 UI")]
    public Image      hpBarImage;
    public GameObject hpBarRoot;

    [Header("References")]
    public GameObject enemyBodyObject;

    // ────────────────────────────────────────────
    //  ★ 공격 패턴 프리팹 목록
    //  각 패턴은 별도 프리팹으로 관리
    //  패턴 추가 시 프리팹 만들어서 슬롯에 연결만 하면 됨
    // ────────────────────────────────────────────
    [Header("공격 패턴 프리팹")]
    [Tooltip("각 패턴을 별도 프리팹으로 연결. 랜덤으로 하나 선택해 실행됨.")]
    public GameObject[] attackPatternPrefabs;

    public enum MODE_STATE { IDLE, TRACE, SURPRISE, ATTACK, HIT, DIE }

    private Animator     _anim;
    private Transform    myTr;
    private NavMeshAgent _agent;
    public  Transform    traceTarget;   // 패턴 프리팹에서 참조할 수 있도록 public
    private Rigidbody    rbody;

    private bool  isActing       = false;
    private bool  hasPlayedAggro = false;
    private bool  isHit          = false;
    private float isHitEndTime;
    private int   maxHp;

    private float targetSearchInterval = 0.3f;
    private float lastTargetSearchTime;

    // 현재 실행 중인 패턴 인스턴스
    private GameObject _currentPatternObj;

    // 패턴 실행 중 여부 (true인 동안 플레이어 방향 회전 정지)
    private bool _isPatternRunning = false;

    // ────────────────────────────────────────────
    //  초기화
    // ────────────────────────────────────────────
    void Awake()
    {
        _anim  = GetComponentInChildren<Animator>();
        myTr   = GetComponent<Transform>();
        rbody  = GetComponent<Rigidbody>();
        _agent = GetComponent<NavMeshAgent>();

        maxHp = hp;
        UpdateHpBar();
        SetHpBarVisible(false);
    }

    // ────────────────────────────────────────────
    //  Update
    // ────────────────────────────────────────────
    void Update()
    {
        if (enemyMode == MODE_STATE.DIE) return;

        UpdateHitState();
        UpdateTarget();
        UpdateModeState();
        UpdateRotation();
        UpdateAction();
    }

    void UpdateHitState()
    {
        if (isHit && Time.time > isHitEndTime)
            isHit = false;
    }

    void UpdateTarget()
    {
        if (Time.time < lastTargetSearchTime + targetSearchInterval) return;
        lastTargetSearchTime = Time.time;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length == 0) { traceTarget = null; return; }

        Transform closest     = players[0].transform;
        float     closestDist = (closest.position - myTr.position).sqrMagnitude;

        foreach (GameObject p in players)
        {
            float d = (p.transform.position - myTr.position).sqrMagnitude;
            if (d < closestDist) { closest = p.transform; closestDist = d; }
        }

        traceTarget = closest;
    }

    void UpdateModeState()
    {
        if (isHit)
        {
            ChangeState(MODE_STATE.HIT);
            return;
        }

        if (traceTarget == null)
        {
            SetHpBarVisible(false);
            ChangeState(MODE_STATE.IDLE);
            return;
        }

        float dist = Vector3.Distance(myTr.position, traceTarget.position);
        SetHpBarVisible(dist <= hpBarShowDist);

        if (dist <= attackDist)
        {
            hasPlayedAggro = true;
            ChangeState(MODE_STATE.ATTACK);
        }
        else if (dist <= findDist)
        {
            ChangeState(hasPlayedAggro ? MODE_STATE.TRACE : MODE_STATE.SURPRISE);
        }
        else
        {
            hasPlayedAggro = false;
            ChangeState(MODE_STATE.IDLE);
        }
    }

    void ChangeState(MODE_STATE newState)
    {
        if (enemyMode == newState) return;

        // 상태 전환 시 실행 중인 패턴 정리
        if (enemyMode == MODE_STATE.ATTACK)
        {
            CleanupCurrentPattern();
            _isPatternRunning = false;
        }

        StopAllCoroutines();
        isActing  = false;
        enemyMode = newState;
    }

    void UpdateRotation()
    {
        if (traceTarget == null) return;
        if (_isPatternRunning) return; // 공격 패턴 실행 중에는 회전 고정
        if (enemyMode != MODE_STATE.TRACE && enemyMode != MODE_STATE.ATTACK) return;

        Vector3 dir = traceTarget.position - myTr.position;
        dir.y = 0;
        if (dir.sqrMagnitude < 0.001f) return;
        myTr.rotation = Quaternion.Slerp(
            myTr.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 10f);
    }

    void UpdateAction()
    {
        if (isActing) return;

        switch (enemyMode)
        {
            case MODE_STATE.IDLE:     StartCoroutine(IdleCoroutine());   break;
            case MODE_STATE.SURPRISE: StartCoroutine(AggroCoroutine());  break;
            case MODE_STATE.ATTACK:
                if (Time.time >= lastAttackEndTime + attackCooldown)
                    StartCoroutine(AttackCoroutine());
                break;
            case MODE_STATE.HIT:      StartCoroutine(HitCoroutine());    break;
            case MODE_STATE.TRACE:    UpdateTrace();                     break;
        }
    }

    void UpdateTrace()
    {
        if (_agent == null || traceTarget == null) return;
        if (!_agent.isOnNavMesh) return;

        _agent.isStopped = false;
        _agent.SetDestination(traceTarget.position);
        _anim.SetBool("IsMoving", true);
    }

    // ────────────────────────────────────────────
    //  코루틴
    // ────────────────────────────────────────────
    IEnumerator IdleCoroutine()
    {
        isActing = true;
        if (_agent != null && _agent.isOnNavMesh)
        {
            _agent.isStopped = true;
            _anim.SetBool("IsMoving", false);
        }
        int rand = Rand.Range(1, 3);
        _anim.SetTrigger("Idle" + rand);
        yield return new WaitForSeconds(rand == 1 ? idle1Duration : idle2Duration);
        isActing = false;
    }

    IEnumerator AggroCoroutine()
    {
        isActing = true;
        if (_agent != null && _agent.isOnNavMesh) _agent.isStopped = true;
        _anim.SetTrigger("Aggro");
        yield return new WaitForSeconds(aggroDuration);
        hasPlayedAggro = true;
        isActing = false;
    }

    // ────────────────────────────────────────────
    //  ★ AttackCoroutine
    //  랜덤으로 패턴 프리팹 하나 선택 → Instantiate
    //  → Execute() 실행 → 끝나면 Destroy
    // ────────────────────────────────────────────
    IEnumerator AttackCoroutine()
    {
        isActing = true;
        if (_agent != null && _agent.isOnNavMesh) _agent.isStopped = true;

        if (attackPatternPrefabs == null || attackPatternPrefabs.Length == 0)
        {
            Debug.LogWarning($"[EnemyCtrl] {gameObject.name}: 연결된 패턴 프리팹이 없습니다.");
            isActing = false;
            yield break;
        }

        // 랜덤 패턴 선택
        int index = Rand.Range(0, attackPatternPrefabs.Length);
        GameObject prefab = attackPatternPrefabs[index];

        if (prefab == null)
        {
            Debug.LogWarning($"[EnemyCtrl] {gameObject.name}: index {index} 프리팹이 비어있습니다.");
            isActing = false;
            yield break;
        }

        // 패턴 프리팹 소환
        _currentPatternObj = Instantiate(prefab, myTr.position, Quaternion.identity);

        // IAttackPattern 가져와서 실행
        IAttackPattern pattern = _currentPatternObj.GetComponent<IAttackPattern>();
        if (pattern == null)
        {
            Debug.LogWarning($"[EnemyCtrl] {prefab.name}: IAttackPattern 컴포넌트가 없습니다.");
            Destroy(_currentPatternObj);
            isActing = false;
            yield break;
        }

        // 패턴에 Enemy 정보 주입 후 실행
        pattern.SetContext(myTr, traceTarget);
        _anim.SetTrigger("Attack" + (index + 1));

        _isPatternRunning = true;
        yield return StartCoroutine(pattern.Execute());
        _isPatternRunning = false;

        // 패턴 종료 후 정리
        CleanupCurrentPattern();
        lastAttackEndTime = Time.time;
        isActing = false;
    }

    IEnumerator HitCoroutine()
    {
        isActing = true;
        _anim.SetTrigger("Hit");
        yield return new WaitForSeconds(hitAnimDuration);
        isActing = false;
    }

    // ────────────────────────────────────────────
    //  현재 패턴 정리
    // ────────────────────────────────────────────
    void CleanupCurrentPattern()
    {
        if (_currentPatternObj != null)
        {
            Destroy(_currentPatternObj);
            _currentPatternObj = null;
        }
    }

    // ────────────────────────────────────────────
    //  데미지 / HP
    // ────────────────────────────────────────────
    public void TakeDamage(int damage)
    {
        if (enemyMode == MODE_STATE.DIE) return;

        hp = Mathf.Max(hp - damage, 0);
        UpdateHpBar();

        isHit        = true;
        isHitEndTime = Time.time + hitStateDuration;

        if (hp <= 0) Die();
    }

    void UpdateHpBar()
    {
        if (hpBarImage == null) return;
        hpBarImage.fillAmount = (float)hp / maxHp;
    }

    void SetHpBarVisible(bool visible)
    {
        if (hpBarRoot != null) hpBarRoot.SetActive(visible);
    }

    // ────────────────────────────────────────────
    //  사망
    // ────────────────────────────────────────────
    void Die()
    {
        StopAllCoroutines();
        enemyMode = MODE_STATE.DIE;
        isActing  = false;
        StartCoroutine(DieCoroutine());
    }

    IEnumerator DieCoroutine()
    {
        CleanupCurrentPattern();
        _anim.SetTrigger("Die");

        gameObject.tag = "Untagged";
        if (enemyBodyObject != null)
            enemyBodyObject.tag = "Untagged";
        else
            Debug.LogWarning($"[EnemyCtrl] {gameObject.name}: enemyBodyObject가 없습니다.");

        foreach (Collider c in GetComponentsInChildren<Collider>())
            c.enabled = false;

        if (_agent != null && _agent.isOnNavMesh)
            _agent.isStopped = true;

        SetHpBarVisible(false);

        yield return new WaitForSeconds(deathDestroyDelay);
        Destroy(gameObject);
    }

    void OnDestroy() => CleanupCurrentPattern();
}

