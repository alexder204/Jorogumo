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
        Debug.Log("Autosave loop started");

        while (true)
        {
            Debug.Log("Started wait at: " + Time.realtimeSinceStartup);
            yield return new WaitForSecondsRealtime(autosaveInterval);
            Debug.Log("Finished wait at: " + Time.realtimeSinceStartup);

            Debug.Log("Autosave interval passed. Checking if paused...");
            if (!PauseManager.isGamePaused)
            {
                Debug.Log("Not paused, saving...");
                SaveGame(autosaveSlot);
                Debug.Log($"Autosaved to slot {autosaveSlot} at {System.DateTime.Now}");
            }
            else
            {
                Debug.Log("Paused — skipping autosave");
            }
        }
    }


    public void SaveGame(int slot)
    {
        FindPlayerTransform();

        if (playerTransform == null)
        {
            Debug.LogWarning("Player transform is not set. Cannot save.");
            return;
        }

        SaveData data = new SaveData
        {
            sceneName = SceneManager.GetActiveScene().name,
            playerPosX = playerTransform.position.x,
            playerPosY = playerTransform.position.y,
            playerPosZ = playerTransform.position.z
        };

        UniqueID[] allObjects = uniqueIDRegistry.GetAllUniqueIDs();
        foreach (var obj in allObjects)
        {
            var state = new ObjectState
            {
                id = obj.id,
                isActive = obj.gameObject.activeSelf,
                hasBeenPickedUp = false
            };

            PickupItem pickup = obj.GetComponent<PickupItem>();
            if (pickup != null)
            {
                state.hasBeenPickedUp = pickup.hasBeenPickedUp;
            }

            JournalNotePickup notePickup = obj.GetComponent<JournalNotePickup>();
            if (notePickup != null)
            {
                state.hasBeenPickedUp = notePickup.hasBeenPickedUp;
            }

            data.objectStates.Add(state);
        }

        foreach (var item in Inventory.instance.items)
        {
            data.inventory.Add(new InventoryItemData
            {
                itemId = item.id,
                currentAmount = item.currentAmount
            });
        }

        foreach (var note in JournalManager.instance.GetCollectedNotes())
        {
            data.collectedJournalNotes.Add(new SavedJournalNote
            {
                noteId = note.noteID
            });
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetSlotPath(slot), json);
        Debug.Log($"Game Saved to slot {slot}");
    }

    public void LoadGame(int slot)
    {
        StartCoroutine(LoadGameCoroutine(slot));
    }

    private IEnumerator LoadGameCoroutine(int slot)
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

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(data.sceneName);
        while (!asyncLoad.isDone)
            yield return null;

        yield return null;

        FindPlayerTransform();
        if (playerTransform == null)
        {
            Debug.LogWarning("Player transform not found after scene load.");
            yield break;
        }

        playerTransform.position = new Vector3(data.playerPosX, data.playerPosY, data.playerPosZ);

        uniqueIDRegistry.RefreshUniqueIDs();
        UniqueID[] allObjects = uniqueIDRegistry.GetAllUniqueIDs();

        foreach (var obj in allObjects)
        {
            ObjectState state = data.objectStates.Find(s => s.id == obj.id);
            if (state == null) continue;

            obj.gameObject.SetActive(state.isActive);

            PickupItem pickup = obj.GetComponent<PickupItem>();
            if (pickup != null)
            {
                pickup.hasBeenPickedUp = state.hasBeenPickedUp;

                if (pickup.hasBeenPickedUp)
                {
                    pickup.GetComponent<SpriteRenderer>()?.gameObject.SetActive(false);
                    pickup.GetComponent<Collider2D>()?.gameObject.SetActive(false);
                    pickup.interactPopUp?.SetActive(false);
                }
            }

            JournalNotePickup notePickup = obj.GetComponent<JournalNotePickup>();
            if (notePickup != null)
            {
                notePickup.hasBeenPickedUp = state.hasBeenPickedUp;
                notePickup.gameObject.SetActive(!state.hasBeenPickedUp);
            }
        }

        Inventory.instance.ClearInventory();
        foreach (var saved in data.inventory)
        {
            var baseItem = ItemDatabase.GetItemByID(saved.itemId);
            if (baseItem != null)
                Inventory.instance.AddItem(baseItem, saved.currentAmount);
            else
                Debug.LogWarning($"Item ID {saved.itemId} not found in ItemDatabase");
        }

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
                Debug.LogWarning($"Saved note with ID {savedNote.noteId} not found.");
            }
        }

        if (JournalManager.instance.GetCollectedNotes().Count > 0)
        {
            JournalManager.instance.DisplayNote(JournalManager.instance.GetCollectedNotes()[0]);
        }

        Debug.Log($"Game Loaded from slot {slot}");
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
