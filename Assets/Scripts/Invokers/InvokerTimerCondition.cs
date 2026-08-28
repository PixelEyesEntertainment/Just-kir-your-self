using UnityEngine;
using UnityEngine.Events;

public enum Operator
{
    equal,
    greaterOrEqualThan,
    lessOrEqualThan
}

public class InvokerTimerCondition : MonoBehaviour
{
    [Header("⚠️ Help ⚠️")]
    [Header("Checks the remaining time of an InvokerTimer against a condition.")]
    [Header("Call CheckCondition() manually or hook it to onTick / other events.")]
    [Header("Equal uses exact float comparison – consider small tolerance if needed.")]
    [Header("------------------------------------------------------------------------")]

    [Header("Settings")]
    public InvokerTimer invokerTimer;
    public float conditionNumber;
    public Operator conditionOperator;

    [Header("Events")]
    public UnityEvent onConditionCheckTrue;
    public UnityEvent onConditionCheckFalse;

    public void CheckCondition()
    {
        switch (conditionOperator)
        {
            case Operator.equal:
                if (invokerTimer.SecondsRemaining == conditionNumber)
                {
                    onConditionCheckTrue.Invoke();
                }
                else
                {
                    onConditionCheckFalse.Invoke();
                }
                break;
            case Operator.greaterOrEqualThan:
                if (invokerTimer.SecondsRemaining >= conditionNumber)
                {
                    onConditionCheckTrue.Invoke();
                }
                else
                {
                    onConditionCheckFalse.Invoke();
                }
                break;
            case Operator.lessOrEqualThan:
                if (invokerTimer.SecondsRemaining <= conditionNumber)
                {
                    onConditionCheckTrue.Invoke();
                }
                else
                {
                    onConditionCheckFalse.Invoke();
                }
                break;
        }
    }
}