using UnityEngine;
using UnityEngine.Events;

public class Dialogue : MonoBehaviour
{
    public enum TextFlowDirection
    {
        LeftToRight,
        RightToLeft
    }

    public enum AudioMode
    {
        TypingSound,  
        VoiceSound
    }

    [Header("Text Direction")]
    public TextFlowDirection textDirection = TextFlowDirection.LeftToRight;

    [Header("Timing")]
    public float delayBeforeText = 0f;
    public float imageDelay = 0f;

    [Header("Typing Settings")]
    public float typeSpeed = 0.05f;

    [Header("Audio")]
    public AudioMode audioMode = AudioMode.TypingSound;
    public AudioSource typingSound;  
    public AudioSource voiceSound;  

    [Header("Speaker")]
    public Sprite speakerSprite;

    [Header("Dialogue Content")]
    [TextArea(0, 10000)]
    public string sentence;

    [Header("Choices")]
    public string[] choices;

    [Header("Choice Actions")]
    public UnityEvent[] actions;
}