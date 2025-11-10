using UnityEngine;
using UnityEngine.UI;
using System;

public class CrossControl : MonoBehaviour
{
    [System.Serializable]
    public class CrossExaminationPhase
    {
        public DialogManager.DialogueEvent[] testimonyEvents;
        public int correctIndex;
        public string requiredEvidence;

        public float logicPointsGain = 20f;
        public float logicPointsPenalty = -10f;

        public DialogManager.DialogueEvent[] correctEvidenceDialogue;
        public DialogManager.DialogueEvent[] wrongEvidenceDialogue;
    }

    [Header("Dependencies")]
    public DialogManager dialogManager;
    public SpotlightControl spotlightControl;
    public FileCollection fileCollection;
    public ProfileCollection profileCollection;
    public UIControl uiControl;

    [Header("UI")]
    public Button presentEvidenceButton;
    public Button presentProfileButton;

    [Header("Cross Examination Phases")]
    public CrossExaminationPhase[] phases;

    [Header("Final Dialogue After All Phases")]
    public DialogManager.DialogueEvent[] finalDialogue;

    private int currentPhase = 0;
    private int currentIndex = 0;
    private bool testimonyActive = false;

    public event Action OnCrossExaminationEnd;

    void Start()
    {
        if (presentEvidenceButton != null)
            presentEvidenceButton.gameObject.SetActive(false);
        if (presentProfileButton != null)
            presentProfileButton.gameObject.SetActive(false);
    }

    public void StartCrossExamination()
    {
        if (dialogManager == null || phases.Length == 0)
        {
            Debug.LogError("[CrossExamination] No phases set!");
            return;
        }

        currentPhase = 0;
        StartPhase(currentPhase);
    }

    private void StartPhase(int phaseIndex)
    {
        testimonyActive = true;
        currentIndex = 0;

        var phase = phases[phaseIndex];

        dialogManager.events = phase.testimonyEvents;
        dialogManager.StartDialogueAt(currentIndex);

        dialogManager.onDiagEnd += LoopDialogue;

        presentEvidenceButton.gameObject.SetActive(true);
        presentProfileButton.gameObject.SetActive(true);
    }

    private void LoopDialogue()
    {
        if (!testimonyActive) return;

        currentIndex++;
        if (currentIndex >= phases[currentPhase].testimonyEvents.Length)
            currentIndex = 0;

        dialogManager.StartDialogueAt(currentIndex);
    }

    public void PresentEvidence(string evidenceName)
    {
        if (!testimonyActive) return;

        var phase = phases[currentPhase];

        bool correct = (currentIndex == phase.correctIndex && evidenceName == phase.requiredEvidence);

        if (correct)
        {
            spotlightControl.AdjustMeter(phase.logicPointsGain);
            testimonyActive = false;

            dialogManager.onDiagEnd -= LoopDialogue;

            presentEvidenceButton.gameObject.SetActive(false);
            presentProfileButton.gameObject.SetActive(false);

            CloseUI();

            dialogManager.events = phase.correctEvidenceDialogue;
            dialogManager.StartDialogueAt(0);

            dialogManager.onDiagEnd += OnCorrectDialogueFinished;
        }
        else
        {
            spotlightControl.AdjustMeter(phase.logicPointsPenalty);

            testimonyActive = false;
            dialogManager.onDiagEnd -= LoopDialogue;

            CloseUI();

            dialogManager.events = phase.wrongEvidenceDialogue;
            dialogManager.StartDialogueAt(0);

            dialogManager.onDiagEnd += ResumePhaseAfterWrong;
        }
    }

    private void ResumePhaseAfterWrong()
    {
        dialogManager.onDiagEnd -= ResumePhaseAfterWrong;

        testimonyActive = true;
        var phase = phases[currentPhase];

        dialogManager.events = phase.testimonyEvents;
        dialogManager.StartDialogueAt(currentIndex);

        dialogManager.onDiagEnd += LoopDialogue;
    }

    private void OnCorrectDialogueFinished()
    {
        dialogManager.onDiagEnd -= OnCorrectDialogueFinished;

        currentPhase++;

        if (currentPhase >= phases.Length)
        {
            if (finalDialogue != null && finalDialogue.Length > 0)
            {
                dialogManager.events = finalDialogue;
                dialogManager.StartDialogueAt(0);
            }

            OnCrossExaminationEnd?.Invoke();
            return;
        }

        StartPhase(currentPhase);
    }

    public void PresentEvidenceFromFiles()
    {
        fileCollection?.PresentSelectedItem();
        CloseUI();
    }

    public void PresentProfileFromCollection()
    {
        profileCollection?.PresentSelectedProfile();
        CloseUI();
    }

    public void CloseUI()
    {
        uiControl.disableFiles();
    }
}
