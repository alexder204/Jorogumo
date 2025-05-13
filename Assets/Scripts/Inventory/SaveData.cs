using System;
using System.Collections.Generic;

[System.Serializable]
public class ObjectState
{
    public string id;
    public bool isActive;
    public bool hasBeenPickedUp;
}

[System.Serializable]
public class InventoryItemData
{
    public string itemId;
    public int currentAmount;
}

[System.Serializable]
public class SaveData
{
    public float playerPosX;
    public float playerPosY;
    public float playerPosZ;
    public List<ObjectState> objectStates = new List<ObjectState>();
    public List<InventoryItemData> inventory = new List<InventoryItemData>();
}

