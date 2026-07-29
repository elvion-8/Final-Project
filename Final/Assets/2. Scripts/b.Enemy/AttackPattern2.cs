using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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
    public float bindEffectHeightOffset = 1.5f;

    [Header("탈출 게이지 설정")]
    public GameObject bindBarPrefab;
    public float bindBarHeightOffset = 2.2f;
    [Range(0.01f, 1f)]
    public float bindFillPerPress = 0.12f;
    public float bindFillDecayPerSec = 0.05f;
    public float bindFillSyncInterval = 0.15f;

    private Transform _enemyTr;
    private Transform _traceTarget;

    private bool       _isActive    = false;
    private bool       _earlyReleased = false; 
    private GameObject _warningEffect;
    private GameObject _bindEffect;

    private GameObject  _boundPlayer;
    private bool        _iAmBound    = false;
    private bool        _iAmConfused = false;

    private GameObject           _localPlayerObj;
    private PlayerCtrl           _localCtrl;
    private CharacterController  _localCharCon;

    private GameObject _bindBarInstance;
    private Slider      _bindBarSlider;
    private Image        _bindBarFillImage;
    private float        _bindFillAmount = 0f;
    private float        _bindFillSyncTimer = 0f;

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
        }
        else
        {
            SelectLocalTarget();
        }

        yield return new WaitForSeconds(warningDuration + activeDuration);
    }

    void Update()
    {
        if (!_isActive || !_iAmBound || _earlyReleased) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            IncreaseBindFill(bindFillPerPress);
        }

        if (bindFillDecayPerSec > 0f && _bindFillAmount > 0f)
        {
            IncreaseBindFill(-bindFillDecayPerSec * Time.deltaTime);
        }
    }

    void CacheLocalPlayer()
    {
        if (_localPlayerObj != null) return;

        _localPlayerObj = FindLocalPlayer();
        if (_localPlayerObj == null) return;

        _localCtrl    = _localPlayerObj.GetComponent<PlayerCtrl>();
        _localCharCon = _localPlayerObj.GetComponent<CharacterController>();
    }

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
    //  타겟 선정 브로드캐스트
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
        if (pv != null) ownerId = pv.ownerId;

        PhotonView enemyPv = _enemyTr.GetComponent<PhotonView>();
        if (enemyPv != null)
        {
            enemyPv.RPC("RpcSetBoundTarget", PhotonTargets.AllBuffered, ownerId);
        }
    }

    // ────────────────────────────────────────────
    // 로컬 적용 메서드
    // ────────────────────────────────────────────
    public void ApplyBoundTargetLocal(int ownerId)
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

    IEnumerator ZoneRoutine()
    {
        _isActive = false;

        if (_boundPlayer != null)
            StartCoroutine(TrackingWarningEffectRoutine());

        yield return new WaitForSeconds(warningDuration);

        if (_earlyReleased) yield break; 

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

    IEnumerator TrackingWarningEffectRoutine()
    {
        if (_boundPlayer == null) yield break;

        if (warningEffectPrefab != null)
        {
            _warningEffect = Instantiate(warningEffectPrefab, 
                _boundPlayer.transform.position, 
                warningEffectPrefab.transform.rotation);
            _warningEffect.transform.SetParent(transform);
        }

        float elapsed = 0f;
        while (elapsed < warningDuration && _boundPlayer != null)
        {
            if (_warningEffect != null)
                _warningEffect.transform.position = _boundPlayer.transform.position;

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (_warningEffect != null)
            Destroy(_warningEffect);
    }

    void SpawnBindEffect(Transform boundPlayerTr)
    {
        if (bindEffectPrefab == null) return;
        Vector3 spawnPos = boundPlayerTr.position + Vector3.up * bindEffectHeightOffset;
        _bindEffect = Instantiate(bindEffectPrefab, spawnPos, bindEffectPrefab.transform.rotation);
        _bindEffect.transform.SetParent(transform);
    }

    void SpawnBindBar(Transform boundPlayerTr)
    {
        if (bindBarPrefab == null) return;
        _bindFillAmount = 0f;
        Vector3 spawnPos = boundPlayerTr.position + Vector3.up * bindBarHeightOffset;
        _bindBarInstance = Instantiate(bindBarPrefab, spawnPos, bindBarPrefab.transform.rotation);
        _bindBarInstance.transform.SetParent(transform);
        _bindBarSlider    = _bindBarInstance.GetComponentInChildren<Slider>();
        _bindBarFillImage = _bindBarInstance.GetComponentInChildren<Image>();
        UpdateBindBarVisual();
    }

    // ────────────────────────────────────────────
    // 게이지 증감 브로드캐스트
    // ────────────────────────────────────────────
    void IncreaseBindFill(float amount)
    {
        float prev = _bindFillAmount;
        _bindFillAmount = Mathf.Clamp01(_bindFillAmount + amount);

        if (Mathf.Approximately(_bindFillAmount, prev)) return;

        UpdateBindBarVisual();

        _bindFillSyncTimer += Time.deltaTime;
        if (PhotonNetwork.inRoom && _bindFillSyncTimer >= bindFillSyncInterval)
        {
            _bindFillSyncTimer = 0f;
            PhotonView enemyPv = _enemyTr.GetComponent<PhotonView>();
            if (enemyPv != null)
                enemyPv.RPC("RpcUpdateBindFill", PhotonTargets.Others, _bindFillAmount);
        }

        if (_bindFillAmount >= 1f)
        {
            TriggerEarlyRelease();
        }
    }

    void UpdateBindBarVisual()
    {
        if (_bindBarSlider != null) _bindBarSlider.value = _bindFillAmount;
        else if (_bindBarFillImage != null) _bindBarFillImage.fillAmount = _bindFillAmount;
    }

    // ────────────────────────────────────────────
    // 로컬 적용 메서드
    // ────────────────────────────────────────────
    public void ApplyBindFillLocal(float amount)
    {
        _bindFillAmount = amount;
        UpdateBindBarVisual();
    }

    // ────────────────────────────────────────────
    // 조기 해제 브로드캐스트
    // ────────────────────────────────────────────
    void TriggerEarlyRelease()
    {
        if (_earlyReleased) return;
        _earlyReleased = true;

        if (PhotonNetwork.inRoom)
        {
            PhotonView enemyPv = _enemyTr.GetComponent<PhotonView>();
            if (enemyPv != null)
                enemyPv.RPC("RpcReleaseBindEarly", PhotonTargets.AllBuffered);
        }
        else
        {
            ReleaseBindEarlyLocal();
        }
    }

    // ────────────────────────────────────────────
    // 로컬 적용 메서드
    // ────────────────────────────────────────────
    public void ApplyReleaseBindEarlyLocal()
    {
        _earlyReleased = true;
        ReleaseBindEarlyLocal();
    }

    void ReleaseBindEarlyLocal()
    {
        _isActive = false;
        EndGimmick();
    }

    IEnumerator BindSanityDrain()
    {
        if (!_iAmBound) yield break;
        while (_isActive)
        {
            //ApplyLocalSanity(bindSanityPerSec * Time.deltaTime);
            yield return null;
        }
    }

    IEnumerator GazeConfusionLoop()
    {
        if (_iAmBound)            yield break;
        if (_boundPlayer == null) yield break;
        if (GameObject.FindGameObjectsWithTag("Player").Length <= 1) yield break;
        if (_localPlayerObj == null) yield break;

        while (_isActive)
        {
            bool isGazing = IsLookingAt(_localPlayerObj, _boundPlayer, gazeAngleThreshold);

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
        if (_localCharCon != null) _localCharCon.enabled = !lockMove;
        if (Managers.Input != null) Managers.Input.enabled = !lockMove;
    }

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

        if (_warningEffect != null)   Destroy(_warningEffect);
        if (_bindEffect != null)      Destroy(_bindEffect);
        if (_bindBarInstance != null) Destroy(_bindBarInstance);
    }

    void OnDestroy()
    {
        _isActive = false;
        EndGimmick();
    }
}