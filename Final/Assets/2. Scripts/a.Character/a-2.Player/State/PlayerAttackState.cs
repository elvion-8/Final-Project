using UnityEngine;

public class PlayerAttackState : StateMachineBehaviour
{
    protected PlayerCtrl _player;
    protected bool isLocalPlayer;

    override public void OnStateEnter(Animator anim, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _player = anim.GetComponentInParent<PlayerCtrl>();
        if (_player == null)
        {
            _player = anim.GetComponent<PlayerCtrl>();
        }
        isLocalPlayer = (_player != null && _player == PlayerCtrl.localPlayer);
    }
}
