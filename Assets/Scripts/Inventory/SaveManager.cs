using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SaveSystem : MonoBehaviour
{
    public Transform playerTransform;  // Assign in Inspector
    public UniqueIDRegistry uniqueIDRegistry;  // Assign in Inspector
    private string savePath;

    private void Awake()
    {
        savePath = Application.persistentDataPath + "/savefile.json";
    }

    public void SaveGame()
    {
        SaveData data = new SaveData
        {
            playerPosX = playerTransform.position.x,
            playerPosY = playerTransform.position.y,
            playerPosZ = playerTransform.position.z
        };

        // Save all objects in registry (including inactive ones)
        UniqueID[] allObjects = uniqueIDRegistry.GetAllUniqueIDs();
        foreach (var obj in allObjects)
        {
            data.objectStates.Add(new ObjectState
            {
                id = obj.id,
                isActive = obj.gameObject.activeSelf
            });
        }

        // Save inventory
        foreach (Item item in Inventory.instance.items)
        {
            data.inventory.Add(new InventoryItemData
            {
                itemId = item.id,
                currentAmount = item.currentAmount
            });
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log("Game Saved to " + savePath);
    }

    public void LoadGame()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("No save file found at " + savePath);
            return;
        }

        string json = File.ReadAllText(savePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        // Restore player position
        playerTransform.position = new Vector3(data.playerPosX, data.playerPosY, data.playerPosZ);

        // Restore object active states (including inactive)
        UniqueID[] allObjects = uniqueIDRegistry.GetAllUniqueIDs();
        foreach (var obj in allObjects)
        {
            ObjectState state = data.objectStates.Find(s => s.id == obj.id);
            if (state != null)
            {
                obj.gameObject.SetActive(state.isActive);
            }
        }

        // Restore inventory
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

        Debug.Log("Game Loaded from " + savePath);
    }
}
