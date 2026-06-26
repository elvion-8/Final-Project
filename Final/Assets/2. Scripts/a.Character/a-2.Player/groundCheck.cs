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
        Vector3 origin = colCenter + Vector3.down * (player.height / 2f) + Vector3.up * 0.1f;
        float sphereRadius = Mathf.Max(0.01f, player.radius - radiusOffset);
        float maxDistance = castDist + 0.1f;
        return Physics.SphereCast(origin, sphereRadius, Vector3.down, out RaycastHit hit, maxDistance, groundMask);
    }

    private void OnDrawGizmosSelected()
    {
        if (player == null)
        {
            player = GetComponentInParent<CharacterController>();
        }
        if (player == null) return;

        Vector3 controllerCenter = player.transform.TransformPoint(player.center);
        Vector3 origin = controllerCenter + Vector3.down * (player.height / 2f) + Vector3.up * 0.1f;

        float sphereRadius = Mathf.Max(0.01f, player.radius - radiusOffset);
        float maxDistance = castDist + 0.1f;
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
