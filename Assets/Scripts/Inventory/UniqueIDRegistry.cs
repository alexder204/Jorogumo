using UnityEngine;
using System.Collections.Generic;

public class UniqueIDRegistry : MonoBehaviour
{
    [SerializeField] private UniqueID[] allUniqueIDs;

    public UniqueID[] GetAllUniqueIDs() => allUniqueIDs;
}
