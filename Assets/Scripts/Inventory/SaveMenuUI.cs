using UnityEngine;
using TMPro;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class SaveMenuUI : MonoBehaviour
{
    public GameObject saveSlotsMenu;
    public SaveSystem saveSystem;
    public bool isSaveMode = true; // Toggle this when switching between save/load

    public TextMeshProUGUI slot0Label;
    public TextMeshProUGUI slot1Label;
    public TextMeshProUGUI slot2Label;
    public TextMeshProUGUI slot3Label;

    private void Update()
    {
        // Close the save menu when pressing the Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            saveSlotsMenu.SetActive(false);
        }
    }

    public void ToggleSaveSlots()
    {
        if (!saveSlotsMenu.activeSelf)
        {
            StartCoroutine(ShowSaveSlotsWithDelay(0.25f));
        }
        else
        {
            saveSlotsMenu.SetActive(false);
        }
    }

    private IEnumerator ShowSaveSlotsWithDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        saveSlotsMenu.SetActive(true);
        UpdateSlotLabels();
    }

    public void OnClickSlot0() => HandleSlotPress(0);
    public void OnClickSlot1() => HandleSlotPress(1);
    public void OnClickSlot2() => HandleSlotPress(2);
    public void OnClickSlot3() => HandleSlotPress(3);

    private void HandleSlotPress(int slotIndex)
    {
        if (isSaveMode)
        {
            saveSystem.SaveGame(slotIndex);
            Debug.Log($"Saved to slot {slotIndex}");
        }
        else
        {
            saveSystem.LoadGame(slotIndex);
            Debug.Log($"Loaded from slot {slotIndex}");
        }

        StartCoroutine(HideSaveSlotsWithDelay(0.25f));
    }

    private IEnumerator HideSaveSlotsWithDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        saveSlotsMenu.SetActive(false);
        UpdateSlotLabels();
    }

    public void SetSaveMode(bool saveMode)
    {
        isSaveMode = saveMode;
        UpdateSlotLabels();
    }

    public void UpdateSlotLabels()
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
        string status = File.Exists(path) ? "Saved" : "Empty";
        label.text = $"SaveSlot{slot} - {status}";
    }

    public void OnDeleteSlot0()
    {
        saveSystem.DeleteSave(0);
        UpdateSlotLabels();
    }

    public void OnDeleteSlot1()
    {
        saveSystem.DeleteSave(1);
        UpdateSlotLabels();
    }

    public void OnDeleteSlot2()
    {
        saveSystem.DeleteSave(2);
        UpdateSlotLabels();
    }

    public void OnDeleteSlot3()
    {
        saveSystem.DeleteSave(3);
        UpdateSlotLabels();
    }
}
