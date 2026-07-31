using UnityEngine;

public class VenomSymbioteController : MonoBehaviour
{
    [Header("Symbiote Material")]
    public Material symbioteMaterial;

    [Header("Dynamic Dripping & Motion Scaling")]
    public float idleDripSpeed = 2.0f;
    public float moveDripSpeed = 5.0f;
    public float idleDripAmount = 0.08f;
    public float moveDripAmount = 0.22f;

    private SkinnedMeshRenderer[] skinnedRenderers;
    private PlayerCtrl playerCtrl;

    // 위상 연속성을 위한 누적 변수 (리셋 방지)
    private float accumulatedDripPhase = 0f;
    private float smoothedDripAmount = 0.08f;

    void Awake()
    {
        playerCtrl = GetComponentInParent<PlayerCtrl>();
        if (playerCtrl == null) playerCtrl = GetComponent<PlayerCtrl>();

        skinnedRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        ApplySymbioteMaterial();
    }

    void ApplySymbioteMaterial()
    {
        if (symbioteMaterial == null) return;

        foreach (var smr in skinnedRenderers)
        {
            if (smr == null) continue;

            Material[] mats = smr.sharedMaterials;
            bool alreadyApplied = false;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] != null && mats[i].shader != null && mats[i].shader.name == "Custom/VenomSymbioteShader")
                {
                    alreadyApplied = true;
                    break;
                }
            }

            if (!alreadyApplied)
            {
                Material[] newMats = new Material[mats.Length];
                for (int i = 0; i < mats.Length; i++)
                {
                    newMats[i] = symbioteMaterial;
                }
                smr.materials = newMats;
            }
        }
    }

    void Update()
    {
        bool isMoving = false;
        if (playerCtrl != null)
        {
            Vector3 horiz = new Vector3(playerCtrl.MoveDir.x, 0, playerCtrl.MoveDir.z);
            isMoving = horiz.sqrMagnitude > 0.05f || playerCtrl.isAttacking;
        }

        float targetDripSpeed = isMoving ? moveDripSpeed : idleDripSpeed;
        float targetDripAmount = isMoving ? moveDripAmount : idleDripAmount;

        // 1. 위상(Phase) 연속 누적: 속도가 바뀌어도 패턴이 끊기거나 처음부터 리셋되지 않고 끊김 없이 이어서 흘러내림
        accumulatedDripPhase += Time.deltaTime * targetDripSpeed;

        // 2. 흘러내림 두께 부드러운 보간 (Pop 방지)
        smoothedDripAmount = Mathf.Lerp(smoothedDripAmount, targetDripAmount, Time.deltaTime * 4.0f);

        foreach (var smr in skinnedRenderers)
        {
            if (smr == null) continue;

            for (int i = 0; i < smr.materials.Length; i++)
            {
                Material mat = smr.materials[i];
                if (mat != null)
                {
                    if (mat.HasProperty("_DripPhase")) mat.SetFloat("_DripPhase", accumulatedDripPhase);
                    if (mat.HasProperty("_DripAmount")) mat.SetFloat("_DripAmount", smoothedDripAmount);
                }
            }
        }
    }
}
