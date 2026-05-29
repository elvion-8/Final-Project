using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class csGamePadVibMng : MonoBehaviour
{
    public void TriggerVib(float duration, float leftIntensity, float rightIntensity)
    {
        if(Gamepad.current !=null)
        {
            StartCoroutine(VibCoroutine(duration,leftIntensity,rightIntensity));
        }
    }
    private IEnumerator VibCoroutine(float duration,float leftIntensity,float rightIntensity)
    {
        Gamepad.current.SetMotorSpeeds(leftIntensity,rightIntensity);

        yield return new WaitForSeconds(duration);
        Gamepad.current.ResetHaptics();
    }
    private void OnDisable()
    {
        if(Gamepad.current !=null)
        {
            Gamepad.current.ResetHaptics();
        }
    }
}
