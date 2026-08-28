using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Custom/Invoker Trigger 2D")]
public class InvokerTrigger2D : MonoBehaviour
{
    [Header("⚠️ Help ⚠️")]
    [Header("Invoke events when an object with a specified tag enters or exits this trigger.")]
    [Header("Useful for area-based triggers, traps, or interactive zones.")]
    [Header("------------------------------------------------------------------------")]

    [Header("Settings")]
    public float invokeDelay = 0f;
    public string tag = "Player";

    [Header("Events")]
    public UnityEvent onEnterTriggerArea;
    public UnityEvent onExitTriggerArea;

    public void SetInvokeDelay(float invokeDelay_)
    {
        invokeDelay = invokeDelay_;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(tag))
            StartCoroutine(OnEnterDelay());
    }

    private IEnumerator OnEnterDelay()
    {
        yield return new WaitForSeconds(invokeDelay);
        onEnterTriggerArea.Invoke();
    }

    private void OnTriggerExit2D(Collider2D other)
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