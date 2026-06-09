using System.Collections;
using UnityEngine;

public class Skill : MonoBehaviour
{
    //Axe ÆÄÆ¼Å¬
    public ParticleSystem rockParticle;

    public Transform playerPoint;

    

    public void PlayRockParticle()
    {
        playerPoint = GameObject.Find("Player").transform;

        if(rockParticle != null && playerPoint != null)
        {
            Vector3 usPoint = playerPoint.position + (playerPoint.forward * 2.5f);

            ParticleSystem instance = Instantiate(rockParticle, usPoint, playerPoint.rotation);
            instance.Play();

            Destroy(instance.gameObject, 1);

        }
    }

}
