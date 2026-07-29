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
    // ────────────────────────────────────────────
    //  ★ 애니메이션 테이블
    //  Animator의 Trigger 이름 + 클립을 짝지어 등록.
    //  인스펙터에서 +/- 로 자유롭게 추가/삭제 가능.
    //  (예: Idle1, Idle2, Aggro, Hit, Die, Attack1 ...)
    // ────────────────────────────────────────────
    [System.Serializable]
    public struct AnimEntry
    {
        [Tooltip("Animator의 Trigger 파라미터 이름. 코드에서 이 이름으로 찾습니다.")]
        public string trigger;

        [Tooltip("이 동작의 애니메이션 클립. 클립 길이가 그대로 대기 시간이 됩니다.")]
        public AnimationClip clip;

        [Tooltip("클립 길이에 더할 여유 시간(초). 0이면 클립 길이 그대로.")]
        [Min(0f)] public float extraDelay;
    }

    [Header("애니메이션 테이블")]
    [Tooltip("Trigger 이름과 클립을 짝지어 등록. 필요한 만큼 +/- 로 추가하세요.")]
    public List<AnimEntry> animTable = new List<AnimEntry>();

    [Tooltip("Animator State의 Speed가 1이 아닐 때 보정용. 1이면 보정 없음.")]
    [Min(0.01f)] public float animSpeedMultiplier = 1.0f;

    [Tooltip("테이블에 없는 트리거가 호출됐을 때 사용할 기본 대기 시간(초)")]
    [SerializeField] float fallbackDuration = 1.0f;

    // trigger 이름 → 항목 (Awake에서 구성)
    private Dictionary<string, AnimEntry> _animMap;

    [Header("공격 텀")]
    [Tooltip("한 번의 공격 패턴이 끝난 후 다음 공격 패턴이 시작되기까지의 대기 시간(초)")]
    [SerializeField] float attackCooldown = 1.5f;
    private float lastAttackEndTime = -999f;

    // ────────────────────────────────────────────
    //  ★ 어그로(위협 수치) 설정
    //  누적 피해량이 가장 큰 플레이어를 타겟으로 삼음
    // ────────────────────────────────────────────
    [Header("어그로 / 타겟팅")]
    [Tooltip("체크 시 '가장 많이 때린 플레이어'를 타겟으로 삼음. 해제 시 가장 가까운 플레이어.")]
    [SerializeField] bool useThreatTargeting = true;

    [Tooltip("현재 타겟을 뺏으려면 위협 수치가 이 배율 이상이어야 함. 타겟이 깜빡이는 것을 방지.")]
    [Range(1f, 3f)][SerializeField] float threatSwitchRatio = 1.2f;

    [Tooltip("초당 감소하는 위협 수치 비율(0 = 감소 없음). 0.1이면 초당 10%씩 감소.")]
    [Range(0f, 1f)][SerializeField] float threatDecayPerSec = 0f;

    [Tooltip("이 거리를 벗어난 플레이어는 위협 목록에서 제거")]
    [SerializeField] float threatLeashDist = 40f;

    // viewID → 누적 피해량
    private readonly Dictionary<int, float> _threat      = new Dictionary<int, float>();
    // viewID → Transform 캐시
    private readonly Dictionary<int, Transform> _actorTr = new Dictionary<int, Transform>();
    private int _currentTargetId = -1;
    private PhotonView _pv;

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
    private EnemyItemDrop _itemDrop;

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
        _itemDrop = GetComponent<EnemyItemDrop>();
        _pv       = GetComponent<PhotonView>();

        BuildAnimMap();

        maxHp = hp;
        UpdateHpBar();
        SetHpBarVisible(false);
    }

    // ────────────────────────────────────────────
    //  ★ 애니메이션 테이블 유틸
    // ────────────────────────────────────────────
    void BuildAnimMap()
    {
        _animMap = new Dictionary<string, AnimEntry>(animTable.Count);
        foreach (AnimEntry e in animTable)
        {
            if (string.IsNullOrEmpty(e.trigger)) continue;
            _animMap[e.trigger] = e;   // 같은 이름이 있으면 뒤쪽이 덮어씀
        }
    }

    /// <summary>등록된 클립 길이(초) + 여유 시간. 등록이 없으면 fallback.</summary>
    public float GetAnimLength(string trigger)
    {
        if (_animMap != null && _animMap.TryGetValue(trigger, out AnimEntry e))
        {
            float len = e.clip != null
                      ? e.clip.length / animSpeedMultiplier
                      : fallbackDuration;
            return len + e.extraDelay;
        }

        Debug.LogWarning($"[EnemyCtrl] {gameObject.name}: 애니메이션 테이블에 '{trigger}' 항목이 없습니다.");
        return fallbackDuration;
    }

    /// <summary>트리거 발동 + 그 클립 길이만큼 대기</summary>
    IEnumerator PlayAnim(string trigger)
    {
        _anim.SetTrigger(trigger);
        yield return new WaitForSeconds(GetAnimLength(trigger));
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        // 클립만 넣고 이름을 비워두면 클립 이름으로 자동 채움
        for (int i = 0; i < animTable.Count; i++)
        {
            AnimEntry e = animTable[i];
            if (string.IsNullOrEmpty(e.trigger) && e.clip != null)
            {
                e.trigger = e.clip.name;
                animTable[i] = e;
            }
        }
    }
#endif

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
        float delta = Time.time - lastTargetSearchTime;
        lastTargetSearchTime = Time.time;

        if (threatDecayPerSec > 0f) DecayThreat(delta);

        Transform target = null;

        // 1순위: 누적 피해량이 가장 큰 플레이어
        if (useThreatTargeting) target = GetHighestThreatTarget();

        // 2순위: 아직 아무도 때리지 않았다면 가장 가까운 플레이어
        if (target == null) { target = GetClosestPlayer(); _currentTargetId = -1; }

        traceTarget = target;
    }

    // ────────────────────────────────────────────
    //  ★ 위협 수치 기반 타겟 선정
    // ────────────────────────────────────────────
    Transform GetHighestThreatTarget()
    {
        if (_threat.Count == 0) return null;

        List<int> remove = null;
        Transform best = null;
        float bestThreat = 0f;
        int   bestId = -1;
        float leashSqr = threatLeashDist * threatLeashDist;

        foreach (KeyValuePair<int, float> kv in _threat)
        {
            Transform tr = FindActor(kv.Key);

            // 죽었거나 나갔거나 너무 멀어진 대상은 목록에서 제거
            if (tr == null || !tr.gameObject.activeInHierarchy ||
                (tr.position - myTr.position).sqrMagnitude > leashSqr)
            {
                if (remove == null) remove = new List<int>();
                remove.Add(kv.Key);
                continue;
            }

            // 동점이면 viewID가 작은 쪽 → 모든 클라이언트가 같은 타겟을 고르도록
            if (kv.Value > bestThreat || (kv.Value == bestThreat && kv.Key < bestId))
            {
                bestThreat = kv.Value;
                best       = tr;
                bestId     = kv.Key;
            }
        }

        if (remove != null)
        {
            foreach (int id in remove)
            {
                _threat.Remove(id);
                _actorTr.Remove(id);
                if (_currentTargetId == id) _currentTargetId = -1;
            }
        }

        if (best == null) return null;

        // 현재 타겟을 뺏으려면 threatSwitchRatio 배 이상이어야 함
        if (_currentTargetId != -1 && _currentTargetId != bestId &&
            _threat.TryGetValue(_currentTargetId, out float curThreat))
        {
            Transform curTr = FindActor(_currentTargetId);
            if (curTr != null && bestThreat < curThreat * threatSwitchRatio)
                return curTr;
        }

        _currentTargetId = bestId;
        return best;
    }

    Transform GetClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length == 0) return null;

        Transform closest     = players[0].transform;
        float     closestDist = (closest.position - myTr.position).sqrMagnitude;

        foreach (GameObject p in players)
        {
            float d = (p.transform.position - myTr.position).sqrMagnitude;
            if (d < closestDist) { closest = p.transform; closestDist = d; }
        }
        return closest;
    }

    void DecayThreat(float delta)
    {
        float factor = Mathf.Pow(1f - threatDecayPerSec, delta);
        List<int> keys = new List<int>(_threat.Keys);
        foreach (int id in keys)
        {
            float v = _threat[id] * factor;
            if (v < 1f) { _threat.Remove(id); _actorTr.Remove(id); }
            else        _threat[id] = v;
        }
    }

    /// <summary>viewID로 실제 Transform 찾기 (캐시 우선)</summary>
    Transform FindActor(int viewId)
    {
        if (_actorTr.TryGetValue(viewId, out Transform cached) && cached != null)
            return cached;

        PhotonView pv = PhotonView.Find(viewId);
        if (pv == null) return null;

        _actorTr[viewId] = pv.transform;
        return pv.transform;
    }

    /// <summary>공격자 GameObject → 네트워크 식별자(viewID)</summary>
    int GetActorId(GameObject attacker)
    {
        if (attacker == null) return -1;

        PhotonView pv = attacker.GetComponentInParent<PhotonView>();
        if (pv != null) return pv.viewID;

        // PhotonView가 없는 경우(싱글 테스트) — 로컬에서만 유효
        return attacker.GetInstanceID();
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
        SetCombat(dist <= findDist);

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
        yield return PlayAnim("Idle" + rand);
        isActing = false;
    }

    IEnumerator AggroCoroutine()
    {
        isActing = true;
        if (_agent != null && _agent.isOnNavMesh) _agent.isStopped = true;
        yield return PlayAnim("Aggro");
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
        yield return PlayAnim("Hit");
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
    /// <summary>기존 호환용. 공격자를 알 수 없으므로 어그로에는 반영되지 않음.</summary>
    public void TakeDamage(int damage) => TakeDamage(damage, null);

    /// <summary>★ 공격자를 넘겨주는 버전. 무기 쪽에서 이 함수를 호출할 것.</summary>
    public void TakeDamage(int damage, GameObject attacker)
    {
        if (enemyMode == MODE_STATE.DIE) return;

        int actorId = GetActorId(attacker);
        if (attacker != null && actorId != -1)
            _actorTr[actorId] = attacker.transform;   // 로컬 캐시

        // 모든 클라이언트가 같은 위협 목록을 갖도록 브로드캐스트
        if (_pv != null && _pv.viewID > 0 && PhotonNetwork.inRoom)
            _pv.RPC("RpcApplyDamage", PhotonTargets.All, damage, actorId);
        else
            ApplyDamage(damage, actorId);
    }

    [PunRPC]
    void RpcApplyDamage(int damage, int actorId)
    {
        ApplyDamage(damage, actorId);
    }

    void ApplyDamage(int damage, int actorId)
    {
        if (enemyMode == MODE_STATE.DIE) return;

        hp = Mathf.Max(hp - damage, 0);
        Debug.Log($"[EnemyCtrl] 피격! 받은 데미지: {damage}, 남은 HP: {hp}/{maxHp}");
        UpdateHpBar();

        // 위협 수치 누적
        if (actorId != -1 && damage > 0)
        {
            _threat.TryGetValue(actorId, out float t);
            _threat[actorId] = t + damage;
        }

        isHit        = true;
        isHitEndTime = Time.time + GetAnimLength("Hit");

        if (hp <= 0) Die();
    }

    void UpdateHpBar()
    {
        if (hpBarImage == null) return;
        hpBarImage.fillAmount = (float)hp / maxHp;
    }

    private bool hpBarVisible; 

    void SetHpBarVisible(bool visible)
    {
        if (hpBarRoot != null) hpBarRoot.SetActive(visible);
        SetCombat(visible);   
    }
    void OnDisable()
    {
        inCombat = false;
        HUDFadeManager.Instance?.ExitCombat(this);
    }

    private bool inCombat;

    void SetCombat(bool value)
    {
        if (inCombat == value) return;
        inCombat = value;

        if (value) HUDFadeManager.Instance?.EnterCombat(this);
        else       HUDFadeManager.Instance?.ExitCombat(this);
    }

    // ────────────────────────────────────────────
    //  사망
    // ────────────────────────────────────────────
    void Die()
    {
        StopAllCoroutines();
        enemyMode = MODE_STATE.DIE;
        isActing  = false;
        _threat.Clear();
        _actorTr.Clear();
        _currentTargetId = -1;
        SetCombat(false);  
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

        // 죽음 애니메이션 재생 시간만큼 대기 (여유 시간은 Die 항목의 extraDelay로 조절)
        yield return new WaitForSeconds(GetAnimLength("Die"));

        // 애니메이션이 끝난 시점에 아이템 드랍
        if (_itemDrop != null)
            _itemDrop.Drop(myTr.position);
        Destroy(gameObject);
    }

    void OnDestroy() => CleanupCurrentPattern();

    // ────────────────────────────────────────────
    // RPC 중계 메서드
    // ────────────────────────────────────────────
    [PunRPC]
    public void RpcSetBoundTarget(int ownerId)
    {
        if (_currentPatternObj != null)
        {
            AttackPattern2 pattern = _currentPatternObj.GetComponent<AttackPattern2>();
            if (pattern != null) pattern.ApplyBoundTargetLocal(ownerId);
        }
    }

    [PunRPC]
    public void RpcUpdateBindFill(float amount)
    {
        if (_currentPatternObj != null)
        {
            AttackPattern2 pattern = _currentPatternObj.GetComponent<AttackPattern2>();
            if (pattern != null) pattern.ApplyBindFillLocal(amount);
        }
    }

    [PunRPC]
    public void RpcReleaseBindEarly()
    {
        if (_currentPatternObj != null)
        {
            AttackPattern2 pattern = _currentPatternObj.GetComponent<AttackPattern2>();
            if (pattern != null) pattern.ApplyReleaseBindEarlyLocal();
        }
    }
}