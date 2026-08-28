using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Custom/Invoker Key Switch")]
public class InvokerKeySwitch : MonoBehaviour
{
    [Header("⚠️ Help ⚠️")]
    [Header("Toggle events on key press or release.")]
    [Header("OnKeyDownOn/Off triggers when the key is pressed, toggling the state.")]
    [Header("OnKeyUpOn/Off triggers when the key is released, toggling the state.")]
    [Header("Useful for switches, lights, or any on/off mechanics.")]
    [Header("------------------------------------------------------------------------")]

    [Header("Key Down Settings")]
    [Tooltip("Initial state for key down toggle.")]
    public bool onKeyDownIsOn;

    [Tooltip("The key to detect for key down events.")]
    public KeyCode onKeyDownKeyCode;

    [Tooltip("Event invoked when key down toggles on.")]
    public UnityEvent onKeyDownOn;

    [Tooltip("Event invoked when key down toggles off.")]
    public UnityEvent onKeyDownOff;

    [Header("Key Up Settings")]
    [Tooltip("Initial state for key up toggle.")]
    public bool onKeyUpIsOn;

    [Tooltip("The key to detect for key up events.")]
    public KeyCode onKeyUpKeyCode;

    [Tooltip("Event invoked when key up toggles on.")]
    public UnityEvent onKeyUpOn;

    [Tooltip("Event invoked when key up toggles off.")]
    public UnityEvent onKeyUpOff;

    private void Update()
    {
        // Key Down toggle
        if (!onKeyDownIsOn)
        {
            if (Input.GetKeyDown(onKeyDownKeyCode))
            {
                onKeyDownOn.Invoke();
                onKeyDownIsOn = true;
            }
        }
        else
        {
            if (Input.GetKeyDown(onKeyDownKeyCode))
            {
                onKeyDownOff.Invoke();
                onKeyDownIsOn = false;
            }
        }

        // Key Up toggle
        if (!onKeyUpIsOn)
        {
            if (Input.GetKeyUp(onKeyUpKeyCode))
            {
                onKeyUpOn.Invoke();
                onKeyUpIsOn = true;
            }
        }
        else
        {
            if (Input.GetKeyUp(onKeyUpKeyCode))
            {
                onKeyUpOff.Invoke();
                onKeyUpIsOn = false;
            }
        }
    }
}