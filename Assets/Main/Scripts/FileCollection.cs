using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ItemData
{
    public string itemName;
    [TextArea(2, 5)] public string description;
    public DialogManager.DialogueEvent[] dialogueEvents;
    public Transform worldLocation;
}

public class FileCollection : MonoBehaviour
{
    private HashSet<string> collectedItems = new HashSet<string>();

    [Header("UI References")]
    public Transform buttonContainer;
    public TMP_Text nameText;
    public TMP_Text descriptionText;

    [Header("Prefabs")]
    public GameObject itemButtonPrefab;

    [Header("Item Database")]
    public List<ItemData> itemDatabase = new List<ItemData>();

    [Header("Systems")]
    public Controls controls;
    public DialogManager dialogManager;
    public CrossControl crossExamManager;
    public SequenceControl sequenceControl;

    private Dictionary<string, ItemData> itemLookup = new Dictionary<string, ItemData>();

    void Start()
    {
        if (descriptionText != null) descriptionText.text = "";
        foreach (var data in itemDatabase)
        {
            if (!itemLookup.ContainsKey(data.itemName))
                itemLookup.Add(data.itemName, data);
        }
    }

    public bool CollectItem(string itemName)
    {
        if (!itemLookup.ContainsKey(itemName)) return false;
        if (collectedItems.Contains(itemName)) return false;

        ItemData item = itemLookup[itemName];
        collectedItems.Add(itemName);
        CreateButton(itemName);
        ShowDescription(itemName);

        if (controls != null && item.worldLocation != null)
            controls.FocusOnItem(item.worldLocation.position);

        if (dialogManager != null && item.dialogueEvents.Length > 0)
            StartItemDialogue(item);

        CheckCompletion();
        return true;
    }

    private void CheckCompletion()
    {
        if (collectedItems.Count >= itemDatabase.Count)
        {
            if (sequenceControl != null)
                sequenceControl.EndInvestigation();
        }
    }

    private void CreateButton(string itemName)
    {
        if (itemButtonPrefab == null || buttonContainer == null) return;
        GameObject newButton = Instantiate(itemButtonPrefab, buttonContainer);
        TMP_Text buttonText = newButton.GetComponentInChildren<TMP_Text>();
        Button btn = newButton.GetComponent<Button>();

        if (buttonText != null) buttonText.text = itemName;
        if (btn != null) btn.onClick.AddListener(() => ShowDescription(itemName));
    }

    private void ShowDescription(string itemName)
    {
        if (descriptionText == null) return;
        if (itemLookup.TryGetValue(itemName, out ItemData data))
        {
            nameText.text = data.itemName;
            descriptionText.text = data.description;
        }
        else
        {
            nameText.text = "Unnamed";
            descriptionText.text = "No description available.";
        }
    }

    private void StartItemDialogue(ItemData item)
    {
        if (item.dialogueEvents == null || item.dialogueEvents.Length == 0) return;
        dialogManager.events = item.dialogueEvents;
        dialogManager.StartDialogueAt(0);
        dialogManager.onDiagEnd += HandleDiagEnd;
    }

    public void PresentSelectedItem()
    {
        if (nameText == null) return;
        string selectedItem = nameText.text;
        if (crossExamManager != null)
            crossExamManager.PresentEvidence(selectedItem);
    }

    private void HandleDiagEnd()
    {
        if (controls != null)
            controls.ResetFocus();
        dialogManager.onDiagEnd -= HandleDiagEnd;
    }

    public IEnumerable<string> GetCollectedItems()
    {
        return collectedItems;
    }
}
