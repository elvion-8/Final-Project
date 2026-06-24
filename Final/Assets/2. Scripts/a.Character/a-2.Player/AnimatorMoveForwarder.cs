using UnityEngine;

public class AnimatorMoveForwarder : MonoBehaviour
{
    private Animator anim;
    private CharacterController charCon;
    private PlayerCtrl player;

    void Awake()
    {
        anim = GetComponent<Animator>();
        charCon = GetComponentInParent<CharacterController>();
        player = GetComponentInParent<PlayerCtrl>();
    }

    void OnAnimatorMove()
    {
        if (anim == null || charCon == null || player == null) return;

        if (anim.applyRootMotion)
        {
            Vector3 deltaPosition = anim.deltaPosition;

            deltaPosition.y = player.MoveDir.y * Time.deltaTime;

            charCon.Move(deltaPosition);

            charCon.transform.rotation *= anim.deltaRotation;
        }
    }
}
