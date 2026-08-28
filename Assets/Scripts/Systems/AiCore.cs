using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[AddComponentMenu("Custom/AI Core")]
public class AiCore : MonoBehaviour
{
    [Header("⚠️ Help ⚠️")]
    [Header("This script controls AI movement with NavMeshAgent.")]
    [Header("------------------------------------------------------")]
    [Header("✔ Requirements:")]
    [Header("   - NavMeshAgent on the same GameObject")]
    [Header("   - Animator with a bool parameter named 'isMoving'")]
    [Header("------------------------------------------------------")]
    [Header("✔ How to use:")]
    [Header("   - Call FollowTarget(Transform) to chase a target (one-shot set destination).")]
    [Header("   - Call StopFollowInstant() to stop immediately.")]
    [Header("   - Call StopFollowWithDelay(float) to stop after a delay (no condition checks).")]
    [Header("   - Call SeenTarget()/UnSeenTarget() to toggle visibility state and invoke events.")]
    [Header("------------------------------------------------------")]
    [Header("✔ Events (must be assigned in Inspector):")]
    [Header("   - OnSeenTarget: fired when SeenTarget() is called.")]
    [Header("   - OnUnseenTarget: fired when UnSeenTarget() is called.")]
    [Header("------------------------------------------------------")]
    [Header("TIP: Use the events to decide what the AI does when it sees/loses the target.")]

    private NavMeshAgent agent;
    private Animator anim;

    [Header("Runtime State")]
    [Tooltip("True if the AI currently has a target in sight.")]
    public bool seenTarget;

    [Tooltip("True if the AI is currently moving.")]
    public bool isMoving;

    [Header("Events (must not be empty)")]
    [Tooltip("Invoked when the AI sees a target.")]
    public UnityEvent OnSeenTarget;

    [Tooltip("Invoked when the AI loses the target.")]
    public UnityEvent OnUnseenTarget;

    private Coroutine stopFollowCoroutine;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
    }

    /// <summary>
    /// Stops following immediately.
    /// </summary>
    public void StopFollowInstant()
    {
        if (stopFollowCoroutine != null)
        {
            StopCoroutine(stopFollowCoroutine);
            stopFollowCoroutine = null;
        }

        agent.isStopped = true;
        isMoving = false;
        if (anim != null)
        {
            anim.SetBool("isMoving", false);
        }
    }

    /// <summary>
    /// Stops following after a delay. Cancels previous stop coroutines if needed.
    /// </summary>
    public void StopFollowWithDelay(float delay)
    {
        if (stopFollowCoroutine != null)
        {
            StopCoroutine(stopFollowCoroutine);
        }
        stopFollowCoroutine = StartCoroutine(StopFollowDelay(delay));
    }

    private IEnumerator StopFollowDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Stop AI regardless of events
        agent.isStopped = true;
        isMoving = false;
        if (anim != null)
        {
            anim.SetBool("isMoving", false);
        }

        stopFollowCoroutine = null;
    }

    /// <summary>
    /// Marks that the AI has seen a target. Always invokes OnSeenTarget.
    /// </summary>
    public void SeenTarget()
    {
        seenTarget = true;

        // Cancel any pending stop
        if (stopFollowCoroutine != null)
        {
            StopCoroutine(stopFollowCoroutine);
            stopFollowCoroutine = null;
        }

        OnSeenTarget.Invoke();
    }

    /// <summary>
    /// Marks that the AI no longer sees a target. Always invokes OnUnseenTarget.
    /// </summary>
    public void UnSeenTarget()
    {
        seenTarget = false;
        OnUnseenTarget.Invoke();
    }

    /// <summary>
    /// Starts following a target Transform (one-shot SetDestination).
    /// Call repeatedly if you want continuous chasing.
    /// </summary>
    public void FollowTarget(Transform target)
    {
        agent.isStopped = false;
        isMoving = true;
        if (anim != null)
        {
            anim.SetBool("isMoving", true);
        }
        agent.SetDestination(target.position);
    }
}
