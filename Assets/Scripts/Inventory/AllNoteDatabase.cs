using UnityEngine;

public static class AllNoteDatabase
{
    public static JournalNote GetNoteByID(string id)
    {
        var allNotes = Resources.LoadAll<JournalNote>("Notes"); // Make sure Notes are under Resources/Notes
        foreach (var note in allNotes)
        {
            if (note.noteID == id)
                return note;
        }
        Debug.LogWarning("Note with ID " + id + " not found.");
        return null;
    }
}
