using System.Collections;
using UnityEngine;
using FIMSpace.FTail;

public class TentacleIKController : MonoBehaviour
{
    [Header("제어할 모든 촉수 다리들")]
    [Tooltip("배열의 순서(Element 0, 1, 2...)가 다리의 고유 번호가 됩니다.")]
    public TailAnimator2[] tentacles;

    [Header("IK 제어 설정")]
    public float smoothStopDuration = 1.0f;

    // ============================================================
    // [초기화] 게임 시작 시 기본 상태 설정
    // ============================================================
    private void Start()
    {
        // 게임이 시작될 때 모든 다리의 추적(IK) 기능을 즉시 비활성화합니다.
        if (tentacles == null) return;

        foreach (var tail in tentacles)
        {
            if (tail != null)
            {
                tail.IKBlend = 0f;
                tail.UseIK = false;
            }
        }
    }

    // ============================================================
    // [전체 제어] 모든 촉수 통제
    // ============================================================
    public void StopAllTentaclesIK()
    {
        StartCoroutine(SmoothStopAllIKRoutine(smoothStopDuration));
    }

    public void StartAllTentaclesIK()
    {
        foreach (var tail in tentacles)
        {
            if (tail != null)
            {
                tail.UseIK = true;
                tail.IKBlend = 1f;
                tail.IKSetCustomPosition(null);
            }
        }
    }

    public void AttackTargetPositionAll(Vector3 targetPos)
    {
        foreach (var tail in tentacles)
        {
            if (tail != null)
            {
                tail.UseIK = true;
                tail.IKBlend = 1f;
                tail.IKSetCustomPosition(targetPos);
            }
        }
    }

    // ============================================================
    // [개별 제어] 특정 번호(index)의 촉수만 통제
    // ============================================================
    
    // 특정 다리 추적 시작
    public void StartTentacleIK(int index)
    {
        if (IsValidIndex(index))
        {
            tentacles[index].UseIK = true;
            tentacles[index].IKBlend = 1f;
            tentacles[index].IKSetCustomPosition(null);
        }
    }

    // 특정 다리 추적 정지
    public void StopTentacleIK(int index)
    {
        if (IsValidIndex(index))
        {
            StartCoroutine(SmoothStopSingleIKRoutine(index, smoothStopDuration));
        }
    }

    // 특정 다리만 목표 좌표로 공격(뻗기)
    public void AttackTargetPositionSingle(int index, Vector3 targetPos)
    {
        if (IsValidIndex(index))
        {
            tentacles[index].UseIK = true;
            tentacles[index].IKBlend = 1f;
            tentacles[index].IKSetCustomPosition(targetPos);
        }
    }

    // ────────────────────────────────────────────
    // 내부 헬퍼 함수 및 코루틴
    // ────────────────────────────────────────────
    
    // 인덱스가 배열 범위 내에 있는지 안전하게 검사
    private bool IsValidIndex(int index)
    {
        if (tentacles == null || index < 0 || index >= tentacles.Length)
        {
            Debug.LogWarning($"[TentacleIKController] {index}번 다리는 존재하지 않습니다! 인스펙터 배열을 확인하세요.");
            return false;
        }
        return tentacles[index] != null;
    }

    // 특정 다리 하나만 부드럽게 멈추는 코루틴
    private IEnumerator SmoothStopSingleIKRoutine(int index, float duration)
    {
        TailAnimator2 tail = tentacles[index];
        float startBlend = tail.IKBlend;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            tail.IKBlend = Mathf.Lerp(startBlend, 0f, elapsed / duration);
            yield return null;
        }

        tail.IKBlend = 0f;
        tail.UseIK = false;
    }

    // 모든 다리 부드럽게 멈추는 코루틴 (기존과 동일)
    private IEnumerator SmoothStopAllIKRoutine(float duration)
    {
        if (tentacles == null || tentacles.Length == 0) yield break;

        float elapsed = 0f;
        float[] startBlends = new float[tentacles.Length];
        
        for (int i = 0; i < tentacles.Length; i++)
        {
            if (tentacles[i] != null) startBlends[i] = tentacles[i].IKBlend;
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            for (int i = 0; i < tentacles.Length; i++)
            {
                if (tentacles[i] != null)
                    tentacles[i].IKBlend = Mathf.Lerp(startBlends[i], 0f, t);
            }
            yield return null;
        }

        foreach (var tail in tentacles)
        {
            if (tail != null)
            {
                tail.IKBlend = 0f;
                tail.UseIK = false;
            }
        }
    }

    // ============================================================
    // [타겟 변경 및 그룹 제어] 특정 다리(들)의 타겟을 변경하고 추적
    // ============================================================

    /// <summary>
    /// 특정 다리 하나만 새로운 타겟을 쫓게 합니다.
    /// </summary>
    /// <param name="index">다리 번호</param>
    /// <param name="newTarget">새롭게 쫓을 타겟 오브젝트</param>
    /// <param name="stopOthers">나머지 다리들의 IK를 끌 것인지 여부 (기본값: true)</param>
    public void SetTargetAndFollowSingle(int index, Transform newTarget, bool stopOthers = true)
    {
        // 1. 나머지 다리들을 모두 정지시킴
        if (stopOthers) 
        {
            StopAllTentaclesIK();
        }

        // 2. 지정한 다리만 타겟을 변경하고 활성화
        if (IsValidIndex(index))
        {
            tentacles[index].IKTarget = newTarget; // 새로운 타겟 오브젝트 할당
            tentacles[index].UseIK = true;
            tentacles[index].IKBlend = 1f;
            tentacles[index].IKSetCustomPosition(null); // 특정 좌표 고정을 풀고 오브젝트를 쫓게 함
        }
    }

    /// <summary>
    /// 여러 개의 다리(그룹)를 동시에 새로운 타겟을 쫓게 합니다.
    /// </summary>
    /// <param name="indices">다리 번호들의 배열 (예: {0, 1, 2})</param>
    /// <param name="newTarget">새롭게 쫓을 타겟 오브젝트</param>
    /// <param name="stopOthers">나머지 다리들의 IK를 끌 것인지 여부 (기본값: true)</param>
    public void SetTargetAndFollowGroup(int[] indices, Transform newTarget, bool stopOthers = true)
    {
        // 1. 나머지 다리들을 모두 정지시킴
        if (stopOthers) 
        {
            StopAllTentaclesIK();
        }

        // 2. 배열로 전달받은 다리들만 타겟을 변경하고 활성화
        foreach (int i in indices)
        {
            if (IsValidIndex(i))
            {
                tentacles[i].IKTarget = newTarget;
                tentacles[i].UseIK = true;
                tentacles[i].IKBlend = 1f;
                tentacles[i].IKSetCustomPosition(null);
            }
        }
    }
}