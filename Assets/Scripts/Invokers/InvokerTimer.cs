using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

[AddComponentMenu("Custom/Invoker Timer")]
public class InvokerTimer : MonoBehaviour
{
    [Header("⚠️ Help ⚠️")]
    [Header("Starts a timer that invokes events on start, each tick, and finish.")]
    [Header("Set startAtStart to automatically start timer on scene start.")]
    [Header("ChangeMaxTime() or ChangeMaxTimeRandom() can adjust duration dynamically.")]
    [Header("Display: assign a TextMeshPro text to show remaining time.")]
    [Header("Choose format: SecondsOnly, MinutesSeconds, or HoursMinutesSeconds.")]
    [Header("ChangeMaxTimeRandom() --> min,max format")]
    [Header("------------------------------------------------------------------------")]

    [Header("Settings")]
    public float invokeDelay = 0f;
    public bool startAtStart = false;
    public float maxTime = 0f;

    [Header("Tick Settings")]
    [Range(0.1f, 1f)]
    public float tickInterval = 1f;

    [Header("Events")]
    public UnityEvent onTimerStart;
    public UnityEvent onTick;
    public UnityEvent onTimerFinish;

    [Header("Runtime Info")]
    [SerializeField]
    public float SecondsRemaining;

    [Header("Display (optional)")]
    public TMP_Text displayText;
    public TimeDisplayFormat displayFormat = TimeDisplayFormat.MinutesSeconds;

    public enum TimeDisplayFormat
    {
        SecondsOnly,
        MinutesSeconds,
        HoursMinutesSeconds
    }

    private Coroutine timerCoroutine;
    private float startTime;
    private bool isPaused = false;
    private float pauseTimeOffset;

    public void SetInvokeDelay(float invokeDelay_)
    {
        invokeDelay = invokeDelay_;
    }

    public void ChangeMaxTime(float newMaxTime)
    {
        maxTime = newMaxTime;
    }

    public void ChangeMaxTimeRandom(string minAndMax)
    {
        string[] parts = minAndMax.Split(',');
        if (parts.Length != 2)
        {
            Debug.LogError("ChangeMaxTimeRandom: input must be in format \"min,max\"");
            return;
        }
        if (!float.TryParse(parts[0], out float min) || !float.TryParse(parts[1], out float max))
        {
            Debug.LogError("ChangeMaxTimeRandom: could not parse numbers from \"" + minAndMax + "\"");
            return;
        }
        maxTime = Random.Range(min, max);
    }

    private void Start()
    {
        if (startAtStart)
            StartTimer();
    }

    public void StartTimer()
    {
        if (timerCoroutine != null)
            StopTimer();

        timerCoroutine = StartCoroutine(TimerRoutine());
    }

    public void PauseTimer()
    {
        if (timerCoroutine != null && !isPaused)
        {
            isPaused = true;
            pauseTimeOffset = Time.time - startTime;
        }
    }

    public void ResumeTimer()
    {
        if (timerCoroutine != null && isPaused)
        {
            isPaused = false;
            startTime = Time.time - pauseTimeOffset;
        }
    }

    public void StopTimer()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
        isPaused = false;
        SecondsRemaining = 0f;
        UpdateDisplayText(0f);
    }

    private IEnumerator TimerRoutine()
    {
        if (invokeDelay > 0f)
            yield return new WaitForSeconds(invokeDelay);

        onTimerStart.Invoke();
        startTime = Time.time;
        float elapsed = 0f;
        int lastTickCount = -1;
        bool finished = false;

        while (elapsed < maxTime && !finished)
        {
            if (isPaused)
            {
                yield return null;
                continue;
            }

            elapsed = Time.time - startTime;
            float remaining = Mathf.Max(0f, maxTime - elapsed);
            SecondsRemaining = remaining;
            UpdateDisplayText(remaining);

            int currentTick = Mathf.FloorToInt(elapsed / tickInterval);
            if (currentTick > lastTickCount && currentTick > 0)
            {
                lastTickCount = currentTick;
                onTick.Invoke();
            }

            if (elapsed >= maxTime)
            {
                finished = true;
                break;
            }

            yield return null;
        }

        SecondsRemaining = 0f;
        UpdateDisplayText(0f);

        timerCoroutine = null;

        onTimerFinish.Invoke();
    }

    private void UpdateDisplayText(float remainingSeconds)
    {
        if (displayText == null) return;
        displayText.text = FormatTime(remainingSeconds, displayFormat);
    }

    private string FormatTime(float seconds, TimeDisplayFormat format)
    {
        int totalSeconds = Mathf.FloorToInt(Mathf.Max(0f, seconds));

        switch (format)
        {
            case TimeDisplayFormat.SecondsOnly:
                return totalSeconds.ToString();

            case TimeDisplayFormat.MinutesSeconds:
                int totalMinutes = totalSeconds / 60;
                int remainingSecs = totalSeconds % 60;
                return string.Format("{0:0}:{1:00}", totalMinutes, remainingSecs);

            case TimeDisplayFormat.HoursMinutesSeconds:
                int hours = totalSeconds / 3600;
                int minutes = (totalSeconds % 3600) / 60;
                int secs = totalSeconds % 60;
                return string.Format("{0:0}:{1:00}:{2:00}", hours, minutes, secs);

            default:
                return totalSeconds.ToString();
        }
    }

    private void OnDisable()
    {
        StopTimer();
    }
}