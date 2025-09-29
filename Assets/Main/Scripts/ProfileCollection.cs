using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CharacterData
{
    public string characterName;
    [TextArea(2, 5)]
    public string profileDescription;

    [Header("Dialogue Events")]
    public DialogManager.DialogueEvent[] firstDialogue;
    public DialogManager.DialogueEvent[] afterDialogue;

    [Header("World Reference")]
    public Transform worldLocation;
}

public class ProfileCollection : MonoBehaviour
{
    private HashSet<string> metCharacters = new HashSet<string>();

    [Header("UI References")]
    public Transform buttonContainer;
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    [Header("Prefabs")]
    public GameObject characterButtonPrefab;

    [Header("Character Database")]
    public List<CharacterData> characterDatabase = new List<CharacterData>();

    [Header("Systems")]
    public Controls controls;
    public DialogManager dialogManager;

    private Dictionary<string, CharacterData> characterLookup = new Dictionary<string, CharacterData>();

    void Start()
    {
        if (descriptionText != null) descriptionText.text = "";

        foreach (var data in characterDatabase)
        {
            if (!characterLookup.ContainsKey(data.characterName))
                characterLookup.Add(data.characterName, data);
        }
    }

    public void TalkToCharacter(string characterName)
    {
        if (!characterLookup.ContainsKey(characterName))
        {
            Debug.LogWarning("Character not found in database: " + characterName);
            return;
        }

        CharacterData character = characterLookup[characterName];

        // Add to profiles if first time
        if (!metCharacters.Contains(characterName))
        {
            metCharacters.Add(characterName);
            CreateButton(characterName);
            ShowProfile(characterName);
            Debug.Log("Met new character: " + characterName);

            if (character.firstDialogue != null && character.firstDialogue.Length > 0)
                StartCharacterDialogue(character.firstDialogue, character);
        }
        else
        {
            Debug.Log("Talking again with: " + characterName);

            if (character.afterDialogue != null && character.afterDialogue.Length > 0)
                StartCharacterDialogue(character.afterDialogue, character);
        }

        if (controls != null && character.worldLocation != null)
        {
            controls.FocusOnItem(character.worldLocation.position);
        }
    }

    private void CreateButton(string characterName)
    {
        if (characterButtonPrefab == null || buttonContainer == null) return;

        GameObject newButton = Instantiate(characterButtonPrefab, buttonContainer);
        TMP_Text buttonText = newButton.GetComponentInChildren<TMP_Text>();
        Button btn = newButton.GetComponent<Button>();

        if (buttonText != null)
            buttonText.text = characterName;

        if (btn != null)
            btn.onClick.AddListener(() => ShowProfile(characterName));
    }

    private void ShowProfile(string characterName)
    {
        if (descriptionText == null) return;

        if (characterLookup.TryGetValue(characterName, out CharacterData data))
        {
            nameText.text = data.characterName;
            descriptionText.text = data.profileDescription;
        }
        else
        {
            nameText.text = "Unknown";
            descriptionText.text = "No profile available.";
        }
    }

    private void StartCharacterDialogue(DialogManager.DialogueEvent[] dialogue, CharacterData character)
    {
        dialogManager.events = dialogue;
        dialogManager.StartDialogueAt(0);
        dialogManager.onDiagEnd += HandleDiagEnd;
    }

    private void HandleDiagEnd()
    {
        if (controls != null)
        {
            controls.ResetFocus();
        }

        dialogManager.onDiagEnd -= HandleDiagEnd;
    }

    public IEnumerable<string> GetMetCharacters()
    {
        return metCharacters;
    }
}
