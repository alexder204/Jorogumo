using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string itemName;  // Name of the item
    public Sprite icon;  // Icon for UI representation
    public int stackAmount = 10;  // Maximum stackable amount
    public int currentAmount = 0;  // Current stack in the inventory
    public bool isUsable = false;  // Determines if the item can be used (e.g., consumable)
}