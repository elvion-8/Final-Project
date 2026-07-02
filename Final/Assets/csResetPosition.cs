using UnityEngine;
using System.Collections;

public class csResetPosition : MonoBehaviour
{
    public Transform playerPos;
    public Animator playerAnim;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(0.5f);
        playerPos = GameObject.Find("Player").GetComponent<Transform>();
        playerAnim = GameObject.Find("Player").GetComponentInChildren<Animator>();

    }

    void OnTriggerEnter(Collider col)
    {
        if(col.CompareTag("Player"))
        {
            CharacterController playerCtrl = col.GetComponent<CharacterController>();
            if(playerCtrl != null)
            {
                playerCtrl.enabled = false;
                playerPos.position = Vector3.zero;
                playerAnim.SetBool("Run",false);
                playerAnim.SetBool("Walk",false);
                playerCtrl.enabled = true;
            }
            else playerPos.position = Vector3.zero;
            
        }
    }
}
