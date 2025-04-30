using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlot : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI amountText;

    public void AddItem(Item newItem)
    {
        icon.sprite = newItem.icon;
        icon.enabled = true;

        // Show the current amount in the stack
        amountText.text = newItem.currentAmount.ToString();
    }

    public void ClearSlot()
    {
        icon.sprite = null;
        icon.enabled = false;
        amountText.text = "";
    }
}