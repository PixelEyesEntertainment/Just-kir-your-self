using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Events;

public class SceneManagement : MonoBehaviour
{
    [Header("⚠️ HELP ⚠️")]
    [Header("Save/load scenes with optional delays.")]
    [Header("")]
    [Header("SaveCurrentScene() → saves active scene as 'LastScene'")]
    [Header("")]
    [Header("LoadLastScene(\"0\")      → loads saved scene (delay optional)")]
    [Header("LoadLastScene(\"1.5\")    → loads with 1.5s delay")]
    [Header("")]
    [Header("SaveCustomScene(\"Key\")  → saves current scene under custom key")]
    [Header("LoadCustomScene(\"Key\")   → loads scene under key (immediate)")]
    [Header("LoadCustomScene(\"Key,3\") → loads scene under key with delay")]
    [Header("")]
    [Header("LoadScene(\"Level2\")      → loads any scene by name (immediate)")]
    [Header("LoadScene(\"Level2,2.5\") → loads any scene by name with delay")]
    [Header("")]
    [Header("onSave      → fires after save")]
    [Header("onLoadStart → fires immediately when load is called (before delay)")]
    [Header("------------------------------------------------------------------------")]

    [Header("Runtime (Read-Only)")]
    public string lastScene;

    [Header("Events")]
    public UnityEvent onSave;
    public UnityEvent onLoadStart;

    private const string LASTSCENE_KEY = "LastScene";
    private Coroutine loadCoroutine;
    private bool isLoading;

    private void Start()
    {
        if (PlayerPrefs.HasKey(LASTSCENE_KEY))
            lastScene = PlayerPrefs.GetString(LASTSCENE_KEY);
        else
            lastScene = "No saved scene.";
    }

    // === Save Current Scene ===

    public void SaveCurrentScene()
    {
        lastScene = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString(LASTSCENE_KEY, lastScene);
        PlayerPrefs.Save();
        onSave.Invoke();
    }

    // === Load Last Saved Scene (delay as string, e.g., "0" or "1.5") ===

    public void LoadLastScene(string delayString)
    {
        if (isLoading)
        {
            Debug.LogWarning("Already loading a scene. Please wait.");
            return;
        }

        if (!PlayerPrefs.HasKey(LASTSCENE_KEY))
        {
            Debug.LogWarning("No saved scene found. Save one first.");
            return;
        }

        float delay = 0f;
        if (!string.IsNullOrEmpty(delayString))
            float.TryParse(delayString, out delay);

        lastScene = PlayerPrefs.GetString(LASTSCENE_KEY);
        StartLoad(lastScene, delay);
    }

    // === Custom Scene Save ===

    public void SaveCustomScene(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogError("Key cannot be empty.");
            return;
        }

        string currentScene = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString(key, currentScene);
        PlayerPrefs.Save();
        onSave.Invoke();
    }

    // === Load Custom Scene (key or key,delay) ===

    public void LoadCustomScene(string keyAndDelay)
    {
        if (isLoading)
        {
            Debug.LogWarning("Already loading a scene. Please wait.");
            return;
        }

        if (string.IsNullOrEmpty(keyAndDelay))
        {
            Debug.LogError("Key cannot be empty.");
            return;
        }

        string key = keyAndDelay;
        float delay = 0f;

        if (keyAndDelay.Contains(","))
        {
            string[] parts = keyAndDelay.Split(',');
            if (parts.Length == 2)
            {
                key = parts[0].Trim();
                float.TryParse(parts[1].Trim(), out delay);
            }
            else
            {
                Debug.LogWarning($"Invalid format '{keyAndDelay}'. Using key as is, delay=0.");
            }
        }

        if (string.IsNullOrEmpty(key))
        {
            Debug.LogError("Key cannot be empty.");
            return;
        }

        if (!PlayerPrefs.HasKey(key))
        {
            Debug.LogWarning($"No scene saved under key '{key}'.");
            return;
        }

        string scene = PlayerPrefs.GetString(key);
        StartLoad(scene, delay);
    }

    // === Load Any Scene by Name (name or name,delay) ===

    public void LoadScene(string combined)
    {
        if (isLoading)
        {
            Debug.LogWarning("Already loading a scene. Please wait.");
            return;
        }

        if (string.IsNullOrEmpty(combined))
        {
            Debug.LogError("Scene name is empty.");
            return;
        }

        string sceneName = combined;
        float delay = 0f;

        if (combined.Contains(","))
        {
            string[] parts = combined.Split(',');
            if (parts.Length == 2)
            {
                sceneName = parts[0].Trim();
                float.TryParse(parts[1].Trim(), out delay);
            }
            else
            {
                Debug.LogWarning($"Invalid format '{combined}'. Using scene name as is, delay=0.");
            }
        }

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Scene name is empty.");
            return;
        }

        StartLoad(sceneName, delay);
    }

    // === Internal: fires onLoadStart immediately, then delays, then loads ===

    private void StartLoad(string sceneName, float delay)
    {
        if (loadCoroutine != null)
            StopCoroutine(loadCoroutine);

        isLoading = true;
        onLoadStart.Invoke();

        loadCoroutine = StartCoroutine(LoadSceneAsync(sceneName, delay));
    }

    private IEnumerator LoadSceneAsync(string sceneName, float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);
        while (!async.isDone)
            yield return null;

        isLoading = false;
        loadCoroutine = null;
    }
}