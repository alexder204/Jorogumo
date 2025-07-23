using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialoguePickUp : MonoBehaviour
{
    [SerializeField] private GameObject dialogueCanvas, interactItem, interactPopUp, charImage1, charImage2;
    [SerializeField] private TextMeshProUGUI speakerText, dialogueText;
    [SerializeField] private Image portraitImage1, portraitImage2;
    [SerializeField] private string[] speaker;
    [SerializeField] private Sprite[] portrait1, portrait2;
    [SerializeField, TextArea(3, 10)] private string[] dialogueWords;

    [Header("Item Pickup")]
    [SerializeField] private Item itemToPickUp;  // The item to give the player
    private UniqueID uniqueID;

    private bool dialogueActived = false, playerInRange = false, canContinueText = true;
    private int step;
    private float typingSpeed = 0.02f;
    private Coroutine typingRoutine;
    private TopDownMovement playerMovement;

    private DialogueID dialogueID;

    private void Start()
    {
        dialogueID = GetComponent<DialogueID>();
        uniqueID = GetComponent<UniqueID>();

        playerMovement = GameObject.Find("Player").GetComponent<TopDownMovement>();

        var ui = DialogueUIManager.Instance;

        dialogueCanvas = ui.dialogueCanvas;
        charImage1 = ui.charImage1;
        charImage2 = ui.charImage2;
        speakerText = ui.speakerText;
        dialogueText = ui.dialogueText;
        portraitImage1 = ui.portraitImage1;
        portraitImage2 = ui.portraitImage2;

        if (interactPopUp == null)
            interactPopUp = GameObject.Find("InteractPopUp");

        // **Exactly the same as Dialogue.cs**
        if (DialogueUIManager.Instance.HasCompletedDialogue(dialogueID))
        {
            if (interactPopUp != null)
                interactPopUp.SetActive(false);

            if (interactItem != null)
                interactItem.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetButtonDown("Interact"))
        {
            if (dialogueActived && canContinueText)
            {
                if (typingRoutine != null)
                    StopCoroutine(typingRoutine);

                if (step >= speaker.Length)
                {
                    DialogueUIManager.Instance.MarkDialogueComplete(dialogueID);
                    EndDialogue();  // Same method name as Dialogue.cs
                }
                else
                {
                    ContinueDialogue();
                }
            }
            else if (playerInRange && !dialogueActived)
            {
                if (DialogueUIManager.Instance.HasCompletedDialogue(dialogueID))
                    return;

                StartDialogue();
            }
        }
    }

    private void StartDialogue()
    {
        dialogueActived = true;
        dialogueCanvas.SetActive(true);
        interactItem?.SetActive(true);
        interactPopUp?.SetActive(true);
        charImage1.SetActive(true);
        charImage2.SetActive(true);

        playerMovement.moveSpeed = 0f;
        playerMovement.sprintSpeed = 0f;
        TopDownMovement.isInDialogue = true;

        step = 0;
        ContinueDialogue();
    }

    private void ContinueDialogue()
    {
        typingRoutine = StartCoroutine(Typing(dialogueWords[step]));
        UpdateUIForDialogue(step);
        step++;
    }

    private void EndDialogue()
    {
        dialogueCanvas.SetActive(false);
        interactItem?.SetActive(false);
        interactPopUp?.SetActive(false);
        charImage1.SetActive(false);
        charImage2.SetActive(false);

        playerMovement.moveSpeed = 6f;
        playerMovement.sprintSpeed = 14f;
        TopDownMovement.isInDialogue = false;

        step = 0;
        dialogueActived = false;

        // **Add the item pickup logic here exactly as you want:**
        if (itemToPickUp != null)
        {
            bool added = Inventory.instance.Add(itemToPickUp);
            if (added)
            {
                UIManager.instance.ShowMessage($"Picked up {itemToPickUp.itemName}!");

                if (uniqueID != null)
                    ObjectStateTracker.Instance?.MarkUsed(uniqueID.id);

                gameObject.SetActive(false);
            }
            else
            {
                UIManager.instance.ShowMessage("Inventory full.");
            }
        }
    }

    private void UpdateUIForDialogue(int index)
    {
        speakerText.text = speaker[index];
        portraitImage1.sprite = portrait1[index];
        portraitImage2.sprite = portrait2[index];
    }

    private IEnumerator Typing(string line)
    {
        dialogueText.text = "";
        canContinueText = false;

        bool addingRichTextTag = false;
        yield return new WaitForSeconds(0.5f);

        foreach (char letter in line)
        {
            if (Input.GetButtonDown("Interact"))
            {
                dialogueText.text = line;
                break;
            }

            if (letter == '<' || addingRichTextTag)
            {
                addingRichTextTag = true;
                dialogueText.text += letter;
                if (letter == '>') addingRichTextTag = false;
            }
            else
            {
                dialogueText.text += letter;
                yield return new WaitForSeconds(typingSpeed);
            }
        }

        canContinueText = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            if (!DialogueUIManager.Instance.HasCompletedDialogue(dialogueID))
                interactPopUp?.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            interactPopUp?.SetActive(false);
            dialogueCanvas.SetActive(false);
            dialogueActived = false;
            TopDownMovement.isInDialogue = false;
        }
    }
}
