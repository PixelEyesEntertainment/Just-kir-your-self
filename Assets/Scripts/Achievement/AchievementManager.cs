using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    [Header("Auto-Registration")]
    public bool autoRegister = true;

    [Header("Popup")]
    public GameObject popupPrefab;
    public Transform popupParent;

    [Header("Events")]
    public UnityEvent onAnyAchievementUnlocked;
    public event Action<Achievement> OnAchievementUnlocked;

    public List<Achievement> AllAchievements => allAchievements;

    private List<Achievement> allAchievements = new List<Achievement>();
    private Dictionary<string, Achievement> achievementMap = new Dictionary<string, Achievement>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (autoRegister)
            RegisterAllInScene();
    }

    private void Start()
    {
        Load();
    }

    public void RegisterAllInScene()
    {
        Achievement[] found = FindObjectsOfType<Achievement>(true);
        foreach (var ach in found)
        {
            Register(ach);
        }
        Debug.Log($"[AchievementManager] Registered {allAchievements.Count} achievements.");
    }

    public void Register(Achievement ach)
    {
        if (ach == null) return;
        if (achievementMap.ContainsKey(ach.achievementID))
        {
            Debug.LogWarning($"Achievement ID '{ach.achievementID}' already registered. Skipping duplicate.");
            return;
        }
        ach.Manager = this;
        allAchievements.Add(ach);
        achievementMap[ach.achievementID] = ach;
    }

    public void NotifyAchievementUnlocked(Achievement ach)
    {
        if (ach == null) return;

        onAnyAchievementUnlocked.Invoke();
        OnAchievementUnlocked?.Invoke(ach);

        ShowPopup(ach);
        Save();
    }

    private void ShowPopup(Achievement ach)
    {
        if (popupPrefab == null)
        {
            Debug.LogError("[AchievementManager] Popup prefab not assigned!");
            return;
        }

        if (popupParent == null)
        {
            Debug.LogError("[AchievementManager] Popup parent not assigned!");
            return;
        }

        GameObject popup = Instantiate(popupPrefab, popupParent, false);
        popup.name = "AchievementPopup";

        AchievementPopup popupScript = popup.GetComponent<AchievementPopup>();
        if (popupScript != null)
        {
            popupScript.SetAchievement(ach);
        }
        else
        {
            Debug.LogError("[AchievementManager] AchievementPopup script not found!");
            Destroy(popup);
            return;
        }

        popup.SetActive(true);
    }

    public void Save()
    {
        foreach (var ach in allAchievements)
        {
            string id = ach.achievementID;
            PlayerPrefs.SetInt(GetKeyUnlocked(id), ach.isUnlocked ? 1 : 0);
            PlayerPrefs.SetInt(GetKeyProgress(id), ach.currentProgress);
        }
        PlayerPrefs.Save();
    }

    public void Load()
    {
        foreach (var ach in allAchievements)
        {
            string id = ach.achievementID;
            string keyUnlocked = GetKeyUnlocked(id);
            string keyProgress = GetKeyProgress(id);

            if (PlayerPrefs.HasKey(keyUnlocked))
            {
                bool wasUnlocked = PlayerPrefs.GetInt(keyUnlocked) == 1;
                int progress = PlayerPrefs.GetInt(keyProgress, 0);

                if (wasUnlocked)
                {
                    ach.isUnlocked = true;
                    ach.currentProgress = ach.targetProgress;
                }
                else
                {
                    ach.isUnlocked = false;
                    ach.currentProgress = progress;
                }
            }
        }
    }

    public void ResetAll()
    {
        foreach (var ach in allAchievements)
        {
            ach.Reset();
            PlayerPrefs.DeleteKey(GetKeyUnlocked(ach.achievementID));
            PlayerPrefs.DeleteKey(GetKeyProgress(ach.achievementID));
        }
        PlayerPrefs.Save();
    }

    public Achievement GetAchievement(string id)
    {
        achievementMap.TryGetValue(id, out Achievement ach);
        return ach;
    }

    private string GetKeyUnlocked(string id) => $"Ach_{id}_Unlocked";
    private string GetKeyProgress(string id) => $"Ach_{id}_Progress";

    private void OnDestroy() => Save();
    private void OnApplicationQuit() => Save();
}