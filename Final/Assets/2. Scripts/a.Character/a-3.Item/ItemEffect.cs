using UnityEngine;

public abstract class ItemEffect : ScriptableObject
{
    public abstract void OnApply(PlayerCtrl player);
    public abstract void OnRemove(PlayerCtrl player);
}
