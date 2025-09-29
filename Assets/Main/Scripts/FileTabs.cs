using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FileTabs : MonoBehaviour
{
    [System.Serializable]
    public class Tab
    {
        public Button button;
        public GameObject panel;
    }

    [Header("Tabs")]
    public List<Tab> tabs = new List<Tab>();

    [Header("Visuals")]
    public Color activeColor = Color.white;
    public Color inactiveColor = Color.gray;

    private int activeIndex = -1;

    void Start()
    {
        for (int i = 0; i < tabs.Count; i++)
        {
            int index = i;
            tabs[i].button.onClick.AddListener(() => SwitchTab(index));
        }

        if (tabs.Count > 0)
            SwitchTab(0);
    }

    public void SwitchTab(int index)
    {
        if (index < 0 || index >= tabs.Count) return;

        for (int i = 0; i < tabs.Count; i++)
        {
            if (tabs[i].panel != null)
                tabs[i].panel.SetActive(false);

            if (tabs[i].button != null)
            {
                Image btnImage = tabs[i].button.GetComponent<Image>();
                if (btnImage != null)
                    btnImage.color = inactiveColor;
            }
        }

        if (tabs[index].panel != null)
            tabs[index].panel.SetActive(true);

        if (tabs[index].button != null)
        {
            Image btnImage = tabs[index].button.GetComponent<Image>();
            if (btnImage != null)
                btnImage.color = activeColor;
        }

        activeIndex = index;
    }
}
