using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponVFXState : StateMachineBehaviour
{
    protected GameObject trail;
    protected ParticleSystem _particle;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (trail == null) 
            trail = FindChildWithTag(animator.gameObject, "Trail");

        if (trail != null && _particle == null)
        {
            _particle = trail.GetComponent<ParticleSystem>();
        }
    }

    protected Transform GetWeaponTransform(Animator animator)
    {
        if (trail != null) return trail.transform.parent;
        return animator.transform;
    }

    private GameObject FindChildWithTag(GameObject go, string tag)
    {
        Transform[] allGo = go.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allGo)
        {
            if (child.CompareTag(tag))
            {
                return child.gameObject;
            }
        }
        return null;
    }

    public void SetTrailActive(bool active)
    {
        if (trail == null) return;
        if (active)
        {
            trail.SetActive(true);

            if (_particle != null)
            {
                _particle.Play(true);
            }
        }
        else
        {
            if (_particle != null)
            {
                _particle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
            else 
            {
                trail.SetActive(false);
            }
        }
    }

    protected GameObject SpawnSwingVFX(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (VFXManager.Instance == null) return null;
        return VFXManager.Instance.PlayVFX(prefab, position, rotation, parent);
    }

    protected GameObject SpawnHitVFX(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (VFXManager.Instance == null) return null;
        return VFXManager.Instance.PlayVFX(prefab, position, rotation, null);
    }
}
