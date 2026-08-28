using UnityEngine;
using System.Collections.Generic;

public class DialogueLanguage : MonoBehaviour
{
    [System.Serializable]
    public class Entry
    {
        public string languageName;
        [TextArea] public string sentence;
        public string[] choices;
        public Dialogue.TextFlowDirection direction;
        public AudioClip voiceClip;
    }

    public List<Entry> entries = new List<Entry>();
    private Dialogue dialogue;

    private void Start()
    {
        dialogue = GetComponent<Dialogue>();
        if (dialogue == null) return;

        if (LanguageManager.Instance == null || LanguageManager.Instance.CurrentLanguage == null)
            return;

        string current = LanguageManager.Instance.CurrentLanguage.name;
        foreach (var entry in entries)
        {
            if (entry.languageName == current)
            {
                dialogue.sentence = entry.sentence;
                dialogue.choices = entry.choices;
                dialogue.textDirection = entry.direction;

                if (dialogue.voiceSound != null)
                    dialogue.voiceSound.clip = entry.voiceClip;

                break;
            }
        }
    }

    private void Reset() => SyncLanguages();

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
                entries.Add(new Entry
                {
                    languageName = lang.name,
                    sentence = "",
                    choices = new string[0],
                    direction = Dialogue.TextFlowDirection.LeftToRight,
                    voiceClip = null
                });
            }
        }
    }
}