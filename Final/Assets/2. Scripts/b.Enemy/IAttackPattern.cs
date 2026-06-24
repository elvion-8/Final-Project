using System.Collections;
using UnityEngine;

// ============================================================
//  IAttackPattern
//  모든 패턴 프리팹에 붙는 컴포넌트가 구현하는 인터페이스
// ============================================================
public interface IAttackPattern
{
    /// <summary>
    /// EnemyCtrl이 Instantiate 직후 Enemy 정보를 주입
    /// </summary>
    void SetContext(Transform enemyTr, Transform traceTarget);

    /// <summary>
    /// 패턴 실행. EnemyCtrl.AttackCoroutine에서 yield return으로 호출.
    /// </summary>
    IEnumerator Execute();
}