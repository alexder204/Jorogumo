using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string id;  // <-- Add this!
    public string itemName;
    public Sprite icon;
    public int stackAmount = 10;
    public int currentAmount = 0;
    public bool isUsable = false;
}
