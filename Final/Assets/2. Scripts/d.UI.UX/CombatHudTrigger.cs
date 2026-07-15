using UnityEngine;

public class CombatHudTrigger : MonoBehaviour
{
    void OnEnable()  => HUDFadeManager.Instance?.EnterCombat(this);
    void OnDisable() => HUDFadeManager.Instance?.ExitCombat(this);
}