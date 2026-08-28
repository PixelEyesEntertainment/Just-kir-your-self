using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Custom/Invoker Trigger")]
public class InvokerTrigger : MonoBehaviour
{
    [Header("⚠️ Help ⚠️")]
    [Header("Invoke events when an object with a specified tag enters or exits this trigger.")]
    [Header("Useful for area-based triggers, traps, or interactive zones.")]
    [Header("------------------------------------------------------------------------")]

    [Header("Settings")]
    [Tooltip("Delay before invoking enter/exit events.")]
    public float invokeDelay = 0f;

    [Tooltip("Tag of the object that can trigger events.")]
    public string tag = "Player";

    [Header("Events")]
    [Tooltip("Event invoked when object enters the trigger area.")]
    public UnityEvent onEnterTriggerArea;

    [Tooltip("Event invoked when object exits the trigger area.")]
    public UnityEvent onExitTriggerArea;

    public void SetInvokeDelay(float invokeDelay_)
    {
        invokeDelay = invokeDelay_;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(tag))
            StartCoroutine(OnEnterDelay());
    }

    private IEnumerator OnEnterDelay()
    {
        yield return new WaitForSeconds(invokeDelay);
        onEnterTriggerArea.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(tag))
            StartCoroutine(OnExitDelay());
    }

    private IEnumerator OnExitDelay()
    {
        yield return new WaitForSeconds(invokeDelay);
        onExitTriggerArea.Invoke();
    }
}