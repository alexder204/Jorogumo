using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IPointerClickHandler
{
    public Image icon;
    public TextMeshProUGUI amountText;

    private Item currentItem;

    public ItemDetailsUI itemDetailsUI; // Reference to the details UI script

    public void AddItem(Item newItem)
    {
        currentItem = newItem;
        icon.sprite = newItem.icon;
        icon.enabled = true;
        amountText.text = newItem.currentAmount.ToString();
    }

    public void ClearSlot()
    {
        currentItem = null;
        icon.sprite = null;
        icon.enabled = false;
        amountText.text = "";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentItem != null)
        {
            itemDetailsUI.ShowItemDetails(currentItem);
        }
    }
}
