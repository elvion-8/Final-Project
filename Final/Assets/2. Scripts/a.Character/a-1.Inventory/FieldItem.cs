using UnityEngine;

public class FieldItem : MonoBehaviour
{
    [Header("아이템 설정")]
    public ItemData itemData;

    [Header("효과 설정 (선택 사항)")]
    [Tooltip("획득 시 재생할 오디오 클립")]
    public AudioClip pickupSound;
    [Tooltip("획득 시 생성할 시각 효과 프리팹")]
    public GameObject pickupEffectPrefab;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (itemData == null) return;
            bool success = csInvenManager.Instance.AddItem(itemData);
            if (success)
            {
                PlayPickupEffects();

                Destroy(gameObject);
            }
        }
    }

    private void PlayPickupEffects()
    {
        // 사운드 재생
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }

        // VFX 생성
        if (pickupEffectPrefab != null)
        {
            GameObject vfx = Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
            Destroy(vfx, 2f);
        }
    }
}
