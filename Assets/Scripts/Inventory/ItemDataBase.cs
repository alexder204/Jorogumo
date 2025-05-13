using UnityEngine;

public static class ItemDatabase
{
    public static Item GetItemByID(string id)
    {
        var allItems = Resources.LoadAll<Item>("Items"); // Assumes all ScriptableObjects are in a Resources folder
        foreach (Item item in allItems)
        {
            if (item.id == id)
                return item;
        }
        Debug.LogWarning("Item with ID " + id + " not found.");
        return null;
    }
}
