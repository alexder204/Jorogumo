using UnityEngine;

[CreateAssetMenu(menuName = "Journal/Note")]
public class JournalNote : ScriptableObject
{
    public string noteID;
    public string noteTitle;
    [TextArea(5, 20)] public string noteText;
}
