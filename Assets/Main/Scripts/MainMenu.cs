using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public void NewGame()
    {
        StartCoroutine(newstat());
    }

    IEnumerator staticc()
    {
        staticAsset.SetActive(true);
        staticfx.Play();
        yield return new WaitUntil(() => !staticfx.isPlaying);
        staticAsset.SetActive(false);
    }

    IEnumerator newstat()
    {
        staticAsset.SetActive(true);
        staticfx.Play();
        yield return new WaitUntil(() => !staticfx.isPlaying);
        SceneManager.LoadScene(1);
    }
}
