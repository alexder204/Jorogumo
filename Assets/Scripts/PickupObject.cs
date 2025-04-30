using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [Header("Item Data")]
    [SerializeField] private Item item;  // This will be assigned automatically from the Item ScriptableObject
    [Header("UI References")]
    public GameObject interactPopUp;

    private bool isPlayerNearby = false;

    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            AttemptPickup();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        StartPickup();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        EndPickup();
    }

    private void StartPickup()
    {
        isPlayerNearby = true;
        interactPopUp.SetActive(true);
    }

    private void AttemptPickup()
    {
        bool wasPickedUp = Inventory.instance.Add(item);
        if (wasPickedUp)
        {
            EndPickup();
            Destroy(gameObject);  // Destroy the item after pickup
        }
        else
        {
            Debug.Log("Inventory full, cannot pick up item.");
        }
    }

    private void EndPickup()
    {
        if (!isPlayerNearby) return;

        isPlayerNearby = false;
        interactPopUp.SetActive(false);
    }
}
