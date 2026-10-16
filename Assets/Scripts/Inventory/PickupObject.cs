using UnityEngine;
using PlayerDialogue;

/*
 * PURPOSE:
 * Collects either an inventory item or a journal note.
 * Optionally opens a collected note immediately and plays
 * a character reaction after the player closes it.
 *
 * IMPORTANT VARIABLES:
 * pickupType            - Item or journal note.
 * item                  - Inventory item to collect.
 * note                  - Journal note to collect.
 * readImmediately       - Opens the note after collection.
 * reactionAfterReading  - Optional separate Manual dialogue.
 * interactPopUp         - Existing shared player popup.
 * uniqueID              - Existing save-state identifier.
 * isPlayerNearby        - Whether interaction is available.
 *
 * SETUP:
 * Attach to the pickup object with its existing trigger.
 * Keep its UniqueID and ID value unchanged.
 * Assign either an item or a note based on pickupType.
 * Put reaction dialogue on a separate active GameObject.
 */

public class PickupItem : MonoBehaviour
{
    public enum PickupType
    {
        InventoryItem,
        JournalNote
    }

    [Header("Pickup Type")]
    [SerializeField] private PickupType pickupType =
        PickupType.InventoryItem;

    [Header("Inventory Item")]
    [SerializeField] private Item item;

    [Header("Journal Note")]
    [SerializeField] private JournalNote note;
    [SerializeField] private bool readImmediately = true;
    [SerializeField] private Dialogue reactionAfterReading;

    [Header("Interaction")]
    public GameObject interactPopUp;

    private bool isPlayerNearby;
    private UniqueID uniqueID;

    private void Awake()
    {
        uniqueID = GetComponent<UniqueID>();
    }

    private void Update()
    {
        if (GameUIController.IsGameplayBlocked)
            return;

        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
            AttemptPickup();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        isPlayerNearby = true;

        if (interactPopUp != null)
            interactPopUp.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        isPlayerNearby = false;

        if (interactPopUp != null)
            interactPopUp.SetActive(false);
    }

    private void AttemptPickup()
    {
        if (pickupType == PickupType.InventoryItem)
            PickUpInventoryItem();
        else
            PickUpJournalNote();
    }

    private void PickUpInventoryItem()
    {
        if (item == null || Inventory.instance == null)
        {
            Debug.LogWarning(
                "Pickup needs an item and an Inventory instance.", this);
            return;
        }

        if (!Inventory.instance.Add(item))
        {
            Debug.Log("Inventory full.", this);
            return;
        }

        MarkPickedUp();

        if (UIManager.instance != null)
            UIManager.instance.ShowMessage($"Picked up {item.itemName}!");

        DisablePickup();
    }

    private void PickUpJournalNote()
    {
        if (note == null || JournalManager.instance == null)
        {
            Debug.LogWarning(
                "Pickup needs a note and a JournalManager.", this);
            return;
        }

        bool wasLocked = !JournalManager.instance.HasNotes;

        if (readImmediately)
        {
            if (!JournalManager.instance.TryReadPickedUpNote(
                    note, reactionAfterReading))
                return;
        }
        else
        {
            JournalManager.instance.AddNote(note);

            if (!wasLocked && UIManager.instance != null)
                UIManager.instance.ShowMessage(
                    $"Note added: {note.noteTitle}");
        }

        if (wasLocked && JournalManager.instance.HasNotes)
            JournalManager.instance.NotifyFirstNoteCollected();

        MarkPickedUp();
        DisablePickup();
    }

    private void MarkPickedUp()
    {
        if (uniqueID != null &&
            ObjectStateTracker.Instance != null)
        {
            ObjectStateTracker.Instance.MarkPickedUp(uniqueID.id);
        }
    }

    private void DisablePickup()
    {
        if (interactPopUp != null)
            interactPopUp.SetActive(false);

        isPlayerNearby = false;
        gameObject.SetActive(false);
    }
}