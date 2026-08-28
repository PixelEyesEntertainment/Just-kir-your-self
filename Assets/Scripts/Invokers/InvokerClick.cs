using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Custom/Invoker Click")]
public class InvokerClick : MonoBehaviour
{
    [Header("⚠️ Help ⚠️")]
    [Header("Invoke the OnClick event when the object is clicked.")]
    [Header("Supports a delay before invoking the event and prevents multiple triggers with cooldown.")]
    [Header("Requires a 2D collider on this object and a camera in the scene.")]
    [Header("------------------------------------------------------------------------")]

    [Header("Click Settings")]
    [Tooltip("Delay in seconds before invoking the click event.")]
    public float invokeDelay = 0f;

    [Header("Event")]
    [Tooltip("Event invoked after clicking the object with the specified delay.")]
    public UnityEvent onClick;

    private Camera mainCam;
    private bool isCoolDown;

    private void Start()
    {
        mainCam = Camera.main;
    }

    public void SetInvokeDelay(float invokeDelay_)
    {
        invokeDelay = invokeDelay_;
    }

    private void Update()
    {
        if (isCoolDown) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorld = mainCam.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mouseWorld.x, mouseWorld.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                StartCoroutine(InvokeAfterDelay());
                isCoolDown = true;
            }
        }
    }

    private IEnumerator InvokeAfterDelay()
    {
        yield return new WaitForSeconds(invokeDelay);
        isCoolDown = false;
        onClick.Invoke();
    }
}
