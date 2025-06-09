using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

public class JournalManager : MonoBehaviour
{
    public static JournalManager instance;

    public GameObject journalUI;
    public TextMeshProUGUI noteTitleText;
    public TextMeshProUGUI noteContentText;
    public Transform notesListContainer;
    public GameObject noteButtonPrefab;

    private List<JournalNote> collectedNotes = new List<JournalNote>();

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void AddNote(JournalNote note)
    {
        if (collectedNotes.Contains(note)) return;

        collectedNotes.Add(note);

        GameObject buttonObj = Instantiate(noteButtonPrefab, notesListContainer);
        buttonObj.GetComponentInChildren<TMP_Text>().text = note.noteTitle;

        buttonObj.GetComponent<Button>().onClick.AddListener(() =>
        {
            DisplayNote(note);
        });
    }

    public void ToggleJournal()
    {
        bool isActive = journalUI.activeSelf;
        journalUI.SetActive(!isActive);

        Time.timeScale = isActive ? 1f : 0f; // Pause/unpause game
    }

    private void DisplayNote(JournalNote note)
    {
        noteTitleText.text = note.noteTitle;
        noteContentText.text = note.noteText;
    }
}
