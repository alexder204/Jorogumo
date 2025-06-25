using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;

public class SaveSystem : MonoBehaviour
{
    public Transform playerTransform;
    public UniqueIDRegistry uniqueIDRegistry;

    public float autosaveInterval = 10f; // 10 minutes = 600 seconds
    public int autosaveSlot = 99;         // Reserved slot for autosave
    private Coroutine autosaveCoroutine;

    private bool isLoading = false;

    private string GetSlotPath(int slot) =>
        Application.persistentDataPath + $"/saveslot{slot}.json";

    private void FindPlayerTransform()
    {
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("Player object not found in scene.");
            }
        }
    }

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
                Debug.Log("Skipping autosave because game is paused or loading");
                continue;
            }

            SaveGame(autosaveSlot);
            Debug.Log($"Autosaved at {System.DateTime.Now}");
        }
    }


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

            pickedUpIDs = PickedUpObjectsManager.Instance.GetPickedUpIDs(),
            usedInteractableIDs = PickedUpObjectsManager.Instance.GetUsedIDs(),

            inventory = new List<InventoryItemData>(),
            collectedJournalNotes = new List<SavedJournalNote>()
        };

        foreach (var item in Inventory.instance.items)
        {
            data.inventory.Add(new InventoryItemData { itemId = item.id, currentAmount = item.currentAmount });
        }

        foreach (var note in JournalManager.instance.GetCollectedNotes())
        {
            data.collectedJournalNotes.Add(new SavedJournalNote { noteId = note.noteID });
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetSlotPath(slot), json);
        Debug.Log($"Game Saved to slot {slot}");
    }


    public void LoadGame(int slot)
    {
        if (autosaveCoroutine != null)
            StopCoroutine(autosaveCoroutine);

        isLoading = true;
        StartCoroutine(LoadGameCoroutine(slot));
    }

    public IEnumerator LoadGameCoroutine(int slot)
    {
        FindPlayerTransform();

        string path = GetSlotPath(slot);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"No save file at {path}");
            yield break;
        }

        string json = File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        yield return null; // wait a frame

        FindPlayerTransform();
        if (playerTransform == null)
        {
            Debug.LogWarning("Player transform not found.");
            yield break;
        }

        playerTransform.position = new Vector3(data.playerPosX, data.playerPosY, data.playerPosZ);

        // Clear & restore picked/used IDs
        PickedUpObjectsManager.Instance.Clear();

        foreach (string id in data.pickedUpIDs)
            PickedUpObjectsManager.Instance.MarkPickedUp(id);

        foreach (string id in data.usedInteractableIDs)
            PickedUpObjectsManager.Instance.MarkUsed(id);

        // Immediately disable picked/used objects
        foreach (var obj in uniqueIDRegistry.GetAllUniqueIDs())
        {
            if (PickedUpObjectsManager.Instance.HasBeenPickedUp(obj.id) ||
                PickedUpObjectsManager.Instance.HasBeenUsed(obj.id))
            {
                obj.gameObject.SetActive(false);
            }
            else
            {
                obj.gameObject.SetActive(true);
            }
        }

        // Inventory
        Inventory.instance.ClearInventory();
        foreach (var saved in data.inventory)
        {
            var baseItem = ItemDatabase.GetItemByID(saved.itemId);
            if (baseItem != null)
                Inventory.instance.AddItem(baseItem, saved.currentAmount);
            else
                Debug.LogWarning($"Item ID {saved.itemId} not found in ItemDatabase");
        }

        // Journal
        JournalManager.instance.ClearNotes();
        foreach (var savedNote in data.collectedJournalNotes)
        {
            JournalNote note = JournalManager.instance.GetNoteByID(savedNote.noteId);
            if (note != null)
                JournalManager.instance.AddNote(note);
            else
                Debug.LogWarning($"Saved note with ID {savedNote.noteId} not found.");
        }

        if (JournalManager.instance.GetCollectedNotes().Count > 0)
        {
            JournalManager.instance.DisplayNote(JournalManager.instance.GetCollectedNotes()[0]);
        }

        Debug.Log($"Game Loaded from slot {slot}");

        yield return null;

        StartAutosave();
    }


    public void DeleteSave(int slot)
    {
        string path = GetSlotPath(slot);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"Deleted save slot {slot}");
        }
        else
        {
            Debug.LogWarning($"No save at slot {slot} to delete");
        }
    }
}
