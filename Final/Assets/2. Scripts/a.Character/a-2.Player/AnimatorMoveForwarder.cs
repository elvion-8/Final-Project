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

            // 클라이밍 상태가 아닐 때만 Y축 이동을 스크립트 값으로 덮어씌웁니다.
            if (!player.IsClimbing)
            {
                deltaPosition.y = player.MoveDir.y * Time.deltaTime;
            }

            charCon.Move(deltaPosition);

            charCon.transform.rotation *= anim.deltaRotation;
        }
    }
}
