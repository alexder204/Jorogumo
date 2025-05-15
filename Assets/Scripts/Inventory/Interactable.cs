using UnityEngine;

public class Interactable : MonoBehaviour
{
    public InteractableType type;          // Choose the type in Inspector
    [HideInInspector] public Item requiredItem;  // Item only needed if LockedDoor
    public GameObject interactIcon;
    private bool isPlayerNearby = false;

    public enum InteractableType
    {
        Door,
        LockedDoor
        // Add more types easily later!
    }

    private void Start()
    {
        if (interactIcon != null)
            interactIcon.SetActive(false);
    }

    private void Update()
    {
        if (TopDownMovement.isInDialogue) return;
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        isPlayerNearby = true;
        if (interactIcon != null)
            interactIcon.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        isPlayerNearby = false;
        if (interactIcon != null)
            interactIcon.SetActive(false);
    }

    private void TryInteract()
    {
        if (type == InteractableType.LockedDoor)
        {
            // ✅ Use ID comparison instead of instance reference
            Item itemToUse = Inventory.instance.items.Find(item =>
                item.id == requiredItem.id && item.currentAmount > 0);

            if (itemToUse != null)
            {
                Inventory.instance.UseItem(itemToUse);
                PerformAction();
            }
            else
            {
                ShowMessage($"You need a {requiredItem?.itemName ?? "key"} to interact!");
            }
        }
        else
        {
            PerformAction();
        }
    }

    private void ShowMessage(string message)
    {
        UIManager.instance.ShowMessage(message);  // Use UIManager to show message
    }

    private void PerformAction()
    {
        switch (type)
        {
            case InteractableType.Door:
            case InteractableType.LockedDoor:
                gameObject.SetActive(false);  // Deactivates the object
                break;

            default:
                Debug.Log("Default interaction.");
                break;
        }
    }
}
