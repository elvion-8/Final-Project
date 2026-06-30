using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class Skill : MonoBehaviour
{
    //Axe ÆÄÆ¼Å¬
    public ParticleSystem rockParticle;

    public Transform playerPoint;

    

    public void PlayRockParticle()
    {
        playerPoint = transform.root.GetComponentInChildren<PlayerCtrl>().transform;

        if(rockParticle != null && playerPoint != null)
        {
            Vector3 usPoint = playerPoint.position + (playerPoint.forward * 3.5f);

            ParticleSystem instance = Instantiate(rockParticle, usPoint, playerPoint.rotation);
            instance.Play();

            Destroy(instance.gameObject, 1);

        }
    }

}
