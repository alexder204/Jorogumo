using UnityEngine;
using System.Collections;

public class PickupItem : MonoBehaviour
{
    [Header("Item Data")]
    [SerializeField] private Item item;  // Assigned automatically from the Item ScriptableObject

    [Header("UI References")]
    public GameObject interactPopUp;

    private bool isPlayerNearby = false;
    private SpriteRenderer spriteRenderer;
    private Collider2D collider2d;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        collider2d = GetComponent<Collider2D>();
    }

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
            spriteRenderer.enabled = false;
            collider2d.enabled = false;

            UIManager.instance.ShowMessage($"Picked up {item.itemName}!");

            StartCoroutine(DisableAfterDelay(2f));
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

    private IEnumerator DisableAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);  // Disable the object after a delay
    }
}
