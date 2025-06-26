using UnityEngine;

public class JournalNotePickup : MonoBehaviour
{
    [Header("Note Data")]
    [SerializeField] private JournalNote note;

    [Header("UI")]
    public GameObject interactPopup;

    private bool isPlayerNearby = false;
    private SpriteRenderer spriteRenderer;
    private Collider2D noteCollider;

    private UniqueID uniqueID;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        noteCollider = GetComponent<Collider2D>();
        uniqueID = GetComponent<UniqueID>();
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
        interactPopup?.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        isPlayerNearby = false;
        interactPopup?.SetActive(false);
    }

    private void PickupNote()
    {
        JournalManager.instance.AddNote(note);

        if (uniqueID != null)
            ObjectStateTracker.Instance.MarkPickedUp(uniqueID.id);

        UIManager.instance.ShowMessage($"Note added: {note.noteTitle}");

        gameObject.SetActive(false);
    }
}
