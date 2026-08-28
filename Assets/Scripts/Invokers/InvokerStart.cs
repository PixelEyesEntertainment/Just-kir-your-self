using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InvokerStart : MonoBehaviour
{
    public float invokeDelay;
    public UnityEvent OnStart;
    private void Start()
    {
        Invoke("DoIt", invokeDelay);
    }

    private void DoIt()
    {
        OnStart.Invoke();
    }
}
