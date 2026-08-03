using System.Collections;
using UnityEngine;

public class EnemyDeath : MonoBehaviour
{
    [Header("Next Phase Prefabs")]
    public GameObject explosionVFX;       // 폭발 VFX
    public GameObject BlackHole;          // 블랙홀 프리팹

    [Header("Animation Settings")]
    [Tooltip("빨려 들어가는 연출의 총 지속 시간(초)")]
    public float suckInDuration = 2.5f;  

    private void Start()
    {
        StartCoroutine(SuckInAndExplode());
    }

    private IEnumerator SuckInAndExplode()
    {
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;

        while (elapsed < suckInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / suckInDuration; 

            // 회전 연출
            float spinSpeed = Mathf.Lerp(500f, 3000f, t);
            transform.Rotate(Vector3.up * spinSpeed * Time.deltaTime, Space.World); 
            
            // 스케일 축소 연출 (마지막에 확 빨려들어가는 느낌 유지)
            float scaleProgress = 1f - Mathf.Pow(t, 3);
            transform.localScale = startScale * scaleProgress;

            yield return null;
        }

        // 연출 종료 시 폭발 소환 및 루프/삭제 제어
        if (explosionVFX != null)
        {
            GameObject vfxInstance = Instantiate(explosionVFX, transform.position, Quaternion.identity);
            
            // 파티클 시스템 컴포넌트를 가져와서 설정 제어
            ParticleSystem ps = vfxInstance.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.loop = false; // 강제로 루프 해제
                Destroy(vfxInstance, main.duration); // 파티클 재생 시간(duration)이 끝나면 오브젝트 삭제
            }
            else
            {
                Destroy(vfxInstance, 3f);
            }
        }

        // 블랙홀 소환
        if (BlackHole != null)
        {
            Instantiate(BlackHole, transform.position, transform.rotation);
        }

        if (PhotonNetwork.isMasterClient)
        {
            // 씬에서 Portal 오브젝트를 찾기(Reasources의 Portal아님)
            GameObject spawnerObj = GameObject.FindWithTag("Portal");
            if (spawnerObj != null)
            {
                PortalSpawner spawner = spawnerObj.GetComponent<PortalSpawner>();
                if (spawner != null)
                {
                    spawner.SpawnNetworkPortal();
                }
            }
            else
            {
                Debug.LogWarning("씬에 'Portal' 태그를 가진 오브젝트가 없습니다!");
            }
        }

        // 연출용 오브젝트 삭제
        Destroy(gameObject);
    }
}