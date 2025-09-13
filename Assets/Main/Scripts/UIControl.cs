using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIControl : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject investigate;
    public GameObject files;
    public GameObject options;

    [Header("Script Control")]
    public MonoBehaviour scriptDisable;

    public void OptionsClick()
    {
        options.SetActive(!options.activeSelf);
        if (scriptDisable != null)
            scriptDisable.enabled = !(files.activeSelf || options.activeSelf);
    }

    public void FileClick()
    {
        files.SetActive(!files.activeSelf);
        if (scriptDisable != null)
            scriptDisable.enabled = !(files.activeSelf || options.activeSelf);

        GameObject clicked = EventSystem.current.currentSelectedGameObject;
        if (clicked != null)
        {
            TMP_Text label = clicked.GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = files.activeSelf ? "Close" : "Files";
        }
    }
}
