using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

[AddComponentMenu("Custom/Invoker Password")]
public class Invokerpassword : MonoBehaviour
{
    [Header("⚠️ Help ⚠️")]
    [Header("Manages a numeric password input using TMP_InputField.")]
    [Header("Checks the password, invokes onpasswordTrue or onpasswordFalse events.")]
    [Header("Supports UI pop-up animations and delayed button input.")]
    [Header("Use GenerateRandompassword() to randomize password.")]
    [Header("Use EnterNumber(int number) to add digits with optional delay.")]
    [Header("------------------------------------------------------------------------")]

    [Header("Password Settings")]
    [Tooltip("Delay before invoking success/fail events.")]
    public float invokeDelaypassword = 0f;

    [Tooltip("The password to check against.")]
    public int password = 123456;

    [Header("UI Settings")]
    [Tooltip("TMP_InputField for entering the password.")]
    public TMP_InputField input;

    [Tooltip("Animator controlling password UI.")]
    public Animator passwordUi;

    [Tooltip("Delay between number button inputs.")]
    public float buttonsDelay = 0.1f;

    [Header("Pop-Up Events")]
    [Tooltip("Delay before invoking pop-up events.")]
    public int invokeDelayPops = 0;

    public UnityEvent onPopUp;
    public UnityEvent onPopDown;

    [Header("Password Events")]
    public UnityEvent onpasswordTrue;
    public UnityEvent onpasswordFalse;

    public void GenerateRandompassword()
    {
        password = Random.Range(111111, 999999);
    }

    public void Checkpassword()
    {
        if (int.Parse(input.text) == password)
            Invoke(nameof(OnPasswordTrueInvoke), invokeDelaypassword);
        else
            Invoke(nameof(OnPasswordFalseInvoke), invokeDelaypassword);
    }

    private void OnPasswordTrueInvoke()
    {
        onpasswordTrue.Invoke();
    }

    private void OnPasswordFalseInvoke()
    {
        onpasswordFalse.Invoke();
    }

    public void ClearInput()
    {
        input.text = "";
    }

    public void PopUppasswordUI()
    {
        passwordUi.SetBool("isUp", true);
        Invoke(nameof(OnPopUpInvoke), invokeDelayPops);
    }

    private void OnPopUpInvoke()
    {
        onPopUp.Invoke();
    }

    public void PopDownpasswordUI()
    {
        passwordUi.SetBool("isUp", false);
        Invoke(nameof(OnPopDownInvoke), invokeDelayPops);
    }

    private void OnPopDownInvoke()
    {
        onPopDown.Invoke();
    }

    public void EnterNumber(int number)
    {
        StartCoroutine(EnterNumberInvoke(number));
    }

    private IEnumerator EnterNumberInvoke(int number)
    {
        yield return new WaitForSeconds(buttonsDelay);
        if (input.text.Length < 6)
            input.text += number;
    }
}