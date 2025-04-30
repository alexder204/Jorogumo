using UnityEngine;

public class Door : MonoBehaviour
{
    public Item keyItem;            // Drag the actual Item object needed to open the door
    public GameObject interactIcon; // Drag the interact icon here
    private bool isPlayerNearby = false;

    private void Start()
    {
        if (interactIcon != null)
            interactIcon.SetActive(false);
    }

    private void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            Interact();
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

    private void Interact()
    {
        // Check if the player has the key item
        if (Inventory.instance.items.Exists(item => item == keyItem && item.currentAmount > 0))
        {
            Debug.Log("You used the key to open the door!");
            Inventory.instance.UseItem(keyItem);  // Consume the key
            OpenDoor();                           // Open the door (destroy or animation)
        }
        else
        {
            Debug.Log("You need a key to open this door!");
        }
    }

    private void OpenDoor()
    {
        Debug.Log("Door is now open!");
        Destroy(gameObject); // This destroys the door object
    }
}
