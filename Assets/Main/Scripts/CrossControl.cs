using UnityEngine;
using UnityEngine.UI;
using System;

public class CrossControl : MonoBehaviour
{
    [Header("Dependencies")]
    public DialogManager dialogManager;
    public SpotlightControl spotlightControl;
    public FileCollection fileCollection;
    public ProfileCollection profileCollection;
    public UIControl uiControl;

    [Header("UI")]
    public Button presentEvidenceButton;
    public Button presentProfileButton;

    [Header("Cross Examination Settings")]
    public DialogManager.DialogueEvent[] testimonyEvents;
    public int correctIndex = 0;
    public string requiredEvidence;
    public float logicPointsGain = 20f;
    public float logicPointsPenalty = -10f;

    [Header("Correct Evidence Settings")]
    public DialogManager.DialogueEvent[] correctEvidenceDialogue;

    [Header("Wrong Evidence Settings")]
    public DialogManager.DialogueEvent[] wrongEvidenceDialogue;

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

        if (presentEvidenceButton != null)
            presentEvidenceButton.gameObject.SetActive(true);
        if (presentProfileButton != null)
            presentProfileButton.gameObject.SetActive(true);
    }

    private void LoopDialogue()
    {
        if (!testimonyActive) return;

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
            spotlightControl.AdjustMeter(logicPointsGain);
            testimonyActive = false;
            dialogManager.onDiagEnd -= LoopDialogue;

            if (presentEvidenceButton != null)
                presentEvidenceButton.gameObject.SetActive(false);
            if (presentProfileButton != null)
                presentProfileButton.gameObject.SetActive(false);

            CloseUI();

            if (correctEvidenceDialogue != null && correctEvidenceDialogue.Length > 0)
            {
                dialogManager.events = correctEvidenceDialogue;
                dialogManager.StartDialogueAt(0);
            }

            OnCrossExaminationEnd?.Invoke();
        }
        else
        {
            spotlightControl.AdjustMeter(logicPointsPenalty);
            dialogManager.onDiagEnd -= LoopDialogue;
            testimonyActive = false;

            CloseUI();

            if (wrongEvidenceDialogue != null && wrongEvidenceDialogue.Length > 0)
            {
                dialogManager.events = wrongEvidenceDialogue;
                dialogManager.StartDialogueAt(0);
                dialogManager.onDiagEnd += ResumeTestimony;
            }
        }
    }

    private void ResumeTestimony()
    {
        dialogManager.onDiagEnd -= ResumeTestimony;

        if (testimonyEvents != null && testimonyEvents.Length > 0)
        {
            testimonyActive = true;
            dialogManager.events = testimonyEvents;
            dialogManager.StartDialogueAt(currentIndex);
            dialogManager.onDiagEnd += LoopDialogue;
        }
    }

    public void PresentEvidenceFromFiles()
    {
        if (fileCollection != null)
            fileCollection.PresentSelectedItem();

        CloseUI();
    }

    public void PresentProfileFromCollection()
    {
        if (profileCollection != null)
            profileCollection.PresentSelectedProfile();

        CloseUI();
    }

    public void EndCrossExamination()
    {
        testimonyActive = false;
        dialogManager.onDiagEnd -= LoopDialogue;

        if (presentEvidenceButton != null)
            presentEvidenceButton.gameObject.SetActive(false);
        if (presentProfileButton != null)
            presentProfileButton.gameObject.SetActive(false);

        CloseUI();
    }

    public void CloseUI()
    {
        uiControl.disableFiles();
    }
}
