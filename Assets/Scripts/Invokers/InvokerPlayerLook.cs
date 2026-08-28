using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Custom/Invoker Player Look")]
public class InvokerPlayerLook : MonoBehaviour
{
    [Header("⚠️ Help ⚠️")]
    [Header("Detects when the player looks at this object using RayCaster.")]
    [Header("Invokes onPlayerLookEnter after a delay when player looks.")]
    [Header("Invokes onPlayerLookExit after a delay when player looks away.")]
    [Header("------------------------------------------------------------------------")]

    [Header("Settings")]
    [Tooltip("Delay in seconds before invoking look events.")]
    public float invokeDelay = 0f;

    [Header("Events")]
    [Tooltip("Event invoked when player starts looking at this object.")]
    public UnityEvent onPlayerLookEnter;

    [Tooltip("Event invoked when player stops looking at this object.")]
    public UnityEvent onPlayerLookExit;

    private RayCaster ray;
    private bool enter = false;
    private Coroutine currentCoroutine; // track active coroutine to cancel

    private void Start()
    {
        ray = FindFirstObjectByType<RayCaster>();
    }

    /// <summary>
    /// Sets the delay before invoking look events.
    /// </summary>
    public void SetInvokeDelay(float invokeDelay_)
    {
        invokeDelay = invokeDelay_;
    }

    private void OnDisable()
    {
        // If object is disabled while looking, force exit
        if (enter)
        {
            if (currentCoroutine != null)
                StopCoroutine(currentCoroutine);
            onPlayerLookExit?.Invoke();
            enter = false;
            currentCoroutine = null;
        }
    }

    private void Update()
    {
        if (ray == null) return;

        bool currentlyLooking = (ray.HitObject == gameObject);

        if (currentlyLooking && !enter)
        {
            enter = true;
            if (currentCoroutine != null)
                StopCoroutine(currentCoroutine);
            currentCoroutine = StartCoroutine(OnEnterDelay());
        }

        if (enter && !currentlyLooking)
        {
            enter = false;
            if (currentCoroutine != null)
                StopCoroutine(currentCoroutine);
            currentCoroutine = StartCoroutine(OnExitDelay());
        }
    }

    private IEnumerator OnEnterDelay()
    {
        yield return new WaitForSeconds(invokeDelay);
        // Only invoke if we're still looking after the delay
        if (enter && ray != null && ray.HitObject == gameObject)
            onPlayerLookEnter.Invoke();
        currentCoroutine = null;
    }

    private IEnumerator OnExitDelay()
    {
        yield return new WaitForSeconds(invokeDelay);
        // Only invoke if we're still not looking after the delay
        if (!enter && (ray == null || ray.HitObject != gameObject))
            onPlayerLookExit.Invoke();
        currentCoroutine = null;
    }
}