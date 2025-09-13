using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class ItemData
{
    public string itemName;
    [TextArea(2, 5)]
    public string description;
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

    private Dictionary<string, string> itemLookup = new Dictionary<string, string>();

    void Start()
    {
        if (descriptionText != null) descriptionText.text = "";

        foreach (var data in itemDatabase)
        {
            if (!itemLookup.ContainsKey(data.itemName))
                itemLookup.Add(data.itemName, data.description);
        }
    }

    public bool CollectItem(string itemName)
    {
        if (collectedItems.Contains(itemName))
        {
            Debug.Log("Item already collected: " + itemName);
            return false;
        }
        else
        {
            collectedItems.Add(itemName);
            CreateButton(itemName);
            ShowDescription(itemName);
            Debug.Log("Collected new item: " + itemName);
            return true;
        }
    }

    private void CreateButton(string itemName)
    {
        if (itemButtonPrefab == null || buttonContainer == null) return;

        GameObject newButton = Instantiate(itemButtonPrefab, buttonContainer);
        TMP_Text buttonText = newButton.GetComponentInChildren<TMP_Text>();
        Button btn = newButton.GetComponent<Button>();

        if (buttonText != null)
            buttonText.text = itemName;

        if (btn != null)
            btn.onClick.AddListener(() => ShowDescription(itemName));
    }

    private void ShowDescription(string itemName)
    {
        if (descriptionText == null) return;

        if (itemLookup.TryGetValue(itemName, out string desc))
        {
            nameText.text = itemName;
            descriptionText.text = desc;
        }
        else
        {
            nameText.text = "Unnamed";
            descriptionText.text = "No description available.";
        }
    }

    public IEnumerable<string> GetCollectedItems()
    {
        return collectedItems;
    }
}
