using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public Transform itemsParent;
    public GameObject slotPrefab;  // Slot prefab to instantiate
    public GameObject inventoryUI;

    private Inventory inventory;

    void Start()
    {
        inventory = Inventory.instance;
        inventory.onItemChangedCallback += UpdateUI;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))  // Toggle inventory UI
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        inventoryUI.SetActive(!inventoryUI.activeSelf);
    }

    public void UpdateUI()
    {
        if (itemsParent == null || slotPrefab == null)
        {
            Debug.LogWarning("InventoryUI not fully initialized yet. Skipping UI update.");
            return;
        }

        // Clear existing slots
        foreach (Transform child in itemsParent)
        {
            Destroy(child.gameObject);
        }

        // Add new slots based on the inventory items
        foreach (Item item in inventory.items)
        {
            GameObject newSlot = Instantiate(slotPrefab, itemsParent);
            InventorySlot slot = newSlot.GetComponent<InventorySlot>();
            slot.AddItem(item);
        }
    }
}
