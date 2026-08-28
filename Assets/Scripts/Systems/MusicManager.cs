using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    [Header("⚠️ Help ⚠️")]
    [Header("Background music manager with smooth crossfades, volume, pitch, and position memory.")]
    [Header("")]
    [Header("PlayMusic()      – starts/restarts music (fade in to volume 1)")]
    [Header("StopMusic()      – fades out and stops")]
    [Header("ChangeMusic(clip) – crossfades to a new clip (fade out → change → fade in)")]
    [Header("ChangeVolume(val) – smoothly changes volume (overrides all other fades)")]
    [Header("ChangePitch(val) – smoothly changes pitch")]
    [Header("")]
    [Header("Position Memory:")]
    [Header("Toggle rememberPosition to save each clip's playback position.")]
    [Header("When switching back to a clip, it continues from where it left off.")]
    [Header("------------------------------------------------------------------------")]

    [Header("Fade Settings")]
    public float fadeDuration = 1f;

    [Header("Position Memory")]
    [Tooltip("If true, each clip remembers its playback position when switching away.")]
    public bool rememberPosition = true;

    [Header("Preload (optional)")]
    [Tooltip("Clips you want to preload into memory to avoid stutter on first switch.")]
    public AudioClip[] clipsToPreload;

    [Header("Debug")]
    [Tooltip("Enable to see detailed logs for debugging.")]
    public bool enableDebugLogs = true;

    private AudioSource audioSource;
    private Coroutine currentFade;
    private Coroutine volumeRoutine;
    private Coroutine pitchRoutine;

    private Dictionary<string, float> clipPositions = new Dictionary<string, float>();
    private AudioClip pendingClip;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = 0f;
        audioSource.pitch = 1f;
        audioSource.loop = true;

        // Preload any clips listed in the Inspector
        foreach (AudioClip clip in clipsToPreload)
        {
            if (clip != null)
            {
                clip.LoadAudioData();
                DebugLog($"  - Preloaded clip: {clip.name}");
            }
        }

        DebugLog("MusicManager Awake - AudioSource initialized");
        DebugLog($"  - Volume: {audioSource.volume}, Pitch: {audioSource.pitch}, Loop: {audioSource.loop}");
        DebugLog($"  - Clip assigned in Inspector: {(audioSource.clip != null ? audioSource.clip.name : "null")}");
    }

    private void DebugLog(string message)
    {
        if (enableDebugLogs)
            Debug.Log($"[MusicManager] {message}");
    }

    // === Main Music Controls ===

    public void PlayMusic()
    {
        DebugLog($"PlayMusic() called - Current clip: {(audioSource.clip != null ? audioSource.clip.name : "null")}");
        DebugLog($"  - isPlaying: {audioSource.isPlaying}, currentFade: {(currentFade != null ? "active" : "null")}");

        StopVolumeRoutine();
        if (currentFade != null)
        {
            DebugLog("  - Stopping existing fade");
            StopCoroutine(currentFade);
            currentFade = null;
        }

        if (audioSource.isPlaying && currentFade == null)
        {
            DebugLog("  - Already playing, no fade active. Returning.");
            return;
        }

        if (audioSource.clip == null)
        {
            DebugLog("  - WARNING: audioSource.clip is null! Assign a clip to the AudioSource or set it before calling PlayMusic.");
            return;
        }

        pendingClip = null;
        DebugLog("  - Starting FadeIn coroutine");
        currentFade = StartCoroutine(FadeIn(audioSource.clip));
    }

    public void StopMusic()
    {
        DebugLog($"StopMusic() called - isPlaying: {audioSource.isPlaying}");

        if (!audioSource.isPlaying)
        {
            DebugLog("  - Not playing. Returning.");
            return;
        }

        SaveCurrentClipPosition();
        StopVolumeRoutine();
        if (currentFade != null)
        {
            DebugLog("  - Stopping existing fade");
            StopCoroutine(currentFade);
            currentFade = null;
        }
        pendingClip = null;
        DebugLog("  - Starting FadeOut coroutine");
        currentFade = StartCoroutine(FadeOut());
    }

    public void ChangeMusic(AudioClip newClip)
    {
        DebugLog($"ChangeMusic() called - New clip: {(newClip != null ? newClip.name : "null")}");
        DebugLog($"  - Current clip: {(audioSource.clip != null ? audioSource.clip.name : "null")}");
        DebugLog($"  - isPlaying: {audioSource.isPlaying}, currentFade: {(currentFade != null ? "active" : "null")}");

        if (newClip == null)
        {
            DebugLog("  - ERROR: newClip is null!");
            return;
        }

        pendingClip = newClip;

        SaveCurrentClipPosition();
        StopVolumeRoutine();
        if (currentFade != null)
        {
            DebugLog("  - Stopping existing fade");
            StopCoroutine(currentFade);
            currentFade = null;
        }
        DebugLog("  - Starting FadeOutIn coroutine");
        currentFade = StartCoroutine(FadeOutIn(newClip));
    }

    // === Volume & Pitch Control ===

    public void ChangeVolume(float targetVolume)
    {
        DebugLog($"ChangeVolume() called - Target: {targetVolume}, Current: {audioSource.volume}");

        StopVolumeRoutine();

        if (currentFade != null)
        {
            DebugLog("  - Interrupting a fade. Checking for pending clip.");
            if (pendingClip != null)
            {
                DebugLog($"  - Switching to pending clip now: {pendingClip.name}");
                audioSource.clip = pendingClip;
                audioSource.time = GetClipPosition(pendingClip);
                audioSource.Play();
                pendingClip = null;
            }
            StopCoroutine(currentFade);
            currentFade = null;
        }

        DebugLog("  - Starting SmoothVolume coroutine");
        volumeRoutine = StartCoroutine(SmoothVolume(targetVolume));
    }

    public void ChangePitch(float targetPitch)
    {
        DebugLog($"ChangePitch() called - Target: {targetPitch}, Current: {audioSource.pitch}");

        if (pitchRoutine != null)
        {
            DebugLog("  - Stopping existing pitch routine");
            StopCoroutine(pitchRoutine);
        }
        DebugLog("  - Starting SmoothPitch coroutine");
        pitchRoutine = StartCoroutine(SmoothPitch(targetPitch));
    }

    // === Position Memory ===

    private void SaveCurrentClipPosition()
    {
        if (!rememberPosition) return;
        if (audioSource.clip == null) return;

        float currentTime = audioSource.time;
        string clipName = audioSource.clip.name;

        DebugLog($"  - Saving position for '{clipName}': {currentTime}s");

        if (clipPositions.ContainsKey(clipName))
            clipPositions[clipName] = currentTime;
        else
            clipPositions.Add(clipName, currentTime);
    }

    private float GetClipPosition(AudioClip clip)
    {
        if (!rememberPosition) return 0f;
        if (clip == null) return 0f;

        string clipName = clip.name;
        if (clipPositions.TryGetValue(clipName, out float position))
        {
            DebugLog($"  - Retrieved position for '{clipName}': {position}s");
            return position;
        }

        DebugLog($"  - No saved position for '{clipName}', starting from 0");
        return 0f;
    }

    public void SavePosition()
    {
        DebugLog("SavePosition() called manually");
        SaveCurrentClipPosition();
    }

    public void ClearClipPosition(string clipName)
    {
        DebugLog($"ClearClipPosition() called for '{clipName}'");
        if (clipPositions.ContainsKey(clipName))
            clipPositions.Remove(clipName);
    }

    public void ClearAllPositions()
    {
        DebugLog($"ClearAllPositions() called - Clearing {clipPositions.Count} entries");
        clipPositions.Clear();
    }

    // === Helpers ===

    private void StopVolumeRoutine()
    {
        if (volumeRoutine != null)
        {
            DebugLog("  - Stopping volume routine");
            StopCoroutine(volumeRoutine);
            volumeRoutine = null;
        }
    }

    // === Fade Coroutines ===

    private IEnumerator FadeIn(AudioClip clip)
    {
        DebugLog($"[FadeIn] START - Clip: {(clip != null ? clip.name : "null")}");

        audioSource.clip = clip;
        audioSource.volume = 0f;

        if (rememberPosition)
        {
            float savedPos = GetClipPosition(clip);
            audioSource.time = savedPos;
            DebugLog($"  - Set time to: {audioSource.time}");
        }

        audioSource.Play();
        DebugLog($"  - Play() called - isPlaying: {audioSource.isPlaying}");

        float timer = 0f;
        while (timer < fadeDuration)
        {
            audioSource.volume = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        audioSource.volume = 1f;
        currentFade = null;
        pendingClip = null;

        DebugLog($"[FadeIn] END - Volume: {audioSource.volume}, Time: {audioSource.time}");
    }

    private IEnumerator FadeOut()
    {
        DebugLog($"[FadeOut] START - Current volume: {audioSource.volume}");

        float startVolume = audioSource.volume;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeDuration);
            timer += Time.deltaTime;
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = 0f;
        currentFade = null;

        DebugLog($"[FadeOut] END - isPlaying: {audioSource.isPlaying}, Volume: {audioSource.volume}");
    }

    private IEnumerator FadeOutIn(AudioClip newClip)
    {
        DebugLog($"[FadeOutIn] START - New clip: {(newClip != null ? newClip.name : "null")}");
        DebugLog($"  - Current isPlaying: {audioSource.isPlaying}, Volume: {audioSource.volume}");

        float startVolume = audioSource.volume;
        float timer = 0f;

        // Fade out current (if playing)
        if (audioSource.isPlaying)
        {
            DebugLog($"  - Fading out from {startVolume} to 0");
            while (timer < fadeDuration)
            {
                audioSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeDuration);
                timer += Time.deltaTime;
                yield return null;
            }
            audioSource.Stop();
            DebugLog($"  - Stopped - isPlaying: {audioSource.isPlaying}");
        }
        else
        {
            DebugLog($"  - Not playing, skipping fade-out");
            yield return null;
        }

        // Set new clip and position
        DebugLog($"  - Setting new clip: {newClip.name}");
        audioSource.clip = newClip;
        if (rememberPosition)
        {
            float savedPos = GetClipPosition(newClip);
            audioSource.time = savedPos;
            DebugLog($"  - Set time to: {audioSource.time}");
        }

        audioSource.Play();
        DebugLog($"  - Play() called - isPlaying: {audioSource.isPlaying}");
        timer = 0f;

        // Fade in
        while (timer < fadeDuration)
        {
            audioSource.volume = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            timer += Time.deltaTime;
            yield return null;
        }

        audioSource.volume = 1f;
        currentFade = null;
        pendingClip = null;

        DebugLog($"[FadeOutIn] END - Clip: {audioSource.clip.name}, Volume: {audioSource.volume}, Time: {audioSource.time}");
    }

    private IEnumerator SmoothVolume(float targetVolume)
    {
        DebugLog($"[SmoothVolume] START - From {audioSource.volume} to {targetVolume}");

        float start = audioSource.volume;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            audioSource.volume = Mathf.Lerp(start, targetVolume, timer / fadeDuration);
            timer += Time.deltaTime;
            yield return null;
        }

        audioSource.volume = targetVolume;
        volumeRoutine = null;

        DebugLog($"[SmoothVolume] END - Volume: {audioSource.volume}");
    }

    private IEnumerator SmoothPitch(float targetPitch)
    {
        DebugLog($"[SmoothPitch] START - From {audioSource.pitch} to {targetPitch}");

        float start = audioSource.pitch;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            audioSource.pitch = Mathf.Lerp(start, targetPitch, timer / fadeDuration);
            timer += Time.deltaTime;
            yield return null;
        }

        audioSource.pitch = targetPitch;
        pitchRoutine = null;

        DebugLog($"[SmoothPitch] END - Pitch: {audioSource.pitch}");
    }
}