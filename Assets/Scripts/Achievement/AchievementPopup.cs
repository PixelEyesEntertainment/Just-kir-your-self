using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class AchievementPopup : MonoBehaviour
{
    public enum PopupPosition
    {
        MiddleBottom,
        MiddleTop
    }

    [Header("UI Elements")]
    public Image iconImage;
    public TMP_Text titleText;
    public TMP_Text descriptionText;

    [Header("Animation")]
    public float showDuration = 3f;
    public float slideSpeed = 200f;

    [Header("Position Presets")]
    public PopupPosition position = PopupPosition.MiddleBottom;

    // Middle Bottom Preset
    public float bottomStartY = -70f;
    public float bottomTargetY = 0f;

    // Middle Top Preset
    public float topStartY = 610f;
    public float topTargetY = 530f;

    private RectTransform rectTransform;
    private Coroutine showCoroutine;

    public void SetAchievement(Achievement ach)
    {
        if (iconImage != null) iconImage.sprite = ach.icon;
        if (titleText != null) titleText.text = ach.GetDisplayTitle();
        if (descriptionText != null) descriptionText.text = ach.GetDisplayDescription();
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (showCoroutine != null) StopCoroutine(showCoroutine);
        showCoroutine = StartCoroutine(ShowSequence());
    }

    private IEnumerator ShowSequence()
    {
        // Get start and target Y based on preset
        float startY, targetY;
        if (position == PopupPosition.MiddleBottom)
        {
            startY = bottomStartY;
            targetY = bottomTargetY;
        }
        else // MiddleTop
        {
            startY = topStartY;
            targetY = topTargetY;
        }

        // Start position
        Vector2 startPos = new Vector2(0, startY);
        Vector2 targetPos = new Vector2(0, targetY);
        rectTransform.anchoredPosition = startPos;

        // Calculate duration based on speed
        float distance = Mathf.Abs(targetY - startY);
        float duration = distance / slideSpeed;

        // Move to target
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }
        rectTransform.anchoredPosition = targetPos;

        // Wait
        yield return new WaitForSeconds(showDuration);

        // Move back to start
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            rectTransform.anchoredPosition = Vector2.Lerp(targetPos, startPos, t);
            yield return null;
        }
        rectTransform.anchoredPosition = startPos;

        // Destroy
        showCoroutine = null;
        Destroy(gameObject);
    }
}