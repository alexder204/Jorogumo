using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SaveSystem : MonoBehaviour
{
    public Transform playerTransform;
    public UniqueIDRegistry uniqueIDRegistry;

    private string GetSlotPath(int slot) =>
        Application.persistentDataPath + $"/saveslot{slot}.json";

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

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetSlotPath(slot), json);
        Debug.Log($"Game Saved to slot {slot}");
    }

    public void LoadGame(int slot)
    {
        string path = GetSlotPath(slot);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"No save file at {path}");
            return;
        }

        string json = File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        // Restore player position
        playerTransform.position = new Vector3(data.playerPosX, data.playerPosY, data.playerPosZ);

        // Restore all tracked objects
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

                if (state.hasBeenPickedUp)
                {
                    // Make it look picked up
                    var sprite = pickup.GetComponent<SpriteRenderer>();
                    if (sprite != null) sprite.enabled = false;

                    var coll = pickup.GetComponent<Collider2D>();
                    if (coll != null) coll.enabled = false;

                    if (pickup.interactPopUp != null)
                        pickup.interactPopUp.SetActive(false);
                }
                else
                {
                    // Fully re-enable
                    var sprite = pickup.GetComponent<SpriteRenderer>();
                    if (sprite != null) sprite.enabled = true;

                    var coll = pickup.GetComponent<Collider2D>();
                    if (coll != null) coll.enabled = true;

                    if (pickup.interactPopUp != null)
                        pickup.interactPopUp.SetActive(false);
                }
            }
        }

        // Restore inventory
        Inventory.instance.ClearInventory();
        foreach (var saved in data.inventory)
        {
            var baseItem = ItemDatabase.GetItemByID(saved.itemId);
            if (baseItem != null)
                Inventory.instance.AddItem(baseItem, saved.currentAmount);
            else
                Debug.LogWarning($"Item ID {saved.itemId} not found in ItemDatabase");
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
