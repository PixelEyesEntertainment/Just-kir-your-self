using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Custom/Invoker Key")]
public class InvokerKey : MonoBehaviour
{
    [Header("⚠️ Help ⚠️")]
    [Header("Invoke events when specified keys are pressed or released.")]
    [Header("OnKeyDown is triggered when the key is pressed.")]
    [Header("OnKeyUp is triggered when the key is released.")]
    [Header("Useful for instant reactions without using RayCaster.")]
    [Header("------------------------------------------------------------------------")]

    [Header("Key Down Settings")]
    [Tooltip("The key to detect for key down events.")]
    public KeyCode onKeyDownKeyCode;

    [Tooltip("Event invoked when key is pressed down.")]
    public UnityEvent onKeyDown;

    [Header("Key Up Settings")]
    [Tooltip("The key to detect for key up events.")]
    public KeyCode onKeyUpKeyCode;

    [Tooltip("Event invoked when key is released.")]
    public UnityEvent onKeyUp;

    private void Update()
    {
        if (Input.GetKeyDown(onKeyDownKeyCode))
        {
            onKeyDown.Invoke();
        }

        if (Input.GetKeyUp(onKeyUpKeyCode))
        {
            onKeyUp.Invoke();
        }
    }
}