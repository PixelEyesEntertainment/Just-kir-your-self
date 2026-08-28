using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Custom/Invoker Random")]
public class InvokerRandom : MonoBehaviour
{
    [Header("⚠️ Help ⚠️")]
    [Header("Check a random number within a range.")]
    [Header("Invoke onCheck immediately, then after a delay fire onChanceTrue or onChanceFalse.")]
    [Header("Useful for random events, loot drops, or chance-based mechanics.")]
    [Header("Min is Inclusive, Max is Exclusive")]
    [Header("------------------------------------------------------------------------")]

    [Header("Settings")]
    [Tooltip("Delay in seconds before invoking chance events.")]
    public float invokeDelay = 0f;

    [Tooltip("Minimum random number (inclusive).")]
    public int minChance = 0;

    [Tooltip("Maximum random number (exclusive).")]
    public int maxChance = 10;

    [Tooltip("The number that triggers onChanceTrue if randomly selected.")]
    public int chosenChance = 5;

    [Header("Events")]
    [Tooltip("Invoked immediately when CheckRandom is called.")]
    public UnityEvent onCheck;

    [Tooltip("Event invoked if random number matches chosen chance (after delay).")]
    public UnityEvent onChanceTrue;

    [Tooltip("Event invoked if random number does not match chosen chance (after delay).")]
    public UnityEvent onChanceFalse;

    [Header("Read-Only")]
    public int chance;

    /// <summary>
    /// Check a random number, fire onCheck instantly, then after delay invoke the matching event.
    /// </summary>
    public void CheckRandom()
    {
        // Fire immediately
        onCheck.Invoke();

        // Generate random number
        chance = Random.Range(minChance, maxChance);

        // Start coroutine for delayed true/false
        if (chance == chosenChance)
            StartCoroutine(DelayToOnChanceTrue());
        else
            StartCoroutine(DelayToOnChanceFalse());
    }

    private IEnumerator DelayToOnChanceTrue()
    {
        yield return new WaitForSeconds(invokeDelay);
        onChanceTrue.Invoke();
    }

    private IEnumerator DelayToOnChanceFalse()
    {
        yield return new WaitForSeconds(invokeDelay);
        onChanceFalse.Invoke();
    }
}