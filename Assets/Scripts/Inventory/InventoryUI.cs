using UnityEngine;

/*
 * PURPOSE:
 * Draws inventory slots and refreshes them when items change.
 * PauseManager controls whether the inventory screen is visible.
 *
 * IMPORTANT VARIABLES:
 * itemsParent   - Container where slots are created.
 * slotPrefab    - Prefab containing InventorySlot.
 * inventoryUI   - Inventory panel.
 * itemDetailsUI - Displays the selected item's details.
 * inventory     - Inventory data reference.
 *
 * SETUP:
 * Keep this component on an always-active object.
 * Assign its panel, slot container, prefab, and details component.
 */

public class InventoryUI : MonoBehaviour
{
    [Header("Inventory Display")]
    public Transform itemsParent;
    public GameObject slotPrefab;
    public GameObject inventoryUI;
    public ItemDetailsUI itemDetailsUI;

    private Inventory inventory;

    private void OnEnable()
    {
        ConnectInventory();
    }

    private void Start()
    {
        // Retry after other objects have completed Awake.
        ConnectInventory();
    }

    private void ConnectInventory()
    {
        Inventory currentInventory = Inventory.instance;

        if (currentInventory == null)
            return;

        if (inventory == currentInventory)
            return;

        if (inventory != null)
            inventory.onItemChangedCallback -= UpdateUI;

        inventory = currentInventory;
        inventory.onItemChangedCallback += UpdateUI;

        UpdateUI();
    }

    private void OnDisable()
    {
        if (inventory != null)
            inventory.onItemChangedCallback -= UpdateUI;

        inventory = null;
    }

    public void SetVisible(bool visible)
    {
        if (inventoryUI == null)
            return;

        bool wasVisible = inventoryUI.activeSelf;

        // Only redraw when opening, not when another menu changes.
        if (visible && !wasVisible)
            UpdateUI();

        inventoryUI.SetActive(visible);

        if (visible != wasVisible && itemDetailsUI != null && itemDetailsUI.panel != null)
        {
            itemDetailsUI.panel.SetActive(false);
        }
    }

    // Retains the existing method for Inspector button connections.
    public void ToggleInventoryWithButton()
    {
        if (GameUIController.Instance != null)
            GameUIController.Instance.ToggleInventory();
    }

    public void UpdateUI()
    {
        if (inventory == null)
            inventory = Inventory.instance;

        if (inventory == null || itemsParent == null || slotPrefab == null)
            return;

        foreach (Transform child in itemsParent)
            Destroy(child.gameObject);

        foreach (Item item in inventory.items)
        {
            if (item == null)
                continue;

            GameObject newSlot = Instantiate(slotPrefab, itemsParent);

            InventorySlot slot = newSlot.GetComponent<InventorySlot>();

            if (slot == null)
            {
                Debug.LogError("Slot prefab needs an InventorySlot component.");

                Destroy(newSlot);
                continue;
            }

            slot.itemDetailsUI = itemDetailsUI;
            slot.AddItem(item);
        }
    }
}