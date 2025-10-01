using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Logzzza : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform logContentContainer;
    public GameObject logEntryPrefab;
    public ScrollRect logScrollRect;

    private string lastCharacter = "";
    private string lastDialogue = "";

    public void LogDialogue(string character, string dialogue)
    {
        if (logContentContainer == null || logEntryPrefab == null) return;
        if (character == lastCharacter && dialogue == lastDialogue) return;

        GameObject entry = Instantiate(logEntryPrefab, logContentContainer);

        TMP_Text[] texts = entry.GetComponentsInChildren<TMP_Text>();
        if (texts.Length >= 2)
        {
            texts[0].text = string.IsNullOrEmpty(character) ? "" : character;
            texts[1].text = string.IsNullOrEmpty(dialogue) ? "" : dialogue;
        }

        lastCharacter = character;
        lastDialogue = dialogue;

        LayoutRebuilder.ForceRebuildLayoutImmediate(logContentContainer);
        Canvas.ForceUpdateCanvases();
        if (logScrollRect != null)
            logScrollRect.verticalNormalizedPosition = 0f;
    }

    public void ClearLog()
    {
        foreach (Transform child in logContentContainer)
            Destroy(child.gameObject);

        lastCharacter = "";
        lastDialogue = "";

        LayoutRebuilder.ForceRebuildLayoutImmediate(logContentContainer);
        Canvas.ForceUpdateCanvases();
        if (logScrollRect != null)
            logScrollRect.verticalNormalizedPosition = 0f;
    }
}
