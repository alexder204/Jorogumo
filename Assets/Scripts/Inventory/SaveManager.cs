using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SaveSystem : MonoBehaviour
{
    public Transform playerTransform;  // Assign in Inspector
    public UniqueIDRegistry uniqueIDRegistry;  // Reference to the UniqueIDRegistry (assign in Inspector)
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

        // Get all UniqueID objects from the registry (which includes inactive objects)
        UniqueID[] allObjects = uniqueIDRegistry.GetAllUniqueIDs();

        foreach (var obj in allObjects)
        {
            ObjectState state = new ObjectState
            {
                id = obj.id,
                isActive = obj.gameObject.activeSelf
            };
            data.objectStates.Add(state);
        }

        // Save the data to a JSON file
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

        // Get all UniqueID objects from the registry (which includes inactive objects)
        UniqueID[] allObjects = uniqueIDRegistry.GetAllUniqueIDs();

        foreach (var obj in allObjects)
        {
            ObjectState state = data.objectStates.Find(s => s.id == obj.id);
            if (state != null)
            {
                obj.gameObject.SetActive(state.isActive);
            }
        }

        Debug.Log("Game Loaded from " + savePath);
    }
}
