using UnityEngine;
using System.Collections.Generic;

public class GameTick : MonoBehaviour
{
    [Header("Settings")]
    public float defaultTick = 1f;
    public float smoothTime = 0.5f;

    [Header("Audio")]
    public bool scaleAudioPitch = true;   // if true, all AudioSources pitch scales with tick

    [Header("Runtime")]
    [SerializeField] private float currentTick = 1f;
    [SerializeField] private float targetTick = 1f;

    // ─── Smoothing state ──────────────────────────────────────────────────
    private float startTick;
    private float transitionDuration;
    private float elapsedTime;
    private bool isSmoothing = false;

    // ─── Audio pitch storage ─────────────────────────────────────────────
    private Dictionary<AudioSource, float> originalPitches = new Dictionary<AudioSource, float>();
    private bool audioInitialized = false;

    public static GameTick Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        currentTick = Time.timeScale;
        targetTick = currentTick;
        defaultTick = currentTick;
    }

    void Start()
    {
        if (scaleAudioPitch)
            InitAudio();
    }

    void Update()
    {
        if (!isSmoothing) return;

        elapsedTime += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(elapsedTime / transitionDuration);

        currentTick = Mathf.Lerp(startTick, targetTick, t);
        Time.timeScale = currentTick;

        if (scaleAudioPitch)
            UpdateAudioPitch(currentTick);

        if (t >= 1f)
        {
            currentTick = targetTick;
            Time.timeScale = currentTick;
            if (scaleAudioPitch)
                UpdateAudioPitch(currentTick);
            isSmoothing = false;
        }
    }

    // ─── Audio helpers ──────────────────────────────────────────────────

    private void InitAudio()
    {
        if (audioInitialized) return;

        AudioSource[] sources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        originalPitches.Clear();
        foreach (var src in sources)
        {
            if (!originalPitches.ContainsKey(src))
                originalPitches.Add(src, src.pitch);
        }
        audioInitialized = true;
        UpdateAudioPitch(currentTick);
    }

    private void UpdateAudioPitch(float tick)
    {
        if (!audioInitialized) InitAudio();

        foreach (var kvp in originalPitches)
        {
            if (kvp.Key != null)
                kvp.Key.pitch = kvp.Value * tick;
        }
    }

    // ─── Public methods ──────────────────────────────────────────────────

    public float GetTick() => currentTick;
    public float GetTargetTick() => targetTick;
    public bool IsSmoothing() => isSmoothing;

    public void SetTick(float newTick) => SetTick(newTick, smoothTime);

    public void SetTick(float newTick, float duration)
    {
        if (Mathf.Approximately(newTick, targetTick) && !isSmoothing) return;

        startTick = currentTick;
        targetTick = newTick;
        transitionDuration = Mathf.Max(duration, 0.01f);
        elapsedTime = 0f;
        isSmoothing = true;
    }

    public void SetTickInstant(float newTick)
    {
        currentTick = newTick;
        targetTick = newTick;
        Time.timeScale = currentTick;
        if (scaleAudioPitch) UpdateAudioPitch(currentTick);
        isSmoothing = false;
    }

    public void IncrementTick(float amount) => SetTick(targetTick + amount, smoothTime);
    public void DecrementTick(float amount) => SetTick(targetTick - amount, smoothTime);
    public void ResetTick() => SetTick(defaultTick, smoothTime);

    // ─── Refresh audio sources (call after adding new AudioSources at runtime) ──
    public void RefreshAudio()
    {
        audioInitialized = false;
        InitAudio();
    }
}