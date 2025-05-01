using UnityEngine;
using TMPro;
using System.Collections;


public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("Pickup UI")]
    public GameObject pickUpPanel;
    public TMP_Text pickUpText;

    void Awake()
    {
        // Singleton pattern to ensure only one instance of UIManager
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of UIManager found!");
            return;
        }
        instance = this;
    }

    public void ShowMessage(string message)
    {
        if (pickUpPanel != null)
        {
            pickUpPanel.SetActive(true);  // Show the pickup panel
        }

        if (pickUpText != null)
        {
            pickUpText.text = message;  // Set the message text
        }

        // Start the coroutine to hide the panel after 3 seconds
        StartCoroutine(HidePanelAfterDelay(3f));  // 3 seconds delay
    }

    private IEnumerator HidePanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);  // Wait for the specified time
        if (pickUpPanel != null)
        {
            pickUpPanel.SetActive(false);  // Hide the panel after the delay
        }
    }
}
