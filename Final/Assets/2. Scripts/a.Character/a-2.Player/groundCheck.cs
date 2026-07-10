using System;
using UnityEngine;

public class groundCheck : MonoBehaviour
{
    public bool isGrounded;
    [SerializeField] private CharacterController player;

    [Header("Ground Check")]
    public LayerMask groundMask;
    public float castDist = 0.2f;
    public float radiusOffset = 0.05f;

    private bool isAttackLock = false;
    private bool lockedGroundedValue = false;

    void Awake()
    {
        player = GetComponentInParent<CharacterController>();
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // 공격 중(Lock)일 때는 물리 검사를 하지 않고, 공격 시작 시점의 값을 그대로 유지
        if (isAttackLock)
        {
            isGrounded = lockedGroundedValue;
        }
        else
        {
            isGrounded = CheckGround();
        }
    }

    public void LockGroundState()
    {
        // 공격 직전의 진짜 물리적인 지면 상태를 체크해서 저장
        lockedGroundedValue = CheckGround();
        isAttackLock = true;
        isGrounded = lockedGroundedValue; // 즉시 반영
    }

    public void UnlockGroundState()
    {
        isAttackLock = false;
    }

    bool CheckGround()
    {
        if (player == null) return false;

        Vector3 colCenter = player.transform.TransformPoint(player.center);
        Vector3 feetPosition = colCenter + Vector3.down * (player.height / 2f);
        float sphereRadius = Mathf.Max(0.01f, player.radius - radiusOffset);
        
        Vector3 origin = feetPosition + Vector3.up * (sphereRadius + 0.05f);
        float maxDistance = 0.05f + castDist;

        if (Physics.SphereCast(origin, sphereRadius, Vector3.down, out RaycastHit hit, maxDistance, groundMask))
        {
            float slopeLimit = player.slopeLimit;
            float minNormalY = Mathf.Cos(slopeLimit * Mathf.Deg2Rad);

            if (hit.normal.y >= minNormalY)
            {
                return true;
            }
        }
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (player == null)
        {
            player = GetComponentInParent<CharacterController>();
        }
        if (player == null) return;

        Vector3 colCenter = player.transform.TransformPoint(player.center);
        Vector3 feetPosition = colCenter + Vector3.down * (player.height / 2f);
        float sphereRadius = Mathf.Max(0.01f, player.radius - radiusOffset);
        Vector3 origin = feetPosition + Vector3.up * (sphereRadius + 0.05f);
        float maxDistance = 0.05f + castDist;
        Vector3 endPoint = origin + Vector3.down * maxDistance;

        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(origin, sphereRadius);
        Gizmos.DrawLine(origin, endPoint);
        Gizmos.DrawWireSphere(endPoint, sphereRadius);

        if (isGrounded)
        {
            if (Physics.SphereCast(origin, sphereRadius, Vector3.down, out RaycastHit hit, maxDistance, groundMask))
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(hit.point, 0.05f);
                Gizmos.DrawRay(hit.point, hit.normal * 0.2f);
            }
        }
    }
}
