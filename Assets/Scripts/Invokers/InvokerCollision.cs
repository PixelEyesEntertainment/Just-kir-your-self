using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Custom/Invoker Collision")]
public class InvokerCollision : MonoBehaviour
{
    [Header("⚠️ Help ⚠️")]
    [Header("Invoke events when a GameObject with the specified tag collides with this object.")]
    [Header("OnEnterCollisionArea triggers after a delay when collision starts.")]
    [Header("OnExitCollisionArea triggers after a delay when collision ends.")]
    [Header("------------------------------------------------------------------------")]

    [Header("Settings")]
    [Tooltip("Delay in seconds before invoking the collision events.")]
    public float invokeDelay = 0f;

    [Tooltip("Tag to check for collision events.")]
    public string targetTag = "Player";

    [Header("Events")]
    [Tooltip("Event invoked after delay when object enters collision.")]
    public UnityEvent onEnterCollisionArea;

    [Tooltip("Event invoked after delay when object exits collision.")]
    public UnityEvent onExitCollisionArea;

    /// <summary>
    /// Sets the delay time for invoking collision events.
    /// </summary>
    public void SetInvokeDelay(float invokeDelay_)
    {
        invokeDelay = invokeDelay_;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag(targetTag))
        {
            StartCoroutine(OnEnterDelay());
        }
    }

    private IEnumerator OnEnterDelay()
    {
        yield return new WaitForSeconds(invokeDelay);
        onEnterCollisionArea.Invoke();
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag(targetTag))
        {
            StartCoroutine(OnExitDelay());
        }
    }

    private IEnumerator OnExitDelay()
    {
        yield return new WaitForSeconds(invokeDelay);
        onExitCollisionArea.Invoke();
    }
}