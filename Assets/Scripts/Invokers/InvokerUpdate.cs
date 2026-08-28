using UnityEngine;
using UnityEngine.Events;

public class InvokerUpdate : MonoBehaviour
{
    public UnityEvent onUpdate;

    private void Update()
    {
        onUpdate.Invoke();
    }
}
