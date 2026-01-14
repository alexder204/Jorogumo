using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveSystem : MonoBehaviour
{
    [Header("References")]
    public Transform playerTransform;
    public UniqueIDRegistry uniqueIDRegistry;

    [Header("Autosave")]
    public float autosaveInterval = 10f; // seconds
    public int autosaveSlot = 99;        // Reserved slot for autosave

    private Coroutine autosaveCoroutine;
    private bool isLoading = false;

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private string GetSlotPath(int slot) =>
        Application.persistentDataPath + $"/saveslot{slot}.json";

    private void FindPlayerTransform()
    {
        if (playerTransform != null) return;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("SaveSystem: Player object not found in scene.");
        }
    }

    private void FindRegistry()
    {
        if (uniqueIDRegistry == null)
            uniqueIDRegistry = FindAnyObjectByType<UniqueIDRegistry>();
    }

    private void ResetPauseState()
    {
        // Global unpause
        PauseManager.isGamePaused = false;
        Time.timeScale = 1f;

        // Try to hide pause menu UI if tagged
        GameObject pauseMenu = GameObject.FindWithTag("PauseMenu");
        if (pauseMenu != null)
            pauseMenu.SetActive(false);
    }

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------

    private void Start()
    {
        if (PendingLoadSlot.loadSlot != -1)
        {
            int slotToLoad = PendingLoadSlot.loadSlot;
            PendingLoadSlot.loadSlot = -1;
            LoadGame(slotToLoad);
        }

        StartAutosave();
    }

    // -------------------------------------------------------------------------
    // Autosave
    // -------------------------------------------------------------------------

    public void StartAutosave()
    {
        if (autosaveCoroutine != null)
            StopCoroutine(autosaveCoroutine);

        autosaveCoroutine = StartCoroutine(AutosaveLoop());
        Debug.Log("Autosave path: " + GetSlotPath(autosaveSlot));
    }

    private IEnumerator AutosaveLoop()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(autosaveInterval);

            if (PauseManager.isGamePaused || isLoading)
            {
                Debug.Log("SaveSystem: Skipping autosave (paused or loading).");
                continue;
            }

            SaveGame(autosaveSlot);
            Debug.Log($"SaveSystem: Autosaved at {System.DateTime.Now}");
        }
    }

    // -------------------------------------------------------------------------
    // Save
    // -------------------------------------------------------------------------

    public void SaveGame(int slot)
    {
        FindPlayerTransform();
        if (playerTransform == null) return;

        SaveData data = new SaveData
        {
            sceneName = SceneManager.GetActiveScene().name,
            playerPosX = playerTransform.position.x,
            playerPosY = playerTransform.position.y,
            playerPosZ = playerTransform.position.z,

            pickedUpIDs = ObjectStateTracker.Instance.GetPickedUpIDs(),
            usedInteractableIDs = ObjectStateTracker.Instance.GetUsedIDs(),

            inventory = new List<InventoryItemData>(),
            collectedJournalNotes = new List<SavedJournalNote>(),

            completedDialogueIDs = new List<string>(DialogueUIManager.Instance.GetCompletedDialogueIDs())
        };

        // Inventory
        foreach (var item in Inventory.instance.items)
        {
            data.inventory.Add(new InventoryItemData
            {
                itemId = item.id,
                currentAmount = item.currentAmount
            });
        }

        // Journal
        foreach (var note in JournalManager.instance.GetCollectedNotes())
        {
            data.collectedJournalNotes.Add(new SavedJournalNote
            {
                noteId = note.noteID
            });
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetSlotPath(slot), json);
        Debug.Log($"SaveSystem: Game saved to slot {slot}");
    }

    // -------------------------------------------------------------------------
    // Load
    // -------------------------------------------------------------------------

    public void LoadGame(int slot)
    {
        if (autosaveCoroutine != null)
            StopCoroutine(autosaveCoroutine);

        // Make sure the game is not paused and audio is unpaused
        PauseManager.ForceUnpause();

        isLoading = true;
        StartCoroutine(LoadGameCoroutine(slot));
    }

    public IEnumerator LoadGameCoroutine(int slot)
    {
        string path = GetSlotPath(slot);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"SaveSystem: No save file at {path}");
            isLoading = false;
            StartAutosave();
            yield break;
        }

        // 1. Read the save file
        string json = File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        // 2. Load the correct scene if needed
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene != data.sceneName)
        {
            Debug.Log($"SaveSystem: Loading saved scene '{data.sceneName}'");
            AsyncOperation loadOp = SceneManager.LoadSceneAsync(data.sceneName);

            while (!loadOp.isDone)
                yield return null;

            // Extra frame so everything initializes
            yield return null;
        }

        // 2.5. Ensure game is unpaused and pause menu closed
        ResetPauseState();

        // 3. Re-acquire references that exist in the scene
        FindPlayerTransform();
        if (playerTransform == null)
        {
            Debug.LogWarning("SaveSystem: Player transform not found after scene load.");
            isLoading = false;
            StartAutosave();
            yield break;
        }

        FindRegistry();
        if (uniqueIDRegistry == null)
        {
            Debug.LogWarning("SaveSystem: UniqueIDRegistry not found after scene load.");
            isLoading = false;
            StartAutosave();
            yield break;
        }

        // Cache list once instead of calling GetAllUniqueIDs repeatedly
        var uniqueObjects = uniqueIDRegistry.GetAllUniqueIDs();

        // 4. Restore player position
        playerTransform.position = new Vector3(data.playerPosX, data.playerPosY, data.playerPosZ);

        // 5. Restore object state
        ObjectStateTracker.Instance.Clear();

        foreach (string id in data.pickedUpIDs)
            ObjectStateTracker.Instance.MarkPickedUp(id);

        foreach (string id in data.usedInteractableIDs)
            ObjectStateTracker.Instance.MarkUsed(id);

        foreach (var obj in uniqueObjects)
        {
            bool picked = ObjectStateTracker.Instance.HasBeenPickedUp(obj.id);
            bool used = ObjectStateTracker.Instance.HasBeenUsed(obj.id);
            obj.gameObject.SetActive(!(picked || used));
        }

        // 6. Restore inventory
        Inventory.instance.ClearInventory();
        foreach (var saved in data.inventory)
        {
            var baseItem = ItemDatabase.GetItemByID(saved.itemId);
            if (baseItem != null)
            {
                Inventory.instance.AddItem(baseItem, saved.currentAmount);
            }
            else
            {
                Debug.LogWarning($"SaveSystem: Item ID '{saved.itemId}' not found in ItemDatabase");
            }
        }

        // after inventory restore
        yield return null; // let UI objects enable

        var uis = FindObjectsByType<InventoryUI>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var ui in uis)
            ui.UpdateUI();


        // 7. Restore dialogue
        DialogueUIManager.Instance.ClearCompletedDialogues();

        foreach (string dialogueId in data.completedDialogueIDs)
            DialogueUIManager.Instance.MarkDialogueCompleteById(dialogueId);

        // Enable all dialogue objects first
        foreach (var obj in uniqueObjects)
        {
            var dialogue = obj.GetComponent<DialogueID>();
            if (dialogue != null)
                obj.gameObject.SetActive(true);
        }

        // Then disable completed dialogue objects
        foreach (var obj in uniqueObjects)
        {
            var dialogue = obj.GetComponent<DialogueID>();
            if (dialogue != null &&
                DialogueUIManager.Instance.HasCompletedDialogue(dialogue))
            {
                obj.gameObject.SetActive(false);
                Debug.Log($"SaveSystem: Disabling completed dialogue object '{dialogue.id}'");
            }
        }

        // 8. Restore journal
        JournalManager.instance.ClearNotes();
        foreach (var savedNote in data.collectedJournalNotes)
        {
            JournalNote note = JournalManager.instance.GetNoteByID(savedNote.noteId);
            if (note != null)
            {
                JournalManager.instance.AddNote(note);
            }
            else
            {
                Debug.LogWarning($"SaveSystem: Saved note with ID '{savedNote.noteId}' not found.");
            }
        }

        if (JournalManager.instance.GetCollectedNotes().Count > 0)
        {
            JournalManager.instance.DisplayNote(
                JournalManager.instance.GetCollectedNotes()[0]
            );
        }

        Debug.Log($"SaveSystem: Game loaded from slot {slot}");

        isLoading = false;
        StartAutosave();
    }

    // -------------------------------------------------------------------------
    // Delete
    // -------------------------------------------------------------------------

    public void DeleteSave(int slot)
    {
        string path = GetSlotPath(slot);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"SaveSystem: Deleted save slot {slot}");
        }
        else
        {
            Debug.LogWarning($"SaveSystem: No save at slot {slot} to delete");
        }
    }
}
