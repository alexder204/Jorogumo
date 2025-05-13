using UnityEngine;

public class UniqueID : MonoBehaviour
{
    public string id;

    [HideInInspector]
    public bool defaultActiveState;

    private void Awake()
    {
        defaultActiveState = gameObject.activeSelf;
    }
}
