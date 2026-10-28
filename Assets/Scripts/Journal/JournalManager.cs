using UnityEngine;
using TMPro;
using System.Collections.Generic;
using PlayerDialogue;
using UnityEngine.UI;

/*
 * PURPOSE:
 * Stores collected notes and displays two journal pages instantly.
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
 * Existing noteTitleText/noteContentText fields are the RIGHT page.
 * Assign separate left-page texts and optional note containers.
 * Note containers must contain only their paper and texts, not the book.
 * Keep the pickup reading panel separate from the journal.
 * Assign the separate reading panel and its text fields.
 * Connect its Close button to ClosePickedUpNote().
 */

public class JournalManager : MonoBehaviour
{
    public static JournalManager instance;

    public static bool IsJournalOpen => GameUIController.isJournalOpen;

    [Header("Journal Panel")]
    public GameObject journalUI;
    [Header("Right Page - Existing Fields")]
    public GameObject rightNoteContainer;
    public TextMeshProUGUI noteTitleText;
    public TextMeshProUGUI noteContentText;

    [Header("Left Page")]
    public GameObject leftNoteContainer;
    public TextMeshProUGUI leftNoteTitleText;
    public TextMeshProUGUI leftNoteContentText;

    [Header("Pickup Note Reading")]
    public GameObject noteReadingUI;
    public TextMeshProUGUI readingTitleText;
    public TextMeshProUGUI readingContentText;
    public TextMeshProUGUI readingStatusText;

    [Header("Page Arrows")]
    [SerializeField] private JournalArrow leftArrow;
    [SerializeField] private JournalArrow rightArrow;

    [Header("Note Paper Images")]
    [SerializeField] private Image leftPaperImage;
    [SerializeField] private Image rightPaperImage;
    [SerializeField] private Image readingPaperImage;

    // Remember each Image's original paper as its fallback.
    private Sprite defaultLeftPaper;
    private Sprite defaultRightPaper;
    private Sprite defaultReadingPaper;
    private bool paperDefaultsCached;

    // Store a separate fallback for every text field before applying note fonts.
    private readonly Dictionary<TMP_Text, TMP_FontAsset> defaultFonts = new Dictionary<TMP_Text, TMP_FontAsset>();

    private void CacheFont(TMP_Text text)
    {
        if (text != null && !defaultFonts.ContainsKey(text))
            defaultFonts.Add(text, text.font);
    }

    private void CacheFontDefaults()
    {
        CacheFont(noteTitleText);
        CacheFont(noteContentText);
        CacheFont(leftNoteTitleText);
        CacheFont(leftNoteContentText);
        CacheFont(readingTitleText);
        CacheFont(readingContentText);
    }

    private void ApplyNoteFont(TMP_Text text, JournalNote note)
    {
        if (text == null)
            return;

        CacheFont(text);
        text.font = note != null && note.writingFont != null ? note.writingFont : defaultFonts[text];
    }

    public bool HasNotes => collectedNotes.Count > 0;

    private bool journalUnlockHintPending;
    private int journalUnlockQueuedFrame;

    // Called by a successful world pickup only. AddNote stays silent for saves.
    public void NotifyFirstNoteCollected()
    {
        if (collectedNotes.Count != 1)
            return;

        journalUnlockHintPending = true;
        journalUnlockQueuedFrame = Time.frameCount;
    }

    private void LateUpdate()
    {
        if (!journalUnlockHintPending || !HasNotes || isReadingPickedUpNote || Time.frameCount == journalUnlockQueuedFrame || GameUIController.IsGameplayBlocked || UIManager.instance == null)
            return;

        journalUnlockHintPending = false;
        UIManager.instance.ShowMessage("You can now open the journal using J.");
    }

    private int currentPageIndex;
    private int readingOpenedFrame;
    private bool isReadingPickedUpNote;
    private Dialogue pendingReaction;

    private readonly List<JournalNote> collectedNotes = new List<JournalNote>();

    public bool CanDisplayPickedUpNote => noteReadingUI != null && readingTitleText != null && readingContentText != null;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        CacheFontDefaults();
        CachePaperDefaults();
        SetNoteReadingVisible(false);
    }

    private void CachePaperDefaults()
    {
        if (paperDefaultsCached)
            return;

        defaultLeftPaper = leftPaperImage != null ? leftPaperImage.sprite : null;
        defaultRightPaper = rightPaperImage != null ? rightPaperImage.sprite : null;
        defaultReadingPaper = readingPaperImage != null ? readingPaperImage.sprite : null;
        paperDefaultsCached = true;
    }

    private void ApplyPaper(Image paperImage, JournalNote note, Sprite fallback)
    {
        if (paperImage == null)
            return;

        paperImage.sprite = note != null && note.paperSprite != null ? note.paperSprite : fallback;
    }

    private void Update()
    {
        if (!isReadingPickedUpNote || GameUIController.CurrentScreen != GameUIController.MenuScreen.NoteReading || Time.frameCount == readingOpenedFrame || TopDownMovement.isFading || TopDownMovement.isSceneLoading)
            return;

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.J))
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

        RefreshJournalPages();
    }

    public void ToggleJournal()
    {
        if (GameUIController.Instance != null)
            GameUIController.Instance.ToggleJournal();
    }

    public void FlipForward()
    {
        if (currentPageIndex >= collectedNotes.Count)
            return;

        currentPageIndex++;
        RefreshJournalPages();
    }

    public void FlipBackward()
    {
        if (currentPageIndex <= 0)
            return;

        currentPageIndex--;
        RefreshJournalPages();
    }

    public void AddNote(JournalNote note)
    {
        if (note == null || collectedNotes.Contains(note))
            return;

        collectedNotes.Add(note);

        if (collectedNotes.Count == 1)
            currentPageIndex = 0;

        RefreshJournalPages();
    }

    public bool TryReadPickedUpNote(
        JournalNote note, Dialogue reaction)
    {
        if (note == null || !CanDisplayPickedUpNote || GameUIController.Instance == null || isReadingPickedUpNote)
        {
            Debug.LogWarning("Cannot open note: check the reading panel, " + "text fields, and GameUIController.", this);
            return false;
        }

        CacheFontDefaults();
        ApplyNoteFont(readingTitleText, note);
        ApplyNoteFont(readingContentText, note);

        readingTitleText.text = note.noteTitle;
        readingContentText.text = note.noteText;

        CachePaperDefaults();
        ApplyPaper(readingPaperImage, note, defaultReadingPaper);

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
        if (!isReadingPickedUpNote || GameUIController.CurrentScreen != GameUIController.MenuScreen.NoteReading || GameUIController.Instance == null || TopDownMovement.isFading || TopDownMovement.isSceneLoading)
            return;

        // Closing the menu clears pendingReaction, so capture it first.
        Dialogue reaction = pendingReaction;
        bool showUnlockHint = journalUnlockHintPending;

        GameUIController.Instance.CloseMenus();

        journalUnlockHintPending = showUnlockHint;

        // Starts synchronously: world input stays blocked throughout
        // the transition from reading to dialogue.
        if (reaction != null)
            reaction.PlayNoteReaction();
    }

    public void CancelPickedUpNote()
    {
        journalUnlockHintPending = false;
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
        if (GameUIController.CurrentScreen == GameUIController.MenuScreen.NoteReading && GameUIController.Instance != null)
        {
            GameUIController.Instance.CloseMenus();
        }

        CancelPickedUpNote();
        collectedNotes.Clear();
        currentPageIndex = 0;

        RefreshJournalPages();
    }

    public JournalNote GetNoteByID(string id)
    {
        return AllNoteDatabase.GetNoteByID(id);
    }

    // Retained for callers that select a collected note directly.
    // The selected note appears on the right with its predecessor on the left.
    public void DisplayNote(JournalNote note)
    {
        if (note == null)
            return;

        int index = collectedNotes.IndexOf(note);
        if (index >= 0)
        {
            currentPageIndex = index;
            RefreshJournalPages();
            return;
        }

        // Preserve the previous ability to display a note before collecting it.
        SetPage(leftNoteContainer, leftNoteTitleText, leftNoteContentText, null);
        SetPage(rightNoteContainer, noteTitleText, noteContentText, note);
    }

    private void RefreshJournalPages()
    {
        // Index 0: blank / note 1. Index Count: last note / blank.
        currentPageIndex = Mathf.Clamp(currentPageIndex, 0, collectedNotes.Count);

        JournalNote left = currentPageIndex > 0 ? collectedNotes[currentPageIndex - 1] : null;
        JournalNote right = currentPageIndex < collectedNotes.Count ? collectedNotes[currentPageIndex] : null;

        SetPage(leftNoteContainer, leftNoteTitleText, leftNoteContentText, left);
        SetPage(rightNoteContainer, noteTitleText, noteContentText, right);

        if (leftArrow != null)
            leftArrow.SetAvailable(currentPageIndex > 0);

        if (rightArrow != null)
            rightArrow.SetAvailable(currentPageIndex < collectedNotes.Count);
    }

    private void SetPage(
        GameObject container, TextMeshProUGUI title,
        TextMeshProUGUI content, JournalNote note)
    {
        CachePaperDefaults();

        CacheFontDefaults();
        ApplyNoteFont(title, note);
        ApplyNoteFont(content, note);

        if (title != null)
            title.text = note != null ? note.noteTitle : "";

        if (content != null)
            content.text = note != null ? note.noteText : "";

        if (container == leftNoteContainer)
            ApplyPaper(leftPaperImage, note, defaultLeftPaper);

        if (container == rightNoteContainer)
            ApplyPaper(rightPaperImage, note, defaultRightPaper);

        if (container != null)
            container.SetActive(note != null);
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