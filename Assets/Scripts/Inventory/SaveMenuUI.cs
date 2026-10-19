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

    [Header("Slot Labels (0�9 + autosave)")]
    public List<TextMeshProUGUI> manualSlotLabels; // Assign 10 labels in Inspector
    public TextMeshProUGUI autosaveLabel;          // Assign autosave TMP label
    [Tooltip("The whole autosave row, including its button and label.")]
    [SerializeField] private GameObject autosaveRow;
    [Header("Manual Slot Rows — Same Order As Labels")]
    [SerializeField] private List<GameObject> manualSlotRows = new List<GameObject>();

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

        // Ignore load requests if the file no longer exists.
        if (!isSaveMode &&
            !File.Exists(Application.persistentDataPath +
                $"/saveslot{slot}.json"))
        {
            UpdateSlotLabels();
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
        if (manualSlotLabels != null)
        {
            for (int i = 0; i < manualSlotLabels.Count; i++)
            {
                bool exists = File.Exists(
                    Application.persistentDataPath +
                    $"/saveslot{i}.json");

                if (manualSlotLabels[i] != null)
                {
                    manualSlotLabels[i].text = GetSlotDescription(i, $"Slot {i + 1}");
                }

                if (manualSlotRows != null &&
                    i < manualSlotRows.Count &&
                    manualSlotRows[i] != null)
                {
                    manualSlotRows[i].SetActive(isSaveMode || exists);
                }
            }
        }

        bool autosaveExists = File.Exists(
            Application.persistentDataPath + "/saveslot99.json");

        if (autosaveLabel != null)
        {
            autosaveLabel.text = GetSlotDescription(autosaveSlot, "Autosave");
        }

        if (autosaveRow != null)
            autosaveRow.SetActive(!isSaveMode && autosaveExists);
    }

    private void OnEnable()
    {
        UpdateSlotLabels();
    }

    public void SetSaveMode(bool saveMode)
    {
        isSaveMode = saveMode;
        UpdateSlotLabels();
    }

    private string GetSlotDescription(int slot, string title)
    {
        string path = Application.persistentDataPath +
            $"/saveslot{slot}.json";

        if (!File.Exists(path))
            return $"{title} - Empty";

        try
        {
            SaveData data = JsonUtility.FromJson<SaveData>(
                File.ReadAllText(path));

            if (data == null)
                return $"{title} - Unreadable save";

            string location =
                !string.IsNullOrWhiteSpace(data.sceneDisplayName)
                ? data.sceneDisplayName
                : data.sceneName;

            System.DateTimeOffset savedTime;

            if (!System.DateTimeOffset.TryParse(
                    data.savedAtUtc,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out savedTime))
            {
                // Older saves do not contain a timestamp.
                savedTime = new System.DateTimeOffset(
                    File.GetLastWriteTimeUtc(path));
            }

            string date = savedTime.ToLocalTime().ToString(
                "yyyy-MM-dd  HH:mm");

            return $"{title}\n{location}\n{date}";
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning(
                $"Cannot read slot {slot}: {exception.Message}", this);

            return $"{title} - Unreadable save";
        }
    }
}
