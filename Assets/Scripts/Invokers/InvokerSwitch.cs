using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Custom/Invoker Switch")]
public class InvokerSwitch : MonoBehaviour
{
    [Header("⚠️ Help ⚠️")]
    [Header("Toggle this object on/off using a key press.")]
    [Header("onButtonPressedOn is invoked when switch turns on.")]
    [Header("onButtonPressedOff is invoked when switch turns off.")]
    [Header("Can initialize in on/off state using switchInStart.")]
    [Header("------------------------------------------------------------------------")]

    [Header("Settings")]
    [Tooltip("Delay before invoking switch events.")]
    public float invokeDelay = 0f;

    [Tooltip("Key to toggle switch.")]
    public KeyCode keyCode;

    [Header("Events")]
    [Tooltip("Event invoked when switch turns ON.")]
    public UnityEvent onButtonPressedOn;

    [Tooltip("Event invoked when switch turns OFF.")]
    public UnityEvent onButtonPressedOff;

    [Header("Runtime State")]
    [Tooltip("Current switch state.")]
    public bool isOn;

    private RayCaster ray;
    private bool cantPressTillDelay;

    private void Start()
    {
        ray = FindFirstObjectByType<RayCaster>();
    }

    public void TurnOff()
    {
        isOn = false;
        StartCoroutine(WaitForSec(invokeDelay, onButtonPressedOff));
    }

    public void TurnOn()
    {
        isOn = true;
        StartCoroutine(WaitForSec(invokeDelay, onButtonPressedOn));
    }

    public void SetInvokeDelay(float invokeDelay_)
    {
        invokeDelay = invokeDelay_;
    }

    public void SetIsOnTrue() => isOn = true;
    public void SetIsOnFalse() => isOn = false;

    private IEnumerator WaitForSec(float delay, UnityEvent unityEvent)
    {
        yield return new WaitForSeconds(delay);
        cantPressTillDelay = false;
        unityEvent.Invoke();
    }

    private void Update()
    {
        if (cantPressTillDelay) return;

        if (!isOn)
        {
            if (Input.GetKeyDown(keyCode) && ray.HitObject == gameObject)
            {
                TurnOn();
                cantPressTillDelay = true;
            }
        }
        else
        {
            if (Input.GetKeyDown(keyCode) && ray.HitObject == gameObject)
            {
                TurnOff();
                cantPressTillDelay = true;
            }
        }
    }
}