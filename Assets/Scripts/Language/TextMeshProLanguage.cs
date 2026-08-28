using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class TextMeshProLanguage : MonoBehaviour
{
    [System.Serializable]
    public class Entry
    {
        public string languageName;
        [TextArea] public string text;
    }

    public List<Entry> entries = new List<Entry>();
    private TMP_Text tmp;

    private void Start()
    {
        tmp = GetComponent<TMP_Text>();
        if (tmp == null) return;

        if (LanguageManager.Instance == null || LanguageManager.Instance.CurrentLanguage == null)
            return;

        var currentLang = LanguageManager.Instance.CurrentLanguage;
        foreach (var entry in entries)
        {
            if (entry.languageName == currentLang.name)
            {
                tmp.text = entry.text;
                tmp.font = currentLang.font;
                break;
            }
        }
    }

    private void Reset() => SyncLanguages();

    private void SyncLanguages()
    {
        var manager = FindObjectOfType<LanguageManager>();
        if (manager == null) return;

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
                entries.Add(new Entry { languageName = lang.name, text = "" });
            }
        }
    }
}