#define CBT_MODE
//#define RELEASE_MODE

using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
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
    public float hitDuration    = 0.5f;

    [Header("STATE")]
    public MODE_STATE enemyMode = MODE_STATE.IDLE;

    [Header("몬스터 인공지능")]
    [Range(0, 1000)] public int hp = 1000;
    [Range(10f, 30f)][SerializeField] float findDist   = 20.0f;
    [Range(1f,  30f)][SerializeField] float attackDist = 5.0f;

    public enum MODE_STATE { IDLE, TRACE, SURPRISE, ATTACK, HIT, DIE }

    private Animator    _anim;
    private Transform   myTr;
    private Transform   traceTarget;
    private Rigidbody   rbody;

    // 상태 플래그
    private bool isActing       = false; // 애니메이션 진행 중 여부 (단일 플래그로 통합)
    private bool hasPlayedAggro = false;
    private bool isHit          = false;
    private float isHitEndTime;

    // 타겟 탐색 주기 조절
    private float targetSearchInterval = 0.3f;
    private float lastTargetSearchTime;

    void Awake()
    {
        _anim = GetComponentInChildren<Animator>();
        myTr  = GetComponent<Transform>();
        rbody = GetComponent<Rigidbody>();
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
        if (enemyMode == newState) return; // ✅ 동일 상태면 무시

        enemyMode = newState;
        isActing  = false; // 상태 바뀌면 진행 중 액션 초기화
        StopAllCoroutines();
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

    // =============================================
    // 애니메이션 코루틴 — 대기 목적으로만 사용
    // =============================================
    IEnumerator IdleCoroutine()
    {
        isActing = true;
        int rand = Rand.Range(1, 3);
        _anim.SetTrigger("Idle" + rand);
        yield return new WaitForSeconds(rand == 1 ? idle1Duration : idle2Duration);
        isActing = false;
    }

    IEnumerator AggroCoroutine()
    {
        isActing = true;
        _anim.SetTrigger("Aggro");
        yield return new WaitForSeconds(aggroDuration);
        hasPlayedAggro = true;
        isActing = false;
    }

    IEnumerator AttackCoroutine()
    {
        isActing = true;
        int rand = Rand.Range(1, 4);
        float duration = rand switch { 1 => attack1Duration, 2 => attack2Duration, _ => attack3Duration };
        _anim.SetTrigger("Attack" + rand);
        yield return new WaitForSeconds(duration + 0.5f);
        isActing = false;
    }

    IEnumerator HitCoroutine()
    {
        isActing = true;
        _anim.SetTrigger("Hit");
        yield return new WaitForSeconds(hitDuration);
        isActing = false;
    }

    // =============================================
    // 데미지 / 사망
    // =============================================
    public void TakeDamage(int damage)
    {
        hp -= damage;
        isHit       = true;
        isHitEndTime = Time.time + hitDuration;
        if (hp <= 0) StartCoroutine(DieCoroutine());
    }

    IEnumerator DieCoroutine()
    {
        ChangeState(MODE_STATE.DIE);
        _anim.SetTrigger("Die");
        this.gameObject.tag = "Untagged";
        this.gameObject.transform.Find("EnemyBody").tag = "Untagged";
        foreach (Collider c in gameObject.GetComponentsInChildren<Collider>())
            c.enabled = false;
        yield return new WaitForSeconds(4.5f);
        Destroy(gameObject);
    }

    void OnDestroy() => StopAllCoroutines();
}
