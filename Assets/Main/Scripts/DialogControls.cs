using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    [System.Serializable]
    public class DialogueChoice
    {
        public string choiceText;
        public int jumpIndex = -1;
    }

    [System.Serializable]
    public class DialogueEvent
    {
        public string characterName;
        [TextArea(3, 5)] public string dialogueText;
        public AudioSource voiceLine;
        public GameObject activeChar;
        public float delay = 0.5f;
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

    [Header("Event Data")]
    public DialogueEvent[] events;

    [Header("Settings")]
    public float scrollSpeed = 0.05f;

    private int currentEventIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine = null;
    private bool hasFinishedTyping = false;

    private bool awaitingPlayerInput = false; 
    private int lastShownIndex = -1; 
    private bool endOnNext = false;        
    void Start()
    {
        if (events == null || events.Length == 0)
        {
            Debug.LogWarning("[DialogManager] No events assigned.");
            return;
        }

        StartDialogueAt(0);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping) SkipDialogue();
            else OnNextPressed();
        }
    }

    // --- Public API ---
    public void StartDialogueAt(int startIndex)
    {
        endOnNext = false;
        awaitingPlayerInput = false;

        if (events == null || events.Length == 0)
        {
            Debug.LogWarning("[DialogManager] No dialogue events.");
            return;
        }

        if (startIndex < 0 || startIndex >= events.Length)
        {
            Debug.LogWarning("[DialogManager] Invalid start index: " + startIndex);
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
        // reset UI state for this event
        if (nextButton != null) nextButton.SetActive(false);
        if (choicePanel != null) choicePanel.SetActive(false);
        endOnNext = false;
        awaitingPlayerInput = false;
        hasFinishedTyping = false;

        if (ev.delay > 0f)
            yield return new WaitForSeconds(ev.delay);

        foreach (var e in events)
        {
            if (e.activeChar != null)
                e.activeChar.SetActive(false);
        }
        if (ev.activeChar != null) ev.activeChar.SetActive(true);

        if (ev.voiceLine != null) ev.voiceLine.Play();

        diagBox.SetActive(true);
        diagChar.text = ev.characterName ?? "";

        typingCoroutine = StartCoroutine(TypeDialogue(ev.dialogueText ?? ""));
        yield return typingCoroutine; 

        hasFinishedTyping = true;
        awaitingPlayerInput = true;
        lastShownIndex = currentEventIndex;

        if (ev.choices != null && ev.choices.Length > 0)
        {
            ShowChoices(ev.choices);
            yield break;
        }

        if (ev.jumpIndex == -2)
        {
            endOnNext = true;
            if (nextButton != null) nextButton.SetActive(true);
            yield break;
        }

        if (nextButton != null) nextButton.SetActive(true);
    }

    // --- Typing ---
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

    // --- Choices ---
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
            Button btn = newBtn.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(() => OnChoiceSelected(target));
            else
                Debug.LogWarning("[DialogManager] choiceButtonPrefab needs a Button component.");
        }
    }

    private void OnChoiceSelected(int jumpIndex)
    {
        if (choicePanel != null) choicePanel.SetActive(false);
        awaitingPlayerInput = false;

        if (jumpIndex == -2)
        {
            endOnNext = true;
            if (nextButton != null) nextButton.SetActive(true);
            awaitingPlayerInput = true;
            lastShownIndex = currentEventIndex;
            return;
        }

        if (jumpIndex >= 0 && jumpIndex < events.Length)
        {
            currentEventIndex = jumpIndex;
        }
        else
        {
            currentEventIndex++;
        }

        DisplayNextDialogue();
    }

    // --- Next button handler (wire this method on the Next button OnClick) ---
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

    // --- End ---
    private void EndDialogue()
    {
        Debug.Log("[DialogManager] Dialogue ended.");
        if (diagBox != null) diagBox.SetActive(false);
        if (choicePanel != null) choicePanel.SetActive(false);
        if (nextButton != null) nextButton.SetActive(false);

        awaitingPlayerInput = false;
        lastShownIndex = -1;
        endOnNext = false;
    }
}
