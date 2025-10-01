using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class UIControl : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject investigate;
    public GameObject files;
    public GameObject options;
    public GameObject crosshair;
    public GameObject dialog;
    public GameObject logs;

    [Header("Script Control")]
    public MonoBehaviour dragControls;

    void Update()
    {
        UpdateState();
    }

    void UpdateState()
    {
        bool disableDrag = dialog.activeSelf || files.activeSelf || options.activeSelf || logs.activeSelf;

        dragControls.enabled = !disableDrag;
        crosshair.SetActive(!disableDrag);
    }

    public void OptionsClick()
    {
        options.SetActive(!options.activeSelf);
    }

    public void FileClick()
    {
        files.SetActive(!files.activeSelf);

        Debug.Log(!(files.activeSelf || options.activeSelf));

        GameObject clicked = EventSystem.current.currentSelectedGameObject;
        if (clicked != null)
        {
            TMP_Text label = clicked.GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = files.activeSelf ? "Close" : "Files";
        }
    }

    public void LogClick()
    {
        logs.SetActive(!logs.activeSelf);
    }
}
