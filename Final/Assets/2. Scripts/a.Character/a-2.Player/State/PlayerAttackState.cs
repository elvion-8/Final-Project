using UnityEngine;

public class PlayerAttackState : StateMachineBehaviour
{
    protected PlayerCtrl _player;
    override public void OnStateEnter(Animator anim, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _player = anim.GetComponent<PlayerCtrl>();
    }
}
