using System.Collections.Generic;
using UnityEngine;

public class PickedUpObjectsManager : MonoBehaviour
{
    public static PickedUpObjectsManager Instance { get; private set; }

    private HashSet<string> pickedUpIDs = new HashSet<string>();
    private HashSet<string> usedInteractableIDs = new HashSet<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ========== PICKUPS ==========
    public void MarkPickedUp(string id)
    {
        if (!pickedUpIDs.Contains(id))
            pickedUpIDs.Add(id);
    }

    public bool HasBeenPickedUp(string id)
    {
        return pickedUpIDs.Contains(id);
    }

    // ========== INTERACTABLES ==========
    public void MarkUsed(string id)
    {
        if (!usedInteractableIDs.Contains(id))
            usedInteractableIDs.Add(id);
    }

    public bool HasBeenUsed(string id)
    {
        return usedInteractableIDs.Contains(id);
    }

    public void Clear()
    {
        pickedUpIDs.Clear();
        usedInteractableIDs.Clear();
    }

    public List<string> GetPickedUpIDs()
    {
        return new List<string>(pickedUpIDs);
    }

    public List<string> GetUsedIDs()
    {
        return new List<string>(usedInteractableIDs);
    }

}
