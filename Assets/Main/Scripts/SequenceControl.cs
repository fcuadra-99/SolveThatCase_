using UnityEngine;
using System.Collections.Generic;

public enum GameEventType
{
    Investigation,
    Dialogue,
    Trial
}

[System.Serializable]
public class GameEvent
{
    public string eventName;
    public GameEventType eventType;
    public int dialogueStartIndex = 0;
}

public class SequenceControl : MonoBehaviour
{
    [Header("Controls")]
    public DialogManager dialog;

    [Header("Event Sequence")]
    public List<GameEvent> events = new List<GameEvent>();

    private int currentIndex = 0;

    private void Start()
    {
        if (events.Count > 0)
            StartEvent(currentIndex);
    }

    public void StartEvent(int index)
    {
        if (index < 0 || index >= events.Count)
            return;

        GameEvent gameEvent = events[index];
        Debug.Log($"Starting Event: {gameEvent.eventName} | Type: {gameEvent.eventType}");

        switch (gameEvent.eventType)
        {
            case GameEventType.Investigation:
                StartInvestigation(gameEvent);
                break;
            case GameEventType.Dialogue:
                StartDialogue(gameEvent);
                break;
            case GameEventType.Trial:
                StartTrial(gameEvent);
                break;
        }
    }

    private void StartInvestigation(GameEvent gameEvent)
    {
        Debug.Log("Investigation started: " + gameEvent.eventName);
        NextEvent();
    }

    private void StartDialogue(GameEvent gameEvent)
    {
        Debug.Log("Dialogue started: " + gameEvent.eventName);
        dialog.StartDialogueAt(gameEvent.dialogueStartIndex);
        NextEvent();
    }

    private void StartTrial(GameEvent gameEvent)
    {
        Debug.Log("Trial started: " + gameEvent.eventName);
        NextEvent();
    }

    public void NextEvent()
    {
        currentIndex++;
        if (currentIndex < events.Count)
            StartEvent(currentIndex);
        else
            Debug.Log("All events completed!");
    }
}
