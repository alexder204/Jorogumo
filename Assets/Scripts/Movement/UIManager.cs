using UnityEngine;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("Pickup UI")]
    public GameObject pickUpPanel;
    public TMP_Text pickUpText;

    private Coroutine messageCoroutine;  // To track and cancel ongoing message

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of UIManager found!");
            return;
        }
        instance = this;
    }

    public void ShowMessage(string message)
    {
        pickUpPanel.SetActive(true);
        pickUpText.text = message;
        TopDownMovement.isInDialogue = true; // Freeze player movement
        StartCoroutine(WaitForCloseInput());
    }

    private IEnumerator WaitForCloseInput()
    {
        yield return null; // Wait one frame so the panel fully activates

        while (!Input.GetKeyDown(KeyCode.E))
        {
            yield return null;
        }

        pickUpPanel.SetActive(false);
        TopDownMovement.isInDialogue = false;
    }
}
