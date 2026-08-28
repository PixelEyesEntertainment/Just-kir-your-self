using System.Collections;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class InvokerInvoker : MonoBehaviour
{
    public UnityEvent[] unityEvents;
    public float delay;          
    public float delayBetween;    

    public void InvokeTheInvoker()
    {
        Invoke(nameof(StartDelayedSequence), delay);
    }

    private void StartDelayedSequence()
    {
        StartCoroutine(InvokeEventsWithDelay());
    }

    private IEnumerator InvokeEventsWithDelay()
    {
        foreach (UnityEvent ue in unityEvents)
        {
            ue.Invoke();

            if (delayBetween > 0f)
                yield return new WaitForSeconds(delayBetween);
        }
    }
}