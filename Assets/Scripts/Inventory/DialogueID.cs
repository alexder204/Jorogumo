using UnityEngine;

public class DialogueID : MonoBehaviour
{
    public string id;

    [HideInInspector]
    public bool defaultActiveState;

    private void Awake()
    {
        defaultActiveState = gameObject.activeSelf;
    }
}
