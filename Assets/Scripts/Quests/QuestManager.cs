using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;
    public List<Quest> activeQuests = new List<Quest>();

    public event System.Action<Quest> OnQuestAdded;
    public event System.Action<Quest> OnQuestRemoved;

    private void Awake() => Instance = this;

    public void AddQuest(Quest quest)
    {
        if (!activeQuests.Contains(quest))
        {
            activeQuests.Add(quest);
            OnQuestAdded?.Invoke(quest);
        }
    }

    public void RemoveQuest(Quest quest)
    {
        if (activeQuests.Remove(quest))
            OnQuestRemoved?.Invoke(quest);
    }
}