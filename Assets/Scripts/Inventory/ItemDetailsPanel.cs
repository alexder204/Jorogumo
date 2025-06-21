using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ItemDetailsUI : MonoBehaviour
{
    public GameObject panel;
    public Image itemIcon;
    public TMP_Text itemNameText;
    public TMP_Text itemDescriptionText;

    void Start()
    {
        panel.SetActive(false);
    }

    public void ShowItemDetails(Item item)
    {
        if (item == null)
        {
            panel.SetActive(false);
            return;
        }

        itemIcon.sprite = item.icon;
        itemNameText.text = item.itemName;
        itemDescriptionText.text = item.description;
        panel.SetActive(true);
    }

    public void HideDetails()
    {
        StartCoroutine(HideDetailsTimer(0.25f));
    }

    private IEnumerator HideDetailsTimer(float duration)
    {
        yield return new WaitForSeconds(duration);
        panel.SetActive(false);
    }
}
