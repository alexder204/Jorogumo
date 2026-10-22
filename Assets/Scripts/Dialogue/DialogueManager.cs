using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class DialogueUIManager : MonoBehaviour
{
    public static DialogueUIManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject dialogueCanvas;
    public GameObject charImage1;
    public GameObject charImage2;
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI dialogueText;
    public Image portraitImage1;
    public Image portraitImage2;

    private HashSet<string> completedDialogues = new HashSet<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool HasCompletedDialogue(DialogueID dialogue)
    {
        return completedDialogues.Contains(dialogue.id);
    }

    public void MarkDialogueComplete(DialogueID dialogue)
    {
        if (!completedDialogues.Contains(dialogue.id))
            completedDialogues.Add(dialogue.id);
    }

    public List<string> GetCompletedDialogueIDs()
    {
        return new List<string>(completedDialogues);
    }

    // Also add to clear all, useful on load:
    public void ClearCompletedDialogues()
    {
        completedDialogues.Clear();
    }

    // Add a method to mark complete by ID string:
    public void MarkDialogueCompleteById(string dialogueId)
    {
        if (!completedDialogues.Contains(dialogueId))
            completedDialogues.Add(dialogueId);
    }

}
