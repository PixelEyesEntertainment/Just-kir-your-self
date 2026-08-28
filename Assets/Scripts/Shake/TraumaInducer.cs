using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class TraumaInducer : MonoBehaviour
{
    [Header("⚠️ Help ⚠️")]
    [Header("Applies stress to nearby StressReceivers (camera shake).")]
    [Header("Call PlayStress() to trigger.")]
    [Header("------------------------------------------------------------------------")]

    [Header("Settings")]
    public float delay = 1f;
    public float maximumStress = 0.6f;
    public float range = 45f;

    [Header("Events")]
    public UnityEvent onStressStart;
    public UnityEvent onStressApplied;

    private StressReceiver[] receivers; // Cache for performance

    void Awake()
    {
        // Cache all StressReceivers in the scene once
        receivers = FindObjectsOfType<StressReceiver>();
    }

    public void PlayStress()
    {
        onStressStart.Invoke();
        StartCoroutine(DoStress());
    }

    private IEnumerator DoStress()
    {
        yield return new WaitForSeconds(delay);

        if (receivers == null || receivers.Length == 0)
            yield break;

        Vector3 pos = transform.position;

        foreach (var receiver in receivers)
        {
            if (receiver == null) continue;

            float distance = Vector3.Distance(pos, receiver.transform.position);
            if (distance > range) continue;

            float distance01 = Mathf.Clamp01(distance / range);
            float stress = (1f - distance01 * distance01) * maximumStress;
            receiver.InduceStress(stress);
        }

        onStressApplied.Invoke();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}