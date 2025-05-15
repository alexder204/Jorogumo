using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistBetweenScenes : MonoBehaviour
{
    private static GameObject[] persistentObjects = new GameObject[5];
    public int objectIndex;

    void Awake()
    {
        if (persistentObjects[objectIndex] == null)
        {
            persistentObjects[objectIndex] = gameObject;
            DontDestroyOnLoad(gameObject);
        }
        else if (persistentObjects[objectIndex] != gameObject)
        {
            Destroy(gameObject);
        }
    }
}