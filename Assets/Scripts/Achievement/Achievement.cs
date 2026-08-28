using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Custom/Achievement")]
public class Achievement : MonoBehaviour
{
    [Header("⚠️ HELP ⚠️")]
    [Header("An achievement with progress tracking and unlock event.")]
    [Header("")]
    [Header("AddProgress(int) – increases progress; if target reached, calls Unlock().")]
    [Header("Unlock() – forces unlock (fires event, saves).")]
    [Header("Reset() – resets progress and locked state.")]
    [Header("")]
    [Header("onUnlocked – invoked when achievement is unlocked (e.g., play sound, show popup).")]
    [Header("------------------------------------------------------------------------")]

    [Header("Identification")]
    [Tooltip("Unique ID used for saving (e.g., 'Kill10Enemies').")]
    public string achievementID;

    [Header("Display Info")]
    public string title;
    [TextArea(3, 5)]
    public string description;
    public Sprite icon;

    [Header("Progress")]
    public int targetProgress = 1;
    public int currentProgress = 0;

    [Header("Visibility")]
    [Tooltip("If true, title and description are hidden (shown as '???') until unlocked.")]
    public bool isHidden = false;

    [Header("Runtime")]
    public bool isUnlocked = false;

    [Header("Events")]
    public UnityEvent onUnlocked;

    // Internal reference to manager (set by manager on registration)
    public AchievementManager Manager { get; set; }

    /// <summary>
    /// Adds progress. If progress reaches or exceeds target, unlocks.
    /// </summary>
    public void AddProgress(int amount)
    {
        if (isUnlocked) return;
        if (amount <= 0) return;

        currentProgress = Mathf.Min(currentProgress + amount, targetProgress);
        if (currentProgress >= targetProgress)
            Unlock();
    }

    /// <summary>
    /// Force-unlocks the achievement, even if progress isn't full.
    /// </summary>
    public void Unlock()
    {
        if (isUnlocked) return;
        isUnlocked = true;
        currentProgress = targetProgress;

        onUnlocked.Invoke();

        // Notify manager using the correct method name
        if (Manager != null)
            Manager.NotifyAchievementUnlocked(this);
    }

    /// <summary>
    /// Resets progress and lock state (for testing).
    /// </summary>
    public void Reset()
    {
        isUnlocked = false;
        currentProgress = 0;
    }

    /// <summary>
    /// Returns the display title – if hidden and not unlocked, returns "???".
    /// </summary>
    public string GetDisplayTitle()
    {
        if (isHidden && !isUnlocked)
            return "???";
        return title;
    }

    /// <summary>
    /// Returns the display description – if hidden and not unlocked, returns "???".
    /// </summary>
    public string GetDisplayDescription()
    {
        if (isHidden && !isUnlocked)
            return "???";
        return description;
    }

    /// <summary>
    /// Returns progress as a 0-1 float for UI sliders.
    /// </summary>
    public float GetProgressPercent()
    {
        if (targetProgress <= 0) return 0f;
        return Mathf.Clamp01((float)currentProgress / targetProgress);
    }

    private void Awake()
    {
        // Ensure we have a valid ID if not set
        if (string.IsNullOrEmpty(achievementID))
            achievementID = gameObject.name;
    }
}