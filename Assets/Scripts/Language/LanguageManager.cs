using UnityEngine;
using TMPro;
using System;

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance { get; private set; }

    [Serializable]
    public class Language
    {
        public string name;
        public TMP_FontAsset font;
    }

    public Language[] languages;

    [Header("Current Language (read-only)")]
    public string currentLanguageName;

    public Language CurrentLanguage { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (languages == null || languages.Length == 0)
        {
            Debug.LogError("LanguageManager: No languages defined.");
            return;
        }

        string saved = PlayerPrefs.GetString("language", languages[0].name);
        SetLanguage(saved);
    }

    private void Start()
    {
        // Apply fonts to the DialogueManager's text fields once at start
        ApplyDialogueFonts();
    }

    public void SetLanguage(string languageName)
    {
        foreach (var lang in languages)
        {
            if (lang.name == languageName)
            {
                CurrentLanguage = lang;
                currentLanguageName = lang.name;
                PlayerPrefs.SetString("language", languageName);
                return;
            }
        }
        // Fallback
        CurrentLanguage = languages[0];
        currentLanguageName = languages[0].name;
        PlayerPrefs.SetString("language", languages[0].name);
        Debug.LogWarning($"Language '{languageName}' not found. Falling back to '{languages[0].name}'.");
    }

    private void ApplyDialogueFonts()
    {
        if (CurrentLanguage == null) return;

        var dialogueManager = FindObjectOfType<DialogueManager>();
        if (dialogueManager == null) return;

        // Apply to the main dialogue text
        if (dialogueManager.dialogueText != null)
            dialogueManager.dialogueText.font = CurrentLanguage.font;

        // Apply to all choice texts
        if (dialogueManager.choiceText != null)
        {
            foreach (var choice in dialogueManager.choiceText)
                if (choice != null) choice.font = CurrentLanguage.font;
        }
    }
}