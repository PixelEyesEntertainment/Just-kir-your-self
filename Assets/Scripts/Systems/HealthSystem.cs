using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public class HealthSystem : MonoBehaviour
{
    [Header("⚠️ HELP ⚠️")]
    [Header("Health manager – damage, heal, and display health.")]
    [Header("")]
    [Header("TakeDamage(int)  → returns true if health reaches 0 (death)")]
    [Header("Damage(int)      → void wrapper for UnityEvents")]
    [Header("GiveHeal(int)    → returns true if health reaches max")]
    [Header("Heal(int)        → void wrapper for UnityEvents")]
    [Header("RestoreHealth()  → fully heals to max")]
    [Header("SetHealth(int)   → sets exact health (clamped)")]
    [Header("SetMaxHealth(int) → changes max health")]
    [Header("GetHealth()      → returns current health")]
    [Header("GetMaxHealth()   → returns max health")]
    [Header("IsDead()         → returns true if health <= 0")]
    [Header("")]
    [Header("Events: onHealthChanged, onTakeDamage, onHeal, onDeath, onFullHealth")]
    [Header("------------------------------------------------------------------------")]

    [Header("Health Settings")]
    [Tooltip("Current health (clamped 0 to maxHealth).")]
    public int health = 100;

    [Tooltip("Maximum health.")]
    public int maxHealth = 100;

    [Header("UI References (optional)")]
    public Slider healthSlider;
    public TMP_Text healthText;
    public TMP_Text maxHealthText;

    [Header("Text Formatting")]
    public string healthPrefix = "";
    public string healthSuffix = "";
    public string maxHealthPrefix = "";
    public string maxHealthSuffix = "";

    [Header("Events")]
    public UnityEvent onHealthChanged;
    public UnityEvent onTakeDamage;
    public UnityEvent onHeal;
    public UnityEvent onDeath;
    public UnityEvent onFullHealth;

    // Smoothing time (hardcoded, not exposed in Inspector)
    private const float SMOOTH_TIME = 0.3f;

    private bool isDead = false;
    private Coroutine smoothCoroutine;

    private void Start()
    {
        health = Mathf.Clamp(health, 0, maxHealth);
        UpdateUI();
    }

    private void OnDisable()
    {
        if (smoothCoroutine != null)
            StopCoroutine(smoothCoroutine);
    }

    // ---------- Take Damage ----------
    public bool TakeDamage(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("Use Heal for positive amounts.");
            return false;
        }

        if (isDead) return true;

        int oldHealth = health;
        health = Mathf.Max(0, health - amount);
        bool died = (health == 0 && oldHealth > 0);

        onTakeDamage.Invoke();
        onHealthChanged.Invoke();
        UpdateUI();

        if (died)
        {
            isDead = true;
            onDeath.Invoke();
            return true;
        }
        return false;
    }

    public void Damage(int amount) => TakeDamage(amount);

    // ---------- Heal ----------
    public bool GiveHeal(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("Use TakeDamage for negative amounts.");
            return false;
        }

        if (isDead) return false;

        int oldHealth = health;
        health = Mathf.Min(maxHealth, health + amount);
        bool reachedMax = (health == maxHealth && oldHealth < maxHealth);

        onHeal.Invoke();
        onHealthChanged.Invoke();
        UpdateUI();

        if (reachedMax)
        {
            onFullHealth.Invoke();
            return true;
        }
        return false;
    }

    public void Heal(int amount) => GiveHeal(amount);

    // ---------- Restore ----------
    public void RestoreHealth()
    {
        health = maxHealth;
        isDead = false;
        onFullHealth.Invoke();
        onHealthChanged.Invoke();
        UpdateUI();
    }

    // ---------- Set ----------
    public void SetHealth(int newHealth)
    {
        health = Mathf.Clamp(newHealth, 0, maxHealth);
        if (health == 0 && !isDead)
        {
            isDead = true;
            onDeath.Invoke();
        }
        else if (health > 0 && isDead)
        {
            isDead = false;
        }
        onHealthChanged.Invoke();
        UpdateUI();
    }

    public void SetMaxHealth(int newMax)
    {
        if (newMax <= 0)
        {
            Debug.LogError("Max health must be positive.");
            return;
        }
        maxHealth = newMax;
        health = Mathf.Clamp(health, 0, maxHealth);
        onHealthChanged.Invoke();
        UpdateUI();
    }

    // ---------- Getters ----------
    public int GetHealth() => health;
    public int GetMaxHealth() => maxHealth;
    public float GetHealthPercent() => (float)health / maxHealth;
    public bool IsDead() => isDead || health <= 0;

    // ---------- UI Update ----------
    private void UpdateUI()
    {
        if (healthText != null)
            healthText.text = healthPrefix + health.ToString() + healthSuffix;

        if (maxHealthText != null)
            maxHealthText.text = maxHealthPrefix + maxHealth.ToString() + maxHealthSuffix;

        if (healthSlider != null)
        {
            float targetValue = GetHealthPercent();

            if (smoothCoroutine != null)
                StopCoroutine(smoothCoroutine);

            smoothCoroutine = StartCoroutine(SmoothSlider(targetValue));
        }
    }

    private IEnumerator SmoothSlider(float target)
    {
        float start = healthSlider.value;
        float elapsed = 0f;

        while (elapsed < SMOOTH_TIME)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / SMOOTH_TIME;
            healthSlider.value = Mathf.Lerp(start, target, t);
            yield return null;
        }

        healthSlider.value = target;
        smoothCoroutine = null;
    }

    public void RefreshUI() => UpdateUI();
}