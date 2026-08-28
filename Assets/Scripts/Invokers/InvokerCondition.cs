using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Custom/Invoker Condition")]
public class InvokerCondition : MonoBehaviour
{
    [Header("⚠️ Help ⚠️")]
    [Header("Checks if currentNumber equals conditionNumber.")]
    [Header("Invoke onConditionTrue if numbers match, otherwise onConditionFalse.")]
    [Header("Use CurrentNumberIncrement, CurrentNumberDecrement, SetCurrentNumber, or SetConditionNumber to control values at runtime.")]
    [Header("------------------------------------------------------------------------")]

    [Header("Condition Settings")]
    [Tooltip("The number to compare against.")]
    public int conditionNumber;

    [Tooltip("The current number to be checked.")]
    public int currentNumber;

    [Header("Events")]
    [Tooltip("Invoked if currentNumber == conditionNumber.")]
    public UnityEvent onConditionTrue;

    [Tooltip("Invoked if currentNumber != conditionNumber.")]
    public UnityEvent onConditionFalse;

    private int defaultConditionNumber;
    private int defaultCurrentNumber;

    private void Start()
    {
        defaultConditionNumber = conditionNumber;
        defaultCurrentNumber = currentNumber;
    }

    public void CurrentNumberIncrement(int increment)
    {
        currentNumber += increment;
    }

    public void CurrentNumberDecrement(int decrement)
    {
        currentNumber -= decrement;
    }

    public void SetConditionNumber(int number)
    {
        conditionNumber = number;
    }

    public void SetCurrentNumber(int number)
    {
        currentNumber = number;
    }

    public void ResetConditionNumber()
    {
        conditionNumber = defaultConditionNumber;
    }

    public void ResetCurrentNumber()
    {
        currentNumber = defaultCurrentNumber;
    }

    public void ConditionCheck()
    {
        if (currentNumber == conditionNumber)
        {
            onConditionTrue.Invoke();
        }
        else
        {
            onConditionFalse.Invoke();
        }
    }
}