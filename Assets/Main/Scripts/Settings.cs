using System;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    public SettingsData settings;

    [Header("Gameplay UI")]
    public Slider textSpeedSlider;
    public Slider crosshairSpeedSlider;

    [Header("Audio UI")]
    public Slider masterVolumeSlider;
    public Slider bgmVolumeSlider;
    public Slider sfxVolumeSlider;

    [Header("Visual UI")]
    public Slider brightnessSlider;

    void Start()
    {
        if (settings == null) return;

        settings.textSpeed = PlayerPrefs.GetFloat("TextSpeed", settings.textSpeed);
        settings.crosshairSpeed = PlayerPrefs.GetFloat("CrosshairSpeed", settings.crosshairSpeed);

        settings.masterVolume = PlayerPrefs.GetFloat("MasterVolume", settings.masterVolume);
        settings.bgmVolume = PlayerPrefs.GetFloat("BGMVolume", settings.bgmVolume);
        settings.sfxVolume = PlayerPrefs.GetFloat("SFXVolume", settings.sfxVolume);

        settings.brightness = PlayerPrefs.GetFloat("Brightness", settings.brightness);

        if (textSpeedSlider != null) textSpeedSlider.value = settings.textSpeed;
        if (crosshairSpeedSlider != null) crosshairSpeedSlider.value = settings.crosshairSpeed;

        if (masterVolumeSlider != null) masterVolumeSlider.value = settings.masterVolume;
        if (bgmVolumeSlider != null) bgmVolumeSlider.value = settings.bgmVolume;
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = settings.sfxVolume;

        if (brightnessSlider != null) brightnessSlider.value = settings.brightness;
    }

    public void SetTextSpeed()
    {
        if (textSpeedSlider == null) return;
        settings.textSpeed = textSpeedSlider.value;
        PlayerPrefs.SetFloat("TextSpeed", settings.textSpeed);
    }

    public void SetCrosshairSpeed()
    {
        if (crosshairSpeedSlider == null) return;
        settings.crosshairSpeed = crosshairSpeedSlider.value;
        PlayerPrefs.SetFloat("CrosshairSpeed", settings.crosshairSpeed);
        Debug.Log(settings.crosshairSpeed);
    }

    public void SetMasterVolume()
    {
        if (masterVolumeSlider == null) return;
        settings.masterVolume = masterVolumeSlider.value;
        PlayerPrefs.SetFloat("MasterVolume", settings.masterVolume);
    }

    public void SetBGMVolume()
    {
        if (bgmVolumeSlider == null) return;
        settings.bgmVolume = bgmVolumeSlider.value;
        PlayerPrefs.SetFloat("BGMVolume", settings.bgmVolume);
    }

    public void SetSFXVolume()
    {
        if (sfxVolumeSlider == null) return;
        settings.sfxVolume = sfxVolumeSlider.value;
        PlayerPrefs.SetFloat("SFXVolume", settings.sfxVolume);
    }

    public void SetBrightness()
    {
        if (brightnessSlider == null) return;
        settings.brightness = brightnessSlider.value;
        PlayerPrefs.SetFloat("Brightness", settings.brightness);
    }


    public void ResetToDefaults()
    {
        textSpeedSlider.value = settings.textSpeed = 0.5f;
        crosshairSpeedSlider.value = settings.crosshairSpeed = 1f;

        masterVolumeSlider.value = settings.masterVolume = 1f;
        bgmVolumeSlider.value = settings.bgmVolume = 1f;
        sfxVolumeSlider.value = settings.sfxVolume = 1f;

        brightnessSlider.value = settings.brightness = 1f;

        PlayerPrefs.SetFloat("TextSpeed", settings.textSpeed);
        PlayerPrefs.SetFloat("CrosshairSpeed", settings.crosshairSpeed);
        PlayerPrefs.SetFloat("MasterVolume", settings.masterVolume);
        PlayerPrefs.SetFloat("BGMVolume", settings.bgmVolume);
        PlayerPrefs.SetFloat("SFXVolume", settings.sfxVolume);
        PlayerPrefs.SetFloat("Brightness", settings.brightness);
    }
}
