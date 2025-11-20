using UnityEngine;
using TMPro;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class SaveMenuUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject saveSlotsMenu;
    public SaveSystem saveSystem;
    public bool isSaveMode = true;

    [Header("Slot Labels (0–9 + autosave)")]
    public List<TextMeshProUGUI> manualSlotLabels; // Assign 10 labels in Inspector
    public TextMeshProUGUI autosaveLabel;          // Assign autosave TMP label

    private const int manualSlotCount = 10; // slots 0–9
    private const int autosaveSlot = 99;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            saveSlotsMenu.SetActive(false);
    }

    public void ToggleSaveSlots()
    {
        if (!saveSlotsMenu.activeSelf)
            StartCoroutine(ShowSaveSlots(0.25f));
        else
            saveSlotsMenu.SetActive(false);
    }

    private IEnumerator ShowSaveSlots(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        saveSlotsMenu.SetActive(true);
        UpdateSlotLabels();
    }

    // ============================
    // HANDLE SLOT PRESSED
    // ============================

    public void OnClickSlot(int slot)
    {
        if (slot == autosaveSlot && isSaveMode)
        {
            Debug.LogWarning("Autosave slot is read-only!");
            return;
        }

        if (isSaveMode)
            saveSystem.SaveGame(slot);
        else
            saveSystem.LoadGame(slot);

        StartCoroutine(HideAndRefresh(0.25f));
    }

    private IEnumerator HideAndRefresh(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        saveSlotsMenu.SetActive(false);
        UpdateSlotLabels();
    }

    // ============================
    // DELETE SLOT
    // ============================

    public void DeleteSlot(int slot)
    {
        saveSystem.DeleteSave(slot);
        UpdateSlotLabels();
    }

    // ============================
    // UPDATE LABELS
    // ============================

    public void UpdateSlotLabels()
    {
        // Manual slots: 0–9
        for (int i = 0; i < manualSlotCount; i++)
        {
            string path = Application.persistentDataPath + $"/saveslot{i}.json";
            string status = File.Exists(path) ? "Saved" : "Empty";
            manualSlotLabels[i].text = $"Slot {i} - {status}";
        }

        // Autosave slot (99)
        string autoPath = Application.persistentDataPath + "/saveslot99.json";
        autosaveLabel.text = $"Autosave - {(File.Exists(autoPath) ? "Saved" : "Empty")}";
    }

    public void SetSaveMode(bool saveMode)
    {
        isSaveMode = saveMode;
        UpdateSlotLabels();
    }
}
