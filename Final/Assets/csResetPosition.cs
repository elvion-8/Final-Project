using UnityEngine;

public class csResetPosition : MonoBehaviour
{
    public Transform playerPos;
    public Animator playerAnim;

    void Awake()
    {
        playerPos = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        playerAnim = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<Animator>();
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
