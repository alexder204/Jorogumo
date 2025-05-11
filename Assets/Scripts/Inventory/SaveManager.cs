using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SaveSystem : MonoBehaviour
{
    public Transform playerTransform;          // Assign in Inspector
    public UniqueIDRegistry uniqueIDRegistry;  // Assign in Inspector
    private string GetSlotPath(int slot) => Application.persistentDataPath + $"/saveslot{slot}.json";

    public void SaveGame(int slot)
    {
        SaveData data = new SaveData
        {
            playerPosX = playerTransform.position.x,
            playerPosY = playerTransform.position.y,
            playerPosZ = playerTransform.position.z
        };

        UniqueID[] allObjects = uniqueIDRegistry.GetAllUniqueIDs();
        foreach (var obj in allObjects)
        {
            data.objectStates.Add(new ObjectState
            {
                id = obj.id,
                isActive = obj.gameObject.activeSelf
            });
        }

        foreach (Item item in Inventory.instance.items)
        {
            data.inventory.Add(new InventoryItemData
            {
                itemId = item.id,
                currentAmount = item.currentAmount
            });
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetSlotPath(slot), json);
        Debug.Log($"Game Saved to slot {slot}");
    }

    public void LoadGame(int slot)
    {
        string path = GetSlotPath(slot);
        if (!File.Exists(path))
        {
            Debug.LogWarning("No save file found at " + path);
            return;
        }

        string json = File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        playerTransform.position = new Vector3(data.playerPosX, data.playerPosY, data.playerPosZ);

        UniqueID[] allObjects = uniqueIDRegistry.GetAllUniqueIDs();
        foreach (var obj in allObjects)
        {
            ObjectState state = data.objectStates.Find(s => s.id == obj.id);
            if (state != null)
            {
                obj.gameObject.SetActive(state.isActive);
            }
        }

        Inventory.instance.ClearInventory();
        foreach (var savedItem in data.inventory)
        {
            Item baseItem = ItemDatabase.GetItemByID(savedItem.itemId);
            if (baseItem != null)
            {
                Inventory.instance.AddItem(baseItem, savedItem.currentAmount);
            }
            else
            {
                Debug.LogWarning($"Item with ID {savedItem.itemId} not found in database.");
            }
        }

        Debug.Log($"Game Loaded from slot {slot}");
    }
}
