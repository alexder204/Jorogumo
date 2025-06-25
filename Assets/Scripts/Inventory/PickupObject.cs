using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [Header("Item Data")]
    [SerializeField] private Item item;

    [Header("UI References")]
    public GameObject interactPopUp;

    private bool isPlayerNearby = false;
    private SpriteRenderer spriteRenderer;
    private Collider2D collider2d;

    private UniqueID uniqueID;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        collider2d = GetComponent<Collider2D>();
        uniqueID = GetComponent<UniqueID>();
    }

    private void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            AttemptPickup();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        isPlayerNearby = true;
        interactPopUp?.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        isPlayerNearby = false;
        interactPopUp?.SetActive(false);
    }

    private void AttemptPickup()
    {
        bool wasPickedUp = Inventory.instance.Add(item);
        if (wasPickedUp)
        {
            if (uniqueID != null)
                PickedUpObjectsManager.Instance.MarkPickedUp(uniqueID.id);

            UIManager.instance.ShowMessage($"Picked up {item.itemName}!");

            gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("Inventory full, cannot pick up item.");
        }
    }
}
