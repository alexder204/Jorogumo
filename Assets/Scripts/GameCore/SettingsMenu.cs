using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private AudioMixer mainMixer;

    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider SFXSlider;

    public enum SettingsTab
    {
        Audio,
        Controls
    }

    [Header("Tab Panels")]
    [SerializeField] private GameObject audioPanel;
    [SerializeField] private GameObject controlsPanel;

    [Header("Opening Tab")]
    [SerializeField] private SettingsTab openingTab = SettingsTab.Audio;

    private void OnEnable()
    {
        ShowTab(openingTab);
    }

    private void ShowTab(SettingsTab tab)
    {
        if (audioPanel != null)
            audioPanel.SetActive(tab == SettingsTab.Audio);

        if (controlsPanel != null)
            controlsPanel.SetActive(tab == SettingsTab.Controls);
    }

    public void ShowAudioTab()
    {
        ShowTab(SettingsTab.Audio);
    }

    public void ShowControlsTab()
    {
        ShowTab(SettingsTab.Controls);
    }

    private bool initialized;

    private void Start()
    {
        InitializeSlider(masterSlider, "MasterVolume");
        InitializeSlider(musicSlider, "MusicVolume");
        InitializeSlider(SFXSlider, "SFXVolume");

        initialized = true;

        ApplyVolume("MasterVolume", masterSlider);
        ApplyVolume("MusicVolume", musicSlider);
        ApplyVolume("SFXVolume", SFXSlider);
    }

    private void InitializeSlider(Slider slider, string parameter)
    {
        if (slider == null)
            return;

        // Existing preferences contain decibel values.
        float savedDb = Mathf.Clamp(
            PlayerPrefs.GetFloat(parameter, 0f), -80f, 0f);

        float value = savedDb <= -80f
            ? 0f
            : Mathf.Pow(10f, savedDb / 20f);

        // Prevent existing Inspector callbacks during setup.
        slider.SetValueWithoutNotify(0f);
        slider.wholeNumbers = false;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.SetValueWithoutNotify(value);
    }

    private void ApplyVolume(string parameter, Slider slider)
    {
        if (mainMixer == null || slider == null)
            return;

        float value = Mathf.Clamp01(slider.value);

        float decibels = value <= 0f
            ? -80f
            : Mathf.Clamp(20f * Mathf.Log10(value), -80f, 0f);

        if (!mainMixer.SetFloat(parameter, decibels))
        {
            Debug.LogWarning(
                $"Check exposed mixer parameter: {parameter}", this);
            return;
        }

        // Keep the existing preference format: decibels.
        PlayerPrefs.SetFloat(parameter, decibels);
    }

    public void SetMasterVolume()
    {
        if (initialized)
            ApplyVolume("MasterVolume", masterSlider);
    }

    public void SetMusicVolume()
    {
        if (initialized)
            ApplyVolume("MusicVolume", musicSlider);
    }

    public void SetSFXVolume()
    {
        if (initialized)
            ApplyVolume("SFXVolume", SFXSlider);
    }

    private void OnDisable()
    {
        if (initialized)
            PlayerPrefs.Save();
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused && initialized)
            PlayerPrefs.Save();
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }
}