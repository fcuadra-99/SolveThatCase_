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
public class PhaseConfig
{
    public string phaseName;
    public GamePhase phaseType;
    public int dialogueStartIndex = 0;
    public CrossControl crossExamManager;
    public AudioClip backgroundMusic;
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
        if (index < 0 || index >= phases.Count)
            return;

        if (transitioning) return;

        PhaseConfig phase = phases[index];
        PlayMusic(phase.backgroundMusic);

        phaseActive = true;

        switch (phase.phaseType)
        {
            case GamePhase.Dialogue:
                StartDialogue(phase);
                break;
            case GamePhase.Investigation:
                StartInvestigation(phase);
                break;
            case GamePhase.PostInvestigationDialogue:
                StartDialogue(phase);
                break;
            case GamePhase.Trial:
                StartTrial(phase);
                break;
            case GamePhase.Complete:
                phaseActive = false;
                break;
        }
    }

    private void StartDialogue(PhaseConfig phase)
    {
        dialogManager.onDiagEnd -= HandleDialogueEnd;
        dialogManager.onDiagEnd += HandleDialogueEnd;
        dialogManager.StartDialogueAt(phase.dialogueStartIndex);
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
    }

    public void EndInvestigation()
    {
        if (!phaseActive) return;
        PhaseConfig current = phases[currentPhaseIndex];
        if (current.phaseType != GamePhase.Investigation) return;
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
