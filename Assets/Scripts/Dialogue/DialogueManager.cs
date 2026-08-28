using UnityEngine;
using TMPro;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI[] choiceText;
    public Image speakerImage;

    [Header("Skip Settings")]
    public KeyCode[] skipKeys = new KeyCode[] { KeyCode.Space, KeyCode.Return, KeyCode.Mouse0 };
    public bool allowAnyKeyToSkip = false;

    [Header("Dialogue Events")]
    public UnityEvent onDialogueStart;
    public UnityEvent onDialogueFinish;

    private Dialogue currentDialogue;
    private bool isDialogueActive = false;
    private bool isTyping = false;
    private bool canSelectChoice = false;
    private Coroutine typingCoroutine = null;
    private string processedSentence = "";

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

    public void StartDialogue(Dialogue dialogue)
    {
        if (isDialogueActive)
        {
            StartCoroutine(WaitAndStartNewDialogue(dialogue));
            return;
        }

        onDialogueStart?.Invoke();

        currentDialogue = dialogue;
        dialogueText.gameObject.SetActive(true);

        // 👇 Play voice sound if selected
        if (currentDialogue.audioMode == Dialogue.AudioMode.VoiceSound && currentDialogue.voiceSound != null)
            currentDialogue.voiceSound.Play();

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        if (currentDialogue.textDirection == Dialogue.TextFlowDirection.RightToLeft)
        {
            processedSentence = ReverseString(currentDialogue.sentence);
            dialogueText.alignment = TextAlignmentOptions.TopRight;
            dialogueText.isRightToLeftText = true;
        }
        else
        {
            processedSentence = currentDialogue.sentence;
            dialogueText.alignment = TextAlignmentOptions.TopLeft;
            dialogueText.isRightToLeftText = false;
        }

        typingCoroutine = StartCoroutine(DelayedTypeStart(
            currentDialogue.delayBeforeText,
            processedSentence,
            currentDialogue.typeSpeed,
            currentDialogue.typingSound,
            currentDialogue.speakerSprite,
            currentDialogue.imageDelay
        ));

        isDialogueActive = true;
        canSelectChoice = false;
    }

    private IEnumerator WaitAndStartNewDialogue(Dialogue newDialogue)
    {
        EndDialogue();
        yield return new WaitForSeconds(0.5f);
        StartDialogue(newDialogue);
    }

    private IEnumerator DelayedTypeStart(float delay, string sentence, float typeSpeed, AudioSource typingSound, Sprite speakerSprite, float imageDelay)
    {
        isTyping = true;
        dialogueText.text = "";

        yield return new WaitForSeconds(delay);

        if (speakerImage != null && speakerSprite != null)
        {
            yield return new WaitForSeconds(imageDelay);
            speakerImage.sprite = speakerSprite;
            speakerImage.gameObject.SetActive(true);
        }

        for (int i = 0; i < sentence.Length; i++)
        {
            dialogueText.text = sentence.Substring(0, i + 1);

            // 👇 Only play typing sound if the mode is TypingSound
            if (currentDialogue != null &&
                currentDialogue.audioMode == Dialogue.AudioMode.TypingSound &&
                typingSound != null)
            {
                typingSound.Play();
            }

            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
        ShowChoices();
        canSelectChoice = true;
    }

    private void ShowChoices()
    {
        for (int i = 0; i < choiceText.Length; i++)
        {
            if (i < currentDialogue.choices.Length)
            {
                choiceText[i].gameObject.SetActive(true);
                choiceText[i].text = currentDialogue.choices[i];
            }
            else
            {
                choiceText[i].gameObject.SetActive(false);
            }
        }
    }

    void Update()
    {
        if (isTyping)
        {
            if (ShouldSkipDialogue())
            {
                StopCoroutine(typingCoroutine);
                dialogueText.text = processedSentence;
                isTyping = false;

                if (speakerImage != null && currentDialogue.speakerSprite != null)
                {
                    speakerImage.sprite = currentDialogue.speakerSprite;
                    speakerImage.gameObject.SetActive(true);
                }

                ShowChoices();
                canSelectChoice = true;
            }
            return;
        }

        if (!isDialogueActive || !canSelectChoice)
            return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            SelectChoice(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2) && currentDialogue.choices.Length >= 2)
            SelectChoice(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3) && currentDialogue.choices.Length >= 3)
            SelectChoice(2);
    }

    private bool ShouldSkipDialogue()
    {
        if (allowAnyKeyToSkip && (Input.anyKeyDown || Input.GetKeyDown(KeyCode.Space)))
            return true;

        foreach (KeyCode key in skipKeys)
        {
            if (Input.GetKeyDown(key))
                return true;
        }

        return false;
    }

    public void OnChoiceClicked(int index)
    {
        SelectChoice(index);
    }

    private void SelectChoice(int choiceIndex)
    {
        if (!canSelectChoice)
            return;

        if (choiceIndex < currentDialogue.choices.Length)
        {
            canSelectChoice = false;
            currentDialogue.actions[choiceIndex].Invoke();
        }

        EndDialogue();
    }

    private void EndDialogue()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        isDialogueActive = false;
        canSelectChoice = false;
        isTyping = false;
        dialogueText.gameObject.SetActive(false);

        if (speakerImage != null)
            speakerImage.gameObject.SetActive(false);

        foreach (var choice in choiceText)
            choice.gameObject.SetActive(false);

        onDialogueFinish?.Invoke();
    }
}