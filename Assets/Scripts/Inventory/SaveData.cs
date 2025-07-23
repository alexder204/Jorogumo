using System;
using System.Collections.Generic;

[System.Serializable]
public class ObjectState
{
    public string id;
    public bool isActive;
    public bool hasBeenPickedUp;
}

[System.Serializable]
public class InventoryItemData
{
    public string itemId;
    public int currentAmount;
}

[System.Serializable]
public class SavedJournalNote
{
    public string noteId;  // Assuming JournalNote has a unique ID or title string
}


[System.Serializable]
public class SaveData
{
    public string sceneName;

    public float playerPosX;
    public float playerPosY;
    public float playerPosZ;

    // Store only IDs of used/picked objects
    public List<string> pickedUpIDs = new List<string>();
    public List<string> usedInteractableIDs = new List<string>();

    // Inventory & Journal as before
    public List<InventoryItemData> inventory = new List<InventoryItemData>();
    public List<SavedJournalNote> collectedJournalNotes = new List<SavedJournalNote>();

    public List<string> completedDialogueIDs = new List<string>();
}