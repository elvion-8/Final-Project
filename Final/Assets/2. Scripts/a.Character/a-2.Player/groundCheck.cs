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

    void Awake()
    {
        player = GetComponentInParent<CharacterController>();
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = CheckGround();
    }

    bool CheckGround()
    {
        if (player == null) return false;

        Vector3 colCenter = player.transform.TransformPoint(player.center);
        Vector3 feetPosition = colCenter + Vector3.down * (player.height / 2f);
        float sphereRadius = Mathf.Max(0.01f, player.radius - radiusOffset);
        
        // 바닥보다 약간 위에서 sphere가 시작되도록 설정하여 시작 지점 겹침(Overlap) 현상을 방지합니다.
        Vector3 origin = feetPosition + Vector3.up * (sphereRadius + 0.05f);
        float maxDistance = 0.05f + castDist;

        if (Physics.SphereCast(origin, sphereRadius, Vector3.down, out RaycastHit hit, maxDistance, groundMask))
        {
            // 경사각 체크: 충돌한 표면의 노멀 벡터 Y 성분이 캐릭터의 경사 제한(slopeLimit) 이하일 때만 땅으로 판정합니다.
            // (벽면이나 장애물의 수직면에 비벼질 때 ground가 true가 되는 현상을 방지)
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
