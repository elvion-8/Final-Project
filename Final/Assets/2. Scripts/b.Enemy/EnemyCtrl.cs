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
    [Header("고정형 몬스터 설정")]
    [Tooltip("체크하면 플레이어를 쫓아가지 않고 제자리에서 공격합니다.")]
    public bool isStationary = false; 

    // ────────────────────────────────────────────
    // 애니메이션 테이블
    // ────────────────────────────────────────────
    [System.Serializable]
    public struct AnimEntry
    {
        public string trigger;
        public AnimationClip clip;
        [Min(0f)] public float extraDelay;
    }

    [Header("애니메이션 테이블")]
    public List<AnimEntry> animTable = new List<AnimEntry>();
    [Min(0.01f)] public float animSpeedMultiplier = 1.0f;
    [SerializeField] float fallbackDuration = 1.0f;

    private Dictionary<string, AnimEntry> _animMap;

    [Header("공격 텀")]
    [SerializeField] float attackCooldown = 1.5f;
    private float lastAttackEndTime = -999f;

    [Header("어그로 / 타겟팅")]
    [SerializeField] bool useThreatTargeting = true;
    [Range(1f, 3f)][SerializeField] float threatSwitchRatio = 1.2f;
    [Range(0f, 1f)][SerializeField] float threatDecayPerSec = 0f;
    [SerializeField] float threatLeashDist = 40f;

    private readonly Dictionary<int, float> _threat      = new Dictionary<int, float>();
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

    [Header("사망 연출")]
    public GameObject deathSequencePrefab;

    [Header("공격 패턴 프리팹")]
    public GameObject[] attackPatternPrefabs;

    public enum MODE_STATE { IDLE, TRACE, SURPRISE, ATTACK, HIT, DIE }

    private Animator     _anim;
    private Transform    myTr;
    private NavMeshAgent _agent;
    public  Transform    traceTarget; 
    private Rigidbody    rbody;
    private EnemyItemDrop _itemDrop;

    private bool  isActing       = false;
    private bool  hasPlayedAggro = false;
    private bool  isHit          = false;
    private float isHitEndTime;
    private int   maxHp;

    private float targetSearchInterval = 0.3f;
    private float lastTargetSearchTime;

    private GameObject _currentPatternObj;
    private bool _isPatternRunning = false;

    // ★ 제어 변수
    private bool isPlayingIdle1 = false; 
    private bool isIdle1PermanentlyDisabled = false;
    private int idlePlayCount = 0; 

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

    void BuildAnimMap()
    {
        _animMap = new Dictionary<string, AnimEntry>();
        foreach (AnimEntry e in animTable)
        {
            if (string.IsNullOrEmpty(e.trigger)) continue;
            string cleanKey = e.trigger.Trim().ToLower(); // 띄어쓰기 제거 및 소문자화
            _animMap[cleanKey] = e; 
        }
    }

    public float GetAnimLength(string trigger)
    {
        string cleanKey = trigger.Trim().ToLower();
        if (_animMap != null && _animMap.TryGetValue(cleanKey, out AnimEntry e))
        {
            float len = e.clip != null ? e.clip.length / animSpeedMultiplier : fallbackDuration;
            return len + e.extraDelay;
        }
        
        return fallbackDuration;
    }

    IEnumerator PlayAnim(string trigger)
    {
        ResetAnimatorTriggerSafe("Idle1");
        ResetAnimatorTriggerSafe("Idle2");
        ResetAnimatorTriggerSafe("Aggro");

        SetAnimatorTriggerSafe(trigger); 
        yield return new WaitForSeconds(GetAnimLength(trigger));
    }

    void SetAnimatorTriggerSafe(string triggerName)
    {
        if (_anim == null) return;
        foreach (AnimatorControllerParameter p in _anim.parameters)
        {
            if (p.type == AnimatorControllerParameterType.Trigger && p.name == triggerName)
            {
                _anim.SetTrigger(triggerName);
                return;
            }
        }
    }

    void ResetAnimatorTriggerSafe(string triggerName)
    {
        if (_anim == null) return;
        foreach (AnimatorControllerParameter p in _anim.parameters)
        {
            if (p.type == AnimatorControllerParameterType.Trigger && p.name == triggerName)
            {
                _anim.ResetTrigger(triggerName);
                return;
            }
        }
    }

    void SetAnimatorBoolSafe(string paramName, bool value)
    {
        if (_anim == null) return;
        foreach (AnimatorControllerParameter p in _anim.parameters)
        {
            if (p.type == AnimatorControllerParameterType.Bool && p.name == paramName)
            {
                _anim.SetBool(paramName, value);
                return;
            }
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
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
        if (useThreatTargeting) target = GetHighestThreatTarget();
        if (target == null) { target = GetClosestPlayer(); _currentTargetId = -1; }

        traceTarget = target;
    }

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
            if (tr == null || !tr.gameObject.activeInHierarchy ||
                (tr.position - myTr.position).sqrMagnitude > leashSqr)
            {
                if (remove == null) remove = new List<int>();
                remove.Add(kv.Key);
                continue;
            }

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

    Transform FindActor(int viewId)
    {
        if (_actorTr.TryGetValue(viewId, out Transform cached) && cached != null) return cached;
        PhotonView pv = PhotonView.Find(viewId);
        if (pv == null) return null;
        _actorTr[viewId] = pv.transform;
        return pv.transform;
    }

    int GetActorId(GameObject attacker)
    {
        if (attacker == null) return -1;
        PhotonView pv = attacker.GetComponentInParent<PhotonView>();
        if (pv != null) return pv.viewID;
        return attacker.GetInstanceID();
    }

    void UpdateModeState()
    {
        if (traceTarget == null)
        {
            SetHpBarVisible(false);
            if (!isPlayingIdle1) ChangeState(MODE_STATE.IDLE);
            return;
        }

        if (isPlayingIdle1) return;

        float dist = Vector3.Distance(myTr.position, traceTarget.position);
        SetHpBarVisible(dist <= hpBarShowDist);
        SetCombat(dist <= findDist);

        bool hasAttacks = (attackPatternPrefabs != null && attackPatternPrefabs.Length > 0);

        if (dist <= attackDist)
        {
            isIdle1PermanentlyDisabled = true; 
            hasPlayedAggro = true;
            ChangeState(hasAttacks ? MODE_STATE.ATTACK : MODE_STATE.IDLE);
        }
        else if (dist <= findDist)
        {
            isIdle1PermanentlyDisabled = true; 
            if (!hasPlayedAggro)
            {
                ChangeState(MODE_STATE.SURPRISE);
            }
            else
            {
                ChangeState(isStationary ? MODE_STATE.IDLE : MODE_STATE.TRACE);
            }
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

        if (enemyMode == MODE_STATE.ATTACK)
        {
            CleanupCurrentPattern();
            _isPatternRunning = false;
        }

        StopAllCoroutines();
        isActing  = false;
        isPlayingIdle1 = false; 

        ResetAnimatorTriggerSafe("Idle1");
        ResetAnimatorTriggerSafe("Idle2");
        ResetAnimatorTriggerSafe("Aggro");

        enemyMode = newState;
    }

    void UpdateRotation()
    {
        if (traceTarget == null) return;
        if (_isPatternRunning) return; 
        
        if (enemyMode != MODE_STATE.TRACE && 
            enemyMode != MODE_STATE.ATTACK && 
            enemyMode != MODE_STATE.SURPRISE && 
            !(enemyMode == MODE_STATE.IDLE && hasPlayedAggro)) return;

        Vector3 dir = traceTarget.position - myTr.position;
        dir.y = 0;
        if (dir.sqrMagnitude < 0.001f) return;
        myTr.rotation = Quaternion.Slerp(myTr.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 10f);
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
            case MODE_STATE.TRACE:    UpdateTrace();                     break;
        }
    }

    void UpdateTrace()
    {
        if (isStationary) return; 
        if (_agent == null || traceTarget == null) return;
        if (!_agent.isOnNavMesh) return;

        _agent.isStopped = false;
        _agent.SetDestination(traceTarget.position);
        SetAnimatorBoolSafe("IsMoving", true); 
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
            SetAnimatorBoolSafe("IsMoving", false); 
        }

        int playIndex = 1;

        if (isIdle1PermanentlyDisabled)
        {
            playIndex = 1;
        }
        else
        {
            if (idlePlayCount >= 1) 
            {
                playIndex = 1;
                idlePlayCount = 0; 
            } 
            else 
            {
                playIndex = 2;
                idlePlayCount++;   
            }
        }

        if (playIndex == 1) isPlayingIdle1 = true; 

        yield return PlayAnim("Idle" + playIndex);

        if (playIndex == 1) isPlayingIdle1 = false; 
        
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

    IEnumerator AttackCoroutine()
    {
        isActing = true;
        if (_agent != null && _agent.isOnNavMesh) _agent.isStopped = true;

        if (attackPatternPrefabs == null || attackPatternPrefabs.Length == 0)
        {
            isActing = false;
            yield break;
        }

        int index = Rand.Range(0, attackPatternPrefabs.Length);
        GameObject prefab = attackPatternPrefabs[index];

        if (prefab == null)
        {
            isActing = false;
            yield break;
        }

        _currentPatternObj = Instantiate(prefab, myTr.position, Quaternion.identity);

        IAttackPattern pattern = _currentPatternObj.GetComponent<IAttackPattern>();
        if (pattern == null)
        {
            Destroy(_currentPatternObj);
            isActing = false;
            yield break;
        }

        pattern.SetContext(myTr, traceTarget);
        SetAnimatorTriggerSafe("Attack" + (index + 1));

        _isPatternRunning = true;
        yield return StartCoroutine(pattern.Execute());
        _isPatternRunning = false;

        CleanupCurrentPattern();
        lastAttackEndTime = Time.time;
        isActing = false;
    }

    void CleanupCurrentPattern()
    {
        if (_currentPatternObj != null)
        {
            Destroy(_currentPatternObj);
            _currentPatternObj = null;
        }
    }

    public void TakeDamage(int damage) => TakeDamage(damage, null);

    public void TakeDamage(int damage, GameObject attacker)
    {
        if (enemyMode == MODE_STATE.DIE) return;

        int actorId = GetActorId(attacker);
        if (attacker != null && actorId != -1)
            _actorTr[actorId] = attacker.transform;   

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
        UpdateHpBar();

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

    void Die()
    {
        StopAllCoroutines();
        enemyMode = MODE_STATE.DIE;
        isActing  = false;
        isPlayingIdle1 = false; 
        _threat.Clear();
        _actorTr.Clear();
        _currentTargetId = -1;
        SetCombat(false);  
        StartCoroutine(DieCoroutine());
    }

    IEnumerator DieCoroutine()
    {
        CleanupCurrentPattern();
        
        gameObject.tag = "Untagged";
        if (enemyBodyObject != null) enemyBodyObject.tag = "Untagged";

        foreach (Collider c in GetComponentsInChildren<Collider>())
            c.enabled = false;

        if (_agent != null && _agent.isOnNavMesh)
            _agent.isStopped = true;

        SetHpBarVisible(false);

        if (_itemDrop != null) _itemDrop.Drop(myTr.position);

        if (deathSequencePrefab != null)
        {
            Instantiate(deathSequencePrefab, myTr.position, myTr.rotation);
        }

        Destroy(gameObject);
        yield break; 
    }

    void OnDestroy() => CleanupCurrentPattern();

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

