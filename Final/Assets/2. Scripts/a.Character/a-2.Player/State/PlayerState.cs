using System.Collections.Generic;
using UnityEngine;

public enum PhysicsDir
{
    Forward, Back, Left, Right, Up, Down, Custom
}

[System.Serializable]
public class PhysicsTimeLineData
{
    public string label = "물리 설정";
    [Range(0f, 1f), Tooltip("start Time")]
    public float startTime = 0.1f;
    [Range(0f, 1f), Tooltip("end Time")]
    public float endTime = 0.3f;

    public PhysicsDir direction = PhysicsDir.Forward;
    [Tooltip("custom전용")]
    public Vector3 customDir = Vector3.forward;

    public float force = 10f;
    public bool isImpulse = false;
    [HideInInspector] public bool isTriggered = false;
}

public class PlayerState : PlayerAttackState
{
    [Header("Root Motion Settings")]
    [Tooltip("이 애니메이션 상태 진입 시 루트 모션을 강제로 끌지 여부")]
    public bool disableRootMotion = true;

    [Header("Physics Settings (Character Controller)")]
    [Tooltip("CharacterController 환경에서 수평 Impulse 힘의 감속 속도")]
    public float horizontalDrag = 5f;

    [Header("Combo Settings")]
    public bool resetComboOnExit = false;

    public List<PhysicsTimeLineData> physicsEvents = new List<PhysicsTimeLineData>();
    
    private Rigidbody rb;
    private CharacterController cc;
    private bool originalRootMotionState;
    private Vector3 scriptVelocity;
    private float lastNormalizedTime;

    public override void OnStateEnter(Animator anim, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(anim, stateInfo, layerIndex);

        // Find PlayerCtrl
        if (_player == null)
        {
            _player = anim.GetComponentInParent<PlayerCtrl>();
        }

        // Find Components
        rb = anim.GetComponent<Rigidbody>();
        if (rb == null) rb = anim.GetComponentInParent<Rigidbody>();

        cc = anim.GetComponent<CharacterController>();
        if (cc == null) cc = anim.GetComponentInParent<CharacterController>();

        // Disable Root Motion
        if (disableRootMotion)
        {
            originalRootMotionState = anim.applyRootMotion;
            anim.applyRootMotion = false;
        }

        // Init
        scriptVelocity = Vector3.zero;
        lastNormalizedTime = 0f;

        foreach (var ev in physicsEvents)
        {
            ev.isTriggered = false;
        }
    }

    public override void OnStateUpdate(Animator anim, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(anim, stateInfo, layerIndex);

        if (rb == null && cc == null) return;
        
        float currentTime = stateInfo.normalizedTime % 1f;

        // Reset triggers on loop
        if (stateInfo.normalizedTime < lastNormalizedTime || (lastNormalizedTime % 1f > currentTime))
        {
            foreach (var ev in physicsEvents)
            {
                ev.isTriggered = false;
            }
        }
        lastNormalizedTime = stateInfo.normalizedTime;

        bool isAnyVelocityEventActive = false;
        Vector3 targetVelocity = Vector3.zero;
        Transform referenceTransform = anim.transform.parent != null ? anim.transform.parent : anim.transform;

        foreach (var ev in physicsEvents)
        {
            if (currentTime >= ev.startTime && currentTime <= ev.endTime)
            {
                Vector3 moveDir = GetDirectionVector(referenceTransform, ev);

                if (ev.isImpulse)
                {
                    if (!ev.isTriggered)
                    {
                        if (rb != null)
                        {
                            rb.AddForce(moveDir * ev.force, ForceMode.Impulse);
                        }
                        else if (cc != null)
                        {
                            if (ev.direction == PhysicsDir.Up)
                            {
                                if (_player != null) _player.MoveDir.y = ev.force;
                            }
                            else if (ev.direction == PhysicsDir.Down)
                            {
                                if (_player != null) _player.MoveDir.y = -ev.force;
                            }
                            else
                            {
                                scriptVelocity += moveDir * ev.force;
                            }
                        }
                        ev.isTriggered = true;
                    }
                }
                else
                {
                    Vector3 currentEventVelocity = moveDir * ev.force;
                    
                    if (rb != null)
                    {
                        if (ev.direction != PhysicsDir.Up && ev.direction != PhysicsDir.Down)
                        {
                            currentEventVelocity.y = rb.velocity.y;
                        }
                        targetVelocity = currentEventVelocity;
                        isAnyVelocityEventActive = true;
                    }
                    else if (cc != null)
                    {
                        if (ev.direction == PhysicsDir.Up || ev.direction == PhysicsDir.Down)
                        {
                            if (_player != null) _player.MoveDir.y = currentEventVelocity.y;
                        }
                        else
                        {
                            targetVelocity = currentEventVelocity;
                            isAnyVelocityEventActive = true;
                        }
                    }
                }
            }
        }

        // Apply physics
        if (rb != null)
        {
            if (isAnyVelocityEventActive)
            {
                rb.velocity = targetVelocity;
            }
            else
            {
                if (rb.velocity.magnitude > 0.1f && rb.velocity.y <= 0.1f) 
                {
                    rb.velocity = new Vector3(0, rb.velocity.y, 0);
                }
            }
        }
        else if (cc != null)
        {
            // Apply constant velocity
            if (isAnyVelocityEventActive)
            {
                cc.Move(targetVelocity * Time.deltaTime);
            }
            
            // Apply impulse velocity with drag decay
            Vector3 horizontalImpulse = new Vector3(scriptVelocity.x, 0f, scriptVelocity.z);
            if (horizontalImpulse.sqrMagnitude > 0.001f)
            {
                cc.Move(horizontalImpulse * Time.deltaTime);
                scriptVelocity = Vector3.MoveTowards(scriptVelocity, Vector3.zero, horizontalDrag * Time.deltaTime);
            }
            else
            {
                scriptVelocity = Vector3.zero;
            }
        }
    }

    public override void OnStateExit(Animator anim, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (rb != null)
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }
        else if (cc != null && _player != null)
        {
            _player.MoveDir = new Vector3(0f, _player.MoveDir.y, 0f);
        }

        if (resetComboOnExit)
        {
            ComboAttack combo = anim.GetComponent<ComboAttack>();
            if (combo == null) combo = anim.GetComponentInParent<ComboAttack>();
            if (combo != null) combo.ResetCombo();
        }

        // Restore Root Motion
        if (disableRootMotion)
        {
            anim.applyRootMotion = originalRootMotionState;
        }
        
        base.OnStateExit(anim, stateInfo, layerIndex);
    }

    private Vector3 GetDirectionVector(Transform t, PhysicsTimeLineData ev)
    {
        switch (ev.direction)
        {
            case PhysicsDir.Forward: return t.forward;
            case PhysicsDir.Back: return -t.forward;
            case PhysicsDir.Left: return -t.right;
            case PhysicsDir.Right: return t.right;
            case PhysicsDir.Up: return t.up;
            case PhysicsDir.Down: return -t.up;
            case PhysicsDir.Custom: return t.TransformDirection(ev.customDir.normalized);
            default: return t.forward;
        }
    }
}