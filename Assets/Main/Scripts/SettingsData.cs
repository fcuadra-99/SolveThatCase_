using UnityEngine;

[CreateAssetMenu(fileName = "Settings", menuName = "Config/Settings")]
public class SettingsData : ScriptableObject
{
    [Header("Gameplay")]
    [Range(0f, 1f)] public float textSpeed = 0.5f;
    [Range(10f, 30f)] public float crosshairSpeed = 20f;

    [Header("Audio")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float bgmVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    [Header("Visual")]
    [Range(0f, 2f)] public float brightness = 1f;
}