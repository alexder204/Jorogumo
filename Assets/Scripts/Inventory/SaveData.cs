using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public float[] playerPosition;
    public List<InventoryItemData> inventory = new();
    public List<string> disabledObjectIDs = new(); // Changed from disabledObjectPaths
    public string saveTimestamp;
}

[Serializable]
public class InventoryItemData
{
    public string id;
    public int amount;
}
