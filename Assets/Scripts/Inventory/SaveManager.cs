using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SaveManager : MonoBehaviour
{
    public Transform player;
    public Inventory inventory;

    private static string savePath => Application.persistentDataPath + "/save.json";

    public void SaveGame()
    {
        SaveData data = new SaveData();

        // Save player position
        Vector3 pos = player.position;
        data.playerPosition = new float[] { pos.x, pos.y, pos.z };

        // Save inventory items
        foreach (Item item in inventory.items)
        {
            data.inventory.Add(new InventoryItemData { id = item.id, amount = item.currentAmount });
        }

        // Save disabled objects using FindObjectsByType
        var allObjects = UnityEngine.Object.FindObjectsByType<GameObject>(UnityEngine.FindObjectsSortMode.None);
        foreach (GameObject obj in allObjects)
        {
            if (!obj.activeInHierarchy)
            {
                PickupItem pickupItem = obj.GetComponent<PickupItem>();
                if (pickupItem != null)
                {
                    data.disabledObjectIDs.Add(pickupItem.GetUniqueID());
                }
            }
        }

        // Save timestamp
        data.saveTimestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        // Write to disk
        File.WriteAllText(savePath, JsonUtility.ToJson(data, true));
        Debug.Log("Game saved to: " + savePath);
    }

    public void LoadGame()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("No save file found.");
            return;
        }

        SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(savePath));

        // Load player position
        player.position = new Vector3(data.playerPosition[0], data.playerPosition[1], data.playerPosition[2]);

        // Load inventory
        inventory.items.Clear();
        foreach (var savedItem in data.inventory)
        {
            Item item = ItemDatabase.GetItemByID(savedItem.id);
            if (item != null)
            {
                Item newItem = Instantiate(item);
                newItem.currentAmount = savedItem.amount;
                inventory.Add(newItem);
            }
        }

        // Restore disabled objects by unique ID
        foreach (string uniqueID in data.disabledObjectIDs)
        {
            PickupItem pickup = FindPickupByID(uniqueID);
            if (pickup != null)
            {
                pickup.gameObject.SetActive(false); // Disable the pickup item in the scene
            }
        }

        Debug.Log("Game loaded. Last saved at: " + data.saveTimestamp);
    }

    private PickupItem FindPickupByID(string uniqueID)
    {
        // Find the PickupItem with the matching unique ID
        PickupItem[] pickups = FindObjectsOfType<PickupItem>();
        foreach (PickupItem pickup in pickups)
        {
            if (pickup.GetUniqueID() == uniqueID)
            {
                return pickup;
            }
        }
        return null;
    }
}
