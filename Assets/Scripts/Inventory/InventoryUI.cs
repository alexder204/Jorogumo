using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    public Transform itemsParent;
    public GameObject slotPrefab;  // Slot prefab to instantiate
    public GameObject inventoryUI;

    private Inventory inventory;
    public ItemDetailsUI itemDetailsUI;
    private bool canToggle = true;

    void Start()
    {
        inventory = Inventory.instance;
        inventory.onItemChangedCallback += UpdateUI;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            // Trying to open inventory but cooldown is active
            if (!inventoryUI.activeSelf && !canToggle)
            {
                Debug.Log("Inventory opening is on cooldown.");
                return;
            }

            // If both inventory and item details are open, close both
            if (inventoryUI.activeSelf && itemDetailsUI.panel.activeSelf)
            {
                itemDetailsUI.panel.SetActive(false); // Instantly hide detail panel
                inventoryUI.SetActive(false);         // Close inventory
                canToggle = false;
                StartCoroutine(CooldownTimer(1f));    // Start cooldown after closing
                return;
            }

            // Otherwise just toggle inventory normally
            ToggleInventory();
        }
    }


    public void ToggleInventoryWithButton()
    {
        StartCoroutine(ToggleInventoryTimer(0.25f));
    }

    public void ToggleInventory()
    {
        bool isOpening = !inventoryUI.activeSelf;

        inventoryUI.SetActive(isOpening);

        if (!isOpening)
        {
            // Inventory just closed, start cooldown
            canToggle = false;
            StartCoroutine(CooldownTimer(1f));
        }
    }

    private IEnumerator ToggleInventoryTimer(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        bool isOpening = !inventoryUI.activeSelf;

        inventoryUI.SetActive(isOpening);

        if (!isOpening)
        {
            // Inventory closed after animation delay - start cooldown
            canToggle = false;
            StartCoroutine(CooldownTimer(1f));
        }
    }

    private IEnumerator CooldownTimer(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        canToggle = true;
    }

    public void UpdateUI()
    {
        if (itemsParent == null || slotPrefab == null)
        {
            Debug.LogWarning("InventoryUI not fully initialized yet. Skipping UI update.");
            return;
        }

        foreach (Transform child in itemsParent)
        {
            Destroy(child.gameObject);
        }

        foreach (Item item in inventory.items)
        {
            GameObject newSlot = Instantiate(slotPrefab, itemsParent);
            InventorySlot slot = newSlot.GetComponent<InventorySlot>();
            slot.itemDetailsUI = itemDetailsUI;  // Assign the reference here
            slot.AddItem(item);
        }
    }
}
