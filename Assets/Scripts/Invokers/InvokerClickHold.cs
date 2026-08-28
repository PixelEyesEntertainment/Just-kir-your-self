using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Custom/Invoker Click Hold")]
public class InvokerClickHold : MonoBehaviour
{
    [Header("⚠️ Help ⚠️")]
    [Header("Invoke events when the object is clicked and held.")]
    [Header("OnClickDown triggers immediately when clicking the object.")]
    [Header("OnHoldComplete triggers after holding the click for the specified duration.")]
    [Header("OnHoldCanceled triggers if released early or pointer moves away.")]
    [Header("Requires a 2D collider and a camera in the scene.")]
    [Header("------------------------------------------------------------------------")]

    [Header("Hold Settings")]
    [Tooltip("Duration in seconds to hold the click for OnHoldComplete.")]
    public float holdTime = 2f;

    [Header("Events")]
    [Tooltip("Invoked immediately when the object is clicked.")]
    public UnityEvent onClickDown;

    [Tooltip("Invoked after holding for holdTime seconds.")]
    public UnityEvent onHoldComplete;

    [Tooltip("Invoked if holding is canceled before completion.")]
    public UnityEvent onHoldCanceled;

    private Camera mainCam;
    private bool isHolding = false;
    private float holdTimer = 0f;

    private void Start()
    {
        mainCam = Camera.main;
    }

    public void SetHoldTime(float holdTime_)
    {
        holdTime = holdTime_;
    }

    private void Update()
    {
        Vector3 mouseWorld = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(mouseWorld.x, mouseWorld.y);

        RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

        if (!isHolding && Input.GetMouseButtonDown(0) && hit.collider != null && hit.collider.gameObject == gameObject)
        {
            onClickDown.Invoke();
            isHolding = true;
            holdTimer = 0f;
        }

        if (isHolding)
        {
            if (Input.GetMouseButton(0))
            {
                if (hit.collider == null || hit.collider.gameObject != gameObject)
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
