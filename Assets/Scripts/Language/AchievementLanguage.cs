using UnityEngine;
using System.Collections.Generic;

public class AchievementLanguage : MonoBehaviour
{
    [System.Serializable]
    public class Entry
    {
        public string languageName;
        public string title;
        [TextArea] public string description;
    }

    public List<Entry> entries = new List<Entry>();

    private Achievement achievement;

    private void Start()
    {
        achievement = GetComponent<Achievement>();
        if (achievement == null)
        {
            Debug.LogWarning("AchievementLanguage must be placed on the same GameObject as an Achievement.");
            return;
        }

        if (LanguageManager.Instance == null || LanguageManager.Instance.CurrentLanguage == null)
            return;

        string current = LanguageManager.Instance.CurrentLanguage.name;
        foreach (var entry in entries)
        {
            if (entry.languageName == current)
            {
                achievement.title = entry.title;
                achievement.description = entry.description;
                break;
            }
        }
    }

    private void Reset()
    {
        SyncLanguages();
    }

    private void SyncLanguages()
    {
        var manager = FindObjectOfType<LanguageManager>();
        if (manager == null) return;

        // Remove stale
        for (int i = entries.Count - 1; i >= 0; i--)
        {
            bool exists = false;
            foreach (var lang in manager.languages)
            {
                if (lang.name == entries[i].languageName)
                {
                    exists = true;
                    break;
                }
            }
            if (!exists) entries.RemoveAt(i);
        }

        // Add missing
        foreach (var lang in manager.languages)
        {
            bool found = false;
            foreach (var entry in entries)
            {
                if (entry.languageName == lang.name)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                entries.Add(new Entry { languageName = lang.name, title = "", description = "" });
            }
        }
    }
}