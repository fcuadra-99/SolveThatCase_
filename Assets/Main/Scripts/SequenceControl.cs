using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GamePhase
{
    Dialogue,
    Investigation,
    PostInvestigationDialogue,
    Trial,
    Complete
}

[System.Serializable]
public class SimpleDialogueLine
{
    public string characterName;
    [TextArea(2, 5)] public string dialogueText;
    public AudioClip voiceLine;
}

[System.Serializable]
public class PhaseConfig
{
    public string phaseName;
    public GamePhase phaseType;

    [Header("For Dialogue Phases")]
    public int dialogueStartIndex = 0;

    [Header("For Trial Phases")]
    public CrossControl crossExamManager;

    [Header("Background Music")]
    public AudioClip backgroundMusic;

    [Header("For Investigation Phases")]
    public bool hasAfterInvestigationDialogue = false;
    public List<SimpleDialogueLine> afterInvestigationDialogue = new List<SimpleDialogueLine>();
    public AudioClip afterInvestigationMusic;
}

public class SequenceControl : MonoBehaviour
{
    [Header("Systems")]
    public DialogManager dialogManager;
    public FileCollection fileCollection;
    public CrossControl trialManager;
    public AudioSource musicSource;

    [Header("Phase Sequence")]
    public List<PhaseConfig> phases = new List<PhaseConfig>();

    private int currentPhaseIndex = 0;
    private Coroutine musicFadeRoutine;
    private bool phaseActive = false;
    private bool transitioning = false;
    private CrossControl activeTrialManager = null;

    private void Start()
    {
        if (fileCollection != null)
            fileCollection.sequenceControl = this;

        if (phases.Count > 0)
            StartPhase(currentPhaseIndex);
    }

    private void StartPhase(int index)
    {
        if (index < 0 || index >= phases.Count || transitioning)
            return;

        PhaseConfig phase = phases[index];
        PlayMusic(phase.backgroundMusic);
        phaseActive = true;

        switch (phase.phaseType)
        {
            case GamePhase.Dialogue:
            case GamePhase.PostInvestigationDialogue:
                StartDialogue(phase.dialogueStartIndex);
                break;

            case GamePhase.Investigation:
                StartInvestigation(phase);
                break;

            case GamePhase.Trial:
                StartTrial(phase);
                break;

            case GamePhase.Complete:
                Debug.Log("All phases complete!");
                phaseActive = false;
                break;
        }
    }

    private void StartDialogue(int startIndex)
    {
        dialogManager.onDiagEnd -= HandleDialogueEnd;
        dialogManager.onDiagEnd += HandleDialogueEnd;
        dialogManager.StartDialogueAt(startIndex);
    }

    private void HandleDialogueEnd()
    {
        dialogManager.onDiagEnd -= HandleDialogueEnd;
        EndCurrentPhase();
    }

    private void StartInvestigation(PhaseConfig phase)
    {
        if (fileCollection != null)
            fileCollection.sequenceControl = this;
        Debug.Log($"Investigation phase started: {phase.phaseName}");
    }

    public void EndInvestigation()
    {
        if (!phaseActive) return;
        PhaseConfig current = phases[currentPhaseIndex];
        if (current.phaseType != GamePhase.Investigation) return;

        if (current.hasAfterInvestigationDialogue && current.afterInvestigationDialogue.Count > 0)
        {
            Debug.Log($"Starting after-investigation dialogue for {current.phaseName}");
            StartCoroutine(StartAfterInvestigationDialogue(current));
        }
        else
        {
            EndCurrentPhase();
        }
    }

    private IEnumerator StartAfterInvestigationDialogue(PhaseConfig phase)
    {
        phaseActive = false;
        yield return new WaitForSeconds(0.2f);

        PlayMusic(phase.afterInvestigationMusic);

        // Create temporary dialogue events for DialogManager
        var tempEvents = new List<DialogManager.DialogueEvent>();
        foreach (var line in phase.afterInvestigationDialogue)
        {
            var ev = new DialogManager.DialogueEvent
            {
                characterName = line.characterName,
                dialogueText = line.dialogueText,
                voiceLine = null,
                activeChar = null,
                delay = 0f,
                jumpIndex = -1,
                choices = null
            };
            tempEvents.Add(ev);
        }

        dialogManager.events = tempEvents.ToArray();
        dialogManager.onDiagEnd -= HandleAfterInvestigationEnd;
        dialogManager.onDiagEnd += HandleAfterInvestigationEnd;
        dialogManager.StartDialogueAt(0);
    }

    private void HandleAfterInvestigationEnd()
    {
        dialogManager.onDiagEnd -= HandleAfterInvestigationEnd;
        EndCurrentPhase();
    }

    private void StartTrial(PhaseConfig phase)
    {
        CrossControl manager = phase.crossExamManager != null ? phase.crossExamManager : trialManager;
        if (manager == null)
        {
            EndCurrentPhase();
            return;
        }

        if (activeTrialManager != null)
        {
            activeTrialManager.OnCrossExaminationEnd -= HandleTrialEnd;
            activeTrialManager = null;
        }

        activeTrialManager = manager;
        activeTrialManager.OnCrossExaminationEnd -= HandleTrialEnd;
        activeTrialManager.OnCrossExaminationEnd += HandleTrialEnd;
        activeTrialManager.StartCrossExamination();
    }

    private void HandleTrialEnd()
    {
        if (activeTrialManager != null)
        {
            activeTrialManager.OnCrossExaminationEnd -= HandleTrialEnd;
            activeTrialManager = null;
        }
        EndCurrentPhase();
    }

    private void EndCurrentPhase()
    {
        if (transitioning) return;
        transitioning = true;
        phaseActive = false;
        StartCoroutine(AdvancePhaseOneFrame());
    }

    private IEnumerator AdvancePhaseOneFrame()
    {
        yield return null;
        currentPhaseIndex++;
        transitioning = false;

        if (currentPhaseIndex < phases.Count)
            StartPhase(currentPhaseIndex);
        else
            Debug.Log("All phases finished.");
    }

    private void PlayMusic(AudioClip clip)
    {
        if (musicSource == null) return;
        if (musicFadeRoutine != null)
            StopCoroutine(musicFadeRoutine);
        musicFadeRoutine = StartCoroutine(FadeMusic(clip));
    }

    private IEnumerator FadeMusic(AudioClip newClip)
    {
        if (musicSource.isPlaying)
        {
            float t = 1f;
            while (t > 0f)
            {
                t -= Time.deltaTime;
                musicSource.volume = Mathf.Max(0f, t);
                yield return null;
            }
            musicSource.Stop();
        }

        if (newClip != null)
        {
            musicSource.clip = newClip;
            musicSource.loop = true;
            musicSource.Play();
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime;
                musicSource.volume = Mathf.Min(1f, t);
                yield return null;
            }
            musicSource.volume = 1f;
        }
    }
}
