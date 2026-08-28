using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuestListUI : MonoBehaviour
{
    public GameObject questUIPrefab;           // Prefab with a TMP_Text component
    public Transform contentParent;            // Usually the "Content" object of a ScrollRect

    private Dictionary<Quest, GameObject> activeUIItems = new Dictionary<Quest, GameObject>();

    // Reverse each line (for RTL languages like Persian)
    private string ReverseString(string str)
    {
        string[] lines = str.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            char[] arr = lines[i].ToCharArray();
            System.Array.Reverse(arr);
            lines[i] = new string(arr);
        }
        return string.Join("\n", lines);
    }

    private void Start()
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogError("QuestManager instance not found!");
            return;
        }

        QuestManager.Instance.OnQuestAdded += OnQuestAdded;
        QuestManager.Instance.OnQuestRemoved += OnQuestRemoved;
    }

    private void OnQuestAdded(Quest quest)
    {
        GameObject uiItem = Instantiate(questUIPrefab, contentParent);
        TMP_Text tmpText = uiItem.GetComponentInChildren<TMP_Text>();
        if (tmpText == null)
        {
            Debug.LogError("Quest UI prefab is missing a TMP_Text component!");
            return;
        }

        // Set alignment and RTL based on quest's rightToLeft flag
        if (quest.rightToLeft)
        {
            tmpText.alignment = TextAlignmentOptions.TopRight;
            tmpText.isRightToLeftText = true;
        }
        else
        {
            tmpText.alignment = TextAlignmentOptions.TopLeft;
            tmpText.isRightToLeftText = false;
        }

        UpdateQuestText(quest, tmpText);
        activeUIItems[quest] = uiItem;

        // Subscribe to progress and completion events to refresh text
        quest.onQuestProgress.AddListener(() => UpdateQuestText(quest, tmpText));
        quest.onQuestComplete.AddListener(() => UpdateQuestText(quest, tmpText));
    }

    private void OnQuestRemoved(Quest quest)
    {
        if (activeUIItems.TryGetValue(quest, out GameObject uiItem))
        {
            // Unsubscribe to prevent memory leaks
            quest.onQuestProgress.RemoveAllListeners();
            quest.onQuestComplete.RemoveAllListeners();

            Destroy(uiItem);
            activeUIItems.Remove(quest);
        }
    }

    private void UpdateQuestText(Quest quest, TMP_Text uiText)
    {
        string displayText;

        if (quest.questStatus == QuestStatus.Completed)
        {
            displayText = quest.rightToLeft
                ? ReverseString($"{quest.questText} (تکمیل شد)")   // Optional: Persian "Completed"
                : $"{quest.questText} (COMPLETE)";
        }
        else
        {
            if (quest.showNumeration)
            {
                string numeration = $"({quest.questProgress}/{quest.maxQuestProgress})";

                if (quest.rightToLeft)
                {
                    // For RTL: put numeration at the start (so it appears on the right after reversal)
                    displayText = $"{numeration} {quest.questText}";
                    displayText = ReverseString(displayText);
                }
                else
                {
                    // For LTR: standard order
                    displayText = $"{quest.questText} {numeration}";
                }
            }
            else
            {
                // No numeration, just quest text (apply reversal if RTL)
                displayText = quest.rightToLeft ? ReverseString(quest.questText) : quest.questText;
            }
        }

        uiText.text = displayText;
    }

    private void OnDestroy()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestAdded -= OnQuestAdded;
            QuestManager.Instance.OnQuestRemoved -= OnQuestRemoved;
        }
    }
}