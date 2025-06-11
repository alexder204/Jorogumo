using System.Collections.Generic;
using UnityEngine;

public class PickedUpObjectsManager : MonoBehaviour
{
    public static PickedUpObjectsManager Instance { get; private set; }

    private HashSet<string> pickedUpIDs = new HashSet<string>();

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

    public void MarkPickedUp(string id)
    {
        if (!pickedUpIDs.Contains(id))
            pickedUpIDs.Add(id);
    }

    public bool HasBeenPickedUp(string id)
    {
        return pickedUpIDs.Contains(id);
    }

    public void Clear()
    {
        pickedUpIDs.Clear();
    }
}
