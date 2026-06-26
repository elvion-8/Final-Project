using System.Collections;
using UnityEngine;

// ============================================================
//  AttackPattern2
//  ① MasterClient가 랜덤 플레이어 1명을 속박
//  ② 속박된 플레이어 → Sanity 지속 감소 + 이동 불가 (10초)
//  ③ 속박 대상을 바라보는 다른 플레이어 → 혼란(Sanity감소 + 이동불가)
//  ④ activeDuration(10초) 후 속박/혼란 전부 해제, 컨트롤 복귀
//  ※ 솔로 플레이(Player 1명)이면 혼란X, 속박만 진행
// ============================================================
public class AttackPattern2 : MonoBehaviour, IAttackPattern
{
    [Header("타이밍")]
    public float warningDuration = 1.5f;
    public float activeDuration  = 10.0f;

    [Header("속박 설정")]
    public float bindSanityPerSec   = 10f;

    [Header("혼란 설정")]
    public float gazeSanityPerSec   = 8f;
    public float gazeAngleThreshold = 40f;

    [Header("이펙트 프리팹")]
    public GameObject warningEffectPrefab;
    public GameObject bindEffectPrefab;

    [Header("이펙트 오프셋")]
    public float bindEffectHeightOffset = 1.5f;  // 플레이어 머리 위 높이

    // EnemyCtrl에서 SetContext()로 주입
    private Transform _enemyTr;
    private Transform _traceTarget;

    private bool       _isActive    = false;
    private GameObject _warningEffect;
    private GameObject _bindEffect;

    private GameObject  _boundPlayer;
    private bool        _iAmBound    = false;
    private bool        _iAmConfused = false;

    private PlayerCtrl           _localCtrl;
    private CharacterController  _localCharCon;

    // ────────────────────────────────────────────
    //  IAttackPattern 구현
    // ────────────────────────────────────────────
    public void SetContext(Transform enemyTr, Transform traceTarget)
    {
        _enemyTr     = enemyTr;
        _traceTarget = traceTarget;
    }

    public IEnumerator Execute()
    {
        CacheLocalPlayer();

        if (PhotonNetwork.inRoom)
        {
            if (PhotonNetwork.isMasterClient)
                SelectAndBroadcastTarget();
            // 비마스터는 RPC_SetBoundTarget 호출까지 대기
        }
        else
        {
            SelectLocalTarget();
        }

        // ZoneRoutine은 SelectLocalTarget or RPC_SetBoundTarget 안에서 시작됨
        // Execute는 activeDuration + warningDuration 만큼 대기 후 종료
        yield return new WaitForSeconds(warningDuration + activeDuration);
    }

    // ────────────────────────────────────────────
    //  로컬 플레이어 컴포넌트 캐시
    // ────────────────────────────────────────────
    void CacheLocalPlayer()
    {
        GameObject localPlayer = FindLocalPlayer();
        if (localPlayer == null) return;

        _localCtrl    = localPlayer.GetComponent<PlayerCtrl>();
        _localCharCon = localPlayer.GetComponent<CharacterController>();
    }

    // ────────────────────────────────────────────
    //  [솔로] 로컬에서 직접 대상 선택
    // ────────────────────────────────────────────
    void SelectLocalTarget()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length == 0)
        {
            StartCoroutine(ZoneRoutine());
            return;
        }

        _boundPlayer = players[Random.Range(0, players.Length)];
        _iAmBound    = true;

        StartCoroutine(ZoneRoutine());
    }

    // ────────────────────────────────────────────
    //  [멀티] MasterClient : 랜덤 선택 → RPC 전파
    // ────────────────────────────────────────────
    void SelectAndBroadcastTarget()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length == 0)
        {
            StartCoroutine(ZoneRoutine());
            return;
        }

        GameObject chosen = players[Random.Range(0, players.Length)];

        int ownerId = -1;
        PhotonView pv = chosen.GetComponent<PhotonView>();
        if (pv != null)
            ownerId = pv.ownerId;

        GetComponent<PhotonView>().RPC(
            "RPC_SetBoundTarget", PhotonTargets.AllBuffered, ownerId);
    }

    // ────────────────────────────────────────────
    //  [멀티] RPC : 전 클라이언트에서 속박 대상 확정
    // ────────────────────────────────────────────
    [PunRPC]
    void RPC_SetBoundTarget(int ownerId)
    {
        foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
        {
            PhotonView pv = p.GetComponent<PhotonView>();
            if (pv != null && pv.ownerId == ownerId)
            {
                _boundPlayer = p;
                break;
            }
        }

        if (_boundPlayer != null)
        {
            PhotonView myPV = _boundPlayer.GetComponent<PhotonView>();
            _iAmBound = (myPV != null && myPV.isMine);
        }

        StartCoroutine(ZoneRoutine());
    }

    // ────────────────────────────────────────────
    //  메인 루틴 : 경고 → 활성화(10초) → 해제
    // ────────────────────────────────────────────
    IEnumerator ZoneRoutine()
    {
        _isActive = false;

        // Warning Effect: 대상자를 따라다니는 이펙트
        if (_boundPlayer != null)
            StartCoroutine(TrackingWarningEffectRoutine());

        yield return new WaitForSeconds(warningDuration);

        // Bind Effect: 활성화 시작 → 머리 위에 고정된 이펙트
        _isActive = true;

        if (_boundPlayer != null)
            SpawnBindEffect(_boundPlayer.transform);

        if (_iAmBound)
            SetMovementLock(true);

        StartCoroutine(BindSanityDrain());
        StartCoroutine(GazeConfusionLoop());

        yield return new WaitForSeconds(activeDuration);

        _isActive = false;
        EndGimmick();
    }

    // ────────────────────────────────────────────
    //  Warning Effect: 대상자 추적 루틴
    // ────────────────────────────────────────────
    IEnumerator TrackingWarningEffectRoutine()
    {
        if (_boundPlayer == null) yield break;

        // Warning Effect 생성
        if (warningEffectPrefab != null)
        {
            _warningEffect = Instantiate(warningEffectPrefab, 
                _boundPlayer.transform.position, 
                warningEffectPrefab.transform.rotation);
            _warningEffect.transform.SetParent(transform);
        }

        // warningDuration 동안 계속 따라다니기
        float elapsed = 0f;
        while (elapsed < warningDuration && _boundPlayer != null)
        {
            if (_warningEffect != null)
            {
                _warningEffect.transform.position = _boundPlayer.transform.position;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Warning 종료 후 이펙트 제거
        if (_warningEffect != null)
            Destroy(_warningEffect);
    }

    // ────────────────────────────────────────────
    //  Bind Effect: 플레이어 머리 위에 스폰
    // ────────────────────────────────────────────
    void SpawnBindEffect(Transform boundPlayerTr)
    {
        if (bindEffectPrefab == null) return;

        // 플레이어 머리 위 오프셋 위치 계산
        Vector3 spawnPos = boundPlayerTr.position + Vector3.up * bindEffectHeightOffset;

        _bindEffect = Instantiate(bindEffectPrefab, spawnPos, bindEffectPrefab.transform.rotation);
        _bindEffect.transform.SetParent(transform);

        // Bind Effect는 고정 위치이므로 추적 안 함
    }

    // ────────────────────────────────────────────
    //  속박 대상 Sanity 지속 감소
    // ────────────────────────────────────────────
    IEnumerator BindSanityDrain()
    {
        if (!_iAmBound) yield break;

        while (_isActive)
        {
            //ApplyLocalSanity(bindSanityPerSec * Time.deltaTime);
            yield return null;
        }
    }

    // ────────────────────────────────────────────
    //  시선 판정 + 혼란 상태 루프
    // ────────────────────────────────────────────
    IEnumerator GazeConfusionLoop()
    {
        if (_iAmBound)            yield break;
        if (_boundPlayer == null) yield break;

        if (GameObject.FindGameObjectsWithTag("Player").Length <= 1) yield break;

        GameObject localPlayer = FindLocalPlayer();
        if (localPlayer == null) yield break;

        while (_isActive)
        {
            bool isGazing = IsLookingAt(localPlayer, _boundPlayer, gazeAngleThreshold);

            if (isGazing && !_iAmConfused)
            {
                _iAmConfused = true;
                SetMovementLock(true);
            }
            else if (!isGazing && _iAmConfused)
            {
                _iAmConfused = false;
                SetMovementLock(false);
            }

            if (_iAmConfused)
                //ApplyLocalSanity(gazeSanityPerSec * Time.deltaTime);

            yield return null;
        }

        if (_iAmConfused)
        {
            _iAmConfused = false;
            SetMovementLock(false);
        }
    }

    bool IsLookingAt(GameObject from, GameObject target, float angleThreshold)
    {
        Vector3 toTarget = (target.transform.position - from.transform.position).normalized;
        return Vector3.Angle(from.transform.forward, toTarget) < angleThreshold;
    }

    void SetMovementLock(bool lockMove)
    {
        if (_localCharCon != null)
            _localCharCon.enabled = !lockMove;

        if (Managers.Input != null)
            Managers.Input.enabled = !lockMove;
    }

    // void ApplyLocalSanity(float amount)
    // {
    //     if (_localCtrl == null) return;

    //     SanityManager sanity = _localCtrl.GetComponent<SanityManager>();
    //     if (sanity != null)
    //         sanity.DecreaseSanity(amount);
    // }

    GameObject FindLocalPlayer()
    {
        foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
        {
            PhotonView pv = p.GetComponent<PhotonView>();
            if (PhotonNetwork.inRoom)
            {
                if (pv != null && pv.isMine) return p;
            }
            else
            {
                return p;
            }
        }
        return null;
    }

    void EndGimmick()
    {
        if (_iAmBound)
        {
            _iAmBound = false;
            SetMovementLock(false);
        }

        if (_iAmConfused)
        {
            _iAmConfused = false;
            SetMovementLock(false);
        }

        if (_warningEffect != null)
            Destroy(_warningEffect);

        if (_bindEffect != null)
            Destroy(_bindEffect);
    }

    void OnDestroy()
    {
        _isActive = false;
        EndGimmick();
    }
}

