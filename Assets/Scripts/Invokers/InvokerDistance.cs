using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Custom/Invoker Distance")]
public class InvokerDistance : MonoBehaviour
{
    [Header("⚠️ Help ⚠️")]
    [Header("Invoke events depending on the distance between two objects.")]
    [Header("If the distance is less than or equal to the threshold, OnDistanceClose is invoked.")]
    [Header("If the distance is greater, OnDistanceFar is invoked.")]
    [Header("------------------------------------------------------------------------")]
    [Header("🔧 Runtime usage:")]
    [Header("• Call SetObjectA(Transform) or SetObjectB(Transform) to change targets.")]
    [Header("• The 'once' events fire only when the close/far state changes.")]
    [Header("• The continuous events fire every frame while the condition holds.")]

    [Header("Distance Settings")]
    [Tooltip("The distance threshold to compare the objects' positions.")]
    public float distanceThreshold = 3f;

    [Header("Objects")]
    [Tooltip("The first object.")]
    public Transform objectA;

    [Tooltip("The second object.")]
    public Transform objectB;

    [Header("Events")]
    [Tooltip("Invoked when distance is less than or equal to threshold.")]
    public UnityEvent onDistanceClose;

    [Tooltip("Invoked when distance is greater than threshold.")]
    public UnityEvent onDistanceFar;

    [Tooltip("Invoked once when the distance becomes close.")]
    public UnityEvent onDistanceCloseOnce;

    [Tooltip("Invoked once when the distance becomes far.")]
    public UnityEvent onDistanceFarOnce;

    private bool wasClose = false;
    private bool initialStateSet = false;

    private void Update()
    {
        if (objectA == null || objectB == null) return;

        float dist = Vector3.Distance(objectA.position, objectB.position);
        bool isClose = dist <= distanceThreshold;

        if (!initialStateSet)
        {
            wasClose = isClose;
            initialStateSet = true;
            return;
        }

        if (isClose != wasClose)
        {
            if (isClose)
                onDistanceCloseOnce.Invoke();
            else
                onDistanceFarOnce.Invoke();
            wasClose = isClose;
        }

        if (isClose)
            onDistanceClose.Invoke();
        else
            onDistanceFar.Invoke();
    }

    private void OnDrawGizmosSelected()
    {
        if (objectA == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(objectA.position, distanceThreshold);

        if (objectB != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(objectA.position, objectB.position);
        }
    }

    // ----- Public methods to change targets (as requested) -----
    public void SetObjectA(Transform newObjectA)
    {
        objectA = newObjectA;
        initialStateSet = false; // force recalculation of state
    }

    public void SetObjectB(Transform newObjectB)
    {
        objectB = newObjectB;
        initialStateSet = false;
    }
}