using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

[AddComponentMenu("Custom/Invoker Random Invoke")]
public class InvokerRandomInvoke : MonoBehaviour
{
    [Header("⚠️ Help ⚠️")]
    [Header("Randomly picks 'invokeCount' winners from the event list.")]
    [Header("Winners fire their 'events', losers fire 'loserEvents' (optional).")]
    [Header("")]
    [Header("events[0] & loserEvents[0] = same slot (if loserEvents exists).")]
    [Header("LoserEvents can be empty or shorter – missing slots are skipped.")]
    [Header("")]
    [Header("Public functions: InvokeRandom(), SetInvokeCount(int).")]
    [Header("------------------------------------------------------------------------")]

    [Header("Settings")]
    [Tooltip("How many winners to randomly pick. Clamped to the list size.")]
    public int invokeCount = 1;

    [Tooltip("Delay in seconds before each event is invoked.")]
    public float invokeDelay = 0f;

    [Header("Event Lists")]
    [Tooltip("Events fired for the randomly chosen winners.")]
    public List<UnityEvent> events = new List<UnityEvent>();

    [Tooltip("(Optional) Events fired for the slots that are NOT chosen.")]
    public List<UnityEvent> loserEvents = new List<UnityEvent>();

    [Header("Read-Only")]
    [Tooltip("Number of winners actually picked in the last call.")]
    public int lastPickedCount;

    /// <summary>
    /// Randomly picks 'invokeCount' winners and fires the corresponding events.
    /// </summary>
    [ContextMenu("Invoke Random Events")]
    public void InvokeRandom()
    {
        int slotCount = events.Count;
        if (slotCount == 0)
        {
            Debug.LogWarning("No winner events assigned to InvokerRandomInvoke.", this);
            return;
        }

        int count = Mathf.Clamp(invokeCount, 0, slotCount);
        lastPickedCount = count;

        int[] indices = new int[slotCount];
        for (int i = 0; i < indices.Length; i++) indices[i] = i;

        // Fisher-Yates shuffle
        for (int i = indices.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int temp = indices[i];
            indices[i] = indices[j];
            indices[j] = temp;
        }

        for (int i = 0; i < indices.Length; i++)
        {
            int slot = indices[i];
            bool isWinner = (i < count);

            if (isWinner)
            {
                UnityEvent evt = events[slot];
                if (evt != null)
                {
                    if (invokeDelay <= 0f)
                        evt.Invoke();
                    else
                        StartCoroutine(DelayedInvoke(evt));
                }
            }
            else
            {
                // Fire loser event only if we have one for this slot
                if (slot < loserEvents.Count)
                {
                    UnityEvent evt = loserEvents[slot];
                    if (evt != null)
                    {
                        if (invokeDelay <= 0f)
                            evt.Invoke();
                        else
                            StartCoroutine(DelayedInvoke(evt));
                    }
                }
            }
        }
    }

    /// <summary>
    /// Change the number of winners to pick at runtime.
    /// </summary>
    /// <param name="newCount">New number of winners (clamped to 0).</param>
    public void SetInvokeCount(int newCount)
    {
        invokeCount = Mathf.Max(0, newCount);
    }

    private System.Collections.IEnumerator DelayedInvoke(UnityEvent evt)
    {
        yield return new WaitForSeconds(invokeDelay);
        evt.Invoke();
    }
}