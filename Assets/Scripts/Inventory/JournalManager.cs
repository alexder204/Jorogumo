using UnityEngine;
using TMPro;
using System.Collections.Generic;
using PlayerDialogue;

/*
 * PURPOSE:
 * Stores collected notes and displays the normal journal.
 * Also displays a newly collected note and optionally starts
 * a character reaction when the player closes it.
 *
 * IMPORTANT VARIABLES:
 * journalUI          - Existing journal panel.
 * noteTitleText      - Existing journal title field.
 * noteContentText    - Existing journal content field.
 * noteReadingUI      - Separate pickup reading panel.
 * readingTitleText   - Pickup reading title field.
 * readingContentText - Pickup reading content field.
 * readingStatusText  - Optional "Note added" label.
 * pendingReaction    - Dialogue started after closing the note.
 * collectedNotes     - Notes used by the existing save system.
 *
 * SETUP:
 * Keep this component on an always-active object.
 * Keep existing journal references assigned.
 * Assign the separate reading panel and its text fields.
 * Connect its Close button to ClosePickedUpNote().
 */

public class JournalManager : MonoBehaviour
{
    public static JournalManager instance;

    public static bool IsJournalOpen =>
        GameUIController.isJournalOpen;

    [Header("Existing Journal Display")]
    public GameObject journalUI;
    public TextMeshProUGUI noteTitleText;
    public TextMeshProUGUI noteContentText;

    [Header("Pickup Note Reading")]
    public GameObject noteReadingUI;
    public TextMeshProUGUI readingTitleText;
    public TextMeshProUGUI readingContentText;
    public TextMeshProUGUI readingStatusText;

    private int currentPageIndex;
    private int readingOpenedFrame;
    private bool isReadingPickedUpNote;
    private Dialogue pendingReaction;

    private readonly List<JournalNote> collectedNotes =
        new List<JournalNote>();

    public bool CanDisplayPickedUpNote =>
        noteReadingUI != null &&
        readingTitleText != null &&
        readingContentText != null;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        SetNoteReadingVisible(false);
    }

    private void Update()
    {
        if (!isReadingPickedUpNote ||
            GameUIController.CurrentScreen !=
                GameUIController.MenuScreen.NoteReading ||
            Time.frameCount == readingOpenedFrame ||
            TopDownMovement.isFading ||
            TopDownMovement.isSceneLoading)
            return;

        // Escape is handled by GameUIController.
        // Ignore simultaneous menu shortcuts here.
        if (Input.GetKeyDown(KeyCode.Escape) ||
            Input.GetKeyDown(KeyCode.I) ||
            Input.GetKeyDown(KeyCode.J))
            return;

        if (Input.GetButtonDown("Interact"))
            ClosePickedUpNote();
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

    public bool TryReadPickedUpNote(
        JournalNote note, Dialogue reaction)
    {
        if (note == null ||
            !CanDisplayPickedUpNote ||
            GameUIController.Instance == null ||
            isReadingPickedUpNote)
        {
            Debug.LogWarning(
                "Cannot open note: check the reading panel, " +
                "text fields, and GameUIController.", this);

            return false;
        }

        readingTitleText.text = note.noteTitle;
        readingContentText.text = note.noteText;

        if (readingStatusText != null)
            readingStatusText.text = $"Note added: {note.noteTitle}";

        pendingReaction = reaction;
        readingOpenedFrame = Time.frameCount;
        isReadingPickedUpNote = true;

        if (!GameUIController.Instance.OpenNoteReading())
        {
            CancelPickedUpNote();
            return false;
        }

        AddNote(note);
        return true;
    }

    public void SetNoteReadingVisible(bool visible)
    {
        if (noteReadingUI != null)
            noteReadingUI.SetActive(visible);
    }

    public void ClosePickedUpNote()
    {
        if (!isReadingPickedUpNote ||
            GameUIController.CurrentScreen !=
                GameUIController.MenuScreen.NoteReading ||
            GameUIController.Instance == null ||
            TopDownMovement.isFading ||
            TopDownMovement.isSceneLoading)
            return;

        // Closing the menu clears pendingReaction, so capture it first.
        Dialogue reaction = pendingReaction;

        GameUIController.Instance.CloseMenus();

        // Starts synchronously: world input stays blocked throughout
        // the transition from reading to dialogue.
        if (reaction != null)
            reaction.PlayNoteReaction();
    }

    public void CancelPickedUpNote()
    {
        pendingReaction = null;
        isReadingPickedUpNote = false;
        SetNoteReadingVisible(false);
    }

    public List<JournalNote> GetCollectedNotes()
    {
        return new List<JournalNote>(collectedNotes);
    }

    public void ClearNotes()
    {
        if (GameUIController.CurrentScreen ==
                GameUIController.MenuScreen.NoteReading &&
            GameUIController.Instance != null)
        {
            GameUIController.Instance.CloseMenus();
        }

        CancelPickedUpNote();
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
        {
            CancelPickedUpNote();
            instance = null;
        }
    }
}