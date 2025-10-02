using UnityEngine;
using System;
using System.Collections;

public class CrossControl : MonoBehaviour
{
    [Header("Dependencies")]
    public DialogManager dialogManager;
    public SpotlightControl spotlightControl;
    public FileCollection fileCollection;
    public ProfileCollection profileCollection;

    [Header("Cross Examination Settings")]
    public DialogManager.DialogueEvent[] testimonyEvents;
    public int correctIndex = 0;
    public string requiredEvidence;
    public float logicPointsGain = 20f;
    public float logicPointsPenalty = -10f;

    private int currentIndex = 0;
    private bool testimonyActive = false;

    public event Action OnCrossExaminationEnd;

    public void StartCrossExamination()
    {
        if (dialogManager == null || testimonyEvents.Length == 0)
        {
            Debug.LogError("[CrossExamination] Missing setup!");
            return;
        }

        testimonyActive = true;
        currentIndex = 0;
        dialogManager.events = testimonyEvents;
        dialogManager.StartDialogueAt(currentIndex);
        dialogManager.onDiagEnd += LoopDialogue;
    }

    private void LoopDialogue()
    {
        if (!testimonyActive) return;

        // Cycle back to start
        currentIndex++;
        if (currentIndex >= testimonyEvents.Length)
            currentIndex = 0;

        dialogManager.StartDialogueAt(currentIndex);
    }

    public void PresentEvidence(string evidenceName)
    {
        if (!testimonyActive) return;

        if (currentIndex == correctIndex && evidenceName == requiredEvidence)
        {
            Debug.Log($"[CrossExamination] Correct evidence: {evidenceName}!");
            spotlightControl.AdjustMeter(logicPointsGain);

            testimonyActive = false;
            dialogManager.onDiagEnd -= LoopDialogue;

            if (dialogManager != null)
            {
                dialogManager.events = testimonyEvents;
                dialogManager.StartDialogueAt(correctIndex + 1);
            }

            OnCrossExaminationEnd?.Invoke();
        }
        else
        {
            Debug.Log($"[CrossExamination] Wrong evidence: {evidenceName}");
            spotlightControl.AdjustMeter(logicPointsPenalty);
        }
    }
}
