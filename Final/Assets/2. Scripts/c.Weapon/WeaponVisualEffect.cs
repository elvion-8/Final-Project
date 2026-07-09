using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeaponVisualEffect : MonoBehaviour
{
    [Header("Shader Settings")]
    [SerializeField] private string dissolvePropertyName = "_DissolveAmount";
    [SerializeField] private float disappearVfxDuration = 1.5f;
    [SerializeField] private float appearVfxDuration = 1f;

    [Header("Movement Settings (Disappear)")]
    [SerializeField] private float floatUpDistance = 0.4f; // 소멸 시 떠오를 높이
    [SerializeField] private float rotationSpeed = 90f;     // 소멸 시 회전 각도

    [Header("VFX Prefabs")]
    [SerializeField] private GameObject appearVFXPrefab;
    [SerializeField] private GameObject disappearVFXPrefab;

    private List<Material> materials = new List<Material>();
    private Collider[] weaponColliders;

    void Awake()
    {
        // 무기와 자식 오브젝트의 모든 렌더러에서 머티리얼 수집
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        foreach (Renderer ren in renderers)
        {
            foreach (Material mat in ren.materials)
            {
                if (mat.HasProperty(dissolvePropertyName))
                {
                    materials.Add(mat);
                }
            }
        }

        weaponColliders = GetComponentsInChildren<Collider>(true);
    }

    /// 무기 소환 이펙트 재생
    public void PlayAppearEffect()
    {
        StopAllCoroutines();
        StartCoroutine(CoAppear());
    }

    /// 무기 해제/소멸 이펙트 재생
    public void PlayDisappearEffect()
    {
        transform.SetParent(null);
        
        foreach (Collider col in weaponColliders)
        {
            if (col != null) col.enabled = false;
        }

        StopAllCoroutines();
        StartCoroutine(CoDisappear());
    }

    private IEnumerator CoAppear()
    {
        SetDissolveAmount(1f);

        // 소환 파티클 재생
        if (appearVFXPrefab != null)
        {
            GameObject vfx = Instantiate(appearVFXPrefab, transform.position, transform.rotation, transform);
            Destroy(vfx, appearVfxDuration + 1f);
        }

        float elapsed = 0f;
        while (elapsed < appearVfxDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed /appearVfxDuration;
            // 1->0으로 변경
            SetDissolveAmount(Mathf.Lerp(1f, 0f, normalizedTime));
            yield return null;
        }

        SetDissolveAmount(0f);
    }

    private IEnumerator CoDisappear()
    {
        GameObject vfx = null;
        // 소멸 파티클 재생
        if (disappearVFXPrefab != null)
        {
            vfx = Instantiate(disappearVFXPrefab, transform.position, transform.rotation, transform);
        }

        float elapsed = 0f;
        Vector3 startPos = transform.position;

        while (elapsed < disappearVfxDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / disappearVfxDuration;

            // 0->1으로 변경
            SetDissolveAmount(Mathf.Lerp(0f, 1f, normalizedTime));

            // 허공에 둥실 떠오르며 회전
            transform.position = startPos + Vector3.up * (normalizedTime * floatUpDistance);
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);

            yield return null;
        }

        SetDissolveAmount(1f);

        // 무기의 메쉬 렌더러만 비활성화
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        foreach (Renderer ren in renderers)
        {
            // 파티클 시스템 내의 렌더러는 꺼지지 않도록 보호
            if (vfx == null || !ren.transform.IsChildOf(vfx.transform))
            {
                ren.enabled = false;
            }
        }

        // 파티클 재생 여유 시간 (자연스럽게 사라지게)
        yield return new WaitForSeconds(1.0f);

        Destroy(gameObject);
    }

    private void SetDissolveAmount(float val)
    {
        foreach (Material mat in materials)
        {
            if (mat != null)
            {
                mat.SetFloat(dissolvePropertyName, val);
            }
        }
    }
}
