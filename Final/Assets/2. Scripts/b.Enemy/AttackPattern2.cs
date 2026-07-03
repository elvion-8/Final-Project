using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// ============================================================
//  AttackPattern2
//  ① MasterClient가 랜덤 플레이어 1명을 속박
//  ② 속박된 플레이어 → Sanity 지속 감소 + 이동 불가 (10초)
//  ③ 속박 대상을 바라보는 다른 플레이어 → 혼란(Sanity감소 + 이동불가)
//  ④ activeDuration(10초) 후 속박/혼란 전부 해제, 컨트롤 복귀
//  ⑤ 속박 중 Space를 연타하면 머리 위 게이지가 채워지고,
//     가득 차면 activeDuration을 기다리지 않고 즉시 조기 해제
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

    [Header("탈출 게이지 설정")]
    public GameObject bindBarPrefab;        // Slider(또는 fillAmount용 Image)를 가진 월드스페이스 캔버스 프리팹
    public float bindBarHeightOffset = 2.2f; // bindEffect보다 살짝 위
    [Range(0.01f, 1f)]
    public float bindFillPerPress = 0.12f;   // Space 한 번 누를 때 채워지는 양(0~1)
    public float bindFillDecayPerSec = 0.05f; // 가만히 있으면 서서히 줄어드는 양(연타 유도용, 0이면 감소 없음)
    public float bindFillSyncInterval = 0.15f; // 게이지 값을 다른 클라이언트에 동기화하는 주기(초)

    // EnemyCtrl에서 SetContext()로 주입
    private Transform _enemyTr;
    private Transform _traceTarget;

    private bool       _isActive    = false;
    private bool       _earlyReleased = false; // 게이지로 조기 해제됐는지 여부(중복 EndGimmick 방지용)
    private GameObject _warningEffect;
    private GameObject _bindEffect;

    private GameObject  _boundPlayer;
    private bool        _iAmBound    = false;
    private bool        _iAmConfused = false;

    private PlayerCtrl           _localCtrl;
    private CharacterController  _localCharCon;

    // ── 탈출 게이지 관련 ──
    private GameObject _bindBarInstance;
    private Slider      _bindBarSlider;      // bindBarPrefab에 Slider가 있을 경우
    private Image        _bindBarFillImage;   // Slider 대신 Image(Filled)만 있을 경우
    private float        _bindFillAmount = 0f; // 0~1
    private float        _bindFillSyncTimer = 0f;

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
    //  매 프레임 : 속박된 로컬 플레이어의 Space 입력 처리
    // ────────────────────────────────────────────
    void Update()
    {
        if (!_isActive || !_iAmBound || _earlyReleased) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            IncreaseBindFill(bindFillPerPress);
        }

        // 연타를 멈추면 서서히 감소 (게이지를 계속 채우도록 유도)
        if (bindFillDecayPerSec > 0f && _bindFillAmount > 0f)
        {
            IncreaseBindFill(-bindFillDecayPerSec * Time.deltaTime);
        }
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

        if (_earlyReleased) yield break; // 혹시 대기 중에 이미 풀렸다면 진행하지 않음

        // Bind Effect: 활성화 시작 → 머리 위에 고정된 이펙트
        _isActive = true;

        if (_boundPlayer != null)
        {
            SpawnBindEffect(_boundPlayer.transform);
            SpawnBindBar(_boundPlayer.transform);
        }

        if (_iAmBound)
            SetMovementLock(true);

        StartCoroutine(BindSanityDrain());
        StartCoroutine(GazeConfusionLoop());

        // activeDuration이 끝나거나, 게이지로 조기 해제될 때까지 대기
        float elapsed = 0f;
        while (elapsed < activeDuration && !_earlyReleased)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        _isActive = false;

        if (!_earlyReleased)
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
    //  탈출 게이지 바 스폰 (bindEffect보다 조금 위, 빈 상태로 시작)
    // ────────────────────────────────────────────
    void SpawnBindBar(Transform boundPlayerTr)
    {
        if (bindBarPrefab == null) return;

        _bindFillAmount = 0f;

        Vector3 spawnPos = boundPlayerTr.position + Vector3.up * bindBarHeightOffset;
        _bindBarInstance = Instantiate(bindBarPrefab, spawnPos, bindBarPrefab.transform.rotation);
        _bindBarInstance.transform.SetParent(transform);

        // Slider 또는 fillAmount용 Image 중 있는 쪽을 사용
        _bindBarSlider    = _bindBarInstance.GetComponentInChildren<Slider>();
        _bindBarFillImage = _bindBarInstance.GetComponentInChildren<Image>();

        UpdateBindBarVisual();
    }

    // ────────────────────────────────────────────
    //  게이지 증감 (양수: 채움 / 음수: 감소), 가득 차면 조기 해제 트리거
    // ────────────────────────────────────────────
    void IncreaseBindFill(float amount)
    {
        float prev = _bindFillAmount;
        _bindFillAmount = Mathf.Clamp01(_bindFillAmount + amount);

        if (Mathf.Approximately(_bindFillAmount, prev)) return;

        UpdateBindBarVisual();

        // 주기적으로 동기화
        _bindFillSyncTimer += Time.deltaTime;
        if (PhotonNetwork.inRoom && _bindFillSyncTimer >= bindFillSyncInterval)
        {
            _bindFillSyncTimer = 0f;
            GetComponent<PhotonView>().RPC(
                "RPC_UpdateBindFill", PhotonTargets.Others, _bindFillAmount);
        }

        if (_bindFillAmount >= 1f)
        {
            TriggerEarlyRelease();
        }
    }

    void UpdateBindBarVisual()
    {
        if (_bindBarSlider != null)
            _bindBarSlider.value = _bindFillAmount;
        else if (_bindBarFillImage != null)
            _bindBarFillImage.fillAmount = _bindFillAmount;
    }

    // ────────────────────────────────────────────
    //  [멀티] 다른 클라이언트에 게이지 값만 동기화 (시각적 표시용)
    // ────────────────────────────────────────────
    [PunRPC]
    void RPC_UpdateBindFill(float amount)
    {
        _bindFillAmount = amount;
        UpdateBindBarVisual();
    }

    // ────────────────────────────────────────────
    //  게이지가 가득 찼을 때 : 조기 해제 시작 (본인이 직접 호출)
    // ────────────────────────────────────────────
    void TriggerEarlyRelease()
    {
        if (_earlyReleased) return;
        _earlyReleased = true;

        if (PhotonNetwork.inRoom)
        {
            GetComponent<PhotonView>().RPC(
                "RPC_ReleaseBindEarly", PhotonTargets.AllBuffered);
        }
        else
        {
            ReleaseBindEarlyLocal();
        }
    }

    // ────────────────────────────────────────────
    //  [멀티] RPC : 전 클라이언트에서 조기 해제 실행
    // ────────────────────────────────────────────
    [PunRPC]
    void RPC_ReleaseBindEarly()
    {
        _earlyReleased = true;
        ReleaseBindEarlyLocal();
    }

    // ────────────────────────────────────────────
    //  실제 해제 처리 (이펙트/게이지 제거, 이동 잠금 해제, 루프 종료)
    // ────────────────────────────────────────────
    void ReleaseBindEarlyLocal()
    {
        _isActive = false;
        EndGimmick();
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

        if (_bindBarInstance != null)
            Destroy(_bindBarInstance);
    }

    void OnDestroy()
    {
        _isActive = false;
        EndGimmick();
    }
}
