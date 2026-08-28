using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AchievementListUI : MonoBehaviour
{
    [Header("UI Prefab & Parent")]
    public GameObject achievementUIPrefab;
    public Transform contentParent;

    [Header("Child Names")]
    public string titleChildName = "Title";
    public string descChildName = "Description";
    public string sliderChildName = "ProgressSlider";
    public string iconChildName = "Icon";

    [Header("Icons")]
    public Sprite hiddenIcon;

    private Dictionary<Achievement, GameObject> uiItems = new Dictionary<Achievement, GameObject>();

    private void Start()
    {
        if (AchievementManager.Instance == null)
        {
            Debug.LogError("AchievementManager instance not found!");
            return;
        }

        AchievementManager.Instance.OnAchievementUnlocked += OnAchievementUnlocked;
        BuildUI();
    }

    private void BuildUI()
    {
        var all = AchievementManager.Instance.AllAchievements;
        if (all == null || all.Count == 0)
        {
            Debug.LogWarning("No achievements registered.");
            return;
        }

        foreach (var ach in all)
        {
            CreateUIItem(ach);
        }
    }

    private void CreateUIItem(Achievement ach)
    {
        if (ach == null || uiItems.ContainsKey(ach)) return;

        GameObject uiItem = Instantiate(achievementUIPrefab, contentParent);
        uiItems[ach] = uiItem;

        // Find children (direct children only)
        TMP_Text title = FindTMP(uiItem.transform, titleChildName);
        TMP_Text desc = FindTMP(uiItem.transform, descChildName);
        Slider slider = FindSlider(uiItem.transform, sliderChildName);
        Image icon = FindImage(uiItem.transform, iconChildName);

        UpdateUI(ach, title, desc, slider, icon);

        Achievement localAch = ach;
        ach.onUnlocked.AddListener(() => UpdateUI(localAch, title, desc, slider, icon));
    }

    private void OnAchievementUnlocked(Achievement ach)
    {
        if (uiItems.TryGetValue(ach, out GameObject uiItem))
        {
            TMP_Text title = FindTMP(uiItem.transform, titleChildName);
            TMP_Text desc = FindTMP(uiItem.transform, descChildName);
            Slider slider = FindSlider(uiItem.transform, sliderChildName);
            Image icon = FindImage(uiItem.transform, iconChildName);
            UpdateUI(ach, title, desc, slider, icon);
        }
    }

    private void UpdateUI(Achievement ach, TMP_Text title, TMP_Text desc, Slider slider, Image icon)
    {
        if (title != null) title.text = ach.GetDisplayTitle();
        if (desc != null) desc.text = ach.GetDisplayDescription();
        if (slider != null) slider.value = ach.GetProgressPercent();

        if (icon != null)
        {
            if (ach.isHidden && !ach.isUnlocked && hiddenIcon != null)
                icon.sprite = hiddenIcon;
            else
                icon.sprite = ach.icon;
        }
    }

    // Simple direct child finders (looks only at first level)
    private TMP_Text FindTMP(Transform parent, string childName)
    {
        Transform child = parent.Find(childName);
        if (child != null) return child.GetComponent<TMP_Text>();
        return null;
    }

    private Slider FindSlider(Transform parent, string childName)
    {
        Transform child = parent.Find(childName);
        if (child != null) return child.GetComponent<Slider>();
        return null;
    }

    private Image FindImage(Transform parent, string childName)
    {
        Transform child = parent.Find(childName);
        if (child != null) return child.GetComponent<Image>();
        return null;
    }

    private void OnDestroy()
    {
        if (AchievementManager.Instance != null)
            AchievementManager.Instance.OnAchievementUnlocked -= OnAchievementUnlocked;

        foreach (var pair in uiItems)
        {
            if (pair.Key != null)
                pair.Key.onUnlocked.RemoveAllListeners();
        }
        uiItems.Clear();
    }
}