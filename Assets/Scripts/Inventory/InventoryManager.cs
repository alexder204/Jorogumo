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
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Add items to inventory, stacking them if possible
    public bool Add(Item item)
    {
        if (item == null)
        {
            Debug.LogWarning("Inventory.Add called with null item.");
            return false;
        }

        // Try to stack with existing item of same ID
        foreach (Item invItem in items)
        {
            if (invItem.id == item.id && invItem.currentAmount < invItem.stackAmount)
            {
                invItem.currentAmount++;
                onItemChangedCallback?.Invoke();
                return true;
            }
        }

        // No stack found: need a new slot
        if (items.Count >= maxSpace)
        {
            Debug.Log("Inventory: Not enough room.");
            return false;
        }

        // Clone ScriptableObject so we don't mutate the asset
        Item itemClone = ScriptableObject.Instantiate(item);
        itemClone.currentAmount = 1;
        items.Add(itemClone);

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
        if (item == null || amount <= 0)
            return;

        // First, try to stack onto existing stacks
        foreach (Item invItem in items)
        {
            if (invItem.id != item.id)
                continue;

            int spaceLeft = invItem.stackAmount - invItem.currentAmount;
            if (spaceLeft <= 0)
                continue;

            int toAdd = Mathf.Min(spaceLeft, amount);
            invItem.currentAmount += toAdd;
            amount -= toAdd;

            if (amount <= 0)
            {
                onItemChangedCallback?.Invoke();
                return;
            }
        }

        // Then create new stacks as needed
        while (amount > 0)
        {
            if (items.Count >= maxSpace)
            {
                Debug.Log("Inventory: Not enough room while loading/adding stack.");
                break;
            }

            Item clone = ScriptableObject.Instantiate(item);
            int toAdd = Mathf.Min(clone.stackAmount, amount);
            clone.currentAmount = toAdd;
            items.Add(clone);

            amount -= toAdd;
        }

        onItemChangedCallback?.Invoke();
    }
}
