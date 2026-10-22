using UnityEngine;
using TMPro;

[CreateAssetMenu(menuName = "Journal/Note")]
public class JournalNote : ScriptableObject
{
    public string noteID;
    public string noteTitle;

    [TextArea(5, 20)]
    public string noteText;

    [Header("Appearance")]
    public Sprite paperSprite;

    [Header("Writing Style")]
    public TMP_FontAsset writingFont;

    [Header("Text Size Overrides — 0 Uses Default")]
    [Min(0f)] public float titleFontSize;
    [Min(0f)] public float bodyFontSize;
}