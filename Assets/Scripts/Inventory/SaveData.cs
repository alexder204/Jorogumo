using System;
using System.Collections.Generic;

[System.Serializable]
public class ObjectState
{
    public string id;
    public bool isActive;
}

[System.Serializable]
public class SaveData
{
    public float playerPosX;
    public float playerPosY;
    public float playerPosZ;
    public List<ObjectState> objectStates = new List<ObjectState>();
}