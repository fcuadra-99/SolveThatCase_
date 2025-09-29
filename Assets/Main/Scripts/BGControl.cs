using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class DoorData
{
    public GameObject doorObject;  
    public int targetSceneIndex = -1;
    public AudioClip doorSound;     
}

public class BGControl : MonoBehaviour
{
    [Header("Doors & Scenes")]
    public List<DoorData> doors = new List<DoorData>();
    public List<GameObject> scenes = new List<GameObject>();
    public int startingSceneIndex = 0;

    [Header("Transition")]
    public Image fadeOverlay;
    public float fadeDuration = 1f;
    public AudioSource sfxSource;

    private int currentSceneIndex = -1;

    void Start()
    {
        // Deactivate all scenes
        foreach (var s in scenes)
            if (s != null) s.SetActive(false);

        // Activate starting scene
        if (scenes.Count > 0 && startingSceneIndex >= 0 && startingSceneIndex < scenes.Count)
        {
            ActivateScene(startingSceneIndex);
        }
    }

    public void OnTouchDown(GameObject clicked)
    {
        foreach (DoorData door in doors)
        {
            if (door.doorObject == clicked && door.targetSceneIndex >= 0)
            {
                StartCoroutine(HandleTransition(door));
                break;
            }
        }
    }

    private IEnumerator HandleTransition(DoorData door)
    {
        if (sfxSource != null && door.doorSound != null)
            sfxSource.PlayOneShot(door.doorSound);

        yield return StartCoroutine(Fade(1f));

        ActivateScene(door.targetSceneIndex);

        yield return StartCoroutine(Fade(0f));
    }

    private void ActivateScene(int index)
    {
        // Disable current
        if (currentSceneIndex >= 0 && currentSceneIndex < scenes.Count)
        {
            if (scenes[currentSceneIndex] != null)
                scenes[currentSceneIndex].SetActive(false);
        }

        // Enable target
        if (index >= 0 && index < scenes.Count && scenes[index] != null)
        {
            scenes[index].SetActive(true);
            currentSceneIndex = index;
        }
    }

    private IEnumerator Fade(float targetAlpha)
    {
        if (fadeOverlay == null) yield break;

        if (!fadeOverlay.gameObject.activeSelf)
            fadeOverlay.gameObject.SetActive(true);

        Color c = fadeOverlay.color;
        float startAlpha = c.a;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            c.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            fadeOverlay.color = c;
            yield return null;
        }

        c.a = targetAlpha;
        fadeOverlay.color = c;

        if (Mathf.Approximately(targetAlpha, 0f))
            fadeOverlay.gameObject.SetActive(false);
    }

    public GameObject GetActiveScene()
    {
        if (currentSceneIndex >= 0 && currentSceneIndex < scenes.Count)
            return scenes[currentSceneIndex];
        return null;
    }
}
