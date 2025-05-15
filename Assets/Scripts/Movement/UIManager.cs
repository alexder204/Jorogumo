using UnityEngine;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("Pickup UI")]
    public GameObject pickUpPanel;
    public TMP_Text pickUpText;

    private Coroutine messageCoroutine;  // Tracks active message display

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
        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
            messageCoroutine = null;
        }

        pickUpPanel.SetActive(true);
        pickUpText.text = message;
        TopDownMovement.isInDialogue = true;

        messageCoroutine = StartCoroutine(WaitForCloseInput());
    }

    private IEnumerator WaitForCloseInput()
    {
        yield return null; // Allow UI to render

        while (!Input.GetKeyDown(KeyCode.E))
        {
            yield return null;
        }

        pickUpPanel.SetActive(false);
        TopDownMovement.isInDialogue = false;
        messageCoroutine = null;
    }
}
