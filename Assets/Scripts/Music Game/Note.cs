using UnityEngine;
using UnityEngine.UI;

public class Note : MonoBehaviour
{
    public enum State
    {
        Waiting,
        Holding,
        Done
    }

    public float hitTime;
    public int lane;
    public int soundId;
    public float holdDuration;
    public State state = State.Waiting;
    public float holdStartTime;

    public bool CanHit = true;
    public bool IsPressed = false;
    public bool WasMissed = false;

    [Header("Fill Child")]
    public RectTransform fillRect;

    private RectTransform rectTransform;
    private Image image;
    private float spawnY;
    private float targetY;
    private float speed;
    private float spawnTime;
    private float fullHeight;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        CanHit = true;
        IsPressed = false;
        WasMissed = false;
    }

    public void Initialize(float hitTime, int lane, int soundId, float spawnY, float targetY, float speed, float spawnTime, float holdDuration)
    {
        this.hitTime = hitTime;
        this.lane = lane;
        this.soundId = soundId;
        this.spawnY = spawnY;
        this.targetY = targetY;
        this.speed = speed;
        this.spawnTime = spawnTime;
        this.holdDuration = holdDuration;
        state = State.Waiting;
        CanHit = true;
        IsPressed = false;
        WasMissed = false;

        Vector2 size = rectTransform.sizeDelta;

        if (holdDuration > 0)
        {
            fullHeight = holdDuration * speed;
            float maxHeight = (spawnY - targetY) * 2f;
            fullHeight = Mathf.Min(fullHeight, maxHeight);
            rectTransform.sizeDelta = new Vector2(size.x, fullHeight);
            if (image != null)
                image.color = new Color(0.4f, 0.6f, 1f, 1f);

            if (fillRect != null)
            {
                fillRect.sizeDelta = new Vector2(size.x, 0);
                Image fillImg = fillRect.GetComponent<Image>();
                if (fillImg != null)
                    fillImg.color = new Color(0.8f, 0.8f, 1f, 0.8f);
                fillRect.gameObject.SetActive(true);
            }
        }
        else
        {
            rectTransform.sizeDelta = new Vector2(size.x, 40f);
            if (image != null)
                image.color = Color.white;
            if (fillRect != null)
                fillRect.gameObject.SetActive(false);
        }
    }

    public void UpdatePosition(float currentTime)
    {
        if (state == State.Done) return;

        float y = spawnY - (currentTime - spawnTime) * speed;
        rectTransform.anchoredPosition = new Vector2(0, y);

        if (state == State.Holding && holdDuration > 0 && fillRect != null)
        {
            float progress = (currentTime - holdStartTime) / holdDuration;
            progress = Mathf.Clamp01(progress);
            float fillHeight = progress * fullHeight;
            fillRect.sizeDelta = new Vector2(fillRect.sizeDelta.x, fillHeight);
        }
    }

    public float GetYAtTime(float time)
    {
        return spawnY - (time - spawnTime) * speed;
    }

    public void StartHold(float currentTime)
    {
        state = State.Holding;
        holdStartTime = currentTime;
    }

    public void CompleteHold()
    {
        state = State.Done;
    }

    public float GetCurrentY() => rectTransform.anchoredPosition.y;
}