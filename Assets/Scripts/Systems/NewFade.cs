using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

public class FadeSystem : MonoBehaviour
{
    [Header("⚠️ HELP ⚠️")]
    [Header("FadeIn()  – fades to the target color.")]
    [Header("FadeOut() – fades back to clear.")]
    [Header("Set autoFadeOut to true and a delay to auto-fade out after FadeIn().")]
    [Header("Events: onFadeStart, onFadeEnd")]
    [Header("------------------------------------------------------------------------")]

    [Header("Fade Settings")]
    public Color fadeColor = Color.black;
    public float fadeInDuration = 1f;
    public float fadeOutDuration = 1f;

    [Header("Auto Fade Out")]
    public bool autoFadeOut = false;
    public float fadeOutDelay = 1f;

    [Header("Events")]
    public UnityEvent onFadeStart;
    public UnityEvent onFadeEnd;

    private Canvas canvas;
    private Image image;
    private Coroutine fadeRoutine;
    private Coroutine autoFadeRoutine;

    // === Public Methods ===

    public void FadeIn()
    {
        if (autoFadeRoutine != null)
        {
            StopCoroutine(autoFadeRoutine);
            autoFadeRoutine = null;
        }

        StartFade(0f, 1f, fadeInDuration);

        if (autoFadeOut)
        {
            autoFadeRoutine = StartCoroutine(AutoFadeOut());
        }
    }

    public void FadeOut()
    {
        if (autoFadeRoutine != null)
        {
            StopCoroutine(autoFadeRoutine);
            autoFadeRoutine = null;
        }

        StartFade(1f, 0f, fadeOutDuration);
    }

    public void SetColor(Color color)
    {
        fadeColor = color;
        if (image != null)
        {
            Color c = image.color;
            c.r = color.r;
            c.g = color.g;
            c.b = color.b;
            image.color = c;
        }
    }

    public void SetFadeInDuration(float duration)
    {
        fadeInDuration = duration;
    }

    public void SetFadeOutDuration(float duration)
    {
        fadeOutDuration = duration;
    }

    // === Internal ===

    private IEnumerator AutoFadeOut()
    {
        yield return new WaitForSeconds(fadeInDuration + fadeOutDelay);
        FadeOut();
        autoFadeRoutine = null;
    }

    private void StartFade(float startAlpha, float targetAlpha, float duration)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        EnsureCanvasExists();

        Color c = image.color;
        c.a = startAlpha;
        image.color = c;

        onFadeStart.Invoke();
        fadeRoutine = StartCoroutine(FadeCoroutine(targetAlpha, duration));
    }

    private void EnsureCanvasExists()
    {
        if (canvas != null) return;

        GameObject canvasGO = new GameObject("FadeCanvas");
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasGO.AddComponent<GraphicRaycaster>();

        GameObject imageGO = new GameObject("FadeImage");
        imageGO.transform.SetParent(canvasGO.transform, false);

        RectTransform rect = imageGO.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        image = imageGO.AddComponent<Image>();
        image.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        image.raycastTarget = false;
    }

    private IEnumerator FadeCoroutine(float targetAlpha, float duration)
    {
        float elapsed = 0f;
        float startAlpha = image.color.a;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            Color c = image.color;
            c.a = alpha;
            image.color = c;
            yield return null;
        }

        Color finalColor = image.color;
        finalColor.a = targetAlpha;
        image.color = finalColor;

        onFadeEnd.Invoke();

        if (targetAlpha == 0f)
            DestroyCanvas();

        fadeRoutine = null;
    }

    private void DestroyCanvas()
    {
        if (canvas != null)
        {
            Destroy(canvas.gameObject);
            canvas = null;
            image = null;
        }
    }

    private void OnDestroy()
    {
        if (canvas != null)
            Destroy(canvas.gameObject);
    }
}