using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class TestInputAction 
{
    public InputAction inputAction;
    public UnityEvent OnInputActionPerformed;


}
public class Test : MonoBehaviour
{
    public InputAction inputAction;

    public UnityEvent unityEvent;


    public void OnEnable()
    {

    }

    public void OnDisable()
    {
        // inputAction.Disable();
    }

    public void Performed(InputAction.CallbackContext context)
    {
    }
}
