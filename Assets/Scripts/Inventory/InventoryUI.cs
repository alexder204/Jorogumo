using UnityEngine;
using System.Collections;

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
            HandleInventoryKey();
        }
    }

    private void HandleInventoryKey()
    {
        bool isOpening = !inventoryUI.activeSelf;

        if (isOpening)
        {
            // Cooldown check
            if (!canToggle)
            {
                Debug.Log("Inventory opening is on cooldown.");
                return;
            }

            // Block opening when:
            // - game is already paused by pause menu
            // - in dialogue
            // - journal is open
            if (PauseManager.isGamePaused ||
                TopDownMovement.isInDialogue ||
                PauseManager.isJournalOpen)
            {
                return;
            }

            OpenInventory();
        }
        else
        {
            CloseInventory();
        }
    }

    public void ToggleInventoryWithButton()
    {
        StartCoroutine(ToggleInventoryTimer(0.25f));
    }

    private void OpenInventory()
    {
        inventoryUI.SetActive(true);

        if (itemDetailsUI != null && itemDetailsUI.panel != null)
            itemDetailsUI.panel.SetActive(false);

        // Mark inventory as open and pause the game
        PauseManager.isInventoryOpen = true;
        PauseManager.isGamePaused = true;
        Time.timeScale = 0f;
    }

    private void CloseInventory()
    {
        inventoryUI.SetActive(false);

        if (itemDetailsUI != null && itemDetailsUI.panel != null)
            itemDetailsUI.panel.SetActive(false);

        // Mark inventory as closed and unpause the game
        PauseManager.isInventoryOpen = false;
        PauseManager.isGamePaused = false;
        Time.timeScale = 1f;

        // Start cooldown so player can't spam
        canToggle = false;
        StartCoroutine(CooldownTimer(1f));
    }

    private IEnumerator ToggleInventoryTimer(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        HandleInventoryKey();
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
            slot.itemDetailsUI = itemDetailsUI;
            slot.AddItem(item);
        }
    }
}
