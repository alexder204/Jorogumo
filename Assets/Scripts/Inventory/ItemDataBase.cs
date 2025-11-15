using System.Collections.Generic;
using UnityEngine;

public static class ItemDatabase
{
    private static Dictionary<string, Item> cache;

    private static void BuildCache()
    {
        cache = new Dictionary<string, Item>();

        Item[] allItems = Resources.LoadAll<Item>("Items");

        foreach (Item item in allItems)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.id))
                continue;

            if (!cache.ContainsKey(item.id))
            {
                cache.Add(item.id, item);
            }
            else
            {
                Debug.LogWarning($"ItemDatabase: Duplicate item id '{item.id}' (asset: {item.name}).");
            }
        }
    }

    public static Item GetItemByID(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            Debug.LogWarning("ItemDatabase: GetItemByID called with null/empty id.");
            return null;
        }

        if (cache == null)
            BuildCache();

        if (cache.TryGetValue(id, out Item item))
            return item;

        Debug.LogWarning($"ItemDatabase: Item with ID '{id}' not found.");
        return null;
    }
}