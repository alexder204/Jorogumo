using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class JournalManager : MonoBehaviour
{
    public static JournalManager instance;

    public GameObject journalUI;
    public TextMeshProUGUI noteTitleText;
    public TextMeshProUGUI noteContentText;

    private int currentPageIndex = 0;
    private List<JournalNote> collectedNotes = new List<JournalNote>();

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            ToggleJournal();
        }
    }

    public void FlipForward()
    {
        if (currentPageIndex < collectedNotes.Count - 1)
        {
            currentPageIndex++;
            DisplayNote(collectedNotes[currentPageIndex]);
        }
    }

    public void FlipBackward()
    {
        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            DisplayNote(collectedNotes[currentPageIndex]);
        }
    }

    public void AddNote(JournalNote note)
    {
        if (collectedNotes.Contains(note)) return;

        collectedNotes.Add(note);

        // If this is the first note, show it immediately
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
        // Optionally clear displayed text:
        noteTitleText.text = "";
        noteContentText.text = "";
    }

    public JournalNote GetNoteByID(string id)
    {
        return AllNoteDatabase.GetNoteByID(id);
    }



    public void ToggleJournal()
    {
        bool isActive = journalUI.activeSelf;
        journalUI.SetActive(!isActive);

        Time.timeScale = isActive ? 1f : 0f; // Pause/unpause game
    }

    public void DisplayNote(JournalNote note)
    {
        noteTitleText.text = note.noteTitle;
        noteContentText.text = note.noteText;
    }
}
