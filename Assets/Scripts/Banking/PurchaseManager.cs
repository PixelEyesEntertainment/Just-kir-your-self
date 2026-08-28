using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class PurchaseManager : MonoBehaviour
{
    [Header("⚠️ Purchase Manager ⚠️")]
    [Header("Try to buy something – fires events with optional delay.")]
    [Header("")]
    [Header("onPurchaseAttempt → fires immediately when TryPurchase() is called.")]
    [Header("Delay → wait X seconds, then fire either success or fail.")]
    [Header("onPurchaseSuccess → fires after delay if enough funds.")]
    [Header("onPurchaseFail → fires after delay if insufficient funds.")]
    [Header("------------------------------------------------------------------------")]

    public BankSystem bank;
    public int price;
    public float delay = 0f;

    [Header("Events")]
    public UnityEvent onPurchaseAttempt;
    public UnityEvent onPurchaseSuccess;
    public UnityEvent onPurchaseFail;

    private Coroutine purchaseCoroutine;

    public void TryPurchase()
    {
        // Cancel any pending purchase coroutine
        if (purchaseCoroutine != null)
            StopCoroutine(purchaseCoroutine);

        // Fire attempt event immediately
        onPurchaseAttempt.Invoke();

        // Start the delayed check
        purchaseCoroutine = StartCoroutine(ProcessPurchase());
    }

    private IEnumerator ProcessPurchase()
    {
        // Wait for the delay
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        // Now check the balance
        if (bank.SubtractCurrency(price))
        {
            onPurchaseSuccess.Invoke();
        }
        else
        {
            onPurchaseFail.Invoke();
        }

        purchaseCoroutine = null;
    }

    public void SetPrice(int newPrice)
    {
        price = newPrice;
    }

    public void SetDelay(float newDelay)
    {
        delay = newDelay;
    }
}