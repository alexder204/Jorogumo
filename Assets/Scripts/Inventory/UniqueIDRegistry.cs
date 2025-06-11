using UnityEngine.SceneManagement;
using UnityEngine;

public class UniqueIDRegistry : MonoBehaviour
{
    public static UniqueIDRegistry Instance { get; private set; }

    private UniqueID[] allUniqueIDs;

    public UniqueID[] GetAllUniqueIDs() => allUniqueIDs;


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // Unsubscribe before destroying
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshUniqueIDs();
    }

    public void RefreshUniqueIDs()
    {
        allUniqueIDs = Object.FindObjectsByType<UniqueID>(FindObjectsSortMode.None);

        var sceneObjects = new System.Collections.Generic.List<UniqueID>();
        foreach (var obj in allUniqueIDs)
        {
            if (obj.gameObject.scene.IsValid())
            {
                sceneObjects.Add(obj);

                if (PickedUpObjectsManager.Instance != null && PickedUpObjectsManager.Instance.HasBeenPickedUp(obj.id))
                {
                    obj.gameObject.SetActive(false);
                }
                else
                {
                    // Optional: restore default active state
                    obj.gameObject.SetActive(obj.defaultActiveState);
                }
            }
        }
        allUniqueIDs = sceneObjects.ToArray();

        Debug.Log($"UniqueIDRegistry refreshed: found {allUniqueIDs.Length} UniqueID objects.");
    }

}
