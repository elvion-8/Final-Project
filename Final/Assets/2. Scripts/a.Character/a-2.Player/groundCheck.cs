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

    private PlayerCtrl playerCtrl;

    void Awake()
    {
        player = GetComponentInParent<CharacterController>();
        playerCtrl = GetComponentInParent<PlayerCtrl>();
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    void Update()
    {
        bool grounded = CheckGround();

        if (playerCtrl != null && playerCtrl.isAttacking)
        {
            if (grounded)
            {
                isGrounded = true;
            }
            return;
        }

        isGrounded = grounded;
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
