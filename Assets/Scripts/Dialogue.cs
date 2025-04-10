using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace PlayerDialogue
{
    public class Dialogue : MonoBehaviour
    {
        [SerializeField] private GameObject dialogueCanvas, interactItem, interactPopUp, charImage1, charImage2;
        [SerializeField] private TextMeshProUGUI speakerText, dialogueText;
        [SerializeField] private Image portraitImage1, portraitImage2;
        [SerializeField] private string[] speaker;
        [SerializeField] private Sprite[] portrait1, portrait2;
        [SerializeField, TextArea(3, 10)] private string[] dialogueWords;

        private bool dialogueActived, canContinueText = true;
        private int step;
        private float typingSpeed = 0.02f;
        private Coroutine typingRoutine;
        private TopDownMovement playerMovement;

        private void Start()
        {
            playerMovement = GameObject.Find("Player").GetComponent<TopDownMovement>();
        }

        private void Update()
        {
            if (Input.GetButtonDown("Interact") && dialogueActived && canContinueText)
            {
                if (typingRoutine != null)
                {
                    StopCoroutine(typingRoutine);
                }

                if (step >= speaker.Length)
                {
                    EndDialogue();
                }
                else
                {
                    ContinueDialogue();
                }
            }
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
            interactItem.SetActive(false);
            interactPopUp.SetActive(false);
            charImage1.SetActive(false);
            charImage2.SetActive(false);
            playerMovement.moveSpeed = 5f;
            playerMovement.sprintSpeed = 3f;
            step = 0;
        }

        private void UpdateUIForDialogue(int index)
        {
            dialogueCanvas.SetActive(true);
            interactItem.SetActive(true);
            interactPopUp.SetActive(true);
            charImage1.SetActive(true);
            charImage2.SetActive(true);

            playerMovement.moveSpeed = 0f;
            playerMovement.sprintSpeed = 0f;

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
                dialogueActived = true;
                interactPopUp.SetActive(true);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            dialogueActived = false;
            interactPopUp.SetActive(false);
            dialogueCanvas.SetActive(false);
        }
    }
}
