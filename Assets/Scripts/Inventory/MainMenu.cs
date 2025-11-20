using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.IO;
using TMPro;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenu;
    public GameObject settingsMenu;
    public GameObject areYouSure;
    public GameObject loadPanel;

    [Header("Save Slot Labels (0–9)")]
    public List<TextMeshProUGUI> slotLabels;   // assign 10 labels in inspector
    public TextMeshProUGUI autosaveLabel;

    [SerializeField] private string newLevel;

    private bool isConfirmingQuit = false;

    private const int ManualSlotCount = 10;    // slots 0..9

    private void Start()
    {
        if (mainMenu != null) mainMenu.SetActive(true);
        if (areYouSure != null) areYouSure.SetActive(false);
        if (settingsMenu != null) settingsMenu.SetActive(false);

        // Safety check: do we actually have 10 labels?
        if (slotLabels == null || slotLabels.Count != ManualSlotCount)
        {
            Debug.LogWarning(
                $"MainMenu: Expected {ManualSlotCount} slot labels, " +
                $"but found {(slotLabels == null ? 0 : slotLabels.Count)}. " +
                $"Slots and labels may be out of sync."
            );
        }

        UpdateSaveSlotLabels();
    }

    // =========================
    // NEW GAME
    // =========================
    public void LoadSceneByName()
    {
        if (SceneFader.instance != null)
            SceneFader.instance.FadeOutAndLoad(newLevel);
        else
            SceneManager.LoadScene(newLevel);
    }

    // =========================
    // CONTINUE (AUTOSAVE 99)
    // =========================
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
            Debug.LogWarning($"MainMenu: No autosave found at {path}");
        }
    }

    // =========================
    // LOAD SPECIFIC MANUAL SLOT
    // =========================
    public void LoadSlot(int slot)
    {
        string path = Application.persistentDataPath + $"/saveslot{slot}.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);
            PendingLoadSlot.loadSlot = slot;

            Debug.Log($"MainMenu: Loading slot {slot} from {path}");
            SceneFader.instance.FadeOutAndLoad(saveData.sceneName);
        }
        else
        {
            Debug.LogWarning($"MainMenu: No save in slot {slot} at path {path}");
        }
    }

    // =========================
    // UPDATE SLOT LABELS
    // =========================
    private void UpdateSaveSlotLabels()
    {
        // update normal slots
        for (int i = 0; i < slotLabels.Count; i++)
            UpdateSlotLabel(slotLabels[i], i);

        // update autosave separately
        if (autosaveLabel != null)
            UpdateSlotLabel(autosaveLabel, 99);
    }

    private void UpdateSlotLabel(TextMeshProUGUI label, int slot)
    {
        if (label == null) return;

        string path = Application.persistentDataPath + $"/saveslot{slot}.json";
        bool exists = File.Exists(path);

        if (slot == 99)
            label.text = $"Autosave - {(exists ? "Saved" : "Empty")}";
        else
            label.text = $"Save Slot {slot} - {(exists ? "Saved" : "Empty")}";
    }

    // =========================
    // SETTINGS
    // =========================
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

    // =========================
    // QUIT
    // =========================
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

    // =========================
    // BUTTON INTERACTABILITY
    // =========================
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

    // =========================
    // LOAD PANEL
    // =========================
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
