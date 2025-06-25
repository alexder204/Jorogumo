using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor.Overlays;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject settingsMenu;
    public GameObject areYouSure;
    public GameObject loadPanel;

    public TextMeshProUGUI slot0Label;
    public TextMeshProUGUI slot1Label;
    public TextMeshProUGUI slot2Label;
    public TextMeshProUGUI slot3Label;

    [SerializeField] private string newLevel;

    private bool isConfirmingQuit = false;

    private void Start()
    {
        mainMenu.SetActive(true);
        areYouSure.SetActive(false);
        settingsMenu.SetActive(false);
        UpdateSaveSlotLabels();
    }

    public void LoadSceneByName()
    {
        if (SceneFader.instance != null)
            SceneFader.instance.FadeOutAndLoad(newLevel);
        else
            SceneManager.LoadScene(newLevel);
    }

    public void ContinueGame()
    {
        string path = Application.persistentDataPath + "/saveslot99.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);
            PendingLoadSlot.loadSlot = 99;

            SceneFader.instance.FadeOutAndLoad(saveData.sceneName);
        }
        else
        {
            Debug.LogWarning("No autosave found!");
        }
    }

    public void LoadSlot(int slot)
    {
        string path = Application.persistentDataPath + $"/saveslot{slot}.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);
            PendingLoadSlot.loadSlot = slot;

            SceneFader.instance.FadeOutAndLoad(saveData.sceneName);
        }
        else
        {
            Debug.LogWarning($"No save in slot {slot}.");
        }
    }

    private void UpdateSaveSlotLabels()
    {
        UpdateSlotLabel(slot0Label, 0);
        UpdateSlotLabel(slot1Label, 1);
        UpdateSlotLabel(slot2Label, 2);
        UpdateSlotLabel(slot3Label, 3);
    }

    private void UpdateSlotLabel(TextMeshProUGUI label, int slot)
    {
        if (label == null) return;

        string path = Application.persistentDataPath + $"/saveslot{slot}.json";
        bool exists = File.Exists(path);
        label.text = $"Save Slot {slot} - {(exists ? "Saved" : "Empty")}";
    }

    public void OpenSettings()
    {
        StartCoroutine(OpenSettingsDelay(0.25f));
    }

    private IEnumerator OpenSettingsDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        DisableMainMenuInteractions();
        settingsMenu.SetActive(true);
    }

    public void BackToMainMenu()
    {
        StartCoroutine(CloseSettingsDelay(0.25f));
    }

    private IEnumerator CloseSettingsDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        settingsMenu.SetActive(false);
        EnableMainMenuInteractions();
    }

    public void OnQuitButtonClicked()
    {
        if (!isConfirmingQuit)
        {
            StartCoroutine(QuitButtonClickedDelay(0.25f));
            isConfirmingQuit = true;
            DisableMainMenuInteractions();
        }
    }

    private IEnumerator QuitButtonClickedDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        areYouSure.SetActive(true);
    }

    public void OnYesQuitClicked()
    {
        Debug.Log("Game is quitting...");
        StartCoroutine(YesQuitButtonClickedDelay(0.25f));
    }

    private IEnumerator YesQuitButtonClickedDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnNoQuitClicked()
    {
        isConfirmingQuit = false;
        StartCoroutine(NoQuitButtonClickedDelay(0.25f));
    }

    private IEnumerator NoQuitButtonClickedDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        areYouSure.SetActive(false);
        EnableMainMenuInteractions();
    }

    private void DisableMainMenuInteractions()
    {
        Button[] buttons = mainMenu.GetComponentsInChildren<Button>();
        foreach (Button button in buttons)
            button.interactable = false;
    }

    private void EnableMainMenuInteractions()
    {
        Button[] buttons = mainMenu.GetComponentsInChildren<Button>();
        foreach (Button button in buttons)
            button.interactable = true;
    }

    public void ShowLoadPanel()
    {
        StartCoroutine(ShowLoadPanelAfterDelay(0.25f));
    }

    public void HideLoadPanel()
    {
        StartCoroutine(HideLoadPanelAfterDelay(0.25f));
    }

    private IEnumerator ShowLoadPanelAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        loadPanel.SetActive(true);
        DisableMainMenuInteractions();
    }

    private IEnumerator HideLoadPanelAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        loadPanel.SetActive(false);
        EnableMainMenuInteractions();
    }
}