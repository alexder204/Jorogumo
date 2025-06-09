using UnityEngine;

public class JournalNotePickup : MonoBehaviour
{
    [Header("Note Data")]
    [SerializeField] private JournalNote note;  // This is a ScriptableObject or custom class

    [Header("UI")]
    public GameObject interactPopup;

    private bool isPlayerNearby = false;
    private SpriteRenderer spriteRenderer;
    private Collider2D noteCollider;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        noteCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            PickupNote();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        isPlayerNearby = true;
        interactPopup.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        isPlayerNearby = false;
        interactPopup.SetActive(false);
    }

    private void PickupNote()
    {
        // Add the full JournalNote object
        JournalManager.instance.AddNote(note);

        spriteRenderer.enabled = false;
        noteCollider.enabled = false;

        interactPopup.SetActive(false);
        isPlayerNearby = false;

        UIManager.instance.ShowMessage($"Note added: {note.noteTitle}");

        gameObject.SetActive(false);
    }
}
