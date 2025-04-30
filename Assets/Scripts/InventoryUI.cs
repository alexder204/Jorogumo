using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
            inventoryUI.SetActive(!inventoryUI.activeSelf);
        }
    }

    void UpdateUI()
    {
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
