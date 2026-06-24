using UnityEngine;

public class AnimatorMoveProxy : MonoBehaviour
{
    private Animator anim;
    private CharacterController parentController; // 또는 Rigidbody
    private Transform parentTransform;

    void Start()
    {
        anim = GetComponent<Animator>();
        
        // 부모 오브젝트의 컴포넌트들을 참조합니다.
        parentTransform = transform.parent;
        if (parentTransform != null)
        {
            parentController = parentTransform.GetComponent<CharacterController>();
        }
    }

    void OnAnimatorMove()
    {
        if (parentTransform == null) return;

        // 1. 부모에게 CharacterController가 있는 경우
        if (parentController != null && parentController.enabled)
        {
            // 애니메이션의 이동량(deltaPosition)을 부모 컨트롤러에 전달하여 이동시킵니다.
            // 시간당 이동량이므로 Move 메서드에 그대로 넣어줍니다.
            parentController.Move(anim.deltaPosition);
            
            // 회전도 부모에게 적용합니다.
            parentTransform.rotation *= anim.deltaRotation;
        }
        // 2. 만약 부모를 그냥 Transform이나 Rigidbody로 움직인다면
        else
        {
            parentTransform.position += anim.deltaPosition;
            parentTransform.rotation *= anim.deltaRotation;
        }

        // 3. [중요] 자식 오브젝트 자체의 로컬 좌표는 항상 (0, 0, 0)으로 고정합니다.
        // 부모가 움직였으므로 자식은 부모 중심에 그대로 붙어있어야 합니다.
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }
}