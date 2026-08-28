using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Custom/Invoker Collision 2D")]
public class InvokerCollision2D : MonoBehaviour
{
    [Header("⚠️ Help ⚠️")]
    [Header("Invoke events when a GameObject with the specified tag collides with this object.")]
    [Header("OnEnterCollisionArea triggers after a delay when collision starts.")]
    [Header("OnExitCollisionArea triggers after a delay when collision ends.")]
    [Header("------------------------------------------------------------------------")]

    [Header("Settings")]
    public float invokeDelay = 0f;
    public string targetTag = "Player";

    [Header("Events")]
    public UnityEvent onEnterCollisionArea;
    public UnityEvent onExitCollisionArea;

    public void SetInvokeDelay(float invokeDelay_)
    {
        invokeDelay = invokeDelay_;
    }

    private void OnCollisionEnter2D(Collision2D other)
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

    private void OnCollisionExit2D(Collision2D other)
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