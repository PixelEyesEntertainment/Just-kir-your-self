using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Custom/Invoker Button")]
public class InvokerButton : MonoBehaviour
{
    [Header("⚠️ Help ⚠️")]
    [Header("Invoke the OnButtonPressed event when the specified key is pressed.")]
    [Header("Requires a RayCaster in the scene to detect if the player is looking at this object.")]
    [Header("Supports a delay before invoking the event and prevents multiple triggers with cooldown.")]
    [Header("------------------------------------------------------------------------")]

    [Header("Input Settings")]
    [Tooltip("The key that will trigger the event.")]
    public KeyCode keyCode;

    [Tooltip("Delay in seconds before invoking the event.")]
    public float invokeDelay = 0f;

    [Header("Event")]
    [Tooltip("Event invoked after pressing the key with the specified delay.")]
    public UnityEvent onButtonPressed;
    
    private RayCaster ray;
    private bool isCoolDown;

    private void Start()
    {
        ray = FindFirstObjectByType<RayCaster>();
    }

    /// <summary>aa
    /// Sets the delay time for invoking the button event.
    /// </summary>
    public void SetInvokeDelay(float invokeDelay_)
    {
        invokeDelay = invokeDelay_;
    }

    private void Update()
    {
        if (isCoolDown) return;

        if (Input.GetKeyDown(keyCode) && ray.HitObject == gameObject)
        {
            StartCoroutine(InvokeAfterDelay());
            isCoolDown = true;
        }
    }

    private IEnumerator InvokeAfterDelay()
    {
        yield return new WaitForSeconds(invokeDelay);
        isCoolDown = false;
        onButtonPressed.Invoke();
    }
}