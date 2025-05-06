using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string id;  // Unique identifier for the item
    public string itemName;
    public Sprite icon;
    public int stackAmount = 10;
    public int currentAmount = 0;
    public bool isUsable = false;
}
