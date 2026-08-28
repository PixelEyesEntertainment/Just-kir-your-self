using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Custom/Invoker Button Hold")]
public class InvokerButtonHold : MonoBehaviour
{
    [Header("⚠️ Help ⚠️")]
    [Header("Invoke events when a specified key is held down.")]
    [Header("OnButtonDown triggers immediately when pressing the key.")]
    [Header("OnHoldComplete triggers after the key is held for the specified duration.")]
    [Header("OnHoldCanceled triggers if the key is released or player looks away before hold completes.")]
    [Header("Requires a RayCaster in the scene to detect if the player is looking at this object.")]
    [Header("------------------------------------------------------------------------")]

    [Header("Input Settings")]
    [Tooltip("The key to hold for triggering events.")]
    public KeyCode keyCode;

    [Tooltip("Duration in seconds the key must be held to trigger OnHoldComplete.")]
    public float holdTime = 2f;

    [Header("Events")]
    [Tooltip("Event invoked immediately when key is first pressed.")]
    public UnityEvent onButtonDown;

    [Tooltip("Event invoked after key is held for holdTime seconds.")]
    public UnityEvent onHoldComplete;

    [Tooltip("Event invoked if holding is canceled before completion.")]
    public UnityEvent onHoldCanceled;

    private float holdTimer = 0f;
    private bool isHolding = false;
    private RayCaster ray;

    private void Start()
    {
        ray = FindFirstObjectByType<RayCaster>();
    }

    /// <summary>
    /// Sets the required hold duration for the key.
    /// </summary>
    public void SetholdTime(float holdTime_)
    {
        holdTime = holdTime_;
    }

    private void Update()
    {
        if (!isHolding && Input.GetKeyDown(keyCode) && ray.HitObject == gameObject)
        {
            onButtonDown.Invoke();
            isHolding = true;
            holdTimer = 0f;
        }

        if (isHolding)
        {
            if (Input.GetKey(keyCode))
            {
                if (ray.HitObject != gameObject)
                {
                    CancelHold();
                    return;
                }

                holdTimer += Time.deltaTime;

                if (holdTimer >= holdTime)
                {
                    isHolding = false;
                    onHoldComplete.Invoke();
                }
            }
            else
            {
                CancelHold();
            }
        }
    }

    private void CancelHold()
    {
        onHoldCanceled.Invoke();
        isHolding = false;
        holdTimer = 0f;
    }
}