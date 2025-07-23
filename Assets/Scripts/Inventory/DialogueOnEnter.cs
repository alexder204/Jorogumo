using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Xml;

namespace PlayerDialogue
{
    public class DialogueOnEnter : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject dialogueCanvas;
        [SerializeField] private GameObject interactPopUp;
        [SerializeField] private GameObject interactItem;
        [SerializeField] private GameObject charImage1;
        [SerializeField] private GameObject charImage2;
        [SerializeField] private TextMeshProUGUI speakerText;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private Image portraitImage1;
        [SerializeField] private Image portraitImage2;

        [Header("Dialogue Data")]
        [SerializeField] private string[] speaker;
        [SerializeField] private Sprite[] portrait1;
        [SerializeField] private Sprite[] portrait2;
        [SerializeField][TextArea(3, 10)] private string[] dialogueWords;

        [Header("Settings")]
        [SerializeField] private float typingSpeed = 0.02f;

        private TopDownMovement playerMovement;
        private Coroutine typingRoutine;
        private int step;
        private bool dialogueActive = false;
        private bool canContinueText = false;

        private DialogueID dialogueID;

        private void Start()
        {
            dialogueID = GetComponent<DialogueID>();

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

            // At start, check if this dialogue was completed before; if yes, disable interaction UI
            if (DialogueUIManager.Instance.HasCompletedDialogue(dialogueID))
            {
                if (interactPopUp != null)
                    interactPopUp.SetActive(false);

                if (interactItem != null)
                    interactItem.SetActive(false);
            }
        }


        void Update()
        {
            if (!dialogueActive || !canContinueText) return;

            if (Input.GetButtonDown("Interact"))
            {
                if (typingRoutine != null)
                {
                    StopCoroutine(typingRoutine);
                    FinishTyping();
                }
                else
                {
                    AdvanceDialogue();
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.CompareTag("Player")) return;

            // If dialogue was completed, do not start or show interaction prompt
            if (DialogueUIManager.Instance.HasCompletedDialogue(dialogueID))
            {
                if (interactPopUp != null)
                    interactPopUp.SetActive(false);

                if (interactItem != null)
                    interactItem.SetActive(false);
                return;
            }

            StartDialogue();
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (!collision.CompareTag("Player")) return;

            EndDialogue();
        }

        private void StartDialogue()
        {
            dialogueActive = true;
            canContinueText = false;
            step = 0;

            TopDownMovement.isInDialogue = true;
            playerMovement.moveSpeed = 0f;
            playerMovement.sprintSpeed = 0f;

            dialogueCanvas.SetActive(true);
            interactItem.SetActive(true);
            interactPopUp.SetActive(true);
            charImage1.SetActive(true);
            charImage2.SetActive(true);

            DisplayCurrentDialogue();
        }

        private void AdvanceDialogue()
        {
            step++;

            if (step >= dialogueWords.Length)
            {
                // Mark this dialogue as completed before ending
                DialogueUIManager.Instance.MarkDialogueComplete(dialogueID);

                EndDialogue();
            }
            else
            {
                canContinueText = false;
                DisplayCurrentDialogue();
            }
        }

        private void DisplayCurrentDialogue()
        {
            speakerText.text = speaker[step];
            portraitImage1.sprite = portrait1[step];
            portraitImage2.sprite = portrait2[step];

            if (typingRoutine != null)
            {
                StopCoroutine(typingRoutine);
            }
            typingRoutine = StartCoroutine(Typing(dialogueWords[step]));
        }

        private IEnumerator Typing(string line)
        {
            dialogueText.text = "";
            bool addingRichTextTag = false;
            yield return new WaitForSeconds(0.1f); // Reduced initial delay for faster feedback

            foreach (char letter in line)
            {
                if (Input.GetButtonDown("Interact"))
                {
                    FinishTyping();
                    yield break;
                }

                if (letter == '<' || addingRichTextTag)
                {
                    addingRichTextTag = true;
                    dialogueText.text += letter;
                    if (letter == '>')
                        addingRichTextTag = false;
                }
                else
                {
                    dialogueText.text += letter;
                    yield return new WaitForSeconds(typingSpeed);
                }
            }

            typingRoutine = null;
            canContinueText = true;
        }

        private void FinishTyping()
        {
            dialogueText.text = dialogueWords[step];
            typingRoutine = null;
            canContinueText = true;
        }

        private void EndDialogue()
        {
            if (typingRoutine != null)
            {
                StopCoroutine(typingRoutine);
                typingRoutine = null;
            }

            dialogueCanvas.SetActive(false);
            interactItem.SetActive(false);
            interactPopUp.SetActive(false);
            charImage1.SetActive(false);
            charImage2.SetActive(false);

            playerMovement.moveSpeed = 6f;
            playerMovement.sprintSpeed = 14f;
            TopDownMovement.isInDialogue = false;

            dialogueActive = false;
            canContinueText = false;
        }
    }
}
