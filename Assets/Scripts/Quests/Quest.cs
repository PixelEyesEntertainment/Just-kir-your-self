using UnityEngine;
using UnityEngine.Events;

public enum QuestStatus
{
    Completed,
    Started,
    Available,
    Unavailable
}

public class Quest : MonoBehaviour
{
    [TextArea(0, 10000)]
    public string questText;

    public int maxQuestProgress;
    public int questProgress;
    public QuestStatus questStatus = QuestStatus.Available;
    public UnityEvent onQuestComplete, onQuestStart, onQuestProgress;

    [Header("UI Options")]
    public bool showNumeration = true;   // Show (progress/max) next to text
    public bool rightToLeft = false;     // Persian/Arabic mode

    public void MakeAvailable()
    {
        if (questStatus == QuestStatus.Unavailable)
            questStatus = QuestStatus.Available;
    }

    public void MakeUnAvailable()
    {
        if (questStatus == QuestStatus.Unavailable)
            return;
        questStatus = QuestStatus.Unavailable;
    }

    public void QuestComplete()
    {
        if (questStatus == QuestStatus.Unavailable || questStatus == QuestStatus.Completed)
            return;

        questStatus = QuestStatus.Completed;
        QuestManager.Instance.RemoveQuest(this);
        onQuestComplete.Invoke();
    }

    public void QuestProgress()
    {
        if (questStatus != QuestStatus.Started)
            return;
        if (questProgress >= maxQuestProgress)
            return;

        questProgress++;
        onQuestProgress.Invoke();

        if (questProgress >= maxQuestProgress)
            QuestComplete();
    }

    public void QuestStart()
    {
        if (questStatus != QuestStatus.Available && questStatus != QuestStatus.Unavailable)
            return;

        questStatus = QuestStatus.Started;
        onQuestStart.Invoke();

        QuestManager.Instance.AddQuest(this);
    }
}