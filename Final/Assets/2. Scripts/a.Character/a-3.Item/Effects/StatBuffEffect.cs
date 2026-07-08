using UnityEngine;

[CreateAssetMenu(fileName = "New Stat Buff Effect", menuName = "ItemEffects/StatBuff")]
public class StatBuffEffect : ItemEffect
{
    public CharacterStatType statType;
    public float modifierValue;
    [Tooltip("지속 시간 (초 단위, 0 이하는 스테이지/런 종료 시까지 유지)")]
    public float duration = 0f;

    public override void OnApply(PlayerCtrl player)
    {
        var statCtrl = player.GetComponent<PlayerStatController>();
        if (statCtrl != null)
        {
            statCtrl.ApplyBuff(statType, modifierValue, duration);
        }
    }

    public override void OnRemove(PlayerCtrl player)
    {
        var statCtrl = player.GetComponent<PlayerStatController>();
        if (statCtrl != null)
        {
            statCtrl.RemoveBuff(statType, modifierValue);
        }
    }
}
