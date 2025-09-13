using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public class MenuNav
    {
        public GameObject panel;
        public int jumpID = -1;
    }

    public GameObject[] bgs;
    public MenuNav[] menus;
    public GameObject staticAsset;
    public AudioSource staticfx;

    bool playing;

    public void staticVis()
    {
        StartCoroutine(staticc());
    }

    void Update()
    {
        playing = staticfx.isPlaying;
    }

    IEnumerator staticc()
    {
        staticAsset.SetActive(true);
        staticfx.Play();
        yield return new WaitUntil(() => !staticfx.isPlaying);
        staticAsset.SetActive(false);
    }
}
