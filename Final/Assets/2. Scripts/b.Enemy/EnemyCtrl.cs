#define CBT_MODE
//#define RELEASE_MODE

using System.Collections;
using System.Collections.Generic;
using TMPro;      
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
//using UnityEditor;
using Rand = UnityEngine.Random;

public class EnemyCtrl : MonoBehaviour, ITakeDamage
{
    [Header("ANIMATION DURATION")]
    public float idle1Duration  = 2.0f;
    public float idle2Duration  = 2.0f;
    public float aggroDuration  = 1.5f;
    public float attack1Duration = 1.0f;
    public float attack2Duration = 1.0f;
    public float attack3Duration = 1.0f;

    [Header("Hit Duration")]
    [Tooltip("피격 무적 / 상태 유지 시간")]
    public float hitStateDuration = 0.5f;
    [Tooltip("Hit 애니메이션 재생 대기 시간")]
    public float hitAnimDuration  = 0.5f;

    [Header("STATE")]
    public MODE_STATE enemyMode = MODE_STATE.IDLE;


    [Header("몬스터 인공지능")]
    [Range(0, 1000)] public int hp = 1000;
    [Range(10f, 30f)][SerializeField] float findDist   = 20.0f;
    [Range(1f,  30f)][SerializeField] float attackDist = 5.0f;
    [Range(10f, 100f)][SerializeField] float hpBarShowDist = 40.0f;


    [Header("HP바 UI")]
    [Tooltip("HP바 Image")]
    public Image    hpBarImage;
    [Tooltip("HP바 루트 오브젝트")]
    public GameObject hpBarRoot;


    [Header("Attack1 패턴 - 장판")]
    public GameObject attackZonePrefab;
    [Range(1f, 20f)] public float attack1SpawnRadius   = 8f;
    [Range(10f, 120f)] public float attack1TotalDuration = 60f;
    [Range(1, 20)] public int attack1SpawnCount    = 10;  // 총 소환 횟수 (그룹 수)
    [Range(1, 10)] public int attack1GroupMin      = 4;   // 그룹당 최소 개수
    [Range(1, 10)] public int attack1GroupMax      = 5;   // 그룹당 최대 개수
    public LayerMask groundLayer;                         // 바닥 감지

    [Header("References")]
    [Tooltip("태그 해제 대상 자식 오브젝트 (EnemyBody)")]
    public GameObject enemyBodyObject;

    public enum MODE_STATE { IDLE, TRACE, SURPRISE, ATTACK, HIT, DIE }

    private Animator     _anim;
    private Transform    myTr;
    private NavMeshAgent _agent;
    private Transform    traceTarget;
    private Rigidbody    rbody;

    // 상태 플래그
    private bool isActing       = false; // 애니메이션 진행 중 여부 (단일 플래그로 통합)
    private bool hasPlayedAggro = false;
    private bool isHit          = false;
    private float isHitEndTime;
    private int   maxHp;

    // 타겟 탐색 주기 조절
    private float targetSearchInterval = 0.3f;
    private float lastTargetSearchTime;

    private readonly List<GameObject> _activeAttackZones = new List<GameObject>();

    void Awake()
    {
        _anim = GetComponentInChildren<Animator>();
        myTr  = GetComponent<Transform>();
        rbody = GetComponent<Rigidbody>();
        _agent = GetComponent<NavMeshAgent>();

        if (hpBarImage == null)
            Debug.LogWarning($"[EnemyCtrl] {gameObject.name}: hpBarImage가 연결되지 않았습니다.");
        if (hpBarRoot == null)
            Debug.LogWarning($"[EnemyCtrl] {gameObject.name}: hpBarRoot가 연결되지 않았습니다.");

        maxHp = hp;
        UpdateHpBar();
        SetHpBarVisible(false);
    }

    void Update()
    {
        if (enemyMode == MODE_STATE.DIE) return;

        UpdateHitState();
        UpdateTarget();     // 일정 간격으로만 탐색
        UpdateModeState();  // 매 프레임 상태 판단
        UpdateRotation();
        UpdateAction();     // 상태에 따른 액션 실행
    }

    // =============================================
    // 피격 상태 업데이트
    // =============================================
    void UpdateHitState()
    {
        if (isHit && Time.time > isHitEndTime)
            isHit = false;
    }

    // =============================================
    // 타겟 탐색 — 매 프레임이 아닌 일정 간격으로만 실행
    // =============================================
    void UpdateTarget()
    {
        // 마지막 탐색 이후 interval이 지나지 않았으면 스킵
        if (Time.time < lastTargetSearchTime + targetSearchInterval) return;
        lastTargetSearchTime = Time.time;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length == 0) { traceTarget = null; return; }

        Transform closest = players[0].transform;
        float closestDist = (closest.position - myTr.position).sqrMagnitude;

        foreach (GameObject p in players)
        {
            float d = (p.transform.position - myTr.position).sqrMagnitude;
            if (d < closestDist) { closest = p.transform; closestDist = d; }
        }

        traceTarget = closest;
    }

    // =============================================
    // 상태 전환 판단 — Update()에서 직접 처리
    // =============================================
    void UpdateModeState()
    {
        if (isHit) { ChangeState(MODE_STATE.HIT); return; }
        if (traceTarget == null) { ChangeState(MODE_STATE.IDLE); return; }

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

    // =============================================
    // 상태 전환 — 같은 상태면 중복 전환 방지
    // =============================================
     void ChangeState(MODE_STATE newState)
    {
        if (enemyMode == newState) return;
        // 상태 전환 시 활성 AttackZone 정리
        if (enemyMode == MODE_STATE.ATTACK)
            CleanupAttackZones();

        StopAllCoroutines();
        isActing  = false; // 코루틴 중단 완료 후 리셋 → 새 액션 진입 허용
        enemyMode = newState;
    }

    // =============================================
    // 회전
    // =============================================
    void UpdateRotation()
    {
        if (traceTarget == null) return;
        if (enemyMode != MODE_STATE.TRACE && enemyMode != MODE_STATE.ATTACK) return;

        Vector3 dir = traceTarget.position - myTr.position;
        dir.y = 0;
        if (dir.sqrMagnitude < 0.001f) return;
        myTr.rotation = Quaternion.Slerp(
            myTr.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 10f);
    }

    // =============================================
    // 액션 실행 — isActing으로 중복 실행 방지
    // =============================================
    void UpdateAction()
    {
        if (isActing) return; // 애니메이션 진행 중이면 스킵

        switch (enemyMode)
        {
            case MODE_STATE.IDLE:     StartCoroutine(IdleCoroutine());    break;
            case MODE_STATE.SURPRISE: StartCoroutine(AggroCoroutine());   break;
            case MODE_STATE.ATTACK:   StartCoroutine(AttackCoroutine());  break;
            case MODE_STATE.HIT:      StartCoroutine(HitCoroutine());     break;
            case MODE_STATE.TRACE:    /* 이동 로직 */                      break;
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

    // =============================================
    // 애니메이션 코루틴 — 대기 목적으로만 사용
    // =============================================
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

    IEnumerator AttackCoroutine()
    {
        isActing = true;
        if (_agent != null && _agent.isOnNavMesh) _agent.isStopped = true;

        int rand = Rand.Range(1, 4);
        _anim.SetTrigger("Attack" + rand);

        if (rand == 1)
            yield return StartCoroutine(Attack1PatternCoroutine());
        else
        {
            float duration = rand switch { 2 => attack2Duration, _ => attack3Duration };
            yield return new WaitForSeconds(duration + 0.5f);
        }

        isActing = false;
    }

    // =============================================
    // Attack1 장판 패턴
    // =============================================
    IEnumerator Attack1PatternCoroutine()
    {
        // 60초 안에 10번의 그룹 소환 타이밍을 랜덤 생성
        float[] spawnTimes = new float[attack1SpawnCount];
        for (int i = 0; i < attack1SpawnCount; i++)
            spawnTimes[i] = Rand.Range(0f, attack1TotalDuration * 0.9f);
        System.Array.Sort(spawnTimes);

        float elapsed   = 0f;
        int   nextSpawn = 0;

        while (elapsed < attack1TotalDuration)
        {
            while (nextSpawn < attack1SpawnCount && elapsed >= spawnTimes[nextSpawn])
            {
                int groupCount = Rand.Range(attack1GroupMin, attack1GroupMax + 1);
                for (int i = 0; i < groupCount; i++)
                    SpawnAttackZone();
                nextSpawn++;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    // Attack1 장판 소환 위치 계산 및 생성
    void SpawnAttackZone()
    {
        if (attackZonePrefab == null)
        {
            Debug.LogWarning($"[{gameObject.name}] AttackZone 프리팹이 할당되지 않았습니다!");

            return;
        }

        // 플레이어가 있으면 플레이어 주변, 없으면 자기 주변에 소환
        Vector3 center     = traceTarget != null ? traceTarget.position : myTr.position;

        // 반경 내 랜덤 XZ 위치 (최소 1.5f 이상 떨어지게)
        Vector2 randCircle = Rand.insideUnitCircle.normalized
                           * Rand.Range(1.5f, attack1SpawnRadius);

        Vector3 rayOrigin = new Vector3(
        center.x + randCircle.x,
        center.y + 50f,             // 충분히 높은 곳에서 쏨
        center.z + randCircle.y
        );

        Vector3 spawnPos;
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 100f, groundLayer))
        {
            spawnPos = hit.point; // 바닥 콜라이더 표면 위에 정확히 배치
        }
        else
        {
            // 레이캐스트 실패 시 fallback (플레이어 높이 기준)
            spawnPos = new Vector3(rayOrigin.x, center.y, rayOrigin.z);
            Debug.LogWarning($"[{gameObject.name}] 바닥을 찾지 못했습니다. GroundLayer 설정을 확인하세요.");
        }

        GameObject zone = Instantiate(attackZonePrefab, spawnPos, Quaternion.identity);
        _activeAttackZones.Add(zone);
    }

     void CleanupAttackZones()
    {
        foreach (GameObject zone in _activeAttackZones)
        {
            if (zone != null) Destroy(zone);
        }
        _activeAttackZones.Clear();
    }

    // =============================================
    // 데미지 / 사망
    // =============================================
    IEnumerator HitCoroutine()
    {
        isActing = true;
        _anim.SetTrigger("Hit");
        yield return new WaitForSeconds(hitAnimDuration); // 분리된 필드 사용
        isActing = false;
    }

    IEnumerator DieCoroutine()
    {
        CleanupAttackZones(); // 사망 시에도 장판 정리
        ChangeState(MODE_STATE.DIE);
        _anim.SetTrigger("Die");

        gameObject.tag = "Untagged";
        if (enemyBodyObject != null)
            enemyBodyObject.tag = "Untagged";
        else
            Debug.LogWarning($"[EnemyCtrl] {gameObject.name}: enemyBodyObject가 연결되지 않았습니다.");

        foreach (Collider c in GetComponentsInChildren<Collider>())
            c.enabled = false;

        if (_agent != null && _agent.isOnNavMesh)
            _agent.isStopped = true;

        yield return new WaitForSeconds(4.5f);
        Destroy(gameObject);
    }

    // ──────────────────────────────────────────
    //  데미지 / HP바
    // ──────────────────────────────────────────
    public void TakeDamage(int damage)
    {
        hp -= damage;
        hp  = Mathf.Max(hp, 0);

        UpdateHpBar();

        isHit        = true;
        isHitEndTime = Time.time + hitStateDuration; // [변경 4] 분리된 필드 사용
        if (hp <= 0) StartCoroutine(DieCoroutine());
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

    void OnDestroy()
    {
        CleanupAttackZones(); // [변경 6] Destroy 직전에도 정리
        StopAllCoroutines();
    }
}
