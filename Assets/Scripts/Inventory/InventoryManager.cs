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
        if (instance != null && instance != this)
        {
            return;
        }
        instance = this;
    }

    // Add items to inventory, stacking them if possible
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

    // Remove item from inventory
    public void Remove(Item item)
    {
        items.Remove(item);
        onItemChangedCallback?.Invoke();
    }

    // Use an item from the inventory
    public void UseItem(Item item)
    {
        if (item.isUsable)
        {
            string usedItemName = item.itemName;  // Save the name first

            item.currentAmount--;  // Reduce the quantity of the item in the inventory

            if (item.currentAmount <= 0)
            {
                Remove(item); // Remove item if its count reaches 0
            }

            onItemChangedCallback?.Invoke();  // Update the UI

            // Show message using UIManager
            UIManager.instance.ShowMessage($"Used item: {usedItemName}");
        }
    }

    public void ClearInventory()
    {
        items.Clear();
        onItemChangedCallback?.Invoke();
    }

    public void AddItem(Item item, int amount)
    {
        Item itemClone = ScriptableObject.Instantiate(item);
        itemClone.currentAmount = amount;
        items.Add(itemClone);
        onItemChangedCallback?.Invoke();
    }
}
