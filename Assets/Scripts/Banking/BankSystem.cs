using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class BankSystem : MonoBehaviour
{
    [Header("⚠️ HELP ⚠️")]
    [Header("Currency manager – add, subtract, set, and display balance.")]
    [Header("")]
    [Header("AddCurrency(int)   → adds amount to balance")]
    [Header("SubtractCurrency(int) → returns true if successful, false if insufficient funds")]
    [Header("SetCurrency(int)   → sets balance to exact value")]
    [Header("GetCurrency()      → returns current balance")]
    [Header("ResetCurrency()    → sets balance to 0")]
    [Header("")]
    [Header("If balanceText is assigned, it updates automatically on every operation.")]
    [Header("Use textPrefix and textSuffix to format the displayed text.")]
    [Header("------------------------------------------------------------------------")]

    [Header("Settings")]
    public int balance;
    public TMP_Text balanceText;
    public string textPrefix = "";
    public string textSuffix = "";

    [Header("Events")]
    public UnityEvent onBalanceChanged;
    public UnityEvent onDeposit;
    public UnityEvent onWithdraw;
    public UnityEvent onInsufficientFunds;

    private void Start()
    {
        UpdateText();
    }

    public void AddCurrency(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("Use SubtractCurrency for negative amounts.");
            return;
        }

        balance += amount;
        onDeposit.Invoke();
        onBalanceChanged.Invoke();
        UpdateText();
    }

    /// <summary>
    /// Subtracts currency if enough funds exist.
    /// Returns true if successful, false if insufficient funds.
    /// </summary>

    public void SpendCurrency(int amount)
    {
        SubtractCurrency(amount);
    }

    public bool SubtractCurrency(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("Use AddCurrency for positive amounts.");
            return false;
        }

        if (balance < amount)
        {
            onInsufficientFunds.Invoke();
            return false;
        }

        balance -= amount;
        onWithdraw.Invoke();
        onBalanceChanged.Invoke();
        UpdateText();
        return true;
    }

    public void SetCurrency(int newBalance)
    {
        balance = newBalance;
        onBalanceChanged.Invoke();
        UpdateText();
    }

    public int GetCurrency()
    {
        return balance;
    }

    public void ResetCurrency()
    {
        balance = 0;
        onBalanceChanged.Invoke();
        UpdateText();
    }

    public void UpdateText()
    {
        if (balanceText != null)
            balanceText.text = textPrefix + balance.ToString() + textSuffix;
    }
}