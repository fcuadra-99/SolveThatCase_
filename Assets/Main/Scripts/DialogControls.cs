using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class DialogManager : MonoBehaviour
{
    [Header("Meter Integration")]
    public MonoBehaviour meterController;

    [System.Serializable]
    public class DialogueChoice
    {
        public string choiceText;
        public int jumpIndex = -1;
        public float meterAdjustment = 0f;
    }

    [System.Serializable]
    public class DialogueEvent
    {
        public string characterName;
        [TextArea(3, 5)] public string dialogueText;
        public AudioSource voiceLine;

        public AudioClip typingSFX;
        public GameObject activeChar;
        public float delay = 0f;
        public int jumpIndex = -1;
        public DialogueChoice[] choices;
    }

    [Header("UI Elements")]
    public GameObject diagBox;
    public TMP_Text diagChar;
    public TMP_Text diagText;
    public GameObject nextButton;

    [Header("Choices UI")]
    public GameObject choicePanel;
    public GameObject choiceButtonPrefab;

    [Header("Audio Setup")]
    public AudioSource typingAudioSource;
    public AudioClip defaultTypingSFX;

    [Header("Event Data")]
    public DialogueEvent[] events;

    [Header("Settings")]
    public float scrollSpeed = 0.05f;

    [Header("Scripts")]
    public Logzzza dialogueLog;
    public SpotlightControl spotlightControl;

    // Internal state
    private int currentEventIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine = null;
    private bool hasFinishedTyping = false;
    public bool running = false;

    private bool awaitingPlayerInput = false;
    private int lastShownIndex = -1;
    private bool endOnNext = false;

    // Tracks which characters have been enabled during dialogue
    private List<GameObject> trackedActiveChars = new List<GameObject>();

    void Start()
    {
        if (events == null || events.Length == 0)
        {
            Debug.LogWarning("[DialogManager] No events assigned.");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping) SkipDialogue();
            else OnNextPressed();
        }
    }

    public void StartDialogueAt(int startIndex)
    {
        Debug.Log($"[DialogManager] Starting dialogue at index {startIndex}.");
        endOnNext = false;
        awaitingPlayerInput = false;

        if (events == null || events.Length == 0)
        {
            Debug.LogWarning("[DialogManager] No dialogue events.");
            return;
        }

        if (startIndex < 0 || startIndex >= events.Length)
        {
            Debug.LogWarning($"[DialogManager] Invalid start index: {startIndex}");
            return;
        }

        currentEventIndex = startIndex;
        diagBox.SetActive(true);
        choicePanel.SetActive(false);
        if (nextButton != null) nextButton.SetActive(false);

        DisplayNextDialogue();
    }

    public void DisplayNextDialogue()
    {
        if (isTyping) return;

        if (awaitingPlayerInput && lastShownIndex == currentEventIndex)
        {
            OnNextPressed();
            return;
        }

        if (currentEventIndex < 0 || currentEventIndex >= events.Length)
        {
            EndDialogue();
            return;
        }

        StartCoroutine(RunDialogueEvent(events[currentEventIndex]));
    }

    private IEnumerator RunDialogueEvent(DialogueEvent ev)
    {
        spotlightControl.MoveUp();
        running = true;
        if (nextButton != null) nextButton.SetActive(false);
        if (choicePanel != null) choicePanel.SetActive(false);
        endOnNext = false;
        awaitingPlayerInput = false;
        hasFinishedTyping = false;

        if (ev.delay > 0f)
            yield return new WaitForSeconds(ev.delay);

        // Disable all active characters first
        foreach (var e in events)
        {
            if (e.activeChar != null)
                e.activeChar.SetActive(false);
        }

        // Activate the current one and track it
        if (ev.activeChar != null)
        {
            ev.activeChar.SetActive(true);
            if (!trackedActiveChars.Contains(ev.activeChar))
                trackedActiveChars.Add(ev.activeChar);
            Debug.Log($"[DialogManager] Activated character: {ev.activeChar.name}");
        }

        // Play voiceline
        if (ev.voiceLine != null)
            ev.voiceLine.Play();

        // Set typing SFX
        if (typingAudioSource != null)
        {
            typingAudioSource.clip = ev.typingSFX ?? defaultTypingSFX;
        }

        diagBox.SetActive(true);
        diagChar.text = ev.characterName ?? "";

        // Log dialogue
        if (dialogueLog != null)
            dialogueLog.LogDialogue(ev.characterName, ev.dialogueText);

        typingCoroutine = StartCoroutine(TypeDialogue(ev.dialogueText ?? ""));
        yield return typingCoroutine;

        hasFinishedTyping = true;
        awaitingPlayerInput = true;
        lastShownIndex = currentEventIndex;

        // Show choices if any
        if (ev.choices != null && ev.choices.Length > 0)
        {
            ShowChoices(ev.choices);
            yield break;
        }

        // End trigger
        if (ev.jumpIndex == -2)
        {
            endOnNext = true;
            if (nextButton != null) nextButton.SetActive(true);
            yield break;
        }

        if (nextButton != null) nextButton.SetActive(true);
    }

    private IEnumerator TypeDialogue(string text)
    {
        isTyping = true;
        hasFinishedTyping = false;
        diagText.text = "";

        if (string.IsNullOrEmpty(text))
        {
            yield return null;
        }
        else
        {
            foreach (char c in text)
            {
                diagText.text += c;

                if (typingAudioSource != null && typingAudioSource.clip != null)
                    typingAudioSource.Play();

                yield return new WaitForSeconds(scrollSpeed);
            }
        }

        isTyping = false;
        hasFinishedTyping = true;
    }

    public void SkipDialogue()
    {
        if (currentEventIndex < 0 || currentEventIndex >= events.Length) return;

        DialogueEvent ev = events[currentEventIndex];

        if (isTyping && typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
            diagText.text = ev.dialogueText ?? "";
            isTyping = false;
            hasFinishedTyping = true;
        }

        if (ev.choices != null && ev.choices.Length > 0)
        {
            ShowChoices(ev.choices);
        }
        else
        {
            if (nextButton != null) nextButton.SetActive(true);
            awaitingPlayerInput = true;
            lastShownIndex = currentEventIndex;
        }
    }

    private void ShowChoices(DialogueChoice[] choices)
    {
        awaitingPlayerInput = true;
        if (nextButton != null) nextButton.SetActive(false);
        if (choicePanel != null) choicePanel.SetActive(true);

        foreach (Transform t in choicePanel.transform)
            Destroy(t.gameObject);

        foreach (var choice in choices)
        {
            GameObject newBtn = Instantiate(choiceButtonPrefab, choicePanel.transform);
            TMP_Text t = newBtn.GetComponentInChildren<TMP_Text>();
            if (t != null) t.text = choice.choiceText;

            int target = choice.jumpIndex;
            float adjustment = choice.meterAdjustment;

            Button btn = newBtn.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(() => OnChoiceSelected(target, adjustment));
            else
                Debug.LogWarning("[DialogManager] choiceButtonPrefab needs a Button component.");
        }
    }

    private void OnChoiceSelected(int jumpIndex, float meterAdjustment)
    {
        if (choicePanel != null) choicePanel.SetActive(false);
        awaitingPlayerInput = false;

        if (meterController != null)
        {
            meterController.SendMessage("AdjustMeter", meterAdjustment, SendMessageOptions.DontRequireReceiver);
            Debug.Log($"Meter adjusted by: {meterAdjustment}");
        }

        if (jumpIndex == -2)
        {
            endOnNext = true;
            if (nextButton != null) nextButton.SetActive(true);
            awaitingPlayerInput = true;
            lastShownIndex = currentEventIndex;
            return;
        }

        if (jumpIndex >= 0 && jumpIndex < events.Length)
            currentEventIndex = jumpIndex;
        else
            currentEventIndex++;

        DisplayNextDialogue();
    }

    public void OnNextPressed()
    {
        if (isTyping)
        {
            SkipDialogue();
            return;
        }

        if (choicePanel != null && choicePanel.activeInHierarchy) return;

        if (endOnNext)
        {
            EndDialogue();
            return;
        }

        if (currentEventIndex < 0 || currentEventIndex >= events.Length)
        {
            EndDialogue();
            return;
        }

        DialogueEvent ev = events[currentEventIndex];

        if (ev.jumpIndex >= 0 && ev.jumpIndex < events.Length)
            currentEventIndex = ev.jumpIndex;
        else
            currentEventIndex++;

        awaitingPlayerInput = false;
        lastShownIndex = -1;

        if (currentEventIndex < 0 || currentEventIndex >= events.Length)
        {
            EndDialogue();
            return;
        }

        DisplayNextDialogue();
    }

    public event System.Action onDiagEnd;

    private void EndDialogue()
    {
        Debug.Log("[DialogManager] EndDialogue called.");

        if (diagBox != null) diagBox.SetActive(false);
        if (choicePanel != null) choicePanel.SetActive(false);
        if (nextButton != null) nextButton.SetActive(false);
        if (diagText != null) diagText.text = "";
        if (diagChar != null) diagChar.text = "";

        // Disable all tracked characters
        for (int i = trackedActiveChars.Count - 1; i >= 0; i--)
        {
            var ch = trackedActiveChars[i];
            if (ch != null && ch.activeSelf)
            {
                ch.SetActive(false);
                Debug.Log($"[DialogManager] Disabled tracked character: {ch.name}");
            }
        }
        trackedActiveChars.Clear();

        // Fallback: disable all event characters too
        foreach (var ev in events)
        {
            if (ev.activeChar != null && ev.activeChar.activeSelf)
            {
                ev.activeChar.SetActive(false);
                Debug.Log($"[DialogManager] Disabled leftover event character: {ev.activeChar.name}");
            }
        }

        // Stop typing coroutine if still running
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
            isTyping = false;
        }

        awaitingPlayerInput = false;
        lastShownIndex = -1;
        endOnNext = false;
        running = false;

        if (spotlightControl != null)
            spotlightControl.MoveDown();
        else
            Debug.LogWarning("[DialogManager] spotlightControl is null in EndDialogue.");

        Debug.Log("[DialogManager] Invoking onDiagEnd listeners.");
        onDiagEnd?.Invoke();
    }
}
