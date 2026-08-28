using UnityEngine;

[AddComponentMenu("Custom/Invoker Animator Params")]
public class InvokerAnimatorParams : MonoBehaviour
{
    [Header("⚠️ Help ⚠️")]
    [Header("Controls an Animator via UnityEvents.")]
    [Header("Triggers: SetTrigger('name') or ResetTrigger('name')")]
    [Header("Bools: SetBool('name=true/false') or SetBoolTrue/False('name')")]
    [Header("Floats: SetFloat('name=value')")]
    [Header("Integers: SetInteger('name=value')")]
    [Header("------------------------------------------------------------------------")]

    [Header("Animator Reference")]
    public Animator animator;

    public void SetTrigger(string name)
    {
        if (animator == null) { LogWarning(); return; }
        animator.SetTrigger(name);
    }

    public void ResetTrigger(string name)
    {
        if (animator == null) { LogWarning(); return; }
        animator.ResetTrigger(name);
    }

    public void SetBool(string nameAndValue)
    {
        if (animator == null) { LogWarning(); return; }
        string[] parts = nameAndValue.Split('=');
        if (parts.Length != 2) { LogError("Invalid bool format. Use 'name=true/false'"); return; }
        string name = parts[0].Trim();
        if (!bool.TryParse(parts[1].Trim(), out bool value)) { LogError("Could not parse bool value"); return; }
        animator.SetBool(name, value);
    }

    public void SetBoolTrue(string name)
    {
        if (animator == null) { LogWarning(); return; }
        animator.SetBool(name, true);
    }

    public void SetBoolFalse(string name)
    {
        if (animator == null) { LogWarning(); return; }
        animator.SetBool(name, false);
    }

    public void SetFloat(string nameAndValue)
    {
        if (animator == null) { LogWarning(); return; }
        string[] parts = nameAndValue.Split('=');
        if (parts.Length != 2) { LogError("Invalid float format. Use 'name=number'"); return; }
        string name = parts[0].Trim();
        if (!float.TryParse(parts[1].Trim(), out float value)) { LogError("Could not parse float value"); return; }
        animator.SetFloat(name, value);
    }

    public void SetInteger(string nameAndValue)
    {
        if (animator == null) { LogWarning(); return; }
        string[] parts = nameAndValue.Split('=');
        if (parts.Length != 2) { LogError("Invalid integer format. Use 'name=number'"); return; }
        string name = parts[0].Trim();
        if (!int.TryParse(parts[1].Trim(), out int value)) { LogError("Could not parse int value"); return; }
        animator.SetInteger(name, value);
    }

    private void LogWarning()
    {
        Debug.LogWarning("Animator not assigned on " + gameObject.name);
    }

    private void LogError(string message)
    {
        Debug.LogError(message + " on " + gameObject.name);
    }
}