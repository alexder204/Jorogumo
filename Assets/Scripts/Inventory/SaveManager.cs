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
    public bool autosaveOnSceneChange = true;
    public float autosaveAfterSceneLoadDelay = 0.2f;
    public int autosaveSlot = 99;        // Reserved slot for autosave

    [Header("Autosave Scene Blocking")]
    public List<string> scenesWithoutAutosave = new List<string>
    {
        "MainMenu",
        "Cutscene1"
    };

    private bool suppressNextSceneAutosave = false;

    private Coroutine autosaveCoroutine;
    private bool isLoading = false;

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private string GetSlotPath(int slot) =>
        Application.persistentDataPath + $"/saveslot{slot}.json";

    private void FindPlayerTransform(bool force = false)
    {
        // Unity "fake null" handling: destroyed objects compare == null
        if (!force && playerTransform != null)
            return;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            playerTransform = playerObj.transform;
        else
            playerTransform = null;
    }

    private void FindRegistry()
    {
        if (uniqueIDRegistry == null)
            uniqueIDRegistry = FindAnyObjectByType<UniqueIDRegistry>();
    }

    private void ResetPauseState()
    {
        // Global unpause
        GameUIController.isGamePaused = false;
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
            return;
        }

        StartAutosave();

        if (autosaveOnSceneChange)
        {
            Scene activeScene = SceneManager.GetActiveScene();

            if (!scenesWithoutAutosave.Contains(activeScene.name))
            {
                if (autosaveCoroutine != null)
                    StopCoroutine(autosaveCoroutine);

                autosaveCoroutine = StartCoroutine(AutosaveAfterSceneLoad());
            }
        }
    }

    // -------------------------------------------------------------------------
    // Autosave
    // -------------------------------------------------------------------------

    public void StartAutosave()
    {
        // Stop old timer coroutine if it exists
        if (autosaveCoroutine != null)
        {
            StopCoroutine(autosaveCoroutine);
            autosaveCoroutine = null;
        }

        // Subscribe once
        SceneManager.sceneLoaded -= OnSceneLoaded_Autosave; // safety
        SceneManager.sceneLoaded += OnSceneLoaded_Autosave;

        Debug.Log("Autosave (scene-change) path: " + GetSlotPath(autosaveSlot));
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded_Autosave;
    }

    private void OnSceneLoaded_Autosave(Scene scene, LoadSceneMode mode)
    {
        if (!autosaveOnSceneChange) return;
        if (GameUIController.isGamePaused || isLoading) return;

        if (scenesWithoutAutosave.Contains(scene.name))
        return;

        if (suppressNextSceneAutosave)
        {
            suppressNextSceneAutosave = false;
            return;
        }

        if (autosaveCoroutine != null)
            StopCoroutine(autosaveCoroutine);

        autosaveCoroutine = StartCoroutine(AutosaveAfterSceneLoad());
    }

    private IEnumerator AutosaveAfterSceneLoad()
    {
        yield return new WaitForSecondsRealtime(autosaveAfterSceneLoadDelay);

        FindPlayerTransform(force: true);
        int safetyFrames = 120;
        while (playerTransform == null && safetyFrames-- > 0)
        {
            yield return null;
            FindPlayerTransform(force: true);
        }

        if (playerTransform == null)
        {
            Debug.LogWarning("SaveSystem: Autosave skipped (player not found).");
            yield break;
        }

        SaveGame(autosaveSlot);
        Debug.Log($"SaveSystem: Autosaved on scene load '{SceneManager.GetActiveScene().name}' at {System.DateTime.Now}");
    }

    // -------------------------------------------------------------------------
    // Save
    // -------------------------------------------------------------------------

    public void SaveGame(int slot)
    {
        FindPlayerTransform(force: true);
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

        data.sceneDisplayName = GetSceneDisplayName(data.sceneName);
        data.savedAtUtc = System.DateTime.UtcNow.ToString(
            "O", System.Globalization.CultureInfo.InvariantCulture);

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetSlotPath(slot), json);
        // Remove the previous image so a failed capture cannot show an old location.
        string previewPath = GetPreviewPath(slot);

        if (File.Exists(previewPath))
            File.Delete(previewPath);

        StartCoroutine(CaptureSavePreview(slot, json));
        Debug.Log($"SaveSystem: Game saved to slot {slot}");
    }

    [System.Serializable]
    public class SceneDisplayName
    {
        public string sceneName;
        public string displayName;
    }

    [Header("Save Location Names")]
    [SerializeField] private List<SceneDisplayName> sceneDisplayNames =
        new List<SceneDisplayName>();

    private string GetSceneDisplayName(string sceneName)
    {
        foreach (var entry in sceneDisplayNames)
        {
            if (entry != null &&
                entry.sceneName == sceneName &&
                !string.IsNullOrWhiteSpace(entry.displayName))
            {
                return entry.displayName;
            }
        }

        return sceneName;
    }

    private bool capturingPreview;

    private string GetPreviewPath(int slot) =>
        Application.persistentDataPath + $"/saveslot{slot}.png";

    private IEnumerator CaptureSavePreview(int slot, string savedJson)
    {
        if (capturingPreview)
            yield break;

        capturingPreview = true;

        var hiddenCanvases = new List<Canvas>();
        Texture2D screenshot = null;
        Texture2D thumbnail = null;
        RenderTexture target = null;

        RenderTexture previousTarget = RenderTexture.active;
        var sceneHandle = SceneManager.GetActiveScene().handle;

        try
        {
            // Hide rendering only. Do not deactivate menu GameObjects.
            foreach (Canvas canvas in
                    FindObjectsByType<Canvas>(FindObjectsInactive.Exclude))
            {
                if (canvas.enabled)
                {
                    hiddenCanvases.Add(canvas);
                    canvas.enabled = false;
                }
            }

            yield return new WaitForEndOfFrame();

            try
            {
                string savePath = GetSlotPath(slot);

                // Cancel if the scene changed, or this save was deleted/replaced.
                if (SceneManager.GetActiveScene().handle != sceneHandle ||
                    !File.Exists(savePath) ||
                    File.ReadAllText(savePath) != savedJson)
                {
                    yield break;
                }

                screenshot = ScreenCapture.CaptureScreenshotAsTexture();

                // Small preview, preserving the screen's aspect ratio.
                int width = Mathf.Min(480, screenshot.width);
                int height = Mathf.Max(1, Mathf.RoundToInt(
                    width * (float)screenshot.height / screenshot.width));

                target = RenderTexture.GetTemporary(
                    width, height, 0, RenderTextureFormat.ARGB32);

                Graphics.Blit(screenshot, target);
                RenderTexture.active = target;

                thumbnail = new Texture2D(
                    width, height, TextureFormat.RGB24, false);

                thumbnail.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                thumbnail.Apply();

                File.WriteAllBytes(
                    GetPreviewPath(slot), thumbnail.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previousTarget;

                if (target != null)
                    RenderTexture.ReleaseTemporary(target);

                if (screenshot != null)
                    Destroy(screenshot);

                if (thumbnail != null)
                    Destroy(thumbnail);
            }
        }
        finally
        {
            foreach (Canvas canvas in hiddenCanvases)
            {
                if (canvas != null)
                    canvas.enabled = true;
            }

            capturingPreview = false;
        }
    }

    // -------------------------------------------------------------------------
    // Load
    // -------------------------------------------------------------------------

    public void LoadGame(int slot)
    {
        if (autosaveCoroutine != null)
            StopCoroutine(autosaveCoroutine);

        // Make sure the game is not paused and audio is unpaused
        GameUIController.ForceUnpause();

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
            suppressNextSceneAutosave = true;
            AsyncOperation loadOp = SceneManager.LoadSceneAsync(data.sceneName);

            while (!loadOp.isDone)
                yield return null;

            // Extra frame so everything initializes
            yield return null;
        }

        // 2.5. Ensure game is unpaused and pause menu closed
        ResetPauseState();

        // 3. Re-acquire references that exist in the scene
        FindPlayerTransform(force: true);
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

        var uis = FindObjectsByType<InventoryUI>(FindObjectsInactive.Exclude);
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

        string previewPath = GetPreviewPath(slot);

        if (File.Exists(previewPath))
            File.Delete(previewPath);
    }
}
