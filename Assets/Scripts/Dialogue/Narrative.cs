using UnityEngine;
using TMPro;
using System.Collections;

public class Narrative : MonoBehaviour
{
    [Header("⚠️ Help ⚠️")]
    [Header("Helps you show a message as text to player.")]
    [Header("You just need to edit fadeDuration, but if autoHide is checked adjust hideDelay too.")]
    [Header("autoHide is used when u dont want to call HideMessage().")]
    [Header("ShowMessage() and HideMessage() to call the message and hide.")]
    [Header("You can also use SetText() to change the message content before showing.")]
    [Header("------------------------------------------------------------------------")]

    public TMP_Text text;   // accepts both UI and 3D
    public string textString;
    public bool autoHide = false;
    public float hideDelay = 2f;
    public float fadeDuration = 0.5f;

    private Coroutine fadeRoutine;

    private void Awake()
    {
        // Hide the text at start
        if (text != null)
        {
            Color c = text.color;
            c.a = 0f;
            text.color = c;
        }
    }

    public void SetText(string newText)
    {
        textString = newText;
    }

    public void ShowMessage()
    {
        text.text = textString;
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeText(1f));

        if (autoHide)
        {
            StartCoroutine(AutoHideAfterDelay());
        }
    }

    public void HideMessage()
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeText(0f));
    }

    private IEnumerator FadeText(float targetAlpha)
    {
        float startAlpha = text.color.a;
        float t = 0;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, t / fadeDuration);
            text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
            yield return null;
        }

        text.color = new Color(text.color.r, text.color.g, text.color.b, targetAlpha);
    }

    private IEnumerator AutoHideAfterDelay()
    {
        yield return new WaitForSeconds(hideDelay);
        HideMessage();
    }
}