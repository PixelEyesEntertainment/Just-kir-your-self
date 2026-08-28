using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

public class MusicGameManager : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent OnGameWon;
    public UnityEvent OnGameCanceled;
    public UnityEvent OnPerfect;
    public UnityEvent OnPressed;
    public UnityEvent OnMissed;

    [Header("Audio")]
    public AudioSource[] laneAudioSources;
    public AudioSource audioSourceFallback;
    public AudioClip[] noteSounds;
    public AudioClip[] wrongNoteSounds;

    [Header("Canvas")]
    public Canvas gameCanvas;

    [Header("Settings")]
    public float speed = 200f;
    public float startDelay = 0f;

    [Header("Position Thresholds (pixels)")]
    public float perfectThreshold = 20f;
    public float goodThreshold = 50f;

    [Header("Hold Settings")]
    public float holdEarlyWindow = 0.15f;
    public float holdLateWindow = 0.5f;

    [Header("Key Mapping")]
    public KeyCode[] laneKeys = new KeyCode[4]
    {
        KeyCode.LeftArrow,
        KeyCode.DownArrow,
        KeyCode.UpArrow,
        KeyCode.RightArrow
    };

    [Header("References")]
    public RectTransform lane0;
    public RectTransform lane1;
    public RectTransform lane2;
    public RectTransform lane3;
    public GameObject notePrefab;
    public RectTransform targetPoint;

    [Header("Debug")]
    public bool showHitZones = true;
    public bool showNotePivots = true;

    private ChartData chart;
    private List<Note> activeNotes = new List<Note>();
    private int nextNoteIndex = 0;
    private float startTime;
    private bool isRunning = false;

    private Note[] heldNotes = new Note[4];

    private float canvasHeight;
    private float spawnY;
    private float targetY;
    private float travelTime;
    private float bottomY;

    private int pressedCount = 0;
    private int perfectCount = 0;
    private int missedCount = 0;
    private int totalNotes = 0;

    private Vector3 targetWorldPos;
    private float worldScale = 1f;

    void Awake()
    {
        if (gameCanvas == null) { Debug.LogError("❌ gameCanvas not assigned!"); return; }

        CanvasScaler scaler = gameCanvas.GetComponent<CanvasScaler>();
        if (scaler != null && scaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
            canvasHeight = scaler.referenceResolution.y;
        else
            canvasHeight = gameCanvas.GetComponent<RectTransform>().rect.height;

        spawnY = canvasHeight;
        bottomY = -canvasHeight * 2f;
        targetY = targetPoint != null ? targetPoint.anchoredPosition.y : 0f;
        travelTime = (spawnY - targetY) / speed;

        if (targetPoint != null)
        {
            targetWorldPos = targetPoint.transform.position;
            worldScale = targetPoint.lossyScale.y;
        }
        else
        {
            targetWorldPos = gameCanvas.transform.position;
            worldScale = gameCanvas.transform.lossyScale.y;
        }

        Debug.Log($"📐 Canvas: {canvasHeight} | Spawn: {spawnY} | Target: {targetY} | Bottom: {bottomY}");

        if (lane0 == null || lane1 == null || lane2 == null || lane3 == null)
            Debug.LogError("❌ Missing lane references!");
        if (notePrefab == null)
            Debug.LogError("❌ NotePrefab not assigned!");
        if (laneAudioSources == null || laneAudioSources.Length != 4)
            Debug.LogError("❌ Assign 4 AudioSources to laneAudioSources!");
        if (wrongNoteSounds == null || wrongNoteSounds.Length == 0)
            Debug.LogWarning("⚠️ wrongNoteSounds is empty – misses will be silent.");

        for (int i = 0; i < 4; i++) heldNotes[i] = null;
    }

    void OnDrawGizmos()
    {
        if (!showHitZones && !showNotePivots) return;

        if (targetPoint != null)
        {
            targetWorldPos = targetPoint.transform.position;
            worldScale = targetPoint.lossyScale.y;
        }
        else if (gameCanvas != null)
        {
            targetWorldPos = gameCanvas.transform.position;
            worldScale = gameCanvas.transform.lossyScale.y;
        }

        if (showHitZones && targetPoint != null)
        {
            float perfectWorld = perfectThreshold * worldScale;
            float goodWorld = goodThreshold * worldScale;

            Gizmos.color = new Color(0, 1, 0, 0.2f);
            Gizmos.DrawWireSphere(targetWorldPos, perfectWorld);
            Gizmos.color = new Color(0, 1, 0, 0.05f);
            Gizmos.DrawSphere(targetWorldPos, perfectWorld);

            Gizmos.color = new Color(1, 1, 0, 0.15f);
            Gizmos.DrawWireSphere(targetWorldPos, goodWorld);
            Gizmos.color = new Color(1, 1, 0, 0.03f);
            Gizmos.DrawSphere(targetWorldPos, goodWorld);
        }

        if (showNotePivots && Application.isPlaying)
        {
            foreach (Note note in activeNotes)
            {
                if (note == null) continue;
                Vector3 notePos = note.transform.position;
                Gizmos.color = note.state == Note.State.Holding ? Color.cyan : Color.white;
                Gizmos.DrawSphere(notePos, 5f);
                Gizmos.DrawLine(notePos, notePos + note.transform.up * 20f);
            }
        }
    }

    void LoadChart(string chartName)
    {
        TextAsset json = Resources.Load<TextAsset>(chartName);
        if (json == null) { Debug.LogError($"❌ Chart '{chartName}' not found!"); return; }
        chart = JsonUtility.FromJson<ChartData>(json.text);
        if (chart == null || chart.notes == null) { Debug.LogError($"❌ Failed to parse chart '{chartName}'!"); return; }

        float cumulative = 0f;
        for (int i = 0; i < chart.notes.Length; i++)
        {
            cumulative += chart.notes[i].time;
            chart.notes[i].time = cumulative;
        }

        if (chart.speed > 0) speed = chart.speed;
        totalNotes = chart.notes.Length;
        travelTime = (spawnY - targetY) / speed;
        Debug.Log($"✅ Loaded '{chartName}' with {totalNotes} notes | Travel time: {travelTime:F2}s");
    }

    void Update()
    {
        if (!isRunning || chart == null) return;

        float currentTime = Time.time - startTime;

        while (nextNoteIndex < chart.notes.Length && chart.notes[nextNoteIndex].time <= currentTime + travelTime)
        {
            SpawnNote(chart.notes[nextNoteIndex]);
            nextNoteIndex++;
        }

        for (int i = activeNotes.Count - 1; i >= 0; i--)
        {
            Note note = activeNotes[i];
            note.UpdatePosition(currentTime);

            if (note.state == Note.State.Waiting && note.GetCurrentY() < targetY - goodThreshold)
            {
                note.CanHit = false;
                if (!note.IsPressed && !note.WasMissed)
                {
                    MissedNote(note);
                    note.WasMissed = true;
                }
            }

            if (note.GetCurrentY() < bottomY)
            {
                if (!note.IsPressed && !note.WasMissed)
                {
                    MissedNote(note);
                    note.WasMissed = true;
                }
                activeNotes.RemoveAt(i);
                Destroy(note.gameObject);
                continue;
            }
        }

        // ─── Held notes ──────────────────────────────────────────────
        for (int i = 0; i < 4; i++)
        {
            Note held = heldNotes[i];
            if (held != null)
            {
                bool keyStillDown = Input.GetKey(laneKeys[i]);

                // ─── Release early ─────────────────────────────────────
                if (!keyStillDown && held.state == Note.State.Holding)
                {
                    Debug.Log($"🔷 Hold released early on lane {i} → Pressed");
                    StopHoldSound(i);
                    PressedNote(held);
                    held.CompleteHold();
                    activeNotes.Remove(held);
                    Destroy(held.gameObject);
                    heldNotes[i] = null;
                }
                // ─── Hold duration complete ───────────────────────────
                else if (keyStillDown && held.state == Note.State.Holding)
                {
                    if (currentTime >= held.holdStartTime + held.holdDuration)
                    {
                        float startDistance = Mathf.Abs(held.GetYAtTime(held.holdStartTime) - targetY);
                        if (startDistance <= perfectThreshold)
                        {
                            Debug.Log($"✅ Hold complete (PERFECT) on lane {i}!");
                            PerfectNote(held);
                        }
                        else
                        {
                            Debug.Log($"🔷 Hold complete (Pressed) on lane {i}!");
                            PressedNote(held);
                        }
                        StopHoldSound(i);
                        held.CompleteHold();
                        activeNotes.Remove(held);
                        Destroy(held.gameObject);
                        heldNotes[i] = null;
                    }
                }
            }
        }

        // ─── Input ──────────────────────────────────────────────────────
        for (int i = 0; i < 4; i++)
        {
            if (Input.GetKeyDown(laneKeys[i]))
                TryHit(i);
        }

        if (nextNoteIndex >= chart.notes.Length && activeNotes.Count == 0 && isRunning)
            WinGame();
    }

    // ─── PUBLIC METHODS ────────────────────────────────────────────

    public void StartGame() => StartGame("chart");
    public void StartGame(string chartName)
    {
        if (isRunning) return;
        LoadChart(chartName);
        if (chart == null) return;

        activeNotes.Clear();
        for (int i = 0; i < 4; i++)
        {
            heldNotes[i] = null;
            StopHoldSound(i);
        }
        nextNoteIndex = 0;
        pressedCount = 0;
        perfectCount = 0;
        missedCount = 0;
        startTime = Time.time + startDelay;
        isRunning = true;
        Debug.Log($"🎮 Game started with '{chartName}'!");
    }

    public void CancelGame()
    {
        isRunning = false;
        for (int i = 0; i < 4; i++)
        {
            heldNotes[i] = null;
            StopHoldSound(i);
        }
        foreach (Note n in activeNotes) Destroy(n.gameObject);
        activeNotes.Clear();
        OnGameCanceled?.Invoke();
    }

    // ─── INTERNAL ──────────────────────────────────────────────────

    void SpawnNote(NoteEntry entry)
    {
        RectTransform lane = entry.lane switch
        {
            0 => lane0,
            1 => lane1,
            2 => lane2,
            3 => lane3,
            _ => null
        };
        if (lane == null) return;

        float currentTime = Time.time - startTime;
        float spawnTime = entry.time - travelTime;

        GameObject obj = Instantiate(notePrefab, lane);
        obj.SetActive(true);
        Note note = obj.GetComponent<Note>();

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);

        note.Initialize(entry.time, entry.lane, entry.soundId, spawnY, targetY, speed, spawnTime, entry.hold);

        rt.anchoredPosition = new Vector2(0, spawnY);
        activeNotes.Add(note);
    }

    void TryHit(int lane)
    {
        if (!isRunning) return;
        if (heldNotes[lane] != null) return;

        Note target = null;
        float earliest = float.MaxValue;
        foreach (Note n in activeNotes)
        {
            if (n.lane == lane && n.state == Note.State.Waiting && n.CanHit && n.hitTime < earliest)
            {
                earliest = n.hitTime;
                target = n;
            }
        }

        if (target == null)
        {
            PlayWrongNote(lane);
            return;
        }

        if (!target.CanHit)
        {
            PlayWrongNote(lane);
            MissedNote(target);
            target.state = Note.State.Done;
            activeNotes.Remove(target);
            Destroy(target.gameObject);
            return;
        }

        float currentTime = Time.time - startTime;
        float noteY = target.GetCurrentY();
        float distance = Mathf.Abs(noteY - targetY);

        // ─── TAP NOTE ────────────────────────────────────────────────
        if (target.holdDuration <= 0)
        {
            PlayNoteSound(lane, target.soundId);
            if (distance <= perfectThreshold)
                PerfectNote(target);
            else
                PressedNote(target);

            target.state = Note.State.Done;
            activeNotes.Remove(target);
            Destroy(target.gameObject);
            return;
        }

        // ─── HOLD NOTE ──────────────────────────────────────────────
        if (currentTime < target.hitTime - holdEarlyWindow || currentTime > target.hitTime + holdLateWindow)
        {
            PlayWrongNote(lane);
            MissedNote(target);
            target.state = Note.State.Done;
            activeNotes.Remove(target);
            Destroy(target.gameObject);
            Debug.Log($"❌ Miss Hold! Lane {lane} (timing off)");
            return;
        }

        // ─── Start hold ─────────────────────────────────────────────
        target.StartHold(currentTime);
        heldNotes[lane] = target;

        // Play the note ONCE (clip must be long enough for the hold duration)
        AudioSource src = laneAudioSources[lane];
        if (src != null && noteSounds != null && noteSounds.Length > target.soundId && noteSounds[target.soundId] != null)
        {
            src.clip = noteSounds[target.soundId];
            src.loop = false;   // no loop – rely on long clip
            src.Play();
        }

        Debug.Log($"🔷 Hold started on lane {lane}");
    }

    void StopHoldSound(int lane)
    {
        AudioSource src = laneAudioSources[lane];
        if (src != null && src.isPlaying)
        {
            src.Stop();
            src.clip = null;
        }
    }

    void PerfectNote(Note note)
    {
        perfectCount++;
        pressedCount++;
        OnPerfect?.Invoke();
        if (note != null) note.IsPressed = true;
        Debug.Log($"⭐ PERFECT! (Perfect: {perfectCount}, Pressed: {pressedCount})");
    }

    void PressedNote(Note note)
    {
        pressedCount++;
        OnPressed?.Invoke();
        if (note != null) note.IsPressed = true;
        Debug.Log($"🔷 Pressed! (Total pressed: {pressedCount})");
    }

    void MissedNote(Note note)
    {
        missedCount++;
        OnMissed?.Invoke();
        if (note != null) note.IsPressed = false;
        int lane = note != null ? note.lane : 0;
        PlayWrongNote(lane);
        Debug.Log($"❌ Missed! (Total missed: {missedCount})");
    }

    void PlayNoteSound(int lane, int soundId) // for taps only
    {
        if (laneAudioSources != null && laneAudioSources.Length > lane && laneAudioSources[lane] != null)
        {
            if (noteSounds != null && noteSounds.Length > soundId && noteSounds[soundId] != null)
                laneAudioSources[lane].PlayOneShot(noteSounds[soundId]);
        }
        else if (audioSourceFallback != null)
        {
            audioSourceFallback.PlayOneShot(noteSounds[soundId]);
        }
    }

    void PlayWrongNote(int lane)
    {
        if (wrongNoteSounds != null && wrongNoteSounds.Length > 0)
        {
            AudioClip clip = wrongNoteSounds[Random.Range(0, wrongNoteSounds.Length)];
            if (clip != null)
            {
                if (laneAudioSources != null && laneAudioSources.Length > lane && laneAudioSources[lane] != null)
                    laneAudioSources[lane].PlayOneShot(clip);
                else if (audioSourceFallback != null)
                    audioSourceFallback.PlayOneShot(clip);
            }
        }
        else
        {
            if (noteSounds != null && noteSounds.Length > 0 && audioSourceFallback != null)
                audioSourceFallback.PlayOneShot(noteSounds[0], 0.3f);
        }
    }

    void WinGame()
    {
        isRunning = false;
        OnGameWon?.Invoke();
        Debug.Log($"🏆 All done! Perfect: {perfectCount} | Pressed: {pressedCount} | Missed: {missedCount} / {totalNotes}");
    }
}