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
        public int jumpId; // event to jump to when selected
    }

    [System.Serializable]
    public class DialogueEvent
    {
        [HideInInspector]
        public string eventId;
        public string characterName;
        [TextArea(3, 5)]
        public string dialogueText;
        public AudioSource voiceLine;
        public GameObject activeChar;
        public float delay = 0.5f;
        public int jumpId = -1;
        public DialogueChoice[] choices; // choices for this event
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
    public float scrollSpeed = 0.05f; // adjustable typing speed

    private int currentEventIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    private bool hasFinishedTyping = false;

    private void OnValidate()
    {
        for (int i = 0; i < events.Length; i++)
        {
            events[i].eventId = "Event" + i.ToString();
        }
    }

    void Start()
    {
        StartCoroutine(StartDialogueSequence());
    }

    void Update()
    {
        if (isTyping && Input.GetKeyDown(KeyCode.Space))
        {
            SkipDialogue();
        }
    }

    private IEnumerator StartDialogueSequence()
    {
        yield return new WaitForSeconds(2);
        DisplayNextDialogue();
    }

    public void DisplayNextDialogue()
    {
        if (isTyping)
            return;

        if (currentEventIndex < events.Length)
        {
            StartCoroutine(RunDialogueEvent(events[currentEventIndex]));
        }
        else
        {
            Debug.Log("End of dialogue.");
            diagBox.SetActive(false);
            choicePanel.SetActive(false);
            nextButton.SetActive(false);
        }
    }

    private IEnumerator RunDialogueEvent(DialogueEvent currentEvent)
    {
        nextButton.SetActive(false);
        choicePanel.SetActive(false);

        if (currentEvent.delay > 0)
            yield return new WaitForSeconds(currentEvent.delay);

        if (currentEvent.activeChar != null)
            currentEvent.activeChar.SetActive(true);

        if (currentEvent.voiceLine != null)
            currentEvent.voiceLine.Play();

        if (!string.IsNullOrEmpty(currentEvent.dialogueText))
        {
            diagBox.SetActive(true);
            diagChar.text = currentEvent.characterName;

            typingCoroutine = StartCoroutine(TypeDialogue(currentEvent.dialogueText));
            yield return typingCoroutine;

            // If there are choices, show them instead of next button
            if (currentEvent.choices != null && currentEvent.choices.Length > 0)
            {
                ShowChoices(currentEvent.choices);
            }
            else
            {
                nextButton.SetActive(true);
            }
        }
        else
        {
            diagBox.SetActive(false);

            if (currentEvent.voiceLine != null && currentEvent.voiceLine.clip != null)
                yield return new WaitForSeconds(currentEvent.voiceLine.clip.length);

            JumpToNextEvent(currentEvent);
            DisplayNextDialogue();
            yield break;
        }

        JumpToNextEvent(currentEvent);
    }

    private void JumpToNextEvent(DialogueEvent currentEvent)
    {
        if (currentEvent.choices != null && currentEvent.choices.Length > 0)
        {
            // choices handle jumping
            return;
        }

        if (currentEvent.jumpId > 0)
        {
            int newIndex = FindEventIndexByID(currentEvent.jumpId);
            if (newIndex != -1)
                currentEventIndex = newIndex;
            else
            {
                Debug.LogWarning("Dialogue Event with ID " + currentEvent.jumpId + " not found. Continuing sequentially.");
                currentEventIndex++;
            }
        }
        else
        {
            currentEventIndex++;
        }
    }

    private int FindEventIndexByID(int id)
    {
        for (int i = 0; i < events.Length; i++)
        {
            if (i == id)
                return i;
        }
        return -1;
    }

    private IEnumerator TypeDialogue(string text)
    {
        isTyping = true;
        hasFinishedTyping = false;
        diagText.text = "";

        foreach (char letter in text.ToCharArray())
        {
            diagText.text += letter;
            yield return new WaitForSeconds(scrollSpeed);
        }

        isTyping = false;
        hasFinishedTyping = true;
    }

    public void SkipDialogue()
    {
        DialogueEvent currentEvent = events[currentEventIndex];

        // Stop typing and show full text
        if (isTyping && typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            diagText.text = currentEvent.dialogueText;
            isTyping = false;
            hasFinishedTyping = true;
        }

        // Handle next step after typing
        if (hasFinishedTyping)
        {
            hasFinishedTyping = false; // prevent double-trigger

            // If there are choices, show them
            if (currentEvent.choices != null && currentEvent.choices.Length > 0)
            {
                ShowChoices(currentEvent.choices);
            }
            else
            {
                // No choices: show next button and advance to next event
                nextButton.SetActive(true);
                JumpToNextEvent(currentEvent);
            }
        }
    }

    private void ShowChoices(DialogueChoice[] choices)
    {
        nextButton.SetActive(false);
        choicePanel.SetActive(true);

        foreach (Transform child in choicePanel.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var choice in choices)
        {
            GameObject newButton = Instantiate(choiceButtonPrefab, choicePanel.transform);

            newButton.transform.localScale = Vector3.one;
            newButton.transform.localPosition = Vector3.zero;

            TMP_Text buttonText = newButton.GetComponentInChildren<TMP_Text>();
            buttonText.text = choice.choiceText;

            newButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                OnChoiceSelected(choice.jumpId);
            });
        }
    }

    private void OnChoiceSelected(int jumpId)
    {
        choicePanel.SetActive(false);

        int newIndex = FindEventIndexByID(jumpId);
        if (newIndex != -1)
        {
            currentEventIndex = newIndex;
        }
        else
        {
            Debug.LogWarning("Invalid jumpId: " + jumpId + ". Moving sequentially.");
            currentEventIndex++;
        }

        DisplayNextDialogue();
    }
}
