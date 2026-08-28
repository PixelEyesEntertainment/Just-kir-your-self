using UnityEngine;

public class AchievementHelper : MonoBehaviour
{
    /// <summary>
    /// Adds progress to an achievement.
    /// Usage: "AchievementID,Amount" (e.g., "Kill10,1")
    /// </summary>
    public void AddProgress(string idAndAmount)
    {
        if (string.IsNullOrEmpty(idAndAmount))
        {
            Debug.LogWarning("AddProgress: Input is empty!");
            return;
        }

        string[] parts = idAndAmount.Split(',');
        if (parts.Length != 2)
        {
            Debug.LogWarning($"AddProgress: Invalid format '{idAndAmount}'. Use 'ID,Amount' (e.g., 'Kill10,1')");
            return;
        }

        string id = parts[0].Trim();
        if (!int.TryParse(parts[1].Trim(), out int amount))
        {
            Debug.LogWarning($"AddProgress: Could not parse amount from '{parts[1]}'");
            return;
        }

        Achievement ach = AchievementManager.Instance.GetAchievement(id);
        if (ach != null) ach.AddProgress(amount);
        else Debug.LogWarning($"Achievement '{id}' not found!");
    }

    /// <summary>
    /// Unlocks an achievement immediately.
    /// Usage: "AchievementID"
    /// </summary>
    public void Unlock(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("Unlock: ID is empty!");
            return;
        }

        Achievement ach = AchievementManager.Instance.GetAchievement(id.Trim());
        if (ach != null) ach.Unlock();
        else Debug.LogWarning($"Achievement '{id}' not found!");
    }
}