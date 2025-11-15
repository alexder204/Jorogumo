using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class JournalManager : MonoBehaviour
{
    public static JournalManager instance;
    public static bool IsJournalOpen =>
        instance != null &&
        instance.journalUI != null &&
        instance.journalUI.activeSelf;

    [Header("UI")]
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
        // If dialogue starts while journal is open → auto-close
        if (TopDownMovement.isInDialogue && journalUI.activeSelf)
        {
            CloseJournal();
            return;
        }

        // Do NOT allow opening journal during dialogue
        if (TopDownMovement.isInDialogue)
            return;

        // Toggle journal with J (open or close)
        if (Input.GetKeyDown(KeyCode.J))
        {
            // Cannot open journal while paused
            if (!journalUI.activeSelf && PauseManager.isGamePaused)
                return;

            ToggleJournal();
        }
    }

    public void ToggleJournal()
    {
        if (journalUI.activeSelf)
            CloseJournal();
        else
            OpenJournal();
    }

    private void OpenJournal()
    {
        journalUI.SetActive(true);
        PauseManager.isJournalOpen = true;

        // Pause the game
        PauseManager.isGamePaused = true;
        Time.timeScale = 0f;

        PauseAllAudio();
    }

    private void CloseJournal()
    {
        journalUI.SetActive(false);
        PauseManager.isJournalOpen = false;

        // Unpause the game
        PauseManager.isGamePaused = false;
        Time.timeScale = 1f;

        UnpauseAllAudio();
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
        if (note == null || collectedNotes.Contains(note)) return;

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

        if (noteTitleText != null) noteTitleText.text = "";
        if (noteContentText != null) noteContentText.text = "";
    }

    public JournalNote GetNoteByID(string id)
    {
        return AllNoteDatabase.GetNoteByID(id);
    }

    public void DisplayNote(JournalNote note)
    {
        if (note == null) return;

        noteTitleText.text = note.noteTitle;
        noteContentText.text = note.noteText;
    }

    private void PauseAllAudio()
    {
        var sources = Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (var src in sources)
        {
            if (src.enabled && src.gameObject.activeInHierarchy)
                src.Pause();
        }
    }

    private void UnpauseAllAudio()
    {
        var sources = Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (var src in sources)
        {
            if (src.enabled && src.gameObject.activeInHierarchy)
                src.UnPause();
        }
    }
}
