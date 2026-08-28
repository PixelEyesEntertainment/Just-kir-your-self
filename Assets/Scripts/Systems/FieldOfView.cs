using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FieldOfView : MonoBehaviour
{
    [Header("⚠️ Help ⚠️")]
    [Header("Cone-based detection with line-of-sight checking.")]
    [Header("")]
    [Header("onDetected      → first time a target enters view (once)")]
    [Header("onDetecting     → every frame while ANY target is visible")]
    [Header("onLost          → first time ALL targets leave view (after delay)")]
    [Header("onNotDetecting  → every frame while NO targets are visible")]
    [Header("")]
    [Header("visibleTargets → list of currently visible targets (read at runtime)")]
    [Header("targetsInCone  → list of targets inside the cone (debug)")]
    [Header("------------------------------------------------------------------------")]

    [Header("Detection Settings")]
    public float range = 10f;
    [Range(0, 360)] public float angle = 45f;

    [Tooltip("Leave empty to ignore tag filtering.")]
    public string targetTag = "";

    public LayerMask targetMask = ~0;          // What to detect (default: everything)
    public LayerMask obstructionMask = ~0;     // What blocks view (default: nothing)

    [Header("Event Timing")]
    public float unseenDelay = 1f;

    [Header("Origin Offset")]
    public Vector3 originOffset = new Vector3(0, 1.5f, 0);

    [Header("Performance")]
    public int maxTargets = 30;

    [Header("Events")]
    public UnityEvent onDetected;      // First time a target is seen
    public UnityEvent onDetecting;     // Every frame while any target visible
    public UnityEvent onLost;          // First time all targets lost (after delay)
    public UnityEvent onNotDetecting;  // Every frame while no targets visible

    [Header("Debug")]
    public bool drawGizmos = true;
    public bool drawGizmosOnTargetsInCone = true;

    // Public lists (visible in Inspector)
    public List<Transform> visibleTargets = new List<Transform>();
    public List<Transform> targetsInCone = new List<Transform>();

    private HashSet<Transform> visibleSet = new HashSet<Transform>();
    private Collider[] overlapBuffer;
    private float unseenTimer = 0f;
    private bool wasAnyTarget = false;

    private void Awake()
    {
        overlapBuffer = new Collider[Mathf.Max(20, maxTargets)];
    }

    private void Update()
    {
        CheckFieldOfView();
    }

    private void CheckFieldOfView()
    {
        visibleSet.Clear();
        targetsInCone.Clear();

        Vector3 origin = transform.position + originOffset;
        int finalMask = targetMask.value == 0 ? ~0 : targetMask.value;

        int numTargets = Physics.OverlapSphereNonAlloc(origin, range, overlapBuffer, finalMask);

        for (int i = 0; i < numTargets; i++)
        {
            Collider col = overlapBuffer[i];
            if (col == null) continue;

            // Tag filtering
            if (!string.IsNullOrEmpty(targetTag) && !col.CompareTag(targetTag))
                continue;

            Transform target = col.transform;
            Vector3 dirToTarget = target.position - origin;
            float distance = dirToTarget.magnitude;
            if (distance > range) continue;

            Vector3 dirNorm = dirToTarget / distance;
            float angleToTarget = Vector3.Angle(transform.forward, dirNorm);
            if (angleToTarget > angle * 0.5f) continue;

            targetsInCone.Add(target);

            // Line of sight check
            if (Physics.Raycast(origin, dirNorm, out RaycastHit hit, distance, obstructionMask))
            {
                if (hit.transform != target) continue;
            }

            visibleSet.Add(target);
        }

        visibleTargets.Clear();
        visibleTargets.AddRange(visibleSet);

        bool anySeen = visibleTargets.Count > 0;

        if (anySeen)
        {
            onDetecting.Invoke();
            if (!wasAnyTarget)
            {
                onDetected.Invoke();
                wasAnyTarget = true;
            }
            unseenTimer = 0f;
        }
        else
        {
            onNotDetecting.Invoke();
            unseenTimer += Time.deltaTime;
            if (unseenTimer >= unseenDelay && wasAnyTarget)
            {
                onLost.Invoke();
                wasAnyTarget = false;
            }
        }
    }

    /// <summary>
    /// Returns true if a specific target is currently visible.
    /// </summary>
    public bool IsTargetVisible(Transform target)
    {
        return visibleSet.Contains(target);
    }

    /// <summary>
    /// Returns all currently visible targets.
    /// </summary>
    public List<Transform> GetVisibleTargets()
    {
        return visibleTargets;
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        Vector3 origin = transform.position + originOffset;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(origin, range);

        Vector3 left = Quaternion.Euler(0, -angle * 0.5f, 0) * transform.forward * range;
        Vector3 right = Quaternion.Euler(0, angle * 0.5f, 0) * transform.forward * range;
        Gizmos.DrawLine(origin, origin + left);
        Gizmos.DrawLine(origin, origin + right);

        Gizmos.color = Color.green;
        foreach (Transform t in visibleTargets)
            if (t != null) Gizmos.DrawLine(origin, t.position);

        if (drawGizmosOnTargetsInCone)
        {
            Gizmos.color = Color.yellow;
            foreach (Transform t in targetsInCone)
                if (t != null) Gizmos.DrawSphere(t.position, 0.2f);
        }
    }
}