using UnityEngine;
using UnityEngine.SceneManagement;

public class UniqueIDRegistry : MonoBehaviour
{
    private UniqueID[] allUniqueIDs;

    public UniqueID[] GetAllUniqueIDs() => allUniqueIDs;

    void Awake()
    {
        RefreshUniqueIDs();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshUniqueIDs();
    }

    public void RefreshUniqueIDs()
    {
        // Find all UniqueID objects, including inactive, without sorting (faster)
        allUniqueIDs = Object.FindObjectsByType<UniqueID>(FindObjectsSortMode.None);

        // Optional: filter to only scene objects (exclude assets)
        var sceneObjects = new System.Collections.Generic.List<UniqueID>();
        foreach (var obj in allUniqueIDs)
        {
            if (obj.gameObject.scene.IsValid())
                sceneObjects.Add(obj);
        }
        allUniqueIDs = sceneObjects.ToArray();

        Debug.Log($"UniqueIDRegistry refreshed: found {allUniqueIDs.Length} UniqueID objects.");
    }
}
