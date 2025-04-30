using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;
    public List<Item> items = new List<Item>();  // List of items in the inventory
    public int maxSpace = 20;  // Maximum number of different items
    public delegate void OnItemChanged();
    public OnItemChanged onItemChangedCallback;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of Inventory found!");
            return;
        }
        instance = this;
    }

    public bool Add(Item item)
    {
        // Check if the item already exists and can stack
        foreach (Item invItem in items)
        {
            if (invItem == item && invItem.currentAmount < invItem.stackAmount)
            {
                invItem.currentAmount++;  // Stack the item
                onItemChangedCallback?.Invoke();
                return true;  // Item stacked
            }
        }

        // If there's space and the item doesn't exist, add a new item
        if (items.Count >= maxSpace)
        {
            Debug.Log("Not enough room.");
            return false;  // Inventory full
        }

        // Add new item to the inventory
        item.currentAmount = 1;
        items.Add(item);
        onItemChangedCallback?.Invoke();
        return true;
    }

    public void Remove(Item item)
    {
        items.Remove(item);
        onItemChangedCallback?.Invoke();
    }

    public void UseItem(Item item)
    {
        if (item.isUsable)
        {
            Debug.Log($"Used item: {item.itemName}");
            // Example of item usage: heal the player, etc.
            item.currentAmount--;

            if (item.currentAmount <= 0)
            {
                Remove(item);  // Remove item if it's fully used
            }

            onItemChangedCallback?.Invoke();
        }
    }
}
