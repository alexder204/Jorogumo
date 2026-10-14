using UnityEngine;
using TMPro;
using System.Collections.Generic;

/*
 * PURPOSE:
 * Stores collected journal notes and displays their pages.
 * PauseManager controls opening, closing, and pausing.
 *
 * IMPORTANT VARIABLES:
 * instance         - Shared journal reference.
 * journalUI        - Journal panel.
 * noteTitleText    - Displays the current note title.
 * noteContentText  - Displays the current note contents.
 * currentPageIndex - Index of the displayed note.
 * collectedNotes   - Notes the player has collected.
 *
 * SETUP:
 * Keep this component on an always-active object.
 * Assign the journal panel and both text fields.
 */

public class JournalManager : MonoBehaviour
{
    public static JournalManager instance;

    public static bool IsJournalOpen =>
        GameUIController.isJournalOpen;

    [Header("Journal Display")]
    public GameObject journalUI;
    public TextMeshProUGUI noteTitleText;
    public TextMeshProUGUI noteContentText;

    private int currentPageIndex;

    private readonly List<JournalNote> collectedNotes =
        new List<JournalNote>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    public void SetVisible(bool visible)
    {
        if (journalUI == null)
            return;

        journalUI.SetActive(visible);

        if (!visible)
            return;

        if (collectedNotes.Count > 0)
        {
            currentPageIndex = Mathf.Clamp(
                currentPageIndex, 0, collectedNotes.Count - 1);

            DisplayNote(collectedNotes[currentPageIndex]);
        }
        else
        {
            if (noteTitleText != null)
                noteTitleText.text = "";

            if (noteContentText != null)
                noteContentText.text = "";
        }
    }

    // Retains the existing method for Inspector button connections.
    public void ToggleJournal()
    {
        if (GameUIController.Instance != null)
            GameUIController.Instance.ToggleJournal();
    }

    public void FlipForward()
    {
        if (currentPageIndex >= collectedNotes.Count - 1)
            return;

        currentPageIndex++;
        DisplayNote(collectedNotes[currentPageIndex]);
    }

    public void FlipBackward()
    {
        if (currentPageIndex <= 0)
            return;

        currentPageIndex--;
        DisplayNote(collectedNotes[currentPageIndex]);
    }

    public void AddNote(JournalNote note)
    {
        if (note == null || collectedNotes.Contains(note))
            return;

        collectedNotes.Add(note);

        if (collectedNotes.Count == 1)
        {
            currentPageIndex = 0;
            DisplayNote(note);
        }
    }

    public List<JournalNote> GetCollectedNotes()
    {
        return new List<JournalNote>(collectedNotes);
    }

    public void ClearNotes()
    {
        collectedNotes.Clear();
        currentPageIndex = 0;

        if (noteTitleText != null)
            noteTitleText.text = "";

        if (noteContentText != null)
            noteContentText.text = "";
    }

    public JournalNote GetNoteByID(string id)
    {
        return AllNoteDatabase.GetNoteByID(id);
    }

    public void DisplayNote(JournalNote note)
    {
        if (note == null)
            return;

        if (noteTitleText != null)
            noteTitleText.text = note.noteTitle;

        if (noteContentText != null)
            noteContentText.text = note.noteText;
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
}